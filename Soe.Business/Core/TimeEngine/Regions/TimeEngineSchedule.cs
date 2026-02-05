using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.AnnualLeave.PreFlight;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        #region Schedule - template

        private GetTimeScheduleTemplateOutputDTO TaskGetTimeScheduleTemplate()
        {
            var (iDTO, oDTO) = InitTask<GetTimeScheduleTemplateInputDTO, GetTimeScheduleTemplateOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.TemplateHead = GetTimeScheduleTemplateHeadWithPeriodsAndActiveBlocks(iDTO.TemplateHeadId, iDTO.LoadEmployeeSchedule, iDTO.LoadAccounts);
            }

            return oDTO;
        }

        private SaveTimeScheduleTemplateOutputDTO TaskSaveTimeScheduleTemplate()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeScheduleTemplateInputDTO, SaveTimeScheduleTemplateOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TemplateHead == null || iDTO.TemplateBlockItems == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull);
                return oDTO;
            }

            int timeScheduleTemplateHeadId = 0;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveTimeScheduleTemplate(iDTO.TemplateBlockItems, iDTO.TemplateHead);
                        if (oDTO.Result.Success)
                            timeScheduleTemplateHeadId = oDTO.Result.IntegerValue;

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
                        oDTO.Result.IntegerValue = timeScheduleTemplateHeadId;
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private SaveTimeScheduleTemplateStaffingOutputDTO TaskSaveTimeScheduleTemplateStaffing()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeScheduleTemplateStaffingInputDTO, SaveTimeScheduleTemplateStaffingOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            int timeScheduleTemplateHeadId = 0;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        if (iDTO.StartOnFirstDayOfWeek && iDTO.StartDate.DayOfWeek != DayOfWeek.Monday)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeStartsFirstDayOnWeekInvalid);
                            return oDTO;
                        }

                        Employee employee = iDTO.EmployeeId != 0 ? GetEmployeeWithContactPersonFromCache(iDTO.EmployeeId) : null;
                        EmployeePost employeePost = iDTO.EmployeePostId.HasValue ? TimeScheduleManager.GetEmployeePost(entities, iDTO.EmployeePostId.Value) : null;
                        if (employee == null && employeePost == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                            return oDTO;
                        }

                        string name = string.Empty;
                        string description = string.Empty;
                        if (employee != null)
                            iDTO.GetNameAndDescription(employee, out name, out description);

                        #region Copy shifts from another template or from sent in shifts

                        List<StringKeyValue> guids = new List<StringKeyValue>();

                        if (!iDTO.Shifts.Any() && iDTO.CopyFromTimeScheduleTemplateHeadId.HasValue)
                        {
                            TimeScheduleTemplateHead templateHead = TimeScheduleManager.GetTimeScheduleTemplateHead(entities, iDTO.CopyFromTimeScheduleTemplateHeadId.Value, ActorCompanyId, true);
                            if (templateHead != null)
                            {
                                iDTO.NoOfDays = templateHead.NoOfDays;
                                iDTO.Shifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, ActorCompanyId, RoleId, UserId, (employee != null ? employee.EmployeeId : employeePost.EmployeePostId), iDTO.CopyFromTimeScheduleTemplateHeadId.Value, iDTO.StartDate, iDTO.StartDate, hiddenEmployeeId: GetHiddenEmployeeIdFromCache());
                            }
                        }

                        // If template has shifts without account, target employee must belong to an employee group that allows it.
                        if (iDTO.UseAccountingFromSourceSchedule && UseAccountHierarchy() && iDTO.Shifts.Any(s => !s.AccountId.HasValue))
                        {
                            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(iDTO.StartDate, GetEmployeeGroupsFromCache());
                            if (employeeGroup != null && !employeeGroup.AllowShiftsWithoutAccount)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeGroupDoesNotAllowShiftsWithoutAccount, GetText(12042, "Den anställdes tidavtal tillåter inte pass utan tillhörighet"));
                                return oDTO;
                            }
                        }

                        // Break groups
                        List<TimeCodeBreakGroup> timeCodeBreakGroups = GetTimeCodeBreakGroups();
                        List<TimeCodeBreak> timeCodeBreaks = new List<TimeCodeBreak>();
                        if (!timeCodeBreakGroups.IsNullOrEmpty())
                        {
                            foreach (TimeCodeBreakGroup timeCodeBreakGroup in timeCodeBreakGroups)
                            {
                                TimeCodeBreak timeCodeBreakForGroup = GetTimeCodeBreakForEmployeeGroupFromCache(timeCodeBreakGroup.TimeCodeBreakGroupId, employee.GetEmployeeGroupId(iDTO.StartDate));
                                if (timeCodeBreakForGroup != null)
                                    timeCodeBreaks.Add(timeCodeBreakForGroup);
                            }
                        }

                        foreach (TimeSchedulePlanningDayDTO shift in iDTO.Shifts)
                        {
                            // Clear ids
                            shift.TimeScheduleTemplateBlockId = 0;
                            shift.TimeScheduleTemplatePeriodId = 0;
                            shift.Break1Id = 0;
                            shift.Break1Link = null;
                            shift.Break2Id = 0;
                            shift.Break2Link = null;
                            shift.Break3Id = 0;
                            shift.Break3Link = null;
                            shift.Break4Id = 0;
                            shift.Break4Link = null;

                            // Set target employee
                            shift.EmployeeId = (employee != null ? employee.EmployeeId : employeePost.EmployeePostId);

                            // Correct dates so DayNumber 1 is the same as start date of template
                            int offset = (int)(iDTO.StartDate.AddDays(shift.DayNumber - 1) - shift.StartTime.Date).TotalDays;
                            shift.StartTime = shift.StartTime.AddDays(offset);
                            shift.StopTime = shift.StopTime.AddDays(offset);

                            TimeCodeBreak timeCodeBreak;
                            if (shift.Break1TimeCodeId != 0)
                            {
                                shift.Break1StartTime = shift.Break1StartTime.AddDays(offset);
                                timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == shift.Break1Minutes);
                                if (timeCodeBreak != null && timeCodeBreak.TimeCodeId != shift.Break1TimeCodeId)
                                    shift.Break1TimeCodeId = timeCodeBreak.TimeCodeId;
                            }
                            if (shift.Break2TimeCodeId != 0)
                            {
                                shift.Break2StartTime = shift.Break2StartTime.AddDays(offset);
                                timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == shift.Break2Minutes);
                                if (timeCodeBreak != null && timeCodeBreak.TimeCodeId != shift.Break2TimeCodeId)
                                    shift.Break2TimeCodeId = timeCodeBreak.TimeCodeId;
                            }
                            if (shift.Break3TimeCodeId != 0)
                            {
                                shift.Break3StartTime = shift.Break3StartTime.AddDays(offset);
                                timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == shift.Break3Minutes);
                                if (timeCodeBreak != null && timeCodeBreak.TimeCodeId != shift.Break3TimeCodeId)
                                    shift.Break3TimeCodeId = timeCodeBreak.TimeCodeId;
                            }
                            if (shift.Break4TimeCodeId != 0)
                            {
                                shift.Break4StartTime = shift.Break4StartTime.AddDays(offset);
                                timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == shift.Break4Minutes);
                                if (timeCodeBreak != null && timeCodeBreak.TimeCodeId != shift.Break4TimeCodeId)
                                    shift.Break4TimeCodeId = timeCodeBreak.TimeCodeId;
                            }

                            // Every unique source Guid will get one new target Guid
                            // Linked source shifts will still be linked as targets, but with a new Guid
                            if (shift.Link.HasValue)
                            {
                                StringKeyValue newGuidItem = guids.FirstOrDefault(g => g.Key == shift.Link.ToString());
                                if (newGuidItem != null)
                                {
                                    shift.Link = new Guid(newGuidItem.Value);
                                }
                                else
                                {
                                    Guid newGuid = Guid.NewGuid();
                                    guids.Add(new StringKeyValue(shift.Link.ToString(), newGuid.ToString()));
                                    shift.Link = newGuid;
                                }
                            }
                            else
                            {
                                shift.Link = Guid.NewGuid();
                            }
                        }

                        #endregion

                        List<TimeScheduleTemplateBlockDTO> templateBlockItems = ConvertToTemplateBlockItems(iDTO.Shifts);
                        ApplyAccountingFromShiftType(templateBlockItems);

                        #endregion

                        #region Perform

                        oDTO.Result = SaveTimeScheduleTemplate(templateBlockItems, iDTO.TimeScheduleTemplateHeadId, name, description, (int)SoeEntityState.Active, iDTO.StartDate, iDTO.StopDate, iDTO.CurrentDate, iDTO.FirstMondayOfCycle, iDTO.NoOfDays, iDTO.SimpleSchedule, iDTO.IsPersonalTemplate, iDTO.StartOnFirstDayOfWeek, iDTO.FlexForceSchedule, iDTO.Locked, iDTO.EmployeeId, iDTO.EmployeePostId, iDTO.UseAccountingFromSourceSchedule);
                        if (oDTO.Result.Success)
                            timeScheduleTemplateHeadId = oDTO.Result.IntegerValue;

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
                        oDTO.Result.IntegerValue = timeScheduleTemplateHeadId;
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private UpdateTimeScheduleTemplateStaffingOutputDTO TaskUpdateTimeScheduleTemplateStaffing()
        {
            var (iDTO, oDTO) = InitTask<UpdateTimeScheduleTemplateStaffingInputDTO, UpdateTimeScheduleTemplateStaffingOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Shifts == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_ShiftIsNull);
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                    if (defaultTimeCodeId == 0)
                    {
                        oDTO.Result = new ActionResult(GetText(8924, "Standard tidkod är ej satt i företagsinställning. Välj standard tidkod för att fortsätta"));
                        return oDTO;
                    }

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Delete breaks outside schedule

                        foreach (TimeSchedulePlanningDayDTO shift in iDTO.Shifts)
                        {
                            if (shift.Break1Id != 0 && !iDTO.Shifts.Any(i => i.StartTime < shift.Break1StartTime.AddMinutes(shift.Break1Minutes) && i.StopTime > shift.Break1StartTime))
                                shift.Break1TimeCodeId = 0;
                            if (shift.Break2Id != 0 && !iDTO.Shifts.Any(i => i.StartTime < shift.Break2StartTime.AddMinutes(shift.Break2Minutes) && i.StopTime > shift.Break2StartTime))
                                shift.Break2TimeCodeId = 0;
                            if (shift.Break3Id != 0 && !iDTO.Shifts.Any(i => i.StartTime < shift.Break3StartTime.AddMinutes(shift.Break3Minutes) && i.StopTime > shift.Break3StartTime))
                                shift.Break3TimeCodeId = 0;
                            if (shift.Break4Id != 0 && !iDTO.Shifts.Any(i => i.StartTime < shift.Break4StartTime.AddMinutes(shift.Break4Minutes) && i.StopTime > shift.Break4StartTime))
                                shift.Break4TimeCodeId = 0;
                        }

                        #endregion

                        #region Accounting

                        List<TimeScheduleTemplateBlockDTO> templateBlockItems = ConvertToTemplateBlockItems(iDTO.Shifts);
                        ApplyAccountingFromShiftType(templateBlockItems);

                        #endregion

                        #region Save template schedule

                        oDTO.Result = UpdateTimeScheduleTemplate(templateBlockItems, iDTO.TimeScheduleTemplateHeadId, iDTO.DayNumberFrom, iDTO.DayNumberTo, iDTO.CurrentDate);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Activate schedule

                        if (!iDTO.ActivateDates.IsNullOrEmpty() && iDTO.ActivateDayNumber.HasValue && !iDTO.Shifts.IsNullOrEmpty())
                        {
                            List<TimeSchedulePlanningDayDTO> inputShifts = new List<TimeSchedulePlanningDayDTO>();
                            List<TimeSchedulePlanningDayDTO> inputShiftsForDayNumber = iDTO.Shifts.Where(s => s.DayNumber == iDTO.ActivateDayNumber).ToList();

                            foreach (DateTime activateDate in iDTO.ActivateDates)
                            {
                                inputShiftsForDayNumber.SetNewLinks();

                                foreach (TimeSchedulePlanningDayDTO inputShift in inputShiftsForDayNumber)
                                {
                                    if (inputShift.ActualDate == activateDate)
                                    {
                                        inputShifts.Add(inputShift);
                                    }
                                    else
                                    {
                                        TimeSchedulePlanningDayDTO shift = inputShift.Copy();
                                        shift.CopyBreaks(inputShift);
                                        shift.ChangeDate(activateDate);
                                        inputShifts.Add(shift);
                                    }
                                }
                            }

                            List<TimeSchedulePlanningDayDTO> existingShifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(entities, actorCompanyId, base.UserId, 0, base.RoleId, iDTO.ActivateDates.Min(), iDTO.ActivateDates.Max(), new List<int>() { iDTO.EmployeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.Admin, true, true, false, includePreliminary: true, includeShiftRequest: true, includeAbsenceRequest: false, checkToIncludeDeliveryAdress: false).ToList();
                            existingShifts = existingShifts.Where(s => iDTO.ActivateDates.Contains(s.ActualDate) && s.DayNumber >= iDTO.DayNumberFrom && s.DayNumber <= iDTO.DayNumberTo).ToList();

                            List<TimeSchedulePlanningDayDTO> adjustedShifts = AdjustShiftsToBeSaved(inputShifts, existingShifts);
                            if (!adjustedShifts.IsNullOrEmpty())
                            {
                                oDTO.Result = SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.TemplateScheduleSaveAndActive, adjustedShifts, true, true, false, 0, null);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #region Restore to schedule

                                oDTO.Result = RestoreCurrentDaysToSchedule();
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                #region ExtraShift

                                oDTO.Result = SetHasUnhandledShiftChanges();
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                if (!iDTO.SkipXEMailOnChanges)
                                {
                                    List<DateTime> sendXEMailForDays = adjustedShifts.Where(x => !x.IsPreliminary).Select(x => x.ActualDate).Distinct().ToList();
                                    if (sendXEMailForDays.Any())
                                        SendXEMailOnDaysChanged(iDTO.EmployeeId, sendXEMailForDays, TermGroup_TimeScheduleTemplateBlockType.Schedule);
                                }

                                //Absence?
                            }
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
                    {
                        if (!oDTO.Result.Keys.IsNullOrEmpty())
                            oDTO.StampingTimeBlockDateIds.AddRange(oDTO.Result.Keys);
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private DeleteTimeScheduleTemplateOutputDTO TaskDeleteTimeScheduleTemplate()
        {
            var (iDTO, oDTO) = InitTask<DeleteTimeScheduleTemplateInputDTO, DeleteTimeScheduleTemplateOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHeadWithPeriodsAndActiveBlocks(iDTO.TemplateHeadId, true, false);
                if (templateHead == null)
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");
                    return oDTO;
                }
                if (templateHead.EmployeeSchedule != null && templateHead.EmployeeSchedule.Any(s => s.State != (int)SoeEntityState.Deleted))
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.TimeScheduleTemplateNotDeletedEmployeeScheduleExists, GetText(3394, "Grundschemat kunde inte tas bort, schemat är aktiverat"));
                    return oDTO;
                }

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        ChangeEntityState(templateHead, SoeEntityState.Deleted);

                        foreach (TimeScheduleTemplatePeriod templatePeriod in templateHead.TimeScheduleTemplatePeriod.Where(p => p.State != (int)SoeEntityState.Deleted))
                        {
                            ChangeEntityState(templatePeriod, SoeEntityState.Deleted);
                            foreach (TimeScheduleTemplateBlock templateBlock in templatePeriod.TimeScheduleTemplateBlock.Where(b => b.State != (int)SoeEntityState.Deleted))
                            {
                                ChangeEntityState(templateBlock, SoeEntityState.Deleted);
                                foreach (TimeScheduleTemplateBlockTask templateBlockTask in templateBlock.TimeScheduleTemplateBlockTask)
                                {
                                    ChangeEntityState(templateBlockTask, SoeEntityState.Deleted);
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

        private RemoveEmployeeFromTimeScheduleTemplateOutputDTO TaskRemoveEmployeeFromTimeScheduleTemplate()
        {
            var (iDTO, oDTO) = InitTask<RemoveEmployeeFromTimeScheduleTemplateInputDTO, RemoveEmployeeFromTimeScheduleTemplateOutputDTO>();
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

                        oDTO.Result = RemoveEmployeeFromTimeScheduleTemplate(iDTO.TimeScheduleTemplateHeadId);

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

        private AssignTimeScheduleTemplateToEmployeeOutputDTO TaskAssignTimeScheduleTemplateToEmployee()
        {
            var (iDTO, oDTO) = InitTask<AssignTimeScheduleTemplateToEmployeeInputDTO, AssignTimeScheduleTemplateToEmployeeOutputDTO>();
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

                        oDTO.Result = AssignTimeScheduleTemplateToEmployee(iDTO.TimeScheduleTemplateHeadId, iDTO.EmployeeId, iDTO.StartDate);

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

        private int GetTemplateScheduleMinutes(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<EmployeeSchedule> employeeSchedules = GetEmployeeSchedulesForEmployee(employeeId).Filter(dateFrom, dateTo);
            if (employeeSchedules.IsNullOrEmpty())
                return 0;
            if (!AddTimeScheduleTemplatePeriodsToCache(employeeId, dateFrom, dateTo))
                return 0;

            int minutes = 0;
            Dictionary<int, List<GetTemplateSchedule_Result>> templateHeadScheduleDict = new Dictionary<int, List<GetTemplateSchedule_Result>>();
            Dictionary<int, List<GetTemplateSchedule_Result>> templatePeriodScheduleDict = new Dictionary<int, List<GetTemplateSchedule_Result>>();

            DateTime currentDate = dateFrom;
            while (currentDate <= dateTo)
            {
                try
                {
                    EmployeeSchedule employeeSchedule = employeeSchedules.Get(currentDate);
                    if (employeeSchedule == null)
                        continue;

                    if (!templateHeadScheduleDict.ContainsKey(employeeSchedule.TimeScheduleTemplateHeadId))
                        templateHeadScheduleDict.Add(employeeSchedule.TimeScheduleTemplateHeadId, entities.GetTemplateSchedule(employeeSchedule.TimeScheduleTemplateHeadId).ToList());
                    List<GetTemplateSchedule_Result> templateScheduleForDay = templateHeadScheduleDict.GetList(employeeSchedule.TimeScheduleTemplateHeadId);
                    if (templateScheduleForDay.IsNullOrEmpty())
                        continue;

                    int templatePeriodId = GetTimeScheduleTemplatePeriodIdFromCache(employeeId, currentDate) ?? 0;
                    if (templatePeriodId <= 0)
                        continue;

                    if (!templatePeriodScheduleDict.ContainsKey(templatePeriodId))
                        templatePeriodScheduleDict.Add(templatePeriodId, templateScheduleForDay.GetScheduleForPeriod(templatePeriodId));
                    List<GetTemplateSchedule_Result> templateScheduleForPeriod = templatePeriodScheduleDict.GetList(templatePeriodId);
                    if (templateScheduleForPeriod.IsNullOrEmpty())
                        continue;

                    minutes += templateScheduleForPeriod.GetWorkMinutes();
                }
                finally
                {
                    currentDate = currentDate.AddDays(1);
                }
            }

            return minutes;
        }

        #endregion

        #region Schedule - active

        private GetSequentialScheduleOutputDTO TaskGetSequentialSchedule()
        {
            var (iDTO, oDTO) = InitTask<GetSequentialScheduleInputDTO, GetSequentialScheduleOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.TemplatePeriod = GetSequentialSchedule(iDTO.TimeScheduleTemplatePeriodId, iDTO.Date, iDTO.EmployeeId, iDTO.IncludeStandBy);
            }

            return oDTO;
        }

        private SaveShiftPrelDefOutputDTO TaskSaveShiftPrelToDef()
        {
            var (iDTO, oDTO) = InitTask<SaveShiftPrelDefInputDTO, SaveShiftPrelDefOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            List<int> failedEmployeeIds = null;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    if (!iDTO.EmployeeDates.Any())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                        return oDTO;
                    }

                    foreach (EmployeeDatesDTO employeeDates in iDTO.EmployeeDates)
                    {
                        if (employeeDates.StartDate > employeeDates.StopDate)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EmployeeScheduleStopDateEarlierThanStartDate, GetText(3592, "Slutdatum tidigare än startdatum"));
                            return oDTO;
                        }
                    }

                    bool useAccountHierarchy = UseAccountHierarchy();

                    // Save copy of original schedule in JSON format
                    bool saveCopy = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningSaveCopyOnPublish);
                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        TimeScheduleCopyHead copyHead = null;
                        if (saveCopy)
                        {
                            // Create head that holds information of the publish and who did it
                            copyHead = new TimeScheduleCopyHead
                            {
                                ActorCompanyId = actorCompanyId,
                                Type = (int)TermGroup_TimeScheduleCopyHeadType.PrelToDef,
                                DateFrom = iDTO.EmployeeDates.First().StartDate,
                                DateTo = iDTO.EmployeeDates.First().StopDate,
                                UserId = userId,
                            };
                            SetCreatedProperties(copyHead);
                            entities.AddToTimeScheduleCopyHead(copyHead);
                        }

                        foreach (EmployeeDatesDTO employeeDates in iDTO.EmployeeDates)
                        {
                            Employee employee = GetEmployeeFromCache(employeeDates.EmployeeId);
                            if (employee == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                                return oDTO;
                            }

                            #region TimeScheduleTemplateBlock

                            List<TimeScheduleTemplateBlock> shifts = GetScheduleBlocksForEmployee(null, employeeDates.EmployeeId, employeeDates.StartDate, employeeDates.StopDate, loadStaffingIfUsed: false, includeStandBy: iDTO.IncludeStandbyShifts, includeOnDuty: true).Where(b => b.IsPreliminary || b.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.AnnualLeave).ToList();
                            if (!iDTO.IncludeScheduleShifts)
                                shifts = shifts.Where(b => b.Type != (int)TermGroup_TimeScheduleTemplateBlockType.Schedule).ToList();

                            if (useAccountHierarchy)
                            {
                                AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
                                input.AddParamValue(AccountHierarchyParamType.IncludeAbstract, true);
                                List<int> validAccountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(entities, actorCompanyId, RoleId, userId, employeeDates.StartDate, employeeDates.StopDate, input);
                                shifts = shifts.Where(b => !b.AccountId.HasValue || validAccountIds.Contains(b.AccountId.Value)).ToList();
                            }

                            TimeScheduleCopyRowJsonDataDTO jsonDataDTO = null;
                            if (saveCopy)
                            {
                                // Create DTO structure for copy row
                                jsonDataDTO = new TimeScheduleCopyRowJsonDataDTO()
                                {
                                    Shifts = new List<TimeScheduleCopyRowJsonDataShiftDTO>()
                                };
                            }

                            foreach (TimeScheduleTemplateBlock shift in shifts)
                            {
                                shift.IsPreliminary = false;
                                SetModifiedProperties(shift);

                                if (saveCopy)
                                {
                                    // Add shift to DTO structure
                                    jsonDataDTO.Shifts.Add(shift.ToTimeScheduleCopyRowJsonDataDTO());
                                }
                            }

                            if (saveCopy)
                            {
                                if (useLeisureCodes)
                                {
                                    List<TimeScheduleEmployeePeriodDetail> details = TimeScheduleManager.GetTimeSchedulePlanningLeisureCodes(entities, employeeDates.StartDate, employeeDates.StopDate, new List<int>() { employeeDates.EmployeeId });
                                    foreach (TimeScheduleEmployeePeriodDetail detail in details)
                                    {
                                        jsonDataDTO.Shifts.Add(detail.ToTimeScheduleCopyRowJsonDataDTO());
                                    }
                                }

                                // Create one copy row for each employee published
                                TimeScheduleCopyRow copyRow = new TimeScheduleCopyRow
                                {
                                    TimeScheduleCopyHead = copyHead,
                                    EmployeeId = employeeDates.EmployeeId,
                                    Type = (int)TermGroup_TimeScheduleCopyRowType.Default,
                                    JsonData = jsonDataDTO.ToTimeScheduleCopyRowJsonData()
                                };
                                SetCreatedProperties(copyRow);
                            }

                            #endregion

                            #region TimeBlock

                            List<TimeBlock> employeeTimeBlocks = GetTimeBlocksWithTimeBlockDate(employeeDates.EmployeeId, employeeDates.StartDate, employeeDates.StopDate);
                            SetTimeBlocksToPreliminary(employeeTimeBlocks, false);

                            #endregion

                            #region TimePayrollTransaction

                            List<TimePayrollTransaction> employeeTimePayrollTransactions = GetTimePayrollTransactions(employeeDates.EmployeeId, employeeDates.StartDate, employeeDates.StopDate);
                            SetTimePayrollTransactionsToPreliminary(employeeTimePayrollTransactions, false);

                            #endregion

                            //Save once for each employeee, continue to next if failing
                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                            {
                                if (failedEmployeeIds == null)
                                    failedEmployeeIds = new List<int>();
                                failedEmployeeIds.Add(employeeDates.EmployeeId);
                            }
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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            if (failedEmployeeIds != null && failedEmployeeIds.Any())
                oDTO.Result.Keys = failedEmployeeIds;

            return oDTO;
        }

        private SaveShiftPrelDefOutputDTO TaskSaveShiftDefToPrel()
        {
            var (iDTO, oDTO) = InitTask<SaveShiftPrelDefInputDTO, SaveShiftPrelDefOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            List<int> failedEmployeeIds = null;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    foreach (EmployeeDatesDTO employeeDates in iDTO.EmployeeDates)
                    {
                        if (employeeDates.StartDate > employeeDates.StopDate)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EmployeeScheduleStopDateEarlierThanStartDate, GetText(3592, "Slutdatum tidigare än startdatum"));
                            return oDTO;
                        }
                    }

                    bool useAccountHierarchy = UseAccountHierarchy();

                    // Save copy of original schedule in JSON format
                    bool saveCopy = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningSaveCopyOnPublish);
                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        TimeScheduleCopyHead copyHead = null;
                        if (saveCopy)
                        {
                            // Create head that holds information of the publish and who did it
                            copyHead = new TimeScheduleCopyHead
                            {
                                ActorCompanyId = actorCompanyId,
                                Type = (int)TermGroup_TimeScheduleCopyHeadType.DefToPrel,
                                DateFrom = iDTO.EmployeeDates.First().StartDate,
                                DateTo = iDTO.EmployeeDates.First().StopDate,
                                UserId = userId,
                            };
                            SetCreatedProperties(copyHead);
                            entities.AddToTimeScheduleCopyHead(copyHead);
                        }

                        foreach (EmployeeDatesDTO employeeDates in iDTO.EmployeeDates)
                        {
                            Employee employee = GetEmployeeFromCache(employeeDates.EmployeeId);
                            if (employee == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                                return oDTO;
                            }
                            List<DateTime> notValidDates = new List<DateTime>();
                            List<TimePayrollTransaction> employeeTimePayrollTransactions = GetTimePayrollTransactionsWithTimeBlockDate(employeeDates.EmployeeId, employeeDates.StartDate, employeeDates.StopDate);
                            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);

                            if (attestStateInitial != null && employeeTimePayrollTransactions.Any(w => w.AttestStateId != attestStateInitial.AttestStateId))
                                foreach (var item in employeeTimePayrollTransactions.Where(w => w.AttestStateId != attestStateInitial.AttestStateId).Select(b => b.TimeBlockDate.Date).Distinct())
                                {
                                    if (!notValidDates.Contains(item))
                                        notValidDates.Add(item);
                                }

                            #region TimeScheduleTemplateBlock

                            List<TimeScheduleTemplateBlock> shifts = GetScheduleBlocksForEmployee(null, employeeDates.EmployeeId, employeeDates.StartDate, employeeDates.StopDate, loadStaffingIfUsed: false, includeStandBy: iDTO.IncludeStandbyShifts, includeOnDuty: true).Where(b => !b.IsPreliminary || b.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.AnnualLeave).ToList();
                            if (!iDTO.IncludeScheduleShifts)
                                shifts = shifts.Where(b => b.Type != (int)TermGroup_TimeScheduleTemplateBlockType.Schedule).ToList();

                            if (useAccountHierarchy)
                            {
                                AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
                                input.AddParamValue(AccountHierarchyParamType.IncludeAbstract, true);
                                List<int> validAccountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(entities, actorCompanyId, RoleId, userId, employeeDates.StartDate, employeeDates.StopDate, input);
                                shifts = shifts.Where(b => !b.AccountId.HasValue || validAccountIds.Contains(b.AccountId.Value)).ToList();
                            }

                            TimeScheduleCopyRowJsonDataDTO jsonDataDTO = null;
                            if (saveCopy)
                            {
                                // Create DTO structure for copy row
                                jsonDataDTO = new TimeScheduleCopyRowJsonDataDTO()
                                {
                                    Shifts = new List<TimeScheduleCopyRowJsonDataShiftDTO>()
                                };
                            }

                            foreach (TimeScheduleTemplateBlock shift in shifts.Where(w => w.Date.HasValue && !notValidDates.Contains(w.Date.Value)))
                            {
                                shift.IsPreliminary = true;
                                SetModifiedProperties(shift);

                                if (saveCopy)
                                {
                                    // Add shift to DTO structure
                                    jsonDataDTO.Shifts.Add(shift.ToTimeScheduleCopyRowJsonDataDTO());
                                }
                            }

                            if (saveCopy)
                            {
                                if (useLeisureCodes)
                                {
                                    List<TimeScheduleEmployeePeriodDetail> details = TimeScheduleManager.GetTimeSchedulePlanningLeisureCodes(entities, employeeDates.StartDate, employeeDates.StopDate, new List<int>() { employeeDates.EmployeeId });
                                    foreach (TimeScheduleEmployeePeriodDetail detail in details)
                                    {
                                        jsonDataDTO.Shifts.Add(detail.ToTimeScheduleCopyRowJsonDataDTO());
                                    }
                                }

                                // Create one copy row for each employee published
                                TimeScheduleCopyRow copyRow = new TimeScheduleCopyRow
                                {
                                    TimeScheduleCopyHead = copyHead,
                                    EmployeeId = employeeDates.EmployeeId,
                                    Type = (int)TermGroup_TimeScheduleCopyRowType.Default,
                                    JsonData = jsonDataDTO.ToTimeScheduleCopyRowJsonData()
                                };
                                SetCreatedProperties(copyRow);
                            }

                            #endregion

                            #region TimeBlock

                            List<TimeBlock> employeeTimeBlocks = GetTimeBlocksWithTimeBlockDate(employeeDates.EmployeeId, employeeDates.StartDate, employeeDates.StopDate);
                            employeeTimeBlocks = employeeTimeBlocks.Where(b => !notValidDates.Contains(b.TimeBlockDate.Date)).ToList();
                            SetTimeBlocksToPreliminary(employeeTimeBlocks, true);

                            #endregion

                            #region TimePayrollTransaction

                            employeeTimePayrollTransactions = employeeTimePayrollTransactions.Where(b => !notValidDates.Contains(b.TimeBlockDate.Date)).ToList();
                            SetTimePayrollTransactionsToPreliminary(employeeTimePayrollTransactions, true);

                            #endregion

                            //Save once for each employeee, continue to next if failing
                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                            {
                                if (failedEmployeeIds == null)
                                    failedEmployeeIds = new List<int>();
                                failedEmployeeIds.Add(employeeDates.EmployeeId);
                            }
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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            if (failedEmployeeIds != null && failedEmployeeIds.Any())
                oDTO.Result.Keys = failedEmployeeIds;

            return oDTO;
        }

        private CopyScheduleOutputDTO TaskCopySchedule()
        {
            var (iDTO, oDTO) = InitTask<CopyScheduleInputDTO, CopyScheduleOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if ((iDTO.TargetDateStop.HasValue && iDTO.TargetDateStop < iDTO.TargetDateStart) || (iDTO.SourceDateStop.HasValue && iDTO.SourceDateStop.Value.AddDays(1) < iDTO.TargetDateStart))
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleInvalidDates, GetText(11674, "Schemat måste starta före det slutar"));
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

                        #region Employee

                        Employee sourceEmployee = GetEmployeeFromCache(iDTO.SourceEmployeeId);
                        if (sourceEmployee == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                            return oDTO;
                        }
                        if (sourceEmployee.Hidden)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleHiddenEmployeeNotSupported, GetText(11675, "Kan ej kopiera/överta ledigt pass"));
                            return oDTO;
                        }

                        Employee targetEmployee = GetEmployeeWithContactPersonFromCache(iDTO.TargetEmployeeId); //Need name later
                        if (targetEmployee == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));
                            return oDTO;
                        }
                        if (targetEmployee.Hidden)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleHiddenEmployeeNotSupported, GetText(11675, "Kan ej kopiera/överta ledigt pass"));
                            return oDTO;
                        }
                        if (targetEmployee.EmployeeId == sourceEmployee.EmployeeId)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleSameEmployeeNotSupported, GetText(11676, "Kan ej kopiera/överta schema från sig själv"));
                            return oDTO;
                        }

                        List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

                        #endregion

                        #region Placements

                        DateTime? latestStopDate = CalendarUtility.GetLatestDate(iDTO.SourceDateStop, iDTO.TargetDateStop);

                        //Check if any source placements is overlapping
                        List<EmployeeSchedule> sourcePlacements = GetEmployeeSchedulesForEmployee(sourceEmployee.EmployeeId).Filter(iDTO.TargetDateStart, latestStopDate);
                        if (sourcePlacements.HasOverlapping())
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleEmployeeSourceHasOverlappingPlacements, String.Format(GetText(11681, "Ursprungsperson {0} har överlappande aktiverade scheman. Det måste rättas manuellt"), sourceEmployee.Name));
                            return oDTO;
                        }

                        //Check if any placement ends after the new stop
                        foreach (DateTime sourcePlacementStopDate in sourcePlacements.Select(p => p.StopDate))
                        {
                            if (latestStopDate < sourcePlacementStopDate)
                                latestStopDate = sourcePlacementStopDate;
                        }

                        //Check if target placements overlap source placement
                        List<EmployeeSchedule> targetPlacements = GetEmployeeSchedulesForEmployee(targetEmployee.EmployeeId).Filter(iDTO.TargetDateStart, iDTO.TargetDateStop);
                        if (targetPlacements.IsAnEmployeeScheduleOverlapping(sourcePlacements))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleEmployeeScheduleOverlappingDates, String.Format(GetText(11682, "Det finns aktiverade scheman för målperson {0} inom vald period"), targetEmployee.Name));
                            return oDTO;
                        }

                        #endregion

                        #region Schedule

                        List<TimeScheduleTemplateHead> sourceTemplateHeads = GetPersonalTemplateHeads(sourceEmployee.EmployeeId, iDTO.TargetDateStart, latestStopDate);
                        foreach (int sourcePlacementTemplateHeadId in sourcePlacements.Select(p => p.TimeScheduleTemplateHeadId))
                        {
                            if (sourceTemplateHeads.Any(i => i.TimeScheduleTemplateHeadId == sourcePlacementTemplateHeadId))
                                continue;

                            TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHead(sourcePlacementTemplateHeadId);
                            if (templateHead != null)
                                sourceTemplateHeads.Add(templateHead);
                        }

                        if (HasPersonalTemplateHeadsWithSameStartDate(sourceTemplateHeads))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleSourceHasTemplateHeadsWithSameStartDate, String.Format(GetText(11677, "Ursprungsperson {0} har flera grundscheman med samma startdatum. Det måste rättas manuellt"), sourceEmployee.Name));
                            return oDTO;
                        }

                        oDTO.Result = ValidatePersonalTemplates(0, targetEmployee.EmployeeId, iDTO.TargetDateStart, iDTO.TargetDateStop);
                        if (!oDTO.Result.Success)
                            return oDTO;


                        List<TimeScheduleTemplateHead> targetTemplateHeads = GetPersonalTemplateHeads(targetEmployee.EmployeeId, null, iDTO.TargetDateStop);
                        foreach (int targetPlacementTemplateHeadId in targetPlacements.Select(p => p.TimeScheduleTemplateHeadId))
                        {
                            if (targetTemplateHeads.Any(i => i.TimeScheduleTemplateHeadId == targetPlacementTemplateHeadId))
                                continue;

                            TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHead(targetPlacementTemplateHeadId);
                            if (templateHead != null)
                                targetTemplateHeads.Add(templateHead);
                        }

                        #endregion

                        #region Copy Schedule

                        //Relations between source and target
                        var templateHeadRelationDict = new Dictionary<int, TimeScheduleTemplateHead>();
                        var templatePeriodRelationDict = new Dictionary<int, TimeScheduleTemplatePeriod>();
                        var templateBlockRelationDict = new Dictionary<int, TimeScheduleTemplateBlock>();
                        var linksRelationDict = new Dictionary<string, string>();

                        //All TimeScheduleTemplateBlocks
                        List<TimeScheduleTemplateBlock> allSourceTemplateBlocks = new List<TimeScheduleTemplateBlock>();
                        List<TimeScheduleTemplateBlock> allTargetTemplateBlocksAutogen = new List<TimeScheduleTemplateBlock>();
                        List<TimeScheduleTemplateBlock> allTargetTemplateBlocksStamping = new List<TimeScheduleTemplateBlock>();

                        foreach (TimeScheduleTemplateHead sourceTemplateHead in sourceTemplateHeads)
                        {
                            if (templateHeadRelationDict.ContainsKey(sourceTemplateHead.TimeScheduleTemplateHeadId))
                                continue;

                            #region TimeScheduleTemplateHead

                            TimeScheduleTemplateHead targetTemplateHead = null;

                            if (sourceTemplateHead.EmployeeId.HasValue && sourceTemplateHead.StartDate.HasValue)
                            {
                                #region Copy personal template

                                DateTime newStartDate = iDTO.TargetDateStart;
                                DateTime? newStopDate = iDTO.TargetDateStop;
                                DateTime newFirstMondayOfCycle = CalendarUtility.GetBeginningOfWeek(newStartDate);

                                string targetTemplateName = String.Format("{0} {1}", targetEmployee.Name, newStartDate.ToShortDateString());

                                targetTemplateHead = new TimeScheduleTemplateHead()
                                {
                                    Name = targetTemplateName,
                                    Description = sourceTemplateHead.Description,
                                    NoOfDays = sourceTemplateHead.NoOfDays,
                                    StartOnFirstDayOfWeek = sourceTemplateHead.StartOnFirstDayOfWeek,
                                    FlexForceSchedule = sourceTemplateHead.FlexForceSchedule,
                                    StartDate = newStartDate,
                                    StopDate = newStopDate,
                                    FirstMondayOfCycle = newFirstMondayOfCycle,

                                    //Set FK
                                    EmployeeId = targetEmployee.EmployeeId,
                                    ActorCompanyId = sourceTemplateHead.ActorCompanyId,
                                };
                                SetCreatedProperties(targetTemplateHead);
                                entities.TimeScheduleTemplateHead.AddObject(targetTemplateHead);

                                #endregion
                            }
                            else
                            {
                                #region Use none-personal TimeScheduleTemplateHead

                                targetTemplateHead = sourceTemplateHead;

                                #endregion
                            }

                            //Keep relation
                            templateHeadRelationDict.Add(sourceTemplateHead.TimeScheduleTemplateHeadId, targetTemplateHead);

                            #endregion

                            #region TimeScheduleTemplatePeriod

                            if (!sourceTemplateHead.TimeScheduleTemplatePeriod.IsLoaded)
                                sourceTemplateHead.TimeScheduleTemplatePeriod.Load();

                            foreach (TimeScheduleTemplatePeriod sourceTemplatePeriod in sourceTemplateHead.TimeScheduleTemplatePeriod)
                            {
                                if (templatePeriodRelationDict.ContainsKey(sourceTemplatePeriod.TimeScheduleTemplatePeriodId))
                                    continue;

                                #region TimeScheduleTemplatePeriod

                                TimeScheduleTemplatePeriod targetTemplatePeriod = null;

                                if (sourceTemplateHead.EmployeeId.HasValue)
                                {
                                    #region Copy personal TimeScheduleTemplatePeriod

                                    targetTemplatePeriod = new TimeScheduleTemplatePeriod()
                                    {
                                        DayNumber = sourceTemplatePeriod.DayNumber,

                                        //Set references
                                        TimeScheduleTemplateHead = targetTemplateHead,
                                    };
                                    SetCreatedProperties(targetTemplatePeriod);
                                    entities.TimeScheduleTemplatePeriod.AddObject(targetTemplatePeriod);

                                    #endregion
                                }
                                else
                                {
                                    #region Use none-personal TimeScheduleTemplatePeriod

                                    targetTemplatePeriod = sourceTemplatePeriod;

                                    #endregion
                                }

                                //Keep relation
                                templatePeriodRelationDict.Add(sourceTemplatePeriod.TimeScheduleTemplatePeriodId, targetTemplatePeriod);

                                #endregion
                            }

                            #endregion

                            #region TimeScheduleTemplateBlock

                            List<TimeScheduleTemplateBlock> sourceTemplateBlocks = GetScheduleBlocksForEmployeeAndTemplateHeadWithStaffingAndAccounting(null, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceEmployee.EmployeeId, iDTO.TargetDateStart, latestStopDate);
                            if (sourceTemplateHead.EmployeeId.HasValue)
                                sourceTemplateBlocks.InsertRange(0, GetTemplateScheduleBlocksForTemplateHeadWithEmployeePeriodAndAccounts(null, sourceTemplateHead.TimeScheduleTemplateHeadId));

                            //Add to collection
                            allSourceTemplateBlocks.AddRange(sourceTemplateBlocks);

                            #region Validate account

                            // If template has shifts without account,
                            // target employee must belong to an employee group that allows it.
                            if (iDTO.UseAccountingFromSourceSchedule && UseAccountHierarchy() && sourceTemplateBlocks.Any(s => !s.IsBreak && !s.AccountId.HasValue && s.StartTime != s.StopTime))
                            {
                                EmployeeGroup employeeGroup = targetEmployee.GetEmployeeGroup(iDTO.TargetDateStart, GetEmployeeGroupsFromCache());
                                if (employeeGroup != null && !employeeGroup.AllowShiftsWithoutAccount)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeGroupDoesNotAllowShiftsWithoutAccount, GetText(12042, "Den anställdes tidavtal tillåter inte pass utan tillhörighet"));
                                    return oDTO;
                                }
                            }

                            #endregion

                            foreach (TimeScheduleTemplateBlock sourceTemplateBlock in sourceTemplateBlocks.OrderBy(i => i.Date))
                            {
                                #region Prereq

                                EmployeeGroup sourceEmployeeGroup = sourceEmployee.GetEmployeeGroup(sourceTemplateBlock.Date, employeeGroups: employeeGroups);
                                if (sourceEmployeeGroup == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), sourceEmployee.EmployeeNr, sourceTemplateBlock.Date.ToShortDateString()));
                                    return oDTO;
                                }

                                bool isTemplateBlock = !sourceTemplateBlock.Date.HasValue;
                                if (isTemplateBlock)
                                {
                                    //Do not copy template from none-personal
                                    if (!sourceTemplateHead.EmployeeId.HasValue)
                                        continue;
                                }
                                else
                                {
                                    if (!iDTO.IsDateValidForCopySchedule(sourceTemplateBlock.Date.Value))
                                        continue;
                                }

                                TimeScheduleTemplatePeriod targetTemplatePeriod = null;
                                if (sourceTemplateBlock.TimeScheduleTemplatePeriodId.HasValue && templatePeriodRelationDict.ContainsKey(sourceTemplateBlock.TimeScheduleTemplatePeriodId.Value))
                                    targetTemplatePeriod = templatePeriodRelationDict[sourceTemplateBlock.TimeScheduleTemplatePeriodId.Value];
                                if (targetTemplatePeriod == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplatePeriod");
                                    return oDTO;
                                }

                                TimeScheduleEmployeePeriod targetEmployeePeriod = null;
                                if (sourceTemplateBlock.Date.HasValue)
                                {
                                    CreateTimeBlockDateIfNotExists(targetEmployee.EmployeeId, sourceTemplateBlock.Date.Value);
                                    targetEmployeePeriod = CreateTimeScheduleEmployeePeriodIfNotExist(sourceTemplateBlock.Date.Value, targetEmployee.EmployeeId, out _);
                                }

                                #endregion

                                #region Copy TimeScheduleTemplateBlock

                                TimeScheduleTemplateBlock targetTemplateBlock = new TimeScheduleTemplateBlock()
                                {
                                    StartTime = sourceTemplateBlock.StartTime,
                                    StopTime = sourceTemplateBlock.StopTime,
                                    Date = sourceTemplateBlock.Date,
                                    Description = sourceTemplateBlock.Description,
                                    BreakNumber = sourceTemplateBlock.BreakNumber,
                                    BreakType = sourceTemplateBlock.BreakType,
                                    IsPreliminary = sourceTemplateBlock.IsPreliminary,
                                    TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.None, //Do not copy
                                    ShiftStatus = (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned, //Do not copy
                                    ShiftUserStatus = (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted, //Do not copy
                                    NbrOfWantedInQueue = 0, //Do not copy
                                    NbrOfSuggestionsInQueue = 0, //Do not copy

                                    //Set FK
                                    EmployeeId = targetEmployee.EmployeeId,
                                    TimeCodeId = sourceTemplateBlock.TimeDeviationCauseId.HasValue ? GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode) : sourceTemplateBlock.TimeCodeId,
                                    TimeHalfdayId = sourceTemplateBlock.TimeHalfdayId,
                                    TimeScheduleTypeId = sourceTemplateBlock.TimeScheduleTypeId,
                                    ShiftTypeId = sourceTemplateBlock.ShiftTypeId,
                                    AccountId = sourceTemplateBlock.AccountId != null && !sourceTemplateBlock.IsZeroBlock() ? sourceTemplateBlock.AccountId : null, // Zerodays should not have any account
                                    TimeDeviationCauseId = null, //Do not copy
                                    EmployeeChildId = null, //Do not copy
                                    RecalculateTimeRecordId = null, //Do not copy

                                    //Set references
                                    TimeScheduleEmployeePeriod = targetEmployeePeriod,
                                    TimeScheduleTemplatePeriod = targetTemplatePeriod,
                                };
                                SetCreatedProperties(targetTemplateBlock);
                                entities.TimeScheduleTemplateBlock.AddObject(targetTemplateBlock);

                                if (sourceTemplateBlock.AccountId.HasValue && targetTemplateBlock.IsBreak)
                                    targetTemplateBlock.TimeCodeId = GetTimeCodeBreakIdForEmployee(targetTemplateBlock, targetEmployee.EmployeeId);

                                //Set Link
                                SetTimeScheduleTemplateBlockLinked(targetTemplateBlock, sourceTemplateBlock.Link, linksRelationDict);

                                //Set accounting

                                if (!sourceTemplateBlock.IsZeroBlock())
                                {
                                    if (iDTO.UseAccountingFromSourceSchedule)
                                    {
                                        AddAccountInternalsToTimeScheduleTemplateBlock(targetTemplateBlock, sourceTemplateBlock.AccountInternal);
                                    }
                                    else
                                    {
                                        var date = targetTemplateHead.StartDate ?? DateTime.Today;
                                        var employeeAccounts = GetEmployeeAccounts(targetEmployee.EmployeeId, date, date);

                                        if (!employeeAccounts.IsNullOrEmpty())
                                        {
                                            var selectAccount = employeeAccounts.FirstOrDefault(f => f.Default);

                                            if (selectAccount == null)
                                                selectAccount = employeeAccounts.FirstOrDefault(f => f.Default);

                                            if (selectAccount != null)
                                                targetTemplateBlock.AccountId = selectAccount.AccountId;
                                        }

                                        targetTemplateBlock.AccountInternal.Clear();
                                        ApplyAccountingPrioOnTimeScheduleTemplateBlock(targetTemplateBlock, targetEmployee);
                                    }
                                }

                                //Keep relation
                                templateBlockRelationDict.Add(sourceTemplateBlock.TimeScheduleTemplateBlockId, targetTemplateBlock);

                                //Add to collection
                                if (sourceEmployeeGroup.AutogenTimeblocks)
                                    allTargetTemplateBlocksAutogen.Add(targetTemplateBlock);
                                else
                                    allTargetTemplateBlocksStamping.Add(targetTemplateBlock);

                                #endregion
                            }

                            #endregion
                        }

                        #endregion

                        #region Copy Placements

                        //Relations between source and target
                        var placementsTransitionDict = new Dictionary<int, EmployeeSchedule>();

                        //All TimeBlocks
                        List<TimeBlock> allSourceTimeBlocksOutOfRange = new List<TimeBlock>();

                        foreach (EmployeeSchedule sourcePlacement in sourcePlacements)
                        {
                            #region Copy EmployeeSchedule

                            #region Prereq

                            //TimeScheduleTemplateHeads
                            TimeScheduleTemplateHead sourceTemplateHead = sourceTemplateHeads.FirstOrDefault(i => i.TimeScheduleTemplateHeadId == sourcePlacement.TimeScheduleTemplateHeadId);
                            TimeScheduleTemplateHead targetTemplateHead = templateHeadRelationDict.ContainsKey(sourcePlacement.TimeScheduleTemplateHeadId) ? templateHeadRelationDict[sourcePlacement.TimeScheduleTemplateHeadId] : null;
                            if (sourceTemplateHead == null || targetTemplateHead == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.CopyScheduleEmployeeScheduleTemplateNotFound, GetText(11683, "Grundschema hittades inte"));
                                return oDTO;
                            }

                            //Dates
                            DateTime targetPlacementStartDate = iDTO.TargetDateStart;
                            DateTime targetPlacementStopDate = iDTO.TargetDateStop.HasValue && iDTO.TargetDateStop.Value <= sourcePlacement.StopDate ? iDTO.TargetDateStop.Value : sourcePlacement.StopDate;
                            DateTime sourcePlacementStopDate = iDTO.SourceDateStop.HasValue && iDTO.SourceDateStop.Value <= sourcePlacement.StopDate ? iDTO.SourceDateStop.Value : sourcePlacement.StopDate;
                            DateTime? sourcePlacementRemoveStartDate = iDTO.IsDateValidForShortenSchedule(sourcePlacement.StopDate) ? sourcePlacementStopDate.AddDays(1) : (DateTime?)null;

                            DateTime date = targetPlacementStartDate;
                            while (date <= targetPlacementStopDate)
                            {
                                EmployeeGroup sourceEmployeeGroup = sourceEmployee.GetEmployeeGroup(date, employeeGroups: employeeGroups);
                                if (sourceEmployeeGroup == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), sourceEmployee.EmployeeNr, date.ToShortDateString()));
                                    return oDTO;
                                }

                                EmployeeGroup targetEmployeeGroup = targetEmployee.GetEmployeeGroup(date, employeeGroups: employeeGroups);
                                if (targetEmployeeGroup == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), targetEmployee.EmployeeNr, date.ToShortDateString()));
                                    return oDTO;
                                }

                                date = date.AddDays(1);
                            }

                            //StartDayNumber
                            int startDayNumber = 1;
                            if (targetTemplateHead.StartDate.HasValue && targetTemplateHead.FirstMondayOfCycle.HasValue && targetTemplateHead.StartDate.Value != targetTemplateHead.FirstMondayOfCycle.Value)
                                startDayNumber = ((int)(targetTemplateHead.StartDate.Value - targetTemplateHead.FirstMondayOfCycle.Value).TotalDays) + 1;

                            //TimeScheduleTemplateBlocks
                            List<TimeScheduleTemplateBlock> targetPlacementTemplateBlocksAutogen = allTargetTemplateBlocksAutogen.Where(i => i.Date >= targetPlacementStartDate && i.Date <= targetPlacementStopDate).OrderBy(i => i.Date).ToList();
                            List<TimeScheduleTemplateBlock> targetPlacementTemplateBlocksStamping = allTargetTemplateBlocksStamping.Where(i => i.Date >= targetPlacementStartDate && i.Date <= targetPlacementStopDate).OrderBy(i => i.Date).ToList();

                            //TimeBlocks
                            if (sourcePlacementRemoveStartDate.HasValue)
                            {
                                List<TimeBlock> timeBlocks = GetTimeBlocksWithTransactions(sourceEmployee.EmployeeId, sourcePlacementRemoveStartDate.Value, null);

                                //Cannot shorten schedule with manually adjusted TimeBlocks and transactions
                                oDTO.Result = IsOkToDeleteEmployeeSchedule(sourceEmployee.EmployeeNrAndName, timeBlocks);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                allSourceTimeBlocksOutOfRange.AddRange(timeBlocks);
                            }

                            #endregion

                            EmployeeSchedule targetEmployeeSchedule = new EmployeeSchedule()
                            {
                                StartDate = targetPlacementStartDate,
                                StopDate = targetPlacementStopDate,
                                StartDayNumber = startDayNumber,
                                IsPreliminary = sourcePlacement.IsPreliminary,

                                //Set references
                                TimeScheduleTemplateHead = targetTemplateHead,

                                //Set FK
                                EmployeeId = targetEmployee.EmployeeId,
                            };
                            SetCreatedProperties(targetEmployeeSchedule);
                            entities.EmployeeSchedule.AddObject(targetEmployeeSchedule);

                            //Add TimeScheduleTemplateBlocks autogen (deviation)
                            foreach (TimeScheduleTemplateBlock targetPlacementTemplateBlock in targetPlacementTemplateBlocksAutogen)
                            {
                                //Set EmployeeSchedule
                                targetPlacementTemplateBlock.EmployeeSchedule = targetEmployeeSchedule;

                                //Add TimeScheduleTemplateBlock to create TimeBlock's and transactions for later
                                iDTO.AsyncTemplateBlocks.Add(targetPlacementTemplateBlock);
                            }

                            //Add TimeScheduleTemplateBlocks stamping
                            foreach (TimeScheduleTemplateBlock targetPlacementTemplateBlock in targetPlacementTemplateBlocksStamping)
                            {
                                //Set EmployeeSchedule
                                targetPlacementTemplateBlock.EmployeeSchedule = targetEmployeeSchedule;
                            }

                            //Add relation
                            placementsTransitionDict.Add(sourcePlacement.EmployeeScheduleId, targetEmployeeSchedule);

                            #endregion
                        }

                        #endregion

                        #region Remove placement

                        foreach (var sourcePlacement in sourcePlacements)
                        {
                            //Check date range
                            if (!iDTO.SourceDateStop.HasValue || iDTO.SourceDateStop.Value >= sourcePlacement.StopDate)
                                continue;

                            EmployeeSchedule employeeSchedule = GetEmployeeScheduleFromCache(sourceEmployee.EmployeeId, sourcePlacement.EmployeeScheduleId);
                            if (employeeSchedule == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeSchedule");
                                return oDTO;
                            }

                            if (sourcePlacement.StartDate > iDTO.SourceDateStop.Value)
                            {
                                //Delete
                                oDTO.Result = ChangeEntityState(employeeSchedule, SoeEntityState.Deleted);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                            else
                            {
                                //Shorten
                                employeeSchedule.StopDate = iDTO.SourceDateStop.Value;
                                SetModifiedProperties(employeeSchedule);
                            }
                        }

                        #endregion

                        #region Remove Schedule

                        List<TimeScheduleTemplateBlock> sourceTemplateBlocksToDelete = new List<TimeScheduleTemplateBlock>();
                        foreach (var sourceTemplateBlock in allSourceTemplateBlocks.OrderBy(i => i.Date))
                        {
                            //Do not remove template
                            if (!sourceTemplateBlock.Date.HasValue)
                                continue;
                            //Check date range
                            if (!iDTO.SourceDateStop.HasValue || iDTO.SourceDateStop >= sourceTemplateBlock.Date)
                                continue;


                            sourceTemplateBlocksToDelete.Add(sourceTemplateBlock);
                        }

                        //Delete
                        oDTO.Result = SetTimeScheduleBlocksToDeleted(sourceTemplateBlocksToDelete, saveChanges: false);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Remove TimeBlocks and transactions

                        //Delete
                        oDTO.Result = SetTimeBlocksAndTransactionsToDeleted(allSourceTimeBlocksOutOfRange, saveChanges: false);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Save

                        if (oDTO.Result.Success)
                            oDTO.Result = Save();

                        if (oDTO.Result.Success && !iDTO.CreateTimeBlocksAndTransactionsAsync)
                            oDTO.Result = SaveTimeBlocksAndTransactionsFromTemplate(iDTO.AsyncTemplateBlocks);

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

            #region Create TimeBlocks and transactions (async)

            if (oDTO.Result.Success && iDTO.CreateTimeBlocksAndTransactionsAsync && iDTO.AsyncTemplateBlocks.Count > 0)
            {
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SaveTimeBlocksAndTransactionsFromTemplateAsync(iDTO)));
            }

            #endregion

            return oDTO;
        }

        private GenerateAbsenceFromStaffingOutputDTO TaskGenerateAndSaveAbsenceFromStaffing()
        {
            var (iDTO, oDTO) = InitTask<GenerateAndSaveAbsenceFromStaffingInputDTO, GenerateAbsenceFromStaffingOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if ((iDTO == null) || iDTO.TimeDeviationCauseId <= 0 || iDTO.EmployeeId <= 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }
            if (iDTO.Shifts.Any(x => x.EmployeeId == iDTO.EmployeeId))
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, GetText(8832, "Ej tillåtet att tilldela ett pass till samma anställd som ska få frånvaro."));
                return oDTO;
            }

            #region Prereq

            var shifts = iDTO.Shifts.Where(x => !x.IsLended).ToList();
            oDTO.Result = ValidateShiftsAgainstGivenScenario(shifts, iDTO.TimeScheduleScenarioHeadId);
            if (!oDTO.Result.Success)
                return oDTO;

            #endregion

            #region Perform: Save

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Init

                        Guid batchId = GetNewBatchLink();
                        ShiftHistoryLogCallStackProperties logProperties = null;

                        TimeDeviationCause deviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(iDTO.TimeDeviationCauseId);
                        if (deviationCause == null)
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");
                            return oDTO;
                        }

                        var firstDate = shifts.Any() ? shifts.Min(x => x.ActualDate).Date : (DateTime?)null;
                        var lastDate = shifts.Any() ? shifts.Max(x => x.ActualDate).Date : (DateTime?)null;
                        List<int> affectedEmployeeids = new List<int>();
                        var updateSummeryAfterSave = shifts.Count > 1;

                        int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                        bool extraShiftAsDefaultOnHidden = GetCompanyBoolSettingFromCache(CompanySettingType.ExtraShiftAsDefaultOnHidden);
                        bool removeScheduleTypeOnAbsence = GetCompanyBoolSettingFromCache(CompanySettingType.RemoveScheduleTypeOnAbsence);

                        #endregion

                        #region Perform

                        #region Split shifts and save original shift

                        var dates = shifts.Select(tb => tb.ActualDate).Distinct().ToList();
                        if (!dates.IsNullOrEmpty())
                        {
                            AddTimeScheduleTemplatePeriodsToCache(iDTO.EmployeeId, dates.Min(), dates.Max());
                            AddTimeScheduleEmployeePeriodsToCache(iDTO.EmployeeId, dates.Min(), dates.Max());
                            AddTimeBlockDatesToCache(iDTO.EmployeeId, dates);
                        }

                        List<TimeScheduleTemplateBlockTaskDTO> allShiftTasks = GetTimeScheduleTemplateBlockTasks(shifts.Select(s => s.TimeScheduleTemplateBlockId).ToList()).ToDTOs().ToList();

                        foreach (TimeSchedulePlanningDayDTO shift in shifts.Where(x => x.ApprovalTypeId == (int)TermGroup_YesNo.Yes).ToList())
                        {
                            oDTO.Result = SplitTasks(shift.TimeScheduleTemplateBlockId, new List<DateTime>() { shift.AbsenceStartTime, shift.AbsenceStopTime });
                            if (!oDTO.Result.Success)
                                return oDTO;

                            List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = allShiftTasks.Where(s => s.TimeScheduleTemplateBlockId == shift.TimeScheduleTemplateBlockId).ToList();

                            List<TimeSchedulePlanningDayDTO> newShifts = GetAbsenceDividedShifts(shift);
                            foreach (TimeSchedulePlanningDayDTO newShift in newShifts)
                            {
                                #region ShiftHistory

                                logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.AbsencePlanning, null, true);

                                logProperties.AbsenceForEmployeeId = iDTO.EmployeeId;
                                newShift.EmployeeId = iDTO.EmployeeId;

                                oDTO.Result = SaveTimeScheduleShiftAndLogChanges(newShift, logProperties, true, updateScheduledTimeSummary: !updateSummeryAfterSave);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                affectedEmployeeids.Add(newShift.EmployeeId);

                                #endregion

                                #region Tasks (new shift is created from originalshift, connect tasks within the new shift)

                                oDTO.Result = ConnectTasksWithinShift(shiftTasks, newShift);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion
                            }

                            logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.AbsencePlanning, null, true);

                            logProperties.AbsenceForEmployeeId = iDTO.EmployeeId;

                            int newEmployeeid = shift.EmployeeId;
                            shift.BelongsToPreviousDay = (shift.AbsenceStartTime.Date > shift.StartTime.Date);
                            shift.BelongsToNextDay = (shift.AbsenceStartTime.Date < shift.StartTime.Date);

                            // Save original with new start and stop
                            shift.StartTime = shift.AbsenceStartTime;  //new start is absence start
                            shift.StopTime = shift.AbsenceStopTime; //new stop is absence stop
                            shift.EmployeeId = iDTO.EmployeeId; //shift.EmployeeId has been set to destination employeeid in GUI, change it temporarily to originalemployee

                            oDTO.Result = SaveTimeScheduleShiftAndLogChanges(shift, logProperties, true, updateScheduledTimeSummary: !updateSummeryAfterSave);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            affectedEmployeeids.Add(shift.EmployeeId);
                            affectedEmployeeids.Add(newEmployeeid);
                            shift.EmployeeId = newEmployeeid; //restore to new employee

                            //check companysetting for setting this as extrashift when target is Hidden
                            if (shift.EmployeeId == hiddenEmployeeId && extraShiftAsDefaultOnHidden)
                                shift.ExtraShift = true;

                            // Check company setting if schedule type should be removed
                            if (removeScheduleTypeOnAbsence && shift.TimeScheduleTypeId != 0)
                                shift.TimeScheduleTypeId = 0;
                        }

                        #endregion

                        #region Calculate absence

                        if (oDTO.Result.Success && !iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            InitAbsenceDays(iDTO.EmployeeId, shifts.GetDates(), iDTO.EmployeeRequest.GetRatio(), timeDeviationCauseId: iDTO.TimeDeviationCauseId);
                            InitEvaluatePriceFormulaInputDTO(employeeIds: shifts.GetEmployeeIds(iDTO.EmployeeId));
                            InitEmployeeSettingsCache(iDTO.EmployeeId, shifts.GetStartDate(), shifts.GetStopDate());

                            foreach (var shiftsGroupedByDate in shifts.GroupBy(x => x.ActualDate.Date))
                            {
                                List<TimeSchedulePlanningDayDTO> approvedShifts = shiftsGroupedByDate.Where(i => i.ApprovalTypeId == ((int)TermGroup_YesNo.Yes)).ToList();
                                List<TimeSchedulePlanningDayDTO> absenceShifts = approvedShifts.Where(x => x.StartTime != x.StopTime).ToList();
                                List<TimeSchedulePlanningDayDTO> zeroShifts = approvedShifts.Where(x => x.StartTime == x.StopTime).ToList();

                                if (absenceShifts.Any())
                                {
                                    oDTO.Result = SaveDeviationsFromShifts(absenceShifts, iDTO.EmployeeId, iDTO.TimeDeviationCauseId, true, iDTO.EmployeeChildId, iDTO.TimeScheduleScenarioHeadId, recalculateRelatedDays: false, comment: iDTO.Comment, ratio: iDTO.EmployeeRequest.GetRatio());
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                                else if (zeroShifts.Any())
                                {
                                    oDTO.Result = SaveWholedayDeviations(zeroShifts.ToTimeBlockDTOs(), iDTO.TimeDeviationCauseId, iDTO.TimeDeviationCauseId, "", TermGroup_TimeDeviationCauseType.Absence, iDTO.EmployeeId, iDTO.EmployeeChildId, true, recalculateRelatedDays: false);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                            }

                            if (oDTO.Result.Success)
                                oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.ApplyAndRestore, iDTO.EmployeeId);
                        }

                        #endregion

                        #region Absence planning

                        logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.AbsencePlanning, null, iDTO.SkipXEMailOnShiftChanges);
                        List<TimeSchedulePlanningDayDTO> originalShiftCopies = new List<TimeSchedulePlanningDayDTO>();

                        if (!shifts.IsNullOrEmpty())
                        {
                            #region Absence Planning

                            var shiftsApproved = shifts.Where(x => x.ApprovalTypeId == (int)TermGroup_YesNo.Yes).ToList();
                            var scheduleBlocks = GetScheduleBlocks(shiftsApproved.Select(s => s.TimeScheduleTemplateBlockId).ToList());
                            foreach (var shift in shiftsApproved)
                            {
                                var scheduleBlock = scheduleBlocks.FirstOrDefault(b => b.TimeScheduleTemplateBlockId == shift.TimeScheduleTemplateBlockId);
                                if (scheduleBlock != null)
                                {
                                    originalShiftCopies.Add(scheduleBlock.ToTimeSchedulePlanningDayDTO());
                                    scheduleBlock.TimeDeviationCauseId = iDTO.TimeDeviationCauseId;
                                    scheduleBlock.EmployeeChildId = iDTO.EmployeeChildId;
                                    if (deviationCause.TimeCode != null)
                                        scheduleBlock.TimeCode = deviationCause.TimeCode;

                                    scheduleBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.Planned;
                                    SetShiftUserStatus(scheduleBlock.TimeScheduleTemplateBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved);
                                }
                            }

                            oDTO.Result = PerformAbsencePlanning(shifts, batchId, iDTO.SkipXEMailOnShiftChanges, iDTO.TimeScheduleScenarioHeadId);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = LogShiftChanges(originalShiftCopies, logProperties);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion
                        }

                        #endregion

                        #endregion

                        if (!iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            #region RestoreToSchedule

                            oDTO.Result = RestoreCurrentDaysToSchedule(deleteOnlyTimeBlockDateDetailsWithoutRatio: true);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region ExtraShift

                            oDTO.Result = SetHasUnhandledShiftChanges();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Notify

                            if (oDTO.Result.Success)
                            {
                                DoNotifyChangeOfDeviations();
                                DoInitiatePayrollWarnings();
                            }

                            SendXEMailOnDayChanged(iDTO.EmployeeId);

                            #endregion
                        }

                        if (firstDate.HasValue && lastDate.HasValue && updateSummeryAfterSave && base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, firstDate.Value))
                        {
                            foreach (var employeeId in affectedEmployeeids.Distinct())
                                TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employeeId, firstDate.Value, lastDate.Value);
                        }

                        if (base.HasCalculatePayrollOnChanges(entities, actorCompanyId))
                        {
                            foreach (var employeeId in affectedEmployeeids.Distinct())
                                CalculatePayroll(employeeId, firstDate.Value, lastDate.Value);
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
                        //Send XEmail      
                        if (shifts != null && shifts.Count > 0 && !iDTO.SkipXEMailOnShiftChanges && !iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            SendXEMailOnAbsencePlanning(shifts, iDTO.EmployeeId, iDTO.TimeDeviationCauseId);
                        }

                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            #endregion

            return oDTO;
        }

        private int GetScheduleMinutesForEmployee(int employeeId, DateTime dateFrom, DateTime dateTo, List<TimeScheduleType> excludeTimeScheduleTypes = null)
        {
            var scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employeeId, dateFrom, dateTo);
            if (excludeTimeScheduleTypes != null)
                scheduleBlocks = scheduleBlocks.Filter(excludeTimeScheduleTypes);

            return scheduleBlocks.GetWorkMinutes();
        }

        private bool HasDaySchedule(int employeeId, DateTime date)
        {
            if (HasDayScheduleBlockMinutes(employeeId, date))
                return true;
            else if (HasPayrollStartValueRowScheduleMinutesFromCache(employeeId, date))
                return true;
            return false;
        }

        private bool HasDayScheduleBlockMinutes(int employeeId, DateTime date)
        {
            return GetScheduleBlocksFromCache(employeeId, date).GetWork().GetMinutes() > 0;
        }

        #endregion

        #region Schedule Active - Payroll

        private ActionResult CalculatePayroll(int employeeId, DateTime from, DateTime to)
        {
            var employee = GetEmployeeFromCache(employeeId);
            if (employee != null)
            {
                var timePeriodFrom = GetTimePeriodForEmployee(employee, from);

                if (timePeriodFrom == null)
                    return new ActionResult(false);

                var timePeriodTo = GetTimePeriodForEmployee(employee, to);

                if (timePeriodTo == null)
                    return new ActionResult(false);

                var dates = CalendarUtility.GetDates(timePeriodFrom.StartDate, timePeriodFrom.StopDate);
                dates.AddRange(CalendarUtility.GetDates(timePeriodTo.StartDate, timePeriodTo.StopDate));
                dates = dates.Distinct().ToList();
                var dict = new Dictionary<int, List<DateTime>>();
                dict.Add(employeeId, dates);
                return SaveTimePayrollScheduleTransactions(employee, GetTimeBlockDatesFromCache(employeeId, dates), timePeriodFrom, true, true);
            }
            return new ActionResult(false);
        }

        #endregion

        #region Schedule - breaks

        private GetBreaksForScheduleBlockOutputDTO TaskGetBreaksForScheduleBlock()
        {
            var (iDTO, oDTO) = InitTask<GetBreaksForScheduleBlockInputDTO, GetBreaksForScheduleBlockOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.ScheduleBlockBreaks = GetBreaksForScheduleBlock(iDTO.ScheduleBlock);
            }

            return oDTO;
        }

        private HasEmployeeValidTimeCodeBreakOutputDTO TaskHasEmployeeValidTimeCodeBreak()
        {
            var (iDTO, oDTO) = InitTask<HasEmployeeValidTimeCodeBreakInputDTO, HasEmployeeValidTimeCodeBreakOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.Result = HasEmployeeValidTimeCodeBreak(iDTO.Date, iDTO.TimeCodeId, iDTO.EmployeeId);
            }

            return oDTO;
        }

        private ValidateBreakChangeOutputDTO TaskValidateBreakChange()
        {
            var (iDTO, oDTO) = InitTask<ValidateBreakChangeInputDTO, ValidateBreakChangeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.ValidationResult = ValidateBreakChange(iDTO.EmployeeId, iDTO.TimeScheduleTemplateBlockId, iDTO.TimeScheduleTemplatePeriodId, iDTO.TimeCodeBreakId, iDTO.StartTime, iDTO.BreakLength, iDTO.IsTemplate, iDTO.TimeScheduleScenarioHeadId);
            }

            return oDTO;
        }

        #endregion

        #region Scenario

        private SaveTimeScheduleScenarioHeadOutputDTO TaskSaveTimeScheduleScenarioHead()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeScheduleScenarioHeadInputDTO, SaveTimeScheduleScenarioHeadOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            List<TimeSchedulePlanningDayDTO> templateShifts = new List<TimeSchedulePlanningDayDTO>();
            if (iDTO.ScenarioHeadInput.TimeScheduleScenarioHeadId == 0 && iDTO.ScenarioHeadInput.SourceType == TermGroup_TimeScheduleScenarioHeadSourceType.Template)
            {
                using (CompEntities taskEntities = new CompEntities())
                {
                    InitContext(taskEntities);

                    DateTime from = iDTO.ScenarioHeadInput.SourceDateFrom ?? iDTO.ScenarioHeadInput.DateFrom;
                    DateTime to = iDTO.ScenarioHeadInput.SourceDateTo ?? iDTO.ScenarioHeadInput.DateTo;

                    //Must be prefetched in a separate entities, because it changes entites that should not be saved
                    templateShifts = TimeScheduleManager.GetTimeSchedulePlanningDaysFromTemplate(this.entities, actorCompanyId, 0, UserId, from, to, null, iDTO.ScenarioHeadInput.Employees.Select(e => e.EmployeeId).ToList(), loadAccounts: true);
                }
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = CreateTransactionScope(TimeSpan.FromMinutes(30), IsolationLevel.ReadCommitted))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveTimeScheduleScenarioHeadAndCreateShifts(iDTO.ScenarioHeadInput, iDTO.TimeScheduleScenarioHeadId, iDTO.IncludeAbsence, iDTO.DateFunction, templateShifts);

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

        private RemoveAbsenceInScenarioOutputDTO TaskRemoveAbsenceInScenario()
        {
            var (iDTO, oDTO) = InitTask<RemoveAbsenceInScenarioInputDTO, RemoveAbsenceInScenarioOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Items == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull);
                return oDTO;
            }
            if (iDTO.Items.Count == 0)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        if (iDTO.Items.Any())
                        {
                            foreach (var inputItemsByEmployee in iDTO.Items.GroupBy(i => i.EmployeeId))
                            {
                                Employee employee = GetEmployeeFromCache(inputItemsByEmployee.Key);
                                if (employee == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                                    return oDTO;
                                }

                                foreach (AttestEmployeeDaySmallDTO day in inputItemsByEmployee.OrderBy(i => i.Date))
                                {
                                    List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, day.Date, iDTO.TimeScheduleScenarioHeadId, includeStandBy: true);
                                    oDTO.Result = ClearTimeScheduleTemplateBlocksAndApplyAccounting(templateBlocks, employee, clearScheduledAbsence: true, clearScheduledPlacement: true, updateOrderRemainingTime: true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                            }

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;
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

        private ActivateScenarioOutputDTO TaskActivateScenario()
        {
            var (iDTO, oDTO) = InitTask<ActivateScenarioInputDTO, ActivateScenarioOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                taskEntities.CommandTimeout = 300;
                InitContext(taskEntities);

                try
                {
                    oDTO.Result = ActivateScenario(iDTO.TimeScheduleScenarioHeadId, iDTO.Rows, iDTO.SendMessage, iDTO.PreliminaryDateFrom);

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

        private ActivateScenarioOutputDTO TaskCreateTemplateFromScenario()
        {
            var (iDTO, oDTO) = InitTask<CreateTemplateFromScenarioInputDTO, ActivateScenarioOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = CreateTransactionScope(TimeSpan.FromMinutes(30), IsolationLevel.ReadCommitted))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = CreateTemplateFromScenario(iDTO.TimeScheduleScenarioHeadId, iDTO.DateFrom, iDTO.DateTo, iDTO.WeekInCycle);

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

        #region Placement

        private SaveEmployeeSchedulePlacementOutputDTO TaskSaveEmployeeSchedulePlacement()
        {
            var (iDTO, oDTO) = InitTask<SaveEmployeeSchedulePlacementInputDTO, SaveEmployeeSchedulePlacementOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Items.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "SaveEmployeeSchedulePlacementItem");
                return oDTO;
            }
            if (iDTO.Control == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "ActivateScheduleControlDTO");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.Result = ValidateInitiatedEmployeePlacements(iDTO.Items);
                if (!oDTO.Result.Success)
                    return oDTO;

                List<int> employeeIds = iDTO.Items.Select(s => s.EmployeeId).Distinct().ToList();
                AddEmployeeSchedulesToCache(GetEmployeeSchedules(employeeIds));
                AddTimeScheduleTemplateHeadsToCache(GetPersonalTemplateHeads(employeeIds));

                bool hasTemplateGroups = base.HasTimeScheduleTemplateGroupsFromCache(entities, ActorCompanyId);
                if (hasTemplateGroups)
                    AddTimeScheduleTemplateGroupsToCache(GetTimeScheduleTemplateGroups(employeeIds));

                EmployeeSchedulePlacementValidationResult validationResult = ValidateSchedulePlacement(iDTO.Items, iDTO.Control, hasTemplateGroups);
                if (!validationResult.Result.Success || validationResult.RecalculateTimeHead == null)
                {
                    oDTO.Result = validationResult.Result;
                    return oDTO;
                }

                int batchCount = 0;

                try
                {
                    while (validationResult.ValidPlacements.Any())
                    {
                        batchCount++;

                        foreach (var validPlacementsByEmployee in validationResult.ValidPlacements.GroupBy(p => p.EmployeeId))
                        {
                            validationResult.Batch = validPlacementsByEmployee.ToList();
                            validationResult.ValidPlacements = validationResult.ValidPlacements.Where(item => !validationResult.Batch.Contains(item)).ToList();

                            ActionResult employeeResult;

                            using (TransactionScope transaction = CreateTransactionScope(TimeSpan.FromSeconds(120), IsolationLevel.ReadCommitted))
                            {
                                InitTransaction(transaction);

                                List<TimeScheduleTemplateBlock> asyncTemplateBlocks = new List<TimeScheduleTemplateBlock>();
                                employeeResult = SaveEmployeeSchedulePlacement(validationResult, ref asyncTemplateBlocks, iDTO.UseBulk, setHeadStatus: false);
                                if (employeeResult.Success && !asyncTemplateBlocks.IsNullOrEmpty())
                                    iDTO.AsyncTemplateBlocks.AddRange(asyncTemplateBlocks);

                                if (employeeResult.Success)
                                    employeeResult = HandleEmployeeRequestReActivation(validPlacementsByEmployee.Key, validationResult.Batch, iDTO.Control);

                                if (employeeResult.Success)
                                    transaction.Complete();
                            }

                            if (!employeeResult.Success)
                            {
                                using (CompEntities entitiesTemp = new CompEntities())
                                {
                                    try
                                    {
                                        List<RecalculateTimeRecord> headRecords = entitiesTemp.RecalculateTimeRecord.Where(r => r.RecalculateTimeHeadId == validationResult.RecalculateTimeHead.RecalculateTimeHeadId).ToList();
                                        if (headRecords.Any())
                                        {
                                            List<int> batchEmployeeIds = validationResult.Batch.Select(placement => placement.EmployeeId).Distinct().ToList();
                                            List<RecalculateTimeRecord> batchRecords = headRecords.Where(record => batchEmployeeIds.Contains(record.EmployeeId)).ToList();
                                            if (batchRecords.Any())
                                            {
                                                List<RecalculateTimeRecord> batchRecordErrors = new List<RecalculateTimeRecord>();
                                                batchRecordErrors.AddRange(batchRecords.Filter(TermGroup_RecalculateTimeRecordStatus.Waiting));
                                                batchRecordErrors.AddRange(batchRecords.Filter(TermGroup_RecalculateTimeRecordStatus.Unprocessed));
                                                if (batchRecordErrors.Any())
                                                {
                                                    batchRecordErrors.SetStatus(TermGroup_RecalculateTimeRecordStatus.Error);
                                                    if (!SaveChanges(entitiesTemp).Success)
                                                        LogTraceError($"revert save failed");
                                                }
                                                else
                                                    LogTraceError("no records reverted");
                                            }
                                            else
                                                LogTraceError("no records to revert found");
                                        }
                                        else
                                            LogTraceError("records for head not found");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogTraceError("exception in revert");
                                        LogError(ex);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                    LogTraceError($"TaskSaveEmployeeSchedulePlacement exception");
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    using (CompEntities entitiesTemp = new CompEntities())
                    {
                        RecalculateTimeHead batchHead = validationResult.RecalculateTimeHead != null ? entitiesTemp.RecalculateTimeHead.Include("RecalculateTimeRecord").FirstOrDefault(h => h.RecalculateTimeHeadId == validationResult.RecalculateTimeHead.RecalculateTimeHeadId) : null;
                        if (batchHead != null)
                        {
                            batchHead.Status = (int)(iDTO.AsyncTemplateBlocks.Any() ? TermGroup_RecalculateTimeHeadStatus.Unprocessed : TermGroup_RecalculateTimeHeadStatus.Processed);
                            SetModifiedProperties(batchHead);

                            List<RecalculateTimeRecord> batchWaitingRecords = batchHead.RecalculateTimeRecord.Filter(TermGroup_RecalculateTimeRecordStatus.Waiting);
                            if (batchWaitingRecords.Any())
                            {
                                batchWaitingRecords.SetStatus(TermGroup_RecalculateTimeRecordStatus.Error);
                                LogTraceError($"reverting records waiting->error {batchWaitingRecords.Select(r => r.RecalculateTimeRecordId).ToCommaSeparated()}");
                                if (batchHead.RecalculateTimeRecord.All(r => r.Status == (int)TermGroup_RecalculateTimeRecordStatus.Error))
                                {
                                    batchHead.Status = (int)TermGroup_RecalculateTimeHeadStatus.Error;
                                    LogTraceError($"reverting head unprocssed/processed->error");
                                }
                            }

                            var saveResult = SaveChanges(entitiesTemp);
                            if (!saveResult.Success)
                            {
                                if (!oDTO.Result.Success)
                                    oDTO.Result.ErrorMessage += saveResult.ErrorMessage;
                                else
                                    oDTO.Result = saveResult;
                            }
                        }
                        else
                        {
                            LogTraceError($"head {validationResult.RecalculateTimeHead.RecalculateTimeHeadId} not found in finally");
                        }
                        entities.Connection.Close();
                    }
                }

                void LogTraceError(string description)
                {
                    LogError($"SaveEmployeeSchedulePlacement-{description}" +
                        $"#Success:{oDTO.Result.Success}" +
                        $"#ActorCompanyId:{actorCompanyId}" +
                        $"#Guid:{iDTO.Control.Key}" +
                        $"#HasTemplateGroups:{hasTemplateGroups}" +
                        $"#BatchCount:{batchCount}" +
                        $"#Batch:{validationResult?.GetBatchInfo()}" +
                        $"#RecalculateTimeHeadId:{validationResult?.RecalculateTimeHead?.RecalculateTimeHeadId}");
                }
            }

            #region Create TimeBlocks and transactions (async)

            if (iDTO.AsyncTemplateBlocks.Any())
            {
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SaveTimeBlocksAndTransactionsFromTemplateAsync(iDTO)));
            }

            #endregion

            return oDTO;
        }

        private SaveEmployeeSchedulePlacementStaffingOutputDTO TaskSaveEmployeeSchedulePlacementStaffing()
        {
            var (iDTO, oDTO) = InitTask<SaveEmployeeSchedulePlacementStaffingInputDTO, SaveEmployeeSchedulePlacementStaffingOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Item == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "SaveEmployeeSchedulePlacementItem");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    oDTO.Result = ValidateInitiatedEmployeePlacement(iDTO.Item);
                    if (!oDTO.Result.Success)
                        return oDTO;

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        List<TimeScheduleTemplateBlock> asyncTemplateBlocks = new List<TimeScheduleTemplateBlock>();
                        oDTO.Result = SaveEmployeeSchedulePlacement(iDTO.Control, iDTO.Item, ref asyncTemplateBlocks);
                        if (oDTO.Result.Success && asyncTemplateBlocks != null)
                            iDTO.AsyncTemplateBlocks.AddRange(asyncTemplateBlocks);

                        if (oDTO.Result.Success && !iDTO.Item.CreateTimeBlocksAndTransactionsAsync)
                            oDTO.Result = SaveTimeBlocksAndTransactionsFromTemplate(iDTO.AsyncTemplateBlocks);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                    LogError($"TaskSaveEmployeeSchedulePlacementStaffing" +
                        $"#Success:{oDTO.Result.Success}" +
                        $"#ActorCompanyId:{actorCompanyId}" +
                        $"#EmployeeId:{iDTO.Item?.EmployeeId}" +
                        $"#StartDate:{iDTO.Item?.StartDate.ToShortDateString()}" +
                        $"#StopDate:{iDTO.Item?.StopDate.ToShortDateString()}");
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            #region Create TimeBlocks and transactions (async)

            if (oDTO.Result.Success && iDTO.Item.CreateTimeBlocksAndTransactionsAsync && iDTO.AsyncTemplateBlocks.Any())
            {
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SaveTimeBlocksAndTransactionsFromTemplateAsync(iDTO)));
            }

            #endregion

            return oDTO;
        }

        private SaveEmployeeSchedulePlacementFromJobOutputDTO TaskSaveEmployeeSchedulePlacementFromJob()
        {
            var (iDTO, oDTO) = InitTask<SaveEmployeeSchedulePlacementFromJobInputDTO, SaveEmployeeSchedulePlacementFromJobOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    RecalculateTimeHead head = GetRecalculateTimeHead(iDTO.RecalculateTimeHeadId);
                    if (head == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "RecalculateTimeHead");
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    #region Process RecalculateTimeRecord

                    int nrOfProcessedRecords = 0;
                    List<RecalculateTimeRecord> records = head.RecalculateTimeRecord.Where(i => i.Status == (int)TermGroup_RecalculateTimeRecordStatus.Unprocessed).ToList();
                    foreach (RecalculateTimeRecord record in records)
                    {
                        //One transaction per Employee
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            #region Prereq

                            head.Status = (int)TermGroup_RecalculateTimeHeadStatus.UnderProcessing;
                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            //Make sure TimeScheduleTemplateBlock is loaded
                            if (!record.TimeScheduleTemplateBlock.IsLoaded)
                                record.TimeScheduleTemplateBlock.Load();

                            #endregion

                            #region Process

                            bool processed = true;

                            List<TimeScheduleTemplateBlock> templateBlocks = record.TimeScheduleTemplateBlock.Where(tb => tb.State == (int)SoeEntityState.Active).ToList();
                            if (templateBlocks.Any())
                            {
                                oDTO.Result = SaveTimeBlocksAndTransactionsFromTemplate(templateBlocks);
                                if (!oDTO.Result.Success)
                                {
                                    record.Status = (int)TermGroup_RecalculateTimeRecordStatus.Error;
                                    record.ErrorMsg = oDTO.Result.ErrorMessage;
                                    processed = false;
                                }
                            }

                            if (processed)
                            {
                                record.Status = (int)TermGroup_RecalculateTimeRecordStatus.Processed;
                                record.WarningMsg = null;
                                nrOfProcessedRecords++;
                            }

                            #endregion

                            oDTO.Result = Save();

                            TryCommit(oDTO);
                        }
                    }

                    #endregion

                    #region Update RecalculateTimeHead

                    int headStatus = 0;
                    if (nrOfProcessedRecords == records.Count)
                        headStatus = (int)TermGroup_RecalculateTimeHeadStatus.Processed;
                    else
                        headStatus = (int)TermGroup_RecalculateTimeHeadStatus.UnderProcessing;

                    if (head.Status != headStatus)
                    {
                        head.Status = headStatus;
                        SetModifiedProperties(head);

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;
                    }

                    #endregion

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

            return oDTO;
        }

        private DeleteEmployeeSchedulePlacementOutputDTO TaskDeleteEmployeeSchedulePlacement()
        {
            var (iDTO, oDTO) = InitTask<DeleteEmployeeSchedulePlacementInputDTO, DeleteEmployeeSchedulePlacementOutputDTO>();
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

                        Employee employee = GetEmployeeWithContactPersonFromCache(iDTO.EmployeeId);
                        if (employee == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));
                            return oDTO;
                        }

                        EmployeeSchedule employeeSchedule = GetEmployeeScheduleWithEmployee(iDTO.EmployeeScheduleId, iDTO.EmployeeId);
                        if (employeeSchedule == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11756, "Kan ej hitta aktiverat schema"));
                            return oDTO;
                        }

                        List<TimeBlock> timeBlocks = null;
                        List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksForEmployee(null, employeeSchedule.EmployeeId, employeeSchedule.StartDate, employeeSchedule.StopDate, employeeSchedule.EmployeeScheduleId, loadStaffingIfUsed: true, includeStandBy: true, includeOnDuty: true);

                        // Validate dates (only for regular Employees, i.e. not hidden)
                        if (!IsHiddenEmployeeFromCache(employeeSchedule.EmployeeId))
                        {
                            timeBlocks = GetTimeBlocksWithTransactions(iDTO.EmployeeId, employeeSchedule.StartDate, employeeSchedule.StopDate);

                            // Cannot delete schedule with manually adjusted TimeBlocks and transactions
                            oDTO.Result = IsOkToDeleteEmployeeSchedule(employee.EmployeeNrAndName, timeBlocks, templateBlocks, iDTO.Control);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        #endregion

                        #region Perform

                        oDTO.Result = SetScheduleToDeleted(templateBlocks, saveChanges: false);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        AdjustRecalculateTimeRecordAfterShortenedPlacament(templateBlocks);

                        oDTO.Result = SetScheduleToDeleted(employeeSchedule, saveChanges: false, discardCheckes: iDTO.Control.DiscardCheckesAll);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (!IsHiddenEmployeeFromCache(employeeSchedule.EmployeeId))
                        {
                            List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employeeSchedule.EmployeeId, timeBlocks.Select(tb => tb.TimeBlockDateId).Distinct());

                            oDTO.Result = SetTimeBlockDateDetailsToDeleted(timeBlockDates, SoeTimeBlockDateDetailType.Absence, null, saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SetTimeBlocksAndTransactionsToDeleted(timeBlocks, saveChanges: false, discardCheckes: iDTO.Control.DiscardCheckesAll);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = SetTimePayrollScheduleTransactionsToDeleted(timeBlockDates.Select(x => x.TimeBlockDateId).ToList(), iDTO.EmployeeId, saveChanges: false, excludeEmploymentTaxAndSupplementCharge: false);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        #endregion

                        oDTO.Result = Save();
                        if (oDTO.Result.Success)
                            oDTO.Result = TryCreateEmployeeRequestsToReActivate(iDTO.Control, iDTO.EmployeeId, employeeSchedule.StartDate, employeeSchedule.StopDate);

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

        private ControlEmployeeSchedulePlacementOutputDTO TaskControlEmployeeSchedulePlacement()
        {
            var (iDTO, oDTO) = InitTask<ControlEmployeeSchedulePlacementInputDTO, ControlEmployeeSchedulePlacementOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq
                    string hiddenString = "";
                    List<ActivateScheduleGridDTO> shortenedActivations = iDTO.IsDelete ? iDTO.Items : iDTO.Items.Where(e => e.EmployeeScheduleStartDate.HasValue && e.EmployeeScheduleStopDate.HasValue && e.EmployeeScheduleStopDate.Value > iDTO.StopDate && iDTO.StartDate == null).ToList();
                    if (shortenedActivations.IsNullOrEmpty())
                        return oDTO;

                    if (shortenedActivations.Any(w => w.EmployeeHidden))
                    {
                        bool useAccountHierarchy = UseAccountHierarchy();

                        if (useAccountHierarchy)
                        {
                            int defaultDimId = GetCompanyIntSettingFromCache(CompanySettingType.DefaultEmployeeAccountDimEmployee);
                            AccountDim accountDim = GetAccountDim(defaultDimId);
                            hiddenString = accountDim.Name;
                        }
                        else
                        {
                            hiddenString = GetText(514, "Kategori");
                        }

                    }

                    List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromCache();
                    List<TermGroup_EmployeeRequestType> requestTypes = new List<TermGroup_EmployeeRequestType> { TermGroup_EmployeeRequestType.AbsenceRequest };
                    List<TermGroup_EmployeeRequestStatus> requestStatuses = new List<TermGroup_EmployeeRequestStatus> { TermGroup_EmployeeRequestStatus.RequestPending, TermGroup_EmployeeRequestStatus.Definate, TermGroup_EmployeeRequestStatus.PartlyDefinate };

                    #endregion

                    #region Perform

                    foreach (var shortenedActivationsByEmployee in shortenedActivations.GroupBy(e => e.EmployeeId))
                    {
                        EmployeeDTO employee = GetEmployeeWithContactPersonFromCache(shortenedActivationsByEmployee.Key)?.ToDTO();
                        if (employee == null)
                            continue;

                        List<ActivateScheduleControlRowDTO> rowsForEmployee = new List<ActivateScheduleControlRowDTO>();

                        DateTime employeeMinDate = shortenedActivationsByEmployee.Min(i => i.EmployeeScheduleStartDate.Value);
                        DateTime employeeStartDate = iDTO.IsDelete ? employeeMinDate : CalendarUtility.GetLatestDate(employeeMinDate, iDTO.StopDate?.AddDays(1));
                        DateTime employeeStopDate = shortenedActivationsByEmployee.Max(i => i.EmployeeScheduleStopDate.Value);
                        List<TimeScheduleTemplateBlock> employeeScheduleBlocks = GetScheduleBlocksForEmployee(null, employee.EmployeeId, employeeStartDate, employeeStopDate, includeStandBy: true, loadHistory: true, includeOnDuty: true);
                        List<TimeScheduleTemplateBlock> employeeScheduleBlocksAbsence = employeeScheduleBlocks.Where(b => !b.IsBreak && b.TimeDeviationCauseId.HasValue).ToList();
                        List<TimeScheduleTemplateBlock> employeeScheduleBlocksChanged = employeeScheduleBlocks.Where(b => !b.IsBreak && !b.TimeDeviationCauseId.HasValue && b.TimeScheduleTemplateBlockHistory.Any()).ToList();
                        List<TimeBlock> employeeTimeBlocks = GetTimeBlocksWithTimeBlockDate(employee.EmployeeId, employeeStartDate, employeeStopDate);
                        List<TimeBlock> employeeTimeBlocksManuallyAdjusted = employeeTimeBlocks.GetWork(false).Where(b => b.ManuallyAdjusted).ToList();
                        List<EmployeeRequest> employeeRequests = GetEmployeeRequests(employee.EmployeeId, employeeStartDate, employeeStopDate, requestTypes, requestStatuses).Where(r => r.TimeDeviationCauseId.HasValue).ToList();
                        Dictionary<EmployeeRequest, DateRangeDTO> employeeRequestRanges = new Dictionary<EmployeeRequest, DateRangeDTO>();

                        CreateControlRows();
                        CreateControlHeads();

                        void CreateControlRows()
                        {
                            foreach (ActivateScheduleGridDTO activation in shortenedActivationsByEmployee)
                                CreateControlRowsForActivation(activation);
                        }
                        void CreateControlRowsForActivation(ActivateScheduleGridDTO activation)
                        {
                            DateTime activationStartDate = iDTO.IsDelete ? activation.EmployeeScheduleStartDate.Value : CalendarUtility.GetLatestDate(activation.EmployeeScheduleStartDate.Value, iDTO.StopDate?.AddDays(1));
                            DateTime activationStopDate = activation.EmployeeScheduleStopDate.Value;

                            CreateRowsForTypeEmployeRequests();
                            CreateRowsForTypeAbsenceRows();
                            CreateRowsForTypeChangedSchedule();
                            CreateRowsForTypeChangedTimeBlocks();

                            if (employee.Hidden)
                                CreateRowsForTypeHidden();

                            void CreateRowsForTypeEmployeRequests()
                            {
                                foreach (EmployeeRequest employeeRequest in employeeRequests)
                                {
                                    if (CalendarUtility.GetOverlappingDates(employeeRequest.Start, employeeRequest.Stop, activationStartDate, activationStopDate, out DateTime start, out DateTime stop))
                                    {
                                        CreateRowsForScheduleBlocks(TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest, employeeScheduleBlocksAbsence.Filter(employeeRequest.Start, employeeRequest.Stop, employeeRequest.TimeDeviationCauseId), employeeRequest);
                                        employeeRequestRanges.Add(employeeRequest, new DateRangeDTO(start, stop));
                                    }
                                }
                            }
                            void CreateRowsForTypeAbsenceRows()
                            {
                                CreateRowsForScheduleBlocks(TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays, employeeScheduleBlocksAbsence.Filter(activationStartDate, activationStopDate));
                            }
                            void CreateRowsForTypeChangedSchedule()
                            {
                                CreateRowsForScheduleBlocks(TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule, employeeScheduleBlocksChanged.Filter(activationStartDate, activationStopDate));
                            }
                            void CreateRowsForTypeChangedTimeBlocks()
                            {
                                CreateRowsForTypeTimeBlocks(TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedTimeBlocks, employeeTimeBlocksManuallyAdjusted.Filter(activationStartDate, activationStopDate));
                            }
                            void CreateRowsForTypeHidden()
                            {
                                rowsForEmployee.Add(new ActivateScheduleControlRowDTO(TermGroup_ControlEmployeeSchedulePlacementType.ShortenIsHidden, activationStopDate, activationStartDate, activationStopDate, activationStartDate, activationStopDate, false, null, null, null, null));
                            }
                            void CreateRowsForScheduleBlocks(TermGroup_ControlEmployeeSchedulePlacementType type, List<TimeScheduleTemplateBlock> scheduleBlocksForType, EmployeeRequest employeeRequest = null)
                            {
                                foreach (var scheduleBlocksByDeviationCause in scheduleBlocksForType.GroupBy(b => b.TimeDeviationCauseId))
                                {
                                    TimeDeviationCause timeDeviationCause = timeDeviationCauses?.FirstOrDefault(t => t.TimeDeviationCauseId == scheduleBlocksByDeviationCause.Key);
                                    foreach (var scheduleBlocksByDeviationCauseAndDate in scheduleBlocksByDeviationCause.GroupBy(b => b.Date.Value))
                                    {
                                        DateTime date = scheduleBlocksByDeviationCauseAndDate.Key;
                                        if (!DoCreateRow(date, type))
                                            continue;

                                        if (employeeRequest == null && type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays)
                                            employeeRequest = employeeRequests.FirstOrDefault(r => r.TimeDeviationCauseId.HasValue && r.TimeDeviationCauseId.Value == scheduleBlocksByDeviationCause.Key && CalendarUtility.IsDateInRange(date, r.Start, r.Stop));

                                        var (scheduleStart, scheduleStop, isWholedayAbsence) = employeeScheduleBlocks.GetScheduleInfo(date, timeDeviationCause?.TimeDeviationCauseId);
                                        foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocksByDeviationCauseAndDate)
                                            CreateRow(type, date, scheduleStart, scheduleStop, scheduleBlock.StartTime, scheduleBlock.StopTime, isWholedayAbsence, scheduleBlock.TimeScheduleTemplateBlockId, scheduleBlock.Type, employeeRequest?.EmployeeRequestId, timeDeviationCause?.TimeDeviationCauseId);
                                    }
                                }
                            }
                            void CreateRowsForTypeTimeBlocks(TermGroup_ControlEmployeeSchedulePlacementType type, List<TimeBlock> timeBlocksForType)
                            {
                                foreach (var timeBlocksByDeviationCause in timeBlocksForType.GroupBy(b => b.TimeDeviationCauseStartId))
                                {
                                    TimeDeviationCause timeDeviationCause = timeDeviationCauses?.FirstOrDefault(t => t.TimeDeviationCauseId == timeBlocksByDeviationCause.Key);
                                    foreach (var timeBlocksByDate in timeBlocksByDeviationCause.GroupBy(b => b.TimeBlockDate.Date))
                                    {
                                        DateTime date = timeBlocksByDate.Key;
                                        if (!DoCreateRow(date, type))
                                            continue;

                                        var (scheduleStart, scheduleStop, isWholedayAbsence) = employeeScheduleBlocks.GetScheduleInfo(date, timeDeviationCause?.TimeDeviationCauseId);
                                        foreach (TimeBlock timeBlock in timeBlocksByDate)
                                            CreateRow(type, timeBlocksByDate.Key, scheduleStart, scheduleStop, timeBlock.StartTime, timeBlock.StopTime, isWholedayAbsence, null, null, null, timeDeviationCause?.TimeDeviationCauseId);
                                    }
                                }
                            }
                            void CreateRow(TermGroup_ControlEmployeeSchedulePlacementType type, DateTime date, DateTime scheduleStart, DateTime scheduleStop, DateTime absenceStart, DateTime absenceStop, bool isWholedayAbsence, int? timeScheduleTemplateBlockId, int? timeScheduleTemplateBlockType, int? employeeRequestId, int? timeDeviationCauseId)
                            {
                                rowsForEmployee.Add(new ActivateScheduleControlRowDTO(type, date, scheduleStart, scheduleStop, absenceStart, absenceStop, isWholedayAbsence, timeScheduleTemplateBlockId, timeScheduleTemplateBlockType, employeeRequestId, timeDeviationCauseId));
                            }
                            bool DoCreateRow(DateTime date, TermGroup_ControlEmployeeSchedulePlacementType type)
                            {
                                if (type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays && ContainsRow(date, type, TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest))
                                    return false;
                                if (type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule && ContainsRow(date, type, TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest, TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays))
                                    return false;
                                if (type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedTimeBlocks && ContainsRow(date, type, TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest, TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays, TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule))
                                    return false;
                                return true;
                            }
                            bool ContainsRow(DateTime date, params TermGroup_ControlEmployeeSchedulePlacementType[] types)
                            {
                                return rowsForEmployee.ContainsRow(date, types);
                            }
                        }
                        void CreateControlHeads()
                        {
                            CreateHeadsForTypeAbsenceRequest();
                            CreateHeadsForTypeAbsenceDays();
                            CreateHeadsForTypeChangedSchedule();
                            CreateHeadsForTypeChangedTimeBlocks();
                            CreateHeadsForTypeHidden();

                            void CreateHeadsForTypeAbsenceRequest()
                            {
                                TermGroup_ControlEmployeeSchedulePlacementType type = TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest;
                                List<ActivateScheduleControlRowDTO> rowsForType = rowsForEmployee.Filter(type);
                                foreach (var employeeRequestRange in employeeRequestRanges)
                                {
                                    List<ActivateScheduleControlRowDTO> rowsForEmployeeRequest = rowsForType.Where(r => r.EmployeeRequestId == employeeRequestRange.Key.EmployeeRequestId && CalendarUtility.IsDateInRange(r.Date, employeeRequestRange.Value.Start, employeeRequestRange.Value.Stop)).ToList();
                                    CreateHead(type, employeeRequestRange.Value.Start, employeeRequestRange.Value.Stop, rowsForEmployeeRequest, GetTimeDeviationCause(employeeRequestRange.Key.TimeDeviationCauseId), employeeRequestRange.Key);
                                }
                            }
                            void CreateHeadsForTypeAbsenceDays()
                            {
                                TermGroup_ControlEmployeeSchedulePlacementType type = TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays;
                                List<ActivateScheduleControlRowDTO> rowsForType = rowsForEmployee.Filter(type);
                                foreach (var rowsByDeviationCause in rowsForType.GroupBy(b => b.TimeDeviationCauseId.Value))
                                {
                                    CreateHeads(rowsByDeviationCause.ToList(), rowsByDeviationCause.Key);
                                }
                            }
                            void CreateHeadsForTypeChangedSchedule()
                            {
                                CreateHeads(rowsForEmployee.Filter(TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule));
                            }
                            void CreateHeadsForTypeChangedTimeBlocks()
                            {
                                CreateHeads(rowsForEmployee.Filter(TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedTimeBlocks));
                            }
                            void CreateHeadsForTypeHidden()
                            {
                                CreateHeads(rowsForEmployee.Filter(TermGroup_ControlEmployeeSchedulePlacementType.ShortenIsHidden));
                            }
                            void CreateHeads(List<ActivateScheduleControlRowDTO> rows, int? timeDeviationCauseId = null)
                            {
                                if (!rows.IsNullOrEmpty())
                                {
                                    TermGroup_ControlEmployeeSchedulePlacementType type = rows.First().Type;
                                    TimeDeviationCause timeDeviationCause = GetTimeDeviationCause(timeDeviationCauseId);
                                    foreach (var range in rows.Select(t => t.Date).GetCoherentDateRanges())
                                    {
                                        List<ActivateScheduleControlRowDTO> rowsByRange = rows.Filter(range.Item1, range.Item2);
                                        if (!rows.IsNullOrEmpty())
                                            CreateHead(type, range.Item1, range.Item2, rowsByRange, timeDeviationCause);
                                    }
                                }
                            }
                            void CreateHead(TermGroup_ControlEmployeeSchedulePlacementType type, DateTime startDate, DateTime stopDate, List<ActivateScheduleControlRowDTO> rows = null, TimeDeviationCause timeDeviationCause = null, EmployeeRequest employeeRequest = null)
                            {
                                string typeName = GetText((int)type, (int)TermGroup.ControlEmployeeSchedulePlacementType);
                                string statusName = null, resultStatusName = null, comment = null;
                                switch (type)
                                {
                                    case TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest:
                                        if (employeeRequest != null)
                                        {
                                            statusName = GetText(employeeRequest.Status, TermGroup.EmployeeRequestStatus);
                                            resultStatusName = GetText(employeeRequest.ResultStatus, TermGroup.EmployeeRequestResultStatus);
                                        }
                                        comment = GetText(91982, "Frånvaro inom ansökan kommer tas bort");
                                        break;
                                    case TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceDays:
                                        comment = GetText(91981, "Frånvaro kommer tas bort");
                                        break;
                                    case TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule:
                                        comment = GetText(91983, "Förändrat schema kommer tas bort");
                                        break;
                                    case TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedTimeBlocks:
                                        comment = GetText(91984, "Förändrat utfall kommer tas bort");
                                        break;
                                    case TermGroup_ControlEmployeeSchedulePlacementType.ShortenIsHidden:
                                        comment = GetText(91984, "Observera, schema för ledigt pass kommer att tas bort oavsett") + " " + hiddenString;
                                        break;

                                }
                                oDTO.Control.AddHead(new ActivateScheduleControlHeadDTO(type, rows, employee, startDate, stopDate, typeName, statusName, resultStatusName, comment, employeeRequest?.EmployeeRequestId, timeDeviationCause?.TimeDeviationCauseId, timeDeviationCause?.Name, employeeRequest?.ReActivate ?? false));
                            }
                            TimeDeviationCause GetTimeDeviationCause(int? timeDeviationCauseId)
                            {
                                return timeDeviationCauseId.HasValue ? timeDeviationCauses?.FirstOrDefault(t => t.TimeDeviationCauseId == timeDeviationCauseId.Value) : null;
                            }
                        }
                    }

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
                        oDTO.Control.Sort();
                }
            }

            return oDTO;
        }

        #endregion

        #region EmployeeRequest

        private GetEmployeeRequestsOutputDTO TaskGetEmployeeRequests()
        {
            var (iDTO, oDTO) = InitTask<GetEmployeeRequestsInputDTO, GetEmployeeRequestsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                if (iDTO.EmployeeId > 0)
                    oDTO.EmployeeRequests = GetEmployeeRequests(iDTO.EmployeeId, iDTO.DateFrom, iDTO.DateTo, iDTO.RequestTypes, ignoreState: iDTO.IgnoreState);
                else
                    oDTO.EmployeeRequests = GetEmployeeRequests(null, iDTO.DateFrom, iDTO.DateTo, iDTO.RequestTypes, ignoreState: iDTO.IgnoreState);

                List<GenericType> statusTerms = base.GetTermGroupContent(TermGroup.EmployeeRequestStatus);
                List<GenericType> resultStatusTerms = base.GetTermGroupContent(TermGroup.EmployeeRequestResultStatus);
                foreach (var request in oDTO.EmployeeRequests)
                {
                    GenericType statusTerm = statusTerms.FirstOrDefault(t => t.Id == request.Status);
                    if (statusTerm != null)
                        request.StatusName = statusTerm.Name;

                    GenericType resultStatusTerm = resultStatusTerms.FirstOrDefault(t => t.Id == request.ResultStatus);
                    if (resultStatusTerm != null)
                        request.ResultStatusName = resultStatusTerm.Name;
                }
            }

            return oDTO;
        }

        private LoadEmployeeRequestOutputDTO TaskLoadEmployeeRequest()
        {
            var (iDTO, oDTO) = InitTask<LoadEmployeeRequestInputDTO, LoadEmployeeRequestOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                if (iDTO.EmployeeRequestId.HasValue)
                    oDTO.EmployeeRequest = GetEmployeeRequest(iDTO.EmployeeRequestId.Value);
                else if (iDTO.Start.HasValue && iDTO.Stop.HasValue && iDTO.EmployeeId != 0)
                    oDTO.EmployeeRequest = GetEmployeeRequest(iDTO.EmployeeId, iDTO.Start.Value, iDTO.Stop.Value, iDTO.RequestType);

                if (oDTO.EmployeeRequest != null)
                {
                    bool requestIntersectsWithCurrent = DateIntervalIntersectsWithExistingEmployeeRequest(oDTO.EmployeeRequest.Start, oDTO.EmployeeRequest.Stop, oDTO.EmployeeRequest.EmployeeId, oDTO.EmployeeRequest.EmployeeRequestId, (TermGroup_EmployeeRequestType)oDTO.EmployeeRequest.Type, out string intersectMessage);

                    oDTO.EmployeeRequest.RequestIntersectsWithCurrent = requestIntersectsWithCurrent;
                    oDTO.EmployeeRequest.IntersectMessage = intersectMessage;

                    List<GenericType> statusTerms = base.GetTermGroupContent(TermGroup.EmployeeRequestStatus);

                    GenericType term = statusTerms.FirstOrDefault(t => t.Id == oDTO.EmployeeRequest.Status);
                    if (term != null)
                        oDTO.EmployeeRequest.StatusName = term.Name;
                }
            }

            return oDTO;
        }

        private SaveEmployeeRequestOutputDTO TaskSaveEmployeeRequest()
        {
            var (iDTO, oDTO) = InitTask<SaveEmployeeRequestInputDTO, SaveEmployeeRequestOutputDTO>();
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

                        oDTO.Result = SaveEmployeeRequest(iDTO.EmployeeRequest, iDTO.EmployeeId, iDTO.RequestType);

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
                        iDTO.EmployeeRequest.EmployeeRequestId = oDTO.Result.IntegerValue;
                        Employee employee = GetEmployeeFromCache(iDTO.EmployeeId);
                        if (employee != null && employee.UserId == userId && iDTO.EmployeeRequest.TimeDeviationCauseId.HasValue)
                        {
                            bool sentwithErrors = false;
                            SendXEMailOnEmployeeRequest(employee.EmployeeId, iDTO.EmployeeRequest.TimeDeviationCauseId.Value, iDTO.EmployeeRequest, ref sentwithErrors);
                        }

                        if (iDTO.EmployeeRequest.TimeDeviationCauseId.HasValue && !iDTO.SkipXEMailOnChanges && iDTO.IsForcedDefinitive)
                            SendXEMailOnAbsenceRequestPlanning(false, true, 0, iDTO.EmployeeRequest, new List<TimeSchedulePlanningDayDTO>());
                    }

                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private SaveOrDeleteEmployeeRequestOutputDTO TaskSaveOrDeleteEmployeeRequest()
        {
            var (iDTO, oDTO) = InitTask<SaveOrDeleteEmployeeRequestInputDTO, SaveOrDeleteEmployeeRequestOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
                        Dictionary<int, EntityObject> tcDict = new Dictionary<int, EntityObject>();
                        int tempIdCounter = 0;

                        Employee employee = EmployeeManager.GetEmployee(entities, iDTO.EmployeeId, actorCompanyId, loadUser: true);
                        int lockDaysBefore = GetCompanyIntSettingFromCache(CompanySettingType.TimeAvailabilityLockDaysBefore);

                        #endregion

                        #region Delete

                        foreach (EmployeeRequestDTO item in iDTO.DeletedEmployeeRequests)
                        {
                            EmployeeRequest employeeRequest = entities.EmployeeRequest.FirstOrDefault(e => e.EmployeeRequestId == item.EmployeeRequestId);
                            if (employeeRequest != null)
                            {
                                // Can't delete passed requests
                                if (employeeRequest.Start < DateTime.Today)
                                {
                                    oDTO.Result = new ActionResult(false, 1, GetText(11774, "Kan ej ändra på tillgänglighet bakåt i tiden"));
                                    return oDTO;
                                }

                                // Can't delete locked requests
                                if (lockDaysBefore > 0 && employeeRequest.Start.Date.AddDays(-lockDaysBefore) <= DateTime.Today)
                                {
                                    oDTO.Result = new ActionResult(false, 2, String.Format(GetText(3673, "Kan ej ändra på tillgänglighet om det är mindre än {0} dagar kvar tills den infaller"), lockDaysBefore.ToString()));
                                    return oDTO;
                                }

                                string fromValue = String.Format("{0} ", employeeRequest.Type == (int)TermGroup_EmployeeRequestType.InterestRequest ? GetText(3913, 1, "Vill jobba") : GetText(3914, 1, "Kan inte jobba"));
                                if (employeeRequest.IsWholeDay)
                                    fromValue += employeeRequest.Start.ToShortDateString();
                                else
                                    fromValue += String.Format("{0} - {1}", employeeRequest.Start.ToShortDateShortTimeString(), employeeRequest.Start.Date == employeeRequest.Stop.Date ? employeeRequest.Stop.ToShortTimeString() : employeeRequest.Stop.ToShortDateShortTimeString());

                                if (!string.IsNullOrEmpty(item.Comment))
                                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, employeeRequest.EmployeeRequestId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.EmployeeRequest_Comment, employeeRequest.Comment, item.Comment));

                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, employeeRequest.EmployeeRequestId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.Unspecified, fromValue));
                                ChangeEntityState(employeeRequest, SoeEntityState.Deleted, employee.User);
                            }
                        }

                        #endregion

                        #region Add/update

                        foreach (EmployeeRequestDTO item in iDTO.NewOrEditedEmployeeRequests)
                        {
                            // Check times
                            if (item.Start >= item.Stop)
                            {
                                oDTO.Result = new ActionResult(false, 4, GetText(3675, "Kontrollera klockslagen. Till måste vara efter från."));
                                return oDTO;
                            }

                            EmployeeRequest employeeRequest = null;
                            if (item.EmployeeRequestId != 0)
                            {
                                employeeRequest = entities.EmployeeRequest.FirstOrDefault(e => e.EmployeeRequestId == item.EmployeeRequestId);
                                if (employeeRequest == null)
                                    continue;

                                // Can't modify passed requests
                                if (employeeRequest.Stop < DateTime.Today.AddDays(-1) || item.Stop < DateTime.Today.AddDays(-1))
                                {
                                    oDTO.Result = new ActionResult(false, 1, GetText(11774, "Kan ej ändra på tillgänglighet bakåt i tiden"));
                                    return oDTO;
                                }

                                // Can't modify locked requests
                                if (lockDaysBefore > 0 && (employeeRequest.Start.Date.AddDays(-lockDaysBefore) <= DateTime.Today || item.Start.Date.AddDays(-lockDaysBefore) <= DateTime.Today))
                                {
                                    oDTO.Result = new ActionResult(false, 2, String.Format(GetText(3673, "Kan ej ändra på tillgänglighet om det är mindre än {0} dagar kvar tills den infaller"), lockDaysBefore.ToString()));
                                    return oDTO;
                                }

                                if (employeeRequest.Type != (int)item.Type)
                                {
                                    string fromValueName = employeeRequest.Type == (int)TermGroup_EmployeeRequestType.InterestRequest ? GetText(3913, 1, "Vill jobba") : GetText(3914, 1, "Kan inte jobba");
                                    string toValueName = item.Type == TermGroup_EmployeeRequestType.InterestRequest ? GetText(3913, 1, "Vill jobba") : GetText(3914, 1, "Kan inte jobba");
                                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, employeeRequest.EmployeeRequestId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.EmployeeRequest_Type, employeeRequest.Type.ToString(), ((int)item.Type).ToString(), fromValueName, toValueName));
                                    employeeRequest.Type = (int)item.Type;
                                }
                                if (employeeRequest.Start != item.Start)
                                {
                                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, employeeRequest.EmployeeRequestId, SettingDataType.Time, null, TermGroup_TrackChangesColumnType.EmployeeRequest_Start, employeeRequest.Start.ToShortDateShortTimeString(), item.Start.ToShortDateShortTimeString()));
                                    employeeRequest.Start = item.Start;
                                }
                                if (employeeRequest.Stop != item.Stop)
                                {
                                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, employeeRequest.EmployeeRequestId, SettingDataType.Time, null, TermGroup_TrackChangesColumnType.EmployeeRequest_Stop, employeeRequest.Stop.ToShortDateShortTimeString(), item.Stop.ToShortDateShortTimeString()));
                                    employeeRequest.Stop = item.Stop;
                                }
                                if (employeeRequest.Comment != item.Comment)
                                {
                                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, employeeRequest.EmployeeRequestId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.EmployeeRequest_Comment, employeeRequest.Comment, item.Comment));
                                    employeeRequest.Comment = item.Comment;
                                }

                                SetModifiedProperties(employeeRequest, employee.User);
                            }
                            else
                            {
                                // Can't add passed requests
                                if (item.Start < DateTime.Today || item.Stop < DateTime.Today)
                                {
                                    oDTO.Result = new ActionResult(false, 1, GetText(11774, "Kan ej ändra på tillgänglighet bakåt i tiden"));
                                    return oDTO;
                                }

                                // Can't add locked requests
                                if (lockDaysBefore > 0 && item.Start.Date.AddDays(-lockDaysBefore) <= DateTime.Today)
                                {
                                    oDTO.Result = new ActionResult(false, 2, String.Format(GetText(3673, "Kan ej ändra på tillgänglighet om det är mindre än {0} dagar kvar tills den infaller"), lockDaysBefore.ToString()));
                                    return oDTO;
                                }

                                employeeRequest = new EmployeeRequest()
                                {
                                    ActorCompanyId = actorCompanyId,
                                    EmployeeId = iDTO.EmployeeId,
                                    Type = (int)item.Type,
                                    Start = item.Start,
                                    Stop = item.Stop,
                                    Comment = item.Comment
                                };
                                SetCreatedProperties(employeeRequest, employee.User);
                                entities.AddObject("EmployeeRequest", employeeRequest);

                                bool isWholeDay = item.Start.Date == item.Stop.Date &&
                                    item.Start == CalendarUtility.GetBeginningOfDay(item.Start) &&
                                    item.Stop == CalendarUtility.GetEndOfDay(item.Stop);

                                string toValue = String.Format("{0} ", item.Type == TermGroup_EmployeeRequestType.InterestRequest ? GetText(3913, 1, "Vill jobba") : GetText(3914, 1, "Kan inte jobba"));
                                if (isWholeDay)
                                    toValue += item.Start.ToShortDateString();
                                else
                                    toValue += String.Format("{0} - {1}", item.Start.ToShortDateShortTimeString(), item.Start.Date == item.Stop.Date ? item.Stop.ToShortTimeString() : item.Stop.ToShortDateShortTimeString());

                                tempIdCounter++;
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Insert, SoeEntityType.Employee, employeeRequest.EmployeeId, SoeEntityType.EmployeeRequest_Availability, tempIdCounter, SettingDataType.String, null, TermGroup_TrackChangesColumnType.Unspecified, null, toValue));
                                tcDict.Add(tempIdCounter, employeeRequest);
                            }
                        }

                        #endregion

                        #region Check overlapping and schedule

                        if (iDTO.NewOrEditedEmployeeRequests.Any())
                        {
                            // Get date range for modified requests
                            DateTime dateFrom = iDTO.NewOrEditedEmployeeRequests.Min(r => r.Start);
                            DateTime dateTo = iDTO.NewOrEditedEmployeeRequests.Max(r => r.Stop);

                            List<EmployeeRequest> requests = (from e in entities.EmployeeRequest
                                                              where e.EmployeeId == iDTO.EmployeeId &&
                                                              (e.Type == (int)TermGroup_EmployeeRequestType.InterestRequest || e.Type == (int)TermGroup_EmployeeRequestType.NonInterestRequest) &&
                                                              e.State == (int)SoeEntityState.Active &&
                                                              e.Start <= dateTo && e.Stop >= dateFrom
                                                              select e).ToList();

                            // Need to check for state again, if request was deleted in this call.
                            // It will be fetched in query above since it's not saved yet.
                            List<EmployeeRequestDTO> dtos = requests.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList();
                            foreach (EmployeeRequestDTO request in iDTO.NewOrEditedEmployeeRequests.Where(r => r.EmployeeRequestId != 0).ToList())
                            {
                                // Remove existing request from list (the modified one will be added below)
                                EmployeeRequestDTO existing = dtos.FirstOrDefault(d => d.EmployeeRequestId == request.EmployeeRequestId);
                                if (existing != null)
                                    dtos.Remove(existing);
                            }

                            dtos.AddRange(iDTO.NewOrEditedEmployeeRequests);

                            DateTime prevStop = DateTime.MinValue;
                            foreach (EmployeeRequestDTO request in dtos.OrderBy(r => r.Start).ThenBy(r => r.Stop).ToList())
                            {
                                if (request.Start < prevStop)
                                {
                                    oDTO.Result = new ActionResult(false, 1, GetText(11775, "Tillgänglighet får ej överlappa varandra"));
                                    return oDTO;
                                }
                                prevStop = request.Stop;
                            }

                            // Can't add 'Non interest' during existing schedule
                            List<EmployeeRequestDTO> nonInterests = dtos.Where(d => d.Type == TermGroup_EmployeeRequestType.NonInterestRequest).OrderBy(r => r.Start).ThenBy(r => r.Stop).ToList();
                            if (nonInterests.Any())
                            {
                                List<TimeScheduleTemplateBlock> schedules = (from tb in entities.TimeScheduleTemplateBlock
                                                                             where !tb.TimeScheduleScenarioHeadId.HasValue &&
                                                                             tb.EmployeeId == iDTO.EmployeeId &&
                                                                             tb.TimeScheduleEmployeePeriod != null &&
                                                                             !tb.IsPreliminary &&
                                                                             tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                                             (tb.Date.HasValue && tb.Date.Value <= dateTo.Date && DbFunctions.AddDays(tb.Date.Value, DbFunctions.DiffDays(tb.StartTime, tb.StopTime)) >= dateFrom.Date) &&
                                                                             ((tb.StartTime != tb.StopTime && !tb.TimeDeviationCauseId.HasValue) || tb.TimeDeviationCauseId.HasValue) &&
                                                                             tb.State == (int)SoeEntityState.Active
                                                                             orderby tb.Date, tb.StartTime, tb.StopTime
                                                                             select tb).ToList();
                                if (schedules.Any())
                                {
                                    // Create a list of schedule blocks (times)
                                    List<DateRangeDTO> scheduleRanges = new List<DateRangeDTO>();
                                    foreach (TimeScheduleTemplateBlock schedule in schedules)
                                    {
                                        DateTime startTime = CalendarUtility.MergeDateAndTime(schedule.Date.Value.AddDays((schedule.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), schedule.StartTime);
                                        DateTime stopTime = CalendarUtility.MergeDateAndTime(schedule.Date.Value.AddDays((schedule.StopTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), schedule.StopTime);
                                        scheduleRanges.Add(new DateRangeDTO(startTime, stopTime));
                                    }

                                    foreach (EmployeeRequestDTO request in nonInterests)
                                    {
                                        foreach (DateRangeDTO scheduleRange in scheduleRanges)
                                        {
                                            if (CalendarUtility.IsDatesOverlapping(scheduleRange.Start, scheduleRange.Stop, request.Start, request.Stop))
                                            {
                                                oDTO.Result = new ActionResult(false, 3, GetText(3674, "Du kan inte lägga 'Kan inte jobba' på en tidpunkt då du redan har ett aktivt schema"));
                                                return oDTO;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        oDTO.Result = Save();

                        #region TrackChanges

                        if (oDTO.Result.Success)
                        {
                            // Add track changes
                            foreach (TrackChangesDTO dto in trackChangesItems.Where(t => t.Action == TermGroup_TrackChangesAction.Insert))
                            {
                                // Replace temp ids with actual ids created on save
                                EmployeeRequest empReq = tcDict[dto.RecordId] is EmployeeRequest ? tcDict[dto.RecordId] as EmployeeRequest : null;
                                if (empReq != null)
                                    dto.RecordId = empReq.EmployeeRequestId;
                            }
                            if (trackChangesItems.Any())
                                oDTO.Result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);
                        }

                        #endregion

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }

                return oDTO;
            }
        }

        private DeleteEmployeeRequestOutputDTO TaskDeleteEmployeeRequest()
        {
            var (iDTO, oDTO) = InitTask<DeleteEmployeeRequestInputDTO, DeleteEmployeeRequestOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.Result = DeleteEmployeeRequest(iDTO.EmployeeRequestId, false);
                if (oDTO.Result.Success)
                    oDTO.Result = Save();
            }

            return oDTO;
        }

        private PerformAbsenceRequestPlanningActionOutputDTO TaskPerformAbsenceRequestPlanningAction()
        {
            var (iDTO, oDTO) = InitTask<PerformAbsenceRequestPlanningActionInputDTO, PerformAbsenceRequestPlanningActionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimeScheduleScenarioHeadId.HasValue)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, GetText(8832, "Ej tillåtet att godkänna en ansökan inom ett scenario."));
                return oDTO;
            }

            #region Prereq

            var shifts = iDTO.Shifts.Where(x => !x.IsLended).ToList();
            oDTO.Result = ValidateShiftsAgainstGivenScenario(shifts, iDTO.TimeScheduleScenarioHeadId);
            if (!oDTO.Result.Success)
                return oDTO;

            bool isDefinate = false;
            int shiftsLeftToProcess = 0;
            EmployeeRequest absenceRequest = null;

            #endregion

            #region Perform absence planning and save generated absence

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        int hiddenEmployeeId = 0;
                        Guid batchId = GetNewBatchLink();

                        #region Prereq

                        hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                        absenceRequest = GetEmployeeRequest(iDTO.EmployeeRequestId);

                        if ((hiddenEmployeeId == 0 && UseStaffing()) || absenceRequest == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeRequest");
                            return oDTO;
                        }

                        if (iDTO.Shifts.Any(x => x.EmployeeId == absenceRequest.EmployeeId))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, GetText(8832, "Ej tillåtet att tilldela ett pass till samma anställd som ska få frånvaro."));
                            return oDTO;
                        }


                        #endregion

                        #region Split shifts and save original shifts

                        if (absenceRequest.ExtendedAbsenceSettingId.HasValue) //just to be sure that we may need to split
                        {
                            var firstDate = shifts.Min(x => x.ActualDate).Date;
                            var lastDate = shifts.Max(x => x.ActualDate).Date;
                            List<int> affectedEmployeeids = new List<int>();
                            var updateSummeryAfterSave = shifts.Count > 1;

                            ShiftHistoryLogCallStackProperties logProperties = null;
                            foreach (TimeSchedulePlanningDayDTO shift in shifts.Where(x => x.ApprovalTypeId == (int)TermGroup_YesNo.Yes).ToList())
                            {
                                oDTO.Result = SplitTasks(shift.TimeScheduleTemplateBlockId, new List<DateTime>() { shift.AbsenceStartTime, shift.AbsenceStopTime });
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = GetTimeScheduleTemplateBlockTasks(shift.TimeScheduleTemplateBlockId).ToDTOs().ToList();

                                List<TimeSchedulePlanningDayDTO> newShifts = GetAbsenceDividedShifts(shift);
                                foreach (TimeSchedulePlanningDayDTO newShift in newShifts)
                                {
                                    logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanning, absenceRequest.EmployeeRequestId, true);

                                    newShift.EmployeeId = absenceRequest.EmployeeId;
                                    logProperties.AbsenceForEmployeeId = absenceRequest.EmployeeId;

                                    // Save
                                    oDTO.Result = SaveTimeScheduleShiftAndLogChanges(newShift, logProperties, true, updateScheduledTimeSummary: !updateSummeryAfterSave);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    affectedEmployeeids.Add(newShift.EmployeeId);

                                    #region Tasks (new shift is created from originalshift, connect tasks within the new shift)

                                    oDTO.Result = ConnectTasksWithinShift(shiftTasks, newShift);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    #endregion
                                }

                                logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanning, absenceRequest.EmployeeRequestId, true);

                                logProperties.AbsenceForEmployeeId = absenceRequest.EmployeeId;

                                int newEmployeeid = shift.EmployeeId;
                                shift.BelongsToPreviousDay = (shift.AbsenceStartTime.Date > shift.StartTime.Date);
                                shift.BelongsToNextDay = (shift.AbsenceStartTime.Date < shift.StartTime.Date);

                                // Save original with new start and stop
                                shift.StartTime = shift.AbsenceStartTime;  //new start is absence start
                                shift.StopTime = shift.AbsenceStopTime; //new stop is absence stop
                                shift.EmployeeId = absenceRequest.EmployeeId; //shift.EmployeeId has been set to destination employeeid in GUI, change it temporarily to originalemployee

                                oDTO.Result = SaveTimeScheduleShiftAndLogChanges(shift, logProperties, true, updateScheduledTimeSummary: !updateSummeryAfterSave);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                                affectedEmployeeids.Add(shift.EmployeeId);
                                affectedEmployeeids.Add(newEmployeeid);

                                shift.EmployeeId = newEmployeeid; //restore to new employee
                            }


                            if (updateSummeryAfterSave && base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, firstDate))
                            {
                                foreach (var employeeId in affectedEmployeeids.Distinct())
                                    TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employeeId, firstDate, lastDate);
                            }

                            if (base.HasCalculatePayrollOnChanges(entities, actorCompanyId))
                            {
                                foreach (var employeeId in affectedEmployeeids.Distinct())
                                    CalculatePayroll(employeeId, firstDate, lastDate);
                            }
                        }

                        #endregion

                        #region Save absence                        

                        #region Calculate absence

                        if (oDTO.Result.Success)
                        {
                            InitAbsenceDays(absenceRequest.EmployeeId, shifts.GetDates(), absenceRequest.GetRatio(), absenceRequest.TimeDeviationCauseId);
                            InitEvaluatePriceFormulaInputDTO(employeeIds: shifts.GetEmployeeIds(absenceRequest.EmployeeId));
                            InitEmployeeSettingsCache(absenceRequest.EmployeeId, shifts.GetStartDate(), shifts.GetStopDate());

                            foreach (var shiftsGroupedByDate in shifts.GroupBy(x => x.ActualDate.Date))
                            {
                                List<TimeSchedulePlanningDayDTO> approvedShifts = shiftsGroupedByDate.Where(i => i.ApprovalTypeId == ((int)TermGroup_YesNo.Yes)).ToList();
                                List<TimeSchedulePlanningDayDTO> absenceShifts = approvedShifts.Where(x => x.StartTime != x.StopTime).ToList();
                                List<TimeSchedulePlanningDayDTO> zeroShifts = approvedShifts.Where(x => x.StartTime == x.StopTime).ToList();

                                if (absenceShifts.Count > 0)
                                {
                                    oDTO.Result = SaveDeviationsFromShifts(absenceShifts, absenceRequest.EmployeeId, absenceRequest.TimeDeviationCauseId.Value, true, absenceRequest.EmployeeChildId, null, recalculateRelatedDays: false);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                                else if (zeroShifts.Count > 0)
                                {
                                    oDTO.Result = SaveWholedayDeviations(zeroShifts.ToTimeBlockDTOs(), absenceRequest.TimeDeviationCauseId.Value, absenceRequest.TimeDeviationCauseId.Value, "", TermGroup_TimeDeviationCauseType.Absence, absenceRequest.EmployeeId, absenceRequest.EmployeeChildId, scheduledAbsence: true, recalculateRelatedDays: false);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                            }

                            if (oDTO.Result.Success)
                                oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Apply, absenceRequest.EmployeeId);
                        }

                        #endregion

                        #endregion

                        #region Decide Action

                        DecideAbsencePlanningAction(shifts, hiddenEmployeeId);

                        #endregion

                        #region Perform

                        foreach (var shiftsGroupedByDate in shifts.GroupBy(g => g.StartTime.Date))
                        {
                            foreach (var shiftsGroupedByDateEmployee in shiftsGroupedByDate.GroupBy(g => g.EmployeeId).ToList())
                            {
                                Guid link = GetNewShiftLink();
                                foreach (var shift in shiftsGroupedByDateEmployee.ToList())
                                {
                                    switch ((AbsenceRequestShiftPlanningAction)shift.AbsenceRequestShiftPlanningAction)
                                    {
                                        case AbsenceRequestShiftPlanningAction.ReplaceWithOtherEmployee:
                                            oDTO.Result = AbsenceRequestActionCopyShiftToGivenEmployee(shift, absenceRequest, batchId, link, iDTO.SkipXEMailOnShiftChanges);
                                            break;
                                        case AbsenceRequestShiftPlanningAction.ReplaceWithHiddenEmployee:
                                            oDTO.Result = AbsenceRequestActionCopyShiftToHiddenEmployee(shift, absenceRequest, batchId, link, iDTO.SkipXEMailOnShiftChanges);
                                            break;
                                        case AbsenceRequestShiftPlanningAction.NoReplacement:
                                            oDTO.Result = AbsenceRequestActionNoReplacementOnShiftIsNeeded(shift, absenceRequest, batchId, iDTO.SkipXEMailOnShiftChanges);
                                            break;
                                        case AbsenceRequestShiftPlanningAction.NotApproved:
                                            oDTO.Result = AbsenceReqestActionShiftIsNotApproved(shift, absenceRequest, batchId, iDTO.SkipXEMailOnShiftChanges);
                                            break;
                                        default:
                                            break;
                                    }
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                            }
                        }

                        #endregion

                        #region RestoreToSchedule

                        oDTO.Result = RestoreCurrentDaysToSchedule();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region ExtraShift

                        oDTO.Result = SetHasUnhandledShiftChanges();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Notify

                        if (oDTO.Result.Success)
                        {
                            DoNotifyChangeOfDeviations();
                            DoInitiatePayrollWarnings();
                        }

                        SendXEMailOnDayChanged(absenceRequest.EmployeeId);

                        #endregion

                        if (oDTO.Result.Success)
                            oDTO.Result = Save();

                        #region Update AbsenceRequest Status

                        if (oDTO.Result.Success)
                            oDTO.Result = UpdateEmployeeRequestStatus(absenceRequest, ref isDefinate, ref shiftsLeftToProcess);

                        #endregion

                        #region Update AbsenceRequest ResultStatus

                        if (oDTO.Result.Success)
                            oDTO.Result = UpdateEmployeeRequestResultStatus(absenceRequest, shifts, isDefinate);

                        #endregion

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
                    if (oDTO.Result.Success)
                    {
                        #region Send XEmail

                        if (shifts.Any(s => s.AbsenceRequestShiftPlanningAction != (int)AbsenceRequestShiftPlanningAction.Undefined) && absenceRequest != null && !iDTO.SkipXEMailOnShiftChanges)
                            SendXEMailOnAbsenceRequestPlanning(isDefinate, false, shiftsLeftToProcess, absenceRequest, shifts);

                        #endregion
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            #endregion


            return oDTO;
        }

        #endregion

        #region Shift

        private GetAvailableTimeOutputDTO TaskGetAvailableTime()
        {
            var (iDTO, oDTO) = InitTask<GetAvailableTimeInputDTO, GetAvailableTimeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            List<TimeScheduleTemplateBlockDTO> blocks = TimeScheduleManager.GetAllActiveTimeScheduleTemplateBlocks(iDTO.EmployeeId, iDTO.StartTime.Date, CalendarUtility.GetEndOfDay(iDTO.StopTime)).ToDTOs();
            List<TimeScheduleTemplateBlockDTO> availableBlocks = GetAvailableBlocksForOrderAssignment(blocks, iDTO.StartTime, iDTO.StopTime);

            oDTO.ScheduledMinutes = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && !w.IsBreak).Sum(s => s.TotalMinutes) - blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && w.IsBreak).Sum(s => s.TotalMinutes);
            oDTO.PlannedMinutes = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Order).Sum(s => s.TotalMinutes);
            oDTO.BookedMinutes = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Booking).Sum(s => s.TotalMinutes);

            if (blocks.Any(w => w.ActualStopTime > iDTO.StopTime))
            {
                //Entire block after stopTime
                oDTO.ScheduledMinutes -= (blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && !w.IsBreak && w.ActualStartTime > iDTO.StopTime).Sum(s => s.TotalMinutes));
                oDTO.ScheduledMinutes += (blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && w.IsBreak && w.ActualStartTime > iDTO.StopTime).Sum(s => s.TotalMinutes));

                int outsideScheduleMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsideScheduleBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && !w.IsBreak && w.ActualStopTime > iDTO.StopTime && w.ActualStartTime < iDTO.StopTime).ToList();
                outsideScheduleBlocks.ToList().ForEach(f => outsideScheduleMinutes = +CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, iDTO.StopTime, DateTime.MaxValue));

                int outsideBreaksMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsideBreaksBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && w.IsBreak && w.ActualStopTime > iDTO.StopTime && w.ActualStartTime < iDTO.StopTime).ToList();
                outsideBreaksBlocks.ToList().ForEach(f => outsideBreaksMinutes += CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, iDTO.StopTime, DateTime.MaxValue));

                oDTO.ScheduledMinutes = oDTO.ScheduledMinutes - outsideScheduleMinutes + outsideBreaksMinutes;

                int outsideBookingMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsideBookingsBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Booking && w.ActualStopTime > iDTO.StopTime && w.ActualStartTime < iDTO.StopTime).ToList();
                outsideBookingsBlocks.ToList().ForEach(f => outsideBookingMinutes += CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, iDTO.StopTime, DateTime.MaxValue));

                oDTO.BookedMinutes -= outsideBookingMinutes;

                int outsidePlannedMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsidePlannedsBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Order && w.ActualStopTime > iDTO.StopTime && w.ActualStartTime < iDTO.StopTime).ToList();
                outsidePlannedsBlocks.ToList().ForEach(f => outsidePlannedMinutes += CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, iDTO.StopTime, DateTime.MaxValue));

                oDTO.PlannedMinutes -= outsidePlannedMinutes;
            }

            if (blocks.Any(w => w.ActualStopTime > iDTO.StartTime))
            {
                //Entire block before start
                oDTO.ScheduledMinutes -= (blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && !w.IsBreak && w.ActualStopTime <= iDTO.StartTime).Sum(s => s.TotalMinutes));
                oDTO.ScheduledMinutes += (blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && w.IsBreak && w.ActualStopTime <= iDTO.StartTime).Sum(s => s.TotalMinutes));

                int outsideScheduleMinutes = 0;
                var outsideScheduleBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && !w.IsBreak && w.ActualStopTime > iDTO.StartTime && w.ActualStartTime < iDTO.StartTime).ToList();
                outsideScheduleBlocks.ForEach(f => outsideScheduleMinutes = +CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, DateTime.MinValue, iDTO.StartTime));

                int outsideBreaksMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsideBreaksBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule && w.IsBreak && w.ActualStopTime > iDTO.StartTime && w.ActualStartTime < iDTO.StartTime).ToList();
                outsideBreaksBlocks.ForEach(f => outsideBreaksMinutes += CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, DateTime.MinValue, iDTO.StartTime));

                oDTO.ScheduledMinutes = oDTO.ScheduledMinutes - outsideScheduleMinutes + outsideBreaksMinutes;

                int outsideBookingMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsideBookingsBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Booking && w.ActualStopTime > iDTO.StartTime && w.ActualStartTime < iDTO.StartTime).ToList();
                outsideBookingsBlocks.ForEach(f => outsideBookingMinutes += CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, DateTime.MinValue, iDTO.StartTime));

                oDTO.BookedMinutes -= outsideBookingMinutes;

                int outsidePlannedMinutes = 0;
                List<TimeScheduleTemplateBlockDTO> outsidePlannedsBlocks = blocks.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Order && w.ActualStopTime > iDTO.StopTime && w.ActualStartTime < iDTO.StartTime).ToList();
                outsidePlannedsBlocks.ForEach(f => outsidePlannedMinutes += CalendarUtility.GetOverlappingMinutes(f.ActualStartTime.Value, f.ActualStopTime.Value, DateTime.MinValue, iDTO.StartTime));

                oDTO.PlannedMinutes -= outsidePlannedMinutes;
            }

            oDTO.AvailableMinutes = availableBlocks.Sum(s => s.TotalMinutes);

            return oDTO;
        }

        private GetAvailableEmployeesOutputDTO TaskGetAvailableEmployees()
        {
            var (iDTO, oDTO) = InitTask<GetAvailableEmployeesInputDTO, GetAvailableEmployeesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                Dictionary<int, DateTime> templateBlockStartTimesDict = new Dictionary<int, DateTime>();
                Dictionary<int, DateTime> templateBlockStopTimesDict = new Dictionary<int, DateTime>();
                Dictionary<int, List<ShiftTypeSkill>> shiftTypeSkillsDict = new Dictionary<int, List<ShiftTypeSkill>>();
                Dictionary<int, List<CompanyCategoryRecord>> shiftTypeCategoriesDict = new Dictionary<int, List<CompanyCategoryRecord>>();
                List<TimeScheduleTemplateBlockDTO> templateBlocks = new List<TimeScheduleTemplateBlockDTO>();
                List<TimeScheduleTemplateBlockDTO> templateBlockBreaks = new List<TimeScheduleTemplateBlockDTO>();

                if (iDTO.UseExistingScheduleBlocks)
                {
                    templateBlocks = GetScheduleBlocks(iDTO.TimeScheduleTemplateBlockIds).ToDTOs();
                    foreach (int timeScheduleTemplateBlockId in iDTO.TimeScheduleTemplateBlockIds)
                    {
                        foreach (TimeScheduleTemplateBlockDTO templateBlockBreak in GetBreaksToHandleDueToShiftChanges(timeScheduleTemplateBlockId).ToDTOs())
                        {
                            if (!templateBlockBreaks.Any(tb => tb.TimeScheduleTemplateBlockId == templateBlockBreak.TimeScheduleTemplateBlockId))
                                templateBlockBreaks.Add(templateBlockBreak);
                        }
                    }
                }
                else if (iDTO.TimeScheduleTemplateBlockDTOs != null)
                {
                    templateBlocks.AddRange(iDTO.TimeScheduleTemplateBlockDTOs.Where(x => !x.IsBreak).ToList());
                    templateBlockBreaks.AddRange(iDTO.TimeScheduleTemplateBlockDTOs.Where(x => x.IsBreak).ToList());
                }

                if (!templateBlocks.Any())
                    return oDTO;

                templateBlocks = templateBlocks.Where(i => i.Date.HasValue).OrderBy(i => i.Date).ThenBy(i => i.StartTime).ToList();
                foreach (TimeScheduleTemplateBlockDTO templateBlock in templateBlocks)
                {
                    templateBlockStartTimesDict.Add(templateBlock.TimeScheduleTemplateBlockId, CalendarUtility.GetDateTime(templateBlock.Date.Value, templateBlock.StartTime));
                    templateBlockStopTimesDict.Add(templateBlock.TimeScheduleTemplateBlockId, CalendarUtility.GetDateTime(templateBlock.Date.Value, templateBlock.StopTime).AddDays(templateBlock.StopTime.Date.Subtract(templateBlock.StartTime.Date).Days));
                }

                DateTime date = templateBlocks.First().Date.Value;
                bool hasShiftTypes = templateBlocks.Any(i => i.ShiftTypeId.HasValue);
                List<int> employeeIdsWithPlacement = GetEmployeeIdsWithEmployeeSchedules(iDTO.EmployeeIds, date);

                List<Employee> employees = GetEmployeesWithEmployment(employeeIdsWithPlacement);
                if (!iDTO.GetHidden)
                    employees = employees.Where(e => !e.Hidden).ToList();
                if (!iDTO.GetVacant)
                    employees = employees.Where(e => !e.Vacant).ToList();

                List<TimeScheduleTemplateBlock> templateBlocksForEmployees = GetScheduleBlocksForEmployees(null, employeeIdsWithPlacement, date);
                List<EmployeeSkill> employeeSkillsForEmployees = iDTO.FilterOnSkills && hasShiftTypes ? GetEmployeeSkills(employeeIdsWithPlacement) : null;
                List<EmployeeRequest> employeeRequestsForEmployees = iDTO.FilterOnAvailability ? GetEmployeeRequests(employeeIdsWithPlacement, date, date, TermGroup_EmployeeRequestType.NonInterestRequest) : null;

                List<CompanyCategoryRecord> categoryRecordsForCompany = null;
                List<EmployeeAccount> employeeAccounts = null;
                List<AccountDTO> allAccountInternals = null;
                List<int> shiftTypeIds = null;
                List<ShiftType> shiftTypes = null;
                List<AccountDTO> accountsForScheduleBlock = null;

                bool useAccountHierarchy = UseAccountHierarchy();
                if (useAccountHierarchy)
                {
                    employeeAccounts = GetEmployeeAccountsFromCache(entities, CacheConfig.Company(base.ActorCompanyId), employeeIds: employeeIdsWithPlacement);
                    allAccountInternals = GetAccountInternals().ToDTOs();
                    shiftTypeIds = templateBlocks.Where(i => i.ShiftTypeId.HasValue).Select(i => i.ShiftTypeId.Value).ToList();
                    shiftTypes = GetShiftTypes(shiftTypeIds);
                    accountsForScheduleBlock = AccountManager.GetAccountInternalsFromTemplateBlocks(templateBlocks, actorCompanyId, shiftTypes: shiftTypes, allAccountInternals: allAccountInternals);
                }
                else
                {
                    categoryRecordsForCompany = iDTO.FilterOnShiftType && hasShiftTypes ? GetCompanyCategoryRecordsFromCache(entities, CacheConfig.Company(base.ActorCompanyId), onlyEntityEmployee: true) : null;
                }

                #endregion

                foreach (Employee employee in employees)
                {
                    if (templateBlocks.Any(i => i.EmployeeId == employee.EmployeeId))
                        continue;
                    if (!employeeIdsWithPlacement.Contains(employee.EmployeeId))
                        continue;

                    Employment employment = employee.GetEmployment(date);
                    if (employment == null)
                        continue;

                    List<TimeScheduleTemplateBlock> templateBlocksCurrentDayForEmployee = templateBlocksForEmployees.Where(i => i.EmployeeId == employee.EmployeeId).ToList();
                    bool valid = true;

                    #region Check Overlapping (mandatory)

                    if (valid)
                    {
                        foreach (TimeScheduleTemplateBlock templateBlockForEmployeee in templateBlocksCurrentDayForEmployee.Where(x => !x.IsOnDuty()))
                        {
                            templateBlockStartTimesDict.Add(templateBlockForEmployeee.TimeScheduleTemplateBlockId, CalendarUtility.GetDateTime(templateBlockForEmployeee.Date.Value, templateBlockForEmployeee.StartTime));
                            templateBlockStopTimesDict.Add(templateBlockForEmployeee.TimeScheduleTemplateBlockId, CalendarUtility.GetDateTime(templateBlockForEmployeee.Date.Value, templateBlockForEmployeee.StopTime).AddDays(templateBlockForEmployeee.StopTime.Date.Subtract(templateBlockForEmployeee.StartTime.Date).Days));

                            foreach (TimeScheduleTemplateBlockDTO templateBlock in templateBlocks)
                            {
                                if (templateBlock.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty && CalendarUtility.IsDatesOverlapping(templateBlockStartTimesDict[templateBlockForEmployeee.TimeScheduleTemplateBlockId], templateBlockStopTimesDict[templateBlockForEmployeee.TimeScheduleTemplateBlockId], templateBlockStartTimesDict[templateBlock.TimeScheduleTemplateBlockId], templateBlockStopTimesDict[templateBlock.TimeScheduleTemplateBlockId]))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                            if (!valid)
                                break;
                        }
                    }

                    #endregion

                    #region Check ShiftTypes (optional)

                    if (valid && useAccountHierarchy)
                    {
                        if (iDTO.FilterOnShiftType)
                            valid = !employeeAccounts.GetValidAccounts(employee.EmployeeId, date, date, allAccountInternals, accountsForScheduleBlock, onlyDefaultAccounts: true).IsNullOrEmpty();
                        else
                            valid = !TimeScheduleManager.GetShiftTypesForUser(base.ActorCompanyId, base.RoleId, base.UserId, employee.EmployeeId, true, true, date, date).IsNullOrEmpty();
                    }

                    if (valid && !useAccountHierarchy && iDTO.FilterOnShiftType && !categoryRecordsForCompany.IsNullOrEmpty())
                    {
                        List<CompanyCategoryRecord> categoryRecordsForEmployee = categoryRecordsForCompany.Where(i => i.RecordId == employee.EmployeeId).ToList();

                        foreach (int templateBlockShiftTypeId in templateBlocks.Where(t => t.ShiftTypeId.HasValue).Select(t => t.ShiftTypeId.Value))
                        {
                            if (!shiftTypeCategoriesDict.ContainsKey(templateBlockShiftTypeId))
                                shiftTypeCategoriesDict.Add(templateBlockShiftTypeId, CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.ShiftType, templateBlockShiftTypeId, base.ActorCompanyId, onlyDefaultCategories: false));

                            // Employee needs to have at least one of the shift type's categories
                            bool hasCategory = false;
                            foreach (CompanyCategoryRecord categoryRecord in shiftTypeCategoriesDict[templateBlockShiftTypeId])
                            {
                                if (categoryRecordsForEmployee.Any(i => i.CategoryId == categoryRecord.CategoryId))
                                {
                                    hasCategory = true;
                                    break;
                                }
                            }

                            if (!hasCategory)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    #endregion

                    #region Check Skills (optional)

                    if (valid && iDTO.FilterOnSkills && !employeeSkillsForEmployees.IsNullOrEmpty())
                    {
                        List<EmployeeSkill> employeeSkillsForEmployee = employeeSkillsForEmployees.Where(i => i.EmployeeId == employee.EmployeeId).ToList();
                        foreach (TimeScheduleTemplateBlockDTO templateBlock in templateBlocks.Where(i => i.ShiftTypeId.HasValue))
                        {
                            if (!shiftTypeSkillsDict.ContainsKey(templateBlock.ShiftTypeId.Value))
                                shiftTypeSkillsDict.Add(templateBlock.ShiftTypeId.Value, GetShiftTypeSkills(templateBlock.ShiftTypeId.Value));

                            if (!shiftTypeSkillsDict[templateBlock.ShiftTypeId.Value].IsValid(employeeSkillsForEmployee, templateBlock.Date.Value))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    #endregion

                    #region Check Availability (optional)

                    if (valid && iDTO.FilterOnAvailability && !employeeRequestsForEmployees.IsNullOrEmpty())
                    {
                        List<EmployeeRequest> employeeRequestsForEmployee = employeeRequestsForEmployees.Where(i => i.EmployeeId == employee.EmployeeId).ToList();
                        foreach (EmployeeRequest employeeRequest in employeeRequestsForEmployee)
                        {
                            foreach (TimeScheduleTemplateBlockDTO templateBlock in templateBlocks)
                            {
                                if (CalendarUtility.IsDatesOverlapping(employeeRequest.Start, employeeRequest.Stop, templateBlockStartTimesDict[templateBlock.TimeScheduleTemplateBlockId], templateBlockStopTimesDict[templateBlock.TimeScheduleTemplateBlockId]))
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (!valid)
                                break;
                        }
                    }

                    #endregion

                    #region Check WorkRules (optional)

                    if (valid && iDTO.FilterOnWorkRules)
                    {
                        //Selected shifts
                        List<TimeSchedulePlanningDayDTO> selectedShifts = templateBlocks.Union(templateBlockBreaks).ToList().ToTimeSchedulePlanningDayDTOs();
                        selectedShifts.ForEach(shift => shift.EmployeeId = employee.EmployeeId);

                        //Planned shifts
                        List<TimeSchedulePlanningDayDTO> plannedShifts = templateBlocksCurrentDayForEmployee.ToTimeSchedulePlanningDayDTOs();
                        plannedShifts.AddRange(selectedShifts);

                        //Breaks
                        selectedShifts.ForEach(shift => FixBreaks(plannedShifts, date: shift.ActualDate));

                        plannedShifts = plannedShifts.OrderBy(shift => shift.StartTime).ToList();
                        valid = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, employee.EmployeeId, false, null, breakOnNotAllowedToOverride: true).All(result => result.Success);
                    }

                    #endregion

                    #region MessageGroup (optional)

                    if (valid && iDTO.FilterOnMessageGroupId.HasValue)
                    {
                        MessageGroup messageGroup = CommunicationManager.GetMessageGroup(iDTO.FilterOnMessageGroupId.Value, true);
                        if (messageGroup != null)
                        {
                            bool inGroup = CommunicationManager.IsUserInMessageGroup(messageGroup, base.ActorCompanyId, employee.UserId, employee: employee, dateFrom: date, dateTo: date);
                            if (!inGroup)
                                valid = false;
                        }
                    }

                    #endregion

                    if (valid)
                    {
                        oDTO.AvailableEmployees.Add(new AvailableEmployeesDTO()
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeNr = employee.EmployeeNr,
                            EmployeeName = employee.Name,
                            ScheduleMinutes = templateBlocksCurrentDayForEmployee.GetWork().GetMinutes(),
                            WantsExtraShifts = employee.WantsExtraShifts,
                            EmploymentDays = employee.WantsExtraShifts ? employee.Employment.GetEmploymentDaysToDate(DateTime.Today) : (int?)null,
                            Age = employee.WantsExtraShifts ? EmployeeManager.GetEmployeeAge(employee) : (int?)null
                        });
                    }
                }

                if (oDTO.Result.Success)
                {
                    List<AvailableEmployeesDTO> sortedList = new List<AvailableEmployeesDTO>();
                    sortedList.AddRange(oDTO.AvailableEmployees.Where(i => i.WantsExtraShifts).OrderByDescending(i => i.EmploymentDays).ThenByDescending(i => i.Age).ToList());
                    sortedList.AddRange(oDTO.AvailableEmployees.Where(i => !i.WantsExtraShifts).OrderBy(i => i.EmployeeNr).ToList());
                    oDTO.AvailableEmployees = sortedList;
                }
            }

            return oDTO;
        }

        private InitiateScheduleSwapOutputDTO TaskInitiateScheduleSwap()
        {
            var (iDTO, oDTO) = InitTask<InitiateScheduleSwapInputDTO, InitiateScheduleSwapOutputDTO>();
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

                        oDTO.Result = InitiateScheduleSwap(iDTO.InitiatorEmployeeId, iDTO.InitiatorShiftDate, iDTO.InitiatorShiftIds, iDTO.SwapWithEmployeeId, iDTO.SwapShiftDate, iDTO.SwapWithShiftIds, iDTO.Comment);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        TryCommit(oDTO);
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

        private ApproveScheduleSwapOutputDTO TaskApproveScheduleSwap()
        {
            var (iDTO, oDTO) = InitTask<ApproveScheduleSwapInputDTO, ApproveScheduleSwapOutputDTO>();
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

                        oDTO.Result = ApproveScheduleSwap(iDTO.UserId, iDTO.TimeScheduleSwapRequestId, iDTO.Approved, iDTO.Comment);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        TryCommit(oDTO);
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

        private SaveTimeScheduleShiftOutputDTO TaskSaveTimeScheduleShift()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeScheduleShiftInputDTO, SaveTimeScheduleShiftOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                    if (defaultTimeCodeId == 0)
                    {
                        oDTO.Result = new ActionResult(GetText(8924, "Standard tidkod är ej satt i företagsinställning. Välj standard tidkod för att fortsätta"));
                        return oDTO;
                    }

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        oDTO.Result = SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift, iDTO.Shifts, iDTO.UpdateBreaks, iDTO.SkipXEMailOnChanges, iDTO.AdjustTasks, iDTO.MinutesMoved, iDTO.TimeScheduleScenarioHeadId);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        if (!iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            if (!oDTO.Result.Keys.IsNullOrEmpty())
                                oDTO.StampingTimeBlockDateIds.AddRange(oDTO.Result.Keys);

                            #region Restore to schedule

                            oDTO.Result = RestoreCurrentDaysToSchedule();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region ExtraShift

                            oDTO.Result = SetHasUnhandledShiftChanges();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Notify

                            if (oDTO.Result.Success)
                            {
                                DoNotifyChangeOfDeviations();
                                DoInitiatePayrollWarnings();
                            }

                            SendXEMailOnDayChanged();

                            #endregion
                        }

                        TryCommit(oDTO);
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

        private DeleteTimeScheduleShiftOutputDTO TaskDeleteTimeScheduleShift()
        {
            var (iDTO, oDTO) = InitTask<DeleteTimeScheduleShiftInputDTO, DeleteTimeScheduleShiftOutputDTO>();
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

                        foreach (var timeScheduleTemplateBlockId in iDTO.TimeScheduleTemplateBlockIds)
                        {
                            #region Perform

                            oDTO.Result = DeleteTimeScheduleShift(timeScheduleTemplateBlockId, batchId, iDTO.SkipXEMailOnChanges, iDTO.TimeScheduleScenarioHeadId);

                            #region Included OnDuty shifts

                            if (oDTO.Result.Success && !iDTO.IncludedOnDutyShiftIds.IsNullOrEmpty())
                            {
                                List<TimeScheduleTemplateBlock> includedOnDutyShifts = GetScheduleBlocks(iDTO.IncludedOnDutyShiftIds);

                                foreach (TimeScheduleTemplateBlock includedOnDutyShift in includedOnDutyShifts)
                                {
                                    oDTO.Result = DeleteTimeScheduleShift(includedOnDutyShift.TimeScheduleTemplateBlockId, batchId, iDTO.SkipXEMailOnChanges, iDTO.TimeScheduleScenarioHeadId, includedOnDutyShift);
                                }
                            }

                            #endregion

                            #endregion

                            #region Save

                            if (oDTO.Result.Success)
                                oDTO.Result = Save();
                            else
                                break;
                            #endregion
                        }

                        if (!iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            #region RestoreToSchedule

                            oDTO.Result = RestoreCurrentDaysToSchedule();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region ExtraShift

                            oDTO.Result = SetHasUnhandledShiftChanges();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Notify

                            if (oDTO.Result.Success)
                            {
                                DoNotifyChangeOfDeviations();
                                DoInitiatePayrollWarnings();
                            }

                            SendXEMailOnDayChanged();

                            #endregion
                        }

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return oDTO;
            }
        }

        private HandleTimeScheduleShiftOutputDTO TaskHandleTimeScheduleShift()
        {
            var (iDTO, oDTO) = InitTask<HandleTimeScheduleShiftInputDTO, HandleTimeScheduleShiftOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Prereq

            ActionResultSave absenceAnnouncementSaveNumber = ActionResultSave.NothingSaved;
            // Get premissions here (use it's own entities)
            bool autoAssignOpenShiftPermission = !iDTO.PreventAutoPermissions && FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftWanted_AutoAssignOpenShift, Permission.Modify, iDTO.RoleId, actorCompanyId);
            bool autoChangeEmployeePermission = !iDTO.PreventAutoPermissions && FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftChangeEmployee_AutoAssign, Permission.Modify, iDTO.RoleId, actorCompanyId);
            bool autoSwapEmployeePermission = !iDTO.PreventAutoPermissions && FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee_AutoAssign, Permission.Modify, iDTO.RoleId, actorCompanyId);
            var absenceAnnouncementSourceShifts = new List<TimeSchedulePlanningDayDTO>();
            var guids = new Dictionary<Guid, Guid>();

            #endregion

            #region Save absence

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        Guid batchId = GetNewBatchLink();
                        ShiftHistoryLogCallStackProperties logProperties = null;

                        // Get hidden employee
                        int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                        if (hiddenEmployeeId == 0)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_HiddenEmployeeNotFound);
                            return oDTO;
                        }

                        List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();
                        List<TimeScheduleTemplateBlock> swapTemplateBlocks = new List<TimeScheduleTemplateBlock>();

                        // Get existing presence block
                        TimeScheduleTemplateBlock templateBlock = GetScheduleBlockWithPeriodAndStaffing(iDTO.TimeScheduleTemplateBlockId);
                        templateBlocks.Add(templateBlock);
                        templateBlocks.AddRange(GetScheduleBlocksLinkedWithPeriodAndStaffing(null, templateBlock).Where(x => x.TimeScheduleTemplateBlockId != templateBlock.TimeScheduleTemplateBlockId).ToList());

                        // Get existing swap block
                        TimeScheduleTemplateBlock swapTemplateBlock = null;
                        if (iDTO.SwapTimeScheduleTemplateBlockId != 0)
                        {
                            swapTemplateBlock = GetScheduleBlockWithPeriodAndStaffing(iDTO.SwapTimeScheduleTemplateBlockId);
                            swapTemplateBlocks.Add(swapTemplateBlock);
                            swapTemplateBlocks.AddRange(GetScheduleBlocksLinkedWithPeriodAndStaffing(null, swapTemplateBlock).Where(x => x.TimeScheduleTemplateBlockId != swapTemplateBlock.TimeScheduleTemplateBlockId).ToList());
                        }

                        // For actions on owned shifts,
                        // check that specified employee is still the owner of the shift
                        switch (iDTO.Action)
                        {
                            case HandleShiftAction.Unwanted:
                            case HandleShiftAction.UndoUnwanted:
                            case HandleShiftAction.Absence:
                            case HandleShiftAction.UndoAbsence:
                            case HandleShiftAction.AbsenceAnnouncement:
                            case HandleShiftAction.UndoAbsenceAnnouncement:
                                if (templateBlock.EmployeeId != iDTO.EmployeeId)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.HandleTimeScheduleShift_EmployeeNotShiftOwner);
                                    return oDTO;
                                }
                                break;
                            case HandleShiftAction.ChangeEmployee:
                            case HandleShiftAction.SwapEmployee:
                                // iDTO.EmployeeId will contain the new employee
                                // so we can't check it here. In that case we also need to pass old employee id
                                break;
                        }

                        #endregion

                        if (iDTO.Action == HandleShiftAction.Wanted && templateBlock.EmployeeId == hiddenEmployeeId && autoAssignOpenShiftPermission)
                        {
                            #region Wanted - Actions handled in save method

                            // User wants a shift
                            // If shift is free (hidden employee) and user has required permission, assign it to current employee                         

                            logProperties = new ShiftHistoryLogCallStackProperties(Guid.NewGuid(), 0, TermGroup_ShiftHistoryType.HandleShiftActionWanted, null, false);
                            oDTO.Result = DragActionMoveShifts(logProperties, templateBlock.TimeScheduleTemplateBlockId, iDTO.EmployeeId, templateBlock.Date.Value, true, null, GetNewShiftLink(), true);

                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_ShiftAssignedOK;
                            oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;

                            #endregion
                        }
                        else if (iDTO.Action == HandleShiftAction.ChangeEmployee && autoChangeEmployeePermission)
                        {
                            #region Change employee - Actions handled in save method

                            // If user has required permission, change employee on shift (assign it to specified employee)                      

                            logProperties = new ShiftHistoryLogCallStackProperties(Guid.NewGuid(), 0, TermGroup_ShiftHistoryType.HandleShiftActionChangeEmployee, null, false);
                            oDTO.Result = DragActionMoveShifts(logProperties, templateBlock.TimeScheduleTemplateBlockId, iDTO.EmployeeId, templateBlock.Date.Value, true, null, GetNewShiftLink(), true);

                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_EmployeeChangedOK;
                            oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                            Employee employee = GetEmployeeWithContactPersonFromCache(iDTO.EmployeeId, true);
                            if (employee != null)
                                oDTO.Result.StringValue = employee.Name;

                            #endregion
                        }
                        else if (iDTO.Action == HandleShiftAction.SwapEmployee && autoSwapEmployeePermission)
                        {
                            #region Swap employee - Actions handled in save method

                            // If user has required permission, swap employees on shifts
                            // iDTO.EmployeeId will contain the employee to swap with

                            var swapShift = swapTemplateBlock.ToTimeSchedulePlanningDayDTO();
                            var shift = templateBlock.ToTimeSchedulePlanningDayDTO();

                            oDTO.Result = SwapTimeScheduleShifts(shift, swapShift, TermGroup_ShiftHistoryType.HandleShiftActionSwapEmployee, true, true, false, null);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_EmployeeSwappedOK;
                            oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                            Employee employee = GetEmployeeWithContactPersonFromCache(iDTO.EmployeeId, true);
                            if (employee != null)
                                oDTO.Result.StringValue = employee.Name;

                            #endregion
                        }
                        else
                        {
                            #region Actions handled in this method

                            #region Prereq

                            // Get User
                            User user = GetUserFromCache();
                            if (user == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_UserNotFound);
                                return oDTO;
                            }

                            #endregion

                            switch (iDTO.Action)
                            {
                                case HandleShiftAction.Wanted:
                                    #region Wanted
                                    // If shift is already assigned or employee not permitted to take open shifts, add current employee to queue
                                    if (templateBlock.EmployeeId != hiddenEmployeeId || !autoAssignOpenShiftPermission)
                                    {
                                        oDTO.Result = AddEmployeeToShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, iDTO.TimeScheduleTemplateBlockId, iDTO.EmployeeId, true);
                                        if (!oDTO.Result.Success)
                                            return oDTO;

                                        oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_EmployeeQueued;
                                    }
                                    break;
                                #endregion
                                case HandleShiftAction.UndoWanted:
                                    #region UndoWanted
                                    // Remove employee from queue
                                    oDTO.Result = RemoveEmployeeFromShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, iDTO.TimeScheduleTemplateBlockId, iDTO.EmployeeId, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_EmployeeRemovedFromQueue;
                                    break;
                                #endregion
                                case HandleShiftAction.Unwanted:
                                    #region Unwanted
                                    // Change user status on shift to 'Unwanted' to indicate for other employees that it's free to take
                                    oDTO.Result = SetShiftUserStatus(templateBlocks, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted, null, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_ShiftUnwantedOK;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                                    break;
                                #endregion
                                case HandleShiftAction.UndoUnwanted:
                                    #region UndoUnwanted
                                    // Change user status back to 'Accepted'
                                    oDTO.Result = SetShiftUserStatus(templateBlocks, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted, null, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_ShiftUndoUnwantedOK;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                                    break;
                                #endregion
                                case HandleShiftAction.Absence:
                                    #region Absence
                                    // Get absence start/stop times depending on deviation cause
                                    DateTime absenceStart;
                                    DateTime absenceStop;
                                    // Check if deviation cause is whole day, in that case remove times
                                    TimeDeviationCause cause = GetTimeDeviationCauseFromCache(iDTO.TimeDeviationCauseId);
                                    if (cause != null && cause.OnlyWholeDay)
                                    {
                                        absenceStart = templateBlock.Date.Value;
                                        absenceStop = templateBlock.Date.Value.AddDays(1).AddSeconds(-1);
                                    }
                                    else if (templateBlocks.Count > 1)
                                    {
                                        absenceStart = templateBlocks.OrderBy(x => x.ActualStartTime.Value).FirstOrDefault().ActualStartTime.Value;
                                        absenceStop = templateBlocks.OrderBy(x => x.ActualStartTime.Value).LastOrDefault().ActualStopTime.Value;
                                    }
                                    else
                                    {
                                        absenceStart = CalendarUtility.MergeDateAndTime(templateBlock.Date.Value, templateBlock.StartTime);
                                        absenceStop = CalendarUtility.MergeDateAndTime(templateBlock.Date.Value, templateBlock.StopTime);
                                    }

                                    // Create absence request
                                    EmployeeRequest newReq = new EmployeeRequest()
                                    {
                                        ActorCompanyId = actorCompanyId,
                                        EmployeeId = templateBlock.EmployeeId.Value,
                                        TimeDeviationCauseId = iDTO.TimeDeviationCauseId,
                                        Start = absenceStart,
                                        Stop = absenceStop,
                                        Type = (int)TermGroup_EmployeeRequestType.AbsenceRequest,
                                        Status = (int)TermGroup_EmployeeRequestStatus.RequestPending,
                                        ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.None,
                                    };

                                    oDTO.Result = SaveEmployeeRequest(newReq, templateBlock.EmployeeId.Value, TermGroup_EmployeeRequestType.AbsenceRequest);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_ShiftAbsenceQueued;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                                    break;
                                #endregion
                                case HandleShiftAction.UndoAbsence:
                                    #region UndoAbsence
                                    // Find absence request
                                    // First check with shift actual start/stop, if not found check with whole day
                                    EmployeeRequest employeeRequest = GetEmployeeRequestByExactTime(iDTO.EmployeeId, CalendarUtility.MergeDateAndTime(templateBlock.Date.Value, templateBlock.StartTime), CalendarUtility.MergeDateAndTime(templateBlock.Date.Value, templateBlock.StopTime), TermGroup_EmployeeRequestType.AbsenceRequest);
                                    if (employeeRequest == null)
                                    {
                                        employeeRequest = GetEmployeeRequestByExactTime(iDTO.EmployeeId, templateBlock.Date.Value, templateBlock.Date.Value.AddDays(1).AddSeconds(-1), TermGroup_EmployeeRequestType.AbsenceRequest);
                                        if (employeeRequest == null)
                                        {
                                            // No or multiple requests was found, return error
                                            oDTO.Result = new ActionResult((int)ActionResultSave.HandleTimeScheduleShift_ShiftUndoAbsenceNotFound);
                                            return oDTO;
                                        }
                                    }

                                    // Check request status
                                    if (employeeRequest.Status != (int)TermGroup_EmployeeRequestStatus.RequestPending)
                                    {
                                        // Request status must be pending
                                        oDTO.Result = new ActionResult((int)ActionResultSave.HandleTimeScheduleShift_ShiftUndoAbsenceNotPending);
                                        return oDTO;
                                    }

                                    // Remove absence request
                                    ChangeEntityState(employeeRequest, SoeEntityState.Deleted, user);
                                    oDTO.Result = Save();
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    // Change user status back to 'Accepted'
                                    oDTO.Result = SetShiftUserStatus(templateBlocks, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted, null, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_ShiftUndoAbsenceOK;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                                    break;
                                #endregion
                                case HandleShiftAction.ChangeEmployee:
                                    #region Change employee
                                    // Change user status on shift to 'Unwanted'
                                    oDTO.Result = SetShiftUserStatus(templateBlocks, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted, null, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    // Add other employee to queue
                                    oDTO.Result = AddEmployeeToShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, iDTO.TimeScheduleTemplateBlockId, iDTO.EmployeeId, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_EmployeeChangeQueued;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                                    break;
                                #endregion
                                case HandleShiftAction.SwapEmployee:
                                    #region Swap employee
                                    // Change user status on shift to 'Unwanted'
                                    oDTO.Result = SetShiftUserStatus(templateBlocks, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted, null, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    // Add other employee to queue
                                    oDTO.Result = AddEmployeeToShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, iDTO.TimeScheduleTemplateBlockId, iDTO.EmployeeId, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    // Change user status on swap shift to 'Unwanted'
                                    oDTO.Result = SetShiftUserStatus(swapTemplateBlocks, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted, null, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    // Add current employee to queue
                                    oDTO.Result = AddEmployeeToShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, iDTO.SwapTimeScheduleTemplateBlockId, templateBlock.EmployeeId.Value, true);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_EmployeeSwapQueued;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;
                                    break;
                                #endregion

                                case HandleShiftAction.AbsenceAnnouncement:
                                    #region AbsenceAnnouncement

                                    TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(iDTO.TimeScheduleTemplateBlockId);
                                    if (scheduleBlock != null)
                                    {
                                        List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksFromCache(scheduleBlock.EmployeeId.Value, scheduleBlock.Date.Value.Date).Where(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList();
                                        absenceAnnouncementSourceShifts.AddRange(scheduleBlocks.ToTimeSchedulePlanningDayDTOs());
                                    }

                                    if (!absenceAnnouncementSourceShifts.IsNullOrEmpty())
                                    {
                                        this.InitAbsenceDay(scheduleBlock.EmployeeId.Value, absenceAnnouncementSourceShifts.First().ActualDate, timeDeviationCauseId: iDTO.TimeDeviationCauseId);

                                        oDTO.Result = SaveDeviationsFromShifts(absenceAnnouncementSourceShifts, iDTO.EmployeeId, iDTO.TimeDeviationCauseId, false, null, null);
                                        if (!oDTO.Result.Success)
                                            return oDTO;

                                        foreach (var sourceShift in absenceAnnouncementSourceShifts)
                                        {
                                            if (!sourceShift.Link.HasValue)
                                                sourceShift.Link = GetNewShiftLink();

                                            // Every unique source Guid will get one new target Guid
                                            // Linked source shifts will still be linked as targets, but with a new Guid
                                            Guid targetLink;
                                            if (!guids.ContainsKey(sourceShift.Link.Value))
                                            {
                                                targetLink = GetNewShiftLink();
                                                guids.Add(sourceShift.Link.Value, targetLink);
                                            }
                                            else
                                                targetLink = guids[sourceShift.Link.Value];

                                            #region Copy

                                            oDTO.Result = AbsenceAnnouncementActionCopyShiftToHiddenEmployee(sourceShift, iDTO.TimeDeviationCauseId, batchId, targetLink);
                                            if (!oDTO.Result.Success)
                                                return oDTO;

                                            oDTO.Result = Save();
                                            if (!oDTO.Result.Success)
                                                return oDTO;

                                            #endregion
                                        }
                                    }

                                    absenceAnnouncementSaveNumber = ActionResultSave.HandleTimeScheduleShift_AbsenceAnnouncementOK;
                                    oDTO.Result.IntegerValue = iDTO.TimeScheduleTemplateBlockId;

                                    #endregion
                                    break;
                            }
                            #endregion
                        }

                        #region RestoreToSchedule

                        oDTO.Result = RestoreCurrentDaysToSchedule();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region ExtraShift

                        oDTO.Result = SetHasUnhandledShiftChanges();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Notify

                        SendXEMailOnDayChanged();

                        #endregion

                        TryCommit(oDTO);
                        oDTO.Result.Value = null;   // Clear 'Value' which is set to a whole DTO in a previous method
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
                    if (oDTO.Result.Success)
                    {
                        //Send XEmail
                        if (iDTO.Action == HandleShiftAction.AbsenceAnnouncement && absenceAnnouncementSaveNumber == ActionResultSave.HandleTimeScheduleShift_AbsenceAnnouncementOK)
                        {
                            bool sentWithErrors = false;
                            SendXEMailOnAbsenceAnnouncement(absenceAnnouncementSourceShifts, iDTO.EmployeeId, iDTO.TimeDeviationCauseId, ref sentWithErrors);
                            if (sentWithErrors)
                                oDTO.Result.SuccessNumber = (int)ActionResultSave.HandleTimeScheduleShift_AbsenceAnnouncementOKXEmailSentWithErrors;
                        }
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            #endregion

            return oDTO;
        }

        private SplitTimeScheduleShiftOutputDTO TaskSplitTimeScheduleShift()
        {
            var (iDTO, oDTO) = InitTask<SplitTimeScheduleShiftInputDTO, SplitTimeScheduleShiftOutputDTO>();
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

                        // Link
                        if (!iDTO.Shift.Link.HasValue)
                            iDTO.Shift.Link = GetNewShiftLink();

                        // Copy DTO
                        TimeSchedulePlanningDayDTO clone = new TimeSchedulePlanningDayDTO();
                        EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(clone, iDTO.Shift);
                        clone.TimeScheduleTemplateBlockId = 0;

                        // Change properties
                        iDTO.Shift.StopTime = iDTO.SplitTime;
                        iDTO.Shift.EmployeeId = iDTO.EmployeeId1;
                        clone.StartTime = iDTO.SplitTime;
                        clone.EmployeeId = iDTO.EmployeeId2;
                        if (!iDTO.KeepShiftsTogether || iDTO.EmployeeId1 != iDTO.EmployeeId2)
                            clone.Link = Guid.NewGuid();

                        #endregion

                        #region Tasks

                        List<TimeScheduleTemplateBlockTaskDTO> allTasks = GetTimeScheduleTemplateBlockTasks(iDTO.Shift.TimeScheduleTemplateBlockId).ToDTOs().ToList();
                        //Tasks that will get a new parent
                        List<TimeScheduleTemplateBlockTaskDTO> tasksToMove = allTasks.GetTaskThatStartsAfterGivenTime(iDTO.SplitTime);
                        List<TimeScheduleTemplateBlockTaskDTO> splittedTasks = new List<TimeScheduleTemplateBlockTaskDTO>();

                        foreach (var task in allTasks)
                        {
                            if (task.IsOverlapped(iDTO.SplitTime))
                            {
                                #region Split task                                

                                // Copy DTO
                                TimeScheduleTemplateBlockTaskDTO taskClone = new TimeScheduleTemplateBlockTaskDTO();
                                EntityUtil.CopyDTO<TimeScheduleTemplateBlockTaskDTO>(taskClone, task);

                                //Change properties on original task
                                task.StopTime = iDTO.SplitTime;
                                splittedTasks.Add(task);

                                //Change properties on new task
                                taskClone.TimeScheduleTemplateBlockTaskId = 0;
                                taskClone.TimeScheduleTemplateBlockId = null;
                                taskClone.StartTime = iDTO.SplitTime;
                                tasksToMove.Add(taskClone);

                                #endregion

                            }
                        }

                        #endregion

                        #region Breaks

                        //Only handle (MOVE) breaks for the new shift, by moving the breaks the original shift will "loose" the breaks that is overlapping clone.starttime and clone.stoptime
                        TimeScheduleTemplateBlock sourceScheduleBlock = GetScheduleBlockWithPeriodAndStaffing(iDTO.Shift.TimeScheduleTemplateBlockId);
                        if (sourceScheduleBlock != null)
                        {
                            oDTO.Result = ValidateShiftAgainstGivenScenario(sourceScheduleBlock, iDTO.TimeScheduleScenarioHeadId);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            // If hidden employee, only get breaks connected to current shift
                            Guid? link = null;
                            if (IsHiddenEmployeeFromCache(sourceScheduleBlock.EmployeeId.Value) && !String.IsNullOrEmpty(sourceScheduleBlock.Link))
                                link = new Guid(sourceScheduleBlock.Link);

                            List<TimeScheduleTemplateBlock> sourceBreakBlocks = GetTimeScheduleShiftBreaks(iDTO.TimeScheduleScenarioHeadId, sourceScheduleBlock.TimeScheduleEmployeePeriodId.Value, link).ToList();

                            clone.BreaksToHandle = GetBreaksToHandleDueToShiftChanges(clone, sourceBreakBlocks).Select(b => b.TimeScheduleTemplateBlockId).ToList();
                            clone.MoveBreaksWithShift = true;
                            clone.HandleBreaks = true;
                        }

                        #endregion

                        #region Perform/Save

                        Guid batchId = GetNewBatchLink();

                        ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, iDTO.Shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.TaskSplitTimeScheduleShift, null, iDTO.SkipXEMailOnChanges);

                        //Get history for original shift before new history entries are created 
                        var originalHistory = GetShiftHistory(iDTO.Shift.TimeScheduleTemplateBlockId);

                        // Save original
                        oDTO.Result = SaveTimeScheduleShiftAndLogChanges(iDTO.Shift, logProperties);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.TaskSplitTimeScheduleShift, null, iDTO.SkipXEMailOnChanges);

                        // Save copy
                        oDTO.Result = SaveTimeScheduleShiftAndLogChanges(clone, logProperties);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #region Copy history from original shift to the new shift

                        oDTO.Result = CopyShiftHistory(clone.TimeScheduleTemplateBlockId, originalHistory);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Tasks

                        oDTO.Result = SaveTimeScheduleTemplateBlockTasks(splittedTasks);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        //Set new parent
                        foreach (var taskToMove in tasksToMove)
                        {
                            taskToMove.TimeScheduleTemplateBlockId = clone.TimeScheduleTemplateBlockId;
                        }

                        oDTO.Result = SaveTimeScheduleTemplateBlockTasks(tasksToMove);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        oDTO.Result.SuccessNumber = (int)ActionResultSave.SplitTimeScheduleShift_ShiftsSplittedOK;
                        oDTO.Result.IntegerValue = clone.TimeScheduleTemplateBlockId;

                        if (!iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            #region RestoreToSchedule

                            oDTO.Result = RestoreCurrentDaysToSchedule();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region ExtraShift

                            oDTO.Result = SetHasUnhandledShiftChanges();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Notify

                            if (oDTO.Result.Success)
                            {
                                DoNotifyChangeOfDeviations();
                                DoInitiatePayrollWarnings();
                            }

                            SendXEMailOnDayChanged();

                            #endregion
                        }

                        if (oDTO.Result.Success)
                            oDTO.Result = Save();

                        TryCommit(oDTO);

                        #endregion
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

        private DragTimeScheduleShiftOutputDTO TaskDragTimeScheduleShift()
        {
            var (iDTO, oDTO) = InitTask<DragTimeScheduleShiftInputDTO, DragTimeScheduleShiftOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            Guid targetLink;
            List<TimeSchedulePlanningDayDTO> absenceSourceShifts = new List<TimeSchedulePlanningDayDTO>();
            Dictionary<Guid, Guid> guids = new Dictionary<Guid, Guid>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        // Get User
                        User user = GetUserFromCache();
                        if (user == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_UserNotFound);
                            return oDTO;
                        }

                        // Get onDuty shifts
                        List<TimeScheduleTemplateBlock> includedOnDutyShifts = new List<TimeScheduleTemplateBlock>();
                        if (!iDTO.IncludedOnDutyShiftIds.IsNullOrEmpty())
                            includedOnDutyShifts = GetScheduleBlocks(iDTO.IncludedOnDutyShiftIds);


                        if (iDTO.Action == DragShiftAction.Absence || iDTO.Action == DragShiftAction.Copy || iDTO.Action == DragShiftAction.Move)
                        {
                            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(iDTO.SourceShiftId);
                            if (scheduleBlock != null)
                            {
                                if (iDTO.Action == DragShiftAction.Copy || iDTO.Action == DragShiftAction.Move)
                                {
                                    // Do not link on duty shifts with other shifts
                                    if (scheduleBlock.IsOnDuty())
                                        iDTO.KeepTargetShiftsTogether = false;
                                }
                                else
                                {
                                    List<TimeScheduleTemplateBlock> scheduleBlocks = new List<TimeScheduleTemplateBlock>();

                                    if (iDTO.WholeDayAbsence)
                                    {
                                        var shiftsForWholeDay = GetTimeScheduleShifts(iDTO.TimeScheduleScenarioHeadId, scheduleBlock.TimeScheduleEmployeePeriodId.Value, null);
                                        scheduleBlocks.AddRange(shiftsForWholeDay);
                                    }
                                    else
                                    {
                                        scheduleBlocks.Add(scheduleBlock);
                                        if (iDTO.KeepSourceShiftsTogether)
                                            scheduleBlocks.AddRange(GetScheduleBlocksLinked(iDTO.TimeScheduleScenarioHeadId, scheduleBlock.EmployeeId.Value, scheduleBlock.Date.Value, scheduleBlock.Link, (TermGroup_TimeScheduleTemplateBlockType)scheduleBlock.Type).Where(x => x.TimeScheduleTemplateBlockId != scheduleBlock.TimeScheduleTemplateBlockId).ToList());

                                        #region Include OnDuty shifts

                                        if (!includedOnDutyShifts.IsNullOrEmpty())
                                        {
                                            scheduleBlocks.AddRange(includedOnDutyShifts);
                                        }

                                        #endregion
                                    }

                                    absenceSourceShifts.AddRange(scheduleBlocks.ToTimeSchedulePlanningDayDTOs());
                                }
                            }
                        }

                        #endregion

                        #region Perform

                        ShiftHistoryLogCallStackProperties logProperties = null;
                        switch (iDTO.Action)
                        {
                            case DragShiftAction.Move:
                                // (Drag shift from a slot to another slot)
                                #region Move

                                if (!IsHiddenEmployeeFromCache(iDTO.EmployeeId) && iDTO.KeepTargetShiftsTogether && iDTO.TargetLink.HasValue)
                                    targetLink = iDTO.TargetLink.Value;
                                else
                                    targetLink = GetNewShiftLink();

                                // Move source shift to new date and possibly new employee                                
                                logProperties = new ShiftHistoryLogCallStackProperties(Guid.NewGuid(), 0, TermGroup_ShiftHistoryType.DragShiftActionMove, null, iDTO.SkipXEMailOnChanges);
                                oDTO.Result = DragActionMoveShifts(logProperties, iDTO.SourceShiftId, iDTO.EmployeeId, iDTO.Start.Date, iDTO.KeepSourceShiftsTogether, iDTO.TimeScheduleScenarioHeadId, targetLink, true, true, null, includedOnDutyShifts);

                                if (!oDTO.Result.Success)
                                    return oDTO;

                                if (iDTO.UpdateLinkOnTarget)
                                    oDTO.Result = UpdateLinkOnShifts(iDTO.TimeScheduleScenarioHeadId, targetLink, iDTO.EmployeeId, iDTO.Start.Date, iDTO.SourceShiftId);

                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion
                                break;

                            case DragShiftAction.ShiftRequest: //This should be a task by its own
                                                               // (Drag shift from a slot to another slot)
                                #region Move

                                if (iDTO.KeepTargetShiftsTogether && iDTO.TargetLink.HasValue)
                                    targetLink = iDTO.TargetLink.Value;
                                else
                                    targetLink = GetNewShiftLink();

                                // Move source shift to new date and possibly new employee                                
                                logProperties = new ShiftHistoryLogCallStackProperties(Guid.NewGuid(), 0, TermGroup_ShiftHistoryType.ShiftRequest, null, iDTO.SkipXEMailOnChanges);
                                oDTO.Result = DragActionMoveShifts(logProperties, iDTO.SourceShiftId, iDTO.EmployeeId, iDTO.Start.Date, iDTO.KeepSourceShiftsTogether, iDTO.TimeScheduleScenarioHeadId, targetLink, true, removeShiftRequest: false);

                                if (!oDTO.Result.Success)
                                    return oDTO;

                                if (iDTO.UpdateLinkOnTarget)
                                    oDTO.Result = UpdateLinkOnShifts(iDTO.TimeScheduleScenarioHeadId, targetLink, iDTO.EmployeeId, iDTO.Start.Date, iDTO.SourceShiftId);

                                if (!oDTO.Result.Success)
                                    return oDTO;

                                if (iDTO.MessageId.HasValue)
                                {
                                    var message = CommunicationManager.GetMessage(this.entities, iDTO.MessageId.Value);
                                    if (message != null)
                                        message.HandledByJob = true;
                                }

                                #endregion
                                break;
                            case DragShiftAction.Copy:
                                // (Drag shift from a slot to another slot)
                                #region Copy

                                if (!IsHiddenEmployeeFromCache(iDTO.EmployeeId) && iDTO.KeepTargetShiftsTogether && iDTO.TargetLink.HasValue)
                                    targetLink = iDTO.TargetLink.Value;
                                else
                                    targetLink = GetNewShiftLink();

                                // Copy source shift to new date and possibly new employee                                                                                             
                                logProperties = new ShiftHistoryLogCallStackProperties(Guid.NewGuid(), 0, TermGroup_ShiftHistoryType.DragShiftActionCopy, null, iDTO.SkipXEMailOnChanges);
                                oDTO.Result = DragActionCopyShifts(logProperties, iDTO.SourceShiftId, iDTO.EmployeeId, iDTO.Start.Date, iDTO.KeepSourceShiftsTogether, iDTO.CopyTaskWithShift, iDTO.TimeScheduleScenarioHeadId, targetLink, true, includedOnDutyShifts);

                                if (!oDTO.Result.Success)
                                    return oDTO;

                                if (iDTO.UpdateLinkOnTarget)
                                    oDTO.Result = UpdateLinkOnShifts(iDTO.TimeScheduleScenarioHeadId, targetLink, iDTO.EmployeeId, iDTO.Start.Date, iDTO.SourceShiftId);

                                #endregion
                                break;
                            case DragShiftAction.Replace:
                                // (Drag shift to slot with existing shift)
                                #region Replace
                                // Move source shift to new date and possibly new employee
                                // Remove target shift                                
                                oDTO.Result = DragActionReplaceExistingShifts(iDTO.SourceShiftId, iDTO.TargetShiftId, iDTO.EmployeeId, iDTO.Start.Date, iDTO.KeepSourceShiftsTogether, iDTO.KeepTargetShiftsTogether, iDTO.SkipXEMailOnChanges, iDTO.TimeScheduleScenarioHeadId, true, iDTO.IncludeOnDutyShifts, iDTO.IncludedOnDutyShiftIds);
                                #endregion
                                break;
                            case DragShiftAction.ReplaceAndFree:
                                // (Drag shift to slot with existing shift)
                                #region ReplaceAndFree
                                // Move source shift to new date and possibly new employee
                                // Set target shift to open                                
                                oDTO.Result = DragActionReplaceAndFreeExistingShifts(iDTO.SourceShiftId, iDTO.TargetShiftId, iDTO.EmployeeId, iDTO.Start.Date, iDTO.KeepSourceShiftsTogether, iDTO.SkipXEMailOnChanges, iDTO.TimeScheduleScenarioHeadId, true);
                                #endregion
                                break;
                            case DragShiftAction.SwapEmployee:
                                // (Drag shift to slot with existing shift)
                                #region SwapEmployee
                                // Swap employees between source and target shifts
                                oDTO.Result = DragActionSwapEmployeesOnShifts(iDTO.SourceShiftId, iDTO.TargetShiftId, iDTO.KeepSourceShiftsTogether, iDTO.KeepTargetShiftsTogether, iDTO.SkipXEMailOnChanges, iDTO.TimeScheduleScenarioHeadId, false, iDTO.IncludeOnDutyShifts, iDTO.IncludedOnDutyShiftIds);
                                #endregion
                                break;
                            case DragShiftAction.Absence:
                                Guid batchId = GetNewBatchLink();
                                // (Copy source shifts to target and set source shifts to absence)
                                #region Absence

                                if (!absenceSourceShifts.IsNullOrEmpty())
                                {
                                    int sourceEmployeeId = absenceSourceShifts.First().EmployeeId;
                                    this.InitAbsenceDay(sourceEmployeeId, absenceSourceShifts.First().ActualDate, timeDeviationCauseId: iDTO.TimeDeviationCauseId);

                                    oDTO.Result = SaveDeviationsFromShifts(absenceSourceShifts, sourceEmployeeId, iDTO.TimeDeviationCauseId, false, iDTO.EmployeeChildId, iDTO.TimeScheduleScenarioHeadId);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    foreach (var sourceShift in absenceSourceShifts)
                                    {
                                        if (!sourceShift.Link.HasValue)
                                            sourceShift.Link = GetNewShiftLink();

                                        // Every unique source Guid will get one new target Guid
                                        // Linked source shifts will still be linked as targets, but with a new Guid
                                        if (!guids.ContainsKey(sourceShift.Link.Value))
                                        {
                                            targetLink = GetNewShiftLink();
                                            guids.Add(sourceShift.Link.Value, targetLink);
                                        }
                                        else
                                            targetLink = guids[sourceShift.Link.Value];

                                        #region Copy

                                        sourceShift.EmployeeId = iDTO.EmployeeId;
                                        DateTime start = CalendarUtility.MergeDateAndTime(iDTO.Start.Date, sourceShift.StartTime);
                                        if (sourceShift.BelongsToPreviousDay)
                                            start = start.AddDays(1);
                                        else if (sourceShift.BelongsToNextDay)
                                            start = start.AddDays(-1);

                                        DateTime stop = start.Add(sourceShift.StopTime - sourceShift.StartTime);
                                        sourceShift.StartTime = start;
                                        sourceShift.StopTime = stop;
                                        oDTO.Result = DragActionAbsence(sourceShift, iDTO.TimeDeviationCauseId, batchId, targetLink, iDTO.SkipXEMailOnChanges, iDTO.EmployeeChildId);
                                        if (!oDTO.Result.Success)
                                            return oDTO;

                                        oDTO.Result = Save();
                                        if (!oDTO.Result.Success)
                                            return oDTO;

                                        #endregion
                                    }
                                }

                                #endregion
                                break;
                        }

                        #endregion

                        if (!iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            #region RestoreToSchedule

                            oDTO.Result = RestoreCurrentDaysToSchedule();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region ExtraShift

                            oDTO.Result = SetHasUnhandledShiftChanges();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Notify

                            if (oDTO.Result.Success)
                            {
                                DoNotifyChangeOfDeviations();
                                DoInitiatePayrollWarnings();
                            }

                            SendXEMailOnDayChanged();

                            #endregion
                        }

                        TrySaveAndCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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

        private DragTimeScheduleShiftMultipelOutputDTO TaskDragTimeScheduleShiftMultipel()
        {
            var (iDTO, oDTO) = InitTask<DragTimeScheduleShiftMultipelInputDTO, DragTimeScheduleShiftMultipelOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            Guid targetLink;
            bool updateLinkOnTarget = false;
            Guid batchId = GetNewBatchLink();
            Dictionary<Guid, Guid> guids = new Dictionary<Guid, Guid>();
            DateTime? previousDate = null;
            List<TimeSchedulePlanningDayDTO> sourceShifts = new List<TimeSchedulePlanningDayDTO>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    List<TimeScheduleTemplateBlock> originalSourceShifts;
                    if (iDTO.Action == DragShiftAction.MoveWithCycle || iDTO.Action == DragShiftAction.CopyWithCycle)
                    {
                        originalSourceShifts = GetSourceShiftsAndShiftsInCycle(iDTO.SourceShiftIds, iDTO.StandbyCycleWeek.Value, iDTO.StandbyCycleDateFrom.Value, iDTO.StandbyCycleDateTo.Value, iDTO.TimeScheduleScenarioHeadId).Where(x => x.Date.HasValue).ToList();
                    }
                    else
                    {
                        originalSourceShifts = GetScheduleBlocks(iDTO.SourceShiftIds);
                    }

                    if (iDTO.IsStandByView && originalSourceShifts.Any(s => !s.IsStandby()))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, GetText(10117, "Otillåten ändring") + ": " + GetText(8917, "Endast tillåtet att flytta/kopiera pass av typen beredskap"));
                        return oDTO;
                    }

                    foreach (var originalSourceShift in originalSourceShifts)
                    {
                        var dto = originalSourceShift.ToTimeSchedulePlanningDayDTO();
                        if (dto != null)
                            sourceShifts.Add(dto);
                    }
                    sourceShifts = sourceShifts.OrderBy(x => x.StartTime).ToList();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        // Get User
                        User user = GetUserFromCache();
                        if (user == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_UserNotFound);
                            return oDTO;
                        }

                        // Get hidden employee
                        int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();

                        // Get onDuty shifts
                        List<TimeScheduleTemplateBlock> includedOnDutyShifts = new List<TimeScheduleTemplateBlock>();
                        if (!iDTO.IncludedOnDutyShiftIds.IsNullOrEmpty())
                            includedOnDutyShifts = GetScheduleBlocks(iDTO.IncludedOnDutyShiftIds);

                        #endregion

                        #region Perform

                        ShiftHistoryLogCallStackProperties logProperties = null;

                        switch (iDTO.Action)
                        {
                            case DragShiftAction.Move:
                            case DragShiftAction.MoveWithCycle:
                                #region Move

                                Dictionary<int, List<TimeScheduleTemplateBlock>> sourceBreakBlocks = new Dictionary<int, List<TimeScheduleTemplateBlock>>();

                                foreach (var sourceShift in sourceShifts)
                                {
                                    if (!sourceBreakBlocks.ContainsKey(sourceShift.TimeScheduleEmployeePeriodId))
                                        sourceBreakBlocks.Add(sourceShift.TimeScheduleEmployeePeriodId, GetTimeScheduleShiftBreaks(sourceShift.TimeScheduleScenarioHeadId, sourceShift.TimeScheduleEmployeePeriodId, null).ToList());
                                }

                                targetLink = GetNewShiftLink();
                                foreach (var sourceShift in sourceShifts)
                                {
                                    if (!previousDate.HasValue)
                                        previousDate = sourceShift.ActualDate.AddDays(iDTO.OffsetDays);

                                    //We need a unique guid on each day
                                    if (previousDate.Value != sourceShift.ActualDate.AddDays(iDTO.OffsetDays))
                                    {
                                        targetLink = GetNewShiftLink();
                                        previousDate = sourceShift.ActualDate.AddDays(iDTO.OffsetDays);
                                    }

                                    updateLinkOnTarget = false;
                                    if (sourceShift.IsSchedule() && iDTO.LinkWithExistingShiftsIfPossible && iDTO.DestinationEmployeeId != hiddenEmployeeId)
                                    {
                                        List<string> links = GetScheduleBlockLinks(iDTO.TimeScheduleScenarioHeadId, iDTO.DestinationEmployeeId, sourceShift.ActualDate.AddDays(iDTO.OffsetDays));

                                        //set/decide targetlink and updateLinkOnTarget
                                        if (links.All(x => string.IsNullOrEmpty(x))) //handle old data
                                            updateLinkOnTarget = true;
                                        else
                                        {
                                            int nbrOfUniqueLinks = links.Distinct().Count();

                                            if (nbrOfUniqueLinks == 1)
                                                targetLink = new Guid(links.FirstOrDefault());
                                        }
                                    }
                                    else
                                    {
                                        if (!sourceShift.Link.HasValue)
                                            sourceShift.Link = GetNewShiftLink();

                                        // Every unique source Guid will get one new target Guid
                                        // Linked source shifts will still be linked as targets, but with a new Guid
                                        if (!guids.ContainsKey(sourceShift.Link.Value))
                                        {
                                            targetLink = GetNewShiftLink();
                                            guids.Add(sourceShift.Link.Value, targetLink);
                                        }
                                        else
                                            targetLink = guids[sourceShift.Link.Value];
                                    }

                                    logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, iDTO.Action == DragShiftAction.MoveWithCycle ? TermGroup_ShiftHistoryType.DragShiftActionMoveWithCycle : TermGroup_ShiftHistoryType.DragShiftActionMove, null, iDTO.SkipXEMailOnChanges, isStandByView: iDTO.IsStandByView);

                                    // Check any intersecting onDuty shifts and include them in the move
                                    List<TimeScheduleTemplateBlock> includedOnDutyShiftsForShift = new List<TimeScheduleTemplateBlock>();
                                    if (includedOnDutyShifts.Any())
                                    {
                                        includedOnDutyShiftsForShift = includedOnDutyShifts.Where(x => x.EmployeeId == sourceShift.EmployeeId && CalendarUtility.IsDatesOverlapping(x.ActualStartTime.Value, x.ActualStopTime.Value, sourceShift.StartTime, sourceShift.StopTime, true)).ToList();
                                    }

                                    // (Drag shift from a slot to another slot)
                                    // Move source shift to new date and possibly new employee                                
                                    oDTO.Result = DragActionMoveShifts(logProperties, sourceShift.TimeScheduleTemplateBlockId, iDTO.DestinationEmployeeId, sourceShift.ActualDate.AddDays(iDTO.OffsetDays), false, iDTO.TimeScheduleScenarioHeadId, targetLink, true, sourceBreakBlocks: sourceBreakBlocks.ContainsKey(sourceShift.TimeScheduleEmployeePeriodId) ? sourceBreakBlocks[sourceShift.TimeScheduleEmployeePeriodId] : new List<TimeScheduleTemplateBlock>(), includedOnDutyShifts: includedOnDutyShiftsForShift);

                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    if (updateLinkOnTarget)
                                        oDTO.Result = UpdateLinkOnShifts(iDTO.TimeScheduleScenarioHeadId, targetLink, iDTO.DestinationEmployeeId, sourceShift.ActualDate.AddDays(iDTO.OffsetDays), sourceShift.TimeScheduleTemplateBlockId);

                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }

                                #endregion
                                break;
                            case DragShiftAction.Copy:
                            case DragShiftAction.CopyWithCycle:
                                #region Copy

                                targetLink = GetNewShiftLink();
                                foreach (var sourceShift in sourceShifts)
                                {
                                    if (!previousDate.HasValue)
                                        previousDate = sourceShift.ActualDate.AddDays(iDTO.OffsetDays);

                                    //We need a unique guid on each day
                                    if (previousDate.Value != sourceShift.ActualDate.AddDays(iDTO.OffsetDays))
                                    {
                                        targetLink = GetNewShiftLink();
                                        previousDate = sourceShift.ActualDate.AddDays(iDTO.OffsetDays);
                                    }

                                    updateLinkOnTarget = false;
                                    if (sourceShift.IsSchedule() && iDTO.LinkWithExistingShiftsIfPossible && iDTO.DestinationEmployeeId != hiddenEmployeeId)
                                    {
                                        List<string> links = GetScheduleBlockLinks(iDTO.TimeScheduleScenarioHeadId, iDTO.DestinationEmployeeId, sourceShift.ActualDate.AddDays(iDTO.OffsetDays));

                                        //set/decide targetlink and updateLinkOnTarget
                                        if (links.All(x => string.IsNullOrEmpty(x))) //handle old data                                        
                                            updateLinkOnTarget = true;
                                        else
                                        {
                                            int nbrOfUniqueLinks = links.Distinct().Count();

                                            if (nbrOfUniqueLinks == 1)
                                                targetLink = new Guid(links.FirstOrDefault());
                                        }
                                    }
                                    else
                                    {
                                        if (!sourceShift.Link.HasValue)
                                            sourceShift.Link = GetNewShiftLink();

                                        // Every unique source Guid will get one new target Guid
                                        // Linked source shifts will still be linked as targets, but with a new Guid
                                        if (!guids.ContainsKey(sourceShift.Link.Value))
                                        {
                                            targetLink = GetNewShiftLink();
                                            guids.Add(sourceShift.Link.Value, targetLink);
                                        }
                                        else
                                            targetLink = guids[sourceShift.Link.Value];
                                    }

                                    // Check any intersecting onDuty shifts and include them in the move
                                    List<TimeScheduleTemplateBlock> includedOnDutyShiftsForShift = new List<TimeScheduleTemplateBlock>();
                                    if (includedOnDutyShifts.Any())
                                    {
                                        includedOnDutyShiftsForShift = includedOnDutyShifts.Where(x => x.EmployeeId == sourceShift.EmployeeId && CalendarUtility.IsDatesOverlapping(x.ActualStartTime.Value, x.ActualStopTime.Value, sourceShift.StartTime, sourceShift.StopTime, true)).ToList();
                                    }

                                    // Copy source shift to new date and possibly new employee                                                                                             
                                    logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, iDTO.Action == DragShiftAction.CopyWithCycle ? TermGroup_ShiftHistoryType.DragShiftActionCopyWithCycle : TermGroup_ShiftHistoryType.DragShiftActionCopy, null, iDTO.SkipXEMailOnChanges, isStandByView: iDTO.IsStandByView);
                                    oDTO.Result = DragActionCopyShifts(logProperties, sourceShift.TimeScheduleTemplateBlockId, iDTO.DestinationEmployeeId, sourceShift.ActualDate.AddDays(iDTO.OffsetDays), false, iDTO.CopyTaskWithShift, iDTO.TimeScheduleScenarioHeadId, targetLink, true, includedOnDutyShiftsForShift);

                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    if (updateLinkOnTarget)
                                        oDTO.Result = UpdateLinkOnShifts(iDTO.TimeScheduleScenarioHeadId, targetLink, iDTO.DestinationEmployeeId, sourceShift.ActualDate.AddDays(iDTO.OffsetDays), sourceShift.TimeScheduleTemplateBlockId);

                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                }

                                #endregion
                                break;
                        }

                        #endregion

                        if (!iDTO.TimeScheduleScenarioHeadId.HasValue)
                        {
                            #region RestoreToSchedule

                            oDTO.Result = RestoreCurrentDaysToSchedule();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region ExtraShift

                            oDTO.Result = SetHasUnhandledShiftChanges();
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Notify

                            if (oDTO.Result.Success)
                            {
                                DoNotifyChangeOfDeviations();
                                DoInitiatePayrollWarnings();
                            }

                            SendXEMailOnDayChanged();

                            #endregion
                        }

                        if (oDTO.Result.Success)
                            oDTO.Result = Save();

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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

        private SplitTemplateTimeScheduleShiftOutputDTO TaskSplitTemplateShift()
        {
            var (iDTO, oDTO) = InitTask<SplitTemplateTimeScheduleShiftInputDTO, SplitTemplateTimeScheduleShiftOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if ((!iDTO.EmployeeId1.HasValue && !iDTO.EmployeePostId1.HasValue) || (!iDTO.EmployeeId2.HasValue && !iDTO.EmployeePostId2.HasValue))
            {
                oDTO.Result = new ActionResult(false);
                return oDTO;
            }

            if (iDTO.EmployeeId1 == 0)
                iDTO.EmployeeId1 = null;
            if (iDTO.EmployeeId2 == 0)
                iDTO.EmployeeId2 = null;
            if (iDTO.EmployeePostId1 == 0)
                iDTO.EmployeePostId1 = null;
            if (iDTO.EmployeePostId2 == 0)
                iDTO.EmployeePostId2 = null;

            TimeScheduleTemplateHead sourceTemplateHead = null;
            TimeScheduleTemplateHead target1TemplateHead = null;
            TimeScheduleTemplateHead target2TemplateHead = null;

            DateTime target1DateFrom;
            DateTime target2DateFrom;
            DateTime sourceDateFrom;

            List<TimeSchedulePlanningDayDTO> target1TemplateShifts = null;
            List<TimeSchedulePlanningDayDTO> target2TemplateShifts = null;
            List<TimeSchedulePlanningDayDTO> sourceTemplateShifts = null;

            List<int> sourceChangedDayNumbers = new List<int>();
            List<int> target1ChangedDayNumbers = new List<int>();
            List<int> target2ChangedDayNumbers = new List<int>();

            TimeSchedulePlanningDayDTO newShift = null;
            int? sourceEmployeeId = 0;
            int? sourceEmployeePostId = null;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Init

                sourceTemplateHead = GetTimeScheduleTemplateHead(iDTO.SourceTemplateHeadId);
                target1TemplateHead = GetTimeScheduleTemplateHead(iDTO.TemplateHeadId1);
                target2TemplateHead = GetTimeScheduleTemplateHead(iDTO.TemplateHeadId2);
                if (sourceTemplateHead == null || target1TemplateHead == null || target2TemplateHead == null)
                {
                    oDTO.Result = new ActionResult(false);
                    return oDTO;
                }

                #region Get target template 

                //use source shift actual date as relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                target1DateFrom = target1TemplateHead.GetCycleStartFromGivenDate(iDTO.SourceShift.ActualDate.Date);
                target1TemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, target1TemplateHead.TimeScheduleTemplateHeadId, target1DateFrom, target1DateFrom, loadTasksAndDelivery: true);

                target2DateFrom = target2TemplateHead.GetCycleStartFromGivenDate(iDTO.SourceShift.ActualDate.Date);
                target2TemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, target2TemplateHead.TimeScheduleTemplateHeadId, target2DateFrom, target2DateFrom, loadTasksAndDelivery: true);

                #endregion

                #region Get source template

                //use sourcedate as relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                sourceDateFrom = sourceTemplateHead.GetCycleStartFromGivenDate(iDTO.SourceShift.ActualDate.Date);
                sourceTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom, sourceDateFrom, loadTasksAndDelivery: true);

                #endregion

                #region Split shift

                #region Shift

                sourceEmployeeId = iDTO.SourceShift.EmployeeId != 0 ? iDTO.SourceShift.EmployeeId : (int?)null;
                sourceEmployeePostId = iDTO.SourceShift.EmployeePostId;

                // Link
                if (!iDTO.SourceShift.Link.HasValue)
                    iDTO.SourceShift.Link = GetNewShiftLink();

                // Copy DTO
                newShift = new TimeSchedulePlanningDayDTO();
                EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(newShift, iDTO.SourceShift);
                newShift.TimeScheduleTemplateBlockId = 0;

                // Change properties
                iDTO.SourceShift.StopTime = iDTO.SplitTime;
                iDTO.SourceShift.EmployeeId = iDTO.EmployeeId1.ToInt();
                iDTO.SourceShift.EmployeePostId = iDTO.EmployeePostId1;

                newShift.StartTime = iDTO.SplitTime;
                newShift.EmployeeId = iDTO.EmployeeId2.ToInt();
                newShift.EmployeePostId = iDTO.EmployeePostId2;
                if (!iDTO.KeepShiftsTogether || iDTO.EmployeeId1 != iDTO.EmployeeId2 || iDTO.EmployeePostId1 != iDTO.EmployeePostId2)
                    newShift.Link = Guid.NewGuid();

                #endregion

                #region Tasks

                // Convert split time to 1900-01-01
                DateTime splitTime = iDTO.SplitTime;
                int offsetDays = (splitTime < iDTO.SourceShift.StartTime ? 1 : 0);
                splitTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, splitTime);
                splitTime.AddDays(offsetDays);

                List<TimeScheduleTemplateBlockTaskDTO> allTasks = GetTimeScheduleTemplateBlockTasks(iDTO.SourceShift.TimeScheduleTemplateBlockId).ToDTOs().ToList();
                //Tasks that will get a new parent
                List<TimeScheduleTemplateBlockTaskDTO> tasksToMove = allTasks.GetTaskThatStartsAfterGivenTime(splitTime);
                List<TimeScheduleTemplateBlockTaskDTO> splittedTasks = new List<TimeScheduleTemplateBlockTaskDTO>();

                foreach (var task in allTasks)
                {
                    if (task.IsOverlapped(splitTime))
                    {
                        #region Split task                                

                        // Copy DTO
                        TimeScheduleTemplateBlockTaskDTO taskClone = new TimeScheduleTemplateBlockTaskDTO();
                        EntityUtil.CopyDTO<TimeScheduleTemplateBlockTaskDTO>(taskClone, task);

                        //Change properties on original task
                        task.StopTime = splitTime;
                        splittedTasks.Add(task);

                        //Change properties on new task
                        taskClone.TimeScheduleTemplateBlockTaskId = 0;
                        taskClone.TimeScheduleTemplateBlockId = null;
                        taskClone.StartTime = splitTime;
                        tasksToMove.Add(taskClone);

                        #endregion

                    }
                }

                iDTO.SourceShift.Tasks = splittedTasks;
                newShift.Tasks = tasksToMove;

                #endregion

                #endregion

                #endregion

            }
            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        //Move source shift
                        oDTO.Result = ApplyMoveTemplateShift(ref sourceTemplateShifts, ref target1TemplateShifts, iDTO.SourceShift, target1TemplateHead, iDTO.EmployeeId1, iDTO.EmployeePostId1, iDTO.SourceShift.ActualDate, target1DateFrom, iDTO.SourceShift.Link ?? Guid.NewGuid(), ref sourceChangedDayNumbers, ref target1ChangedDayNumbers);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (iDTO.EmployeeId1 != iDTO.EmployeeId2 || iDTO.EmployeePostId1 != iDTO.EmployeePostId2)
                        {
                            if (sourceEmployeeId == iDTO.EmployeeId2 || sourceEmployeePostId == iDTO.EmployeePostId2)
                            {
                                //remove source shift from target2 if source and target2 employee are the SAME and target employee 1 and 2 are NOT THE SAME
                                target2TemplateShifts = target2TemplateShifts.Where(x => x.TimeScheduleTemplateBlockId != iDTO.SourceShift.TimeScheduleTemplateBlockId).ToList();
                                target2ChangedDayNumbers.Add(iDTO.SourceShift.DayNumber);
                            }

                            //Copy new shift
                            oDTO.Result = ApplyCopyTemplateShift(ref target2TemplateShifts, newShift, target2TemplateHead, iDTO.EmployeeId2, iDTO.EmployeePostId2, iDTO.SourceShift.ActualDate, target2DateFrom, newShift.Link ?? Guid.NewGuid(), ref target2ChangedDayNumbers);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }
                        else
                        {
                            //Copy new shift
                            oDTO.Result = ApplyCopyTemplateShift(ref target1TemplateShifts, newShift, target1TemplateHead, iDTO.EmployeeId1, iDTO.EmployeePostId1, iDTO.SourceShift.ActualDate, target1DateFrom, newShift.Link ?? Guid.NewGuid(), ref target1ChangedDayNumbers);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        #endregion

                        #region Save                      

                        oDTO.Result = UpdateTimeScheduleTemplate(target1TemplateShifts, target1ChangedDayNumbers, target1TemplateHead.TimeScheduleTemplateHeadId, target1DateFrom);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (sourceEmployeeId != iDTO.EmployeeId1 || sourceEmployeePostId != iDTO.EmployeePostId1)
                        {
                            oDTO.Result = UpdateTimeScheduleTemplate(sourceTemplateShifts, sourceChangedDayNumbers, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        if (iDTO.EmployeeId1 != iDTO.EmployeeId2 || iDTO.EmployeePostId1 != iDTO.EmployeePostId2)
                        {
                            oDTO.Result = UpdateTimeScheduleTemplate(target2TemplateShifts, target2ChangedDayNumbers, target2TemplateHead.TimeScheduleTemplateHeadId, target2DateFrom);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        TryCommit(oDTO);

                        #endregion
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

        private DragTemplateTimeScheduleShiftOutputDTO TaskDragTemplateTimeScheduleShift()
        {
            var (iDTO, oDTO) = InitTask<DragTemplateTimeScheduleShiftInputDTO, DragTemplateTimeScheduleShiftOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            Guid targetLink;

            TimeScheduleTemplateHead sourceTemplateHead = null;
            TimeScheduleTemplateHead targetTemplateHead = null;
            List<TimeSchedulePlanningDayDTO> sourceTemplateShifts = new List<TimeSchedulePlanningDayDTO>();
            List<TimeSchedulePlanningDayDTO> targetTemplateShifts = new List<TimeSchedulePlanningDayDTO>();
            List<int> sourceChangedDayNumbers = new List<int>();
            List<int> targetChangedDayNumbers = new List<int>();
            DateTime targetDateFrom;
            DateTime sourceDateFrom;
            int offsetDays = 0;
            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);
                #region Prereq

                // Get User
                User user = GetUserFromCache();
                if (user == null)
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_UserNotFound);
                    return oDTO;
                }

                sourceTemplateHead = GetTimeScheduleTemplateHead(iDTO.SourceTemplateHeadId);
                targetTemplateHead = GetTimeScheduleTemplateHead(iDTO.TargetTemplateHeadId);
                if (sourceTemplateHead == null || targetTemplateHead == null)
                {
                    oDTO.Result = new ActionResult(false);
                    return oDTO;
                }

                #region Get source template

                // Need to check if source shift belongs to another day (over midnight)
                TimeScheduleTemplateBlock sourceShift = (entities.TimeScheduleTemplateBlock.FirstOrDefault(b => b.TimeScheduleTemplateBlockId == iDTO.SourceShiftId));
                if (sourceShift != null)
                {
                    if (sourceShift.BelongsToPreviousDay)
                        offsetDays = -1;
                    else if (sourceShift.BelongsToNextDay)
                        offsetDays = 1;

                    if (sourceShift.IsOnDuty())
                        iDTO.KeepTargetShiftsTogether = false;
                }

                //use sourcedate a relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                sourceDateFrom = sourceTemplateHead.GetCycleStartFromGivenDate(iDTO.SourceDate.Date.AddDays(offsetDays));
                sourceTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom, sourceDateFrom.AddDays(sourceTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                #endregion

                #region Get target template 

                //use targetdate a relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                targetDateFrom = targetTemplateHead.GetCycleStartFromGivenDate(iDTO.TargetStart.Date.AddDays(offsetDays));
                targetTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, targetTemplateHead.TimeScheduleTemplateHeadId, targetDateFrom, targetDateFrom.AddDays(targetTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                #endregion

                #endregion
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        switch (iDTO.Action)
                        {
                            case DragShiftAction.Copy:
                            case DragShiftAction.Move:
                                // (Drag shift from a slot to another slot)
                                #region Copy/Move

                                if (iDTO.TargetEmployeeId.HasValue && !IsHiddenEmployeeFromCache(iDTO.TargetEmployeeId.Value) && iDTO.KeepTargetShiftsTogether && iDTO.TargetLink.HasValue)
                                    targetLink = iDTO.TargetLink.Value;
                                else
                                    targetLink = GetNewShiftLink();

                                //Move source shift to new date and possibly new employee
                                oDTO.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, iDTO.SourceShiftId, iDTO.KeepSourceShiftsTogether, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, iDTO.TargetStart.Date, targetTemplateHead, targetDateFrom, targetLink, iDTO.Action == DragShiftAction.Copy, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion
                                break;
                            case DragShiftAction.SwapEmployee:
                                // (Drag shift to slot with existing shift)
                                #region SwapEmployee
                                // Swap employees between source and target shifts
                                oDTO.Result = SwapEmployeesOnTemplateShifts(ref sourceTemplateShifts, ref targetTemplateShifts, iDTO.SourceShiftId, iDTO.KeepSourceShiftsTogether, sourceTemplateHead, iDTO.TargetShiftId, iDTO.KeepTargetShiftsTogether, iDTO.SourceDate, iDTO.TargetStart.Date, sourceDateFrom, targetDateFrom, targetTemplateHead, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                                #endregion
                                break;
                        }

                        #endregion

                        #region Save                      

                        oDTO.Result = UpdateTimeScheduleTemplate(targetTemplateShifts, targetChangedDayNumbers, targetTemplateHead.TimeScheduleTemplateHeadId, targetDateFrom);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (iDTO.Action != DragShiftAction.Copy)
                        {
                            oDTO.Result = UpdateTimeScheduleTemplate(sourceTemplateShifts, sourceChangedDayNumbers, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        TryCommit(oDTO);

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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

        private DragTimeScheduleShiftMultipelOutputDTO TaskDragTemplateTimeScheduleShiftMultipel()
        {
            var (iDTO, oDTO) = InitTask<DragTemplateTimeScheduleShiftMultipelInputDTO, DragTimeScheduleShiftMultipelOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            Guid targetLink;
            Dictionary<Guid, Guid> guids = new Dictionary<Guid, Guid>();
            TimeScheduleTemplateHead sourceTemplateHead = null;
            TimeScheduleTemplateHead targetTemplateHead = null;
            List<TimeSchedulePlanningDayDTO> sourceTemplateShifts = new List<TimeSchedulePlanningDayDTO>();
            List<TimeSchedulePlanningDayDTO> targetTemplateShifts = new List<TimeSchedulePlanningDayDTO>();
            List<int> sourceChangedDayNumbers = new List<int>();
            List<int> targetChangedDayNumbers = new List<int>();
            DateTime? previousDate = null;
            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                // Get User
                User user = GetUserFromCache();
                if (user == null)
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_UserNotFound);
                    return oDTO;
                }

                sourceTemplateHead = GetTimeScheduleTemplateHead(iDTO.SourceTemplateHeadId);
                targetTemplateHead = GetTimeScheduleTemplateHead(iDTO.TargetTemplateHeadId);
                if (sourceTemplateHead == null || targetTemplateHead == null)
                {
                    oDTO.Result = new ActionResult(false);
                    return oDTO;
                }

                // Never link with existing shifts if target is hidden employee
                if (iDTO.LinkWithExistingShiftsIfPossible && targetTemplateHead.EmployeeId == GetHiddenEmployeeIdFromCache())
                    iDTO.LinkWithExistingShiftsIfPossible = false;

                #region Get target template 

                //use targetdate as relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                targetTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, targetTemplateHead.TimeScheduleTemplateHeadId, iDTO.FirstTargetDate.Date, iDTO.FirstTargetDate.Date.AddDays(targetTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                #endregion

                #region Get source template

                //use sourcedate as relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                sourceTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, sourceTemplateHead.TimeScheduleTemplateHeadId, iDTO.FirstSourceDate.Date, iDTO.FirstSourceDate.Date.AddDays(sourceTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                #endregion

                #endregion
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        // Get User
                        User user = GetUserFromCache();
                        if (user == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_UserNotFound);
                            return oDTO;
                        }

                        #endregion

                        #region Perform                        

                        switch (iDTO.Action)
                        {
                            case DragShiftAction.Move:

                                targetLink = GetNewShiftLink();
                                foreach (var sourceShiftId in iDTO.SourceShiftIds)
                                {
                                    var sourceShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
                                    if (sourceShift == null)
                                        continue;

                                    if (!previousDate.HasValue)
                                        previousDate = sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays);

                                    //We need a unique guid on each day
                                    if (previousDate.Value != sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays))
                                    {
                                        targetLink = GetNewShiftLink();
                                        previousDate = sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays);
                                    }

                                    if (iDTO.LinkWithExistingShiftsIfPossible)
                                    {
                                        List<string> links = targetTemplateShifts.Where(x => x.Link.HasValue && x.StartTime.Date == sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays) && x.StartTime != x.StopTime).Select(x => x.Link.Value.ToString()).ToList();
                                        int nbrOfUniqueLinks = links.Distinct().Count();
                                        if (nbrOfUniqueLinks == 1)
                                            targetLink = new Guid(links.FirstOrDefault());
                                    }
                                    else
                                    {
                                        if (!sourceShift.Link.HasValue)
                                            sourceShift.Link = GetNewShiftLink();

                                        // Every unique source Guid will get one new target Guid
                                        // Linked source shifts will still be linked as targets, but with a new Guid
                                        if (!guids.ContainsKey(sourceShift.Link.Value))
                                        {
                                            targetLink = GetNewShiftLink();
                                            guids.Add(sourceShift.Link.Value, targetLink);
                                        }
                                        else
                                            targetLink = guids[sourceShift.Link.Value];
                                    }

                                    // (Drag shift from a slot to another slot)
                                    #region Move

                                    // Move source shift to new date and possibly new employee             
                                    DateTime targetDate = sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays);
                                    oDTO.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, sourceShiftId, false, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, targetDate, targetTemplateHead, targetTemplateHead.GetCycleStartFromGivenDate(targetDate), targetLink, false, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                                #endregion

                                break;
                            case DragShiftAction.Copy:
                                targetLink = GetNewShiftLink();
                                foreach (var sourceShiftId in iDTO.SourceShiftIds)
                                {
                                    var sourceShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
                                    if (sourceShift == null)
                                        continue;

                                    if (!previousDate.HasValue)
                                        previousDate = sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays);

                                    //We need a unique guid on each day
                                    if (previousDate.Value != sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays))
                                    {
                                        targetLink = GetNewShiftLink();
                                        previousDate = sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays);
                                    }

                                    if (iDTO.LinkWithExistingShiftsIfPossible)
                                    {
                                        List<string> links = targetTemplateShifts.Where(x => x.Link.HasValue && x.StartTime.Date == sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays) && x.StartTime != x.StopTime).Select(x => x.Link.Value.ToString()).ToList();
                                        int nbrOfUniqueLinks = links.Distinct().Count();
                                        if (nbrOfUniqueLinks == 1)
                                            targetLink = new Guid(links.FirstOrDefault());
                                    }
                                    else
                                    {
                                        if (!sourceShift.Link.HasValue)
                                            sourceShift.Link = GetNewShiftLink();

                                        // Every unique source Guid will get one new target Guid
                                        // Linked source shifts will still be linked as targets, but with a new Guid
                                        if (!guids.ContainsKey(sourceShift.Link.Value))
                                        {
                                            targetLink = GetNewShiftLink();
                                            guids.Add(sourceShift.Link.Value, targetLink);
                                        }
                                        else
                                            targetLink = guids[sourceShift.Link.Value];
                                    }

                                    #region Copy

                                    DateTime targetDate = sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays);
                                    // Copy source shift to new date and possibly new employee                                                                                                                                                                                                                   
                                    oDTO.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, sourceShiftId, false, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, targetDate, targetTemplateHead, targetTemplateHead.GetCycleStartFromGivenDate(targetDate), targetLink, true, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                }
                                #endregion
                                break;
                        }

                        #endregion

                        #region Save

                        if (iDTO.Action != DragShiftAction.Copy)
                        {
                            oDTO.Result = UpdateTimeScheduleTemplate(sourceTemplateShifts, sourceChangedDayNumbers, sourceTemplateHead.TimeScheduleTemplateHeadId, iDTO.FirstSourceDate.Date);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        oDTO.Result = UpdateTimeScheduleTemplate(targetTemplateShifts, targetChangedDayNumbers, targetTemplateHead.TimeScheduleTemplateHeadId, iDTO.FirstTargetDate.Date);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        TryCommit(oDTO);

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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

        private RemoveEmployeeFromShiftQueueOutputDTO TaskRemoveEmployeeFromShiftQueue()
        {
            var (iDTO, oDTO) = InitTask<RemoveEmployeeFromShiftQueueInputDTO, RemoveEmployeeFromShiftQueueOutputDTO>();
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

                        oDTO.Result = RemoveEmployeeFromShiftQueue(iDTO.Type, iDTO.TimeScheduleTemplateBlockId, iDTO.EmployeeId, true);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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

        private AssignTaskToEmployeeOutputDTO TaskAssignTaskToEmployee()
        {
            var (iDTO, oDTO) = InitTask<AssignTaskToEmployeeInputDTO, AssignTaskToEmployeeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Perform

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = AssignTaskToEmployee(iDTO.EmployeeId, iDTO.Date, iDTO.TaskDTOs, iDTO.SkipXEMailOnShiftChanges);

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

                #endregion
            }

            return oDTO;
        }

        private AssignTemplatShiftTaskOutputDTO TaskAssignTemplateShiftTask()
        {
            var (iDTO, oDTO) = InitTask<AssignTemplateShiftTaskInputDTO, AssignTemplatShiftTaskOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Perform

                oDTO.Shifts = AssignTemplateShiftTask(iDTO.Tasks, iDTO.Date, iDTO.TimeScheduleTemplateHeadId);

                #endregion
            }

            return oDTO;
        }

        private PerformRestoreAbsenceRequestedShiftsOutputDTO TaskPerformRestoreAbsenceRequestedShifts()
        {
            var (iDTO, oDTO) = InitTask<PerformRestoreAbsenceRequestedShiftsInputDTO, PerformRestoreAbsenceRequestedShiftsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            EmployeeRequest absenceRequest = null;
            List<ShiftHistoryDTO> shifts = new List<ShiftHistoryDTO>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        List<ShiftHistoryDTO> allShifts = TimeScheduleManager.GetAbsenceRequestHistory(entities, actorCompanyId, iDTO.EmployeeRequestId, false);

                        foreach (var item in allShifts)
                        {
                            if (!shifts.Any(x => x.TimeScheduleTemplateBlockId == item.TimeScheduleTemplateBlockId))
                                shifts.Add(item);
                        }

                        int hiddenEmployeeId = 0;

                        #region Prereq

                        hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                        absenceRequest = GetEmployeeRequest(iDTO.EmployeeRequestId);

                        if (hiddenEmployeeId == 0 || absenceRequest == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeRequest");
                            return oDTO;
                        }

                        #endregion

                        #region Perform

                        Guid batchId = GetNewBatchLink();

                        List<int> userAccounts = this.GetAccountsFromHierarchyByUser(null, null).Select(x => x.AccountId).ToList();
                        foreach (var shift in shifts)
                        {
                            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(shift.TimeScheduleTemplateBlockId);
                            if (scheduleBlock == null)
                                continue;

                            if (scheduleBlock.AccountId.HasValue && !userAccounts.Contains(scheduleBlock.AccountId.Value))
                                continue;

                            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(scheduleBlock.EmployeeId.Value, scheduleBlock.Date.Value, false);
                            if (timeBlockDate == null)
                                continue;

                            var logProperties = new ShiftHistoryLogCallStackProperties(batchId, scheduleBlock.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanningRestored, absenceRequest.EmployeeRequestId, false)
                            {
                                NewShiftId = scheduleBlock.TimeScheduleTemplateBlockId
                            };

                            if (!shift.EmployeeChanged)
                            {
                                oDTO.Result = RestoreDayToSchedule(timeBlockDate, clearScheduledAbsence: true, logProperties: logProperties);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                        }

                        oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Restore, absenceRequest.EmployeeId);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        //Preserve InfoMessage
                        string infoMessage = oDTO.Result.InfoMessage;

                        if (iDTO.SetRequestAsPending)
                        {
                            absenceRequest.Status = (int)TermGroup_EmployeeRequestStatus.RequestPending;
                            UpdateEmployeeRequest(absenceRequest, false);
                        }
                        else
                        {
                            absenceRequest.Status = (int)TermGroup_EmployeeRequestStatus.Restored;
                        }

                        #endregion

                        #region RestoreToSchedule

                        oDTO.Result = RestoreCurrentDaysToSchedule();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region ExtraShift

                        oDTO.Result = SetHasUnhandledShiftChanges();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Notify

                        if (oDTO.Result.Success)
                        {
                            DoNotifyChangeOfDeviations();
                            DoInitiatePayrollWarnings();
                        }

                        SendXEMailOnDayChanged();

                        #endregion

                        if (oDTO.Result.Success)
                            oDTO.Result = Save();

                        TryCommit(oDTO);

                        //Set InfoMessage
                        oDTO.Result.InfoMessage = infoMessage;
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

        #endregion

        #endregion

        #region Schedule - template

        private ActionResult SaveTimeScheduleTemplate(List<TimeScheduleTemplateBlockDTO> inputItems, TimeScheduleTemplateHead templateHead, DateTime? currentDate = null)
        {
            return SaveTimeScheduleTemplate(inputItems, templateHead.TimeScheduleTemplateHeadId, templateHead.Name, templateHead.Description, templateHead.State, templateHead.StartDate, templateHead.StopDate, currentDate, templateHead.FirstMondayOfCycle, templateHead.NoOfDays, templateHead.SimpleSchedule, templateHead.IsPersonalTemplate, templateHead.StartOnFirstDayOfWeek, templateHead.FlexForceSchedule, templateHead.Locked, templateHead.EmployeeId, employeePostId: null);
        }

        private ActionResult SaveTimeScheduleTemplate(List<TimeScheduleTemplateBlockDTO> inputItems, int timeScheduleTemplateHeadId, string name, string description, int state, DateTime? startDate, DateTime? stopDate, DateTime? currentDate, DateTime? firstMondayOfCycle, int noOfDays, bool simpleSchedule, bool isPersonalTemplate, bool startOnFirstDayOfWeek, bool flexForceSchedule, bool locked, int? employeeId, int? employeePostId, bool useAccountingFromSourceSchedule = true)
        {
            ActionResult result;

            #region Init

            if (inputItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlockDTO");

            #endregion

            #region Prereq

            Employee employee = null;

            //Check if personal exists
            if (isPersonalTemplate)
            {
                #region Personal template

                if (!employeeId.HasValue || !startDate.HasValue)
                    return new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeStartDateMandatory, GetText(3395, "Grundschemat måste ha startdatum"));

                if (startOnFirstDayOfWeek && startDate.HasValue && startDate.Value.DayOfWeek != DayOfWeek.Monday)
                    return new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeStartsFirstDayOnWeekInvalid, GetText(12160, "Grundschemat måste starta på en måndag"));

                employee = GetEmployeeFromCache(employeeId.Value);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

                result = ValidatePersonalTemplates(timeScheduleTemplateHeadId, employeeId.Value, startDate.Value, stopDate);
                if (!result.Success)
                    return result;

                foreach (var inputItem in inputItems)
                {
                    inputItem.EmployeeId = employeeId;
                }

                #endregion
            }

            int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
            if (defaultTimeCodeId > 0)
            {
                //Make sure that each day has atleast one TimeScheduleTemplatePeriod
                for (int i = 1; i <= noOfDays; i++)
                {
                    // Check if period exists
                    TimeScheduleTemplateBlockDTO templateBlockItemInput = inputItems.FirstOrDefault(p => p.DayNumber == i);
                    if (templateBlockItemInput == null)
                    {
                        //Add empty block
                        templateBlockItemInput = new TimeScheduleTemplateBlockDTO()
                        {
                            DayNumber = i,
                            TimeCodeId = defaultTimeCodeId,
                            StartTime = CalendarUtility.DATETIME_DEFAULT,
                            StopTime = CalendarUtility.DATETIME_DEFAULT
                        };

                        if (employeeId != 0)
                            templateBlockItemInput.EmployeeId = employeeId;

                        inputItems.Add(templateBlockItemInput);
                    }
                }
            }

            // Employee
            if (employeeId == 0)
                employeeId = null;

            #endregion

            #region TimeScheduleTemplateHead

            // Find reason for noOfDays being 0?
            if (noOfDays == 0)
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame[] stackFrames = stackTrace.GetFrames();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("TimeScheduleTemplateHead.NoOfDays is zero");
                sb.AppendLine("StackTrace:");
                foreach (StackFrame stackFrame in stackFrames)
                {
                    sb.AppendLine(stackFrame.GetMethod().Name);
                }
                LogError(sb.ToString());

                return new ActionResult(false, 44000, "Grundschemat är noll veckor långt, ändra och försök igen");
            }

            // Get existing template head
            TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHeadWithPeriodsAndActiveBlocks(timeScheduleTemplateHeadId, false, true);
            DateTime originalStart = templateHead?.StartDate ?? (startDate ?? DateTime.Today);
            if (templateHead == null)
            {
                #region Add

                templateHead = new TimeScheduleTemplateHead();
                SetCreatedProperties(templateHead);
                entities.TimeScheduleTemplateHead.AddObject(templateHead);

                #endregion
            }
            else
            {
                #region Update

                if (templateHead.NoOfDays != noOfDays && IsTemplateHeadUsed(templateHead.TimeScheduleTemplateHeadId))
                    return new ActionResult((int)ActionResultSave.TimeScheduleTemplateExistsCannotChangePeriods, GetText(3393, "Grundschemat används redan så antal veckor kan inte ändras"));

                SetModifiedProperties(templateHead);

                #endregion
            }

            #region Common

            templateHead.ActorCompanyId = actorCompanyId;
            templateHead.EmployeeId = employeeId;
            templateHead.EmployeePostId = employeePostId;

            templateHead.Name = name;
            templateHead.Description = description;
            templateHead.NoOfDays = noOfDays;

            templateHead.StartDate = startDate.HasValue ? CalendarUtility.GetBeginningOfDay(startDate) : (DateTime?)null;
            templateHead.StopDate = stopDate.HasValue ? CalendarUtility.GetBeginningOfDay(stopDate) : (DateTime?)null;
            templateHead.FirstMondayOfCycle = firstMondayOfCycle.HasValue ? CalendarUtility.GetBeginningOfDay(firstMondayOfCycle) : (DateTime?)null;
            if (!templateHead.FirstMondayOfCycle.HasValue && templateHead.StartDate.HasValue)
                templateHead.FirstMondayOfCycle = CalendarUtility.GetBeginningOfWeek(templateHead.StartDate);

            templateHead.StartOnFirstDayOfWeek = startOnFirstDayOfWeek;
            templateHead.SimpleSchedule = simpleSchedule;
            templateHead.FlexForceSchedule = flexForceSchedule;
            templateHead.Locked = locked;
            templateHead.State = state;

            #endregion

            #endregion

            #region TimeScheduleTemplateBlock

            // Collection to hold updated blocks, used to set the rest of the blocks as deleted
            List<TimeScheduleTemplateBlock> savedTemplateBlocks = new List<TimeScheduleTemplateBlock>();
            Dictionary<int, List<int>> existingBreakNbrsDict = new Dictionary<int, List<int>>();

            foreach (TimeScheduleTemplateBlockDTO item in inputItems.OrderBy(i => i.DayNumber).ToList())
            {
                if (item.DayNumber > templateHead.NoOfDays)
                    break;

                #region TimeScheduleTemplatePeriod

                TimeScheduleTemplatePeriod templatePeriod = null;

                //Check if period exists and still has same DayNumber
                if (item.TimeScheduleTemplatePeriodId > 0)
                    templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.TimeScheduleTemplatePeriodId == item.TimeScheduleTemplatePeriodId && i.DayNumber == item.DayNumber);

                //Check if period exists with same DayNumber
                if (templatePeriod == null)
                    templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.DayNumber == item.DayNumber);

                if (templatePeriod == null)
                {
                    #region Add

                    templatePeriod = new TimeScheduleTemplatePeriod()
                    {
                        DayNumber = item.DayNumber,
                    };
                    SetCreatedProperties(templatePeriod);

                    #endregion
                }
                else
                {
                    #region Update

                    // Update TimeScheduleTemplatePeriod
                    templatePeriod.State = (int)SoeEntityState.Active;

                    #endregion
                }

                #endregion

                #region TimeScheduleTemplateBlock

                // Not allowed to have an absence TimeCode in template
                TimeCode timeCode = GetTimeCodeFromCache(item.TimeCodeId);
                if (timeCode is TimeCodeAbsense)
                    item.TimeCodeId = defaultTimeCodeId;

                var existingBreakNbrs = new List<int>();
                if (existingBreakNbrsDict.ContainsKey(item.DayNumber))
                    existingBreakNbrs = existingBreakNbrsDict[item.DayNumber];

                //Set data and breaks
                result = SetTimeScheduleTemplateBlockItemData(item, templatePeriod, employee, existingBreakNbrs, useAccountingFromSourceSchedule, startDate);
                if (!result.Success)
                    return result;

                //Track which breaks that are created for daynumber
                if (!existingBreakNbrs.Contains(1) && item.Break1TimeCodeId > 0)
                    existingBreakNbrs.Add(1);
                if (!existingBreakNbrs.Contains(2) && item.Break2TimeCodeId > 0)
                    existingBreakNbrs.Add(2);
                if (!existingBreakNbrs.Contains(3) && item.Break3TimeCodeId > 0)
                    existingBreakNbrs.Add(3);
                if (!existingBreakNbrs.Contains(4) && item.Break4TimeCodeId > 0)
                    existingBreakNbrs.Add(4);

                if (existingBreakNbrsDict.ContainsKey(item.DayNumber))
                    existingBreakNbrsDict[item.DayNumber] = existingBreakNbrs;
                else
                    existingBreakNbrsDict.Add(item.DayNumber, existingBreakNbrs);

                if (result.Value is List<TimeScheduleTemplateBlock> scheduleBlocks)
                    savedTemplateBlocks.AddRange(scheduleBlocks);

                //Only add TimeScheduleTemplatePeriod if block is not null
                templateHead.TimeScheduleTemplatePeriod.Add(templatePeriod);

                #endregion
            }

            #endregion

            #region Delete

            // Delete all existing block that does not exist in item collection
            if (timeScheduleTemplateHeadId != 0)
            {
                List<TimeScheduleTemplatePeriod> originalTemplatePeriods = GetTimeScheduleTemplatePeriods(timeScheduleTemplateHeadId);
                foreach (TimeScheduleTemplatePeriod originalTemplatePeriod in originalTemplatePeriods)
                {
                    #region TimeScheduleTemplatePeriod

                    if (!originalTemplatePeriod.TimeScheduleTemplateBlock.IsLoaded)
                        originalTemplatePeriod.TimeScheduleTemplateBlock.Load();

                    List<TimeScheduleTemplateBlock> originalTemplateBlocks = originalTemplatePeriod.TimeScheduleTemplateBlock.Where(i => i.Date == null && i.State == (int)SoeEntityState.Active).ToList();
                    foreach (TimeScheduleTemplateBlock originalTemplateBlock in originalTemplateBlocks)
                    {
                        #region TimeScheduleTemplateBlock

                        //Block is saved, i.e. not deleted
                        bool isUpdated = savedTemplateBlocks.Any(i => i.TimeScheduleTemplateBlockId == originalTemplateBlock.TimeScheduleTemplateBlockId);

                        //Block's period has a DayNumber in the range of NoOfDays
                        bool hasValidDayNumber = originalTemplateBlock.TimeScheduleTemplatePeriod.DayNumber <= templateHead.NoOfDays;

                        //Decide if block and it's period should be deleted
                        if (!isUpdated || !hasValidDayNumber)
                        {
                            //Delete TimeScheduleTemplateBlock
                            ChangeEntityState(originalTemplateBlock, SoeEntityState.Deleted);
                        }

                        #endregion
                    }

                    //Decide TimeScheduleTemplatePeriod state
                    bool hasActiveBlocks = originalTemplateBlocks.Any(i => i.State == (int)SoeEntityState.Active);
                    originalTemplatePeriod.State = (int)(hasActiveBlocks ? SoeEntityState.Active : SoeEntityState.Deleted);

                    #endregion
                }
            }

            #endregion

            #region Save

            result = Save();

            if (result.Success)
                result.IntegerValue = templateHead.TimeScheduleTemplateHeadId;

            #endregion

            #region Calculate ScheduledTimeSummary

            if (isPersonalTemplate)
            {
                if (currentDate.HasValue && originalStart.Year < currentDate.Value.Year)
                    originalStart = CalendarUtility.GetFirstDateOfYear(currentDate.Value);

                DateTime endDate = CalendarUtility.GetLastDateOfYear(originalStart);
                if (base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, endDate))
                    TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employeeId.Value, originalStart, endDate);
            }

            #endregion

            return result;
        }

        private ActionResult UpdateTimeScheduleTemplate(List<TimeSchedulePlanningDayDTO> templateShifts, List<int> changedDayNumbers, int timeScheduleTemplateHeadId, DateTime currentDate)
        {
            if (changedDayNumbers.IsNullOrEmpty())
                return new ActionResult();

            int dayNumberFrom = changedDayNumbers.OrderBy(x => x).FirstOrDefault();
            int dayNumberTo = changedDayNumbers.OrderBy(x => x).LastOrDefault();
            List<TimeSchedulePlanningDayDTO> shiftsToSave = templateShifts.Where(x => x.DayNumber >= dayNumberFrom && x.DayNumber <= dayNumberTo).ToList();

            var items = ConvertToTemplateBlockItems(shiftsToSave);
            ApplyAccountingFromShiftType(items);
            return UpdateTimeScheduleTemplate(items, timeScheduleTemplateHeadId, dayNumberFrom, dayNumberTo, currentDate);
        }

        private ActionResult UpdateTimeScheduleTemplate(List<TimeScheduleTemplateBlockDTO> inputItems, int timeScheduleTemplateHeadId, int dayNumberFrom, int dayNumberTo, DateTime currentDate)
        {
            ActionResult result;

            #region Init

            if (inputItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlockDTO");

            #endregion

            #region Prereq

            TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHeadWithPeriodsAndActiveBlocks(timeScheduleTemplateHeadId, false, true);
            if (templateHead == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");

            Employee employee = templateHead.EmployeeId.HasValue ? GetEmployeeFromCache(templateHead.EmployeeId.Value) : null;

            #endregion

            #region TimeScheduleTemplateBlock

            // Collection to hold updated blocks, used to set the rest of the blocks as deleted
            List<TimeScheduleTemplateBlock> savedTemplateBlocks = new List<TimeScheduleTemplateBlock>();

            if (employee != null && employee.Hidden)
            {
                #region Hidden employee

                // For hidden employee, we also need to group by link, since you can create groups of shifts on the same day
                Dictionary<int, Dictionary<Guid, List<int>>> existingBreakNbrsDict = new Dictionary<int, Dictionary<Guid, List<int>>>();
                var itemsByDayNumberGrouping = inputItems.GroupBy(i => i.DayNumber).ToList();
                foreach (var itemsByDayNumber in itemsByDayNumberGrouping)
                {
                    var itemsByLinkGrouping = itemsByDayNumber.GroupBy(i => i.Link).ToList();
                    foreach (var itemsByLink in itemsByLinkGrouping)
                    {
                        foreach (TimeScheduleTemplateBlockDTO item in itemsByLink)
                        {
                            if (item.DayNumber > templateHead.NoOfDays)
                                break;

                            #region TimeScheduleTemplatePeriod

                            TimeScheduleTemplatePeriod templatePeriod = null;

                            //Check if period exists and still has same DayNumber
                            if (item.TimeScheduleTemplatePeriodId > 0)
                                templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.TimeScheduleTemplatePeriodId == item.TimeScheduleTemplatePeriodId && i.DayNumber == item.DayNumber);

                            //Check if period exists with same DayNumber
                            if (templatePeriod == null)
                                templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.DayNumber == item.DayNumber);

                            if (templatePeriod == null)
                            {
                                #region Add

                                templatePeriod = new TimeScheduleTemplatePeriod()
                                {
                                    DayNumber = item.DayNumber,
                                };
                                SetCreatedProperties(templatePeriod);

                                #endregion
                            }
                            else
                            {
                                #region Update

                                // Update TimeScheduleTemplatePeriod
                                templatePeriod.State = (int)SoeEntityState.Active;

                                #endregion
                            }

                            #endregion

                            #region TimeScheduleTemplateBlock

                            List<int> existingBreakNbrs = new List<int>();
                            if (existingBreakNbrsDict.ContainsKey(item.DayNumber))
                            {
                                Dictionary<Guid, List<int>> dayNumberLinks = existingBreakNbrsDict[item.DayNumber];
                                if (dayNumberLinks.ContainsKey(item.Link.Value))
                                    existingBreakNbrs = dayNumberLinks[item.Link.Value];
                            }

                            //Set data and breaks
                            result = SetTimeScheduleTemplateBlockItemData(item, templatePeriod, employee, existingBreakNbrs);
                            if (!result.Success)
                                return result;

                            //Track which breaks that are created for daynumber
                            if (!existingBreakNbrs.Contains(1) && item.Break1TimeCodeId > 0)
                                existingBreakNbrs.Add(1);
                            if (!existingBreakNbrs.Contains(2) && item.Break2TimeCodeId > 0)
                                existingBreakNbrs.Add(2);
                            if (!existingBreakNbrs.Contains(3) && item.Break3TimeCodeId > 0)
                                existingBreakNbrs.Add(3);
                            if (!existingBreakNbrs.Contains(4) && item.Break4TimeCodeId > 0)
                                existingBreakNbrs.Add(4);

                            if (existingBreakNbrsDict.ContainsKey(item.DayNumber))
                            {
                                Dictionary<Guid, List<int>> dayNumberLinks = existingBreakNbrsDict[item.DayNumber];
                                if (dayNumberLinks.ContainsKey(item.Link.Value))
                                    dayNumberLinks[item.Link.Value] = existingBreakNbrs;
                                else
                                    dayNumberLinks.Add(item.Link.Value, existingBreakNbrs);
                            }
                            else
                            {
                                Dictionary<Guid, List<int>> dayNumberLinks = new Dictionary<Guid, List<int>>();
                                dayNumberLinks.Add(item.Link.Value, existingBreakNbrs);
                                existingBreakNbrsDict.Add(item.DayNumber, dayNumberLinks);
                            }

                            if (result.Value is List<TimeScheduleTemplateBlock> scheduleBlocks)
                                savedTemplateBlocks.AddRange(scheduleBlocks);

                            //Only add TimeScheduleTemplatePeriod if block is not null
                            templateHead.TimeScheduleTemplatePeriod.Add(templatePeriod);

                            #endregion
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region Regular employee

                Dictionary<int, List<int>> existingBreakNbrsDict = new Dictionary<int, List<int>>();
                var itemsByDayNumberGrouping = inputItems.GroupBy(i => i.DayNumber).ToList();
                foreach (var itemsByDayNumber in itemsByDayNumberGrouping)
                {
                    foreach (TimeScheduleTemplateBlockDTO item in itemsByDayNumber)
                    {
                        if (item.DayNumber > templateHead.NoOfDays)
                            break;

                        #region TimeScheduleTemplatePeriod

                        TimeScheduleTemplatePeriod templatePeriod = null;

                        //Check if period exists and still has same DayNumber
                        if (item.TimeScheduleTemplatePeriodId > 0)
                            templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.TimeScheduleTemplatePeriodId == item.TimeScheduleTemplatePeriodId && i.DayNumber == item.DayNumber);

                        //Check if period exists with same DayNumber
                        if (templatePeriod == null)
                            templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.DayNumber == item.DayNumber);

                        if (templatePeriod == null)
                        {
                            #region Add

                            templatePeriod = new TimeScheduleTemplatePeriod()
                            {
                                DayNumber = item.DayNumber,
                            };
                            SetCreatedProperties(templatePeriod);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            // Update TimeScheduleTemplatePeriod
                            templatePeriod.State = (int)SoeEntityState.Active;

                            #endregion
                        }

                        #endregion

                        #region TimeScheduleTemplateBlock

                        // Not allowed to have an absence TimeCode in template
                        int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                        TimeCode timeCode = GetTimeCodeFromCache(item.TimeCodeId);
                        if (timeCode is TimeCodeAbsense)
                            item.TimeCodeId = defaultTimeCodeId;

                        List<int> existingBreakNbrs = new List<int>();
                        if (existingBreakNbrsDict.ContainsKey(item.DayNumber))
                            existingBreakNbrs = existingBreakNbrsDict[item.DayNumber];

                        //Set data and breaks
                        result = SetTimeScheduleTemplateBlockItemData(item, templatePeriod, employee, existingBreakNbrs);
                        if (!result.Success)
                            return result;

                        //Track which breaks that are created for daynumber
                        if (!existingBreakNbrs.Contains(1) && item.Break1TimeCodeId > 0)
                            existingBreakNbrs.Add(1);
                        if (!existingBreakNbrs.Contains(2) && item.Break2TimeCodeId > 0)
                            existingBreakNbrs.Add(2);
                        if (!existingBreakNbrs.Contains(3) && item.Break3TimeCodeId > 0)
                            existingBreakNbrs.Add(3);
                        if (!existingBreakNbrs.Contains(4) && item.Break4TimeCodeId > 0)
                            existingBreakNbrs.Add(4);

                        if (existingBreakNbrsDict.ContainsKey(item.DayNumber))
                            existingBreakNbrsDict[item.DayNumber] = existingBreakNbrs;
                        else
                            existingBreakNbrsDict.Add(item.DayNumber, existingBreakNbrs);

                        if (result.Value is List<TimeScheduleTemplateBlock> scheduleBlocks)
                            savedTemplateBlocks.AddRange(scheduleBlocks);

                        //Only add TimeScheduleTemplatePeriod if block is not null
                        templateHead.TimeScheduleTemplatePeriod.Add(templatePeriod);

                        #endregion
                    }
                }

                #endregion
            }

            //Need to save so added shifts are handled in delete region below
            result = Save();
            if (!result.Success)
                return result;

            #endregion

            #region Delete

            // Delete all existing block that does not exist in item collection
            if (timeScheduleTemplateHeadId != 0)
            {
                // Loop over original periods inside current scope
                List<TimeScheduleTemplatePeriod> originalTemplatePeriods = GetTimeScheduleTemplatePeriods(timeScheduleTemplateHeadId);
                foreach (TimeScheduleTemplatePeriod originalTemplatePeriod in originalTemplatePeriods.Where(p => p.DayNumber >= dayNumberFrom && p.DayNumber <= dayNumberTo))
                {
                    #region TimeScheduleTemplatePeriod

                    List<TimeScheduleTemplateBlock> originalTemplateBlocks = GetTemplateScheduleBlocksForPeriod(null, originalTemplatePeriod.TimeScheduleTemplatePeriodId, true, true);
                    foreach (TimeScheduleTemplateBlock originalTemplateBlock in originalTemplateBlocks)
                    {
                        #region TimeScheduleTemplateBlock

                        //Block is saved, i.e. not deleted
                        bool isUpdated = savedTemplateBlocks.Any(i => i.TimeScheduleTemplateBlockId == originalTemplateBlock.TimeScheduleTemplateBlockId);

                        //Block's period has a DayNumber in the range of NoOfDays
                        bool hasValidDayNumber = originalTemplateBlock.TimeScheduleTemplatePeriod.DayNumber <= templateHead.NoOfDays;

                        //Decide if block and it's period should be deleted
                        if (!isUpdated || !hasValidDayNumber)
                        {
                            if (originalTemplateBlocks.Count(b => b.State == (int)SoeEntityState.Active) > 1)
                            {
                                //Delete TimeScheduleTemplateBlock
                                ChangeEntityState(originalTemplateBlock, SoeEntityState.Deleted);
                            }
                            else
                            {
                                //Set as zero day
                                SetTimeScheduleTemplateBlockToZero(originalTemplateBlock);
                            }

                            if (!originalTemplateBlock.TimeScheduleTemplateBlockTask.IsLoaded)
                                originalTemplateBlock.TimeScheduleTemplateBlockTask.Load();

                            foreach (TimeScheduleTemplateBlockTask blockTask in originalTemplateBlock.TimeScheduleTemplateBlockTask.Where(i => i.State == (int)SoeEntityState.Active))
                            {
                                ChangeEntityState(blockTask, SoeEntityState.Deleted);
                            }
                        }

                        #endregion
                    }

                    #endregion
                }
            }

            #endregion

            #region Save

            result = Save();
            if (!result.Success)
                return result;

            if (result.Success)
                result.IntegerValue = templateHead.TimeScheduleTemplateHeadId;

            #endregion

            #region Calculate ScheduledTimeSummary

            if (employee != null)
            {
                DateTime endDate = CalendarUtility.GetLastDateOfYear(currentDate);
                if (base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, endDate))
                    TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employee.EmployeeId, CalendarUtility.GetFirstDateOfYear(currentDate), endDate);
            }

            #endregion

            return result;
        }

        private ActionResult AssignTimeScheduleTemplateToEmployee(int timeScheduleTemplateHeadId, int employeeId, DateTime startDate)
        {
            #region Validation

            TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHeadWithPeriodsBlocksAndTasks(timeScheduleTemplateHeadId, true);
            if (templateHead == null || !templateHead.EmployeePostId.HasValue)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");

            TimeScheduleTemplateHead templateHeadEmployeePost = GetEmployeeTimeScheduleTemplateHeadForEmployeePost(templateHead.EmployeePostId.Value, startDate);
            if (templateHeadEmployeePost != null)
                return new ActionResult((int)ActionResultSave.TimeScheduleTemplateHeadForEmployeePostAndEmployeeAlreadyExists, GetText(11532, "Tjänsten har redan anställd kopplad till sig"));

            EmployeePost employeePost = GetEmployeePost(templateHead.EmployeePostId.Value);
            if (employeePost == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeePost");

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            Employment employment = employee.GetEmployment(startDate);
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(4199, "Anställning hittades inte för anställd {0} och datum {1}"), employee.Name, startDate.ToShortDateString()));

            List<TimeScheduleTemplateHead> templateHeadsEmployee = GetPersonalTemplateHeads(employment.EmployeeId, startDate, null);
            if (!templateHeadsEmployee.IsNullOrEmpty())
            {
                TimeScheduleTemplateHead templateHeadEmployee = templateHeadsEmployee.FirstOrDefault(i => i.StartDate.HasValue && i.StartDate.Value.Date == startDate);
                if (templateHeadEmployee != null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, String.Format(GetText(11547, "Anställd {0} har ett grundschema med samma startdatum som tjänstens schema ({1})"), employee.Name, templateHeadEmployee.StartDate.HasValue ? templateHeadEmployee.StartDate.Value.ToShortDateString() : ""));
            }

            ActionResult result = IsEmployeeValidForEmployeePost(employment, employeePost, startDate);
            if (!result.Success)
                return result;

            #endregion

            #region TimeScheduleTemplateHead

            TimeScheduleTemplateHead newTemplateHead = new TimeScheduleTemplateHead()
            {
                Name = String.Format("{0} {1}", employee.EmployeeNr, employee.Name),
                Description = templateHead.Description,
                NoOfDays = templateHead.NoOfDays,
                StartDate = startDate.Date,
                StopDate = null,
                FirstMondayOfCycle = CalendarUtility.GetBeginningOfWeek(startDate.Date),
                StartOnFirstDayOfWeek = true,
                FlexForceSchedule = false,
                Locked = false,

                //Set FK
                ActorCompanyId = base.ActorCompanyId,
                EmployeeId = employeeId,
                EmployeePostId = templateHead.EmployeePostId.Value,
            };
            SetCreatedProperties(newTemplateHead);
            entities.TimeScheduleTemplateHead.AddObject(newTemplateHead);

            #endregion

            #region TimeScheduleTemplatePeriod + TimeScheduleTemplateBlock

            foreach (TimeScheduleTemplatePeriod templatePeriod in templateHead.TimeScheduleTemplatePeriod.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.DayNumber))
            {
                TimeScheduleTemplatePeriod newTemplatePeriod = new TimeScheduleTemplatePeriod()
                {
                    DayNumber = templatePeriod.DayNumber,
                };
                SetCreatedProperties(newTemplatePeriod);
                newTemplateHead.TimeScheduleTemplatePeriod.Add(newTemplatePeriod);

                foreach (TimeScheduleTemplateBlock templateBlock in templatePeriod.TimeScheduleTemplateBlock.Where(i => i.Date == null && i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime))
                {
                    TimeScheduleTemplateBlock newTemplateBlock = new TimeScheduleTemplateBlock()
                    {
                        Type = templateBlock.Type,
                        Date = templateBlock.Date,
                        StartTime = templateBlock.StartTime,
                        StopTime = templateBlock.StopTime,
                        BreakNumber = templateBlock.BreakNumber,
                        BreakType = templateBlock.BreakType,
                        Link = templateBlock.Link,
                        Description = templateBlock.Description,
                        ShiftStatus = templateBlock.ShiftStatus,
                        ShiftUserStatus = templateBlock.ShiftUserStatus,
                        TimeDeviationCauseStatus = templateBlock.TimeDeviationCauseStatus,
                        NbrOfWantedInQueue = templateBlock.NbrOfWantedInQueue,
                        NbrOfSuggestionsInQueue = templateBlock.NbrOfSuggestionsInQueue,

                        //Set FK
                        EmployeeId = employeeId,
                        TimeScheduleEmployeePeriodId = null,
                        TimeCodeId = templateBlock.TimeCodeId,
                        AccountId = templateBlock.AccountId,
                        TimeBreakTemplateId = templateBlock.TimeBreakTemplateId,
                        TimeHalfdayId = templateBlock.TimeHalfdayId,
                        ShiftTypeId = templateBlock.ShiftTypeId,
                        EmployeeScheduleId = templateBlock.EmployeeScheduleId,
                        TimeScheduleTypeId = templateBlock.TimeScheduleTypeId,
                        TimeDeviationCauseId = templateBlock.TimeDeviationCauseId,
                        EmployeeChildId = templateBlock.EmployeeChildId,
                        StaffingNeedsRowId = templateBlock.StaffingNeedsRowId,
                        StaffingNeedsRowPeriodId = templateBlock.StaffingNeedsRowPeriodId,
                        RecalculateTimeRecordId = null,
                        CustomerInvoiceId = null,
                        ProjectId = null,
                    };
                    SetCreatedProperties(newTemplateBlock);
                    newTemplatePeriod.TimeScheduleTemplateBlock.Add(newTemplateBlock);

                    foreach (AccountInternal accountInternal in templateBlock.AccountInternal)
                    {
                        newTemplateBlock.AccountInternal.Add(accountInternal);
                    }

                    foreach (TimeScheduleTemplateBlockTask blockTask in templateBlock.TimeScheduleTemplateBlockTask.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime))
                    {
                        TimeScheduleTemplateBlockTask newBlockTask = new TimeScheduleTemplateBlockTask()
                        {
                            StartTime = blockTask.StartTime,
                            StopTime = blockTask.StopTime,

                            //Set FK
                            ActorCompanyId = base.ActorCompanyId,
                            TimeScheduleTaskId = blockTask.TimeScheduleTaskId,
                            IncomingDeliveryRowId = blockTask.IncomingDeliveryRowId,
                        };
                        SetCreatedProperties(newBlockTask);
                        newTemplateBlock.TimeScheduleTemplateBlockTask.Add(newBlockTask);
                    }
                }
            }

            #endregion

            result = Save();

            return result;
        }

        private ActionResult RemoveEmployeeFromTimeScheduleTemplate(int timeScheduleTemplateHeadId)
        {
            TimeScheduleTemplateHead templateHeadEmployeePost = GetTimeScheduleTemplateHead(timeScheduleTemplateHeadId, onlyActive: true);
            if (templateHeadEmployeePost == null || !templateHeadEmployeePost.EmployeePostId.HasValue || !templateHeadEmployeePost.StartDate.HasValue)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");

            templateHeadEmployeePost.EmployeeId = null;

            TimeScheduleTemplateHead templateHeadEmployee = GetEmployeeTimeScheduleTemplateHeadForEmployeePost(templateHeadEmployeePost.EmployeePostId.Value, templateHeadEmployeePost.StartDate.Value);
            if (templateHeadEmployee != null)
                templateHeadEmployee.EmployeePostId = null;

            return SaveChanges(entities);
        }

        #endregion

        #region Schedule - active

        private TimeScheduleTemplatePeriod GetSequentialSchedule(int timeScheduleTemplatePeriodId, DateTime date, int employeeId, bool includeStandBy)
        {
            TimeScheduleTemplatePeriod newTimeScheduleTemplatePeriod = new TimeScheduleTemplatePeriod
            {
                TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId,
                TimeScheduleTemplateBlock = new EntityCollection<TimeScheduleTemplateBlock>(),
            };

            //Get schedule
            List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, employeeId, date, includeStandBy);
            foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks)
            {
                //Load TimeCode's after, beacause of the complexity of the query otherwise
                if (!templateBlock.TimeCodeReference.IsLoaded)
                    templateBlock.TimeCodeReference.Load();

                if (!templateBlock.TimeDeviationCauseReference.IsLoaded)
                    templateBlock.TimeDeviationCauseReference.Load();
            }

            //Split parents with regard to breaks
            List<TimeScheduleTemplateBlock> splittedTemplateBlocks = SplitTemplateBlocksToActualTimes(templateBlocks);
            newTimeScheduleTemplatePeriod.TimeScheduleTemplateBlock.AddRange(splittedTemplateBlocks);

            return newTimeScheduleTemplatePeriod;
        }

        private List<TimeScheduleTemplateBlockReference> CreateTimeScheduleTemplateBlocks(Employee employee, EmployeeSchedule employeeSchedule, DateTime startDate, DateTime stopDate, List<HolidayDTO> holidays = null, int? shiftTypeId = null, List<int> accountIdsFromPlacement = null, bool deleteExisting = true, bool createOnlyMissing = false, bool includeStandBy = true, bool includeOnDuty = false)
        {
            if (employeeSchedule == null)
                return new List<TimeScheduleTemplateBlockReference>();

            List<TimeScheduleTemplateBlockReference> references = new List<TimeScheduleTemplateBlockReference>();

            #region Prereq

            if (employeeSchedule.TimeScheduleTemplateHead == null && !employeeSchedule.TimeScheduleTemplateHeadReference.IsLoaded)
                employeeSchedule.TimeScheduleTemplateHeadReference.Load();
            if (!employeeSchedule.TimeScheduleTemplateHead.TimeScheduleTemplatePeriod.IsLoaded)
                employeeSchedule.TimeScheduleTemplateHead.TimeScheduleTemplatePeriod.Load();

            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();
            List<HolidayDTO> holidaysForCompany = GetHolidaysWithDayTypeAndHalfDaySettingsFromCache(startDate);
            List<DayType> dayTypesForCompany = GetDayTypesFromCache();
            List<TimeScheduleTemplateBlock> scheduleTemplateBlocksForHead = GetTemplateScheduleBlocksForHeadWithTaskAndAccounting(null, employeeSchedule.TimeScheduleTemplateHeadId, includeStandBy, includeOnDuty);
            List<TimeScheduleEmployeePeriod> employeePeriods = GetTimeScheduleEmployeePeriods(employee.EmployeeId, startDate, stopDate);
            Dictionary<int, List<HolidayDTO>> holidayDict = new Dictionary<int, List<HolidayDTO>>();
            SetAccountingPrioByEmployeeFromCache(startDate, stopDate, employee);

            #endregion

            DateTime date = startDate;
            while (date <= stopDate)
            {
                #region Date

                try
                {
                    #region Prereq

                    Employment employment = employee.GetEmployment(date);
                    if (employment == null)
                        continue;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups: employeeGroups);
                    if (employeeGroup == null)
                        continue;

                    bool useZeroSchedule = false;

                    // Check if EmployeeGroup work at Daytype
                    if (!employeeGroup.DayType.IsLoaded)
                        employeeGroup.DayType.Load();
                    DayType dayType = GetDayType(date, employeeGroup, holidaysForCompany, dayTypesForCompany);
                    if (dayType == null)
                        useZeroSchedule = true;

                    // Check if EmployeeGroup usually work on the specified DayType but it is a Company Holiday and not a HalfDay
                    List<HolidayDTO> holidaysForCompanyAndDate = FilterHolidays(holidaysForCompany, date);
                    bool isCompanyHoliday = holidaysForCompanyAndDate.Any();
                    bool isCompanyHalfday = IsDateHalfDay(holidaysForCompanyAndDate, dayType);
                    if (isCompanyHoliday && !isCompanyHalfday)
                        useZeroSchedule = true;

                    // Get TimeCode
                    int timeCodeId = employeeGroup?.TimeCodeId ?? GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                    TimeCode timeCode = GetTimeCodeFromCache(timeCodeId);

                    // Get Holiday's. May differ between EmployeeGroup, therefore we need to refetch holidays for each new EmployeeGroup and date
                    List<HolidayDTO> currentHolidays = holidays ?? FilterHolidays(holidaysForCompany, date, employeeGroup, holidayDict);

                    int dayNumber = CalendarUtility.GetScheduleDayNumber(date, employeeSchedule.StartDate, employeeSchedule.StartDayNumber, employeeSchedule.TimeScheduleTemplateHead.NoOfDays);

                    #endregion

                    #region Delete existing placements

                    if (deleteExisting)
                    {
                        // Check if placements exists for employee and date
                        List<TimeScheduleTemplateBlock> existingTemplateBlocks = GetScheduleBlocksForEmployeeWithoutScenario(employeeSchedule.EmployeeId, date);

                        // If onlyMissing parameter is true, no existing placements will be removed. Only missing placements in the date interval will be created
                        if (!createOnlyMissing)
                        {
                            SetTimeScheduleBlocksToDeleted(existingTemplateBlocks, saveChanges: false);
                        }
                        else if (existingTemplateBlocks.Any())
                        {
                            date = date.AddDays(1);
                            continue;
                        }
                    }

                    #endregion

                    #region Create schedule

                    List<TimeScheduleTemplateBlock> createdTemplateBlocksForDay = new List<TimeScheduleTemplateBlock>();

                    List<TimeScheduleTemplatePeriod> templatePeriods = employeeSchedule.TimeScheduleTemplateHead?.TimeScheduleTemplatePeriod?.Where(i => i.DayNumber == dayNumber && i.State == (int)SoeEntityState.Active).ToList();
                    if (templatePeriods != null && templatePeriods.Any())
                    {
                        CreateTimeBlockDateIfNotExists(employeeSchedule.EmployeeId, date);
                        TimeScheduleEmployeePeriod employeePeriod = CreateTimeScheduleEmployeePeriodIfNotExist(date, employeeSchedule.EmployeeId, out _, employeePeriods);

                        foreach (TimeScheduleTemplatePeriod templatePeriod in templatePeriods)
                        {
                            List<TimeScheduleTemplateBlock> createdTemplateBlocksForPeriod = new List<TimeScheduleTemplateBlock>();
                            if (!useZeroSchedule)
                            {
                                List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = scheduleTemplateBlocksForHead.Where(tb => tb.TimeScheduleTemplatePeriodId == templatePeriod.TimeScheduleTemplatePeriodId).ToList();
                                createdTemplateBlocksForPeriod = CreateTimeScheduleTemplateBlocks(employee, employeeSchedule, date, scheduleTemplateBlocks, employeePeriod, currentHolidays, shiftTypeId, accountIdsFromPlacement);
                                if (!createdTemplateBlocksForPeriod.IsNullOrEmpty())
                                    createdTemplateBlocksForDay.AddRange(createdTemplateBlocksForPeriod);
                            }
                            if (!createdTemplateBlocksForPeriod.Any() && timeCode != null)
                            {
                                TimeScheduleTemplateBlock templateBlock = CreateTimeScheduleTemplateBlockZero(employee, employeeSchedule, templatePeriod, employeePeriod, timeCode, date, shiftTypeId, accountIdsFromPlacement);
                                if (templateBlock != null)
                                    createdTemplateBlocksForDay.Add(templateBlock);
                            }
                        }
                    }

                    //Set breaks linked to parent block
                    SetTimeScheduleTemplateBlockBreakPropertiesFromParent(createdTemplateBlocksForDay);

                    TimeScheduleTemplateBlockReference reference = references.FirstOrDefault(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId);
                    if (reference != null)
                        reference.Update(createdTemplateBlocksForDay);
                    else
                        references.Add(new TimeScheduleTemplateBlockReference(employeeGroup, createdTemplateBlocksForDay));

                    #endregion
                }
                finally
                {
                    date = date.AddDays(1);
                }

                #endregion
            }

            return references;
        }

        private List<TimeScheduleTemplateBlock> CreateTimeScheduleTemplateBlocks(Employee employee, EmployeeSchedule employeeSchedule, DateTime date, List<TimeScheduleTemplateBlock> scheduleTemplateBlocks, TimeScheduleEmployeePeriod employeePeriod, List<HolidayDTO> holidays, int? shiftTypeId, List<int> accountIdsFromPlacement)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();

            Dictionary<string, string> linksRelationDict = new Dictionary<string, string>();

            TimeHalfdayDTO timeHalfDay = GetTimeHalfday(scheduleTemplateBlocks, holidays, date);
            List<TimeScheduleTemplateBlockDTO> scheduleBlockItems = ConvertToScheduleBlockItems(scheduleTemplateBlocks, timeHalfDay);
            scheduleBlockItems = scheduleBlockItems.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            if (scheduleBlockItems.Count > 1)
                scheduleBlockItems = scheduleBlockItems.Where(i => i.StartTime < i.StopTime).ToList();

            foreach (TimeScheduleTemplateBlockDTO scheduleBlockItem in scheduleBlockItems)
            {
                TimeCode timeCode = GetTimeCodeFromCache(scheduleBlockItem.TimeCodeId);
                if (!scheduleBlockItem.IsBreak || IsBreakWindowDuringSchedule(timeCode, scheduleBlockItems))
                {
                    TimeScheduleTemplateBlock templateBlock = CreateTimeScheduleTemplateBlock(employee, employeePeriod, employeeSchedule, scheduleBlockItem, timeCode, timeHalfDay, date, shiftTypeId, accountIdsFromPlacement, linksRelationDict);
                    if (templateBlock != null)
                        templateBlocks.Add(templateBlock);
                }
            }

            return templateBlocks;
        }

        private TimeScheduleTemplateBlock CreateTimeScheduleTemplateBlock(Employee employee, TimeScheduleEmployeePeriod employeePeriod, EmployeeSchedule employeeSchedule, TimeScheduleTemplateBlockDTO scheduleBlockItem, TimeCode timeCode, TimeHalfdayDTO timeHalfDay, DateTime date, int? shiftTypeId, List<int> accountIdsFromPlacement, Dictionary<string, string> linksRelationDict = null)
        {
            if (scheduleBlockItem == null || employeeSchedule == null || timeCode == null)
                return null;

            if (scheduleBlockItem.ShiftTypeId > 0)
                shiftTypeId = scheduleBlockItem.ShiftTypeId;

            TimeScheduleTemplateBlock newTemplateBlock = new TimeScheduleTemplateBlock()
            {
                StartTime = scheduleBlockItem.StartTime,
                StopTime = scheduleBlockItem.StopTime,
                Date = date.Date,
                BreakType = (int)scheduleBlockItem.BreakType,
                TimeCodeType = timeCode.Type,
                Description = scheduleBlockItem.Description,
                IsPreliminary = employeeSchedule.OverridedPreliminary ?? employeeSchedule.IsPreliminary,
                Type = (int)scheduleBlockItem.Type,

                //Set FK
                EmployeeId = employeeSchedule.EmployeeId,
                TimeScheduleTemplatePeriodId = scheduleBlockItem.TimeScheduleTemplatePeriodId,
                TimeScheduleTypeId = scheduleBlockItem.TimeScheduleTypeId.ToNullable(),
                AccountId = scheduleBlockItem.AccountId,
                StaffingNeedsRowId = scheduleBlockItem.StaffingNeedsRowId,
                TimeHalfdayId = timeHalfDay?.TimeHalfdayId,

                //Set references
                TimeCodeId = timeCode.TimeCodeId,
            };

            if (newTemplateBlock.StartTime == newTemplateBlock.StopTime)        // Zerodays not Preliminary
                newTemplateBlock.IsPreliminary = false;

            SetCreatedProperties(newTemplateBlock);
            entities.TimeScheduleTemplateBlock.AddObject(newTemplateBlock);

            SetTimeScheduleTemplateBlockLinked(newTemplateBlock, scheduleBlockItem.Link.HasValue ? scheduleBlockItem.Link.ToString() : String.Empty, linksRelationDict);
            SetTimeScheduleTemplateBlockStaffing(newTemplateBlock, employeeSchedule, employeePeriod, shiftTypeId);

            //Set accounting 
            List<int> accountIdsFromTemplateSchedule = scheduleBlockItem.AccountInternalIds?.Distinct().ToList() ?? new List<int>();
            ApplyAccountingOnTimeScheduleTemplateBlock(newTemplateBlock, employee, shiftTypeId, accountIdsFromTemplateSchedule, accountIdsFromPlacement);

            //TimeScheduleTemplateBlockTasks from template schedule
            if (scheduleBlockItem.Tasks != null && scheduleBlockItem.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None)
            {
                if (newTemplateBlock.TimeScheduleTemplateBlockTask == null)
                    newTemplateBlock.TimeScheduleTemplateBlockTask = new EntityCollection<TimeScheduleTemplateBlockTask>();

                foreach (TimeScheduleTemplateBlockTaskDTO taskDTO in scheduleBlockItem.Tasks.Where(i => i.State == SoeEntityState.Active))
                {
                    TimeScheduleTemplateBlockTask templateTask = GetTimeScheduleTemplateBlockTaskFromCache(taskDTO.TimeScheduleTemplateBlockTaskId);
                    if (templateTask != null)
                    {
                        TimeScheduleTemplateBlockTask newTask = new TimeScheduleTemplateBlockTask();
                        EntityUtil.Copy<TimeScheduleTemplateBlockTask>(newTask, templateTask);
                        newTask.StartTime = CalendarUtility.GetDateTime(date.Date, newTask.StartTime);
                        newTask.StopTime = CalendarUtility.GetDateTime(date.Date, newTask.StopTime);
                        newTemplateBlock.TimeScheduleTemplateBlockTask.Add(newTask);
                        SetCreatedProperties(newTask);

                        if (!newTask.TimeScheduleTaskId.HasValue && !newTask.IncomingDeliveryRowId.HasValue)
                            newTask.CreatedBy = (newTask.CreatedBy + " (" + this.currentTask + ") ").Left(50);
                    }
                }
            }

            return newTemplateBlock;
        }

        private TimeScheduleTemplateBlock CreateTimeScheduleTemplateBlock(TimeScheduleTemplateBlock originalTemplateBlock, DateTime? startTime = null, DateTime? stopTime = null, DateTime? date = null, TimeDeviationCause timeDeviationCause = null, bool copyAccounting = true, Dictionary<string, string> linksRelationDict = null, TimeScheduleType scheduleType = null, int? employeeChildId = null)
        {
            if (originalTemplateBlock == null)
                return null;

            TimeScheduleTemplateBlock newTemplateBlock = new TimeScheduleTemplateBlock()
            {
                StartTime = startTime ?? originalTemplateBlock.StartTime,
                StopTime = stopTime ?? originalTemplateBlock.StopTime,
                Date = date.HasValue ? date.Value.Date : originalTemplateBlock.Date,
                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                TimeCodeType = originalTemplateBlock.TimeCode?.Type ?? 0,
                Description = originalTemplateBlock.Description,

                //Set FK
                EmployeeId = originalTemplateBlock.EmployeeId,
                EmployeeScheduleId = originalTemplateBlock.EmployeeScheduleId,
                TimeScheduleTemplatePeriodId = originalTemplateBlock.TimeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriod = originalTemplateBlock.TimeScheduleEmployeePeriod,
                AccountId = originalTemplateBlock.AccountId,
                TimeDeviationCauseId = timeDeviationCause?.TimeDeviationCauseId,
                TimeDeviationCauseStatus = originalTemplateBlock.TimeDeviationCauseStatus,
                EmployeeChildId = employeeChildId,
                TimeHalfdayId = originalTemplateBlock.TimeHalfdayId,
                TimeScheduleTypeId = originalTemplateBlock.TimeScheduleTypeId.ToNullable(),
                TimeScheduleTypeName = scheduleType?.Name ?? "",
                ShiftTypeId = originalTemplateBlock.ShiftTypeId,
            };
            SetCreatedProperties(newTemplateBlock);
            entities.TimeScheduleTemplateBlock.AddObject(newTemplateBlock);

            //Set Link
            SetTimeScheduleTemplateBlockLinked(newTemplateBlock, originalTemplateBlock.Link, linksRelationDict);

            //Set TimeCode
            if (base.IsEntityAvailableInContext(entities, originalTemplateBlock.TimeCode))
                newTemplateBlock.TimeCode = originalTemplateBlock.TimeCode;
            else
                newTemplateBlock.TimeCodeId = originalTemplateBlock.TimeCodeId;

            //Set staffing
            SetTimeScheduleTemplateBlockStaffing(newTemplateBlock, originalTemplateBlock.EmployeeScheduleId, originalTemplateBlock.ShiftTypeId);

            //Set accounting
            if (copyAccounting)
                AddAccountInternalsToTimeScheduleTemplateBlock(newTemplateBlock, originalTemplateBlock.AccountInternal);

            return newTemplateBlock;
        }

        private TimeScheduleTemplateBlock CreateTimeScheduleTemplateBlockZero(Employee employee, EmployeeSchedule employeeSchedule, TimeScheduleTemplatePeriod scheduleTemplatePeriod, TimeScheduleEmployeePeriod employeePeriod, TimeCode timeCode, DateTime date, int? shiftTypeId, List<int> accountIdsFromPlacement)
        {
            if (employeeSchedule == null || timeCode == null)
                return null;

            TimeScheduleTemplateBlock newTemplateBlock = new TimeScheduleTemplateBlock()
            {
                StartTime = CalendarUtility.DATETIME_DEFAULT,
                StopTime = CalendarUtility.DATETIME_DEFAULT,
                Date = date,
                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                TimeCodeType = timeCode.Type,
                Description = null,
                IsPreliminary = false, // Zerodays not Preliminary

                //Set FK
                EmployeeId = employeeSchedule.EmployeeId,
                TimeScheduleTemplatePeriodId = scheduleTemplatePeriod.TimeScheduleTemplatePeriodId,
            };
            SetCreatedProperties(newTemplateBlock);
            entities.TimeScheduleTemplateBlock.AddObject(newTemplateBlock);

            //Set Link
            SetTimeScheduleTemplateBlockLinked(newTemplateBlock);

            //Set TimeCode
            if (base.IsEntityAvailableInContext(entities, timeCode))
                newTemplateBlock.TimeCode = timeCode;
            else
                newTemplateBlock.TimeCodeId = timeCode.TimeCodeId;

            //Set staffing
            SetTimeScheduleTemplateBlockStaffing(newTemplateBlock, employeeSchedule, employeePeriod, shiftTypeId);

            //Set accounting
            ApplyAccountingOnTimeScheduleTemplateBlock(newTemplateBlock, employee, shiftTypeId, null, accountIdsFromPlacement);

            return newTemplateBlock;
        }

        private void CreateTimeScheduleTemplateBlockZero(TimeScheduleTemplateBlock originalTemplateBlock, DateTime? date = null)
        {
            if (originalTemplateBlock == null)
                return;

            TimeScheduleTemplateBlock newTemplateBlock = new TimeScheduleTemplateBlock()
            {
                StartTime = CalendarUtility.DATETIME_DEFAULT,
                StopTime = CalendarUtility.DATETIME_DEFAULT,
                Date = date ?? originalTemplateBlock.Date,
                Description = originalTemplateBlock.Description,
                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                TimeCodeType = originalTemplateBlock.TimeCode?.Type ?? 0,

                //Set FK
                EmployeeId = originalTemplateBlock.EmployeeId,
                TimeScheduleTemplatePeriodId = originalTemplateBlock.TimeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriodId = originalTemplateBlock.TimeScheduleEmployeePeriodId,
                TimeScheduleScenarioHeadId = originalTemplateBlock.TimeScheduleScenarioHeadId,
                EmployeeScheduleId = originalTemplateBlock.EmployeeScheduleId,
                TimeHalfdayId = originalTemplateBlock.TimeHalfdayId,
                TimeScheduleTypeId = originalTemplateBlock.TimeScheduleTypeId.ToNullable(),
                ShiftTypeId = originalTemplateBlock.ShiftTypeId,
                AccountId = originalTemplateBlock.AccountId,
            };
            SetCreatedProperties(newTemplateBlock);
            entities.TimeScheduleTemplateBlock.AddObject(newTemplateBlock);

            //Set Link
            SetTimeScheduleTemplateBlockLinked(newTemplateBlock);

            //Set TimeCode
            if (base.IsEntityAvailableInContext(entities, originalTemplateBlock.TimeCode))
                newTemplateBlock.TimeCode = originalTemplateBlock.TimeCode;
            else
                newTemplateBlock.TimeCodeId = originalTemplateBlock.TimeCodeId;
        }

        private TimeScheduleTemplateBlock SetTimeScheduleTemplateBlockData(TimeScheduleTemplateBlockDTO item, TimeScheduleTemplateBlock templateBlock, TimeScheduleTemplatePeriod templatePeriod, List<AccountInternal> accountInternals, Employee employee, int breakNumber = 0, bool useAccountingFromSourceSchedule = true, DateTime? startDate = null)
        {
            if (item == null || templatePeriod == null)
                return null;

            #region Prereq

            bool isBreak = breakNumber > 0;
            int timeCodeId = item.GetTimeCodeId(breakNumber);
            if (isBreak && templateBlock != null && employee != null && employee.EmployeeId != templateBlock.EmployeeId)
            {
                DateTime? date = item.GetBreakDate(breakNumber);
                int newTimeCodeBreakId = GetTimeCodeBreakIdForEmployee(date, templateBlock.TimeCodeId, employee.EmployeeId, templateBlock.EmployeeId);
                if (newTimeCodeBreakId != timeCodeId)
                    timeCodeId = newTimeCodeBreakId;
            }

            if (item.StopTime - item.StartTime < new TimeSpan())
                item.StopTime = item.StopTime.AddDays(1);

            #endregion

            #region TimeScheduleTemplateBlock

            if (templateBlock == null)
            {
                // Do not create block without TimeCodes
                if (timeCodeId == 0)
                    return null;

                // Create block
                templateBlock = new TimeScheduleTemplateBlock();
                SetCreatedProperties(templateBlock);
            }
            else
            {
                // Update block
                SetModifiedProperties(templateBlock);
            }

            //Common
            templateBlock.Description = item.Description;
            templateBlock.StartTime = item.StartTime;
            templateBlock.StopTime = item.StopTime;
            templateBlock.Date = item.Date;
            templateBlock.BreakType = (int)(isBreak ? SoeTimeScheduleTemplateBlockBreakType.NormalBreak : SoeTimeScheduleTemplateBlockBreakType.None);
            templateBlock.State = (int)item.State;
            templateBlock.Type = (int)item.Type;

            //Set references
            templateBlock.TimeScheduleTemplatePeriod = templatePeriod;

            //If TimeCode is not specified, delete block if it exists (typically for removing a break)
            if (timeCodeId > 0)
            {
                templateBlock.TimeCode = GetTimeCodeFromCache(timeCodeId);
                if (templateBlock.TimeCode == null)
                {
                    // Check if TimeCode is inactive
                    TimeCode timeCode = GetTimeCodeDiscardState(timeCodeId);
                    if (timeCode != null && timeCode.State == (int)SoeEntityState.Inactive)
                        throw new SoeGeneralException(string.Format(isBreak ? GetText(12127, "Grundschemat har en rast med en inaktiv rasttyp ({0}). För att kunna spara grundschemat behöver du aktivera den igen, alternativt ändra rasttyp på rasten.") : GetText(12128, "Grundschemat har ett pass med en inaktiv tidkod ({0}). För att kunna spara grundschemat behöver du aktivera den igen."), timeCode.Name), this.ToString());
                }
            }
            else
                ChangeEntityState(templateBlock, SoeEntityState.Deleted);

            //Set employee if specified
            if (item.EmployeeId.HasValue && item.EmployeeId.Value > 0)
            {
                //Use passed Employee if it's the same
                if (employee != null && employee.EmployeeId == item.EmployeeId)
                    templateBlock.Employee = employee;
                else
                    templateBlock.Employee = GetEmployeeFromCache(item.EmployeeId.Value);
            }

            // Schedule type
            templateBlock.TimeScheduleTypeId = item.TimeScheduleTypeId.HasValue && item.TimeScheduleTypeId.Value != 0 ? item.TimeScheduleTypeId.Value : (int?)null;

            //Shift
            if (item.ShiftTypeId > 0 && !isBreak)
            {
                templateBlock.ShiftTypeId = item.ShiftTypeId;
                templateBlock.TimeScheduleTypeId = item.ShiftTypeTimeScheduleTypeId.HasValue && item.ShiftTypeTimeScheduleTypeId.Value != 0 ? item.ShiftTypeTimeScheduleTypeId.Value : (int?)null;
            }
            else
                templateBlock.ShiftTypeId = null;

            //Account
            templateBlock.AccountId = item.AccountId != 0 ? item.AccountId : (int?)null;

            // Links
            if (breakNumber == 0)
                templateBlock.Link = item.Link.HasValue ? item.Link.Value.ToString() : String.Empty;
            else if (breakNumber == 1)
                templateBlock.Link = item.Break1Link.HasValue ? item.Break1Link.Value.ToString() : String.Empty;
            else if (breakNumber == 2)
                templateBlock.Link = item.Break2Link.HasValue ? item.Break2Link.Value.ToString() : String.Empty;
            else if (breakNumber == 3)
                templateBlock.Link = item.Break3Link.HasValue ? item.Break3Link.Value.ToString() : String.Empty;
            else if (breakNumber == 4)
                templateBlock.Link = item.Break4Link.HasValue ? item.Break4Link.Value.ToString() : String.Empty;

            // Staffing needs
            templateBlock.StaffingNeedsRowId = item.StaffingNeedsRowId;
            templateBlock.StaffingNeedsRowPeriodId = item.StaffingNeedsRowPeriodId;

            #region Accounting

            if (useAccountingFromSourceSchedule)
            {
                // Just copy accounting from source schedule
                if (!accountInternals.IsNullOrEmpty())
                {
                    //Clear accounting
                    if (templateBlock.AccountInternal != null)
                        templateBlock.AccountInternal.Clear();

                    bool useEmployeeAccountInPrio = GetCompanyBoolSettingFromCache(CompanySettingType.FallbackOnEmployeeAccountInPrio);
                    if (UseAccountHierarchy() && employee != null && useEmployeeAccountInPrio)
                    {
                        ApplyAccountingPrioOnTimeScheduleTemplateBlock(templateBlock, employee);

                        //Remove all accountinternals on block on dims that are in the accountInternals list
                        foreach (AccountInternal accountInternal in accountInternals)
                        {
                            var dimid = accountInternal?.Account?.AccountDimId ?? 0;
                            if (dimid > 0)
                            {
                                templateBlock.AccountInternal.ToList().RemoveAll(a => a.Account != null && a.Account?.AccountDimId == dimid);
                            }
                        }
                    }

                    //Set accounting
                    AddAccountInternalsToTimeScheduleTemplateBlock(templateBlock, accountInternals);


                }
            }
            else if (employee != null)
            {
                // Get accounting from target employee accounts in a prioritized order

                DateTime date = startDate ?? item.Date ?? templatePeriod?.TimeScheduleTemplateHead?.StartDate ?? DateTime.Today;

                if (date != null)
                {
                    // Get only accounts on same level as company setting
                    List<EmployeeAccount> employeeAccounts = GetEmployeeAccountsOnDefaultLevel(employee.EmployeeId, date, date);

                    if (!employeeAccounts.IsNullOrEmpty())
                    {
                        // Find account prioritized first by main allocation and then by default
                        EmployeeAccount selectAccount = employeeAccounts.FirstOrDefault(f => f.MainAllocation && f.Default);

                        if (selectAccount == null)
                            selectAccount = employeeAccounts.FirstOrDefault(f => f.MainAllocation);

                        if (selectAccount == null)
                            selectAccount = employeeAccounts.FirstOrDefault(f => f.Default);

                        if (selectAccount == null)
                            selectAccount = employeeAccounts.FirstOrDefault();

                        if (selectAccount != null)
                            templateBlock.AccountId = selectAccount.AccountId;

                        if (selectAccount == null)
                        {
                            // No account found, do the same prioritized check on all levels
                            employeeAccounts = GetEmployeeAccounts(employee.EmployeeId, date, date);

                            selectAccount = employeeAccounts.FirstOrDefault(f => f.MainAllocation && f.Default);

                            if (selectAccount == null)
                                selectAccount = employeeAccounts.FirstOrDefault(f => f.MainAllocation);

                            if (selectAccount == null)
                                selectAccount = employeeAccounts.FirstOrDefault(f => f.Default);

                            if (selectAccount == null)
                                selectAccount = employeeAccounts.FirstOrDefault();

                            if (selectAccount != null)
                                templateBlock.AccountId = selectAccount.AccountId;
                        }
                    }

                    if (breakNumber == 0)
                    {
                        templateBlock.AccountInternal.Clear();
                        ApplyAccountingPrioOnTimeScheduleTemplateBlock(templateBlock, employee);
                    }
                }
            }

            #endregion

            //EmployeePeriod
            if (!templateBlock.TimeScheduleEmployeePeriodReference.IsLoaded)
                templateBlock.TimeScheduleEmployeePeriodReference.Load();

            if ((templateBlock.TimeScheduleEmployeePeriod == null) && templateBlock.EmployeeId.HasValue && templateBlock.TimeScheduleTemplatePeriod != null && templateBlock.Date.HasValue)
            {
                var employeePeriod = GetTimeScheduleEmployeePeriod(templateBlock.EmployeeId.Value, templateBlock.Date.Value);
                if (employeePeriod != null)
                    templateBlock.TimeScheduleEmployeePeriod = employeePeriod;
            }

            //Staffing Statuses
            if (UseStaffing() && templateBlock.Date.HasValue && templateBlock.EmployeeId.HasValue)
            {
                SetShiftStatus(templateBlock);
                SetShiftUserStatus(templateBlock, IsHiddenEmployeeFromCache(templateBlock.EmployeeId.Value) ? TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None : TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted);
            }

            if (this.CanEntityLoadReferences(entities, templateBlock) && !templateBlock.TimeScheduleTemplateBlockTask.IsLoaded)
                templateBlock.TimeScheduleTemplateBlockTask.Load();

            if (!item.Tasks.IsNullOrEmpty() && templateBlock.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None)
            {
                if (templateBlock.TimeScheduleTemplateBlockTask == null)
                    templateBlock.TimeScheduleTemplateBlockTask = new EntityCollection<TimeScheduleTemplateBlockTask>();

                var existingTasks = item.Tasks.OrderBy(o => o.StartTime).ToList();

                foreach (TimeScheduleTemplateBlockTask existingTask in templateBlock.TimeScheduleTemplateBlockTask.OrderBy(o => o.StartTime))
                {
                    #region Update / Delete

                    TimeScheduleTemplateBlockTaskDTO inputTask = existingTasks.FirstOrDefault(i => i.TimeScheduleTemplateBlockId.HasValue && i.TimeScheduleTemplateBlockId.Value == existingTask.TimeScheduleTemplateBlockId && i.State == (int)SoeEntityState.Active);
                    if (inputTask != null)
                    {
                        existingTask.StartTime = CalendarUtility.GetDateTime(templateBlock.Date ?? CalendarUtility.DATETIME_DEFAULT, inputTask.StartTime);
                        existingTask.StopTime = CalendarUtility.GetDateTime(templateBlock.Date ?? CalendarUtility.DATETIME_DEFAULT, inputTask.StopTime);
                        existingTask.TimeScheduleTaskId = inputTask.TimeScheduleTaskId;
                        existingTask.IncomingDeliveryRowId = inputTask.IncomingDeliveryRowId;
                        existingTasks.Remove(inputTask);
                    }
                    else
                    {
                        existingTask.State = (int)SoeEntityState.Deleted;
                    }

                    SetModifiedProperties(existingTask);

                    #endregion
                }

                #region Add

                foreach (TimeScheduleTemplateBlockTaskDTO newTask in item.Tasks.Where(i => (!i.TimeScheduleTemplateBlockId.HasValue || i.TimeScheduleTemplateBlockId.Value == 0) && i.State == (int)SoeEntityState.Active))
                {
                    TimeScheduleTemplateBlockTask task = new TimeScheduleTemplateBlockTask()
                    {
                        StartTime = CalendarUtility.GetDateTime(templateBlock.Date ?? CalendarUtility.DATETIME_DEFAULT, newTask.StartTime),
                        StopTime = CalendarUtility.GetDateTime(templateBlock.Date ?? CalendarUtility.DATETIME_DEFAULT, newTask.StopTime),

                        //Set FK
                        ActorCompanyId = this.actorCompanyId,
                        TimeScheduleTaskId = newTask.TimeScheduleTaskId,
                        IncomingDeliveryRowId = newTask.IncomingDeliveryRowId,

                        //Set references
                        TimeScheduleTemplateBlock = templateBlock,
                    };
                    SetCreatedProperties(task);
                    templateBlock.TimeScheduleTemplateBlockTask.Add(task);
                }

                #endregion
            }

            #endregion

            return templateBlock;
        }

        private TimeScheduleTemplateBlock SetTimeScheduleTemplateBlockBreakData(TimeScheduleTemplateBlockDTO item, List<TimeScheduleTemplateBlock> templateBlocks, TimeScheduleTemplatePeriod templatePeriod, List<AccountInternal> accountInternals, Employee employee, int breakNr, bool useAccountingFromSourceSchedule = true, DateTime? startDate = null)
        {
            if (item == null)
                return null;

            TimeScheduleTemplateBlock templateBlockBreak = GetScheduleBlockWithTimeCodeAndAccounting(item.GetBreakId(breakNr));
            templateBlockBreak = SetTimeScheduleTemplateBlockData(item, templateBlockBreak, templatePeriod, accountInternals, employee, breakNr, useAccountingFromSourceSchedule, startDate);
            if (templateBlockBreak != null)
            {
                if (item.HasBreakTimes)
                    SetBreakTime(templateBlockBreak, item.GetBreakStartTime(breakNr), item.ActualDate, item.GetBreakMinutes(breakNr));
                else
                    ParseBreak(templateBlockBreak, templateBlocks);
            }

            return templateBlockBreak;
        }

        private void UpdateScheduleTemplateBlock(TimeSchedulePlanningDayDTO dayDTO, TimeScheduleTemplateBlock originalBlock, Employee employee, bool changeEmployee = false)
        {
            originalBlock.Description = dayDTO.Description;
            originalBlock.Link = dayDTO.Link.HasValue ? dayDTO.Link.Value.ToString() : String.Empty;
            originalBlock.StartTime = CalendarUtility.GetScheduleTime(dayDTO.StartTime);
            originalBlock.StopTime = CalendarUtility.GetScheduleTime(dayDTO.StopTime, dayDTO.StartTime.Date, dayDTO.StopTime.Date);
            originalBlock.Date = dayDTO.StartTime.Date;
            if (dayDTO.BelongsToPreviousDay)
            {
                originalBlock.Date = originalBlock.Date.Value.AddDays(-1).Date;
                originalBlock.StartTime = originalBlock.StartTime.AddDays(1);
                originalBlock.StopTime = originalBlock.StopTime.AddDays(1);
            }
            else if (dayDTO.BelongsToNextDay)
            {
                originalBlock.Date = originalBlock.Date.Value.AddDays(1).Date;
                originalBlock.StartTime = originalBlock.StartTime.AddDays(-1);
                originalBlock.StopTime = originalBlock.StopTime.AddDays(-1);
            }

            if (dayDTO.Type == TermGroup_TimeScheduleTemplateBlockType.Booking)
            {
                //save orgiginal booking dates to be able to handle booking spanning multiple days
                originalBlock.StartTime = dayDTO.StartTime;
                originalBlock.StopTime = dayDTO.StopTime;
            }

            originalBlock.ExtraShift = dayDTO.ExtraShift;
            originalBlock.SubstituteShift = dayDTO.SubstituteShift;
            originalBlock.IsPreliminary = dayDTO.IsPreliminary;
            originalBlock.AbsenceType = (int)dayDTO.AbsenceType;

            //Set FK
            originalBlock.TimeScheduleTemplatePeriodId = dayDTO.TimeScheduleTemplatePeriodId;
            originalBlock.TimeScheduleEmployeePeriodId = dayDTO.TimeScheduleEmployeePeriodId;
            originalBlock.TimeScheduleScenarioHeadId = dayDTO.TimeScheduleScenarioHeadId;
            originalBlock.TimeCodeId = dayDTO.TimeCodeId;
            originalBlock.TimeScheduleTypeId = dayDTO.TimeScheduleTypeId != 0 ? dayDTO.TimeScheduleTypeId : (int?)null;
            originalBlock.ShiftTypeId = dayDTO.ShiftTypeId != 0 ? (int?)dayDTO.ShiftTypeId : null;
            originalBlock.AccountId = dayDTO.AccountId != 0 ? dayDTO.AccountId : (int?)null;

            if (originalBlock.StartTime == originalBlock.StopTime && originalBlock.IsStandby())
            {
                originalBlock.ShiftTypeId = null;
                originalBlock.Type = (int)TermGroup_TimeScheduleTemplateBlockType.Schedule;
            }

            //Connect EmployeeSchedule
            if (!dayDTO.TimeScheduleScenarioHeadId.HasValue)
                originalBlock.EmployeeSchedule = GetEmployeeScheduleFromCache(employee.EmployeeId, originalBlock.Date.Value);

            if (changeEmployee)
            {
                originalBlock.EmployeeId = dayDTO.EmployeeId;
                if (dayDTO.Order != null)
                    SyncOriginUserWithScheduleBlocks(dayDTO.Order.OrderId);
            }

            ApplyAccountingOnTimeScheduleTemplateBlock(originalBlock, employee, originalBlock.ShiftTypeId, null, null);

            if (dayDTO.IsDeleted)
                originalBlock.State = (int)SoeEntityState.Deleted;
            else
                originalBlock.State = (int)SoeEntityState.Active;// If this is a zero schedule block (absence) it has been deleted in previous method
            SetModifiedProperties(originalBlock);
        }

        private void SetTimeScheduleTemplateBlockBreakPropertiesFromParent(List<TimeScheduleTemplateBlock> templateBlocks)
        {
            var days = templateBlocks.Where(tb => tb.Date.HasValue).GroupBy(tb => tb.Date.Value).ToList();
            foreach (var day in days)
            {
                var templateBlocksForDay = day.ToList();
                var templateBlockBreaksForDay = templateBlocksForDay.GetBreaks();

                foreach (var templateBlock in templateBlocksForDay)
                {
                    var currentBreaks = templateBlock.GetOverlappedBreaks(templateBlockBreaksForDay);
                    foreach (var currentBreak in currentBreaks)
                    {
                        currentBreak.Link = templateBlock.Link;
                        currentBreak.AccountId = templateBlock.AccountId;
                    }
                }
            }
        }

        private void SetTimeScheduleTemplateBlockStaffing(TimeScheduleTemplateBlock templateBlock, EmployeeSchedule employeeSchedule, TimeScheduleEmployeePeriod employeePeriod, int? shiftTypeId)
        {
            if (templateBlock == null)
                return;

            if (UseStaffing())
            {
                //Set FK
                templateBlock.ShiftTypeId = shiftTypeId;

                //Set references
                templateBlock.EmployeeSchedule = employeeSchedule;
                templateBlock.TimeScheduleEmployeePeriod = employeePeriod;

                // Set shift statuses
                SetShiftStatus(templateBlock);
                SetShiftUserStatus(templateBlock, IsHiddenEmployeeFromCache(employeeSchedule.EmployeeId) ? TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None : TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted);
            }
        }

        private void SetTimeScheduleTemplateBlockStaffing(TimeScheduleTemplateBlock templateBlock, int? employeeScheduleId, int? shiftTypeId)
        {
            if (templateBlock == null)
                return;

            if (UseStaffing())
            {
                templateBlock.EmployeeScheduleId = employeeScheduleId;
                templateBlock.ShiftTypeId = shiftTypeId;
            }
        }

        private void SetTimeScheduleTemplateBlockToZero(TimeScheduleTemplateBlock templateBlock)
        {
            if (templateBlock == null)
                return;

            templateBlock.StartTime = CalendarUtility.DATETIME_DEFAULT;
            templateBlock.StopTime = CalendarUtility.DATETIME_DEFAULT;
            templateBlock.TimeScheduleTypeId = null;
            templateBlock.Description = String.Empty;

            if (UseStaffing())
            {
                templateBlock.ShiftTypeId = null;
                if (templateBlock.IsStandby())
                    templateBlock.Type = (int)TermGroup_TimeScheduleTemplateBlockType.Schedule;
            }

            SetModifiedProperties(templateBlock);
        }

        private void ClearTimeScheduleTemplateBlocks(int employeeId, DateTime date, bool clearScheduledAbsence, bool clearScheduledPlacement, bool updateOrderRemainingTime)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksFromCache(employeeId, date);
            ClearTimeScheduleTemplateBlocks(templateBlocks, clearScheduledAbsence, clearScheduledPlacement, updateOrderRemainingTime, out _);
        }

        private void ClearTimeScheduleTemplateBlocks(List<TimeScheduleTemplateBlock> scheduleBlocks, bool clearScheduledAbsence, bool clearScheduledPlacement, bool updateOrderRemainingTime, out bool hasChanges)
        {
            hasChanges = false;

            if (scheduleBlocks.IsNullOrEmpty())
                return;

            foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks)
            {
                if (clearScheduledAbsence)
                {
                    if (scheduleBlock.TimeDeviationCauseId.HasValue)
                    {
                        if (updateOrderRemainingTime && scheduleBlock.CustomerInvoiceId.HasValue && scheduleBlock.Date.HasValue)
                        {
                            int length = (int)(scheduleBlock.StopTime - scheduleBlock.StartTime).TotalMinutes;
                            UpdateOrderRemainingTime(length, this.actorCompanyId, scheduleBlock.CustomerInvoiceId.Value);
                        }

                        scheduleBlock.TimeDeviationCauseId = null;
                        scheduleBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.None;
                        scheduleBlock.EmployeeChildId = null;
                        scheduleBlock.TimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                        hasChanges = true;
                    }

                    if (scheduleBlock.ShiftUserStatus != (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None &&
                        scheduleBlock.ShiftUserStatus != (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted)
                    {
                        scheduleBlock.ShiftUserStatus = (int)(IsHiddenEmployeeFromCache(scheduleBlock.EmployeeId.Value) ? TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None : TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted);
                        hasChanges = true;
                    }
                }

                // Clear link to scheduled placement. Otherwise, if schedule has changes until schedule placement is applied, those presence-changes will be lost.
                if (clearScheduledPlacement && scheduleBlock.RecalculateTimeRecordId.HasValue)
                {
                    scheduleBlock.RecalculateTimeRecordId = null;
                    hasChanges = true;
                }
            }

            ClearScheduleFromCache(scheduleBlocks);
        }

        private ActionResult SetTimeScheduleTemplateBlockItemData(TimeScheduleTemplateBlockDTO item, TimeScheduleTemplatePeriod templatePeriod = null, Employee employee = null, List<int> existingBreakNbrs = null, bool useAccountingFromSourceSchedule = true, DateTime? startDate = null)
        {
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<TimeScheduleTemplateBlock> savedTemplateBlocks = new List<TimeScheduleTemplateBlock>();

            TimeScheduleTemplateBlock templateBlockWork = GetScheduleBlockWithTimeCodeAndAccounting(item.TimeScheduleTemplateBlockId);
            if (templateBlockWork != null && templatePeriod == null)
            {
                if (templateBlockWork.TimeScheduleTemplatePeriodId.HasValue)
                    templatePeriod = GetTimeScheduleTemplatePeriodFromCache(templateBlockWork.TimeScheduleTemplatePeriodId.Value);
                if (templatePeriod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplatePeriod");
            }

            #region Accounts

            List<AccountInternal> accountInternals = new List<AccountInternal>();
            List<AccountInternal> allAccountInternals = GetAccountInternalsWithAccountFromCache();
            foreach (int accountDimId in item.DimIds)
            {
                AccountInternal accountInternal = allAccountInternals.FirstOrDefault(a => a.AccountId == accountDimId);
                if (accountInternal != null)
                    accountInternals.Add(accountInternal);
            }

            #endregion

            #endregion

            #region Work

            templateBlockWork = SetTimeScheduleTemplateBlockData(item, templateBlockWork, templatePeriod, accountInternals, employee, useAccountingFromSourceSchedule: useAccountingFromSourceSchedule, startDate: startDate);
            if (templateBlockWork != null)
                savedTemplateBlocks.Add(templateBlockWork);

            #endregion

            #region Breaks

            List<TimeScheduleTemplateBlock> templateBlocks = savedTemplateBlocks.GetWork();
            for (int i = TimeScheduleTemplateBlockDTO.MIN_BREAK; i <= TimeScheduleTemplateBlockDTO.MAX_BREAK; i++)
            {
                if (existingBreakNbrs != null && existingBreakNbrs.Contains(i))
                    continue;

                TimeScheduleTemplateBlock templateBlockBreak = SetTimeScheduleTemplateBlockBreakData(item, templateBlocks, templatePeriod, accountInternals, employee, i, useAccountingFromSourceSchedule: useAccountingFromSourceSchedule, startDate: startDate);
                if (templateBlockBreak != null)
                {
                    if (employee != null && employee.Hidden && templateBlockWork != null && templateBlockBreak.Link != templateBlockWork.Link)
                        templateBlockBreak.Link = templateBlockWork.Link;
                    savedTemplateBlocks.Add(templateBlockBreak);
                }
            }

            #endregion

            #region StaffingNeeds

            if (templateBlockWork != null)
                templateBlockWork.StaffingNeedsRowPeriodId = item.StaffingNeedsRowPeriodId;

            #endregion

            //Updated TimeScheduleTemplateBlocks
            result.Value = savedTemplateBlocks;

            return result;
        }

        private ActionResult SetUselessActiveZeroShiftsToDeleted(int employeeId, DateTime date, int? timeScheduleScenarioHeadId)
        {
            return SetUselessActiveZeroShiftsToDeleted(employeeId, new List<DateTime> { date }, timeScheduleScenarioHeadId);
        }

        private ActionResult SetUselessActiveZeroShiftsToDeleted(int employeeId, List<DateTime> dates, int? timeScheduleScenarioHeadId)
        {
            ActionResult result = new ActionResult();

            List<TimeScheduleTemplateBlock> allBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                         where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == employeeId) &&
                                                         (tb.Date.HasValue && dates.Contains(tb.Date.Value)) &&
                                                         (tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None) &&
                                                         (tb.State == (int)SoeEntityState.Active))
                                                         select tb).ToList();

            allBlocks = allBlocks.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();

            foreach (var blocksByDate in allBlocks.GroupBy(x => x.Date))
            {
                var blocks = blocksByDate.ToList();

                //if only one shifts exists then continue
                if ((blocks.Count == 1))
                    continue;

                //if no zero shifts exists then continue
                if (!blocks.Any(x => x.StartTime == x.StopTime))
                    continue;

                bool notZeroShiftsExists = blocks.Any(x => x.StartTime != x.StopTime);
                bool zeroShiftsExists = blocks.Any(x => x.StartTime == x.StopTime);
                var zeroShifts = blocks.Where(x => x.StartTime == x.StopTime).ToList();

                if (notZeroShiftsExists)
                {
                    //Delete all zero shifts
                    foreach (var zeroShift in zeroShifts)
                    {
                        result = ChangeEntityState(zeroShift, SoeEntityState.Deleted);
                        if (!result.Success)
                            return result;
                    }
                }
                else
                {
                    //if notZeroShifts dont exists, then see if we have zeroShifts
                    if (zeroShiftsExists)
                    {
                        //if only one zero Shifts exists then continue
                        if (zeroShifts.Count == 1)
                            continue;

                        if (zeroShifts.Count > 1)
                        {
                            // delete all zero shifts but keep one, i have choosen the last one
                            var lastCreated = zeroShifts.OrderBy(x => x.Created.HasValue).LastOrDefault();

                            foreach (var zeroShift in zeroShifts)
                            {
                                if (zeroShift.TimeScheduleTemplateBlockId == lastCreated?.TimeScheduleTemplateBlockId)
                                    continue;

                                result = ChangeEntityState(zeroShift, SoeEntityState.Deleted);
                                if (!result.Success)
                                    return result;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private ActionResult ClearTimeScheduleTemplateBlocksAndApplyAccounting(List<TimeScheduleTemplateBlock> templateBlocks, Employee employee, List<DateTime> clearScheduledAbsenceOnDates, bool clearScheduledPlacement, bool updateOrderRemainingTime)
        {
            ActionResult result = new ActionResult(true);

            if (!templateBlocks.IsNullOrEmpty())
            {
                foreach (var templateBlocksByDate in templateBlocks.Where(i => i.Date.HasValue).GroupBy(i => i.Date.Value))
                {
                    bool clearScheduledAbsence = clearScheduledAbsenceOnDates?.Contains(templateBlocksByDate.Key) ?? false;
                    ClearTimeScheduleTemplateBlocksAndApplyAccounting(templateBlocksByDate.ToList(), employee, clearScheduledAbsence, clearScheduledPlacement, updateOrderRemainingTime);
                }
            }

            return result;
        }

        private ActionResult ClearTimeScheduleTemplateBlocksAndApplyAccounting(List<TimeScheduleTemplateBlock> scheduleBlocks, Employee employee, bool clearScheduledAbsence, bool clearScheduledPlacement, bool updateOrderRemainingTime)
        {
            ActionResult result = new ActionResult(true);

            if (!scheduleBlocks.IsNullOrEmpty())
            {
                ClearTimeScheduleTemplateBlocks(scheduleBlocks, clearScheduledAbsence, clearScheduledPlacement, updateOrderRemainingTime, out bool hasChanges);

                foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks)
                {
                    ApplyAccountingOnTimeScheduleTemplateBlock(scheduleBlock, employee, scheduleBlock.ShiftTypeId, new List<int>(), new List<int>());
                }

                if (hasChanges)
                {
                    result = Save();
                    if (!result.Success)
                        LogError($"ClearTimeScheduleTemplateBlocksAndApplyAccounting failed. EmployeeId:{employee.EmployeeId}. Date:{scheduleBlocks.FirstOrDefault()?.Date}");
                }
            }

            return result;
        }

        #endregion

        #region Schedule - breaks

        /// <summary>
        /// Call this method to decide which breaks that should be handled (copied or moved with the shift)        
        /// </summary>
        /// <param name="sourceShiftId">Is the shiftid that WILL be copied or moved</param>
        /// <returns></returns>
        private List<TimeScheduleTemplateBlock> GetBreaksToHandleDueToShiftChanges(int sourceShiftId, List<TimeScheduleTemplateBlock> sourceBreakBlocksInput = null)
        {
            List<TimeScheduleTemplateBlock> breaksToHandle = new List<TimeScheduleTemplateBlock>();

            TimeScheduleTemplateBlock sourceScheduleBlock = GetScheduleBlockWithPeriodAndStaffing(sourceShiftId);
            if (sourceScheduleBlock == null)
                return breaksToHandle;

            if (!sourceScheduleBlock.TimeScheduleEmployeePeriodId.HasValue)
                return breaksToHandle;

            List<TimeScheduleTemplateBlock> sourceBreakBlocks = sourceBreakBlocksInput ?? GetTimeScheduleShiftBreaks(sourceScheduleBlock.TimeScheduleScenarioHeadId, sourceScheduleBlock.TimeScheduleEmployeePeriodId.Value, null).ToList();

            if (IsHiddenEmployeeFromCache(sourceScheduleBlock.EmployeeId.Value))
            {
                if (!String.IsNullOrEmpty(sourceScheduleBlock.Link))
                {
                    //Get breaks with same link
                    breaksToHandle = sourceScheduleBlock.GetOverlappedBreaks(sourceBreakBlocks, true).Where(x => !string.IsNullOrEmpty(x.Link) && x.Link == sourceScheduleBlock.Link).ToList();
                }
                else
                {
                    //Get breaks with no link
                    breaksToHandle = sourceScheduleBlock.GetOverlappedBreaks(sourceBreakBlocks, true).Where(x => string.IsNullOrEmpty(x.Link)).ToList();
                }
            }
            else
            {
                breaksToHandle = sourceScheduleBlock.GetOverlappedBreaks(sourceBreakBlocks, true);
            }

            return breaksToHandle;
        }

        private List<TimeScheduleTemplateBlock> GetBreaksForScheduleBlock(TimeScheduleTemplateBlock sourceScheduleBlock)
        {
            if (sourceScheduleBlock == null || !sourceScheduleBlock.TimeScheduleEmployeePeriodId.HasValue)
                return new List<TimeScheduleTemplateBlock>();

            // If hidden employee, only get breaks connected to current shift
            Guid? link = null;
            if (IsHiddenEmployeeFromCache(sourceScheduleBlock.EmployeeId.Value) && !String.IsNullOrEmpty(sourceScheduleBlock.Link))
                link = new Guid(sourceScheduleBlock.Link);

            List<TimeScheduleTemplateBlock> sourceBreakBlocks = GetTimeScheduleShiftBreaks(sourceScheduleBlock.TimeScheduleScenarioHeadId, sourceScheduleBlock.TimeScheduleEmployeePeriodId.Value, link).ToList();
            return sourceScheduleBlock.GetOverlappedBreaks(sourceBreakBlocks);
        }

        /// <summary>
        /// Call this method to decide which breaks that should be handled (copied or moved with the shift)        
        /// All overlapping breaks will be returned
        /// </summary>
        /// <param name="shift">Is the shift that WILL be copied or moved</param>
        /// <returns></returns>
        private List<TimeScheduleTemplateBlock> GetBreaksToHandleDueToShiftChanges(TimeSchedulePlanningDayDTO shift, List<TimeScheduleTemplateBlock> breaks)
        {
            List<TimeScheduleTemplateBlock> breaksToHandle = new List<TimeScheduleTemplateBlock>();

            foreach (var breakitem in breaks)
            {
                if (breakitem.ActualStartTime.Value >= shift.StartTime && breakitem.ActualStopTime.Value <= shift.StopTime)
                    breaksToHandle.Add(breakitem);
            }

            return breaksToHandle;
        }

        /// <summary>
        /// Get all schedule break blocks for specified period
        /// </summary>
        /// <param name="timeScheduleEmployeePeriodId">TimeScheduleEmployeePeriodId</param>
        /// <param name="link">If specified, only return breaks with specified link</param>
        /// <returns>List of break blocks</returns>
        private List<TimeScheduleTemplateBlock> GetTimeScheduleShiftBreaks(int? timeScheduleScenarioHeadId, int timeScheduleEmployeePeriodId, Guid? link = null)
        {
            IQueryable<TimeScheduleTemplateBlock> query = (from tb in entities.TimeScheduleTemplateBlock
                                                           where tb.TimeScheduleEmployeePeriodId == timeScheduleEmployeePeriodId &&
                                                         tb.Date.HasValue &&
                                                           tb.BreakType > (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                           tb.State == (int)SoeEntityState.Active
                                                           orderby tb.StartTime
                                                           select tb);

            if (link.HasValue)
            {
                string linkStr = link.Value.ToString();
                query = query.Where(b => b.Link.Equals(linkStr));
            }

            var result = query.ToList();
            result = result.FilterScenario(timeScheduleScenarioHeadId);
            return result;
        }

        private List<BreakDTO> GetOverlappedBreaks(TimeSchedulePlanningDayDTO shift, List<BreakDTO> breakItems)
        {
            List<BreakDTO> overlappedBreaks = shift.GetOverlappedBreaks(breakItems, true);

            if (IsHiddenEmployeeFromCache(shift.EmployeeId))
            {
                if (shift.Link.HasValue)
                    overlappedBreaks = overlappedBreaks.Where(x => x.Link.HasValue && x.Link == shift.Link).ToList();
                else
                    overlappedBreaks = overlappedBreaks.Where(x => !x.Link.HasValue).ToList();
            }

            return overlappedBreaks;
        }

        private ActionResult SaveEmployeePeriodBreaks(int employeeId, DateTime date, int? timeScheduleTemplatePeriodId, int timeScheduleEmployeePeriodId, int? timeScheduleScenarioHeadId, List<TimeSchedulePlanningDayDTO> shifts, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = new ActionResult();

            if (!shifts.Any(x => x.IsSchedule()))
                return result;

            Dictionary<string, List<BreakDTO>> pairs = new Dictionary<string, List<BreakDTO>>();

            bool isHidden = IsHiddenEmployeeFromCache(employeeId);
            bool dayHasExtraShifts = shifts.Any(x => x.ExtraShift);

            foreach (var item in shifts.Where(x => x.IsSchedule() && !x.IsDeleted).GroupBy(x => isHidden ? x.Link.ToString() : ""))
            {
                var guidShifts = item.ToList();
                var guidBreaks = item.First().GetBreaks();

                foreach (var shift in guidShifts)
                    shift.CopyShiftInfo(shift.GetMyBreaks(guidBreaks));

                pairs.Add(item.Key, guidBreaks);
            }

            foreach (var pair in pairs)
            {
                Guid? linkGuid = string.IsNullOrEmpty(pair.Key) ? (Guid?)null : new Guid(pair.Key);
                List<BreakDTO> inputBreaks = pair.Value;
                List<TimeScheduleTemplateBlock> existingLinkBreakBlocks = GetTimeScheduleShiftBreaks(timeScheduleScenarioHeadId, timeScheduleEmployeePeriodId, linkGuid);

                if (isHidden)
                {
                    // Get all breaks for this day that does not have the same link.                    
                    List<TimeScheduleTemplateBlock> otherBreakBlocks = GetTimeScheduleShiftBreaks(timeScheduleScenarioHeadId, timeScheduleEmployeePeriodId).Where(b => !b.Link.Equals(linkGuid.ToString())).ToList();

                    foreach (var inputBreak in inputBreaks.Where(x => x.Id != 0))
                    {
                        TimeScheduleTemplateBlock otherBreakBlock = otherBreakBlocks.FirstOrDefault(b => b.TimeScheduleTemplateBlockId == inputBreak.Id);
                        if (otherBreakBlock != null)
                        {
                            //Break exists in input, update it (if you unlink hidden employee shifts the breaks will have different links, so the links should be updated)
                            result = UpdateEmployeePeriodBreak(otherBreakBlock, inputBreak.TimeCodeId, inputBreak.AccountId, date, inputBreak.StartTime, inputBreak.IsPreliminary, inputBreak.Link, logProperties, dayHasExtraShifts: dayHasExtraShifts);
                            if (!result.Success)
                                return result;
                        }
                    }
                }


                foreach (TimeScheduleTemplateBlock existingLinkBreakBlock in existingLinkBreakBlocks)
                {
                    BreakDTO inputBreak = inputBreaks.FirstOrDefault(x => x.Id == existingLinkBreakBlock.TimeScheduleTemplateBlockId);
                    if (inputBreak != null)
                    {
                        #region Update

                        int newTimeCodeBreakId = GetTimeCodeBreakIdForEmployee(date, inputBreak.TimeCodeId, employeeId);

                        existingLinkBreakBlock.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
                        existingLinkBreakBlock.TimeScheduleEmployeePeriodId = timeScheduleEmployeePeriodId;

                        //Existing break exists in input, update it
                        result = UpdateEmployeePeriodBreak(existingLinkBreakBlock, newTimeCodeBreakId, inputBreak.AccountId, date, inputBreak.StartTime, inputBreak.IsPreliminary, inputBreak.Link, logProperties, dayHasExtraShifts: dayHasExtraShifts);
                        if (!result.Success)
                            return result;

                        #endregion
                    }
                    else
                    {
                        //if you unlink hidden employee shifts the breaks will end up in different pairs, so they should not be deleted
                        if (isHidden && pairs.SelectMany(x => x.Value).FirstOrDefault(x => x.Id == existingLinkBreakBlock.TimeScheduleTemplateBlockId) != null)
                            continue;

                        #region Delete

                        //Existing break doesn't exists in input, delete it

                        result = DeleteBreakAndLogChanges(logProperties, existingLinkBreakBlock, dayHasExtraShifts);
                        if (!result.Success)
                            return result;

                        #endregion

                    }
                }

                foreach (var inputBreak in inputBreaks.Where(x => x.Id <= 0))
                {
                    #region Add

                    if (inputBreak.TimeCodeId == 0)
                        continue;

                    int newTimeCodeBreakId = GetTimeCodeBreakIdForEmployee(date, inputBreak.TimeCodeId, employeeId);

                    result = AddEmployeePeriodBreak(employeeId, timeScheduleTemplatePeriodId, timeScheduleEmployeePeriodId, timeScheduleScenarioHeadId, newTimeCodeBreakId, inputBreak.AccountId, date, inputBreak.StartTime, inputBreak.IsPreliminary, inputBreak.Link, dayHasExtraShifts, logProperties);
                    if (!result.Success)
                        return result;

                    #endregion
                }
            }

            result = Save();
            if (!result.Success)
                return result;

            return result;
        }

        /// <summary>
        /// Call this method to move or copy breaks from their current shift to destination shift         
        /// </summary>
        /// <param name="sourceScheduleBlock">Source shift/scheduleBlock</param>
        /// <param name="shift">Destination shift/scheduleBlock</param>
        /// <returns></returns>
        private ActionResult HandleBreaksDueToShiftChanges(TimeSchedulePlanningDayDTO shift, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = new ActionResult();

            if (!shift.HandleBreaks || !shift.IsSchedule())
                return new ActionResult(true);

            // Get break blocks
            List<TimeScheduleTemplateBlock> breaksToHandle = new List<TimeScheduleTemplateBlock>();
            foreach (int breakId in shift.BreaksToHandle)
            {
                breaksToHandle.Add(GetScheduleBlock(breakId));
            }

            foreach (var sourceBreakBlock in breaksToHandle)
            {
                int newTimeCodeBreakId = GetTimeCodeBreakIdForEmployee(sourceBreakBlock, shift.EmployeeId);

                if (shift.MoveBreaksWithShift)
                    result = MoveEmployeePeriodBreakToNewEmployeeOrDate(sourceBreakBlock, shift.EmployeeId, shift.TimeScheduleTemplatePeriodId, shift.TimeScheduleEmployeePeriodId, newTimeCodeBreakId, shift.ActualDate, shift.AccountId, shift.Link, logProperties);
                else if (shift.CopyBreaksWithShift)
                    result = CopyEmployeePeriodBreakToNewEmployeeOrDate(sourceBreakBlock, shift.EmployeeId, shift.TimeScheduleTemplatePeriodId, shift.TimeScheduleEmployeePeriodId, newTimeCodeBreakId, shift.ActualDate, shift.AccountId, shift.Link, logProperties);
            }

            return result;
        }

        private ActionResult MoveEmployeePeriodBreakToNewEmployeeOrDate(TimeScheduleTemplateBlock breakBlock, int newEmployeeId, int? newPeriodId, int newEmployeePeriodId, int newTimeCodeBreakId, DateTime shiftDate, int? accountId, Guid? link, ShiftHistoryLogCallStackProperties logProperties)
        {
            int oldEmployeeId = breakBlock.EmployeeId.Value;
            int? oldPeriodId = breakBlock.TimeScheduleTemplatePeriodId;
            DateTime breakStartTime = GetActualDateTime(breakBlock.StartTime, shiftDate);

            breakBlock.EmployeeId = newEmployeeId;
            breakBlock.TimeScheduleTemplatePeriodId = newPeriodId;
            breakBlock.TimeScheduleEmployeePeriodId = newEmployeePeriodId;

            return UpdateEmployeePeriodBreak(breakBlock, newTimeCodeBreakId, accountId, shiftDate, breakStartTime, breakBlock.IsPreliminary, link, logProperties, oldEmployeeId: oldEmployeeId, oldPeriodId: oldPeriodId);
        }

        private ActionResult CopyEmployeePeriodBreakToNewEmployeeOrDate(TimeScheduleTemplateBlock breakBlock, int destinationEmployeeId, int? destinationTimeScheduleTemplatePeriodId, int destinationTimeScheduleEmployeePeriodId, int destinationTimeCodeBreakId, DateTime shiftDate, int? accountId, Guid? link, ShiftHistoryLogCallStackProperties logProperties)
        {
            DateTime breakStartTime = GetActualDateTime(breakBlock.StartTime, shiftDate);

            return AddEmployeePeriodBreak(destinationEmployeeId, destinationTimeScheduleTemplatePeriodId, destinationTimeScheduleEmployeePeriodId, breakBlock.TimeScheduleScenarioHeadId, destinationTimeCodeBreakId, accountId, shiftDate, breakStartTime, breakBlock.IsPreliminary, link, false, logProperties);
        }

        private ActionResult AddEmployeePeriodBreak(int employeeId, int? timeScheduleTemplatePeriodId, int timeScheduleEmployeePeriodId, int? timeScheduleScenarioHeadId, int timeCodeId, int? accountId, DateTime shiftDate, DateTime breakStartTime, bool isPreliminary, Guid? link, bool dayHasExtraShifts, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            // Get TimeCode
            TimeCodeBreak timeCodeBreak = GetTimeCodeBreakFromCache(timeCodeId);
            if (timeCodeBreak == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

            if (breakStartTime > CalendarUtility.DATETIME_DEFAULT.AddDays(2))
                breakStartTime = CalendarUtility.GetScheduleTime(breakStartTime).AddDays((breakStartTime.Date - shiftDate.Date).Days);
            DateTime breakStopTime = breakStartTime.AddMinutes(timeCodeBreak.DefaultMinutes);

            #endregion

            TimeScheduleEmployeePeriod timeScheduleEmployeePeriod = GetTimeScheduleEmployeePeriodFromCache(timeScheduleEmployeePeriodId);
            if (timeScheduleEmployeePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleEmployeePeriod");

            #region Perform

            TimeScheduleTemplateBlock breakBlock = new TimeScheduleTemplateBlock()
            {
                Date = shiftDate,
                StartTime = breakStartTime,
                StopTime = breakStopTime,
                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak,
                Link = link.HasValue ? link.Value.ToString() : string.Empty,
                IsPreliminary = isPreliminary,
                //Set FK
                TimeCodeId = timeCodeId,
                EmployeeId = employeeId,
                TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriodId = timeScheduleEmployeePeriodId,
                TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId,
                AccountId = accountId.ToNullable(),
            };

            //Connect EmployeeSchedule
            if (!timeScheduleScenarioHeadId.HasValue)
                breakBlock.EmployeeSchedule = GetEmployeeScheduleFromCache(employeeId, shiftDate);

            entities.TimeScheduleTemplateBlock.AddObject(breakBlock);
            SetCreatedProperties(breakBlock);
            SetShiftStatus(breakBlock);
            SetShiftUserStatus(breakBlock, IsHiddenEmployeeFromCache(breakBlock.EmployeeId.Value) ? TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None : TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted);

            #region LogEntrry

            TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory()
            {
                TimeScheduleTemplateBlock = breakBlock,
                ActorCompanyId = this.actorCompanyId,
                BatchId = logProperties.BatchId.ToString(),
                Type = (int)logProperties.HistoryType,
                RecordId = logProperties.RecordId,
                IsBreak = true,
                ToStart = GetActualDateTime(breakBlock.StartTime, breakBlock.Date.Value),
                ToStop = GetActualDateTime(breakBlock.StopTime, breakBlock.Date.Value),
                ToEmployeeId = breakBlock.EmployeeId.Value,
                CurrentEmployeeId = breakBlock.EmployeeId.Value,
            };
            entities.TimeScheduleTemplateBlockHistory.AddObject(logEntry);
            SetCreatedProperties(logEntry);

            if (!breakBlock.IsPreliminary && shiftDate >= DateTime.Today && !logProperties.SkipXEMailOnChanges && !breakBlock.BelongsToPreviousDay && !breakBlock.BelongsToNextDay)
                AddCurrentSendXEMailEmployeeDate(employeeId, breakBlock.Date, TermGroup_TimeScheduleTemplateBlockType.Schedule);

            SetCurrentHasShiftUnhandledChanges(employeeId, breakBlock.Date, extra: dayHasExtraShifts);
            if (!breakBlock.TimeScheduleScenarioHeadId.HasValue)
                SetDayToBeRestoredToSchedule(employeeId, breakBlock.Date, breakBlock.TimeScheduleTemplatePeriodId);

            #endregion

            #endregion

            return result;
        }

        private ActionResult UpdateEmployeePeriodBreak(TimeScheduleTemplateBlock breakBlock, int timeCodeId, int? accountId, DateTime shiftDate, DateTime breakStartTime, bool isPreliminary, Guid? link, ShiftHistoryLogCallStackProperties logProperties, bool dayHasExtraShifts = false, int? oldEmployeeId = null, int? oldPeriodId = null)
        {
            ActionResult result = new ActionResult();

            if (breakBlock == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            if (!breakBlock.Date.HasValue)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8849, "Otillräckliga rastuppgifter") + ": " + GetText(8850, "Datum saknas"));

            if (!breakBlock.EmployeeId.HasValue)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8849, "Otillräckliga rastuppgifter") + ": " + GetText(8851, "Anställd saknas"));

            #region Prereq

            // Get TimeCode
            TimeCodeBreak timeCodeBreak = GetTimeCodeBreakFromCache(timeCodeId);
            if (timeCodeBreak == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

            if (breakStartTime > CalendarUtility.DATETIME_DEFAULT.AddDays(2))
                breakStartTime = CalendarUtility.GetScheduleTime(breakStartTime).AddDays((breakStartTime.Date - shiftDate.Date).Days);
            DateTime breakStopTime = breakStartTime.AddMinutes(timeCodeBreak.DefaultMinutes);

            #endregion

            bool dateHasChanged = shiftDate != breakBlock.Date;
            bool employeeHasChanged = oldEmployeeId.HasValue && oldEmployeeId.Value != breakBlock.EmployeeId.Value;

            #region LogEntry

            bool logChanges = (dateHasChanged) || (breakStartTime != breakBlock.StartTime) || (breakStopTime != breakBlock.StopTime) || (employeeHasChanged);
            if (logChanges)
            {
                TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory()
                {
                    TimeScheduleTemplateBlock = breakBlock,
                    ActorCompanyId = this.actorCompanyId,
                    BatchId = logProperties.BatchId.ToString(),
                    Type = (int)logProperties.HistoryType,
                    IsBreak = true,
                    RecordId = logProperties.RecordId,
                    FromStart = GetActualDateTime(breakBlock.StartTime, breakBlock.Date.Value),
                    ToStart = GetActualDateTime(breakStartTime, shiftDate),
                    FromStop = GetActualDateTime(breakBlock.StopTime, breakBlock.Date.Value),
                    ToStop = GetActualDateTime(breakStopTime, shiftDate),
                    FromEmployeeId = oldEmployeeId ?? breakBlock.EmployeeId.Value,
                    ToEmployeeId = breakBlock.EmployeeId.Value,
                    CurrentEmployeeId = breakBlock.EmployeeId.Value,
                };
                entities.TimeScheduleTemplateBlockHistory.AddObject(logEntry);
                SetCreatedProperties(logEntry);

                if (!breakBlock.IsPreliminary && breakBlock.Date.Value >= DateTime.Today && shiftDate >= DateTime.Today && !logProperties.SkipXEMailOnChanges && !breakBlock.BelongsToPreviousDay && !breakBlock.BelongsToNextDay)
                {
                    AddCurrentSendXEMailEmployeeDate(logEntry.FromEmployeeId, breakBlock.Date, TermGroup_TimeScheduleTemplateBlockType.Schedule);
                    AddCurrentSendXEMailEmployeeDate(logEntry.ToEmployeeId, shiftDate, TermGroup_TimeScheduleTemplateBlockType.Schedule);
                }

                SetCurrentHasShiftUnhandledChanges(logEntry.FromEmployeeId, breakBlock.Date, extra: dayHasExtraShifts);
                SetCurrentHasShiftUnhandledChanges(logEntry.ToEmployeeId, shiftDate, extra: dayHasExtraShifts);
                if (!breakBlock.TimeScheduleScenarioHeadId.HasValue)
                {
                    SetDayToBeRestoredToSchedule(logEntry.FromEmployeeId, breakBlock.Date, oldPeriodId);
                    if (dateHasChanged || employeeHasChanged)
                        SetDayToBeRestoredToSchedule(logEntry.ToEmployeeId, shiftDate, breakBlock.TimeScheduleTemplatePeriodId);
                }
            }

            #endregion

            #region Perform

            breakBlock.Date = shiftDate;
            breakBlock.StartTime = breakStartTime;
            breakBlock.StopTime = breakStopTime;
            breakBlock.TimeCodeId = timeCodeId;
            breakBlock.ShiftType = null;
            breakBlock.IsPreliminary = isPreliminary;
            breakBlock.Link = link.HasValue ? link.Value.ToString() : string.Empty;
            breakBlock.AccountId = accountId.ToNullable();

            //Connect EmployeeSchedule
            if ((dateHasChanged || employeeHasChanged) && !breakBlock.TimeScheduleScenarioHeadId.HasValue)
                breakBlock.EmployeeSchedule = GetEmployeeScheduleFromCache(breakBlock.EmployeeId.Value, shiftDate);

            SetModifiedProperties(breakBlock);
            SetShiftStatus(breakBlock);
            SetShiftUserStatus(breakBlock, IsHiddenEmployeeFromCache(breakBlock.EmployeeId.Value) ? TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None : TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted);

            #endregion

            return result;
        }

        private ActionResult DeleteInvalidBreaks(TimeScheduleTemplateBlock scheduleBlock, ShiftHistoryLogCallStackProperties logPropertis, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();
            if (!scheduleBlock.IsSchedule())
                return result;

            // Get User
            User user = GetUserFromCache();
            if (user == null)
                return new ActionResult((int)ActionResultDelete.TimeSchedulePlanning_UserNotFound);


            var employeePeriod = (from per in entities.TimeScheduleEmployeePeriod
                                  where per.TimeScheduleEmployeePeriodId == scheduleBlock.TimeScheduleEmployeePeriodId &&
                             per.State == (int)SoeEntityState.Active
                                  select per).FirstOrDefault();

            if (employeePeriod != null)
            {
                List<TimeScheduleTemplateBlock> allBreaks = GetTimeScheduleShiftBreaks(scheduleBlock.TimeScheduleScenarioHeadId, employeePeriod.TimeScheduleEmployeePeriodId, null).ToList();
                List<TimeScheduleTemplateBlock> shifts = GetTimeScheduleShifts(scheduleBlock.TimeScheduleScenarioHeadId, employeePeriod.TimeScheduleEmployeePeriodId, null).Where(x => x.TimeScheduleTemplateBlockId != scheduleBlock.TimeScheduleTemplateBlockId).ToList();

                foreach (var scheduledBreak in allBreaks)
                {
                    bool breakIsAlone = false;

                    var startShift = shifts.FirstOrDefault(x => scheduledBreak.ActualStartTime >= x.ActualStartTime && scheduledBreak.ActualStartTime <= x.ActualStopTime);
                    var endShift = shifts.FirstOrDefault(x => scheduledBreak.ActualStopTime >= x.ActualStartTime && scheduledBreak.ActualStopTime <= x.ActualStopTime);
                    if (startShift == null && endShift == null)
                        breakIsAlone = true;

                    if (breakIsAlone)
                    {
                        ChangeEntityState(scheduledBreak, SoeEntityState.Deleted, user);

                        if (logPropertis != null)
                            logPropertis.DeletedBreaks.Add(new DeletedBreakLogData(scheduledBreak.StartTime, scheduledBreak.StopTime, scheduledBreak));
                    }
                }

            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult DeleteOverlappedBreaks(TimeScheduleTemplateBlock scheduleBlock, ShiftHistoryLogCallStackProperties logPropertis, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();
            if (!scheduleBlock.IsSchedule())
                return result;

            // Get User
            User user = GetUserFromCache();
            if (user == null)
                return new ActionResult((int)ActionResultDelete.TimeSchedulePlanning_UserNotFound);

            if (scheduleBlock.TimeScheduleEmployeePeriodId.HasValue)
            {
                // If hidden employee, only get breaks connected to current shift
                Guid? link = null;
                if (IsHiddenEmployeeFromCache(scheduleBlock.EmployeeId.Value) && !String.IsNullOrEmpty(scheduleBlock.Link))
                    link = new Guid(scheduleBlock.Link);

                List<TimeScheduleTemplateBlock> allBreaks = GetTimeScheduleShiftBreaks(scheduleBlock.TimeScheduleScenarioHeadId, scheduleBlock.TimeScheduleEmployeePeriodId.Value, link).ToList();

                bool otherShiftExist = ExistsOtherNonZeroShifts(scheduleBlock);

                if (otherShiftExist)
                {
                    List<TimeScheduleTemplateBlock> overlappedBreaks = scheduleBlock.GetOverlappedBreaks(allBreaks);

                    foreach (var overlappedBreak in overlappedBreaks)
                    {
                        ChangeEntityState(overlappedBreak, SoeEntityState.Deleted, user);

                        if (logPropertis != null)
                            logPropertis.DeletedBreaks.Add(new DeletedBreakLogData(overlappedBreak.StartTime, overlappedBreak.StopTime, overlappedBreak));
                    }
                }
                else
                {
                    foreach (var scheduledBreak in allBreaks)
                    {
                        ChangeEntityState(scheduledBreak, SoeEntityState.Deleted, user);

                        if (logPropertis != null)
                            logPropertis.DeletedBreaks.Add(new DeletedBreakLogData(scheduledBreak.StartTime, scheduledBreak.StopTime, scheduledBreak));
                    }
                }
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult DeleteLonelyBreaks(int? timeScheduleScenarioHeadId, int employeeId, DateTime date)
        {
            // Get User
            User user = GetUserFromCache();
            if (user == null)
                return new ActionResult((int)ActionResultDelete.TimeSchedulePlanning_UserNotFound);

            var templateBlocksForEmployee = (from tb in entities.TimeScheduleTemplateBlock
                                             where tb.EmployeeId.HasValue &&
                                             tb.EmployeeId.Value == employeeId &&
                                             tb.Date.HasValue &&
                                             tb.Date.Value == date &&
                                             tb.StartTime != tb.StopTime &&
                                             tb.State == (int)SoeEntityState.Active
                                             select tb).ToList();

            templateBlocksForEmployee = templateBlocksForEmployee.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();

            // set breaks to deleted only if it doesn't exists scheduleblocks
            if (templateBlocksForEmployee.All(x => x.BreakType > (int)SoeTimeScheduleTemplateBlockBreakType.None))
            {
                foreach (var breakBlock in templateBlocksForEmployee)
                {
                    ChangeEntityState(breakBlock, SoeEntityState.Deleted, user);
                }
            }

            return Save();
        }

        private ActionResult DeleteBreakAndLogChanges(ShiftHistoryLogCallStackProperties logProperties, TimeScheduleTemplateBlock existingLinkBreakBlock, bool dayHasExtraShifts)
        {
            if (existingLinkBreakBlock == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");
            if (!existingLinkBreakBlock.Date.HasValue)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8849, "Otillräckliga rastuppgifter") + ": " + GetText(8850, "Datum saknas"));
            if (!existingLinkBreakBlock.EmployeeId.HasValue)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8849, "Otillräckliga rastuppgifter") + ": " + GetText(8851, "Anställd saknas"));

            //Delete
            ActionResult result = ChangeEntityState(existingLinkBreakBlock, SoeEntityState.Deleted);
            if (!result.Success)
                return result;

            SetModifiedProperties(existingLinkBreakBlock);

            //Log
            TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory()
            {
                TimeScheduleTemplateBlock = existingLinkBreakBlock,
                ActorCompanyId = this.actorCompanyId,
                BatchId = logProperties.BatchId.ToString(),
                Type = (int)logProperties.HistoryType,
                RecordId = logProperties.RecordId,
                IsBreak = true,
                FromStart = existingLinkBreakBlock.ActualStartTime,
                //same date but zero time
                ToStart = existingLinkBreakBlock.ActualStartTime.Value.Date,
                FromStop = existingLinkBreakBlock.ActualStopTime,
                //same date but zero time
                ToStop = existingLinkBreakBlock.ActualStopTime.Value.Date,
                FromEmployeeId = existingLinkBreakBlock.EmployeeId.Value,
                ToEmployeeId = existingLinkBreakBlock.EmployeeId.Value,
                CurrentEmployeeId = existingLinkBreakBlock.EmployeeId.Value,
            };
            entities.TimeScheduleTemplateBlockHistory.AddObject(logEntry);
            SetCreatedProperties(logEntry);

            if (!existingLinkBreakBlock.IsPreliminary && existingLinkBreakBlock.Date.Value >= DateTime.Today && !logProperties.SkipXEMailOnChanges && !existingLinkBreakBlock.BelongsToPreviousDay && !existingLinkBreakBlock.BelongsToNextDay)
                AddCurrentSendXEMailEmployeeDate(existingLinkBreakBlock.EmployeeId, existingLinkBreakBlock.Date, TermGroup_TimeScheduleTemplateBlockType.Schedule);

            SetCurrentHasShiftUnhandledChanges(existingLinkBreakBlock.EmployeeId, existingLinkBreakBlock.Date, extra: dayHasExtraShifts);
            if (!existingLinkBreakBlock.TimeScheduleScenarioHeadId.HasValue)
                SetDayToBeRestoredToSchedule(existingLinkBreakBlock.EmployeeId, existingLinkBreakBlock.Date, existingLinkBreakBlock.TimeScheduleTemplatePeriodId);

            return new ActionResult(true);
        }

        private void AddBreaks(TimeSchedulePlanningDayDTO shift, List<BreakDTO> breakItems, int? newEmployeeId, int oldEmployeeId, Guid link)
        {
            if (shift == null || breakItems.IsNullOrEmpty())
                return;

            DateTime date = shift.ActualDate.Date;

            if (newEmployeeId.HasValue)
            {
                foreach (var breakItem in breakItems)
                {
                    breakItem.TimeCodeId = GetTimeCodeBreakIdForEmployee(date, breakItem.TimeCodeId, newEmployeeId.Value, oldEmployeeId);
                }
            }

            shift.AddNewBreaks(breakItems, link, date);
        }

        /// <summary>Gather unique breaks and update breaks on all shifts (on current daynumber) </summary>
        private void FixBreaks(List<TimeSchedulePlanningDayDTO> targetTemplateShifts, int? dayNumber = null, DateTime? date = null)
        {
            if (!targetTemplateShifts.Any())
                return;
            if (IsHiddenEmployeeFromCache(targetTemplateShifts.First().EmployeeId))
                return;

            List<TimeSchedulePlanningDayDTO> targetShifts = new List<TimeSchedulePlanningDayDTO>();
            if (dayNumber.HasValue)
                targetShifts = targetTemplateShifts.Where(x => x.DayNumber == dayNumber.Value).ToList();
            else if (date.HasValue)
                targetShifts = targetTemplateShifts.Where(x => x.ActualDate == date.Value).ToList();

            if (targetShifts.IsNullOrEmpty())
                return;

            List<BreakDTO> uniqueTargetBreaks = new List<BreakDTO>();

            foreach (TimeSchedulePlanningDayDTO targetShift in targetShifts)
            {
                foreach (BreakDTO breakItem in targetShift.GetBreaks())
                {
                    if (!uniqueTargetBreaks.Any(x => x.StartTime == breakItem.StartTime))
                        uniqueTargetBreaks.Add(breakItem);
                }
            }

            uniqueTargetBreaks = uniqueTargetBreaks.OrderBy(x => x.StartTime).ToList();

            foreach (var targetShift in targetShifts)
            {
                targetShift.ResetBreaks();
                targetShift.AddBreaks(uniqueTargetBreaks);
            }
        }

        #endregion

        #region Schedule - preliminary

        private void SetTimePayrollTransactionStaffingFromTimeBlock(TimePayrollTransaction timePayrollTransaction, TimeBlock timeBlock)
        {
            if (!UseStaffing())
                return;
            if (timePayrollTransaction == null || timeBlock == null)
                return;

            SetTimePayrollTransactionToPreliminary(timePayrollTransaction, timeBlock.IsPreliminary);
        }

        private void SetTimeBlocksToPreliminary(List<TimeBlock> timeBlocks, bool preliminary = true)
        {
            if (timeBlocks == null || !UseStaffing())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                SetTimeBlockToPreliminary(timeBlock, preliminary);
            }
        }

        private void SetTimeBlockToPreliminary(TimeBlock timeBlock, bool preliminary = true)
        {
            if (timeBlock == null || !UseStaffing())
                return;

            if (timeBlock.IsPreliminary != preliminary)
            {
                timeBlock.IsPreliminary = preliminary;
                if (timeBlock.TimeBlockId == 0)
                    SetModifiedProperties(timeBlock);
            }
        }

        private void SetTimePayrollTransactionsToPreliminary(List<TimePayrollTransaction> timePayrollTransactions, bool preliminary = true)
        {
            if (!UseStaffing())
                return;
            if (timePayrollTransactions == null)
                return;

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {
                SetTimePayrollTransactionToPreliminary(timePayrollTransaction, preliminary);
            }
        }

        private void SetTimePayrollTransactionToPreliminary(TimePayrollTransaction timePayrollTransaction, bool preliminary = true)
        {
            if (!UseStaffing())
                return;
            if (timePayrollTransaction == null)
                return;

            if (timePayrollTransaction.IsPreliminary != preliminary)
            {
                timePayrollTransaction.IsPreliminary = preliminary;
                if (timePayrollTransaction.TimePayrollTransactionId == 0)
                    SetModifiedProperties(timePayrollTransaction);
            }
        }

        #endregion

        #region Schedule - shift

        private List<TimeScheduleTemplateBlock> GetTimeScheduleShifts(int? timeScheduleScenarioHeadId, int timeScheduleEmployeePeriodId, Guid? link = null)
        {
            IQueryable<TimeScheduleTemplateBlock> query = (from tb in entities.TimeScheduleTemplateBlock
                                                           where tb.TimeScheduleEmployeePeriodId == timeScheduleEmployeePeriodId &&
                                                           tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                           tb.Date.HasValue &&
                                                           tb.State == (int)SoeEntityState.Active
                                                           orderby tb.StartTime
                                                           select tb);

            if (link.HasValue)
            {
                string linkStr = link.Value.ToString();
                query = query.Where(b => b.Link.Equals(linkStr));
            }

            var result = query.ToList();
            result = result.FilterScenario(timeScheduleScenarioHeadId);
            return result;
        }

        private List<TimeSchedulePlanningDayDTO> GetAbsenceDividedShifts(TimeSchedulePlanningDayDTO dto)
        {
            bool shiftEndsAfterMidnight = (dto.StopTime.Date > dto.StartTime.Date);
            bool shiftStartsBeforeMidnight = (dto.StartTime.Date < dto.StopTime.Date);

            List<TimeSchedulePlanningDayDTO> newShifts = new List<TimeSchedulePlanningDayDTO>();

            if (dto.StartTime == dto.StopTime)
                return newShifts;

            if (dto.StartTime < dto.AbsenceStartTime)
            {
                // Copy DTO
                TimeSchedulePlanningDayDTO clone = new TimeSchedulePlanningDayDTO();
                EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(clone, dto);
                clone.TimeScheduleTemplateBlockId = 0;

                clone.StartTime = dto.StartTime;
                clone.StopTime = dto.AbsenceStartTime;
                newShifts.Add(clone);
            }

            if (dto.StopTime > dto.AbsenceStopTime)
            {
                // Copy DTO
                TimeSchedulePlanningDayDTO clone = new TimeSchedulePlanningDayDTO();
                EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(clone, dto);
                clone.TimeScheduleTemplateBlockId = 0;

                clone.StartTime = dto.AbsenceStopTime;
                clone.StopTime = dto.StopTime;
                newShifts.Add(clone);
            }

            if (shiftEndsAfterMidnight)
            {
                foreach (var newShift in newShifts)
                {
                    if (newShift.StartTime.Date == dto.StopTime.Date)
                        newShift.BelongsToPreviousDay = true;
                }
            }
            else if (shiftStartsBeforeMidnight)
            {
                foreach (var newShift in newShifts)
                {
                    if (newShift.StartTime.Date == dto.StopTime.Date)
                        newShift.BelongsToNextDay = true;
                }
            }

            return newShifts;
        }

        private List<TimeSchedulePlanningDayDTO> AdjustShiftsToBeSaved(List<TimeSchedulePlanningDayDTO> newShifts, List<TimeSchedulePlanningDayDTO> existingShifts)
        {
            List<TimeSchedulePlanningDayDTO> adjustedShifts = new List<TimeSchedulePlanningDayDTO>();

            newShifts.SetUniqueId();
            existingShifts.SetUniqueId();

            // check if all shifts are removed
            if (newShifts.IsNullOrEmpty())
            {
                if (existingShifts.Count > 0)
                {
                    foreach (TimeSchedulePlanningDayDTO existingShift in existingShifts)
                    {
                        existingShift.SetAsDeleted();
                        adjustedShifts.Add(existingShift);
                    }
                }
            }
            else
            {
                foreach (var newShiftsByEmployeeGroup in newShifts.GroupBy(x => x.EmployeeId))
                {
                    foreach (var newShiftsByDateGroup in newShiftsByEmployeeGroup.GroupBy(x => x.ActualDate))
                    {
                        #region Validate changes

                        bool validationCompleted = false;
                        Dictionary<string, TimeSchedulePlanningDayDTO> shiftMapping = new Dictionary<string, TimeSchedulePlanningDayDTO>();
                        List<TimeSchedulePlanningDayDTO> removedExistingShifts = new List<TimeSchedulePlanningDayDTO>();

                        //New
                        List<TimeSchedulePlanningDayDTO> newShiftsByDate = newShiftsByDateGroup.OrderBy(i => i.StartTime).ToList();

                        //Existing
                        List<TimeSchedulePlanningDayDTO> existingShiftsByDate = existingShifts.Where(i => i.ActualDate == newShiftsByDateGroup.Key).OrderBy(i => i.StartTime).ToList();
                        bool isPreliminary = existingShiftsByDate.Any(i => i.IsPreliminary); //If any existing is prel, set all as prel

                        int nrOfNewShifts = newShiftsByDate.Count;
                        int nrOfExistingShifts = existingShiftsByDate.Count;

                        if (nrOfNewShifts == nrOfExistingShifts)
                        {
                            #region Same number of shifts

                            for (int i = 0; i < newShiftsByDate.Count; i++)
                            {
                                TimeSchedulePlanningDayDTO newShift = newShiftsByDate[i];
                                TimeSchedulePlanningDayDTO existingShift = existingShiftsByDate[i];
                                shiftMapping.Add(newShift.UniqueId, existingShift);
                            }

                            validationCompleted = true;

                            #endregion
                        }
                        else
                        {
                            #region Different nummber of shifts

                            foreach (TimeSchedulePlanningDayDTO newShift in newShiftsByDate)
                            {
                                List<string> existingUniqueIdsMapped = shiftMapping.Values.Select(i => i.UniqueId).ToList();
                                TimeSchedulePlanningDayDTO existingShift = existingShiftsByDate.FirstOrDefault(e => e.IsConsideredToBeSame(newShift, existingUniqueIdsMapped));
                                if (existingShift != null)
                                    shiftMapping.Add(newShift.UniqueId, existingShift);
                            }

                            int nrOfUnmappedShifts = nrOfNewShifts - shiftMapping.Count;

                            if (nrOfNewShifts > nrOfExistingShifts)
                            {
                                #region Added new shift

                                int nrOfAddedShifts = nrOfNewShifts - nrOfExistingShifts;
                                if (nrOfAddedShifts == nrOfUnmappedShifts)
                                {
                                    validationCompleted = true;
                                }

                                #endregion
                            }
                            else
                            {
                                #region Removed existing shifts

                                int nrOfRemovedShifts = nrOfExistingShifts - nrOfNewShifts;
                                if (nrOfRemovedShifts > 0)
                                {
                                    validationCompleted = true;

                                    List<TimeSchedulePlanningDayDTO> existingMappedShifts = shiftMapping.Values.ToList();
                                    foreach (TimeSchedulePlanningDayDTO existingShift in existingShiftsByDate)
                                    {
                                        if (!existingMappedShifts.Any(i => i.UniqueId == existingShift.UniqueId))
                                            removedExistingShifts.Add(existingShift);
                                    }
                                }

                                #endregion
                            }

                            #endregion
                        }

                        #endregion

                        #region Adjust changes

                        if (validationCompleted)
                        {
                            #region Shifts mapped - update existing shifts

                            foreach (TimeSchedulePlanningDayDTO newShift in newShiftsByDateGroup.OrderBy(i => i.StartTime))
                            {
                                if (shiftMapping.ContainsKey(newShift.UniqueId))
                                {
                                    TimeSchedulePlanningDayDTO existingShift = shiftMapping[newShift.UniqueId];
                                    newShift.TimeScheduleTemplateBlockId = existingShift.TimeScheduleTemplateBlockId;

                                    if (newShift.Break1Minutes > 0)
                                        newShift.Break1Id = existingShift.Break1Id;
                                    if (newShift.Break2Minutes > 0)
                                        newShift.Break2Id = existingShift.Break2Id;
                                    if (newShift.Break3Minutes > 0)
                                        newShift.Break3Id = existingShift.Break3Id;
                                    if (newShift.Break4Minutes > 0)
                                        newShift.Break4Id = existingShift.Break4Id;
                                }
                                else
                                {
                                    newShift.ClearIds();
                                }

                                if (isPreliminary)
                                    newShift.IsPreliminary = true;

                                adjustedShifts.Add(newShift);
                            }

                            foreach (TimeSchedulePlanningDayDTO removedExistingShift in removedExistingShifts)
                            {
                                removedExistingShift.ClearTime();
                                adjustedShifts.Add(removedExistingShift);
                            }

                            #endregion
                        }
                        else
                        {
                            #region Shifts not mapped - set existing as deleted and add new shifts (disapprove mappings)

                            foreach (TimeSchedulePlanningDayDTO existingShift in existingShiftsByDate)
                            {
                                existingShift.ClearTime();
                                adjustedShifts.Add(existingShift);
                            }

                            foreach (TimeSchedulePlanningDayDTO newShift in newShiftsByDateGroup)
                            {
                                if (isPreliminary)
                                    newShift.IsPreliminary = true;

                                newShift.ClearIds();
                                adjustedShifts.Add(newShift);
                            }

                            #endregion
                        }

                        #endregion
                    }
                }
            }
            return adjustedShifts;
        }

        private ActionResult InitiateScheduleSwap(int initiatorEmployeeId, DateTime initiatorShiftDate, List<int> initiatorShiftIds, int swapWithEmployeeId, DateTime swapWithShiftDate, List<int> swapWithShiftIds, string comment)
        {
            ActionResult result = new ActionResult();
            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();
            string errorMessage = string.Empty;

            if (initiatorEmployeeId == swapWithEmployeeId)
            {
                errorMessage = GetText(8859, "Du kan inte byta pass med dig själv");
                return new ActionResult((int)ActionResultSave.EntityExists, errorMessage);
            }
            Employee initiatorEmployee = GetEmployeeFromCache(initiatorEmployeeId);
            Employee swapWithEmployee = GetEmployeeFromCache(swapWithEmployeeId);

            if (initiatorEmployee == null || !initiatorEmployee.UserId.HasValue || swapWithEmployee == null)
                return new ActionResult((int)ActionResultSave.InsufficientInput);

            User user = GetUserFromCache();
            if (user == null)
                return new ActionResult((int)ActionResultSave.InsufficientInput);

            if (swapWithShiftDate < DateTime.Today || initiatorShiftDate < DateTime.Today)
            {
                errorMessage = GetText(8858, "Det går inte att byta pass bakåt i tiden");
                return new ActionResult((int)ActionResultSave.EntityExists, errorMessage);
            }
            List<TimeSchedulePlanningDayDTO> initiatorShifts = GetScheduleBlocksForEmployee(null, initiatorEmployee.EmployeeId, initiatorShiftDate, initiatorShiftDate, loadStaffingIfUsed: false, includeStandBy: true, loadShiftType: true, loadScheduleType: true, loadDeviationCause: true).ToTimeSchedulePlanningDayDTOs(initiatorEmployee.Hidden)
                .Where(x => initiatorShiftIds.Contains(x.TimeScheduleTemplateBlockId))
                .ToList();
            TimeScheduleManager.SetSwapShiftInfo(initiatorShifts);

            List<TimeSchedulePlanningDayDTO> swapWithShifts = GetScheduleBlocksForEmployee(null, swapWithEmployee.EmployeeId, swapWithShiftDate, swapWithShiftDate, loadStaffingIfUsed: false, includeStandBy: true, loadShiftType: true, loadScheduleType: true, loadDeviationCause: true).ToTimeSchedulePlanningDayDTOs(swapWithEmployee.Hidden)
            .Where(x => swapWithShiftIds.Contains(x.TimeScheduleTemplateBlockId))
            .ToList();
            TimeScheduleManager.SetSwapShiftInfo(swapWithShifts);

            shifts.AddRange(initiatorShifts);
            shifts.AddRange(swapWithShifts);

            bool swapStarted = TimeScheduleManager.EmployeesSwapRequestExistsOnAnyShift(this.entities, this.actorCompanyId, new List<int> { initiatorEmployeeId, swapWithEmployeeId }, shifts);

            if (swapStarted)
            {
                errorMessage = GetText(8855, "Det finns redan ett pågående passbyte på något av de valda passen.");
                return new ActionResult((int)ActionResultSave.EntityExists, errorMessage);
            }
            TimeScheduleSwapRequest swapRequest = new TimeScheduleSwapRequest()
            {
                InitiatorEmployeeId = initiatorEmployee.EmployeeId,
                InitiatorUserId = user.UserId,
                ActorCompanyId = actorCompanyId,
                Comment = comment,
                Status = (int)TermGroup_TimeScheduleSwapRequestStatus.Initiated,
                State = (int)SoeEntityState.Active,
                InitiatedDate = DateTime.Now
            };

            SetCreatedProperties(swapRequest);
            entities.AddToTimeScheduleSwapRequest(swapRequest);

            foreach (var initiatorShift in initiatorShifts)
            {
                CreateTimeScheduleSwapRequestRow(initiatorShift, swapRequest, TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByEmployee);
            }

            foreach (var swapWithShift in swapWithShifts)
            {
                CreateTimeScheduleSwapRequestRow(swapWithShift, swapRequest, TermGroup_TimeScheduleSwapRequestRowStatus.Initiated);
            }

            if (result.Success)
                result = Save();

            if (swapRequest.InitiatorEmployeeId.HasValue)
                SendXEMailOnInitiateScheduleSwapByEmployee(swapRequest);

            return result;
        }

        private ActionResult ApproveScheduleSwap(int userId, int timeScheduleSwapRequestId, bool approved, string comment)
        {
            ActionResult result;
            bool allRowsApproved = false;
            bool allRowsDenied = false;
            bool userIsAdmin = false;
            Employee employeeUser = EmployeeManager.GetEmployeeByUser(this.entities, actorCompanyId, userId);
            User user = GetUser(userId);

            if (user == null || timeScheduleSwapRequestId < 0)
                return new ActionResult((int)ActionResultSave.InsufficientInput);

            // Get shift swap request
            TimeScheduleSwapRequest timeScheduleSwapRequest = GetTimeScheduleSwapRequest(timeScheduleSwapRequestId);
            if (timeScheduleSwapRequest == null)
                return new ActionResult((int)ActionResultSelect.EntityNotFound);

            // No employee connected to user 
            // or
            // Employee exist to user but is not involved in schedule swap
            if (employeeUser == null || !timeScheduleSwapRequest.TimeScheduleSwapRequestRow.Any(e => e.EmployeeId == employeeUser.EmployeeId && e.State == (int)SoeEntityState.Active))
                userIsAdmin = true;

            #region Update status

            if (userIsAdmin)
            {
                result = ApproveScheduleSwapRequestByAdmin(timeScheduleSwapRequest, approved, out allRowsApproved, out allRowsDenied);

                if (result.Success && (allRowsApproved || allRowsDenied))
                {
                    #region Send E-mail

                    SendXEMailOnApproveScheduleSwapRequestByAdmin(timeScheduleSwapRequest, approved, comment);

                    #endregion
                }
            }
            else
            {
                bool isInitiator = timeScheduleSwapRequest.InitiatorEmployeeId == employeeUser.EmployeeId;
                // Approve schedule swap by employee
                result = ApproveScheduleSwapRequestByEmployee(timeScheduleSwapRequest, employeeUser.EmployeeId, approved, isInitiator);

                if (result.Success)
                {
                    #region Send E-mail

                    SendXEMailOnApproveScheduleSwapRequestByEmployee(timeScheduleSwapRequest, approved, isInitiator);

                    #endregion
                }
            }

            #endregion

            return result;
        }

        private ActionResult SaveTimeScheduleShifts(TermGroup_ShiftHistoryType type, List<TimeSchedulePlanningDayDTO> shifts, bool updateBreaks, bool skipXEMailOnChanges, bool adjustTasks, int minutesMoved, int? timeScheduleScenarioHeadId, Guid? batchId = null, bool updateScheduledTimeSummary = true, List<EmployeeGroup> employeeGroups = null)
        {
            #region Prereq

            if (shifts == null)
                return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_ShiftIsNull);

            ActionResult result = ValidateShiftsAgainstGivenScenario(shifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            Dictionary<string, int> strDict = new Dictionary<string, int>();
            List<TimeBlockDate> stampingTimeBlockDates = new List<TimeBlockDate>();
            if (!batchId.HasValue)
                batchId = GetNewBatchLink();

            bool useAccountHierarchy = UseAccountHierarchy();

            if (employeeGroups == null)
                employeeGroups = GetEmployeeGroupsFromCache();

            #endregion

            foreach (var shiftsByEmployee in shifts.GroupBy(x => x.EmployeeId))
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(shiftsByEmployee.Key);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

                if (!type.IsSaveTimeScheduleShift())
                {
                    //Pre cache
                    AddTimeScheduleTemplatePeriodsToCache(employee.EmployeeId, shiftsByEmployee.OrderBy(x => x.ActualDate).First().ActualDate, shiftsByEmployee.OrderBy(x => x.ActualDate).Last().ActualDate);
                    AddTimeScheduleEmployeePeriodsToCache(employee.EmployeeId, shiftsByEmployee.OrderBy(x => x.ActualDate).First().ActualDate, shiftsByEmployee.OrderBy(x => x.ActualDate).Last().ActualDate);
                }

                foreach (var shiftsByEmployeeAndDate in shiftsByEmployee.GroupBy(x => x.ActualDate))
                {
                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(shiftsByEmployeeAndDate.Key, employeeGroups: employeeGroups);
                    if (employeeGroup == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8539, "Tidavtal hittades inte"));

                    bool deleteLonelyBreaks = false;

                    #region Validation

                    // Only do break validation on schedule
                    if (shiftsByEmployeeAndDate.Any(s => s.IsSchedule()))
                    {
                        result = ValidateBreakWindow(shiftsByEmployeeAndDate.ToList());
                        if (!result.Success)
                            return result;
                    }

                    #endregion

                    #region Save shifts

                    foreach (var shift in shiftsByEmployeeAndDate)
                    {
                        #region Prereq

                        if ((!shift.AccountId.HasValue || shift.AccountId == 0) && !shift.IsZeroShift() && useAccountHierarchy && !employeeGroup.AllowShiftsWithoutAccount)
                        {
                            int defaultDimId = GetCompanyIntSettingFromCache(CompanySettingType.DefaultEmployeeAccountDimEmployee);
                            AccountDim accountDim = GetAccountDim(defaultDimId);
                            return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(9314, "Du måste ange {0} på alla pass."), accountDim != null ? accountDim.Name : "") + " " + string.Format(GetText(8340, "Anställd {0}, {1}."), employee.EmployeeNr, shiftsByEmployeeAndDate.Key.ToShortDateString()));
                        }

                        #endregion

                        #region Prepare for logging

                        ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId.Value, shift.TimeScheduleTemplateBlockId, type, null, skipXEMailOnChanges);

                        #endregion

                        #region Perform

                        TimeScheduleTemplateBlock originalTemplateBlock = GetScheduleBlockWithPeriodAndStaffing(shift.TimeScheduleTemplateBlockId);
                        DateTime? originalDate = originalTemplateBlock != null && originalTemplateBlock.Date.HasValue ? originalTemplateBlock.Date.Value : (DateTime?)null;
                        int? employeeId = originalTemplateBlock?.EmployeeId;

                        result = SaveTimeScheduleShiftAndLogChanges(shift, logProperties, updateScheduledTimeSummary: updateScheduledTimeSummary, employeeGroups: employeeGroups);
                        if (!result.Success)
                            return result;

                        if (adjustTasks)
                        {
                            if (shift.Tasks.IsNullOrEmpty())
                                shift.Tasks = GetTimeScheduleTemplateBlockTasks(shift.TimeScheduleTemplateBlockId).ToDTOs().ToList();

                            foreach (TimeScheduleTemplateBlockTaskDTO task in shift.Tasks)
                            {
                                task.StartTime = task.StartTime.AddMinutes(minutesMoved);
                                task.StopTime = task.StopTime.AddMinutes(minutesMoved);
                            }
                        }

                        if (shift.Tasks != null)
                        {
                            result = SaveTimeScheduleTemplateBlockTasks(shift.Tasks);
                            if (!result.Success)
                                return result;
                        }

                        //Item 13976
                        if (employeeId.HasValue && originalDate.HasValue && originalDate.Value.Date != shift.ActualDate)
                            deleteLonelyBreaks = true;

                        if (!string.IsNullOrEmpty(shift.UniqueId))
                            strDict.Add(shift.UniqueId, shift.TimeScheduleTemplateBlockId); // Used to identify the appointment in the schedule

                        #endregion
                    }

                    #endregion

                    #region Save breaks

                    if (result.Success && updateBreaks)
                    {
                        TimeSchedulePlanningDayDTO firstShift = shiftsByEmployeeAndDate.First();

                        result = SaveEmployeePeriodBreaks(
                                    shiftsByEmployee.Key,
                                    shiftsByEmployeeAndDate.Key,
                                    firstShift.TimeScheduleTemplatePeriodId,
                                    firstShift.TimeScheduleEmployeePeriodId,
                                    firstShift.TimeScheduleScenarioHeadId,
                                    shiftsByEmployeeAndDate.ToList(),
                                    new ShiftHistoryLogCallStackProperties(batchId.Value, 0, type, null, skipXEMailOnChanges));
                    }

                    #endregion

                    #region Lonely breaks

                    if (deleteLonelyBreaks)
                    {
                        result = DeleteLonelyBreaks(timeScheduleScenarioHeadId, shiftsByEmployee.Key, shiftsByEmployeeAndDate.Key);
                        if (!result.Success)
                            return result;
                    }

                    #endregion

                    #region Stamping

                    if (!employeeGroup.AutogenTimeblocks && !type.IsCreatingScenario())
                    {
                        TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(shiftsByEmployee.Key, shiftsByEmployeeAndDate.Key, createIfNotExists: true);
                        if (timeBlockDate != null)
                            stampingTimeBlockDates.Add(timeBlockDate);
                    }

                    #endregion
                }
            }

            #region Save

            if (result.Success)
                result = Save();

            if (result.Success)
            {
                result.StrDict2 = strDict;
                if (stampingTimeBlockDates.Any())
                    result.Keys = stampingTimeBlockDates.Where(i => i.TimeBlockDateId > 0).Select(i => i.TimeBlockDateId).Distinct().ToList();
            }

            #endregion

            return result;
        }

        private ActionResult SaveTimeScheduleShiftAndLogChanges(TimeSchedulePlanningDayDTO shift, ShiftHistoryLogCallStackProperties logProperties, bool dontRestoreToSchedule = false, bool updateScheduledTimeSummary = true, List<EmployeeGroup> employeeGroups = null)
        {
            TimeSchedulePlanningDayDTO originalShiftDto = null;

            #region Prepare for logging

            if (logProperties != null)
            {
                TimeScheduleTemplateBlock originalShift = GetScheduleBlock(logProperties.OriginalShiftId);
                if (originalShift != null)
                    originalShiftDto = originalShift.ToTimeSchedulePlanningDayDTO();
            }

            #endregion

            ActionResult result = SaveTimeScheduleShift(shift, logProperties, out TimeScheduleTemplateBlock shiftAfterChange, dontRestoreToSchedule: dontRestoreToSchedule, updateScheduledTimeSummary: updateScheduledTimeSummary, employeeGroups: employeeGroups);
            if (result.Success && logProperties != null)
            {
                logProperties.NewShiftId = shiftAfterChange?.TimeScheduleTemplateBlockId ?? 0;

                //TimeScheduleTemplateBlock shiftAfterChange = GetTemplateBlocksDiscardedStateFromCache(logProperties.NewShiftId);
                if (shiftAfterChange != null)
                {
                    TimeSchedulePlanningDayDTO shiftAfterChangeDto = shiftAfterChange.ToTimeSchedulePlanningDayDTO();

                    #region Perform logging

                    ActionResult logResult = CreateTimeScheduleTemplateBlockHistoryEntry(originalShiftDto, shiftAfterChangeDto, logProperties);
                    if (!logResult.Success)
                        return logResult;

                    #endregion
                }
            }

            return result;
        }

        private ActionResult SaveTimeScheduleShift(TimeSchedulePlanningDayDTO shiftInput, ShiftHistoryLogCallStackProperties logProperties, out TimeScheduleTemplateBlock block, bool dontRestoreToSchedule = false, bool updateScheduledTimeSummary = true, List<EmployeeGroup> employeeGroups = null)
        {
            ActionResult result;
            block = null;

            #region Prereq

            if (shiftInput.EmployeeId == 0)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(9976, "Passet saknar anställd"));

            // Get hidden employee
            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
            if (hiddenEmployeeId == 0 && UseStaffing())
                return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_HiddenEmployeeNotFound);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(shiftInput.EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            if (!shiftInput.TimeScheduleScenarioHeadId.HasValue)
            {
                result = ValidateLockedDay(employee.EmployeeId, shiftInput.ActualDate.Date);
                if (!result.Success)
                    return result;
            }

            if (logProperties.IsStandByView && !shiftInput.IsStandby())
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(10117, "Otillåten ändring") + ": " + GetText(8918, "Endast tillåtet att ändra pass av typen beredskap"));

            bool useAccountHierarchy = UseAccountHierarchy();
            if (useAccountHierarchy && !logProperties.IsCreatingScenario())
            {
                if (employeeGroups == null)
                    employeeGroups = GetEmployeeGroupsFromCache();

                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(shiftInput.ActualDate, employeeGroups: employeeGroups);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8539, "Tidavtal hittades inte"));

                if (!shiftInput.AccountId.HasValue && shiftInput.StartTime != shiftInput.StopTime && !employeeGroup.AllowShiftsWithoutAccount)
                    return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_MandatoryAccountMissing, string.Format(GetText(8841, "Passet {0} - {1} saknar tillhörighet. Anställd {2}, {3}."), shiftInput.StartTime.ToShortTimeString(), shiftInput.StopTime.ToShortTimeString(), employee.EmployeeNr, shiftInput.ActualDate.ToShortDateString()));

                // Check if shift has valid account for current user.
                // Only check new shifts, otherwise a user can not add "own" shifts to lended employee.
                string message = string.Empty;
                if (shiftInput.TimeScheduleTemplateBlockId == 0 && shiftInput.AccountId.HasValue && shiftInput.AccountId.Value != 0 && !CanUserEditShiftOnGivenAcountAndDateForEmployee(employee, shiftInput.AccountId.Value, shiftInput.ActualDate, out message))
                    return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_AccountNotValidForEmployee, message);

                if (logProperties.IsSaveTimeScheduleShift() && shiftInput.TimeScheduleTemplateBlockId != 0)
                {
                    var newbreaks = shiftInput.GetMyBreaks().Where(x => x.Id == 0).ToList();
                    foreach (var newBreak in newbreaks)
                    {
                        //Make sure user cant add breaks on lended shit
                        if (!CanUserEditShiftOnGivenAcountAndDateForEmployee(employee, shiftInput.AccountId.Value, shiftInput.ActualDate, out message))
                            return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_AccountNotValidForEmployee, GetText(8944, "Ej tillåtet att lägga rast på ett utlånat pass"));
                    }
                }
            }

            #endregion

            #region Destination TimeScheduleTemplatePeriod

            // Get TimeScheduleTemplatePeriod (always check period, since employee or date might have changed on an existing)
            TimeScheduleTemplatePeriod destinationPeriod = !shiftInput.TimeScheduleScenarioHeadId.HasValue ? GetTimeScheduleTemplatePeriodFromCache(employee.EmployeeId, shiftInput.ActualDate, !logProperties.IsCreatingScenario()) : null;
            shiftInput.TimeScheduleTemplatePeriodId = destinationPeriod?.TimeScheduleTemplatePeriodId;
            if (!shiftInput.TimeScheduleTemplatePeriodId.HasValue && !shiftInput.TimeScheduleScenarioHeadId.HasValue)
            {
                string errorMessage = string.Empty;
                if (hiddenEmployeeId == employee.EmployeeId)
                    errorMessage = GetText(8438, "Ledigt pass saknar aktiverat schema på aktuell dag, aktivera schemat under menyn Planering->Aktivera schema");
                else
                    errorMessage = employee != null ? string.Format(GetText(8842, "Anställd med anställningsnr {0} saknar aktiverat schema {1}"), employee.EmployeeNr, shiftInput.ActualDate.ToShortDateString()) : GetText(8382, "Aktiverat schema saknas på aktuell dag");
                return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_PeriodNotFound, errorMessage);
            }

            //Remove??
            //if (destinationPeriod != null && !destinationPeriod.TimeScheduleTemplateHeadReference.IsLoaded)
            //    destinationPeriod.TimeScheduleTemplateHeadReference.Load();

            #endregion

            #region Get existing schedule block

            TimeScheduleTemplateBlock originalTemplateBlock = GetScheduleBlockWithPeriodAndStaffingAndAccounting(shiftInput.TimeScheduleTemplateBlockId);
            bool employeeIdHasChanged = originalTemplateBlock != null && originalTemplateBlock.EmployeeId.HasValue && employee != null && originalTemplateBlock.EmployeeId.Value != employee.EmployeeId;
            int originalEmployeeId = originalTemplateBlock?.EmployeeId ?? 0;
            int originalTimeScheduleTemplatePeriodId = originalTemplateBlock?.TimeScheduleTemplatePeriodId ?? 0;
            DateTime? originalDate = originalTemplateBlock?.Date;
            bool extraShiftHasChanged = originalTemplateBlock?.ExtraShift != shiftInput.ExtraShift;
            bool scheduleTypeHasChanged = originalTemplateBlock?.TimeScheduleTypeId != shiftInput.TimeScheduleTypeId.ToNullable();
            bool scheduleAccountHasChanged = originalTemplateBlock?.AccountId != shiftInput.AccountId.ToNullable();
            bool shiftTypeHasChanged = originalTemplateBlock?.ShiftTypeId != shiftInput.ShiftTypeId.ToNullable();
            bool sourceDateOrTimeHasChanged = originalDate.HasValue && (shiftInput.StartTime != originalTemplateBlock.ActualStartTime.Value || shiftInput.StopTime != originalTemplateBlock.ActualStopTime.Value);

            #endregion

            #region Collect breaks to handle

            if (employeeIdHasChanged && !shiftInput.HandleBreaks)
            {
                shiftInput.HandleBreaks = true;
                shiftInput.MoveBreaksWithShift = true;
                shiftInput.BreaksToHandle = GetBreaksToHandleDueToShiftChanges(shiftInput.TimeScheduleTemplateBlockId).Select(b => b.TimeScheduleTemplateBlockId).ToList();
            }

            #endregion

            #region Destination TimeScheduleEmployeePeriod

            TimeScheduleEmployeePeriod destinationEmployeePeriod = CreateTimeScheduleEmployeePeriodIfNotExist(shiftInput.ActualDate, employee.EmployeeId, out bool employeePeriodCreated);
            if (destinationEmployeePeriod == null)
                return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_EmployeePeriodCouldNotBeCreated);

            if (employeePeriodCreated)
            {
                result = Save();
                if (!result.Success)
                    return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_EmployeePeriodCouldNotBeCreated);
            }

            shiftInput.TimeScheduleEmployeePeriodId = destinationEmployeePeriod.TimeScheduleEmployeePeriodId;
            if (shiftInput.TimeScheduleEmployeePeriodId == 0)
                return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_EmployeePeriodCouldNotBeCreated);

            #endregion

            #region Preliminary

            if (!shiftInput.TimeScheduleScenarioHeadId.HasValue && originalTemplateBlock != null && originalTemplateBlock.IsPreliminary != shiftInput.IsPreliminary)
            {
                // Transactions must not be attested
                AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                if (attestStateInitialPayroll == null)
                    return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_PreliminaryNotUpdated);

                // TimePayrollTransaction
                List<TimePayrollTransaction> employeeTimePayrollTransactions = GetTimePayrollTransactions(employee.EmployeeId, shiftInput.StartTime, shiftInput.StopTime);
                if (employeeTimePayrollTransactions.Any(t => t.AttestStateId != attestStateInitialPayroll.AttestStateId))
                    return new ActionResult((int)ActionResultSave.TimeSchedulePlanning_PreliminaryNotUpdated);

                SetTimePayrollTransactionsToPreliminary(employeeTimePayrollTransactions, shiftInput.IsPreliminary);

                // TimeBlock
                List<TimeBlock> employeeTimeBlocks = GetTimeBlocksWithTimeBlockDate(employee.EmployeeId, shiftInput.StartTime, shiftInput.StopTime);
                SetTimeBlocksToPreliminary(employeeTimeBlocks, shiftInput.IsPreliminary);
            }

            #endregion

            #region Set default timeCode if needed

            if (shiftInput.TimeCodeId == 0)
                shiftInput.TimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);

            #endregion

            #region Queue

            if (employeeIdHasChanged)
            {
                if (!originalTemplateBlock.TimeScheduleTemplateBlockQueue.IsLoaded)
                    originalTemplateBlock.TimeScheduleTemplateBlockQueue.Load();

                if (originalTemplateBlock.TimeScheduleTemplateBlockQueue.Any())
                {
                    SendXEMailOnDeniedShift(shiftInput, entities);
                    // Clear the whole queue
                    ClearShiftQueue(originalTemplateBlock, TermGroup_TimeScheduleTemplateBlockQueueType.Wanted);

                    AddCurrentSendXEMailEmployeeDate(originalEmployeeId, originalDate, (TermGroup_TimeScheduleTemplateBlockType)originalTemplateBlock.Type, unWantedShift: true);
                    AddCurrentSendXEMailEmployeeDate(shiftInput.EmployeeId, shiftInput.ActualDate, shiftInput.Type, wantedShift: true);
                }

                // If a user was assigned to a shift, remove all entrys in other queues on shifts that overlaps the assigned shift
                List<int> shiftIds = GetShiftsWhereEmployeeIsInQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, shiftInput.EmployeeId, shiftInput.StartTime, shiftInput.StopTime);
                foreach (int shiftId in shiftIds)
                {
                    RemoveEmployeeFromShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType.Wanted, shiftId, shiftInput.EmployeeId, false);
                }
            }

            #endregion

            #region Decide new shift user status

            TermGroup_TimeScheduleTemplateBlockShiftUserStatus shiftUserStatus = TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None;
            if (logProperties.IsCreatingScenario() || logProperties.IsActivatingScenario())
            {
                // If scenario, just copy user status from source shift
                shiftUserStatus = shiftInput.ShiftUserStatus;
            }
            else if (shiftInput.EmployeeId != hiddenEmployeeId && !IsVacantEmployeeFromCache(shiftInput.EmployeeId))
            {
                shiftUserStatus = (employeeIdHasChanged || originalTemplateBlock == null) ? TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted : (TermGroup_TimeScheduleTemplateBlockShiftUserStatus)originalTemplateBlock.ShiftUserStatus;
            }

            #endregion

            #region Set zero blocks to deleted

            // Delete all zero schedule blocks on destinationPeriod, TimeScheduleTemplatePeriodId for EmployeeId and date.
            // Even if employeeId has not changed this should be okey, since we will not double click on a zero day.
            // This is not needed when creating a new scenario since the scenario is new            
            // OBS!! NOTE to myself: consider doing this outside this method so vi can do it for a longer span
            if (!logProperties.IsCreatingScenario() && logProperties.AbsenceForEmployeeId != shiftInput.EmployeeId)
                SetTimeScheduleBlockZeroToDeleted(shiftInput.TimeScheduleScenarioHeadId, shiftInput.StartTime.Date, shiftInput.TimeScheduleTemplatePeriodId, shiftInput.EmployeeId, saveChanges: false);

            #endregion

            #region Order planning (Connect to order/project)

            int originalLength = 0;
            if (shiftInput.Order != null && originalTemplateBlock != null)
            {
                originalLength = originalTemplateBlock.PlannedTime ?? ((int)(originalTemplateBlock.StopTime - originalTemplateBlock.StartTime).TotalMinutes);
                OrderListDTO order = shiftInput.Order;
                if (order != null)
                {
                    originalTemplateBlock.CustomerInvoiceId = order.OrderId;
                    originalTemplateBlock.ProjectId = order.ProjectId;
                }
            }

            #endregion

            #region Add/Update schedule block

            TimeScheduleTemplateBlock newScheduleBlock = null;
            if (originalTemplateBlock != null && originalTemplateBlock.EmployeeId.HasValue && originalTemplateBlock.Date.HasValue/* && originalTemplateBlock.TimeScheduleEmployeePeriodId.HasValue*/)
            {
                #region Update Schedule blocks

                if (employeeIdHasChanged || originalTemplateBlock.Date.Value != shiftInput.StartTime.Date)
                {
                    #region Create new schedule block as zero day if needed

                    //Only create zero day schedule block if originalblock is the only block for the employee on the same period and date
                    int noOfTemplateBlocks = GetNoOfScheduleBlocks(originalTemplateBlock.TimeScheduleScenarioHeadId, originalTemplateBlock.Date.Value, originalTemplateBlock.EmployeeId.Value);
                    if (noOfTemplateBlocks == 1)
                    {
                        //No need to use return value
                        CreateTimeScheduleTemplateBlockZero(originalTemplateBlock);
                    }

                    #endregion
                }

                if (employeeIdHasChanged)
                {
                    #region Steal schedule block from origin employee and give it to destination employee

                    UpdateScheduleTemplateBlock(shiftInput, originalTemplateBlock, employee, true);

                    #endregion
                }
                else
                {
                    #region Update schedule

                    UpdateScheduleTemplateBlock(shiftInput, originalTemplateBlock, employee);

                    #endregion
                }

                #region Set shift statuses

                SetShiftStatus(originalTemplateBlock);
                SetShiftUserStatus(originalTemplateBlock, shiftUserStatus);

                #endregion

                //if the extrashift property on the shift that we are saving has changed or if the extrashift property is true and the employee or date has changed
                if (employeeIdHasChanged || sourceDateOrTimeHasChanged || extraShiftHasChanged)
                {
                    SetCurrentHasShiftUnhandledChanges(originalEmployeeId, originalDate, originalTemplateBlock.ExtraShift);
                    SetCurrentHasShiftUnhandledChanges(shiftInput.EmployeeId, shiftInput.ActualDate, originalTemplateBlock.ExtraShift);
                }

                #endregion
            }
            else
            {
                #region Add new Schedule Block

                newScheduleBlock = ConvertToTimeScheduleTemplateBlock(shiftInput, employee);
                if (newScheduleBlock != null)
                {
                    #region Set shift statuses

                    SetShiftStatus(newScheduleBlock);
                    SetShiftUserStatus(newScheduleBlock, shiftUserStatus);

                    #endregion

                    #region Order planning

                    if (shiftInput.Order != null)
                    {
                        OrderListDTO order = shiftInput.Order;
                        newScheduleBlock.CustomerInvoiceId = order.OrderId;
                        newScheduleBlock.ProjectId = order.ProjectId;
                    }

                    #endregion

                    //if a new shift is added with the property extrashift set to true
                    if (!newScheduleBlock.TimeScheduleScenarioHeadId.HasValue)
                        SetCurrentHasShiftUnhandledChanges(newScheduleBlock.EmployeeId, newScheduleBlock.Date, extra: newScheduleBlock.ExtraShift);

                }

                #endregion
            }

            #endregion

            #region Handle Breaks

            if (shiftInput.HandleBreaks)
                HandleBreaksDueToShiftChanges(shiftInput, logProperties);

            #endregion

            #region Order planning (Update remaining time on order)

            if (shiftInput.Order != null)
            {
                int newLength = (int)(CalendarUtility.GetScheduleTime(shiftInput.StopTime, shiftInput.StartTime.Date, shiftInput.StopTime.Date) - CalendarUtility.GetScheduleTime(shiftInput.StartTime)).TotalMinutes;
                int length = shiftInput.PlannedTime.HasValue ? shiftInput.PlannedTime.Value - originalLength : newLength - originalLength;
                UpdateOrderRemainingTime(length, this.actorCompanyId, shiftInput.Order.OrderId);

                if (originalTemplateBlock != null)
                    originalTemplateBlock.PlannedTime = shiftInput.PlannedTime;
                else if (newScheduleBlock != null)
                    newScheduleBlock.PlannedTime = shiftInput.PlannedTime;
            }

            #endregion

            result = Save();
            if (result.Success)
            {
                if (newScheduleBlock != null)
                {
                    shiftInput.TimeScheduleTemplateBlockId = newScheduleBlock.TimeScheduleTemplateBlockId;
                    block = newScheduleBlock;
                }
                else
                {
                    block = originalTemplateBlock;
                }

                if (!shiftInput.TimeScheduleScenarioHeadId.HasValue && shiftInput.ChangesAffectsRestore)
                {
                    if (!dontRestoreToSchedule && (originalEmployeeId > 0 && originalDate.HasValue && originalTimeScheduleTemplatePeriodId > 0) && (employeeIdHasChanged || sourceDateOrTimeHasChanged || scheduleTypeHasChanged || shiftTypeHasChanged || scheduleAccountHasChanged))
                        SetDayToBeRestoredToSchedule(originalEmployeeId, originalDate.Value, originalTimeScheduleTemplatePeriodId);
                    if (!dontRestoreToSchedule && ((originalEmployeeId != shiftInput.EmployeeId) || (originalDate.HasValue && originalDate.Value != shiftInput.ActualDate)))
                        SetDayToBeRestoredToSchedule(shiftInput.EmployeeId, shiftInput.ActualDate, shiftInput.TimeScheduleTemplatePeriodId);
                }

                #region Order planning

                if (shiftInput.Order != null)
                    SyncOriginUserWithScheduleBlocks(shiftInput.Order.OrderId);

                #endregion

                #region Calculate ScheduledTimeSummary

                if (updateScheduledTimeSummary && base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, shiftInput.ActualDate))
                {
                    if (originalEmployeeId > 0 && originalDate.HasValue)
                        TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, originalEmployeeId, originalDate.Value, originalDate.Value);
                    if (originalEmployeeId != shiftInput.EmployeeId || (originalDate.HasValue && originalDate.Value != shiftInput.StartTime.Date))
                        TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, shiftInput.EmployeeId, shiftInput.StartTime.Date, shiftInput.StartTime.Date);
                }

                if (base.HasCalculatePayrollOnChanges(entities, actorCompanyId))
                {
                    if (originalEmployeeId > 0 && originalDate.HasValue)
                        CalculatePayroll(originalEmployeeId, originalDate.Value, originalDate.Value);
                    if (originalEmployeeId != shiftInput.EmployeeId || (originalDate.HasValue && originalDate.Value != shiftInput.StartTime.Date))
                        CalculatePayroll(shiftInput.EmployeeId, shiftInput.StartTime.Date, shiftInput.StartTime.Date);
                }

                #endregion

                result.Value = shiftInput;

            }

            return result;
        }

        private ActionResult CreateShiftAndAssignTask(int employeeId, DateTime date, StaffingNeedsTaskDTO staffingNeedsTaskDTO, bool skipXEMailOnChanges, Guid batchId)
        {
            TimeSchedulePlanningDayDTO newShift = CreateTimeSchedulePlanningDayDTO(employeeId, date, staffingNeedsTaskDTO);

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.AssignTaskToEmployee, null, skipXEMailOnChanges);

            ActionResult result = SaveTimeScheduleShiftAndLogChanges(newShift, logProperties);
            if (!result.Success)
                return result;

            int newShiftId = newShift.TimeScheduleTemplateBlockId;

            TimeScheduleTemplateBlockTaskDTO taskDTO = CreateTimeScheduleTemplateBlockTaskDTO(staffingNeedsTaskDTO, newShiftId, date);
            result = SaveTimeScheduleTemplateBlockTask(taskDTO);
            if (!result.Success)
                return result;

            return result;
        }

        private ActionResult DeleteTimeScheduleShifts(List<TimeScheduleTemplateBlock> shifts, int? timeScheduleScenarioHeadId, bool updateScheduleTimeSummary, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = ValidateShiftsAgainstGivenScenario(shifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            foreach (var shiftsByEmployee in shifts.Where(x => (x.IsSchedule() || x.IsOnDuty()) && x.EmployeeId.HasValue).GroupBy(x => x.EmployeeId.Value))
            {
                if (!timeScheduleScenarioHeadId.HasValue)
                {
                    result = ValidateLockedDays(shiftsByEmployee.Key, shiftsByEmployee.Where(x => x.Date.HasValue).Select(x => x.Date.Value).ToList());
                    if (!result.Success)
                        return result;
                }

                bool applyEmployeeCleanup = false;

                int employeeId = shiftsByEmployee.Key;
                List<int> employeeShiftIds = shiftsByEmployee.Select(x => x.TimeScheduleTemplateBlockId).ToList();
                List<DateTime> employeeDates = new List<DateTime>();
                foreach (var shiftsByDate in shiftsByEmployee.Where(x => x.ActualStartTime.HasValue).OrderBy(x => x.ActualStartTime).GroupBy(x => x.Date.Value))
                {
                    bool applyDateCleanup = false;

                    if (shiftsByDate.Count(x => !x.IsBreak) == 1 && shiftsByDate.First(x => !x.IsBreak).IsZero)
                        continue;

                    DateTime date = shiftsByDate.Key;

                    foreach (var shift in shiftsByDate.Where(x => !x.IsBreak))
                    {
                        var breaks = shift.GetOverlappedBreaks(shiftsByDate.Where(x => x.IsBreak)).ToList();
                        if (IsHiddenEmployeeFromCache(employeeId))
                            breaks = breaks.Where(x => x.Link == shift.Link).ToList();

                        foreach (var shiftBreak in breaks)
                        {
                            result = DeleteBreakAndLogChanges(logProperties, shiftBreak, false);
                            if (!result.Success)
                                return result;
                        }

                        TimeSchedulePlanningDayDTO scheduleCopyDto = shift.ToTimeSchedulePlanningDayDTO();
                        logProperties.OriginalShiftId = shift.TimeScheduleTemplateBlockId;

                        // Update presence block
                        SetTimeScheduleTemplateBlockToZero(shift);
                        if (!timeScheduleScenarioHeadId.HasValue)
                        {
                            #region Log shift changes

                            result = CreateTimeScheduleTemplateBlockHistoryEntry(scheduleCopyDto, shift.ToTimeSchedulePlanningDayDTO(), logProperties);
                            if (!result.Success)
                                return result;

                            #endregion
                        }

                        applyEmployeeCleanup = true;
                        applyDateCleanup = true;
                    }

                    #region Date cleanup

                    if (!timeScheduleScenarioHeadId.HasValue && applyDateCleanup)
                    {
                        SetDayToBeRestoredToSchedule(employeeId, date, shiftsByDate.ToList());
                        SetCurrentHasShiftUnhandledChanges(employeeId, date, extra: shiftsByDate.Any(x => x.ExtraShift));
                    }

                    #endregion
                }

                #region Employee Cleanup

                if (!timeScheduleScenarioHeadId.HasValue && applyEmployeeCleanup)
                {
                    #region Tasks

                    result = FreeTimeScheduleTemplateBlockTasks(employeeShiftIds, true);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Order planning
                    // Update remaining time on order
                    List<int> orderIds = shiftsByEmployee.Where(x => x.CustomerInvoiceId.HasValue).Select(x => x.CustomerInvoiceId.Value).ToList();
                    if (orderIds.Any())
                    {
                        List<CustomerInvoice> orders = (from c in this.entities.Invoice.OfType<CustomerInvoice>()
                                                        where c.Origin.ActorCompanyId == actorCompanyId &&
                                                        orderIds.Contains(c.InvoiceId) &&
                                                        c.State == (int)SoeEntityState.Active
                                                        select c).ToList();


                        var shiftsWithOrders = shiftsByEmployee.Where(x => x.CustomerInvoiceId.HasValue).ToList();
                        foreach (var shiftWithOrder in shiftsWithOrders)
                        {
                            var order = orders.FirstOrDefault(x => x.InvoiceId == shiftWithOrder.CustomerInvoiceId.Value);
                            if (order != null)
                            {
                                int length = shiftWithOrder.PlannedTime ?? ((int)(shiftWithOrder.StopTime - shiftWithOrder.StartTime).TotalMinutes);
                                order.RemainingTime += length;
                                SetModifiedProperties(order);
                                SyncOriginUserWithScheduleBlocks(order.InvoiceId);
                            }
                        }
                    }

                    #endregion

                    #region ShiftRequests

                    SetShiftRequestsToDeleted(employeeShiftIds);

                    #endregion

                    #region Check multiple zero shifts

                    result = SetUselessActiveZeroShiftsToDeleted(employeeId, employeeDates, timeScheduleScenarioHeadId);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Calculate ScheduledTimeSummary

                    if (updateScheduleTimeSummary)
                    {
                        DateTime dateRangeFrom = shiftsByEmployee.Where(x => x.Date.HasValue).OrderBy(x => x.Date).First().Date.Value;
                        DateTime dateRangeTo = shiftsByEmployee.Where(x => x.Date.HasValue).OrderBy(x => x.Date).Last().Date.Value;
                        if (base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, dateRangeTo))
                            TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employeeId, dateRangeFrom, dateRangeTo);
                    }

                    if (base.HasCalculatePayrollOnChanges(entities, actorCompanyId))
                        CalculatePayroll(employeeId, employeeDates.First(), employeeDates.Last());

                    #endregion
                }

                #endregion

                result = Save();
                if (!result.Success)
                    return result;
            }

            return result;
        }

        private ActionResult DeleteTimeScheduleShift(int timeScheduleTemplateBlockId, Guid batchId, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, TimeScheduleTemplateBlock scheduleCopy = null)
        {
            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, timeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.TaskDeleteTimeScheduleShift, null, skipXEMailOnChanges);

            TimeSchedulePlanningDayDTO scheduleCopyDto = null;
            if (scheduleCopy == null)
                scheduleCopy = GetScheduleBlock(timeScheduleTemplateBlockId);
            if (scheduleCopy != null)
                scheduleCopyDto = scheduleCopy.ToTimeSchedulePlanningDayDTO();
            if (scheduleCopyDto == null)
                return new ActionResult(false);

            ActionResult result = ValidateShiftAgainstGivenScenario(scheduleCopyDto, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            if (!timeScheduleScenarioHeadId.HasValue)
            {
                result = ValidateLockedDay(scheduleCopyDto.EmployeeId, scheduleCopyDto.ActualDate);
                if (!result.Success)
                    return result;
            }

            result = SetTimeScheduleShiftToZero(timeScheduleTemplateBlockId, logProperties, true, true);
            if (!result.Success)
                return result;

            result = Save();
            if (!result.Success)
                return result;

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(timeScheduleTemplateBlockId);
            if (scheduleBlock != null && !scheduleBlock.TimeScheduleScenarioHeadId.HasValue && scheduleBlock.ChangesAffectsRestore())
            {
                TimeSchedulePlanningDayDTO scheduleBlockDto = scheduleBlock.ToTimeSchedulePlanningDayDTO();

                SetDayToBeRestoredToSchedule(scheduleBlock.EmployeeId.Value, scheduleBlock.Date.Value, scheduleBlock.TimeScheduleTemplatePeriodId, cleanDeviationCauseOnSchedule: true);
                AddCurrentDayPayrollWarning(GetTimeBlockDateFromCache(scheduleBlock.EmployeeId.Value, scheduleBlock.Date.Value));

                #region Shift request

                // Check if there is an existing shift request
                result = RemoveShiftRequest(timeScheduleTemplateBlockId);
                if (!result.Success)
                    return result;

                #endregion

                result = CreateTimeScheduleTemplateBlockHistoryEntry(scheduleCopyDto, scheduleBlockDto, logProperties);
                if (!result.Success)
                    return result;
            }

            //Order planning
            if (scheduleBlock?.CustomerInvoiceId.GetValueOrDefault() > 0)
                SyncOriginUserWithScheduleBlocks(scheduleBlock.CustomerInvoiceId.Value);

            //Calculate ScheduledTimeSummary
            if (!scheduleCopyDto.TimeScheduleScenarioHeadId.HasValue && base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, scheduleCopyDto.StartTime.Date))
                TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, scheduleCopyDto.EmployeeId, scheduleCopyDto.StartTime.Date, scheduleCopyDto.StartTime.Date);

            if (!scheduleCopyDto.TimeScheduleScenarioHeadId.HasValue && base.HasCalculatePayrollOnChanges(entities, actorCompanyId))
                CalculatePayroll(scheduleCopyDto.EmployeeId, scheduleCopyDto.StartTime.Date, scheduleCopyDto.StartTime.Date);

            if (!scheduleCopyDto.TimeScheduleScenarioHeadId.HasValue)
            {
                SetCurrentHasShiftUnhandledChanges(scheduleCopyDto.EmployeeId, scheduleCopyDto.ActualDate, extra: scheduleCopyDto.ExtraShift);
                result = SetUselessActiveZeroShiftsToDeleted(scheduleCopyDto.EmployeeId, scheduleCopyDto.ActualDate, scheduleCopyDto.TimeScheduleScenarioHeadId);
                if (!result.Success)
                    return result;
            }

            return result;
        }

        private ActionResult InactivateShiftRequest(int timeScheduleTemplateBlockId)
        {
            Message message = (from m in this.entities.Message.Include("MessageRecipient")
                               where m.ActorCompanyId == this.ActorCompanyId &&
                               m.RecordId == timeScheduleTemplateBlockId &&
                               m.Type == (int)TermGroup_MessageType.ShiftRequest &&
                               m.State == (int)SoeEntityState.Active
                               select m).FirstOrDefault();

            if (message != null)
            {
                message.State = (int)SoeEntityState.Inactive;
                SetModifiedProperties(message);

                foreach (MessageRecipient recipient in message.MessageRecipient)
                {
                    recipient.State = (int)SoeEntityState.Inactive;
                    SetModifiedProperties(recipient);
                }
                return Save();
            }

            return new ActionResult();
        }

        private ActionResult RemoveShiftRequest(int timeScheduleTemplateBlockId)
        {
            Message message = (from m in this.entities.Message.Include("MessageRecipient")
                               where m.ActorCompanyId == this.ActorCompanyId &&
                               m.RecordId == timeScheduleTemplateBlockId &&
                               m.Type == (int)TermGroup_MessageType.ShiftRequest &&
                               m.State != (int)SoeEntityState.Deleted
                               select m).FirstOrDefault();

            if (message != null)
            {
                message.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(message);

                foreach (MessageRecipient recipient in message.MessageRecipient)
                {
                    recipient.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(recipient);
                }
                return Save();
            }

            return new ActionResult();
        }

        private ActionResult CopyOrMoveTemplateShift(ref List<TimeSchedulePlanningDayDTO> sourceTemplateShifts, ref List<TimeSchedulePlanningDayDTO> targetTemplateShifts, int sourceShiftId, bool keepSourceShiftsTogether, int? targetEmployeeId, int? targetEmployeePostId, DateTime targetDate, TimeScheduleTemplateHead targetHead, DateTime cycleStartOffset, Guid newLink, bool isCopy, ref List<int> sourceChangedDayNumbers, ref List<int> targetChangedDayNumbers)
        {
            ActionResult result = new ActionResult();
            List<TimeSchedulePlanningDayDTO> shiftsToMove = new List<TimeSchedulePlanningDayDTO>();

            TimeSchedulePlanningDayDTO dragedShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
            if (dragedShift == null)
                return new ActionResult(false);

            shiftsToMove.Add(dragedShift);

            if (keepSourceShiftsTogether && dragedShift.Link.HasValue)
                shiftsToMove.AddRange(sourceTemplateShifts.Where(x => x.DayNumber == dragedShift.DayNumber && x.Link.HasValue && x.TimeScheduleTemplateBlockId != dragedShift.TimeScheduleTemplateBlockId && x.Link == dragedShift.Link.Value).ToList());

            foreach (var shiftToMove in shiftsToMove)
            {
                if (isCopy)
                    result = ApplyCopyTemplateShift(ref targetTemplateShifts, shiftToMove, targetHead, targetEmployeeId, targetEmployeePostId, targetDate, cycleStartOffset, newLink, ref targetChangedDayNumbers);
                else
                    result = ApplyMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, shiftToMove, targetHead, targetEmployeeId, targetEmployeePostId, targetDate, cycleStartOffset, newLink, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);

                if (!result.Success)
                    return result;
            }

            return result;
        }

        private ActionResult ApplyMoveTemplateShift(ref List<TimeSchedulePlanningDayDTO> sourceTemplateShifts, ref List<TimeSchedulePlanningDayDTO> targetTemplateShifts, TimeSchedulePlanningDayDTO shiftToMove, TimeScheduleTemplateHead targetHead, int? targetEmployeeId, int? targetEmployeePostId, DateTime targetDate, DateTime cycleStartOffset, Guid newLink, ref List<int> sourceChangedDayNumbers, ref List<int> targetChangedDayNumbers)
        {
            ActionResult result = new ActionResult();

            if (shiftToMove == null)
                return new ActionResult(false);

            //remove it from source templateshifts, it will be deleted when source templateshifts is saved
            sourceTemplateShifts = sourceTemplateShifts.Where(x => x.TimeScheduleTemplateBlockId != shiftToMove.TimeScheduleTemplateBlockId).ToList();
            sourceChangedDayNumbers.Add(shiftToMove.DayNumber);

            //if source and target is same employee the shift will exist in both collections
            targetTemplateShifts = targetTemplateShifts.Where(x => x.TimeScheduleTemplateBlockId != shiftToMove.TimeScheduleTemplateBlockId).ToList();

            //Create a new shift
            TimeSchedulePlanningDayDTO newShift = shiftToMove.CopyAsNew(targetEmployeeId, targetEmployeePostId, targetDate, cycleStartOffset, targetHead.NoOfDays, newLink);
            targetChangedDayNumbers.Add(newShift.DayNumber);

            //Add breaks to new shift
            List<BreakDTO> overlappedBreaks = GetOverlappedBreaks(shiftToMove, shiftToMove.GetBreaks());
            AddBreaks(newShift, overlappedBreaks, targetEmployeeId, shiftToMove.EmployeeId, newLink);

            //Add the new shift to target
            targetTemplateShifts.Add(newShift);

            //Clear existing breaks on other sourceshifts (if exists) and then add the remaining breaks
            var otherSourceShifts = sourceTemplateShifts.Where(x => x.TimeScheduleTemplatePeriodId == shiftToMove.TimeScheduleTemplatePeriodId).ToList();
            foreach (var otherSourceShift in otherSourceShifts)
            {
                otherSourceShift.ClearBreaks(overlappedBreaks);
            }

            //Gather unique breaks and update breaks on all shifts (on current daynumber)
            FixBreaks(targetTemplateShifts, dayNumber: newShift.DayNumber);

            //Handle tasks
            if (shiftToMove.Tasks != null)
            {
                newShift.Tasks = new List<TimeScheduleTemplateBlockTaskDTO>();
                foreach (var task in shiftToMove.Tasks)
                {
                    newShift.Tasks.Add(task.CopyAsNew());
                }
            }

            return result;
        }

        private ActionResult ApplyCopyTemplateShift(ref List<TimeSchedulePlanningDayDTO> targetTemplateShifts, TimeSchedulePlanningDayDTO shiftToMove, TimeScheduleTemplateHead targetHead, int? targetEmployeeId, int? targetEmployeePostId, DateTime targetDate, DateTime cycleStartOffset, Guid newLink, ref List<int> targetChangedDayNumbes)
        {
            ActionResult result = new ActionResult();

            if (shiftToMove == null)
                return new ActionResult(false);

            //Create a new shift
            TimeSchedulePlanningDayDTO newShift = shiftToMove.CopyAsNew(targetEmployeeId, targetEmployeePostId, targetDate, cycleStartOffset, targetHead.NoOfDays, newLink);
            targetChangedDayNumbes.Add(newShift.DayNumber);

            //Add breaks to new shift
            List<BreakDTO> overlappedBreaks = GetOverlappedBreaks(shiftToMove, shiftToMove.GetBreaks());
            foreach (BreakDTO brk in overlappedBreaks)
            {
                // Handle breaks that starts on another day than the shift
                brk.OffsetDaysOnCopy = (int)(brk.StartTime.Date - shiftToMove.ActualDate.Date).TotalDays;
            }

            AddBreaks(newShift, overlappedBreaks, targetEmployeeId, shiftToMove.EmployeeId, newLink);

            //Add the new shift to target
            targetTemplateShifts.Add(newShift);

            //Gather unique breaks and update breaks on all shifts (on current daynumber)
            FixBreaks(targetTemplateShifts, dayNumber: newShift.DayNumber);

            //Handle tasks
            if (shiftToMove.Tasks != null)
            {
                newShift.Tasks = new List<TimeScheduleTemplateBlockTaskDTO>();
                foreach (var task in shiftToMove.Tasks)
                {
                    newShift.Tasks.Add(task.CopyAsNew());
                }
            }

            return result;
        }

        private ActionResult SwapEmployeesOnTemplateShifts(ref List<TimeSchedulePlanningDayDTO> sourceTemplateShifts, ref List<TimeSchedulePlanningDayDTO> targetTemplateShifts, int sourceShiftId, bool keepSourceShiftsTogether, TimeScheduleTemplateHead sourceTemplateHead, int targetShiftId, bool keepTargetShiftsTogether, DateTime sourceDate, DateTime targetDate, DateTime sourceCycleStart, DateTime targetCycleStart, TimeScheduleTemplateHead targetTemplateHead, ref List<int> sourceChangedDayNumbers, ref List<int> targetChangedDayNumbes)
        {
            ActionResult result = new ActionResult();
            List<TimeSchedulePlanningDayDTO> sourceShifts = new List<TimeSchedulePlanningDayDTO>();
            List<TimeSchedulePlanningDayDTO> targetShifts = new List<TimeSchedulePlanningDayDTO>();

            TimeSchedulePlanningDayDTO sourceShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
            if (sourceShift == null)
                return new ActionResult(false);

            sourceShifts.Add(sourceShift);
            if (keepSourceShiftsTogether && sourceShift.Link.HasValue)
                sourceShifts.AddRange(sourceTemplateShifts.Where(x => x.DayNumber == sourceShift.DayNumber && x.Link.HasValue && x.TimeScheduleTemplateBlockId != sourceShift.TimeScheduleTemplateBlockId && x.Link == sourceShift.Link.Value).ToList());

            TimeSchedulePlanningDayDTO targetShift = targetTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == targetShiftId);
            if (targetShift == null)
                return new ActionResult(false);

            targetShifts.Add(targetShift);
            if (keepTargetShiftsTogether)
                targetShifts.AddRange(targetTemplateShifts.Where(x => x.TimeScheduleTemplateBlockId != targetShift.TimeScheduleTemplateBlockId && x.Link == targetShift.Link).ToList());

            //Remove source shifts from source(=> will be deleted from source when saved)
            sourceTemplateShifts.RemoveAll(x => sourceShifts.Select(y => y.TimeScheduleTemplateBlockId).Contains(x.TimeScheduleTemplateBlockId));

            //Remove target shifts from target(=> will be deleted from source when saved)
            targetTemplateShifts.RemoveAll(x => targetShifts.Select(y => y.TimeScheduleTemplateBlockId).Contains(x.TimeScheduleTemplateBlockId));

            //Copy source to target            
            foreach (var shiftsByDayNumber in sourceShifts.GroupBy(x => x.DayNumber))
            {
                Guid newTargetLink = GetNewShiftLink();
                foreach (var shift in shiftsByDayNumber)
                {
                    //we use copy instead of remove because we have already removed shift from source                                     
                    //OBS! we dont change date when swap shifts, only employees
                    result = ApplyCopyTemplateShift(ref targetTemplateShifts, shift, targetTemplateHead, targetShift.EmployeeId, targetShift.EmployeePostId, sourceDate, targetCycleStart, newTargetLink, ref targetChangedDayNumbes);
                }
            }

            //Copy target to source            
            foreach (var shiftsByDayNumber in targetShifts.GroupBy(x => x.DayNumber))
            {
                Guid newSourceLink = GetNewShiftLink();
                foreach (var shift in shiftsByDayNumber)
                {
                    //we use copy instead of remove because we have already removed shift from target                                     
                    //OBS! we dont change date when swap shifts, only employees
                    result = ApplyCopyTemplateShift(ref sourceTemplateShifts, shift, sourceTemplateHead, sourceShift.EmployeeId, sourceShift.EmployeePostId, targetDate, sourceCycleStart, newSourceLink, ref sourceChangedDayNumbers);
                }
            }

            return result;
        }

        private ActionResult DragActionReplaceExistingShifts(int sourceShiftId, int targetShiftId, int destinationEmployeeId, DateTime destinationDate, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, bool saveChanges = false, bool includeOnDutyShifts = false, List<int> includedSourceOnDutyShiftIds = null)
        {
            Guid batchId = GetNewBatchLink();
            Guid newLink = GetNewShiftLink();
            List<TimeScheduleTemplateBlock> shiftsToReplace = new List<TimeScheduleTemplateBlock>();

            TimeScheduleTemplateBlock targetShift = GetScheduleBlockWithPeriodAndStaffing(targetShiftId);
            if (targetShift == null)
                return new ActionResult(false);

            shiftsToReplace.Add(targetShift);
            if (keepTargetShiftsTogether)
                shiftsToReplace.AddRange(GetScheduleBlocksLinkedWithPeriodAndStaffing(null, targetShift).Where(x => x.TimeScheduleTemplateBlockId != targetShift.TimeScheduleTemplateBlockId).ToList());

            #region Included source onDuty shifts

            if (includeOnDutyShifts)
            {
                // We pick the first StartTime and the last StopTime in the linked group of shifts.
                // A risk that it can possibly hit onDuty-shifts that exists in the gap between shifts.

                List<TimeScheduleTemplateBlock> targetOverlappingOnDutyShifts = GetOverlappingOnDutyShifts(targetShift.EmployeeId, targetShift.AccountId, targetShift.Date, shiftsToReplace.OrderBy(s => s.ActualStartTime).Select(s => s.StartTime).FirstOrDefault(), shiftsToReplace.OrderByDescending(s => s.ActualStopTime).Select(s => s.StopTime).FirstOrDefault());
                shiftsToReplace.AddRange(targetOverlappingOnDutyShifts);
            }

            #endregion

            ActionResult result = ValidateShiftsAgainstGivenScenario(shiftsToReplace, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            foreach (var shiftToReplace in shiftsToReplace)
            {
                TimeSchedulePlanningDayDTO targetShiftCopy = shiftToReplace.ToTimeSchedulePlanningDayDTO();
                ShiftHistoryLogCallStackProperties targetShiftlogProperties = new ShiftHistoryLogCallStackProperties(batchId, shiftToReplace.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.DragShiftActionReplace, null, skipXEMailOnChanges)
                {
                    NewShiftId = shiftToReplace.TimeScheduleTemplateBlockId
                };

                // Delete target shift
                result = SetTimeScheduleShiftToZero(shiftToReplace.TimeScheduleTemplateBlockId, targetShiftlogProperties, true, false);
                if (!result.Success)
                    return result;

                result = Save();
                if (!result.Success)
                    break;

                #region Shift request

                // Check if there is an existing shift request
                result = InactivateShiftRequest(shiftToReplace.TimeScheduleTemplateBlockId);
                if (!result.Success)
                    return result;

                #endregion

                #region Order planning

                if (targetShiftCopy.Order != null)
                    SyncOriginUserWithScheduleBlocks(targetShiftCopy.Order.OrderId);

                #endregion

                if (result.Success)
                    CreateTimeScheduleTemplateBlockHistoryEntry(targetShiftCopy, targetShiftlogProperties);
            }

            if (!result.Success)
                return result;

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.DragShiftActionReplace, null, skipXEMailOnChanges);


            List<TimeScheduleTemplateBlock> includedOnDutyShifts = new List<TimeScheduleTemplateBlock>();
            if (!includedSourceOnDutyShiftIds.IsNullOrEmpty())
                includedOnDutyShifts = GetScheduleBlocks(includedSourceOnDutyShiftIds);

            // Move source shifts
            result = DragActionMoveShifts(logProperties, sourceShiftId, destinationEmployeeId, destinationDate, keepSourceShiftsTogether, timeScheduleScenarioHeadId, newLink, saveChanges, true, null, includedOnDutyShifts);
            if (!result.Success)
                return result;

            return result;
        }

        private ActionResult DragActionReplaceAndFreeExistingShifts(int sourceShiftId, int targetShiftId, int destinationEmployeeId, DateTime destinationDate, bool keepSourceShiftsTogether, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, bool saveChanges = false)
        {
            Guid batchId = GetNewBatchLink();

            // Get target shift
            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlockWithPeriodAndStaffing(targetShiftId);
            if (scheduleBlock == null)
                return new ActionResult(false);

            ShiftHistoryLogCallStackProperties targetShiftLogProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.DragShiftActionReplaceAndFree, null, skipXEMailOnChanges);

            // Move target shifts
            ActionResult result = DragActionMoveShifts(targetShiftLogProperties, targetShiftId, GetHiddenEmployeeIdFromCache(), scheduleBlock.Date.Value, keepSourceShiftsTogether, timeScheduleScenarioHeadId, GetNewShiftLink(), saveChanges);
            if (!result.Success)
                return result;


            ShiftHistoryLogCallStackProperties sourceShiftLogProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.DragShiftActionReplaceAndFree, null, skipXEMailOnChanges);

            // Move source shifts
            result = DragActionMoveShifts(sourceShiftLogProperties, sourceShiftId, destinationEmployeeId, destinationDate, keepSourceShiftsTogether, timeScheduleScenarioHeadId, GetNewShiftLink(), saveChanges);
            if (!result.Success)
                return result;

            return result;
        }

        private ActionResult DragActionSwapEmployeesOnShifts(int sourceShiftId, int targetShiftId, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, bool saveChanges = false, bool includeOnDutyShifts = false, List<int> includedOnDutyShiftIds = null)
        {
            #region Prereq

            // Get source shift
            TimeScheduleTemplateBlock sourceBlock = GetScheduleBlockWithPeriodAndStaffing(sourceShiftId);
            if (sourceBlock == null)
                return new ActionResult(false);

            TimeSchedulePlanningDayDTO sourceShift = sourceBlock.ToTimeSchedulePlanningDayDTO();
            if (sourceShift == null)
                return new ActionResult(false);

            // Get target shift
            TimeScheduleTemplateBlock targetBlock = GetScheduleBlockWithPeriodAndStaffing(targetShiftId);
            if (targetBlock == null)
                return new ActionResult(false);

            TimeSchedulePlanningDayDTO targetShift = targetBlock.ToTimeSchedulePlanningDayDTO();
            if (targetShift == null)
                return new ActionResult(false);

            #endregion

            #region Perform

            ActionResult result = SwapTimeScheduleShifts(sourceShift, targetShift, TermGroup_ShiftHistoryType.DragShiftActionSwapEmployee, keepSourceShiftsTogether, keepTargetShiftsTogether, skipXEMailOnChanges, timeScheduleScenarioHeadId, includeOnDutyShifts, includedOnDutyShiftIds);

            if (!result.Success)
                return result;

            #endregion

            #region Shift request

            // Check if there is an existing shift request
            result = InactivateShiftRequest(sourceShiftId);
            if (!result.Success)
                return result;

            result = InactivateShiftRequest(targetShiftId);
            if (!result.Success)
                return result;

            #endregion

            #region Save

            if (saveChanges)
                result = Save();

            #endregion

            return result;
        }

        /// <summary>
        /// The employees on the shifts will be swaped
        /// </summary>
        /// <param name="sourceShift"></param>
        /// <param name="targetShift"></param>
        /// <param name="historyType"></param>
        /// <param name="keepSourceShiftsTogether"></param>
        /// <param name="keepTargetShiftsTogether"></param>
        /// <returns></returns>
        private ActionResult SwapTimeScheduleShifts(TimeSchedulePlanningDayDTO sourceShift, TimeSchedulePlanningDayDTO targetShift, TermGroup_ShiftHistoryType historyType, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, bool includeOnDutyShifts = false, List<int> includedSourceOnDutyShiftIds = null)
        {
            List<TimeScheduleTemplateBlock> linkedSourceShifts = new List<TimeScheduleTemplateBlock>();
            List<TimeScheduleTemplateBlock> linkedTargetShifts = new List<TimeScheduleTemplateBlock>();

            #region Prereq

            if (sourceShift == null || targetShift == null)
                return new ActionResult(false);

            int sourceShiftId = sourceShift.TimeScheduleTemplateBlockId;
            TimeScheduleTemplateBlock dragedSourceShift = GetScheduleBlockWithPeriodAndStaffing(sourceShiftId);
            if (dragedSourceShift == null)
                return new ActionResult(false);
            if (!dragedSourceShift.EmployeeId.HasValue || !dragedSourceShift.Date.HasValue)
                return new ActionResult(false);

            int targetShiftId = targetShift.TimeScheduleTemplateBlockId;
            TimeScheduleTemplateBlock dragedTargetShift = GetScheduleBlockWithPeriodAndStaffing(targetShiftId);
            if (dragedTargetShift == null)
                return new ActionResult(false);
            if (!dragedTargetShift.EmployeeId.HasValue || !dragedTargetShift.Date.HasValue)
                return new ActionResult(false);

            #endregion

            #region Get Source Shifts

            if (keepSourceShiftsTogether)
                linkedSourceShifts = GetScheduleBlocksLinkedWithPeriodAndStaffing(timeScheduleScenarioHeadId, dragedSourceShift).Where(x => x.TimeScheduleTemplateBlockId != dragedSourceShift.TimeScheduleTemplateBlockId).ToList();

            linkedSourceShifts.Add(dragedSourceShift);

            #region Included source onDuty shift ids

            if (!includedSourceOnDutyShiftIds.IsNullOrEmpty())
            {
                List<TimeScheduleTemplateBlock> includedOnDutyShifts = GetScheduleBlocks(includedSourceOnDutyShiftIds);
                linkedSourceShifts.AddRange(includedOnDutyShifts);
            }

            #endregion

            ActionResult result = ValidateShiftsAgainstGivenScenario(linkedSourceShifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            #endregion

            #region Get Target Shifts

            if (keepTargetShiftsTogether)
                linkedTargetShifts = GetScheduleBlocksLinkedWithPeriodAndStaffing(timeScheduleScenarioHeadId, dragedTargetShift).Where(x => x.TimeScheduleTemplateBlockId != dragedTargetShift.TimeScheduleTemplateBlockId).ToList();

            linkedTargetShifts.Add(dragedTargetShift);

            #region Get target onDuty shifts

            if (includeOnDutyShifts)
            {
                // We pick the first StartTime and the last StopTime in the linked group of shifts.
                // A risk that it can possibly hit onDuty-shifts that exists in the gap between shifts.

                List<TimeScheduleTemplateBlock> targetOverlappingOnDutyShifts = GetOverlappingOnDutyShifts(dragedTargetShift.EmployeeId, dragedTargetShift.AccountId, dragedTargetShift.Date, linkedTargetShifts.OrderBy(s => s.ActualStartTime).Select(s => s.StartTime).FirstOrDefault(), linkedTargetShifts.OrderByDescending(s => s.ActualStopTime).Select(s => s.StopTime).FirstOrDefault());
                linkedTargetShifts.AddRange(targetOverlappingOnDutyShifts);
            }

            #endregion

            result = ValidateShiftsAgainstGivenScenario(linkedTargetShifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            #endregion

            return SwapTimeScheduleShifts(linkedSourceShifts, linkedTargetShifts, historyType, skipXEMailOnChanges);

        }

        /// <summary>
        ///Destination employee will recieve a copy of the shift shift.employeeId is the destination employee, if zero destination employee is hidden employee    
        /// </summary>
        /// <param name="logProperties"></param>
        /// <param name="sourceShiftId"></param>
        /// <param name="shift"></param>
        /// <param name="keepShiftsTogether"></param>        
        /// <returns></returns>

        private ActionResult SwapTimeScheduleShifts(List<TimeScheduleTemplateBlock> sourceShifts, List<TimeScheduleTemplateBlock> targetShifts, TermGroup_ShiftHistoryType historyType, bool skipXEMailOnChanges)
        {
            List<TimeSchedulePlanningDayDTO> sourceShiftsDTO = new List<TimeSchedulePlanningDayDTO>();
            List<TimeSchedulePlanningDayDTO> targetShiftsDTO = new List<TimeSchedulePlanningDayDTO>();
            Guid batchId = GetNewBatchLink();
            Guid sourceLink = GetNewShiftLink();
            Guid targetLink = GetNewShiftLink();

            int sourceEmployeeId = sourceShifts.FirstOrDefault()?.EmployeeId.Value ?? 0;
            int targetEmployeeId = targetShifts.FirstOrDefault()?.EmployeeId.Value ?? 0;

            ActionResult result = new ActionResult();

            #region Handle source breaks

            foreach (var sourceShiftToMove in sourceShifts)
            {
                var shift = sourceShiftToMove.ToTimeSchedulePlanningDayDTO();
                if (shift != null)
                {
                    shift.BreaksToHandle = new List<int>();
                    shift.BreaksToHandle = GetBreaksToHandleDueToShiftChanges(sourceShiftToMove.TimeScheduleTemplateBlockId).Select(b => b.TimeScheduleTemplateBlockId).ToList();
                    shift.MoveBreaksWithShift = true;
                    shift.HandleBreaks = true;
                    sourceShiftsDTO.Add(shift);
                }
            }

            #endregion

            #region Handle target breaks

            foreach (var targetShiftToMove in targetShifts)
            {
                var shift = targetShiftToMove.ToTimeSchedulePlanningDayDTO();
                if (shift != null)
                {
                    shift.BreaksToHandle = new List<int>();
                    shift.BreaksToHandle = GetBreaksToHandleDueToShiftChanges(targetShiftToMove.TimeScheduleTemplateBlockId).Select(b => b.TimeScheduleTemplateBlockId).ToList();
                    shift.MoveBreaksWithShift = true;
                    shift.HandleBreaks = true;
                    targetShiftsDTO.Add(shift);
                }
            }

            #endregion

            ShiftHistoryLogCallStackProperties logProperties1 = new ShiftHistoryLogCallStackProperties(batchId, 0, historyType, null, skipXEMailOnChanges);

            #region Move sourceshifts

            foreach (var sourceShiftDTO in sourceShiftsDTO)
            {
                logProperties1.OriginalShiftId = sourceShiftDTO.TimeScheduleTemplateBlockId;
                var sourceSchedule = sourceShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftDTO.TimeScheduleTemplateBlockId);
                if (sourceSchedule == null)
                    continue;

                var sourceDTOCopy = sourceSchedule.ToTimeSchedulePlanningDayDTO();
                sourceShiftDTO.EmployeeId = targetEmployeeId;
                sourceShiftDTO.Link = sourceLink;

                result = MoveTimeScheduleShiftToGivenEmployee(logProperties1, sourceShiftDTO.TimeScheduleTemplateBlockId, sourceShiftDTO);
                if (!result.Success)
                    break;

                #region Tasks

                //No need to handle tasks (calling MoveTimeScheduleTemplateBlockTasks), because they will follow their parent (sourceshiftid) and also
                //when you swap shift, the dates are not changed only the employees

                #endregion

                CreateTimeScheduleTemplateBlockHistoryEntry(sourceDTOCopy, logProperties1);
            }

            #endregion

            if (!result.Success)
                return result;

            ShiftHistoryLogCallStackProperties logProperties2 = new ShiftHistoryLogCallStackProperties(batchId, 0, historyType, null, skipXEMailOnChanges);

            #region Move targetshifts

            foreach (var targetShiftDTO in targetShiftsDTO)
            {
                logProperties2.OriginalShiftId = targetShiftDTO.TimeScheduleTemplateBlockId;
                var targetSchedule = targetShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == targetShiftDTO.TimeScheduleTemplateBlockId);
                if (targetSchedule == null)
                    continue;

                var targetShiftDTOCopy = targetSchedule.ToTimeSchedulePlanningDayDTO();
                targetShiftDTO.EmployeeId = sourceEmployeeId;
                targetShiftDTO.Link = targetLink;

                result = MoveTimeScheduleShiftToGivenEmployee(logProperties2, targetShiftDTO.TimeScheduleTemplateBlockId, targetShiftDTO);
                if (!result.Success)
                    break;

                #region Tasks

                //No need to handle tasks (calling MoveTimeScheduleTemplateBlockTasks), because they will follow their parent (targetshiftid) and also
                //when you swap shift, the dates are not changed only the employees

                #endregion

                CreateTimeScheduleTemplateBlockHistoryEntry(targetShiftDTOCopy, logProperties2);
            }

            #endregion

            //if (!result.Success)
            return result;

        }
        private ActionResult CopyTimeScheduleShiftToGivenEmployee(ShiftHistoryLogCallStackProperties logProperties, int sourceShiftId, TimeSchedulePlanningDayDTO shift)
        {
            if (shift == null)
                return new ActionResult(false);

            if (!shift.HandleBreaks && shift.IsSchedule()) //Calling method may already have done this
            {
                shift.HandleBreaks = true;
                shift.CopyBreaksWithShift = true;

                shift.BreaksToHandle = GetBreaksToHandleDueToShiftChanges(sourceShiftId).Select(b => b.TimeScheduleTemplateBlockId).ToList();
            }

            #region Set props

            if (IsHiddenEmployeeFromCache(shift.EmployeeId) && GetCompanyBoolSettingFromCache(CompanySettingType.ExtraShiftAsDefaultOnHidden))
                shift.ExtraShift = true;

            #endregion

            shift.TimeScheduleTemplateBlockId = 0; //forces SaveTimeScheduleShift to copy the shift instead of changing employee
            shift.TimeDeviationCauseId = null;
            shift.EmployeeChildId = null;
            shift.TimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
            ActionResult result = SaveTimeScheduleShift(shift, logProperties, out TimeScheduleTemplateBlock updatedOrCreatedBlock);
            logProperties.NewShiftId = updatedOrCreatedBlock?.TimeScheduleTemplateBlockId ?? 0;

            return result;
        }

        /// <summary>
        /// Hidden employee will recieve a copy of the shift
        /// </summary>
        /// <param name="logProperties"></param>
        /// <param name="sourceShiftId"></param>
        /// <param name="shift"></param>
        /// <param name="keepShiftsTogether"></param>        
        /// <returns></returns>
        private ActionResult CopyTimeScheduleShiftToHiddenEmployee(ShiftHistoryLogCallStackProperties logProperties, int sourceShiftId, TimeSchedulePlanningDayDTO shift)
        {
            if (shift == null)
                return new ActionResult(false);

            shift.EmployeeId = GetHiddenEmployeeIdFromCache();

            //check companysetting for setting this as extrashift when target is Hidden
            if (GetCompanyBoolSettingFromCache(CompanySettingType.ExtraShiftAsDefaultOnHidden))
                shift.ExtraShift = true;

            ActionResult result = CopyTimeScheduleShiftToGivenEmployee(logProperties, sourceShiftId, shift);

            return result;
        }

        /// <summary>
        /// Shift will be moved to destination employee shift.employeeId is the destination employee, if zero destination employee is hidden employee
        /// </summary>
        /// <param name="logProperties"></param>
        /// <param name="sourceShiftId"></param>
        /// <param name="shift"></param>
        /// <param name="keepShiftsTogether"></param>        
        /// <returns></returns>
        private ActionResult MoveTimeScheduleShiftToGivenEmployee(ShiftHistoryLogCallStackProperties logProperties, int sourceShiftId, TimeSchedulePlanningDayDTO shift, List<TimeScheduleTemplateBlock> sourceBreakBlocks = null)
        {
            if (shift == null)
                return new ActionResult(false);

            if (!shift.HandleBreaks) //Calling method may already have done this
            {
                shift.HandleBreaks = true;
                shift.MoveBreaksWithShift = true;
                shift.BreaksToHandle = GetBreaksToHandleDueToShiftChanges(sourceShiftId, sourceBreakBlocks).Select(b => b.TimeScheduleTemplateBlockId).ToList();
            }

            ActionResult result = SaveTimeScheduleShift(shift, logProperties, out TimeScheduleTemplateBlock updatedOrCreatedBlock);
            logProperties.NewShiftId = updatedOrCreatedBlock?.TimeScheduleTemplateBlockId ?? 0;

            return result;
        }

        private ActionResult DragActionCopyShifts(ShiftHistoryLogCallStackProperties logProperties, int sourceShiftId, int destinationEmployeeId, DateTime destinationDate, bool keepSourceShiftsTogether, bool copyTaskWithShift, int? timeScheduleScenarioHeadId, Guid newLink, bool saveChanges = false, List<TimeScheduleTemplateBlock> includedOnDutyShifts = null)
        {
            List<TimeScheduleTemplateBlock> shiftsToCopy = new List<TimeScheduleTemplateBlock>();
            List<TimeScheduleTemplateBlock> linkedShifts = new List<TimeScheduleTemplateBlock>();
            TimeScheduleTemplateBlock dragedShift = GetScheduleBlockWithPeriodAndStaffing(sourceShiftId);

            if (dragedShift == null)
                return new ActionResult(false);

            if (!dragedShift.EmployeeId.HasValue || !dragedShift.Date.HasValue)
                return new ActionResult(false);

            shiftsToCopy.Add(dragedShift);

            if (keepSourceShiftsTogether)
                linkedShifts = GetScheduleBlocksLinkedWithPeriodAndStaffing(timeScheduleScenarioHeadId, dragedShift).Where(x => x.TimeScheduleTemplateBlockId != dragedShift.TimeScheduleTemplateBlockId).ToList();

            shiftsToCopy.AddRange(linkedShifts);

            #region Included onDuty shifts

            if (!includedOnDutyShifts.IsNullOrEmpty())
                shiftsToCopy.AddRange(includedOnDutyShifts);

            #endregion

            ActionResult result = ValidateShiftsAgainstGivenScenario(shiftsToCopy, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            foreach (var shiftToCopy in shiftsToCopy)
            {
                if (shiftToCopy.ActualStartTime.HasValue && shiftToCopy.ActualStopTime.HasValue)
                {
                    //OBS! destinationDate is the date that the scheduleblock belongs to, but if the original scheduleblock (= shiftToMove) has the flag "BelongsToPreviousDay" set to true
                    //Starttime must start the next day

                    var (start, stop) = shiftToCopy.GetNewActualStartAndStopTime(destinationDate);
                    logProperties.OriginalShiftId = shiftToCopy.TimeScheduleTemplateBlockId;
                    result = DragActionCopyShift(logProperties, shiftToCopy.TimeScheduleTemplateBlockId, destinationEmployeeId, start, stop, newLink, copyTaskWithShift, saveChanges: saveChanges);
                    if (!result.Success)
                        break;
                }
            }

            return result;
        }

        private ActionResult DragActionCopyShift(ShiftHistoryLogCallStackProperties logProperties, int sourceShiftId, int destinationEmployeeId, DateTime startTime, DateTime stopTime, Guid newLink, bool copyTaskWithShift, bool saveChanges = false)
        {
            #region Prereq

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlockWithPeriodAndStaffing(sourceShiftId);
            if (scheduleBlock == null)
                return new ActionResult(false);

            TimeSchedulePlanningDayDTO originalShiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            TimeSchedulePlanningDayDTO shift = ConvertToTimeSchedulePlanningDayDTO(scheduleBlock, destinationEmployeeId, startTime, stopTime);
            if (shift == null)
                return new ActionResult(false);

            #endregion

            #region Set props

            if (IsHiddenEmployeeFromCache(destinationEmployeeId) && GetCompanyBoolSettingFromCache(CompanySettingType.ExtraShiftAsDefaultOnHidden))
                shift.ExtraShift = true;

            #endregion

            #region Perform

            shift.Link = newLink;
            ActionResult result = CopyTimeScheduleShiftToGivenEmployee(logProperties, sourceShiftId, shift);
            logProperties.NewShiftId = shift.TimeScheduleTemplateBlockId;
            if (!result.Success)
                return result;

            #endregion

            #region Tasks
            if (copyTaskWithShift)
            {
                result = CopyTimeScheduleTemplateBlockTasks(sourceShiftId, logProperties.NewShiftId);
                if (!result.Success)
                    return result;
            }
            #endregion

            if (saveChanges)
                result = Save();
            if (result.Success)
                result = CreateTimeScheduleTemplateBlockHistoryEntry(originalShiftCopy, logProperties);

            return result;
        }

        private ActionResult DragActionMoveShifts(ShiftHistoryLogCallStackProperties logProperties, int sourceShiftId, int destinationEmployeeId, DateTime destinationDate, bool keepSourceShiftsTogether, int? timeScheduleScenarioHeadId, Guid newLink, bool saveChanges = false, bool removeShiftRequest = true, List<TimeScheduleTemplateBlock> sourceBreakBlocks = null, List<TimeScheduleTemplateBlock> includedOnDutyShifts = null)
        {
            List<TimeScheduleTemplateBlock> shiftsToMove = new List<TimeScheduleTemplateBlock>();
            List<TimeScheduleTemplateBlock> linkedShifts = new List<TimeScheduleTemplateBlock>();
            TimeScheduleTemplateBlock dragedShift = GetScheduleBlockWithPeriodAndStaffing(sourceShiftId);

            if (dragedShift == null)
                return new ActionResult(false);

            if (!dragedShift.EmployeeId.HasValue || !dragedShift.Date.HasValue)
                return new ActionResult(false);

            shiftsToMove.Add(dragedShift);

            if (keepSourceShiftsTogether)
                linkedShifts = GetScheduleBlocksLinkedWithPeriodAndStaffing(timeScheduleScenarioHeadId, dragedShift).Where(x => x.TimeScheduleTemplateBlockId != dragedShift.TimeScheduleTemplateBlockId).ToList();

            shiftsToMove.AddRange(linkedShifts);

            #region Included onDuty shifts

            if (!includedOnDutyShifts.IsNullOrEmpty())
                shiftsToMove.AddRange(includedOnDutyShifts);

            #endregion

            ActionResult result = ValidateShiftsAgainstGivenScenario(shiftsToMove, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            foreach (var shiftToMove in shiftsToMove)
            {
                if (shiftToMove.ActualStartTime.HasValue && shiftToMove.ActualStopTime.HasValue)
                {
                    //OBS! destinationDate is the date that the scheduleblock belongs to, but if the original scheduleblock (= shiftToMove) has the flag "BelongsToPreviousDay" set to true
                    //Starttime must start the next day

                    var (start, stop) = shiftToMove.GetNewActualStartAndStopTime(destinationDate);
                    logProperties.OriginalShiftId = shiftToMove.TimeScheduleTemplateBlockId;
                    result = DragActionMoveShift(logProperties, shiftToMove, destinationEmployeeId, start, stop, newLink, saveChanges: saveChanges, removeShiftRequest: removeShiftRequest, sourceBreakBlocks);
                    if (!result.Success)
                        break;
                }
            }

            return result;
        }

        private ActionResult DragActionMoveShift(ShiftHistoryLogCallStackProperties logProperties, TimeScheduleTemplateBlock scheduleBlock, int destinationEmployeeId, DateTime startTime, DateTime stopTime, Guid newLink, bool saveChanges = false, bool removeShiftRequest = true, List<TimeScheduleTemplateBlock> sourceBreakBlocks = null)
        {
            #region Prereq

            if (scheduleBlock == null)
                return new ActionResult(false);

            TimeSchedulePlanningDayDTO originalShiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            TimeSchedulePlanningDayDTO shift = ConvertToTimeSchedulePlanningDayDTO(scheduleBlock, destinationEmployeeId, startTime, stopTime);
            if (shift == null)
                return new ActionResult(false);

            shift.Link = newLink;

            #endregion

            #region Set props

            if (IsHiddenEmployeeFromCache(destinationEmployeeId) && GetCompanyBoolSettingFromCache(CompanySettingType.ExtraShiftAsDefaultOnHidden))
                shift.ExtraShift = true;

            #endregion

            #region Perform

            ActionResult result = MoveTimeScheduleShiftToGivenEmployee(logProperties, scheduleBlock.TimeScheduleTemplateBlockId, shift, sourceBreakBlocks);
            if (!result.Success)
                return result;

            #endregion

            #region Tasks

            //the parent (sourceshiftid) is still the same but the date may have changed, so we need to this method anyway
            result = MoveTimeScheduleTemplateBlockTasks(scheduleBlock.TimeScheduleTemplateBlockId, scheduleBlock.TimeScheduleTemplateBlockId);
            if (!result.Success)
                return result;

            #endregion

            #region Shift request

            // Check if there is an existing shift request
            if (removeShiftRequest)
            {
                result = InactivateShiftRequest(scheduleBlock.TimeScheduleTemplateBlockId);
                if (!result.Success)
                    return result;
            }

            #endregion

            #region Save

            if (saveChanges)
                result = Save();

            if (!result.Success)
                return result;

            #endregion

            #region Log
            if (result.Success)
                CreateTimeScheduleTemplateBlockHistoryEntry(originalShiftCopy, logProperties);

            #endregion

            return result;
        }

        private ActionResult SetTimeScheduleShiftToZero(int timeScheduleTemplateBlockId, ShiftHistoryLogCallStackProperties logProperties, bool deleteOverlappedBreaks = false, bool deleteInvalidBreaks = false)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            // Get existing presence block
            TimeScheduleTemplateBlock originalBlock = GetScheduleBlock(timeScheduleTemplateBlockId);

            #endregion

            if (originalBlock == null)
            {
                return new ActionResult((int)ActionResultDelete.TimeSchedulePlanning_ShiftIsNull);
            }
            else
            {
                #region Delete Breaks

                if (deleteOverlappedBreaks)
                    result = DeleteOverlappedBreaks(originalBlock, logProperties, true);

                if (!result.Success)
                    return result;

                if (deleteInvalidBreaks)
                    result = DeleteInvalidBreaks(originalBlock, logProperties, true);

                if (!result.Success)
                    return result;

                #endregion

                #region Order planning

                // Update remaining time on order
                if (originalBlock.CustomerInvoiceId.HasValue)
                {
                    CustomerInvoice order = (from c in this.entities.Invoice.OfType<CustomerInvoice>()
                                             where c.Origin.ActorCompanyId == actorCompanyId &&
                                             c.InvoiceId == originalBlock.CustomerInvoiceId.Value &&
                                             c.State == (int)SoeEntityState.Active
                                             select c).FirstOrDefault();
                    if (order != null)
                    {
                        int length = originalBlock.PlannedTime ?? ((int)(originalBlock.StopTime - originalBlock.StartTime).TotalMinutes);
                        order.RemainingTime += length;
                        SetModifiedProperties(order);
                    }
                }

                #endregion

                #region Tasks

                result = FreeTimeScheduleTemplateBlockTasks(originalBlock.TimeScheduleTemplateBlockId, true);
                if (!result.Success)
                    return result;

                #endregion

                #region Update

                // Update presence block
                SetTimeScheduleTemplateBlockToZero(originalBlock);

                #endregion
            }

            return result;
        }

        private ActionResult SetHasUnhandledShiftChanges()
        {
            if (this.currentHasUnhandledShiftChanges == null)
                return new ActionResult(true);

            foreach (var changesByEmployee in this.currentHasUnhandledShiftChanges.GroupBy(c => c.EmployeeId))
            {
                if (changesByEmployee.Key == GetHiddenEmployeeIdFromCache())
                    continue;

                List<TimeBlockDate> timeBlockDates = GetTimeBlockDates(changesByEmployee.Key, changesByEmployee.Select(c => c.Date).Distinct().ToList());
                foreach (TimeBlockDate timeBlockDate in timeBlockDates)
                {
                    timeBlockDate.HasUnhandledShiftChanges = true;
                    if (changesByEmployee.Any(c => c.Date == timeBlockDate.Date && c.Extra))
                        timeBlockDate.HasUnhandledExtraShiftChanges = true;
                    SetModifiedProperties(timeBlockDate);
                }
            }

            this.currentHasUnhandledShiftChanges.Clear();
            return Save();
        }

        private ActionResult SetShiftStatus(TimeScheduleTemplateBlock block, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();

            if (block.EmployeeId == 0 || !block.EmployeeId.HasValue)
                block.EmployeeId = GetHiddenEmployeeIdFromCache();

            TermGroup_TimeScheduleTemplateBlockShiftStatus status = TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned;
            if (IsHiddenEmployeeFromCache(block.EmployeeId.Value))
                status = TermGroup_TimeScheduleTemplateBlockShiftStatus.Open;

            block.ShiftStatus = (int)status;
            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetShiftUserStatus(int scheduleBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus status, bool saveChanges = false)
        {
            return SetShiftUserStatus(GetScheduleBlock(scheduleBlockId), status, saveChanges);
        }

        private ActionResult SetShiftUserStatus(TimeScheduleTemplateBlock block, TermGroup_TimeScheduleTemplateBlockShiftUserStatus status, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();

            block.ShiftUserStatus = (int)status;
            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetShiftUserStatus(List<TimeScheduleTemplateBlock> shifts, TermGroup_TimeScheduleTemplateBlockShiftUserStatus status, int? timeScheduleScenarioHeadId, bool saveChanges = false)
        {
            if (shifts == null)
                return new ActionResult(false);

            ActionResult result = ValidateShiftsAgainstGivenScenario(shifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            foreach (var shift in shifts)
            {
                SetShiftUserStatus(shift, status);
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetShiftUserStatus(int employeeId, DateTime start, DateTime stop, bool includeZeroDays, ExtendedAbsenceSetting extendedAbsenceSetting, TermGroup_TimeScheduleTemplateBlockShiftUserStatus status, int? timeScheduleScenarioHeadId, bool saveChanges = false, int? timeDeviationCauseId = null)
        {
            List<TimeScheduleTemplateBlock> shifts = TimeScheduleManager.GetShiftsAffectedByIntervall(entities, actorCompanyId, employeeId, timeScheduleScenarioHeadId, start, stop, true, includeZeroDays, false, out _, out _, timeDeviationCauseId).ToList();

            ActionResult result = ValidateShiftsAgainstGivenScenario(shifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            var affectedShifts = TimeScheduleManager.ApplyExtendedAbsenceSettings(entities, start, stop, shifts, extendedAbsenceSetting, this.actorCompanyId);

            foreach (var shift in shifts)
            {
                if (shift.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved)
                    continue;

                if (affectedShifts.Any(x => x.TimeScheduleTemplateBlockId == shift.TimeScheduleTemplateBlockId))
                    SetShiftUserStatus(shift, status);
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SetShiftUserStatus(EmployeeRequest request, TermGroup_TimeScheduleTemplateBlockShiftUserStatus status, int? timeScheduleScenarioHeadId, bool saveChanges = false)
        {
            bool includeZeroDays = false;

            if (request.TimeDeviationCauseId.HasValue)
            {
                TimeDeviationCause timdeDeviationCause = GetTimeDeviationCauseFromCache(request.TimeDeviationCauseId.Value);
                if (timdeDeviationCause != null)
                    includeZeroDays = timdeDeviationCause.ShowZeroDaysInAbsencePlanning;
            }

            List<TimeScheduleTemplateBlock> shifts = TimeScheduleManager.GetShiftsAffectedByIntervall(entities, actorCompanyId, request.EmployeeId, timeScheduleScenarioHeadId, request.Start, request.Stop, true, includeZeroDays, false, out _, out _, timeDeviationCauseId: request.TimeDeviationCauseId).ToList();

            ActionResult result = ValidateShiftsAgainstGivenScenario(shifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            var affectedShifts = TimeScheduleManager.ApplyExtendedAbsenceSettings(entities, request.Start, request.Stop, shifts, request.ExtendedAbsenceSetting, this.actorCompanyId);
            foreach (var shift in shifts)
            {
                if (shift.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved)
                    continue;

                if (affectedShifts.Any(x => x.TimeScheduleTemplateBlockId == shift.TimeScheduleTemplateBlockId))
                    SetShiftUserStatus(shift, status);
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private bool ShiftHasChanged(TimeSchedulePlanningDayDTO beforeShift, TimeSchedulePlanningDayDTO afterShift)
        {
            if (beforeShift == null || afterShift == null)
                return false;

            bool hasChanged = false;
            if (NumberUtility.HasChanged(beforeShift.EmployeeId, afterShift.EmployeeId))
                hasChanged = true;
            if (NumberUtility.HasChanged(beforeShift.TimeDeviationCauseId, afterShift.TimeDeviationCauseId))
                hasChanged = true;
            if (NumberUtility.HasChanged((int)beforeShift.ShiftUserStatus, (int)afterShift.ShiftUserStatus))
                hasChanged = true;
            if (NumberUtility.HasChanged((int)beforeShift.ShiftStatus, (int)afterShift.ShiftStatus))
                hasChanged = true;
            if (beforeShift.StartTime != afterShift.StartTime)
                hasChanged = true;
            if (beforeShift.StopTime != afterShift.StopTime)
                hasChanged = true;
            if (NumberUtility.HasChanged(beforeShift.ShiftTypeId, afterShift.ShiftTypeId))
                hasChanged = true;
            if (beforeShift.ExtraShift != afterShift.ExtraShift)
                hasChanged = true;
            return hasChanged;
        }

        #endregion

        #region Schedule - shift changes/history

        private ActionResult CopyShiftHistory(int shiftIdTo, List<TimeScheduleTemplateBlockHistory> history)
        {
            if (shiftIdTo == 0)
                return new ActionResult(false);

            foreach (var item in history)
            {
                TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory
                {
                    TimeScheduleTemplateBlockId = shiftIdTo,
                    ActorCompanyId = item.ActorCompanyId,
                    CurrentEmployeeId = item.CurrentEmployeeId,
                    BatchId = item.BatchId,
                    Type = item.Type,
                    FromShiftStatus = item.FromShiftStatus,
                    ToShiftStatus = item.ToShiftStatus,
                    FromShiftUserStatus = item.FromShiftUserStatus,
                    ToShiftUserStatus = item.ToShiftUserStatus,
                    FromEmployeeId = item.FromEmployeeId,
                    ToEmployeeId = item.ToEmployeeId,
                    FromStart = item.FromStart,
                    ToStart = item.ToStart,
                    FromStop = item.FromStop,
                    ToStop = item.ToStop,
                    FromShiftTypeId = item.FromShiftTypeId,
                    ToShiftTypeId = item.ToShiftTypeId,
                    FromTimeDeviationCauseId = item.FromTimeDeviationCauseId,
                    ToTimeDeviationCauseId = item.ToTimeDeviationCauseId,
                    Created = item.Created,
                    CreatedBy = item.CreatedBy,
                    State = item.State,
                    RecordId = item.RecordId,
                    IsBreak = item.IsBreak,
                    OriginEmployeeId = item.OriginEmployeeId,
                    OriginDate = item.OriginDate,
                    OriginTimeScheduleTemplateBlockId = item.OriginTimeScheduleTemplateBlockId,
                    FromExtraShift = item.FromExtraShift,
                    ToExtraShift = item.ToExtraShift,
                };
                //OBS! dont call SetCreatedProperties
                entities.TimeScheduleTemplateBlockHistory.AddObject(logEntry);
            }

            return Save();
        }

        private ActionResult CreateTimeScheduleTemplateBlockHistoryEntry(TimeSchedulePlanningDayDTO originalShift, ShiftHistoryLogCallStackProperties logProperties)
        {
            TimeScheduleTemplateBlock newShift = GetScheduleBlock(logProperties.NewShiftId);
            TimeSchedulePlanningDayDTO newshiftDto = newShift?.ToTimeSchedulePlanningDayDTO();

            return CreateTimeScheduleTemplateBlockHistoryEntry(originalShift, newshiftDto, logProperties);
        }

        private ActionResult CreateTimeScheduleTemplateBlockHistoryEntry(TimeSchedulePlanningDayDTO originalShift, TimeSchedulePlanningDayDTO newShift, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result;

            bool createEntry = true;
            if (newShift == null || logProperties == null)
                return new ActionResult(false);

            TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory()
            {
                TimeScheduleTemplateBlockId = newShift.TimeScheduleTemplateBlockId,
                BatchId = logProperties.BatchId.ToString(),
                Type = (int)logProperties.HistoryType,
                RecordId = logProperties.RecordId,
                ActorCompanyId = this.actorCompanyId,
                CurrentEmployeeId = newShift.EmployeeId,
            };

            if (originalShift == null)
            {
                //newShift is a brand new shift
                logEntry.ToEmployeeId = newShift.EmployeeId;
                logEntry.ToTimeDeviationCauseId = newShift.TimeDeviationCauseId;
                logEntry.ToShiftUserStatus = (int)newShift.ShiftUserStatus;
                logEntry.ToShiftStatus = (int)newShift.ShiftStatus;
                logEntry.ToStart = newShift.StartTime;
                logEntry.ToStop = newShift.StopTime;
                logEntry.ToShiftTypeId = newShift.ShiftTypeId;
                logEntry.ToExtraShift = newShift.ExtraShift;
                if (!newShift.IsPreliminary && newShift.StartTime.Date >= DateTime.Today && !logProperties.SkipXEMailOnChanges && !newShift.BelongsToPreviousDay && !newShift.BelongsToNextDay)
                    AddCurrentSendXEMailEmployeeDate(logEntry.ToEmployeeId, newShift.StartTime.Date, newShift.Type);
            }
            else if (originalShift.TimeScheduleTemplateBlockId != newShift.TimeScheduleTemplateBlockId)
            {
                //New shift has been created from original shift                    
                logEntry.ToEmployeeId = newShift.EmployeeId;
                logEntry.ToTimeDeviationCauseId = newShift.TimeDeviationCauseId;
                logEntry.ToShiftUserStatus = (int)newShift.ShiftUserStatus;
                logEntry.ToShiftStatus = (int)newShift.ShiftStatus;
                logEntry.ToStart = newShift.StartTime;
                logEntry.ToStop = newShift.StopTime;
                logEntry.ToShiftTypeId = newShift.ShiftTypeId;
                logEntry.ToExtraShift = newShift.ExtraShift;
                logEntry.OriginEmployeeId = originalShift.EmployeeId;
                logEntry.OriginDate = originalShift.StartTime.Date;
                logEntry.OriginTimeScheduleTemplateBlockId = originalShift.TimeScheduleTemplateBlockId;

                if (!newShift.IsPreliminary && newShift.StartTime.Date >= DateTime.Today && !logProperties.SkipXEMailOnChanges && !newShift.BelongsToPreviousDay && !newShift.BelongsToNextDay)
                    AddCurrentSendXEMailEmployeeDate(newShift.EmployeeId, newShift.StartTime, newShift.Type);

                //See also if originalshift has changed
                result = CreateLogEntryIfChanged(originalShift, logProperties);
                if (!result.Success)
                    return result;
            }
            else
            {
                //newShift is originalShift but with changed properties, only log if changed
                createEntry = LogShiftChanges(logEntry, originalShift, newShift, logProperties);
            }

            if (createEntry)
            {
                SetCreatedProperties(logEntry);
                entities.AddToTimeScheduleTemplateBlockHistory(logEntry);

                result = Save();
                if (!result.Success)
                    return result;
            }

            result = CreateDeleteBreakLogEntry(logProperties);
            return result;
        }

        private ActionResult CreateDeleteBreakLogEntry(ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = new ActionResult();

            if (logProperties.DeletedBreaks == null)
                return result;

            foreach (var deletedBreakLogData in logProperties.DeletedBreaks)
            {
                TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory
                {
                    TimeScheduleTemplateBlockId = deletedBreakLogData.DeletedBreak.TimeScheduleTemplateBlockId,
                    BatchId = logProperties.BatchId.ToString(),
                    Type = (int)logProperties.HistoryType,
                    RecordId = logProperties.RecordId,
                    ActorCompanyId = this.actorCompanyId,
                    CurrentEmployeeId = deletedBreakLogData.DeletedBreak.EmployeeId.Value,
                    IsBreak = true,
                };

                logEntry.FromStart = GetActualDateTime(deletedBreakLogData.OriginStartTime, deletedBreakLogData.DeletedBreak.Date.Value);
                //set new start to same date as origin starttime but with zero time
                logEntry.ToStart = GetActualDateTime(new DateTime(deletedBreakLogData.OriginStartTime.Year, deletedBreakLogData.OriginStartTime.Month, deletedBreakLogData.OriginStartTime.Day, 0, 0, 0), deletedBreakLogData.DeletedBreak.Date.Value);
                logEntry.FromStop = GetActualDateTime(deletedBreakLogData.OriginStopTime, deletedBreakLogData.DeletedBreak.Date.Value);
                //set new stop to same date as originstoptime but with zero time
                logEntry.ToStop = GetActualDateTime(new DateTime(deletedBreakLogData.OriginStopTime.Year, deletedBreakLogData.OriginStopTime.Month, deletedBreakLogData.OriginStopTime.Day, 0, 0, 0), deletedBreakLogData.DeletedBreak.Date.Value);
                logEntry.FromEmployeeId = deletedBreakLogData.DeletedBreak.EmployeeId.Value;
                logEntry.ToEmployeeId = deletedBreakLogData.DeletedBreak.EmployeeId.Value;

                SetCreatedProperties(logEntry);
                entities.AddToTimeScheduleTemplateBlockHistory(logEntry);

                result = Save();
                if (!result.Success)
                    return result;

                if (!deletedBreakLogData.DeletedBreak.IsPreliminary && deletedBreakLogData.DeletedBreak.Date.Value >= DateTime.Today && !logProperties.SkipXEMailOnChanges && !deletedBreakLogData.DeletedBreak.BelongsToPreviousDay && !deletedBreakLogData.DeletedBreak.BelongsToNextDay)
                    AddCurrentSendXEMailEmployeeDate(logEntry.FromEmployeeId, deletedBreakLogData.DeletedBreak.Date, TermGroup_TimeScheduleTemplateBlockType.Schedule);
            }

            logProperties.DeletedBreaks.Clear();

            return result;
        }

        private ActionResult CreateLogEntryIfChanged(TimeSchedulePlanningDayDTO beforeShift, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = new ActionResult();

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(beforeShift.TimeScheduleTemplateBlockId);
            TimeSchedulePlanningDayDTO afterShift = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            if (ShiftHasChanged(beforeShift, afterShift))
            {
                TimeScheduleTemplateBlockHistory logEntry = new TimeScheduleTemplateBlockHistory()
                {
                    TimeScheduleTemplateBlockId = afterShift.TimeScheduleTemplateBlockId,
                    BatchId = logProperties.BatchId.ToString(),
                    Type = (int)logProperties.HistoryType,
                    RecordId = logProperties.RecordId,
                    ActorCompanyId = this.actorCompanyId,
                    CurrentEmployeeId = afterShift.EmployeeId,
                };
                SetCreatedProperties(logEntry);
                entities.AddToTimeScheduleTemplateBlockHistory(logEntry);
                LogShiftChanges(logEntry, beforeShift, afterShift, logProperties);
            }
            return result;
        }

        private bool LogShiftChanges(TimeScheduleTemplateBlockHistory logEntry, TimeSchedulePlanningDayDTO originalShift, TimeSchedulePlanningDayDTO newShift, ShiftHistoryLogCallStackProperties logProperties)
        {
            if (!ShiftHasChanged(originalShift, newShift))
                return false;

            logEntry.FromEmployeeId = originalShift.EmployeeId;
            logEntry.ToEmployeeId = newShift.EmployeeId;

            logEntry.FromTimeDeviationCauseId = originalShift.TimeDeviationCauseId;
            logEntry.ToTimeDeviationCauseId = newShift.TimeDeviationCauseId;

            logEntry.FromShiftUserStatus = (int)originalShift.ShiftUserStatus;
            logEntry.ToShiftUserStatus = (int)newShift.ShiftUserStatus;

            logEntry.FromShiftStatus = (int)originalShift.ShiftStatus;
            logEntry.ToShiftStatus = (int)newShift.ShiftStatus;

            logEntry.FromStart = originalShift.StartTime;
            logEntry.ToStart = newShift.StartTime;

            logEntry.FromStop = originalShift.StopTime;
            logEntry.ToStop = newShift.StopTime;

            logEntry.FromShiftTypeId = originalShift.ShiftTypeId;
            logEntry.ToShiftTypeId = newShift.ShiftTypeId;

            logEntry.FromExtraShift = originalShift.ExtraShift;
            logEntry.ToExtraShift = newShift.ExtraShift;

            if (!newShift.IsPreliminary && originalShift.StartTime.Date >= DateTime.Today && newShift.StartTime.Date >= DateTime.Today && !logProperties.SkipXEMailOnChanges)
            {
                if (!originalShift.BelongsToPreviousDay && !originalShift.BelongsToNextDay)
                    AddCurrentSendXEMailEmployeeDate(originalShift.EmployeeId, originalShift.StartTime.Date, originalShift.Type);

                if (!newShift.BelongsToPreviousDay && !newShift.BelongsToNextDay && (originalShift.EmployeeId != newShift.EmployeeId || originalShift.StartTime.Date != newShift.StartTime.Date))
                    AddCurrentSendXEMailEmployeeDate(newShift.EmployeeId, newShift.StartTime.Date, newShift.Type);
            }

            return true;
        }

        private ActionResult LogShiftChanges(List<TimeSchedulePlanningDayDTO> originalShiftCopies, ShiftHistoryLogCallStackProperties logProperties)
        {
            ActionResult result = new ActionResult();

            if (!originalShiftCopies.IsNullOrEmpty())
            {
                foreach (TimeSchedulePlanningDayDTO originalShiftCopy in originalShiftCopies)
                {
                    result = CreateLogEntryIfChanged(originalShiftCopy, logProperties);
                    if (!result.Success)
                        return result;
                }

                if (result.Success)
                    result = Save();
            }

            return result;
        }

        #endregion

        #region Schedule - shift request

        private EmployeeRequest AddEmployeeRequest(int employeeId, int timeDeviationCauseId, DateTime start, DateTime stop, String comment, TermGroup_EmployeeRequestType type, int? employeeChildId, bool reActivate, ExtendedAbsenceSetting extendedSettings = null)
        {
            bool includeZeroDays = false;

            TimeDeviationCause timdeDeviationCause = GetTimeDeviationCauseFromCache(timeDeviationCauseId);
            if (timdeDeviationCause != null)
                includeZeroDays = timdeDeviationCause.ShowZeroDaysInAbsencePlanning;

            if (!reActivate)
            {
                ActionResult result = SetShiftUserStatus(employeeId, start, stop, includeZeroDays, extendedSettings, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested, null, true, (timdeDeviationCause != null ? timdeDeviationCause.TimeDeviationCauseId : (int?)null));
                if (!result.Success)
                    return null;
            }

            ExtendedAbsenceSetting newExtendedSettings = null;
            if (extendedSettings != null)
            {
                newExtendedSettings = new ExtendedAbsenceSetting
                {
                    AbsenceFirstAndLastDay = extendedSettings.AbsenceFirstAndLastDay,
                    AbsenceWholeFirstDay = extendedSettings.AbsenceWholeFirstDay,
                    AbsenceFirstDayStart = extendedSettings.AbsenceFirstDayStart,
                    AbsenceWholeLastDay = extendedSettings.AbsenceWholeLastDay,
                    AbsenceLastDayStart = extendedSettings.AbsenceLastDayStart,
                    PercentalAbsence = extendedSettings.PercentalAbsence,
                    PercentalValue = extendedSettings.PercentalValue,
                    PercentalAbsenceOccursStartOfDay = extendedSettings.PercentalAbsenceOccursStartOfDay,
                    PercentalAbsenceOccursEndOfDay = extendedSettings.PercentalAbsenceOccursEndOfDay,
                    AdjustAbsencePerWeekDay = extendedSettings.AdjustAbsencePerWeekDay,
                    AdjustAbsenceAllDaysStart = extendedSettings.AdjustAbsenceAllDaysStart,
                    AdjustAbsenceAllDaysStop = extendedSettings.AdjustAbsenceAllDaysStop,
                    AdjustAbsenceMonStart = extendedSettings.AdjustAbsenceMonStart,
                    AdjustAbsenceMonStop = extendedSettings.AdjustAbsenceMonStop,
                    AdjustAbsenceTueStart = extendedSettings.AdjustAbsenceTueStart,
                    AdjustAbsenceTueStop = extendedSettings.AdjustAbsenceTueStop,
                    AdjustAbsenceWedStart = extendedSettings.AdjustAbsenceWedStart,
                    AdjustAbsenceWedStop = extendedSettings.AdjustAbsenceWedStop,
                    AdjustAbsenceThuStart = extendedSettings.AdjustAbsenceThuStart,
                    AdjustAbsenceThuStop = extendedSettings.AdjustAbsenceThuStop,
                    AdjustAbsenceFriStart = extendedSettings.AdjustAbsenceFriStart,
                    AdjustAbsenceFriStop = extendedSettings.AdjustAbsenceFriStop,
                    AdjustAbsenceSatStart = extendedSettings.AdjustAbsenceSatStart,
                    AdjustAbsenceSatStop = extendedSettings.AdjustAbsenceSatStop,
                    AdjustAbsenceSunStart = extendedSettings.AdjustAbsenceSunStart,
                    AdjustAbsenceSunStop = extendedSettings.AdjustAbsenceSunStop,
                };

                SetCreatedProperties(newExtendedSettings);
                entities.ExtendedAbsenceSetting.AddObject(newExtendedSettings);
            }

            EmployeeRequest newRequest = new EmployeeRequest
            {
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                TimeDeviationCauseId = timeDeviationCauseId != 0 ? timeDeviationCauseId : (int?)null,
                Start = start,
                Stop = stop,
                Comment = !String.IsNullOrEmpty(comment) ? comment : String.Empty,
                Type = (int)type,
                Status = (int)TermGroup_EmployeeRequestStatus.RequestPending,
                ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.None,
                ExtendedAbsenceSettingId = newExtendedSettings != null ? newExtendedSettings.ExtendedAbsenceSettingId : (int?)null,
                EmployeeChildId = employeeChildId != 0 ? employeeChildId : (int?)null,
                ReActivate = reActivate,
            };

            SetCreatedProperties(newRequest);
            entities.EmployeeRequest.AddObject(newRequest);

            return newRequest;
        }

        private ActionResult UpdateEmployeeRequest(EmployeeRequest updatedRequest, bool saveChanges = true)
        {
            ActionResult result = new ActionResult();

            DateTime oldDateStart;
            DateTime oldDateStop;

            EmployeeRequest oldRequest = GetEmployeeRequest(updatedRequest.EmployeeRequestId);
            if (oldRequest == null)
                return new ActionResult(false);

            ExtendedAbsenceSetting oldExtendedSettings = oldRequest.ExtendedAbsenceSetting;
            oldDateStart = oldRequest.Start;
            oldDateStop = oldRequest.Stop;
            int previousEmployeeid = oldRequest.EmployeeId;
            int previousDeviationCauseId = oldRequest.TimeDeviationCauseId ?? 0;

            #region Update ShiftUserStatus

            if (!updatedRequest.ReActivate && (updatedRequest.Status == (int)TermGroup_EmployeeRequestStatus.RequestPending) /* && (oldDateStart != updatedRequest.Start || oldDateStop != updatedRequest.Stop)*/)
            {
                bool includeZeroDays = false;

                TimeDeviationCause timdeDeviationCause = GetTimeDeviationCauseFromCache(previousDeviationCauseId);
                if (timdeDeviationCause != null)
                    includeZeroDays = timdeDeviationCause.ShowZeroDaysInAbsencePlanning;

                //Reverse shiftuserstatus to default for old dates
                result = SetShiftUserStatus(previousEmployeeid, oldDateStart, oldDateStop, includeZeroDays, oldExtendedSettings, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted, null, saveChanges);
                if (!result.Success)
                    return result;

                result = SetShiftUserStatus(updatedRequest, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested, null, saveChanges);
                if (!result.Success)
                    return result;
            }

            #endregion

            #region Update request

            oldRequest.ActorCompanyId = updatedRequest.ActorCompanyId;
            oldRequest.EmployeeId = updatedRequest.EmployeeId;
            oldRequest.TimeDeviationCauseId = updatedRequest.TimeDeviationCauseId;
            oldRequest.Start = updatedRequest.Start;
            oldRequest.Stop = updatedRequest.Stop;
            oldRequest.Comment = updatedRequest.Comment;
            oldRequest.Type = updatedRequest.Type;
            oldRequest.Status = updatedRequest.Status;
            oldRequest.ResultStatus = updatedRequest.ResultStatus;
            oldRequest.EmployeeChildId = updatedRequest.EmployeeChildId;
            oldRequest.ReActivate = updatedRequest.ReActivate;

            SetModifiedProperties(oldRequest);

            #region Update extended Settings

            if (oldRequest.ExtendedAbsenceSettingId.HasValue && !oldRequest.ExtendedAbsenceSettingReference.IsLoaded)
                oldRequest.ExtendedAbsenceSettingReference.Load();

            if (updatedRequest.ExtendedAbsenceSetting != null)
            {
                if (!oldRequest.ExtendedAbsenceSettingId.HasValue)
                {
                    ExtendedAbsenceSetting settings = new ExtendedAbsenceSetting();
                    SetCreatedProperties(settings);
                    entities.ExtendedAbsenceSetting.AddObject(settings);
                    oldRequest.ExtendedAbsenceSettingId = settings.ExtendedAbsenceSettingId;
                    oldRequest.ExtendedAbsenceSetting = settings;
                }
                oldRequest.ExtendedAbsenceSetting.AbsenceFirstAndLastDay = updatedRequest.ExtendedAbsenceSetting.AbsenceFirstAndLastDay;
                oldRequest.ExtendedAbsenceSetting.AbsenceWholeFirstDay = updatedRequest.ExtendedAbsenceSetting.AbsenceWholeFirstDay;
                oldRequest.ExtendedAbsenceSetting.AbsenceFirstDayStart = updatedRequest.ExtendedAbsenceSetting.AbsenceFirstDayStart;
                oldRequest.ExtendedAbsenceSetting.AbsenceWholeLastDay = updatedRequest.ExtendedAbsenceSetting.AbsenceWholeLastDay;
                oldRequest.ExtendedAbsenceSetting.AbsenceLastDayStart = updatedRequest.ExtendedAbsenceSetting.AbsenceLastDayStart;
                oldRequest.ExtendedAbsenceSetting.PercentalAbsence = updatedRequest.ExtendedAbsenceSetting.PercentalAbsence;
                oldRequest.ExtendedAbsenceSetting.PercentalValue = updatedRequest.ExtendedAbsenceSetting.PercentalValue;
                oldRequest.ExtendedAbsenceSetting.PercentalAbsenceOccursStartOfDay = updatedRequest.ExtendedAbsenceSetting.PercentalAbsenceOccursStartOfDay;
                oldRequest.ExtendedAbsenceSetting.PercentalAbsenceOccursEndOfDay = updatedRequest.ExtendedAbsenceSetting.PercentalAbsenceOccursEndOfDay;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsencePerWeekDay = updatedRequest.ExtendedAbsenceSetting.AdjustAbsencePerWeekDay;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceAllDaysStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceAllDaysStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceAllDaysStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceAllDaysStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceMonStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceMonStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceMonStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceMonStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceTueStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceTueStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceTueStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceTueStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceWedStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceWedStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceWedStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceWedStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceThuStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceThuStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceThuStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceThuStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceFriStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceFriStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceFriStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceFriStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceSatStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceSatStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceSatStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceSatStop;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceSunStart = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceSunStart;
                oldRequest.ExtendedAbsenceSetting.AdjustAbsenceSunStop = updatedRequest.ExtendedAbsenceSetting.AdjustAbsenceSunStop;
            }
            else
            {
                if (oldRequest.ExtendedAbsenceSettingId.HasValue)
                    oldRequest.ExtendedAbsenceSettingId = null; //disconnet request from settings
            }

            #endregion

            #endregion

            if (saveChanges)
                result = Save();

            if (!result.Success)
                return result;

            return result;
        }

        private ActionResult SaveEmployeeRequest(EmployeeRequest employeeRequest, int employeeId, TermGroup_EmployeeRequestType requestType, bool saveChanges = true)
        {
            ActionResult result = new ActionResult();

            if (employeeRequest == null || !employeeRequest.TimeDeviationCauseId.HasValue || employeeRequest.TimeDeviationCauseId.Value == 0)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");

            //Whole days
            if (employeeRequest.Start.TimeOfDay.TotalMinutes == 0 && employeeRequest.Stop.TimeOfDay.TotalMinutes == 0)
            {
                employeeRequest.Start = CalendarUtility.GetBeginningOfDay(employeeRequest.Start);
                employeeRequest.Stop = CalendarUtility.GetEndOfDay(employeeRequest.Stop);
            }

            #region Prereq

            string intersectMessage = "";
            bool requestIntersectsWithCurrent = false;

            if (employeeRequest.EmployeeRequestId == 0)
                requestIntersectsWithCurrent = DateIntervalIntersectsWithExistingEmployeeRequest(employeeRequest.Start, employeeRequest.Stop, employeeRequest.EmployeeId, employeeRequest.EmployeeRequestId, requestType, out intersectMessage);

            #endregion

            #region Perform

            if (employeeRequest.EmployeeRequestId == 0)
                employeeRequest = AddEmployeeRequest(employeeId, employeeRequest.TimeDeviationCauseId.Value, employeeRequest.Start, employeeRequest.Stop, employeeRequest.Comment, requestType, employeeRequest.EmployeeChildId, employeeRequest.ReActivate, employeeRequest.ExtendedAbsenceSetting);
            else
                result = UpdateEmployeeRequest(employeeRequest, false);

            if (employeeRequest != null && result.Success && saveChanges)
            {
                result = Save();
                result.IntegerValue = employeeRequest.EmployeeRequestId;
            }

            #endregion

            if (result.Success)
            {
                result.BooleanValue = requestIntersectsWithCurrent;
                result.ErrorMessage = intersectMessage;
            }

            return result;
        }

        private ActionResult DeleteEmployeeRequest(int deleteRequestId, bool saveChanges = true)
        {
            ActionResult result;

            User user = GetUserFromCache();

            EmployeeRequest oldRequest = GetEmployeeRequest(deleteRequestId);
            if (oldRequest == null)
                return new ActionResult(false);

            if (oldRequest.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest)
            {
                //Reverse shiftuserstatus to default
                result = SetShiftUserStatus(oldRequest, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted, null, saveChanges);
                if (!result.Success)
                    return result;
            }

            result = ChangeEntityState(entities, oldRequest, SoeEntityState.Deleted, saveChanges, user);
            if (!result.Success)
                return result;

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult HandleEmployeeRequestReActivation(int employeeId, List<SaveEmployeeSchedulePlacementItem> activations, ActivateScheduleControlDTO control)
        {
            if (!activations.IsNullOrEmpty())
            {
                if (control != null && activations.All(a => a.ShortSchedule))
                    return TryCreateEmployeeRequestsToReActivate(employeeId, SaveEmployeeSchedulePlacementItem.GetShortenIntervals(activations), control);
                if (activations.First().ExtendSchedule)
                    return TryReActivateEmployeeRequests(employeeId, SaveEmployeeSchedulePlacementItem.GetExtensionInterval(activations.FirstOrDefault()));
            }
            return new ActionResult(true);
        }

        private ActionResult TryCreateEmployeeRequestsToReActivate(ActivateScheduleControlDTO control, int employeeId, DateTime startDate, DateTime stopDate)
        {
            return TryCreateEmployeeRequestsToReActivate(employeeId, new DateRangeDTO(startDate, stopDate).ObjToList(), control);
        }

        private ActionResult TryCreateEmployeeRequestsToReActivate(int employeeId, List<DateRangeDTO> dateRanges, ActivateScheduleControlDTO control)
        {
            if (dateRanges.IsNullOrEmpty())
                return new ActionResult(true);

            Dictionary<int, bool> employeeRequestIds = control.GetEmployeeRequestIdsToReCreate(employeeId);
            if (employeeRequestIds.IsNullOrEmpty())
                return new ActionResult(true);

            return CreateEmployeeRequestsToReActivate(employeeRequestIds, employeeId, dateRanges);
        }
        private ActionResult CreateEmployeeRequestsToReActivate(Dictionary<int, bool> employeeRequestDicts, int employeeId, List<DateRangeDTO> dateRanges)
        {
            List<EmployeeRequest> employeeRequests = GetEmployeeRequests(employeeRequestDicts.Select(x => x.Key).ToList(), true);
            foreach (var employeeRequestDict in employeeRequestDicts)
            {
                var employeeRequest = employeeRequests.FirstOrDefault(x => x.EmployeeRequestId == employeeRequestDict.Key);
                if (employeeRequest != null)
                {
                    if (employeeRequestDict.Value)
                    {
                        bool reActivateOriginal = false;
                        foreach (var dateRange in dateRanges)
                        {
                            //ReActivate = true
                            if (CalendarUtility.IsCurrentOverlappedByNew(dateRange.Start, dateRange.Stop, employeeRequest.Start.Date, employeeRequest.Stop.Date))
                            {
                                reActivateOriginal = true;
                            }
                            else
                            {
                                var newRequest = CreateIntersectedRequest(employeeRequest, dateRange, true);
                                if (newRequest != null)
                                {
                                    newRequest.ReActivate = true;
                                    SaveEmployeeRequest(newRequest.FromDTO(), employeeId, TermGroup_EmployeeRequestType.AbsenceRequest, saveChanges: false);
                                }
                            }
                        }

                        if (reActivateOriginal)
                        {
                            employeeRequest.ReActivate = true;
                            employeeRequest.Status = (int)TermGroup_EmployeeRequestStatus.RequestPending;
                            employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.None;
                            SetModifiedProperties(employeeRequest);
                        }
                    }
                    else
                    {
                        //ReActivate = false
                        if (employeeRequest.ReActivate)
                        {
                            employeeRequest.ReActivate = false;
                            SetModifiedProperties(employeeRequest);
                        }
                    }
                }
            }

            return Save();
        }

        private ActionResult TryReActivateEmployeeRequests(int employeeId, DateRangeDTO dateRange)
        {
            if (dateRange == null)
                return new ActionResult(true);

            List<EmployeeRequest> employeeRequests = GetEmployeeRequests(employeeId, dateRange.Start, dateRange.Stop, new List<TermGroup_EmployeeRequestType> { TermGroup_EmployeeRequestType.AbsenceRequest }).Where(r => r.ReActivate).ToList();
            foreach (EmployeeRequest employeeRequest in employeeRequests)
            {
                employeeRequest.ReActivate = false;
                ActionResult result = SaveEmployeeRequest(employeeRequest, employeeId, TermGroup_EmployeeRequestType.AbsenceRequest, saveChanges: false);
                if (!result.Success)
                    return result;

                SendXEMailOnReActivatedEmployeeRequest(employeeId, employeeRequest);
            }
            return Save();
        }

        private ActionResult AbsenceRequestActionCopyShiftToGivenEmployee(TimeSchedulePlanningDayDTO shift, EmployeeRequest absenceRequest, Guid batchId, Guid link, bool skipXEMailOnShiftChanges)
        {
            int shiftBlockId = shift.TimeScheduleTemplateBlockId;

            #region Copy Schedule to given employee

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shiftBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanning, absenceRequest.EmployeeRequestId, skipXEMailOnShiftChanges);
            var scheduleBlock = GetScheduleBlock(shiftBlockId);
            if (scheduleBlock == null)
                return new ActionResult(false);

            TimeSchedulePlanningDayDTO shiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();
            ActionResult result = ValidateShiftAgainstGivenScenario(shiftCopy, null);
            if (!result.Success)
                return result;

            shift.Link = link;

            result = CopyTimeScheduleShiftToGivenEmployee(logProperties, shiftBlockId, shift);

            #endregion

            if (!result.Success)
                return result;

            #region Tasks (At this point shift has recived a new TimeScheduleTemplateBlockId => is is a new shift)

            List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = GetTimeScheduleTemplateBlockTasks(shiftBlockId).ToDTOs().ToList();

            result = ConnectTasksWithinShift(shiftTasks, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Update Shift status, flags and deviationcause on scheduleblock

            TimeDeviationCause deviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(absenceRequest.TimeDeviationCauseId.Value);
            if (deviationCause == null)
            {
                result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");
                return result;
            }

            scheduleBlock.TimeDeviationCauseId = absenceRequest.TimeDeviationCauseId.Value;
            scheduleBlock.EmployeeChildId = absenceRequest.EmployeeChildId;
            if (deviationCause.TimeCode != null)
            {
                scheduleBlock.TimeCode = deviationCause.TimeCode;
                scheduleBlock.TimeCodeId = deviationCause.TimeCode.TimeCodeId;
            }

            scheduleBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.Planned;

            result = SetShiftUserStatus(shiftBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved);

            #endregion

            if (!result.Success)
                return result;

            result = CreateTimeScheduleTemplateBlockHistoryEntry(shiftCopy, logProperties);

            if (!result.Success)
                return result;

            #region Clear Queue

            result = ClearShiftQueue(shiftBlockId);

            #endregion

            if (!result.Success)
                return result;

            return result;
        }

        private ActionResult AbsenceRequestActionCopyShiftToHiddenEmployee(TimeSchedulePlanningDayDTO shift, EmployeeRequest absenceRequest, Guid batchId, Guid link, bool skipXEMailOnShiftChanges)
        {
            int shiftBlockId = shift.TimeScheduleTemplateBlockId;

            #region Copy Schedule to hidden employee

            TimeDeviationCause deviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(absenceRequest.TimeDeviationCauseId.Value);
            if (deviationCause == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(shiftBlockId);
            if (scheduleBlock == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            TimeSchedulePlanningDayDTO shiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            ActionResult result = ValidateShiftAgainstGivenScenario(shiftCopy, null);
            if (!result.Success)
                return result;

            shift.Link = link;

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shiftBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanning, absenceRequest.EmployeeRequestId, skipXEMailOnShiftChanges);
            result = CopyTimeScheduleShiftToHiddenEmployee(logProperties, shiftBlockId, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Tasks (At this point shift has recived a new TimeScheduleTemplateBlockId => is is a new shift)

            List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = GetTimeScheduleTemplateBlockTasks(shiftBlockId).ToDTOs().ToList();

            result = ConnectTasksWithinShift(shiftTasks, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Update Shift status, flags and deviationcause on scheduleblock

            scheduleBlock.TimeDeviationCauseId = absenceRequest.TimeDeviationCauseId.Value;
            scheduleBlock.EmployeeChildId = absenceRequest.EmployeeChildId;
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

            #region Clear Queue

            result = ClearShiftQueue(shiftBlockId);
            if (!result.Success)
                return result;

            #endregion

            return result;
        }

        private ActionResult AbsenceRequestActionNoReplacementOnShiftIsNeeded(TimeSchedulePlanningDayDTO shift, EmployeeRequest absenceRequest, Guid batchId, bool skipXEMailOnShiftChanges)
        {
            int shiftBlockId = shift.TimeScheduleTemplateBlockId;

            #region Copy Schedule to hidden employee

            TimeDeviationCause deviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(absenceRequest.TimeDeviationCauseId.Value);
            if (deviationCause == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(shiftBlockId);
            if (scheduleBlock == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            TimeSchedulePlanningDayDTO shiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shiftBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanning, absenceRequest.EmployeeRequestId, skipXEMailOnShiftChanges)
            {
                NewShiftId = shiftBlockId
            };

            #endregion

            #region  Update Shift status, flags and deviationcause on scheduleblock

            scheduleBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.Planned;
            scheduleBlock.TimeDeviationCauseId = absenceRequest.TimeDeviationCauseId.Value;
            scheduleBlock.EmployeeChildId = absenceRequest.EmployeeChildId;
            if (deviationCause.TimeCode != null)
            {
                scheduleBlock.TimeCode = deviationCause.TimeCode;
                scheduleBlock.TimeCodeId = deviationCause.TimeCode.TimeCodeId;
            }

            ActionResult result = SetShiftUserStatus(shiftBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved);
            if (!result.Success)
                return result;

            #endregion

            #region Clear Queue

            result = ClearShiftQueue(shiftBlockId);
            if (!result.Success)
                return result;

            #endregion

            #region Create TemplateBlockHistory

            result = CreateTimeScheduleTemplateBlockHistoryEntry(shiftCopy, logProperties);
            if (!result.Success)
                return result;

            #endregion

            #region Create history for NoReplacement (simulate that NoReplacement har recieved a shift)

            TimeScheduleTemplateBlockHistory logEntryForNoReplacement = new TimeScheduleTemplateBlockHistory()
            {
                TimeScheduleTemplateBlockId = shiftCopy.TimeScheduleTemplateBlockId,
                BatchId = logProperties.BatchId.ToString(),
                Type = (int)logProperties.HistoryType,
                RecordId = logProperties.RecordId,
                ToEmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID,
                ToStart = shiftCopy.StartTime,
                ToStop = shiftCopy.StopTime,
                ToShiftTypeId = shiftCopy.ShiftTypeId,
                ToExtraShift = shiftCopy.ExtraShift,
                OriginEmployeeId = shiftCopy.EmployeeId,

                //Set FK
                ActorCompanyId = this.actorCompanyId,
            };
            SetCreatedProperties(logEntryForNoReplacement);
            entities.AddToTimeScheduleTemplateBlockHistory(logEntryForNoReplacement);

            result = Save();
            if (!result.Success)
                return result;

            #endregion

            return result;
        }

        private ActionResult AbsenceReqestActionShiftIsNotApproved(TimeSchedulePlanningDayDTO shift, EmployeeRequest absenceRequest, Guid batchId, bool skipXEMailOnShiftChanges)
        {
            #region Update ShiftUserStatus

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.AbsenceRequestPlanning, absenceRequest.EmployeeRequestId, skipXEMailOnShiftChanges)
            {
                NewShiftId = shift.TimeScheduleTemplateBlockId
            };

            TimeScheduleTemplateBlock originalShift = GetScheduleBlock(shift.TimeScheduleTemplateBlockId);
            TimeSchedulePlanningDayDTO originalShiftDto = originalShift?.ToTimeSchedulePlanningDayDTO();

            ActionResult result = SetShiftUserStatus(shift.TimeScheduleTemplateBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted);
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

        private ActionResult UpdateEmployeeRequestStatus(EmployeeRequest employeeRequest, ref bool isDefinate, ref int shiftsLeftToProcess, bool saveChanges = true)
        {
            ActionResult result = new ActionResult();

            if (employeeRequest.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest)
            {
                bool includeZeroDays = false;
                if (employeeRequest.TimeDeviationCauseId.HasValue)
                {
                    TimeDeviationCause timdeDeviationCause = GetTimeDeviationCauseFromCache(employeeRequest.TimeDeviationCauseId.Value);
                    if (timdeDeviationCause != null)
                        includeZeroDays = timdeDeviationCause.ShowZeroDaysInAbsencePlanning;

                }

                if (!employeeRequest.ExtendedAbsenceSettingReference.IsLoaded)
                    employeeRequest.ExtendedAbsenceSettingReference.Load();

                shiftsLeftToProcess = TimeScheduleManager.GetAbsenceRequestAffectedShifts(entities, actorCompanyId, employeeRequest.EmployeeId, null, employeeRequest.Start, employeeRequest.Stop, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested, employeeRequest.ExtendedAbsenceSetting, includeZeroDays, employeeRequest.TimeDeviationCauseId).Count();
                bool affectedDaysExists = shiftsLeftToProcess > 0;
                if (affectedDaysExists)
                {
                    employeeRequest.Status = (int)TermGroup_EmployeeRequestStatus.PartlyDefinate;
                }
                else
                {
                    employeeRequest.Status = (int)TermGroup_EmployeeRequestStatus.Definate;
                    isDefinate = true;
                }
                SetModifiedProperties(employeeRequest);
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult UpdateEmployeeRequestResultStatus(EmployeeRequest employeeRequest, List<TimeSchedulePlanningDayDTO> currentPlannedShifts, bool isDefinate, bool saveChanges = true)
        {
            //isDefinate = no more shifts to plan exists

            ActionResult result = new ActionResult();

            if (employeeRequest.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest && currentPlannedShifts.Any())
            {
                bool allCurrentPlannedShiftsAreApproved = currentPlannedShifts.All(x => x.ApprovalTypeId == (int)TermGroup_YesNo.Yes);
                bool allCurrentPlannedlShiftsAreDenied = currentPlannedShifts.All(x => x.ApprovalTypeId == (int)TermGroup_YesNo.No);

                switch ((TermGroup_EmployeeRequestResultStatus)employeeRequest.ResultStatus)//Current resultstatus
                {
                    case TermGroup_EmployeeRequestResultStatus.None:
                        #region Can escalate to all other statuses
                        if (isDefinate) //= no more shifts to plan exists
                        {
                            if (allCurrentPlannedShiftsAreApproved)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.FullyGranted;
                            }
                            else if (allCurrentPlannedlShiftsAreDenied)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.FullyDenied;
                            }
                            else
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied;
                            }
                        }
                        else
                        {
                            if (allCurrentPlannedShiftsAreApproved)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGranted;
                            }
                            else if (allCurrentPlannedlShiftsAreDenied)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyDenied;
                            }
                            else
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied;
                            }
                        }
                        #endregion
                        break;
                    case TermGroup_EmployeeRequestResultStatus.PartlyGranted:

                        #region Can escalate to FullyGranted or PartlyGrantedPartlyDenied or stay on PartlyGranted
                        if (isDefinate)
                        {
                            if (allCurrentPlannedShiftsAreApproved)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.FullyGranted;
                            }
                            else
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied;
                            }

                        }
                        else
                        {
                            if (allCurrentPlannedShiftsAreApproved)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGranted;
                            }
                            else
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied;
                            }
                        }
                        #endregion

                        break;
                    case TermGroup_EmployeeRequestResultStatus.PartlyDenied:

                        #region Can escalate to FullyDenied or PartlyGrantedPartlyDenied or stay on PartlyDenied
                        if (isDefinate)
                        {
                            if (allCurrentPlannedlShiftsAreDenied)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.FullyDenied;
                            }
                            else
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied;
                            }

                        }
                        else
                        {
                            if (allCurrentPlannedlShiftsAreDenied)
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyDenied;
                            }
                            else
                            {
                                employeeRequest.ResultStatus = employeeRequest.ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied;
                            }
                        }
                        #endregion

                        break;
                    case TermGroup_EmployeeRequestResultStatus.PartlyGrantedPartlyDenied:
                    case TermGroup_EmployeeRequestResultStatus.FullyGranted:
                    case TermGroup_EmployeeRequestResultStatus.FullyDenied:
                    default:
                        break;
                }




                SetModifiedProperties(employeeRequest);
            }

            if (saveChanges)
                result = Save();

            return result;
        }

        private void SetShiftRequestsToDeleted(List<int> timeScheduleTemplateBlockIds)
        {
            List<Message> messages = (from m in this.entities.Message.Include("MessageRecipient")
                                      where m.ActorCompanyId == this.ActorCompanyId &&
                                      timeScheduleTemplateBlockIds.Contains(m.RecordId) &&
                                      m.Type == (int)TermGroup_MessageType.ShiftRequest &&
                                      m.State != (int)SoeEntityState.Deleted
                                      select m).ToList();

            foreach (var message in messages)
            {
                message.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(message);

                foreach (MessageRecipient recipient in message.MessageRecipient)
                {
                    recipient.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(recipient);
                }
            }
        }

        #endregion

        #region Period

        public EmployeePeriodTimeSummary GetEmployeePeriodTimeSummaryForEmployee(int employeeId, TimePeriod period, List<SysExtraField> sysExtraFields = null)
        {
            if (period == null)
                return null;

            int? timePeriodHeadId = period.TimePeriodHead?.TimePeriodHeadId ?? GetTimePeriodHeadId(period.TimePeriodId);
            if (!timePeriodHeadId.HasValue)
                return null;

            TimePeriodHead parentHead = GetTimePeriodHeadWithPeriods(timePeriodHeadId.Value);
            if (parentHead == null || parentHead.TimePeriod == null)
                return null;

            return GetEmployeePeriodTimeSummariesForEmployees(employeeId.ObjToList(), parentHead, period.TimePeriodId, sysExtraFields: sysExtraFields).FirstOrDefault();
        }
        public EmployeePeriodTimeSummary GetEmployeePeriodTimeSummaryForEmployee(int employeeId, DateTime dateFrom, DateTime dateTo, int timePeriodHeadId)
        {
            List<EmployeePeriodTimeSummary> summeries = GetEmployeePeriodTimeSummariesForEmployees(employeeId.ObjToList(), dateFrom, dateTo, timePeriodHeadId);
            return summeries.FirstOrDefault();
        }

        public List<EmployeePeriodTimeSummary> GetEmployeePeriodTimeSummariesForEmployees(List<int> employeeIds, DateTime dateFrom, DateTime dateTo, int timePeriodHeadId, List<SysExtraField> sysExtraFields = null)
        {
            dateTo = dateTo.Date;

            TimePeriodHead parentHead = GetTimePeriodHeadWithPeriods(timePeriodHeadId);
            if (parentHead == null || parentHead.TimePeriod == null)
                return new List<EmployeePeriodTimeSummary>();

            TimePeriod matchingParentPeriod = parentHead.TimePeriod.FirstOrDefault(x => x.StartDate <= dateFrom && x.StopDate >= dateTo && x.State == (int)SoeEntityState.Active);
            if (matchingParentPeriod == null)
                return new List<EmployeePeriodTimeSummary>();

            TimePeriodHead childHead = parentHead.ChildId.HasValue ? GetTimePeriodHeadWithPeriods(parentHead.ChildId.Value) : null;
            TimePeriod matchingChildPeriod = childHead?.TimePeriod.FirstOrDefault(x => x.StartDate == dateFrom && x.StopDate == dateTo && x.State == (int)SoeEntityState.Active);

            return GetEmployeePeriodTimeSummariesForEmployees(employeeIds, parentHead, matchingParentPeriod.TimePeriodId, matchingChildPeriod?.TimePeriodId, dateFrom, dateTo, sysExtraFields);
        }

        public List<EmployeePeriodTimeSummary> GetEmployeePeriodTimeSummariesForEmployees(List<int> employeeIds, int parentPeriodHeadId, int timePeriodId, int? childPeriodId = null, DateTime? selectionDateFrom = null, DateTime? selectionToDate = null, List<SysExtraField> sysExtraFields = null)
        {
            TimePeriodHead parentHead = GetTimePeriodHeadWithPeriods(parentPeriodHeadId);
            return GetEmployeePeriodTimeSummariesForEmployees(employeeIds, parentHead, timePeriodId, childPeriodId, selectionDateFrom, selectionToDate, sysExtraFields);
        }

        public List<EmployeePeriodTimeSummary> GetEmployeePeriodTimeSummariesForEmployees(List<int> employeeIds, TimePeriodHead parentPeriodHead, int timePeriodId, int? childPeriodId = null, DateTime? selectionDateFrom = null, DateTime? selectionToDate = null, List<SysExtraField> sysExtraFields = null)
        {
            if (selectionToDate.HasValue)
                selectionToDate = selectionToDate.Value.Date;

            if (parentPeriodHead?.TimePeriod == null)
                return new List<EmployeePeriodTimeSummary>();

            TimePeriod parentPeriod = parentPeriodHead.TimePeriod.FirstOrDefault(f => f.TimePeriodId == timePeriodId);
            if (parentPeriod == null)
                return new List<EmployeePeriodTimeSummary>();

            if (!childPeriodId.HasValue && parentPeriodHead?.ChildId != null && selectionDateFrom.HasValue && selectionToDate.HasValue)
                childPeriodId = entities.TimePeriod.FirstOrDefault(x => x.TimePeriodHead.TimePeriodHeadId == parentPeriodHead.ChildId.Value && x.StartDate >= selectionDateFrom.Value && x.StopDate <= selectionToDate.Value && x.State == (int)SoeEntityState.Active)?.TimePeriodId;

            TimePeriod childPeriod = childPeriodId.HasValue ? entities.TimePeriod.FirstOrDefault(x => x.TimePeriodId == childPeriodId.Value) : null;
            if (childPeriod != null && (childPeriod.StartDate < parentPeriod.StartDate || childPeriod.StopDate > parentPeriod.StopDate))
                throw new ArgumentException("Child period dates must be within the parent period dates.");

            // Get Attest States
            //List<AttestState> attestStates = AttestManager.GetAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);

            // Get lowest attest level for payroll
            //AttestState attestStateAttested = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollMinimumAttestStatus);

            // Get a dictionary of payrollProducts to exclude in worked time calculations
            List<PayrollProductSettingLookupDTO> lookupExcludePayrollProduct = ProductManager.GetPayrollProductSettingsFromSysLookup(entities, actorCompanyId, SysExtraFieldType.AveragingOvertimeCalculation, sysExtraFields);

            // Get shared data for the parent period
            var sharedDataDict = GetSharedDataForEmployees(employeeIds, parentPeriod.StartDate, parentPeriod.StopDate); //attestStateAttested

            // Load employeeCalendars for all employees
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, actorCompanyId);
            List<EmploymentCalenderDTO> employmentCalenderDTOs = EmployeeManager.GetEmploymentCalenderDTOs(sharedDataDict.Values.Select(s => s.Employee).ToList(), parentPeriod.StartDate, parentPeriod.StopDate, payrollGroups: payrollGroups);

            // Fetch employee group rule work time periods
            var timePeriodIds = new List<int> { parentPeriod.TimePeriodId };
            if (childPeriod != null)
                timePeriodIds.Add(childPeriod.TimePeriodId);
            var employeeGroupRuleWorkTimePeriods = entities.EmployeeGroupRuleWorkTimePeriod.Where(w => timePeriodIds.Contains(w.TimePeriodId)).ToList();

            // Create summaries for each employee
            List<EmployeePeriodTimeSummary> summaries = new List<EmployeePeriodTimeSummary>();
            foreach (var employeeId in employeeIds)
            {
                if (!sharedDataDict.TryGetValue(employeeId, out var employeeData))
                    continue;

                // Set EmploymentCalendar for employee
                employeeData.EmploymentCalendar = employmentCalenderDTOs.Where(e => e.EmployeeId == employeeId).ToList();

                var stopDateParent = parentPeriod.StopDate;

                if (selectionToDate.HasValue && selectionToDate.Value < parentPeriod.StopDate)
                    stopDateParent = selectionToDate.Value;

                var parentStartDate = parentPeriod.StartDate;
                var parentStopDate = stopDateParent;
                var parentWorkTimeStopDate = stopDateParent;
                var parentScheduleStartDate = parentStartDate;
                var parentNoScheduleForCalc = false;
                var parentNoWorkedTimeForCalc = false;

                if (DateTime.Today > parentStopDate)
                    parentNoScheduleForCalc = true;

                if (DateTime.Today < parentStartDate)
                    parentNoWorkedTimeForCalc = true;

                if (DateTime.Today > parentStartDate && DateTime.Today < parentStopDate)
                {
                    parentScheduleStartDate = DateTime.Today;
                    parentWorkTimeStopDate = DateTime.Today.AddDays(-1);
                }

                // calculate parent period reduction due to payroll product settings
                int parentWorkedTimeReduction = CalculateWorkedTimeReduction(employeeData, lookupExcludePayrollProduct, parentPeriod.StartDate, parentWorkTimeStopDate);

                var parentScheduledTime = CalculateScheduledTime(employeeData.ScheduleBlocks, parentScheduleStartDate, stopDateParent);
                var parentScheduledTimeForCalc = parentNoScheduleForCalc ? 0 : parentScheduledTime;
                var parentWorkedTime = CalculateWorkedTime(employeeData.Transactions.Where(x => !x.PlanningPeriodCalculationId.HasValue).ToList(), parentPeriod.StartDate, parentWorkTimeStopDate, false) - parentWorkedTimeReduction;
                var parentWorkedTimeAttested = CalculateWorkedTime(employeeData.Transactions.Where(x => !x.PlanningPeriodCalculationId.HasValue).ToList(), parentPeriod.StartDate, parentWorkTimeStopDate) - parentWorkedTimeReduction;
                var parentWorkedTimeUnattested = parentWorkedTime - parentWorkedTimeAttested;
                var parentWorkedTimeForCalc = parentNoWorkedTimeForCalc ? 0 : parentWorkedTimeAttested;
                var parentPayrollWorkedTime = CalculateWorkedTime(employeeData.Transactions.Where(w => !w.IsAddedOrOverTime()).ToList(), parentPeriod.StartDate, parentWorkTimeStopDate) - parentWorkedTimeReduction;
                var parentPayrollOvertime = CalculateWorkedTime(employeeData.Transactions.Where(x => x.PlanningPeriodCalculationId.HasValue && x.IsAddedOrOverTime()).ToList(), parentPeriod.StartDate, parentWorkTimeStopDate);

                // Get rule work time for parent period
                int parentRuleWorkTime = GetEmployeeGroupRuleWorkTime(employeeData.Employee, employeeGroupRuleWorkTimePeriods, parentPeriod.StartDate, stopDateParent, parentPeriod.TimePeriodId);
                int parentTotalWorkTimeMinutes = GetWorkTimeWeekForPeriod(employeeData.Employee, parentPeriod.StartDate, stopDateParent);

                // Calculate child period metrics if child period is provided
                int childScheduledTime = 0, childWorkedTime = 0, childWorkedTimeAttested = 0, childWorkedTimeUnattested = 0, childRuleWorkTime = 0, childTotalWorkTimeMinutes = 0, childPayrollWorkedTime = 0, childScheduledTimeForCalc = 0, childWorkedTimeForCalc = 0, childPayrollOvertime = 0;
                bool noScheduleForCalc = false, noWorkedTimeForCalc = false;
                if (childPeriod != null || (selectionDateFrom.HasValue && selectionToDate.HasValue))
                {
                    var startDate = selectionDateFrom ?? childPeriod.StartDate;
                    var stopDate = selectionToDate ?? childPeriod.StopDate;
                    var workTimeStopDate = stopDate;
                    var scheduleStartDate = startDate;

                    if (DateTime.Today > stopDate)
                        noScheduleForCalc = true;

                    if (DateTime.Today < startDate)
                        noWorkedTimeForCalc = true;

                    if (DateTime.Today > startDate && DateTime.Today < stopDate)
                    {
                        scheduleStartDate = DateTime.Today;
                        workTimeStopDate = DateTime.Today.AddDays(-1);
                    }

                    // calculate child period reduction due to payroll product settings
                    int childWorkedTimeReduction = CalculateWorkedTimeReduction(employeeData, lookupExcludePayrollProduct, startDate, workTimeStopDate);

                    childScheduledTime = CalculateScheduledTime(employeeData.ScheduleBlocks, scheduleStartDate, stopDate);
                    childScheduledTimeForCalc = noScheduleForCalc ? 0 : childScheduledTime;
                    childWorkedTime = CalculateWorkedTime(employeeData.Transactions.Where(x => !x.PlanningPeriodCalculationId.HasValue).ToList(), startDate, workTimeStopDate, false) - childWorkedTimeReduction;
                    childWorkedTimeAttested = CalculateWorkedTime(employeeData.Transactions.Where(x => !x.PlanningPeriodCalculationId.HasValue).ToList(), startDate, workTimeStopDate) - childWorkedTimeReduction;
                    childWorkedTimeUnattested = childWorkedTime - childWorkedTimeAttested;
                    childWorkedTimeForCalc = noWorkedTimeForCalc ? 0 : childWorkedTime;
                    childRuleWorkTime = GetEmployeeGroupRuleWorkTime(employeeData.Employee, employeeGroupRuleWorkTimePeriods, startDate, stopDate, childPeriod?.TimePeriodId ?? 0);
                    childTotalWorkTimeMinutes = GetWorkTimeWeekForPeriod(employeeData.Employee, startDate, stopDate);
                    childPayrollWorkedTime = CalculateWorkedTime(employeeData.Transactions.Where(w => !w.IsAddedOrOverTime()).ToList(), startDate, workTimeStopDate) - childWorkedTimeReduction;
                    childPayrollOvertime = CalculateWorkedTime(employeeData.Transactions.Where(x => x.PlanningPeriodCalculationId.HasValue && x.IsAddedOrOverTime()).ToList(), startDate, workTimeStopDate);
                }

                // Calculate additional work time for periods after the last recorded schedule date
                var lastScheduleDate = employeeData.ScheduleBlocks.Max(x => x.Date);
                int parentTotalTime = parentWorkedTimeAttested + parentScheduledTimeForCalc;
                int childTotalTime = childWorkedTimeAttested + childScheduledTimeForCalc;
                int parentPayrollTotalTime = parentPayrollWorkedTime + parentScheduledTimeForCalc;
                int childPayrollTotalTime = childPayrollWorkedTime + childScheduledTimeForCalc;

                if (lastScheduleDate.HasValue && lastScheduleDate < stopDateParent)
                {
                    DateTime parentCurrentMonday = CalendarUtility.GetBeginningOfWeek(lastScheduleDate.Value.AddDays(1));
                    while (parentCurrentMonday <= parentPeriod.StopDate)
                    {
                        parentTotalTime += employeeData.Employee.GetEmployment(parentCurrentMonday)?.GetWorkTimeWeek(parentCurrentMonday) ?? 0;
                        parentCurrentMonday = parentCurrentMonday.AddDays(7);
                    }

                    if (childPeriod != null)
                    {
                        DateTime childCurrentMonday = CalendarUtility.GetBeginningOfWeek(lastScheduleDate.Value.AddDays(1));
                        while (childCurrentMonday <= childPeriod.StopDate)
                        {
                            childTotalTime += employeeData.Employee.GetEmployment(childCurrentMonday)?.GetWorkTimeWeek(childCurrentMonday) ?? 0;
                            childCurrentMonday = childCurrentMonday.AddDays(7);
                        }
                    }
                }

                //if the days are missing a timeTransactions on scheduled day, we need to add time from the schedule instead (not on payrollroll balancee)
                var adjustmentBalanceParent = 0;
                var adjustmentBalanceChild = 0;
                foreach (var block in employeeData.ScheduleBlocks.Where(w => w.Date.HasValue && w.StartTime != w.StopTime && w.Date < DateTime.Today).GroupBy(g => g.Date.Value))
                {
                    // If there are no transactions that are attested or higher for the date, add the scheduled time to the balance
                    if (!employeeData.Transactions.Any(a => a.TimeBlockDate.Date == block.Key))
                    {
                        var adjustmentForDate = CalculateScheduledTime(employeeData.ScheduleBlocks, block.Key, block.Key);

                        if (block.Key >= parentPeriod.StartDate && block.Key <= parentWorkTimeStopDate)
                            adjustmentBalanceParent += adjustmentForDate;
                        if (childPeriod != null && block.Key >= childPeriod.StartDate && block.Key <= childPeriod.StopDate)
                            adjustmentBalanceChild += adjustmentForDate;
                    }
                }

                // Add unattested transaction minutes to adjustment
                adjustmentBalanceChild += childWorkedTimeUnattested;
                adjustmentBalanceParent += parentWorkedTimeUnattested;

                //parentWorkedTime += adjustmentBalanceParent;
                //childWorkedTime += adjustmentBalanceChild;
                parentScheduledTimeForCalc += adjustmentBalanceParent;
                childScheduledTimeForCalc += adjustmentBalanceChild;
                parentTotalTime += adjustmentBalanceParent;
                childTotalTime += adjustmentBalanceChild;

                // Calculate balances
                int parentBalance = parentTotalTime - parentTotalWorkTimeMinutes;
                int childBalance = childTotalTime - childTotalWorkTimeMinutes;

                // Calcutate payroll balances
                int parentPayrollBalance = parentPayrollTotalTime - parentRuleWorkTime - parentPayrollOvertime;
                int childPayrollBalance = childPayrollTotalTime - childRuleWorkTime - childPayrollOvertime;

                summaries.Add(new EmployeePeriodTimeSummary
                {
                    EmployeeId = employeeId,
                    ParentTimePeriodId = parentPeriod.TimePeriodId,
                    ParentScheduledTimeMinutes = parentScheduledTimeForCalc,
                    ParentWorkedTimeMinutes = parentWorkedTimeAttested,
                    ParentRuleWorkedTimeMinutes = parentTotalWorkTimeMinutes,
                    ParentPeriodBalanceTimeMinutes = parentBalance,
                    ParentPayrollRuleWorkedTimeMinutes = parentRuleWorkTime,
                    ParentPayrollPeriodBalanceTimeMinutes = parentPayrollBalance,
                    ChildTimePeriodId = childPeriod?.TimePeriodId,
                    ChildScheduledTimeMinutes = childScheduledTimeForCalc,
                    ChildWorkedTimeMinutes = childWorkedTimeAttested,
                    ChildRuleWorkedTimeMinutes = childTotalWorkTimeMinutes,
                    ChildPeriodBalanceTimeMinutes = childBalance,
                    ChildPayrollRuleWorkedTimeMinutes = childRuleWorkTime,
                    ChildPayrollPeriodBalanceTimeMinutes = childPayrollBalance,
                });
            }

            return summaries;
        }

        private Dictionary<int, EmployeePeriodData> GetSharedDataForEmployees(List<int> employeeIds, DateTime startDate, DateTime stopDate, AttestState lowestAttestState = null)
        {
            // Fetch employee data only once for the entire period range
            var employees = GetEmployeesWithEmployment(employeeIds);
            var transactions = GetTimePayrollTransactions(employeeIds, startDate, stopDate, true)
                .Where(w => w.IsWorkTime() || w.IsAddedOrOverTime() || w.IsAbsence())
                .GroupBy(g => g.EmployeeId)
                .ToDictionary(k => k.Key, v => v.ToList());

            var scheduleBlocks = entities.TimeScheduleTemplateBlock
                .Where(x => x.EmployeeId.HasValue && employeeIds.Contains(x.EmployeeId.Value) && x.Date >= startDate && x.Date <= stopDate && x.State == (int)SoeEntityState.Active && x.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule && !x.TimeScheduleScenarioHeadId.HasValue)
                .GroupBy(g => g.EmployeeId.Value)
                .ToDictionary(k => k.Key, v => v.ToList());

            // Filter transactions based on attest state
            if (lowestAttestState != null)
                transactions = transactions.ToDictionary(k => k.Key, v => v.Value.Where(w => w.AttestState.Sort >= lowestAttestState.Sort).ToList());

            return employees.ToDictionary(e => e.EmployeeId, e => new EmployeePeriodData
            {
                Employee = e,
                Transactions = transactions.ContainsKey(e.EmployeeId) ? transactions[e.EmployeeId] : new List<TimePayrollTransaction>(),
                ScheduleBlocks = scheduleBlocks.ContainsKey(e.EmployeeId) ? scheduleBlocks[e.EmployeeId] : new List<TimeScheduleTemplateBlock>(),
                PeriodStart = startDate,
                PeriodStop = stopDate,
            });
        }

        private int CalculateScheduledTime(List<TimeScheduleTemplateBlock> scheduleBlocks, DateTime startDate, DateTime stopDate)
        {
            return scheduleBlocks
                .Where(block => block.Date >= startDate && block.Date <= stopDate && block.Type == (int)TimeScheduleBlockType.Schedule && !block.TimeScheduleScenarioHeadId.HasValue).ToList().GetWorkTimeForMultipleDates();
        }

        private int GetEmployeeGroupRuleWorkTime(Employee employee, List<EmployeeGroupRuleWorkTimePeriod> workTimePeriods, DateTime startDate, DateTime stopDate, int timePeriodId)
        {
            // Get the work time rule for the employee based on the periods provided
            var employeeGroupId = employee.GetEmployeeGroupId(startDate);
            var ruleWorkTime = workTimePeriods.FirstOrDefault(x => x.EmployeeGroupId == employeeGroupId && x.TimePeriodId == timePeriodId)?.RuleWorkTime ?? 0;

            if (ruleWorkTime == 0)
            {
                // Calculate rule work time manually if not defined in the rule periods
                DateTime currentMonday = CalendarUtility.GetBeginningOfWeek(startDate);
                while (currentMonday <= stopDate)
                {
                    ruleWorkTime += employee.GetEmployment(currentMonday)?.GetWorkTimeWeek(currentMonday) ?? 0;
                    currentMonday = currentMonday.AddDays(7);
                }
            }

            return ruleWorkTime;
        }

        private int GetWorkTimeWeekForPeriod(Employee employee, DateTime startDate, DateTime stopDate)
        {
            var ruleWorkTime = 0;
            // Calculate rule work time manually if not defined in the rule periods
            DateTime currentMonday = CalendarUtility.GetBeginningOfWeek(startDate);
            while (currentMonday <= stopDate)
            {
                ruleWorkTime += employee.GetEmployment(currentMonday)?.GetWorkTimeWeek(currentMonday) ?? 0;
                currentMonday = currentMonday.AddDays(7);
            }
            return ruleWorkTime;
        }

        private int CalculateWorkedTime(List<TimePayrollTransaction> transactions, DateTime startDate, DateTime stopDate, bool excludeInitialAttestState = true)
        {
            if (excludeInitialAttestState)
            {
                AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                transactions = transactions.Where(transaction => transaction.AttestStateId != attestStateInitial.AttestStateId).ToList();
            }

            return Convert.ToInt32(transactions
                .Where(transaction => transaction.TimeBlockDate.Date >= startDate && transaction.TimeBlockDate.Date <= stopDate)
                .Sum(transaction => transaction.Quantity));
        }

        private int CalculateWorkedTimeReduction(EmployeePeriodData employeeData, List<PayrollProductSettingLookupDTO> lookupExcludePayrollProduct, DateTime startDate, DateTime stopDate)
        {
            List<TimePayrollTransaction> transactions = new List<TimePayrollTransaction>();
            List<TimePayrollTransaction> possibleTransactionsToExclude = employeeData.Transactions.Where(x => x.TimeBlockDate.Date >= startDate && x.TimeBlockDate.Date <= stopDate && !x.PlanningPeriodCalculationId.HasValue && lookupExcludePayrollProduct.Where(l => l.GetBoolValue()).Select(l => l.PayrollProductId).Contains(x.PayrollProduct.ProductId)).ToList();

            foreach (var possibleTransaction in possibleTransactionsToExclude)
            {
                // add transaction that has a payrollproduct setting to exclude based on employees payrollGroupId on the transaction date, or settings with payrollGroupId == null (all)
                int payrollGroupId = employeeData.EmploymentCalendar.FirstOrDefault(x => x.Date == possibleTransaction.Date)?.PayrollGroupId ?? 0;
                if (lookupExcludePayrollProduct.Any(l => (l.PayrollGroupId == payrollGroupId || l.PayrollGroupId == null) && l.PayrollProductId == possibleTransaction.PayrollProduct.ProductId))
                {
                    var excludeForCurrentPayrollGroup = lookupExcludePayrollProduct.FirstOrDefault(l => l.PayrollGroupId == payrollGroupId && l.PayrollProductId == possibleTransaction.PayrollProduct.ProductId);
                    var excludeForAllPayrollGroup = lookupExcludePayrollProduct.FirstOrDefault(l => l.PayrollGroupId == null && l.PayrollProductId == possibleTransaction.PayrollProduct.ProductId);
                    if (excludeForCurrentPayrollGroup != null)
                    {
                        if (excludeForCurrentPayrollGroup.GetBoolValue())
                            transactions.Add(possibleTransaction);
                    }
                    else if (excludeForAllPayrollGroup != null)
                    {
                        if (excludeForAllPayrollGroup.GetBoolValue())
                            transactions.Add(possibleTransaction);
                    }
                }   
            }
            return Convert.ToInt32(transactions.Sum(t => t.Quantity));
        }

        public class EmployeePeriodData
        {
            public List<TimePayrollTransaction> Transactions { get; set; }
            public List<TimeScheduleTemplateBlock> ScheduleBlocks { get; set; }
            public List<AnnualLeaveTransaction> IngoingAnnualLeaveTransactions { get; set; }
            public Employee Employee { get; set; }
            public List<EmploymentCalenderDTO> EmploymentCalendar { get; set; }
            public Employment FirstEmployment { get; set; }
            public DateTime PeriodStart { get; set; }
            public DateTime PeriodStop { get; set; }
        }

        #endregion

        #region Schedule - shift link

        private Guid GetNewShiftLink()
        {
            return Guid.NewGuid();
        }

        private Guid GetNewBatchLink()
        {
            return Guid.NewGuid();
        }

        private ActionResult UpdateLinkOnShifts(int? timeScheduleScenarioHeadId, Guid newLink, int employeeId, DateTime date, int sourceShiftId)
        {
            var shifts = (from tb in entities.TimeScheduleTemplateBlock
                          where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == employeeId) &&
                          (tb.Date.HasValue && tb.Date.Value == date.Date) &&
                          (tb.State == (int)SoeEntityState.Active))
                          select tb).ToList();

            shifts = shifts.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();

            var sourceShift = shifts.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == sourceShiftId);
            int sourceType = -1;
            if (sourceShift != null)
                sourceType = sourceShift.Type;

            foreach (var shift in shifts.Where(s => s.Type == sourceType).ToList())
            {
                shift.Link = newLink.ToString();
            }

            ActionResult result = Save();

            return result;
        }

        private void SetTimeScheduleTemplateBlockLinked(TimeScheduleTemplateBlock templateBlock, string link = "", Dictionary<string, string> linksRelationDict = null)
        {
            if (String.IsNullOrEmpty(link))
            {
                //Generate new unique link for empty links
                link = GetNewShiftLink().ToString();
            }
            else if (linksRelationDict != null)
            {
                //Preserve linked together, but generate new unique link for them
                if (!linksRelationDict.ContainsKey(link))
                    linksRelationDict.Add(link, GetNewShiftLink().ToString());
                link = linksRelationDict[link];
            }

            if (templateBlock.Link != link)
            {
                templateBlock.Link = link;
                if (templateBlock.TimeScheduleTemplateBlockId > 0)
                    SetModifiedProperties(templateBlock);
            }
        }

        #endregion

        #region Schedule - tasks

        private ActionResult AssignTaskToEmployee(int employeeId, DateTime date, List<StaffingNeedsTaskDTO> staffingNeedsTaskDTOs, bool skipXEMailOnChanges)
        {
            if (staffingNeedsTaskDTOs.Any(x => !x.StartTime.HasValue || !x.StopTime.HasValue))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8766, "En eller flera arbetsuppgifter saknar tider."));

            ActionResult result;

            List<TimeScheduleTemplateBlock> existingShifts = GetScheduleBlocksForEmployeeWithTasks(null, employeeId, date);
            Guid batchId = GetNewBatchLink();

            if (!existingShifts.Any())
            {
                foreach (var staffingNeedsTaskDTO in staffingNeedsTaskDTOs)
                {
                    result = CreateShiftAndAssignTask(employeeId, date, staffingNeedsTaskDTO, skipXEMailOnChanges, batchId);
                    if (!result.Success)
                        return result;
                }
            }
            else
            {
                //Try connected tasks to existing shifts. If no shift exists during a tasks start and stop then create a new shift
                foreach (var staffingNeedsTaskDTO in staffingNeedsTaskDTOs)
                {
                    DateTime taskStartTime = CalendarUtility.MergeDateAndTime(date, staffingNeedsTaskDTO.StartTime.Value.RemoveSeconds());
                    DateTime taskStopTime = taskStartTime.Add(staffingNeedsTaskDTO.StopTime.Value.RemoveSeconds() - staffingNeedsTaskDTO.StartTime.Value.RemoveSeconds());

                    TimeScheduleTemplateBlock overlappingShift = existingShifts.FirstOrDefault(x => x.ActualStartTime.Value <= taskStartTime && x.ActualStopTime.Value >= taskStopTime);
                    if (overlappingShift == null)
                    {
                        result = CreateShiftAndAssignTask(employeeId, date, staffingNeedsTaskDTO, skipXEMailOnChanges, batchId);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        //Check if absence
                        if (overlappingShift.TimeDeviationCauseId.HasValue)
                            return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8769, "Arbetsuppgift {0} {1}-{2} kan inte tillsättas eftersom den anställde är frånvarande under den tiden."), staffingNeedsTaskDTO.Name, staffingNeedsTaskDTO.StartTime.Value.ToShortTimeString(), staffingNeedsTaskDTO.StopTime.Value.ToShortTimeString()));

                        //check if shift is valid
                        if (staffingNeedsTaskDTO.ShiftTypeId.HasValue && overlappingShift.ShiftTypeId != staffingNeedsTaskDTO.ShiftTypeId)
                            return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8767, "Arbetsuppgift {0} {1}-{2} kan inte tillsättas eftersom det finns ett schemapass med en annan passtyp under den tiden."), staffingNeedsTaskDTO.Name, staffingNeedsTaskDTO.StartTime.Value.ToShortTimeString(), staffingNeedsTaskDTO.StopTime.Value.ToShortTimeString()));

                        //check if the task overlaps another task                                                    
                        if (overlappingShift.TimeScheduleTemplateBlockTask.Any(x => CalendarUtility.IsDatesOverlapping(taskStartTime, taskStopTime, x.StartTime, x.StopTime)))
                            return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8768, "Arbetsuppgift {0} {1}-{2} kan inte tillsättas eftersom den krockar med en annan arbetsuppgift."), staffingNeedsTaskDTO.Name, staffingNeedsTaskDTO.StartTime.Value.ToShortTimeString(), staffingNeedsTaskDTO.StopTime.Value.ToShortTimeString()));


                        TimeScheduleTemplateBlockTaskDTO taskDTO = new TimeScheduleTemplateBlockTaskDTO
                        {
                            StartTime = staffingNeedsTaskDTO.StartTime.Value,
                            StopTime = staffingNeedsTaskDTO.StopTime.Value,
                            IncomingDeliveryRowId = staffingNeedsTaskDTO.Type == SoeStaffingNeedsTaskType.Delivery ? staffingNeedsTaskDTO.Id : (int?)null,
                            TimeScheduleTaskId = staffingNeedsTaskDTO.Type == SoeStaffingNeedsTaskType.Task ? staffingNeedsTaskDTO.Id : (int?)null,
                            State = SoeEntityState.Active
                        };

                        //connect the task to the shift
                        result = MoveTimeScheduleTemplateBlockTasks(new List<TimeScheduleTemplateBlockTaskDTO> { taskDTO }, overlappingShift);
                        if (!result.Success)
                            return result;
                    }
                }
            }

            result = UpdateLinkOnShifts(null, GetNewShiftLink(), employeeId, date, 0);
            if (!result.Success)
                return result;

            result = Save();
            if (!result.Success)
                return result;

            return result;
        }

        private List<TimeSchedulePlanningDayDTO> AssignTemplateShiftTask(List<StaffingNeedsTaskDTO> tasks, DateTime date, int timeScheduleTemplateHeadId)
        {
            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();

            TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHeadWithPeriods(timeScheduleTemplateHeadId);
            if (templateHead != null)
            {
                Employee employee = templateHead.EmployeeId.HasValue ? GetEmployeeWithContactPersonAndEmploymentFromCache(templateHead.EmployeeId.Value) : null;
                EmployeePost employeePost = templateHead.EmployeePostId.HasValue ? GetEmployeePost(templateHead.EmployeePostId.Value) : null;
                if (employee != null || employeePost != null)
                {
                    int rowNr = CalendarUtility.GetScheduleDayNumber(date, templateHead.StartDate.Value, 1, templateHead.NoOfDays);
                    TimeScheduleTemplatePeriod templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.DayNumber == rowNr && i.State == (int)SoeEntityState.Active);
                    if (templatePeriod != null)
                    {
                        #region Clear dates on new shifts

                        tasks = tasks.Where(i => i.StartTime.HasValue && i.StopTime.HasValue).ToList();
                        foreach (StaffingNeedsTaskDTO task in tasks)
                        {
                            task.StartTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, task.StartTime.Value);
                            task.StopTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, task.StopTime.Value);
                        }

                        #endregion

                        #region Existing shifts

                        if (!templatePeriod.TimeScheduleTemplateBlock.IsLoaded)
                            templatePeriod.TimeScheduleTemplateBlock.Load();

                        List<TimeScheduleTemplateBlock> existingTemplateBlocks = templatePeriod.TimeScheduleTemplateBlock.Where(i => i.Date == null && i.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && i.StartTime < i.StopTime && i.State == (int)SoeEntityState.Active).ToList();
                        foreach (TimeScheduleTemplateBlock existingTemplateBlock in existingTemplateBlocks)
                        {
                            TimeSchedulePlanningDayDTO shift = new TimeSchedulePlanningDayDTO()
                            {
                                EmployeeId = employee != null ? employee.EmployeeId : 0,
                                EmployeePostId = employeePost != null ? employeePost.EmployeePostId : 0,
                                TimeScheduleTemplateBlockId = existingTemplateBlock.TimeScheduleTemplateBlockId,
                                StartTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, existingTemplateBlock.StartTime),
                                StopTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, existingTemplateBlock.StopTime),
                                ShiftTypeId = existingTemplateBlock.ShiftTypeId ?? 0,
                            };

                            if (!existingTemplateBlock.TimeScheduleTemplateBlockTask.IsLoaded)
                                existingTemplateBlock.TimeScheduleTemplateBlockTask.Load();
                            if (!existingTemplateBlock.TimeScheduleTemplateBlockTask.IsNullOrEmpty())
                                shift.Tasks = existingTemplateBlock.TimeScheduleTemplateBlockTask.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();

                            shifts.Add(shift);
                        }

                        #endregion

                        #region Split after new shifts

                        foreach (StaffingNeedsTaskDTO task in tasks.OrderBy(i => i.StartTime))
                        {
                            foreach (TimeSchedulePlanningDayDTO existingShift in shifts.Where(i => i.TimeScheduleTemplateBlockId > 0).OrderBy(i => i.StartTime))
                            {
                                if (CalendarUtility.IsNewOverlappedByCurrent(task.StartTime.Value, task.StopTime.Value, existingShift.StartTime, existingShift.StopTime))
                                {
                                    //Add new shift
                                    TimeSchedulePlanningDayDTO newShift = existingShift.CloneDTO();
                                    newShift.TimeScheduleTemplateBlockId = 0;
                                    newShift.StartTime = task.StopTime.Value;
                                    newShift.Tasks = new List<TimeScheduleTemplateBlockTaskDTO>();
                                    if (!task.IsFixed)
                                        newShift.StopTime = newShift.StopTime.AddMinutes(task.Length);

                                    //Shorten original shift
                                    existingShift.StopTime = task.StartTime.Value;

                                    //Split tasks
                                    foreach (TimeScheduleTemplateBlockTaskDTO existingTask in existingShift.Tasks)
                                    {
                                        //Only adjust task if it also overlaps new task
                                        if (CalendarUtility.IsNewOverlappedByCurrent(task.StartTime.Value, task.StopTime.Value, existingTask.StartTime, existingTask.StopTime))
                                        {
                                            //Add new task
                                            TimeScheduleTemplateBlockTaskDTO newTask = existingTask.CloneDTO();
                                            newTask.TimeScheduleTemplateBlockId = 0;
                                            newTask.StartTime = task.StopTime.Value;
                                            if (!task.IsFixed)
                                                newTask.StopTime = newTask.StopTime.AddMinutes(task.Length);
                                            newShift.Tasks.Add(newTask);

                                            //Shorten original task
                                            existingTask.StopTime = task.StartTime.Value;

                                            //Only adjust 1 task
                                            break;
                                        }
                                    }

                                    shifts.Add(newShift);

                                    //Only adjust 1 shift 
                                    break;
                                }
                            }
                        }

                        #endregion

                        #region New shifts

                        foreach (StaffingNeedsTaskDTO task in tasks.OrderBy(i => i.StartTime))
                        {
                            TimeSchedulePlanningDayDTO newShift = new TimeSchedulePlanningDayDTO()
                            {
                                EmployeeId = employee != null ? employee.EmployeeId : 0,
                                EmployeePostId = employeePost != null ? employeePost.EmployeePostId : 0,
                                TimeScheduleTemplateBlockId = 0,
                                StartTime = task.StartTime.Value,
                                StopTime = task.StopTime.Value,
                                ShiftTypeId = task.ShiftTypeId ?? 0,
                                Tasks = new List<TimeScheduleTemplateBlockTaskDTO>(),
                            };

                            if (UseAccountHierarchy())
                            {
                                // If account hierarchy, use current selected account on shift
                                string accountHierarchyId = SettingManager.GetStringSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, userId, actorCompanyId, 0);
                                if (!string.IsNullOrEmpty(accountHierarchyId))
                                {
                                    string[] accounts = accountHierarchyId.Split('-');
                                    if (accounts.Length > 0 && int.TryParse(accounts.Last(), out int accountId))
                                        newShift.AccountId = accountId;
                                }
                            }

                            newShift.Tasks.Add(new TimeScheduleTemplateBlockTaskDTO()
                            {
                                TimeScheduleTemplateBlockTaskId = 0,
                                TimeScheduleTemplateBlockId = 0,
                                TimeScheduleTaskId = task.Type == SoeStaffingNeedsTaskType.Task ? task.Id : (int?)null,
                                IncomingDeliveryRowId = task.Type == SoeStaffingNeedsTaskType.Delivery ? task.Id : (int?)null,
                                StartTime = task.StartTime.Value,
                                StopTime = task.StopTime.Value,
                                Name = task.Name,
                                Description = task.Description,
                            });
                            shifts.Add(newShift);
                        }

                        #endregion

                        if (shifts.Count > 0)
                        {
                            #region Calculate breaks

                            List<TimeBreakTemplateBreakSlot> breakSlots = new List<TimeBreakTemplateBreakSlot>();

                            List<TimeBreakTemplateDTO> breakTemplates = GetTimeBreakTemplatesWithShiftTypesDayTypesAndRows(date).ToDTOs().ToList();
                            if (breakTemplates != null && breakTemplates.Count > 0)
                            {
                                DateTime startTime = shifts.Min(i => i.StartTime);
                                DateTime stopTime = startTime.AddMinutes(shifts.Sum(i => i.StopTime.Subtract(i.StartTime).TotalMinutes));
                                List<int> shiftTypeIds = shifts.Select(i => i.ShiftTypeId).Distinct().ToList();
                                int? dayTypeId = null;
                                DayOfWeek dayOfWeek = CalendarUtility.GetDayOfWeek(date);

                                List<TimeBreakTemplateTimeSlot> lockedTimeSlots = null;
                                if (tasks.Count == 1 && tasks[0].StopTime.Value.Subtract(tasks[0].StartTime.Value).TotalMinutes <= 60)
                                {
                                    //If task is one hour or less, assume it is created from break and use locked timeslots to prevent new break from being added on that time
                                    lockedTimeSlots = new List<TimeBreakTemplateTimeSlot> { new TimeBreakTemplateTimeSlot(tasks[0].StartTime.Value, tasks[0].StopTime.Value) };
                                }

                                TimeBreakTemplateEvaluationInput breakEvaluationInput = new TimeBreakTemplateEvaluationInput(SoeTimeBreakTemplateEvaluation.Manual, startTime, stopTime, date, shiftTypeIds, dayTypeId, dayOfWeek, lockedTimeSlots: lockedTimeSlots);
                                TimeBreakTemplateEvaluation breakEvaluation = new TimeBreakTemplateEvaluation();
                                TimeBreakTemplateEvaluationOutput breakEvaluationOutput = breakEvaluation.Evaluate(breakEvaluationInput, breakTemplates);
                                if (breakEvaluationOutput.Success)
                                    breakSlots.AddRange(breakEvaluationOutput.BreakSlots);
                            }

                            #endregion

                            #region Set breaks and common properties (and real dates!)

                            foreach (TimeSchedulePlanningDayDTO shift in shifts.OrderBy(i => i.StartTime))
                            {
                                shift.StartTime = CalendarUtility.GetDateTime(date, shift.StartTime);
                                shift.StopTime = CalendarUtility.GetDateTime(date, shift.StopTime);
                                shift.EmployeeId = employee?.EmployeeId ?? 0;
                                shift.EmployeePostId = employeePost?.EmployeePostId ?? 0;
                                if (shift.Tasks != null)
                                    shift.Tasks = shift.Tasks.OrderBy(i => i.StartTime).ToList();

                                int breakNr = 1;
                                foreach (TimeBreakTemplateBreakSlot breakSlot in breakSlots)
                                {
                                    if (breakNr > 4)
                                        break;

                                    TimeCodeBreak timeCodeBreak = GetTimeCodeBreakForEmployeeGroup(employee, employeePost, breakSlot.StartTime.Date, breakSlot.TimeCodeBreakGroupId);
                                    int timeCodeId = timeCodeBreak?.TimeCodeId ?? 0;

                                    switch (breakNr)
                                    {
                                        case 1:
                                            shift.Break1TimeCodeId = timeCodeId;
                                            shift.Break1StartTime = CalendarUtility.GetDateTime(date, breakSlot.StartTime);
                                            shift.Break1Minutes = breakSlot.Length;
                                            break;
                                        case 2:
                                            shift.Break2TimeCodeId = timeCodeId;
                                            shift.Break2StartTime = CalendarUtility.GetDateTime(date, breakSlot.StartTime);
                                            shift.Break2Minutes = breakSlot.Length;
                                            break;
                                        case 3:
                                            shift.Break3TimeCodeId = timeCodeId;
                                            shift.Break3StartTime = CalendarUtility.GetDateTime(date, breakSlot.StartTime);
                                            shift.Break3Minutes = breakSlot.Length;
                                            break;
                                        case 4:
                                            shift.Break4TimeCodeId = timeCodeId;
                                            shift.Break4StartTime = CalendarUtility.GetDateTime(date, breakSlot.StartTime);
                                            shift.Break4Minutes = breakSlot.Length;
                                            break;
                                    }
                                    breakNr++;
                                }
                            }

                            #endregion
                        }
                    }
                }
            }

            return shifts;
        }

        #endregion

        #region Scenario

        private ActionResult ActivateScenario(int timeScheduleScenarioHeadId, List<ActivateScenarioRowDTO> changedEmployeesAndDates, bool sendMessage, DateTime? preliminaryDateFrom)
        {
            ActionResult result = new ActionResult();
            int nrOfFails = 0;
            int nrOfSuccess = 0;
            try
            {
                #region Prereq

                Guid batchId = GetNewBatchLink();
                var timeScheduleScenarioHead = TimeScheduleManager.GetTimeScheduleScenarioHead(entities, timeScheduleScenarioHeadId, actorCompanyId, loadEmployees: true, loadAccounts: true);
                if (timeScheduleScenarioHead == null)
                    return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8843, "Scenario kunde inte hittas"));

                #endregion

                List<int> employeeIds = changedEmployeesAndDates.Select(x => x.EmployeeId).ToList();
                List<int> accountIds = timeScheduleScenarioHead.TimeScheduleScenarioAccount.Select(x => x.AccountId).Distinct().ToList();
                List<Employee> employees = GetEmployeesWithEmployment(employeeIds);//Fetch and cache employees
                List<TimeScheduleEmployeePeriod> allEmployeePeriods = GetTimeScheduleEmployeePeriods(employeeIds, timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo);
                List<TimeScheduleEmployeePeriod> changedEmployeePeriods = new List<TimeScheduleEmployeePeriod>();
                foreach (var employeePeriods in allEmployeePeriods.GroupBy(x => x.EmployeeId))
                {
                    List<DateTime> changedDatesForEmployee = changedEmployeesAndDates.Where(x => x.EmployeeId == employeePeriods.Key).Select(x => x.Date).ToList();
                    changedEmployeePeriods.AddRange(employeePeriods.Where(x => changedDatesForEmployee.Contains(x.Date)));
                }

                List<TimeScheduleTemplateBlock> allScheduleBlocks = GetScheduleBlocksForScheduleEmployeePeriods(changedEmployeePeriods.Select(x => x.TimeScheduleEmployeePeriodId).ToList(), includeOnDuty: true);
                allScheduleBlocks = allScheduleBlocks.Where(x => !x.TimeScheduleScenarioHeadId.HasValue || (x.TimeScheduleScenarioHeadId.HasValue && x.TimeScheduleScenarioHeadId.Value == timeScheduleScenarioHead.TimeScheduleScenarioHeadId)).ToList();
                //Filter on specified account, breaks doesnt have accountid - maybe they should?
                if (accountIds.Any())
                    allScheduleBlocks = allScheduleBlocks.Where(x => x.IsBreak || (!x.IsBreak && (!x.AccountId.HasValue || (x.AccountId.HasValue && accountIds.Contains(x.AccountId.Value))))).ToList();

                Dictionary<int, List<TimeScheduleTemplateBlock>> scheduleBlocksGrouped = allScheduleBlocks.GroupBy(g => g.EmployeeId.Value).ToDictionary(x => x.Key, x => x.ToList());

                foreach (var employee in employees)
                {
                    TimeScheduleScenarioEmployee scenarioEmployee = timeScheduleScenarioHead.TimeScheduleScenarioEmployee.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);
                    if (scenarioEmployee == null)
                        continue;

                    List<TimeScheduleTemplateBlock> currentEmployeeScheduleBlocks = scheduleBlocksGrouped.ContainsKey(employee.EmployeeId) ? scheduleBlocksGrouped[employee.EmployeeId] : new List<TimeScheduleTemplateBlock>();
                    List<TimeScheduleTemplateBlock> currentEmployeeAlreadyActivatedShifts = currentEmployeeScheduleBlocks.Where(x => !x.TimeScheduleScenarioHeadId.HasValue).ToList();
                    List<TimeScheduleTemplateBlock> currentEmployeeCurrentScenarioShifts = currentEmployeeScheduleBlocks.Where(x => x.TimeScheduleScenarioHeadId.HasValue && x.TimeScheduleScenarioHeadId.Value == timeScheduleScenarioHead.TimeScheduleScenarioHeadId && (x.StartTime != x.StopTime || x.TimeDeviationCauseId.HasValue)).ToList();
                    GetTimeBlockDatesFromCache(employee.EmployeeId, timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo);

                    ActionResult resultEmployee = ActivateScenarioForEmployee(employee, scenarioEmployee, currentEmployeeAlreadyActivatedShifts, currentEmployeeCurrentScenarioShifts, preliminaryDateFrom, batchId, sendMessage);
                    if (resultEmployee.Success)
                    {
                        nrOfSuccess++;
                        #region UpdateScheduledTimeSummary

                        try
                        {
                            if (GetCompanyBoolSettingFromCache(CompanySettingType.TimeCalculatePlanningPeriodScheduledTime) && base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, timeScheduleScenarioHead.DateTo))
                            {
                                TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employee.EmployeeId, timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo);
                                Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                        }

                        #endregion

                        if (base.HasCalculatePayrollOnChanges(entities, ActorCompanyId))
                            CalculatePayroll(employee.EmployeeId, timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo);

                    }
                    else
                    {
                        nrOfFails++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                result.Success = false;
                result.Exception = ex;
            }

            if (nrOfFails > 0)
            {
                String errorMessage = string.Format(GetText(9982, "{0} anställd/anställda kunde inte aktiveras"), nrOfFails) + "\n";
                errorMessage += string.Format(GetText(9983, "{0} anställd/anställda har aktiverats"), nrOfSuccess);
                return new ActionResult(errorMessage);
            }

            return result;
        }


        private ActionResult ActivateScenarioForEmployee(Employee employee, TimeScheduleScenarioEmployee scenarioEmployee, List<TimeScheduleTemplateBlock> currentEmployeeAlreadyActivatedShifts, List<TimeScheduleTemplateBlock> currentEmployeeCurrentScenarioShifts, DateTime? preliminaryDateFrom, Guid batchId, bool sendMessage)
        {
            ActionResult result = new ActionResult(true);
            try
            {
                scenarioEmployee.Status = (int)TermGroup_TimeScheduleScenarioEmployeeStatus.Initiated;
                result = Save();
                if (!result.Success)
                    return result;

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    InitTransaction(transaction);

                    ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, 0, TermGroup_ShiftHistoryType.ActivateScenario, null, !sendMessage);

                    #region Delete existing shifts


                    result = DeleteTimeScheduleShifts(currentEmployeeAlreadyActivatedShifts, null, false, logProperties);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Save new shifts

                    List<TimeSchedulePlanningDayDTO> shiftDtos = currentEmployeeCurrentScenarioShifts.ToTimeSchedulePlanningDayDTOs(IsHiddenEmployeeFromCache(employee.EmployeeId));
                    foreach (var shift in shiftDtos)
                    {
                        shift.SetAsNewActivatedShift();
                        shift.ClearTasks();

                        if (!shift.IsPreliminary && preliminaryDateFrom.HasValue)
                            shift.IsPreliminary = preliminaryDateFrom.HasValue && shift.StartTime.Date >= preliminaryDateFrom.Value;
                    }

                    result = SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.ActivateScenario, shiftDtos, true, !sendMessage, false, 0, null, batchId: batchId, updateScheduledTimeSummary: false);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Restore to schedule

                    result = RestoreCurrentDaysToSchedule();
                    if (!result.Success)
                        return result;

                    result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.ApplyAndRestore, employee.EmployeeId);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region ExtraShift

                    result = SetHasUnhandledShiftChanges();
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Apply Absence

                    if (shiftDtos.Any(x => x.TimeDeviationCauseId.HasValue))
                    {
                        var shiftsWithAbsence = shiftDtos.Where(x => x.TimeDeviationCauseId.HasValue).ToList();
                        InitAbsenceDays(employee.EmployeeId, shiftsWithAbsence.GetDates());
                        InitEvaluatePriceFormulaInputDTO(employeeIds: shiftsWithAbsence.GetEmployeeIds(employee.EmployeeId));
                        InitEmployeeSettingsCache(employee.EmployeeId, shiftsWithAbsence.GetStartDate(), shiftsWithAbsence.GetStopDate());

                        foreach (var shiftsGroupedByDate in shiftsWithAbsence.GroupBy(x => x.ActualDate.Date))
                        {
                            List<TimeSchedulePlanningDayDTO> absenceShifts = shiftsGroupedByDate.Where(x => x.StartTime != x.StopTime).ToList();
                            List<TimeSchedulePlanningDayDTO> zeroShifts = shiftsGroupedByDate.Where(x => x.StartTime == x.StopTime).ToList();

                            if (absenceShifts.Any())
                            {
                                result = SaveDeviationsFromShifts(absenceShifts, employee.EmployeeId, 0, false, null, null, recalculateRelatedDays: false, comment: "", useDeviationcauseFromInputShift: true, checkExistingWholeDayAbsence: false);
                                if (!result.Success)
                                    return result;
                            }
                            else if (zeroShifts.Any())
                            {
                                var zeroShift = zeroShifts.First();
                                result = SaveWholedayDeviations(zeroShift.ToTimeBlockDTO().ObjToList(), zeroShift.TimeDeviationCauseId.Value, zeroShift.TimeDeviationCauseId.Value, "", TermGroup_TimeDeviationCauseType.Absence, employee.EmployeeId, zeroShift.EmployeeChildId, true, recalculateRelatedDays: false);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        if (result.Success)
                        {
                            result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.ApplyAndRestore, employee.EmployeeId);
                            if (!result.Success)
                                return result;
                        }
                    }

                    #endregion

                    #region XeMail

                    SendXEMailOnActivateScenario(employee.EmployeeId);

                    #endregion

                    scenarioEmployee.Status = (int)TermGroup_TimeScheduleScenarioEmployeeStatus.Done;
                    scenarioEmployee.Message = null;
                    result = Save();
                    if (!result.Success)
                        return result;

                    if (result.Success)
                        this.currentTransaction.Complete();
                }
            }
            catch (Exception exp)
            {
                LogError(exp);
                result.Success = false;
                result.Exception = exp;
            }
            finally
            {
                if (!result.Success)
                {
                    LogTransactionFailed(this.ToString());

                    scenarioEmployee.Status = (int)TermGroup_TimeScheduleScenarioEmployeeStatus.Error;
                    scenarioEmployee.Message = GetText(8931, "Aktivering kunde inte slutföras.");
                    scenarioEmployee.Message += "\n" + result.ErrorMessage != null && !result.ErrorMessage.IsNullOrEmpty() ? result.ErrorMessage : GetText(8188, "Okänt fel");
                    result = Save();
                }
            }
            return result;
        }

        private ActionResult CreateTemplateFromScenario(int timeScheduleScenarioHeadId, DateTime targetStartDate, DateTime? targetStopDate, int weekInCycle)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            TimeScheduleScenarioHead timeScheduleScenarioHead = TimeScheduleManager.GetTimeScheduleScenarioHead(entities, timeScheduleScenarioHeadId, actorCompanyId, loadEmployees: true, loadAccounts: true);
            if (timeScheduleScenarioHead == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8843, "Scenario kunde inte hittas"));

            int scenarioNumberOfDays = Convert.ToInt32((timeScheduleScenarioHead.DateTo.Date - timeScheduleScenarioHead.DateFrom.Date).TotalDays) + 1;
            if (scenarioNumberOfDays < 1)
                return new ActionResult(false, (int)ActionResultSave.IncorrectInput, GetText(12513, "Scenariot är kortare än en dag"));

            DateTime firstMondayOfCycle = targetStartDate.AddDays(-7 * (weekInCycle - 1));
            int startDateMovedDays = Convert.ToInt32((targetStartDate - timeScheduleScenarioHead.DateFrom.Date).TotalDays);
            int scheduleMovedDays = 7 * (weekInCycle - 1);

            #endregion

            List<int> employeeIds = timeScheduleScenarioHead.TimeScheduleScenarioEmployee.Select(s => s.EmployeeId).Distinct().ToList();
            List<Employee> employees = GetEmployeesWithEmployment(employeeIds);//Fetch and cache employees

            List<TimeScheduleTemplateHead> templateHeads = entities.TimeScheduleTemplateHead.Where(w => w.ActorCompanyId == ActorCompanyId && w.EmployeeId.HasValue && employeeIds.Contains(w.EmployeeId.Value) && w.State == (int)SoeEntityState.Active && w.StartDate == targetStartDate).ToList();
            if (templateHeads.Any())
            {
                StringBuilder errorMessageBuilder = new StringBuilder();
                foreach (TimeScheduleTemplateHead templateHead in templateHeads)
                {
                    Employee employee = employees.FirstOrDefault(f => f.EmployeeId == templateHead.EmployeeId.Value);
                    if (employee != null)
                        errorMessageBuilder.Append($"{employee.NameOrNumber} - {GetText(3396, "Den anställde har redan ett grundschema med samma startdatum") + Environment.NewLine}");
                }

                result.Success = false;
                result.ErrorMessage = errorMessageBuilder.ToString();
                return result;
            }

            List<TimeSchedulePlanningDayDTO> scenarioShifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(entities, timeScheduleScenarioHead.ActorCompanyId, base.UserId, 0, base.RoleId, timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo, employeeIds, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.Admin, true, true, false,
                includeOnDuty: true,
                includePreliminary: true,
                includeAbsenceRequest: false,
                checkToIncludeDeliveryAdress: false,
                timeScheduleScenarioHeadId: timeScheduleScenarioHeadId);

            scenarioShifts = scenarioShifts.Where(w => w.StartTime != w.StopTime && w.StopTime != CalendarUtility.GetEndOfDay(w.StopTime)).ToList();

            foreach (Employee employee in employees)
            {
                List<TimeSchedulePlanningDayDTO> shiftsOnEmployee = scenarioShifts.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                if (!shiftsOnEmployee.IsNullOrEmpty())
                {
                    List<TimeScheduleTemplateBlockDTO> templateBlockItems = ConvertToTemplateBlockItems(shiftsOnEmployee);
                    ApplyAccountingFromShiftType(templateBlockItems);

                    foreach (TimeScheduleTemplateBlockDTO block in templateBlockItems)
                    {
                        DateTime initialStartDate = block.ActualDate;
                        block.ActualDate = block.ActualDate.AddDays(startDateMovedDays).AddDays(-scheduleMovedDays);

                        if (block.ActualDate < targetStartDate)
                        {
                            while (block.ActualDate < targetStartDate)
                            {
                                block.ActualDate = block.ActualDate.AddDays(scenarioNumberOfDays);
                            }
                        }
                        else if (block.ActualDate > targetStartDate.AddDays(scenarioNumberOfDays))
                        {
                            while (block.ActualDate > targetStartDate.AddDays(scenarioNumberOfDays))
                            {
                                block.ActualDate = block.ActualDate.AddDays(-scenarioNumberOfDays);
                            }
                        }

                        block.Date = block.ActualDate;
                        int blockMovedDays = Convert.ToInt32((block.ActualDate - initialStartDate).TotalDays);

                        if (block.Break1Id != 0)
                        {
                            block.Break1StartTime = block.Break1StartTime.AddDays(blockMovedDays);
                            block.Break1IsPreliminary = false;
                        }
                        if (block.Break2Id != 0)
                        {
                            block.Break2StartTime = block.Break2StartTime.AddDays(blockMovedDays);
                            block.Break2IsPreliminary = false;
                        }
                        if (block.Break3Id != 0)
                        {
                            block.Break3StartTime = block.Break3StartTime.AddDays(blockMovedDays);
                            block.Break3IsPreliminary = false;
                        }
                        if (block.Break4Id != 0)
                        {
                            block.Break4StartTime = block.Break4StartTime.AddDays(blockMovedDays);
                            block.Break4IsPreliminary = false;
                        }

                        block.IsPreliminary = false;
                        block.TimeScheduleTemplateBlockId = 0;
                        block.TimeScheduleTemplatePeriodId = null;
                        block.TimeScheduleEmployeePeriodId = null;
                        block.StaffingNeedsRowPeriodId = null;
                        block.Break1Id = 0;
                        block.Break2Id = 0;
                        block.Break3Id = 0;
                        block.Break4Id = 0;
                    }

                    int dayNumber = 1;
                    int diffDays = 7 * (weekInCycle - 1);

                    while (dayNumber <= (scenarioNumberOfDays + diffDays))
                    {
                        int dayNumberInternal = dayNumber;
                        if (dayNumberInternal > scenarioNumberOfDays)
                            dayNumberInternal -= scenarioNumberOfDays;

                        DateTime currentDate = firstMondayOfCycle.AddDays(dayNumber - 1);
                        List<TimeScheduleTemplateBlockDTO> shiftsOnDay = templateBlockItems.Where(w => w.Date == currentDate).ToList();
                        if (!shiftsOnDay.IsNullOrEmpty())
                            shiftsOnDay.ForEach(f => f.DayNumber = dayNumberInternal);
                        dayNumber++;
                    }

                    templateBlockItems.ForEach(f => f.Date = null);
                    result = SaveTimeScheduleTemplate(templateBlockItems, 0, employee.Name, "", (int)SoeEntityState.Active, targetStartDate, targetStopDate, null, firstMondayOfCycle, scenarioNumberOfDays, false, true, false, false, false, employee.EmployeeId, null, useAccountingFromSourceSchedule: true);

                    if (!result.Success)
                        return result;

                    #region Calculate ScheduledTimeSummary

                    //OBS!! Can we do this outside the transaction?

                    if (GetCompanyBoolSettingFromCache(CompanySettingType.TimeCalculatePlanningPeriodScheduledTime) && base.HasTimeValidRuleWorkTimeSettingsFromCache(entities, actorCompanyId, timeScheduleScenarioHead.DateTo))
                        TimeScheduleManager.UpdateScheduledTimeSummary(entities, actorCompanyId, employee.EmployeeId, timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo);

                    #endregion
                }
            }

            return result;
        }

        private ActionResult SaveTimeScheduleScenarioHeadAndCreateShifts(TimeScheduleScenarioHeadDTO scenarioHeadInput, int? timeScheduleScenarioHeadId, bool includeAbsence, int dateFunction, List<TimeSchedulePlanningDayDTO> templateShifts)
        {
            if (scenarioHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleScenarioHead");

            Employee hiddenEmployee = GetHiddenEmployeeFromCache();

            #region ScenarioHead

            TimeScheduleScenarioHead scenarioHead = TimeScheduleManager.GetTimeScheduleScenarioHead(entities, scenarioHeadInput.TimeScheduleScenarioHeadId, actorCompanyId, true, true);
            if (scenarioHead == null)
            {
                scenarioHead = new TimeScheduleScenarioHead()
                {
                    ActorCompanyId = actorCompanyId,
                    SourceType = (int)scenarioHeadInput.SourceType,
                    SourceDateFrom = scenarioHeadInput.SourceDateFrom,
                    SourceDateTo = scenarioHeadInput.SourceDateTo
                };
                SetCreatedProperties(scenarioHead);
                entities.TimeScheduleScenarioHead.AddObject(scenarioHead);
            }
            else
            {
                SetModifiedProperties(scenarioHead);
            }

            scenarioHead.Name = scenarioHeadInput.Name;
            scenarioHead.DateFrom = scenarioHeadInput.DateFrom;
            scenarioHead.DateTo = scenarioHeadInput.DateTo;

            #endregion

            #region Accounts

            List<int> inputAccountIds = new List<int>();
            if (scenarioHeadInput.Accounts != null)
            {
                inputAccountIds = scenarioHeadInput.Accounts.Select(e => e.AccountId).ToList();

                // Check if existing accounts still exists in input
                if (scenarioHead.TimeScheduleScenarioAccount.Any())
                {
                    foreach (TimeScheduleScenarioAccount acc in scenarioHead.TimeScheduleScenarioAccount.ToList())
                    {
                        if (inputAccountIds.Contains(acc.AccountId))
                            inputAccountIds.Remove(acc.AccountId);
                        else
                            entities.DeleteObject(acc);
                    }
                }

                // Add new from input
                foreach (int accountId in inputAccountIds)
                {
                    scenarioHead.TimeScheduleScenarioAccount.Add(new TimeScheduleScenarioAccount() { AccountId = accountId });
                }
            }

            #endregion

            #region Employees

            List<int> inputEmployeeIds = new List<int>();
            Dictionary<int, int> replacementEmployeeIds = new Dictionary<int, int>();

            if (scenarioHeadInput.Employees != null)
            {
                inputEmployeeIds = scenarioHeadInput.Employees.Select(e => e.EmployeeId).ToList();
                foreach (TimeScheduleScenarioEmployeeDTO employee in scenarioHeadInput.Employees.Where(e => e.ReplacementEmployeeId.HasValue && e.ReplacementEmployeeId.Value != 0))
                {
                    replacementEmployeeIds.Add(employee.EmployeeId, employee.ReplacementEmployeeId.Value);
                }

                // Check if existing employees still exists in input
                if (scenarioHead.TimeScheduleScenarioEmployee.Any())
                {
                    foreach (TimeScheduleScenarioEmployee emp in scenarioHead.TimeScheduleScenarioEmployee.ToList())
                    {
                        if (inputEmployeeIds.Contains(emp.EmployeeId))
                        {
                            // Still exists, so remove from input to prevent adding below
                            inputEmployeeIds.Remove(emp.EmployeeId);
                        }
                        else
                        {
                            // Remove shifts linked to employee
                            List<TimeScheduleTemplateBlock> shifts = (from b in entities.TimeScheduleTemplateBlock
                                                                      where b.TimeScheduleScenarioHeadId == scenarioHeadInput.TimeScheduleScenarioHeadId &&
                                                                      b.EmployeeId == emp.EmployeeId &&
                                                                      b.State != (int)SoeEntityState.Deleted
                                                                      select b).ToList();

                            foreach (TimeScheduleTemplateBlock shift in shifts)
                            {
                                ChangeEntityState(shift, SoeEntityState.Deleted);
                            }

                            // Remove scenario absence linked to employee

                            // Remove employee from scenario
                            entities.DeleteObject(emp);
                        }
                    }
                }

                // Add new from input
                foreach (int employeeId in inputEmployeeIds)
                {
                    scenarioHead.TimeScheduleScenarioEmployee.Add(new TimeScheduleScenarioEmployee() { EmployeeId = replacementEmployeeIds.ContainsKey(employeeId) ? replacementEmployeeIds[employeeId] : employeeId });
                }
            }

            #endregion

            ActionResult result = Save();
            if (!result.Success)
                return result;

            #region Shifts

            Guid batchId = GetNewBatchLink();

            if (scenarioHeadInput.TimeScheduleScenarioHeadId == 0)
            {
                int offsetDays = 0;
                if (!scenarioHeadInput.SourceDateFrom.HasValue)
                    scenarioHeadInput.SourceDateFrom = scenarioHeadInput.DateFrom;
                if (!scenarioHeadInput.SourceDateTo.HasValue)
                    scenarioHeadInput.SourceDateTo = scenarioHeadInput.DateTo;
                offsetDays = (int)((scenarioHeadInput.DateFrom - scenarioHeadInput.SourceDateFrom.Value).TotalDays);

                switch (scenarioHeadInput.SourceType)
                {
                    case TermGroup_TimeScheduleScenarioHeadSourceType.Schedule:
                    case TermGroup_TimeScheduleScenarioHeadSourceType.Scenario:
                        {
                            // DateFunction 0: Keep dates (same as source)
                            // DateFunction 1: Offset dates (start date changed, length same)
                            // DateFunction 2: Change dates (start and end date can be changed, but only within source interval)
                            bool shorten = scenarioHeadInput.SourceType == TermGroup_TimeScheduleScenarioHeadSourceType.Scenario && dateFunction == 2;
                            if (shorten)
                                offsetDays = 0;

                            DateTime blockDateFrom = shorten ? scenarioHeadInput.DateFrom : scenarioHeadInput.SourceDateFrom.Value;
                            DateTime blockDateTo = shorten ? scenarioHeadInput.DateTo : scenarioHeadInput.SourceDateTo.Value;
                            List<TimeScheduleTemplateBlock> allBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, inputEmployeeIds, blockDateFrom, blockDateTo, true, scenarioHeadInput.SourceType == TermGroup_TimeScheduleScenarioHeadSourceType.Schedule ? (int?)null : timeScheduleScenarioHeadId).Where(b => b.StartTime != b.StopTime || b.TimeDeviationCauseId.HasValue).ToList();
                            if (!includeAbsence)
                                allBlocks = allBlocks.Where(x => !x.TimeDeviationCauseId.HasValue).ToList();//breaks during absence will be filtered out later

                            // Removed due to task 63270
                            //if (inputAccountIds.Any())
                            //    allBlocks = allBlocks.Where(x => x.IsBreak || (!x.IsBreak && (!x.AccountId.HasValue || x.AccountId.HasValue && inputAccountIds.Contains(x.AccountId.Value)))).ToList();

                            // Added again due to task 109791
                            // With only check on accounts for hidden employee
                            if (inputAccountIds.Any() && inputEmployeeIds.Contains(hiddenEmployee.EmployeeId))
                                allBlocks = allBlocks.Where(x => x.EmployeeId != hiddenEmployee.EmployeeId || (x.AccountId.HasValue && inputAccountIds.Contains(x.AccountId.Value))).ToList();

                            Dictionary<int, List<TimeScheduleTemplateBlock>> scheduleBlocksGrouped = allBlocks.GroupBy(g => g.EmployeeId.Value).ToDictionary(x => x.Key, x => x.ToList());

                            if (allBlocks.Any())
                            {
                                foreach (var employeeId in inputEmployeeIds)
                                {
                                    List<TimeScheduleTemplateBlock> currentEmployeeScheduleBlocks = scheduleBlocksGrouped.ContainsKey(employeeId) ? scheduleBlocksGrouped[employeeId] : new List<TimeScheduleTemplateBlock>();

                                    List<TimeSchedulePlanningDayDTO> shiftsToCopy = new List<TimeSchedulePlanningDayDTO>();

                                    foreach (var scheduleBlocksByDate in currentEmployeeScheduleBlocks.GroupBy(x => x.Date.Value.Date))
                                    {
                                        List<TimeScheduleTemplateBlock> blocksToCopyCurrentEmployeeCurrentDate = new List<TimeScheduleTemplateBlock>();

                                        #region Decide which breaks to include (breaks dont have accountid set)

                                        foreach (var shift in scheduleBlocksByDate.Where(x => !x.IsBreak))
                                        {
                                            blocksToCopyCurrentEmployeeCurrentDate.Add(shift);
                                            var breaks = shift.GetOverlappedBreaks(scheduleBlocksByDate.Where(x => x.IsBreak), true).ToList();
                                            if (hiddenEmployee != null && hiddenEmployee.EmployeeId == employeeId)
                                                breaks = breaks.Where(x => x.Link == shift.Link).ToList();

                                            blocksToCopyCurrentEmployeeCurrentDate.AddRange(breaks);
                                        }

                                        #endregion

                                        if (!blocksToCopyCurrentEmployeeCurrentDate.Any())
                                            continue;

                                        #region Decide new employeeId

                                        int? newEmployeeId = null;
                                        if (replacementEmployeeIds.ContainsKey(scheduleBlocksByDate.First().EmployeeId.Value))
                                            newEmployeeId = replacementEmployeeIds[scheduleBlocksByDate.First().EmployeeId.Value];

                                        #endregion

                                        #region Decide new date

                                        DateTime? newDate = null;
                                        if (offsetDays != 0)
                                            newDate = scheduleBlocksByDate.Key.AddDays(offsetDays);

                                        #endregion

                                        #region Create link mappings

                                        List<Tuple<string, string>> linkMappings = blocksToCopyCurrentEmployeeCurrentDate.GetLinkMappings();

                                        #endregion

                                        #region Create dtos from orginal blocks and set new values

                                        List<TimeSchedulePlanningDayDTO> shiftDtos = blocksToCopyCurrentEmployeeCurrentDate.ToTimeSchedulePlanningDayDTOs(IsHiddenEmployeeFromCache(employeeId));
                                        foreach (var shift in shiftDtos)
                                        {
                                            shift.ClearIds();
                                            shift.ClearTasks();
                                            shift.SetNewLinks(linkMappings);
                                            shift.TimeScheduleScenarioHeadId = scenarioHead.TimeScheduleScenarioHeadId;
                                            shift.TimeScheduleTemplatePeriodId = null;

                                            if (newEmployeeId.HasValue && newEmployeeId.Value > 0)
                                                shift.EmployeeId = newEmployeeId.Value;
                                            if (newDate.HasValue)
                                                shift.ChangeDate(newDate.Value);
                                        }

                                        #endregion

                                        shiftsToCopy.AddRange(shiftDtos);
                                    }

                                    #region Save new shifts

                                    result = SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.CreateScenario, shiftsToCopy, true, true, false, 0, scenarioHead.TimeScheduleScenarioHeadId, batchId: batchId, updateScheduledTimeSummary: false);
                                    if (!result.Success)
                                        return result;

                                    #endregion
                                }
                            }

                            break;
                        }
                    case TermGroup_TimeScheduleScenarioHeadSourceType.Template:
                        {
                            // Removed due to task 63270
                            //if (inputAccountIds.Any())
                            //    templateShifts = templateShifts.Where(x => (!x.AccountId.HasValue || x.AccountId.HasValue && inputAccountIds.Contains(x.AccountId.Value))).ToList();

                            Dictionary<int, List<TimeSchedulePlanningDayDTO>> scheduleBlocksGrouped = templateShifts.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
                            if (templateShifts.Any())
                            {
                                foreach (var employeeId in inputEmployeeIds)
                                {
                                    List<TimeSchedulePlanningDayDTO> currentEmployeeShifts = scheduleBlocksGrouped.ContainsKey(employeeId) ? scheduleBlocksGrouped[employeeId] : new List<TimeSchedulePlanningDayDTO>();

                                    foreach (var shiftsByDate in currentEmployeeShifts.GroupBy(x => x.ActualDate))
                                    {
                                        #region Decide which breaks to include (breaks dont have accountid set)

                                        List<int> validBreakIds = new List<int>();
                                        foreach (var shift in shiftsByDate)
                                        {
                                            List<BreakDTO> overlappedBreaks = shift.GetOverlappedBreaks(shift.GetBreaks(), true);
                                            if (hiddenEmployee.EmployeeId == shift.EmployeeId && shift.Link.HasValue)
                                                overlappedBreaks = overlappedBreaks.Where(x => x.Link.HasValue && x.Link == shift.Link).ToList();

                                            validBreakIds.AddRange(overlappedBreaks.Select(x => x.Id));
                                        }
                                        validBreakIds = validBreakIds.Where(w => w != 0).Distinct().ToList();

                                        #endregion

                                        #region Decide new employee

                                        int? newEmployeeId = null;
                                        if (replacementEmployeeIds.ContainsKey(employeeId))
                                            newEmployeeId = replacementEmployeeIds[employeeId];

                                        #endregion

                                        #region Decide new date

                                        DateTime? newDate = null;
                                        if (offsetDays != 0)
                                            newDate = shiftsByDate.Key.AddDays(offsetDays);

                                        #endregion

                                        #region Create link mappings

                                        var linkMappings = shiftsByDate.ToList().GetLinkMappings();

                                        #endregion

                                        #region Set new values on shifts

                                        foreach (var shift in shiftsByDate)
                                        {
                                            shift.RemoveBreaksNotInCollection(validBreakIds);
                                            shift.ClearIds();
                                            shift.ClearTasks();
                                            shift.SetNewLinks(linkMappings);
                                            shift.TimeScheduleScenarioHeadId = scenarioHead.TimeScheduleScenarioHeadId;
                                            shift.TimeScheduleTemplatePeriodId = null;

                                            if (newEmployeeId.HasValue && newEmployeeId.Value > 0)
                                                shift.EmployeeId = newEmployeeId.Value;
                                            if (newDate.HasValue)
                                                shift.ChangeDate(newDate.Value);
                                        }

                                        #endregion
                                    }

                                    #region Save new shifts

                                    result = SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.CreateScenario, currentEmployeeShifts, true, true, false, 0, scenarioHead.TimeScheduleScenarioHeadId, batchId: batchId, updateScheduledTimeSummary: false);
                                    if (!result.Success)
                                        return result;

                                    #endregion
                                }
                            }

                            break;
                        }
                }
            }

            #endregion

            result = Save();
            if (!result.Success)
                return result;

            result.IntegerValue = scenarioHead.TimeScheduleScenarioHeadId;

            return result;
        }

        #endregion

        #region Placement

        private EmployeeSchedulePlacementValidationResult ValidateSchedulePlacement(List<SaveEmployeeSchedulePlacementItem> inputPlacements, ActivateScheduleControlDTO control = null, bool? hasTemplateGroups = null)
        {
            EmployeeSchedulePlacementValidationResult validationResult = new EmployeeSchedulePlacementValidationResult();
            List<SaveEmployeeSchedulePlacementItem> validatedPlacements = new List<SaveEmployeeSchedulePlacementItem>();

            Guid key = control?.Key != null && control.Key != Guid.Empty.ToString() ? Guid.Parse(control.Key) : Guid.NewGuid();

            #region Analyze and split templates

            Dictionary<int, List<TimeScheduleTemplateHead>> personalTemplateHeadsDict = new Dictionary<int, List<TimeScheduleTemplateHead>>();

            List<TimeScheduleTemplateHead> notPersonalTemplateHeads = null;
            if (inputPlacements.Any(p => !p.IsPersonalTemplate))
            {
                List<int> templateHeadIds = inputPlacements.Where(p => !p.IsPersonalTemplate && p.TimeScheduleTemplateHeadId != 0 && !p.ChangeStopDate).Select(s => s.TimeScheduleTemplateHeadId).ToList();
                notPersonalTemplateHeads = entities.TimeScheduleTemplateHead.Where(h => templateHeadIds.Contains(h.TimeScheduleTemplateHeadId)).ToList();
            }

            if (!hasTemplateGroups.HasValue)
                hasTemplateGroups = base.HasTimeScheduleTemplateGroupsFromCache(entities, actorCompanyId);

            foreach (var inputPlacementsByEmployee in inputPlacements.GroupBy(i => i.EmployeeId))
            {
                if (hasTemplateGroups.Value)
                {
                    #region Non-personal templates

                    foreach (var inputPlacement in inputPlacementsByEmployee)
                    {
                        if (!inputPlacement.IsPersonalTemplate || inputPlacement.ShortSchedule)
                        {
                            TimeScheduleTemplateHead notPersonalTemplateHead = notPersonalTemplateHeads?.FirstOrDefault(h => h.TimeScheduleTemplateHeadId == inputPlacement.TimeScheduleTemplateHeadId);
                            if (notPersonalTemplateHead != null && notPersonalTemplateHead.StartDate > inputPlacement.StartDate)
                                return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleStartDateCannotbeBeforeTemplateStartDate, GetText(3598, "Schemat kan inte aktiveras innan grundschemat startar") + " (" + notPersonalTemplateHead.StartDate.ToShortDateString() + ")"));

                            if (inputPlacement.ChangeStopDate)
                                inputPlacement.TimeScheduleTemplateHeadId = inputPlacement.ExistingTimeScheduleTemplateHeadId;

                            if ((!inputPlacement.IsPersonalTemplate && inputPlacement.TimeScheduleTemplateHeadId != 0) || inputPlacement.ShortSchedule)
                                validatedPlacements.Add(inputPlacement);
                        }
                    }

                    #endregion

                    #region Remaining

                    foreach (var remainingInputPlacements in inputPlacementsByEmployee.Where(placement => !validatedPlacements.Contains(placement)).GroupBy(g => g.IsPersonalTemplate))
                    {
                        DateTime? firstDate = remainingInputPlacements
                            .Where(w => w.StartDate.HasValue)
                            .OrderBy(o => o.StartDate.Value)
                            .FirstOrDefault()?.StartDate.Value ?? remainingInputPlacements
                                .Where(w => w.EmployeeScheduleStopDate.HasValue)
                                .OrderBy(o => o.EmployeeScheduleStartDate)
                                .FirstOrDefault()?.EmployeeScheduleStopDate.Value.AddDays(1);
                        DateTime lastDate = remainingInputPlacements.OrderBy(o => o.StartDate).Last().StopDate;

                        if (firstDate.HasValue)
                        {
                            TimeScheduleTemplateHeadsRangeDTO rangeHead = TimeScheduleManager.GetTimeScheduleTemplateHeadsRangeForEmployee(entities, inputPlacementsByEmployee.Key, actorCompanyId, firstDate.Value, lastDate, true);
                            if (rangeHead != null)
                            {
                                rangeHead.Heads = rangeHead.Heads.Where(w => (!w.StopDate.HasValue || w.StopDate.Value > w.StartDate) && (!w.StopDate.HasValue || w.StopDate > firstDate)).ToList();
                                foreach (TimeScheduleTemplateHeadRangeDTO head in rangeHead.Heads)
                                {
                                    if (head.StartDate < firstDate.Value)
                                        head.StartDate = firstDate.Value;
                                    if (!head.StopDate.HasValue || head.StopDate.Value > lastDate)
                                        head.StopDate = lastDate;
                                }

                                foreach (TimeScheduleTemplateHeadRangeDTO headRange in rangeHead.Heads)
                                {
                                    if (headRange.StartDate != headRange.EmployeeScheduleStartDate || headRange.StopDate != headRange.EmployeeScheduleStopDate)
                                    {
                                        var placement = SaveEmployeeSchedulePlacementItem.Create(headRange, inputPlacementsByEmployee.First().Preliminary, inputPlacementsByEmployee.First().CreateTimeBlocksAndTransactionsAsync);
                                        if (remainingInputPlacements.OrderBy(o => o.StartDate).First().NewPlacement)
                                            placement.MarkAsNewPlacement();
                                        placement.EmployeeInfo = inputPlacements.Where(p => p.EmployeeId == placement.EmployeeId).FirstOrDefault().EmployeeInfo; // Add EmployeeInfo as it's in placement
                                        validatedPlacements.Add(placement);
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    foreach (var inputPlacement in inputPlacementsByEmployee)
                    {
                        if (inputPlacement.IsPersonalTemplate && !inputPlacement.ShortSchedule)
                        {
                            #region Personal template

                            //Get personal templates for Employee
                            if (!personalTemplateHeadsDict.ContainsKey(inputPlacement.EmployeeId))
                                personalTemplateHeadsDict.Add(inputPlacement.EmployeeId, GetPersonalTemplateHeads(inputPlacement.EmployeeId));

                            //Filter personal templates on placement date range
                            DateTime filterStartDate = inputPlacement.StartDate ?? CalendarUtility.DATETIME_DEFAULT;
                            DateTime filterStopDate = inputPlacement.StopDate;
                            List<TimeScheduleTemplateHead> personalTemplateHeads = FilterPersonalTemplateHeads(personalTemplateHeadsDict[inputPlacement.EmployeeId], filterStartDate, filterStopDate);

                            //Check that Employee dont have personal templates with same start date
                            if (HasPersonalTemplateHeadsWithSameStartDate(personalTemplateHeads))
                                return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleTemplateHeadsWithSameStartDate, GetText(3599, "Det finns flera grundscheman med samma startdatum") + "\n" + inputPlacement.EmployeeInfo));

                            //Check that Employee have first template before placement starts
                            DateTime? personalTemplateStartDate = GetFirstPersonalTemplateStartDate(personalTemplateHeads);
                            if (personalTemplateStartDate.HasValue && personalTemplateStartDate.Value > inputPlacement.StartDate)
                                return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleStartDateCannotbeBeforeTemplateStartDate, GetText(3598, "Schemat kan inte aktiveras innan grundschemat startar") + "\n" + inputPlacement.EmployeeInfo));

                            //Check that Employee has personal templates
                            if (!personalTemplateHeads.Any())
                            {
                                validationResult.EmployeesWithoutOrInvalidPersonalSchedules.Add(inputPlacement.EmployeeInfo);
                                continue;
                            }

                            //Sort personal templates on latest StartDate first
                            personalTemplateHeads = personalTemplateHeads.OrderByDescending(i => i.StartDate).ToList();

                            DateTime currentStop = CalendarUtility.GetEndOfDay(inputPlacement.StopDate);
                            for (int i = 0; i < personalTemplateHeads.Count; i++)
                            {
                                var personalTemplateHead = personalTemplateHeads[i];
                                var personalItem = inputPlacement.Copy();

                                //Change to new placment if try to extend placement with another TimeScheduleTemplateHeadId
                                if (personalItem.ExistingTimeScheduleTemplateHeadId != personalTemplateHead.TimeScheduleTemplateHeadId)
                                    personalItem.MarkAsNewPlacement();

                                DateTime startDate, stopDate;

                                //StartDate
                                bool placementExistAfterStartOfTemplate = personalItem.EmployeeScheduleStopDate.HasValue && personalItem.EmployeeScheduleStopDate.Value >= personalTemplateHead.StartDate.Value.Date;
                                bool startDateAndCurrentStopDateIsBeforePlacement = placementExistAfterStartOfTemplate && personalItem.EmployeeScheduleStartDate.HasValue && personalItem.EmployeeScheduleStartDate.Value >= currentStop;
                                if (personalItem.StartDate.HasValue && personalItem.StartDate.Value.Date > personalTemplateHead.StartDate.Value.Date)
                                    startDate = personalItem.StartDate.Value; //New placement starts after personal template starts. Take date from placement.
                                else if (placementExistAfterStartOfTemplate && !startDateAndCurrentStopDateIsBeforePlacement)
                                    startDate = personalItem.EmployeeScheduleStopDate.Value.AddDays(1); //Personal template starts before existing placement stops. Take date after existing placement stops.
                                else
                                {
                                    bool overlapping = personalItem.StartDate.HasValue && entities.EmployeeSchedule.Any(es => es.EmployeeId == personalItem.EmployeeId && es.State == (int)SoeEntityState.Active && es.StartDate <= personalItem.StopDate && es.StopDate >= personalItem.StartDate.Value);
                                    if (personalItem.StartDate.HasValue && !overlapping)
                                    {
                                        // New activation in a hole between existing activations
                                        startDate = personalItem.StartDate.Value;
                                    }
                                    else
                                    {
                                        startDate = personalTemplateHead.StartDate.Value; //Take date from personal template
                                        if (startDateAndCurrentStopDateIsBeforePlacement && entities.EmployeeSchedule.Any(a => a.EmployeeId == personalItem.EmployeeId && a.State == (int)SoeEntityState.Active && a.StartDate < startDate))
                                            startDate = personalItem.EmployeeScheduleStopDate.Value.AddDays(1);
                                    }
                                }

                                //StopDate
                                startDate = startDate.Date;
                                stopDate = currentStop;

                                if (personalTemplateHead.StopDate.HasValue && personalTemplateHead.StopDate.Value < startDate)
                                    continue;
                                if (startDate > stopDate)
                                    continue;

                                //Check if schedule has days. Bug caused this, bug is fixed. This should never happen again, but...
                                if (personalTemplateHead.NoOfDays == 0)
                                    return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EntityIsNull, "Number of days in template schedule is 0. Fix and try again."));

                                if (personalTemplateHead.StopDate.HasValue && personalTemplateHead.StopDate.Value < stopDate.Date)
                                    return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleStopDateCannotBeAfterTemplateStopDate, GetText(3597, "Du kan inte aktivera schemat längre än grundschemat sträcker sig") + "\n" + inputPlacement.EmployeeInfo));

                                //Set values
                                personalItem.TimeScheduleTemplateHeadId = personalTemplateHead.TimeScheduleTemplateHeadId;
                                personalItem.StartDate = startDate;
                                personalItem.StopDate = stopDate;

                                //Set new stop
                                currentStop = personalItem.StartDate.Value.AddDays(-1);

                                validatedPlacements.Add(personalItem);
                            }

                            #endregion
                        }
                        else
                        {
                            #region None-personal template

                            if (inputPlacement.ChangeStopDate)
                                inputPlacement.TimeScheduleTemplateHeadId = inputPlacement.ExistingTimeScheduleTemplateHeadId;

                            validatedPlacements.Add(inputPlacement);

                            #endregion
                        }
                    }
                }
            }

            #endregion

            #region Validate

            if (!validatedPlacements.Any())
                return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleNothingPlaced, GetText(3600, "Schema ej aktiverat. Kontrollera att det finns ett grundschema för den anställde under vald period")));

            List<string> validationErrors = new List<string>();

            foreach (var placementsGroupedByEmployee in validatedPlacements.Where(w => w.UniqueId == Guid.Empty).GroupBy(i => i.EmployeeId).ToList())
            {
                //Sort
                var placementsForEmployee = placementsGroupedByEmployee.OrderBy(i => i.StartDate).ToList();

                //Validate input-dates (only for regular Employees, i.e. not hidden)
                validationResult.Result = IsPlacementDatesValid(placementsForEmployee);
                if (!validationResult.Result.Success)
                {
                    validationResult.Result.StringValue = placementsForEmployee.First().EmployeeInfo;
                    return validationResult;
                }

                foreach (var placement in placementsForEmployee)
                {
                    DateTime? employmentEndDate = GetEmploymentEndDate(placement.EmployeeId);
                    if (employmentEndDate.HasValue && employmentEndDate.Value.Date < placement.StopDate.Date)
                        return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleStopDateCannotBeAfterEmploymentStopDate, GetText(11778, "Aktivering får inte vara längre än anställningen") + "\n" + placement.EmployeeInfo));
                    if (placement.StartDate.HasValue && placement.StartDate.Value.Date > placement.StopDate.Date)
                        return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EmployeeScheduleStopDateEarlierThanStartDate, GetText(3592, "Slutdatum tidigare än startdatum") + "\n" + placement.EmployeeInfo));

                    if (placement.TimeScheduleTemplatePeriodId > 0)
                    {
                        TimeScheduleTemplatePeriod templatePeriodStart = GetTimeScheduleTemplatePeriodFromCache(placement.TimeScheduleTemplatePeriodId);
                        if (templatePeriodStart == null)
                            validationErrors.Add(GetText(3595, "Kan inte aktivera, schema saknas") + " (Period) " + placement.EmployeeInfo);
                    }

                    if (!placement.NewPlacement)
                    {
                        // Cannot extend schedule when no TimeScheduleTemplateHead exists
                        if (placement.TimeScheduleTemplateHeadId == 0)
                            validationErrors.Add(GetText(3595, "Kan inte aktivera, schema saknas") + " (TTH) " + placement.EmployeeInfo);

                        // Get EmployeeSchedule
                        EmployeeSchedule employeeSchedule = GetEmployeeScheduleFromCache(placement.EmployeeId, placement.EmployeeScheduleId);
                        if (employeeSchedule == null)
                            validationErrors.Add(GetText(3595, "Kan inte aktivera, schema saknas") + " (ES) " + placement.EmployeeInfo);

                        if (employeeSchedule != null && placement.ShortSchedule && !IsHiddenEmployeeFromCache(placement.EmployeeId))
                        {
                            DateTime startDate = placement.StopDate.AddDays(1);
                            DateTime stopDate = employeeSchedule.StopDate;

                            List<TimeBlock> timeBlocks = GetTimeBlocksWithTransactions(placement.EmployeeId, startDate, stopDate);
                            List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksForEmployee(null, placement.EmployeeId, startDate, stopDate, includeStandBy: true, loadStaffingIfUsed: true, includeOnDuty: true);

                            //Cannot shorten schedule with manually adjusted TimeBlocks and transactions
                            validationResult.Result = IsOkToDeleteEmployeeSchedule(placement.EmployeeInfo, timeBlocks, templateBlocks, control);
                            if (!validationResult.Result.Success)
                                return validationResult;

                            //Add to dict so we can reuse it later
                            validationResult.ShortenScheduleEmployeeTimeBlocksDict.Add(placement.EmployeeId, timeBlocks);
                            validationResult.ShortenScheduleEmployeeTemplateBlocksDict.Add(placement.EmployeeId, templateBlocks);
                        }
                    }
                }
            }

            if (validationErrors.Any())
                return new EmployeeSchedulePlacementValidationResult(new ActionResult(string.Join(Environment.NewLine + Environment.NewLine, validationErrors)));

            #endregion

            validationResult.RecalculateTimeHead = CreateRecalculateTimeHeadFromPlacement(validatedPlacements, key);
            if (validationResult.RecalculateTimeHead == null)
                return new EmployeeSchedulePlacementValidationResult(new ActionResult((int)ActionResultSave.EntityIsNull, "RecalculateTimeHead"));

            validationResult.ValidPlacements = validatedPlacements;

            return validationResult;
        }

        private ActionResult ValidateInitiatedEmployeePlacement(SaveEmployeeSchedulePlacementItem placement)
        {
            return ValidateInitiatedEmployeePlacements(new List<SaveEmployeeSchedulePlacementItem> { placement });
        }

        private ActionResult ValidateInitiatedEmployeePlacements(List<SaveEmployeeSchedulePlacementItem> placements)
        {
            ActionResult result = new ActionResult(true);

            placements = placements?.Where(i => !i.ShortSchedule).ToList();
            if (placements.IsNullOrEmpty())
                return result;

            // Read uncommited to be able to see records being modified
            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_READUNCOMMITED))
            {
                InitTransaction(transaction);

                List<int> employeeIds = placements.Select(i => i.EmployeeId).Distinct().ToList();
                List<RecalculateTimeRecord> recordsOngoing = GetRecalculateTimeRecords(employeeIds, TermGroup_RecalculateTimeRecordStatus.Waiting, TermGroup_RecalculateTimeRecordStatus.Unprocessed, TermGroup_RecalculateTimeRecordStatus.UnderProcessing);
                if (recordsOngoing.IsNullOrEmpty())
                    return result;

                Dictionary<int, int> employeeIdRecalculateTimeRecordIdDict = new Dictionary<int, int>();

                foreach (var placementsByEmployee in placements.GroupBy(i => i.EmployeeId))
                {
                    int employeeId = placementsByEmployee.Key;

                    List<RecalculateTimeRecord> recordsOngoingForEmployee = recordsOngoing.Where(i => i.EmployeeId == employeeId).ToList();
                    if (!recordsOngoingForEmployee.Any())
                        continue;

                    foreach (RecalculateTimeRecord record in recordsOngoingForEmployee)
                    {
                        List<SaveEmployeeSchedulePlacementItem> overlappingPlacements = placementsByEmployee.Where(placement => CalendarUtility.IsDatesOverlappingNullable(placement.StartDate ?? placement.EmployeeScheduleStopDate, placement.StopDate, record.StartDate, record.StopDate)).ToList();
                        if (overlappingPlacements.Any())
                        {
                            employeeIdRecalculateTimeRecordIdDict.Add(employeeId, record.RecalculateTimeRecordId);
                            break;
                        }
                    }
                }

                if (employeeIdRecalculateTimeRecordIdDict.Any())
                {
                    List<string> employeeInfos = placements.Where(i => employeeIdRecalculateTimeRecordIdDict.ContainsKey(i.EmployeeId)).Select(i => i.EmployeeInfo).Distinct().ToList();
                    result = new ActionResult((int)ActionResultSave.SaveTemplateSchedule_PlacementAlreadyInitiated, $"{GetText(11866, "Det finns redan en initierad aktivering för")} \n {employeeInfos.ToCommaSeparated()}");
                }
            }

            return result;
        }

        private ActionResult SaveEmployeeSchedulePlacement(ActivateScheduleControlDTO control, SaveEmployeeSchedulePlacementItem inputPlacement, ref List<TimeScheduleTemplateBlock> asyncTemplateBlocks)
        {
            if (inputPlacement == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SaveEmployeeSchedulePlacementItem");

            var validationResult = ValidateSchedulePlacement(inputPlacement.ObjToList(), control);
            if (!validationResult.Result.Success)
                return validationResult.Result;

            return SaveEmployeeSchedulePlacement(validationResult, ref asyncTemplateBlocks);
        }

        private ActionResult SaveEmployeeSchedulePlacement(EmployeeSchedulePlacementValidationResult validationResult, ref List<TimeScheduleTemplateBlock> asyncTemplateBlocks, bool useBulk = false, bool setHeadStatus = true)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            if (asyncTemplateBlocks == null)
                asyncTemplateBlocks = new List<TimeScheduleTemplateBlock>();

            entities.CommandTimeout = 3000;

            #endregion

            #region Perform

            bool queuedPlacement = false;

            var placements = validationResult.Batch ?? validationResult.ValidPlacements;
            if (placements.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SaveEmployeeSchedulePlacementItem: No placements to process");

            foreach (var placement in placements.OrderBy(i => i.EmployeeInfo))
            {
                RecalculateTimeRecord originalRecord = validationResult.RecalculateTimeHead?.RecalculateTimeRecord?.FirstOrDefault(i => i.PlacementId == placement.UniqueId || (i.EmployeeId == placement.EmployeeId && i.StartDate == placement.StartDate));
                RecalculateTimeRecord record = originalRecord != null ? GetRecalculateTimeRecord(originalRecord.RecalculateTimeRecordId) : null;

                try
                {
                    if (record == null)
                        return new ActionResult((int)ActionResultSave.EntityIsNull, $"SaveEmployeeSchedulePlacement: Re-fetch to be able to se if it has been cancelled failed.Employee:{placement.EmployeeInfo},EmployeeId:{placement.EmployeeId},PlacementStartDate:{placement.StartDate?.ToShortDateString()},PlacementStopDate:{placement.StopDate.ToShortDateString()},PlacementUniqueId:{placement.UniqueId}");

                    SetRecordStarted();

                    result = Save();
                    if (!result.Success || record.Status == (int)TermGroup_RecalculateTimeRecordStatus.Cancelled)
                        continue;

                    result = SaveEmployeeSchedulePlacementForEmployee(placement, validationResult.ShortenScheduleEmployeeTimeBlocksDict, validationResult.ShortenScheduleEmployeeTemplateBlocksDict, out List<TimeScheduleTemplateBlock> templateBlocksToPlaceDirectly, out List<TimeScheduleTemplateBlock> templateBlocksToPlaceLater, saveBulk: useBulk);
                    if (result.Success)
                    {
                        if (templateBlocksToPlaceLater.Any())
                            SetRecordQueued(templateBlocksToPlaceLater);
                        else
                            SetRecordCompleted();

                        if (placement.ShortSchedule)
                        {
                            if (validationResult.ShortenScheduleEmployeeTemplateBlocksDict.ContainsKey(placement.EmployeeId))
                                AdjustRecalculateTimeRecordAfterShortenedPlacament(validationResult.ShortenScheduleEmployeeTemplateBlocksDict[placement.EmployeeId], placementStopDate: placement.StopDate);
                        }
                        else if (templateBlocksToPlaceDirectly.Any())
                            asyncTemplateBlocks.AddRange(templateBlocksToPlaceDirectly);
                    }
                    else
                    {
                        SetRecordError();
                    }

                    if (result.Success)
                        result = useBulk ? BulkSave() : Save();
                    if (!result.Success)
                        return result;
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    SetRecordError();
                    LogError(ex);
                    return result;
                }

                void SetRecordStarted()
                {
                    if (record.Status == (int)TermGroup_RecalculateTimeRecordStatus.Waiting)
                    {
                        record.Status = (int)TermGroup_RecalculateTimeRecordStatus.UnderProcessing;
                    }
                    else
                    {
                        record.Status = (int)TermGroup_RecalculateTimeRecordStatus.Cancelled;
                        record.WarningMsg = $"{GetText(11867, "Hoppar över. Placering avbruten av användaren")}. {placement.EmployeeInfo}";
                    }
                }
                void SetRecordQueued(List<TimeScheduleTemplateBlock> templateBlocksToPlaceLater)
                {
                    record.Status = (int)TermGroup_RecalculateTimeRecordStatus.Unprocessed;
                    record.TimeScheduleTemplateBlock.AddRange(templateBlocksToPlaceLater); //Do not change Date. Will wipe history and potentially open up for duplicate placements. Job will use link to TimeScheduleTemplateBlock, not the dates.
                    record.WarningMsg = $"{GetText(11868, "Aktivering schemalagd")}. {templateBlocksToPlaceLater.GetStartDate()?.ToShortDateString()} - {templateBlocksToPlaceLater.GetStopDate().ToShortDateString()}";

                    if (!validationResult.RecalculateTimeHead.ExcecutedStartTime.HasValue)
                        validationResult.RecalculateTimeHead.ExcecutedStartTime = DateTime.Now.Date.AddHours(1);

                    queuedPlacement = true;
                }
                void SetRecordCompleted()
                {
                    record.Status = (int)TermGroup_RecalculateTimeRecordStatus.Processed;
                }
                void SetRecordError()
                {
                    record.Status = (int)TermGroup_RecalculateTimeRecordStatus.Error;
                    record.ErrorMsg = result.ErrorMessage;
                }
            }

            if (setHeadStatus)
                validationResult.RecalculateTimeHead.Status = queuedPlacement ? (int)TermGroup_RecalculateTimeHeadStatus.Unprocessed : (int)TermGroup_RecalculateTimeHeadStatus.Processed;

            result = Save();
            if (result.Success)
            {
                result.IntegerValue = validationResult.EmployeesWithoutOrInvalidPersonalSchedules.Count;
                result.BooleanValue = queuedPlacement;
            }

            #endregion

            return result;
        }

        private ActionResult SaveEmployeeSchedulePlacementForEmployee(SaveEmployeeSchedulePlacementItem item, Dictionary<int, List<TimeBlock>> shortenScheduleEmployeeTimeBlocksDict, Dictionary<int, List<TimeScheduleTemplateBlock>> shortenScheduleEmployeeTemplateBlocksDict, out List<TimeScheduleTemplateBlock> templateBlocksToPlaceDirectly, out List<TimeScheduleTemplateBlock> templateBlocksToPlaceLater, bool saveBulk = false)
        {
            ActionResult result;

            templateBlocksToPlaceDirectly = new List<TimeScheduleTemplateBlock>();
            templateBlocksToPlaceLater = new List<TimeScheduleTemplateBlock>();

            #region Prereq

            if (item == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "SaveEmployeeSchedulePlacementItem");

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(item.EmployeeId);
            if (employee == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));

            //Working variables, dont set input dates as it's original value is needed for each new placement
            DateTime? placementStartDate = item.StartDate;
            DateTime placementStopDate = item.StopDate;
            bool shortenSchedule = false; //Can only be true if changing StopDate                          

            #endregion

            #region EmployeeSchedule

            EmployeeSchedule employeeSchedule;

            if (item.ChangeStopDate)
            {
                #region Update placement

                // Cannot extend schedule when no TimeScheduleTemplateHead exists
                if (item.TimeScheduleTemplateHeadId == 0)
                    return new ActionResult((int)ActionResultSave.EmployeeScheduleCannotExtendUnplaced, GetText(3595, "Kan inte aktivera, schema saknas") + "\n" + item.EmployeeInfo);

                employeeSchedule = GetEmployeeScheduleFromCache(item.EmployeeId, item.EmployeeScheduleId);
                if (employeeSchedule == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeSchedule");

                employeeSchedule.TimeScheduleTemplateHead = GetTimeScheduleTemplateHeadWithPeriodsFromCache(employeeSchedule.TimeScheduleTemplateHeadId);
                if (employeeSchedule.TimeScheduleTemplateHead == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");
                if (employeeSchedule.StartDate > placementStopDate)
                    return new ActionResult((int)ActionResultSave.EmployeeScheduleStopDateEarlierThanStartDate, GetText(3592, "Slutdatum tidigare än startdatum") + "\n" + item.EmployeeInfo);

                if (employeeSchedule.IsPreliminary != item.Preliminary)
                    employeeSchedule.OverridedPreliminary = item.Preliminary;

                result = IsPlacementDatesValid(item.EmployeeId, item.EmployeeScheduleId, employeeSchedule.StartDate, placementStopDate);
                if (!result.Success)
                {
                    result.StringValue = item.EmployeeInfo;
                    return result;
                }

                //Change start and stop dates for period to delete
                shortenSchedule = placementStopDate < employeeSchedule.StopDate;
                if (shortenSchedule)
                {
                    //The day after the input StopDate
                    placementStartDate = item.StopDate.AddDays(1);

                    //The StopDate for the existing EmployeeSchedule
                    placementStopDate = employeeSchedule.StopDate;
                }
                else
                {
                    //The day after the existing EmployeeSchedule
                    placementStartDate = employeeSchedule.StopDate.AddDays(1);
                }

                // Existing EmployeeSchedule should always end at input StopDate
                employeeSchedule.StopDate = item.StopDate.Date;
                SetModifiedProperties(employeeSchedule);

                #endregion
            }
            else
            {
                #region New placement

                if (!placementStartDate.HasValue)
                    return new ActionResult((int)ActionResultSave.EmployeeScheduleStartDateMissing, GetText(3594, "Startdatum saknas") + "\n" + item.EmployeeInfo);

                result = IsPlacementDatesValid(item.EmployeeId, 0, placementStartDate.Value, placementStopDate);
                if (!result.Success)
                {
                    result.StringValue = item.EmployeeInfo;
                    return result;
                }

                TimeScheduleTemplateHead templateHead = GetTimeScheduleTemplateHeadWithPeriodsFromCache(item.TimeScheduleTemplateHeadId);
                if (templateHead == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");

                //StartDayNumber
                int startDayNumber = 1;
                TimeScheduleTemplatePeriod templatePeriodStart = GetTimeScheduleTemplatePeriodFromCache(item.TimeScheduleTemplatePeriodId);
                if (templatePeriodStart != null)
                {
                    //Set from TimeScheduleTemplatePeriod
                    startDayNumber = templatePeriodStart.DayNumber;
                }
                else
                {
                    //Set from TimeScheduleTemplateHead
                    if (item.StartDate.HasValue)
                    {
                        DateTime? startDate = templateHead.FirstMondayOfCycle ?? templateHead.StartDate;
                        if (!startDate.HasValue)
                            startDate = CalendarUtility.GetBeginningOfWeek(item.EmployeeScheduleStartDate);

                        startDayNumber = CalendarUtility.GetScheduleDayNumber(item.StartDate.Value, startDate.Value, 1, templateHead.NoOfDays);
                        if (startDayNumber < 1 && templateHead.NoOfDays > 0)
                        {
                            while (startDayNumber < 1)
                            {
                                startDayNumber += templateHead.NoOfDays;
                            }
                        }

                    }
                }

                // Create new EmployeeSchedule
                employeeSchedule = new EmployeeSchedule()
                {
                    StartDate = placementStartDate.Value.Date,
                    StopDate = placementStopDate.Date,
                    StartDayNumber = startDayNumber,
                    IsPreliminary = item.Preliminary,

                    //Set FK
                    EmployeeId = item.EmployeeId,

                    //Set references
                    TimeScheduleTemplateHead = templateHead,
                };
                SetCreatedProperties(employeeSchedule);
                entities.EmployeeSchedule.AddObject(employeeSchedule);

                #endregion
            }

            #endregion

            #region Delete schedule and deviations

            if (IsHiddenEmployeeFromCache(employeeSchedule.EmployeeId))
            {
                //Delete schedule. Deviations not created for hidden Employees
                result = SetScheduleToDeleted(employeeSchedule.EmployeeScheduleId, employeeSchedule.EmployeeId, placementStartDate.Value, placementStopDate, saveChanges: false);
                if (!result.Success)
                    return result;
            }
            else
            {
                if (shortenScheduleEmployeeTemplateBlocksDict.ContainsKey(item.EmployeeId) && shortenScheduleEmployeeTimeBlocksDict.ContainsKey(item.EmployeeId))
                {
                    //Re-use TimeBlocks and TimeScheduleTemplateBlocks used when validated above
                    List<TimeScheduleTemplateBlock> templateBlocks = shortenScheduleEmployeeTemplateBlocksDict[item.EmployeeId];
                    List<TimeBlock> timeBlocks = shortenScheduleEmployeeTimeBlocksDict[item.EmployeeId];
                    List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(item.EmployeeId, timeBlocks.Select(tb => tb.TimeBlockDateId).Distinct());

                    result = SetTimeBlockDateDetailsToDeleted(timeBlockDates, SoeTimeBlockDateDetailType.Absence, null, saveChanges: false);
                    if (!result.Success)
                        return result;

                    result = SetScheduleAndDeviationsToDeleted(templateBlocks, timeBlocks, saveChanges: false);
                    if (!result.Success)
                        return result;

                    result = SetTimePayrollScheduleTransactionsToDeleted(timeBlockDates.Select(x => x.TimeBlockDateId).ToList(), item.EmployeeId, saveChanges: false, excludeEmploymentTaxAndSupplementCharge: false);
                    if (!result.Success)
                        return result;
                }
                else
                {
                    result = SetScheduleAndDeviationsToDeleted(employeeSchedule.EmployeeId, placementStartDate.Value, placementStopDate, saveChanges: false);
                    if (!result.Success)
                        return result;
                }
            }

            #endregion

            #region Create schedule

            if (!shortenSchedule)
            {
                // Create TimeScheduleTemplateBlock's
                List<TimeScheduleTemplateBlockReference> references = CreateTimeScheduleTemplateBlocks(employee, employeeSchedule, placementStartDate.Value, placementStopDate, shiftTypeId: item.ShiftTypeId, accountIdsFromPlacement: item.AccountIds, deleteExisting: false, includeStandBy: true, includeOnDuty: true);
                if (!references.IsNullOrEmpty())
                {
                    List<TimeScheduleTemplateBlock> templateBlocks = references.GetTemplateBlocks(true);
                    if (!templateBlocks.IsNullOrEmpty())
                    {
                        DateTime placeLaterBoundary = templateBlocks.First().Date.Value.AddDays(Constants.PLACEMENT_DAYSTOPLACEDIRECTLY);
                        DateTime lastDate = templateBlocks.Last().Date.Value;

                        foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks)
                        {
                            //Special fix to include last day of month when one month is placed
                            if (templateBlock.Date.Value < placeLaterBoundary || (templateBlock.Date.Value == placeLaterBoundary && templateBlock.Date.Value == lastDate))
                                templateBlocksToPlaceDirectly.Add(templateBlock);
                            else
                                templateBlocksToPlaceLater.Add(templateBlock);
                        }
                    }
                }
            }

            #endregion

            result = saveBulk ? BulkSave() : Save();

            return result;
        }

        private void AdjustRecalculateTimeRecordAfterShortenedPlacament(List<TimeScheduleTemplateBlock> templateBlocks, DateTime? placementStopDate = null)
        {
            templateBlocks = templateBlocks?.Where(i => i.Date.HasValue).OrderBy(i => i.Date.Value).ToList();
            if (templateBlocks.IsNullOrEmpty())
                return;

            int? recalculateTimeRecordId = templateBlocks.FirstOrDefault(i => i.RecalculateTimeRecordId.HasValue)?.RecalculateTimeRecordId;
            if (!recalculateTimeRecordId.HasValue)
                return;

            RecalculateTimeRecord scheduledRecord = GetRecalculateTimeRecordWithBlocks(recalculateTimeRecordId.Value);
            if (scheduledRecord == null || scheduledRecord.Status != (int)TermGroup_RecalculateTimeRecordStatus.Unprocessed || !scheduledRecord.StartDate.HasValue || !scheduledRecord.StopDate.HasValue)
                return;

            DateTime? scheduledStartDate = scheduledRecord.GetScheduledStartDate();
            DateTime newStopDate = templateBlocks.Min(i => i.Date.Value).AddDays(-1);

            if (scheduledStartDate.HasValue && scheduledStartDate.Value <= newStopDate)
            {
                #region Shortened partly of the scheduled placement. Set new stopdate and update WarningsMsg with new scheduled dates

                scheduledRecord.StopDate = newStopDate;
                scheduledRecord.WarningMsg = $"{GetText(11868, "Aktivering schemalagd")}. {scheduledRecord.StartDate.ToShortDateString()} - {scheduledRecord.StopDate.ToShortDateString()}";
                scheduledRecord.WarningMsg += $" ({GetText(11870, "Aktivering backad")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm")})";

                #endregion
            }
            else
            {
                if (placementStopDate.HasValue && placementStopDate.Value > scheduledRecord.StartDate)
                {
                    #region Shortened partly of the placement. Set stopdate and update WarningsMsg

                    scheduledRecord.Status = (int)TermGroup_RecalculateTimeRecordStatus.Processed;
                    scheduledRecord.StopDate = placementStopDate.Value;
                    scheduledRecord.WarningMsg = $"{GetText(11870, "Aktivering backad")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}";

                    #endregion
                }
                else
                {
                    #region Shortened complete placement. Set as processed and delete dates

                    scheduledRecord.Status = (int)TermGroup_RecalculateTimeRecordStatus.Processed;
                    scheduledRecord.StartDate = null;
                    scheduledRecord.StopDate = null;
                    scheduledRecord.WarningMsg = $"{GetText(11870, "Aktivering backad")} {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}";

                    if (scheduledRecord.Status == (int)TermGroup_RecalculateTimeRecordStatus.Processed)
                    {
                        RecalculateTimeHead scheduledHead = GetRecalculateTimeHead(scheduledRecord.RecalculateTimeHeadId);
                        if (scheduledHead != null && scheduledHead.RecalculateTimeRecord.All(i => i.Status == (int)TermGroup_RecalculateTimeRecordStatus.Processed))
                            scheduledHead.Status = (int)TermGroup_RecalculateTimeHeadStatus.Processed;
                    }

                    #endregion
                }
            }
        }

        #endregion

        #region Annual leave

        public List<AnnualLeaveTransactionEarned> GetAnnualLeaveTransactionsEarned(List<int> employeeIds, DateTime startDate, DateTime stopDate, int startNbrOfDays = 0, int startAccMinutes = 0, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AnnualLeaveTransactionEarned> transactions = new List<AnnualLeaveTransactionEarned>();
            AnnualLeaveManager alm = new AnnualLeaveManager(parameterObject);

            if (annualLeaveGroups == null)
                annualLeaveGroups = alm.GetAnnualLeaveGroups(actorCompanyId);

            if (entities == null)
                entities = entitiesReadOnly;

            var firstEmploymentForEmployees = EmployeeManager.GetFirstEmploymentForEachEmployee(actorCompanyId, employeeIds);

            // Get manually earned annual leave transactions
            List<AnnualLeaveTransaction> manuallyEarnedAnnualLeaveTransactions = alm.GetAnnualLeaveTransactionsManuallyEarned(actorCompanyId, startDate, stopDate, employeeIds);

            // Get Attest States
            //List<AttestState> attestStates = AttestManager.GetAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);

            // Get lowest attest level for payroll
            //AttestState attestStateAttested = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollMinimumAttestStatus);

            // Get shared data for the period
            // TODO: For now get all transactions, even those that are not attested. May need to filter later based on attest state.
            var sharedDataDict = GetSharedDataForEmployees(employeeIds, startDate, stopDate); // , attestStateAttested

            // Get EmploymentCalendarDTOs for all employees
            DateTime employmentCalendarStartDate = startDate.AddDays(-30); // Back another 30 days to be able to check gap days on first employment in period
            List<EmploymentCalenderDTO> employmentCalenderDTOs = EmployeeManager.GetEmploymentCalenderDTOs(sharedDataDict.Values.Select(s => s.Employee).ToList(), employmentCalendarStartDate, stopDate, annualLeaveGroups: annualLeaveGroups);

            foreach (int employeeId in employeeIds)
            {
                if (!sharedDataDict.TryGetValue(employeeId, out var employeeData))
                    continue;

                // Set EmploymentCalendar for employee
                employeeData.EmploymentCalendar = employmentCalenderDTOs.Where(e => e.EmployeeId == employeeId).ToList();
                // Set first employment
                employeeData.FirstEmployment = firstEmploymentForEmployees.ContainsKey(employeeId) ? firstEmploymentForEmployees[employeeId] : null;
                // Set manually earned annual leave transactions for employee
                employeeData.IngoingAnnualLeaveTransactions = manuallyEarnedAnnualLeaveTransactions.Where(t => t.EmployeeId == employeeId).ToList();

                var annualLeaveCalculation = new AnnualLeaveCalculation();
                transactions.AddRange(annualLeaveCalculation.GetAnnualLeaveTransactionsEarned(employeeData, annualLeaveGroups.ToDTOs(), startNbrOfDays, startAccMinutes));
            }

            return transactions;
        }



        #endregion
    }
}
