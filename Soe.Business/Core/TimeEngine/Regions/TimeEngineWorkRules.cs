using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        private EvaluateScenarioAgainstWorkRulesOutputDTO TaskEvaluateActivateScenarioAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateActivateScenarioAgainstWorkRulesInputDTO, EvaluateScenarioAgainstWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    var timeScheduleScenarioHead = TimeScheduleManager.GetTimeScheduleScenarioHead(entities, iDTO.TimeScheduleScenarioHeadId, actorCompanyId, loadEmployees: true, loadAccounts: true);
                    if (timeScheduleScenarioHead == null)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8843, "Scenario kunde inte hittas"));
                        return oDTO;
                    }

                    #endregion

                    #region Init

                    List<int> employeeIds = timeScheduleScenarioHead.TimeScheduleScenarioEmployee.Select(x => x.EmployeeId).ToList();
                    List<int> accountIds = timeScheduleScenarioHead.TimeScheduleScenarioAccount.Select(x => x.AccountId).Distinct().ToList();
                    List<Employee> employees = GetEmployeesWithEmployment(employeeIds);//Fetch and cache employees
                    List<TimeScheduleEmployeePeriod> employeePeriods = GetTimeScheduleEmployeePeriods(employeeIds, CalendarUtility.GetDates(timeScheduleScenarioHead.DateFrom, timeScheduleScenarioHead.DateTo));
                    List<TimeScheduleTemplateBlock> allScheduleBlocks = GetScheduleBlocksForScheduleEmployeePeriods(employeePeriods.Select(x => x.TimeScheduleEmployeePeriodId).ToList());
                    allScheduleBlocks = allScheduleBlocks.Where(x => !x.TimeScheduleScenarioHeadId.HasValue || (x.TimeScheduleScenarioHeadId.HasValue && x.TimeScheduleScenarioHeadId.Value == timeScheduleScenarioHead.TimeScheduleScenarioHeadId)).ToList();
                    if (accountIds.Any())
                        allScheduleBlocks = allScheduleBlocks.Where(x => x.IsBreak || (!x.IsBreak && x.AccountId.HasValue && accountIds.Contains(x.AccountId.Value))).ToList();

                    Dictionary<int, List<TimeScheduleTemplateBlock>> scheduleBlocksGrouped = allScheduleBlocks.GroupBy(g => g.EmployeeId.Value).ToDictionary(x => x.Key, x => x.ToList());

                    #endregion

                    foreach (int employeeId in employees.Select(e => e.EmployeeId))
                    {
                        List<TimeScheduleTemplateBlock> currentEmployeeScheduleBlocks = scheduleBlocksGrouped.ContainsKey(employeeId) ? scheduleBlocksGrouped[employeeId] : new List<TimeScheduleTemplateBlock>();
                        List<TimeScheduleTemplateBlock> currentEmployeeAlreadyActivatedShifts = currentEmployeeScheduleBlocks.Where(x => !x.TimeScheduleScenarioHeadId.HasValue).ToList();
                        List<TimeScheduleTemplateBlock> currentEmployeeCurrentScenarioShifts = currentEmployeeScheduleBlocks.Where(x => x.TimeScheduleScenarioHeadId.HasValue && x.TimeScheduleScenarioHeadId.Value == timeScheduleScenarioHead.TimeScheduleScenarioHeadId && x.StartTime != x.StopTime).ToList();

                        List<TimeSchedulePlanningDayDTO> shiftsToEvaluateForEmployee = new List<TimeSchedulePlanningDayDTO>();

                        #region Simulate activated shifts as deleted (Activated shifts will be replaced by scenario shifts)

                        List<TimeSchedulePlanningDayDTO> activatedShiftDTOs = currentEmployeeAlreadyActivatedShifts.ToTimeSchedulePlanningDayDTOs();
                        foreach (var shift in activatedShiftDTOs)
                        {
                            shift.SetAsDeleted();
                            shift.ClearTasks();

                            shiftsToEvaluateForEmployee.Add(shift);
                        }

                        #endregion

                        #region Simulate scenario shifts as activated shifts

                        List<TimeSchedulePlanningDayDTO> scenarioShifts = currentEmployeeCurrentScenarioShifts.ToTimeSchedulePlanningDayDTOs();
                        foreach (var shift in scenarioShifts)
                        {
                            shift.SetAsNewActivatedShift();

                            if (iDTO.PreliminaryDateFrom.HasValue && iDTO.PreliminaryDateFrom.Value <= shift.StartTime.Date)
                                shift.IsPreliminary = true;

                            shiftsToEvaluateForEmployee.Add(shift);
                        }

                        #endregion

                        List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(shiftsToEvaluateForEmployee, employeeId, false, null, rules: iDTO.Rules, breakOnNotAllowedToOverride: false);
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        private EvaluateScenarioAgainstWorkRulesOutputDTO TaskEvaluateScenarioToTemplateAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateScenarioToTemplateAgainstWorkRulesInputDTO, EvaluateScenarioAgainstWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    var timeScheduleScenarioHead = TimeScheduleManager.GetTimeScheduleScenarioHead(entities, iDTO.TimeScheduleScenarioHeadId, actorCompanyId, loadEmployees: true, loadAccounts: true);
                    if (timeScheduleScenarioHead == null)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8843, "Scenario kunde inte hittas"));
                        return oDTO;
                    }

                    #endregion

                    #region Init

                    List<int> employeeIds = timeScheduleScenarioHead.TimeScheduleScenarioEmployee.Select(x => x.EmployeeId).ToList();
                    List<Employee> employees = GetEmployeesWithEmployment(employeeIds);//Fetch and cache employees

                    #endregion

                    foreach (int employeeId in employees.Select(e => e.EmployeeId))
                    {

                        List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(iDTO.Shifts, employeeId, false, iDTO.TimeScheduleScenarioHeadId, rules: iDTO.Rules, breakOnNotAllowedToOverride: false);
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        private SaveEvaluateAllWorkRulesByPassOutputDTO TaskSaveEvaluateAllWorkRulesByPass()
        {
            var (iDTO, oDTO) = InitTask<SaveEvaluateAllWorkRulesByPassInputDTO, SaveEvaluateAllWorkRulesByPassOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Result == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "EvaluateWorkRulesActionResult");
                return oDTO;
            }
            if (iDTO.Result.EvaluatedRuleResults.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "EvaluatedRuleResults");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Perform

                    if (!iDTO.Result.AllRulesSucceded)
                    {
                        DateTime date = DateTime.Now;

                        foreach (EvaluateWorkRuleResultDTO result in iDTO.Result.EvaluatedRuleResults.Where(i => !i.Success))
                        {
                            WorkRuleBypassLog workRuleBypassLog = new WorkRuleBypassLog()
                            {
                                WorkRule = (int)result.EvaluatedWorkRule,
                                Action = (int)result.Action,
                                Date = date,
                                Message = !result.EmployeeName.IsNullOrEmpty() ? result.ErrorMessage.Replace(result.EmployeeName, "[EmployeeName]") : result.ErrorMessage,

                                //Set FK
                                EmployeeId = result.EmployeeId.HasValue ? result.EmployeeId.Value : iDTO.EmployeeId,
                                ActorCompanyId = actorCompanyId,
                                UserId = userId,
                            };
                            SetCreatedProperties(workRuleBypassLog);
                            entities.WorkRuleBypassLog.AddObject(workRuleBypassLog);
                        }
                    }

                    oDTO.Result = Save();

                    #endregion
                }
                catch (Exception ex)
                {
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

        private EvaluateAllWorkRulesOutputDTO TaskEvaluateAllWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateAllWorkRulesInputDTO, EvaluateAllWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);
                    bool useAnnualLeave = GetCompanyBoolSettingFromCache(CompanySettingType.UseAnnualLeave);

                    #endregion

                    #region Perform

                    if (iDTO.Rules == null)
                    {
                        iDTO.Rules = new List<SoeScheduleWorkRules>();
                        foreach (SoeScheduleWorkRules rule in Enum.GetValues(typeof(SoeScheduleWorkRules)))
                        {
                            if (rule == SoeScheduleWorkRules.AttestedDay || rule == SoeScheduleWorkRules.HoursBeforeShiftRequest)
                                continue;
                            iDTO.Rules.Add(rule);
                        }
                    }

                    List<TimeScheduleTemplateBlock> scheduledBlocksForCompany = !iDTO.IsPersonalScheduleTemplate && EmployeeManager.UseEmployeeIdsInQuery(entities, iDTO.EmployeeIds) ? GetScheduleBlocksForWorkRuleEvaluation(iDTO.TimeScheduleScenarioHeadId, iDTO.EmployeeIds, iDTO.StartDate.AddDays(-1), iDTO.StopDate.AddDays(1), false) : null;
                    List<TimeScheduleEmployeePeriodDetail> planningPeriodLeisureCodes = new List<TimeScheduleEmployeePeriodDetail>();

                    if (useLeisureCodes && iDTO.PlanningPeriodStartDate.HasValue && iDTO.PlanningPeriodStopDate.HasValue)
                    {
                        planningPeriodLeisureCodes = TimeScheduleManager.GetTimeSchedulePlanningLeisureCodes(entities, iDTO.PlanningPeriodStartDate.Value.AddDays(-7), iDTO.PlanningPeriodStopDate.Value, iDTO.EmployeeIds);
                    }

                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        List<TimeScheduleTemplateBlock> scheduledBlocks = null;
                        List<TimeSchedulePlanningDayDTO> shiftsToEvalauteForEmployee = null;
                        if (iDTO.IsPersonalScheduleTemplate)
                        {
                            var plannedShiftsForEmployee = iDTO.PlannedShifts.Where(x => x.EmployeeId == employeeId).ToList();
                            if (plannedShiftsForEmployee.Any())
                            {
                                TimeScheduleTemplateHead template = GetTimeScheduleTemplateHead(plannedShiftsForEmployee.OrderBy(x => x.ActualDate).FirstOrDefault().TimeScheduleTemplateHeadId ?? 0);
                                if (template != null)
                                {
                                    DateTime templateDateFrom = template.GetCycleStartFromGivenDate(plannedShiftsForEmployee.OrderBy(x => x.ActualDate).FirstOrDefault().ActualDate);
                                    if (template.StartDate.HasValue && templateDateFrom < template.StartDate)
                                        templateDateFrom = templateDateFrom.AddDays(template.NoOfDays);

                                    shiftsToEvalauteForEmployee = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, template.TimeScheduleTemplateHeadId, templateDateFrom, templateDateFrom.AddDays(template.NoOfDays - 1), loadTasksAndDelivery: false, useNbrOfDaysFromTemplate: false, includeZeroDays: false, loadTimeDeviationCause: false);
                                }
                            }
                        }
                        else
                        {
                            if (scheduledBlocksForCompany != null)
                                scheduledBlocks = scheduledBlocksForCompany.Where(i => i.EmployeeId == employeeId).ToList();
                            else
                                scheduledBlocks = GetScheduleBlocksForWorkRuleEvaluation(iDTO.TimeScheduleScenarioHeadId, employeeId, iDTO.StartDate, iDTO.StopDate, false);

                            shiftsToEvalauteForEmployee = scheduledBlocks.Where(tb => (tb.Date.HasValue && tb.Date.Value >= iDTO.StartDate.Date && tb.Date.Value <= iDTO.StopDate.Date)).ToList().ToTimeSchedulePlanningDayDTOs();
                        }

                        if (shiftsToEvalauteForEmployee.IsNullOrEmpty())
                            continue;

                        List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(shiftsToEvalauteForEmployee, employeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, scheduledBlocks: scheduledBlocks, breakOnNotAllowedToOverride: false, all: true, evaluateScheduleWithTimeDeviationCauseSetting: true, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate, planningPeriodLeisureCodes: planningPeriodLeisureCodes.Where(x => x.TimeScheduleEmployeePeriod.EmployeeId == employeeId).ToList(), keepScheduledBlocks: true, useAnnualLeave: useAnnualLeave);
                        List<EvaluateWorkRuleResultDTO> failedResults = ruleResults.Where(r => !r.Success).ToList();
                        if (failedResults.Any())
                        {
                            EvaluateAllWorkRulesResultDTO failedResult = new EvaluateAllWorkRulesResultDTO
                            {
                                EmployeeId = employeeId,
                                Violations = new List<string>(),
                            };

                            foreach (EvaluateWorkRuleResultDTO result in failedResults)
                            {
                                failedResult.Violations.Add(result.ErrorMessage);
                            }
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.Add(failedResult);
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluatePlannedShiftsAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluatePlannedShiftsAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);
                    bool useAnnualLeave = GetCompanyBoolSettingFromCache(CompanySettingType.UseAnnualLeave);

                    #endregion

                    #region Perform

                    List<TimeSchedulePlanningDayDTO> allShifts = iDTO.PlannedShifts;

                    var shiftsGroupedByEmployee = allShifts.GroupBy(x => x.EmployeeId).ToList();
                    foreach (var shiftsGroupedForEmployee in shiftsGroupedByEmployee)
                    {
                        int employeeId = shiftsGroupedForEmployee.Key;
                        List<TimeSchedulePlanningDayDTO> shiftsForEmployee = shiftsGroupedForEmployee.ToList();

                        if (iDTO.IsPersonalScheduleTemplate && shiftsForEmployee != null && shiftsForEmployee.Any(x => x.ActualDate.DayOfWeek == DayOfWeek.Saturday || x.ActualDate.DayOfWeek == DayOfWeek.Sunday))
                        {
                            TimeScheduleTemplateHead template = GetTimeScheduleTemplateHead(shiftsForEmployee.FirstOrDefault()?.TimeScheduleTemplateHeadId ?? 0);
                            if (template != null)
                            {
                                DateTime? date = shiftsForEmployee.OrderBy(x => x.ActualDate).FirstOrDefault()?.ActualDate;
                                if (date.HasValue)
                                {
                                    DateTime templateDateFrom = template.GetCycleStartFromGivenDate(date.Value);
                                    List<TimeSchedulePlanningDayDTO> originalTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, template.TimeScheduleTemplateHeadId, templateDateFrom, templateDateFrom.AddDays(template.NoOfDays - 1), loadTasksAndDelivery: false, useNbrOfDaysFromTemplate: false, includeZeroDays: false);
                                    List<int> dayNumbersFromGUI = shiftsForEmployee.Select(x => x.DayNumber).Distinct().ToList();
                                    List<TimeSchedulePlanningDayDTO> originalTemplateShiftsToRemove = originalTemplateShifts.Where(x => dayNumbersFromGUI.Contains(x.DayNumber)).ToList();
                                    foreach (var originalTemplateShiftsToRemoveGroup in originalTemplateShiftsToRemove.GroupBy(x => x.DayNumber))
                                    {
                                        //Remove guishifts from original
                                        foreach (var item in originalTemplateShiftsToRemoveGroup)
                                        {
                                            originalTemplateShifts.Remove(item);
                                        }
                                    }

                                    foreach (var shiftForEmployee in shiftsForEmployee)
                                    {
                                        shiftForEmployee.WeekNr = CalendarUtility.GetWeekNr(shiftForEmployee.ActualDate);
                                    }

                                    //Add the rest of the template shifts to shiftsForEmployee, so that the whole template can be evaluated
                                    shiftsForEmployee.AddRange(originalTemplateShifts);
                                }
                            }
                        }

                        if (!iDTO.Dates.IsNullOrEmpty())
                        {
                            foreach (DateTime date in iDTO.Dates)
                            {
                                foreach (TimeSchedulePlanningDayDTO shift in shiftsForEmployee)
                                {
                                    shift.ChangeDate(date);
                                }

                                List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(shiftsForEmployee, employeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, rules: iDTO.Rules, rulesToSkip: iDTO.RulesToSkip, evaluateScheduleWithTimeDeviationCauseSetting: true, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate, useAnnualLeave: useAnnualLeave);
                                ruleResults.ForEach(r => r.Date = date);
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                            }
                        }
                        else
                        {
                            List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(shiftsForEmployee, employeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, rules: iDTO.Rules, rulesToSkip: iDTO.RulesToSkip, evaluateScheduleWithTimeDeviationCauseSetting: true, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate, useAnnualLeave: useAnnualLeave);
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                        }

                        if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesOutputDTO TaskEvaluateAbsenceRequestPlannedShiftsAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesInputDTO, EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Perform

                    List<TimeSchedulePlanningDayDTO> allShifts = iDTO.PlannedShifts;

                    int sourceEmployeeId = iDTO.EmployeeId;

                    var shiftsGroupedByEmployee = allShifts.GroupBy(x => x.EmployeeId).ToList();

                    foreach (var shiftsGroupedForEmployee in shiftsGroupedByEmployee)
                    {
                        int targetEmployeeId = shiftsGroupedForEmployee.Key;

                        List<TimeSchedulePlanningDayDTO> shiftsToConsider = new List<TimeSchedulePlanningDayDTO>();
                        foreach (TimeSchedulePlanningDayDTO shift in shiftsGroupedForEmployee.ToList())
                        {
                            if (shift.AbsenceStartTime == shift.AbsenceStopTime)
                                continue;

                            //simulate original shift as a zero shift
                            TimeSchedulePlanningDayDTO zeroShift = new TimeSchedulePlanningDayDTO();
                            EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(zeroShift, shift);
                            zeroShift.EmployeeId = sourceEmployeeId;
                            zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, new DateTime());
                            zeroShift.StopTime = zeroShift.StartTime;
                            shiftsToConsider.Add(zeroShift);

                            //Simulate new presence shifts for source employee
                            List<TimeSchedulePlanningDayDTO> newPresenceShifts = GetAbsenceDividedShifts(shift);
                            newPresenceShifts.ForEach(x => x.EmployeeId = sourceEmployeeId);
                            shiftsToConsider.AddRange(newPresenceShifts);

                            if (targetEmployeeId != -1 && targetEmployeeId != GetHiddenEmployeeIdFromCache())
                            {
                                //Simulate a new shift for target employee
                                shift.StartTime = shift.AbsenceStartTime;
                                shift.StopTime = shift.AbsenceStopTime;
                                shift.TimeScheduleTemplateBlockId = 0;
                                shiftsToConsider.Add(shift);
                            }
                        }

                        var ruleResults = EvaluatePlannedShiftsAgainstWorkRules(shiftsToConsider, targetEmployeeId, false, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules);
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);

                        if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluatePlannedShiftsAgainstWorkRulesEmployeePost()
        {
            var (iDTO, oDTO) = InitTask<EvaluatePlannedShiftsAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Perform

                    List<TimeSchedulePlanningDayDTO> allShifts = iDTO.PlannedShifts.Where(x => x.EmployeePostId.HasValue).ToList();

                    var shiftsGroupedByEmployeePost = allShifts.GroupBy(x => x.EmployeePostId.Value).ToList();

                    foreach (var shiftsGroupedForEmployeePost in shiftsGroupedByEmployeePost)
                    {
                        int employeePostId = shiftsGroupedForEmployeePost.Key;
                        List<TimeSchedulePlanningDayDTO> shiftsForEmployeePost = shiftsGroupedForEmployeePost.ToList();

                        var ruleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(shiftsForEmployeePost, employeePostId, iDTO.Rules);
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                        if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateDragShiftAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateDragShiftAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    TimeSchedulePlanningDayDTO dragedSourceShift = null;
                    TimeSchedulePlanningDayDTO zeroShift = null;
                    List<TimeSchedulePlanningDayDTO> plannedShifts = new List<TimeSchedulePlanningDayDTO>();
                    List<TimeScheduleTemplateBlock> sourceScheduleBlocks = new List<TimeScheduleTemplateBlock>();
                    List<TimeScheduleTemplateBlock> sourceBreaks = new List<TimeScheduleTemplateBlock>();
                    List<TimeScheduleTemplateBlock> targetShedules = new List<TimeScheduleTemplateBlock>();
                    List<TimeScheduleTemplateBlock> targetBreaks = new List<TimeScheduleTemplateBlock>();

                    TimeScheduleTemplateBlock sourceScheduleBlock = GetScheduleBlock(iDTO.SourceShiftId);
                    TimeScheduleTemplateBlock targetScheduleBlock = GetScheduleBlock(iDTO.TargetShiftId);
                    DateTime destinationDate = iDTO.Start.Date;
                    DateTime zero = new DateTime();
                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);

                    if (sourceScheduleBlock != null && sourceScheduleBlock.EmployeeId.HasValue && sourceScheduleBlock.Date.HasValue && sourceScheduleBlock.TimeScheduleEmployeePeriodId.HasValue)
                    {
                        List<TimeScheduleTemplateBlock> sourceLinkedShifts = GetScheduleBlocksLinked(iDTO.TimeScheduleScenarioHeadId, sourceScheduleBlock.EmployeeId.Value, sourceScheduleBlock.Date.Value, sourceScheduleBlock.Link, (TermGroup_TimeScheduleTemplateBlockType)sourceScheduleBlock.Type, null).Where(x => x.TimeScheduleTemplateBlockId != sourceScheduleBlock.TimeScheduleTemplateBlockId).ToList();
                        sourceBreaks.AddRange(sourceLinkedShifts.Where(x => x.BreakType != (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList());

                        if (iDTO.WholeDayAbsence)
                        {
                            List<TimeScheduleTemplateBlock> sourceShiftsForWholeDay = GetTimeScheduleShifts(iDTO.TimeScheduleScenarioHeadId, sourceScheduleBlock.TimeScheduleEmployeePeriodId.Value, null);
                            sourceScheduleBlocks.AddRange(sourceShiftsForWholeDay);
                        }
                        else
                        {
                            sourceScheduleBlocks.Add(sourceScheduleBlock);
                            if (iDTO.KeepSourceShiftsTogether)
                                sourceScheduleBlocks.AddRange(sourceLinkedShifts.Where(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList());

                        }
                    }

                    //Sourcescheduleblocks are always required
                    if (sourceScheduleBlocks.IsNullOrEmpty())
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8830, "Inga schemablock hittades"));
                        return oDTO;
                    }

                    List<DateTime> sourceDates = sourceScheduleBlocks.Select(i => i.Date.Value).Distinct().OrderBy(i => i.Date).ToList();

                    if (targetScheduleBlock != null && targetScheduleBlock.EmployeeId.HasValue && targetScheduleBlock.Date.HasValue)
                    {
                        List<TimeScheduleTemplateBlock> targetLinkedShifts = GetScheduleBlocksLinked(iDTO.TimeScheduleScenarioHeadId, targetScheduleBlock.EmployeeId.Value, targetScheduleBlock.Date.Value, targetScheduleBlock.Link, (TermGroup_TimeScheduleTemplateBlockType)targetScheduleBlock.Type, null).Where(x => x.TimeScheduleTemplateBlockId != targetScheduleBlock.TimeScheduleTemplateBlockId).ToList();
                        targetBreaks.AddRange(targetLinkedShifts.Where(x => x.BreakType != (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList());

                        targetShedules.Add(targetScheduleBlock);
                        if (iDTO.KeepTargetShiftsTogether)
                            targetShedules.AddRange(targetLinkedShifts.Where(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList());

                    }

                    //Targetscheduleblocks are required only if targetShiftId is given
                    if (iDTO.TargetShiftId != 0 && targetShedules.IsNullOrEmpty())
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8830, "Inga schemablock hittades"));
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    switch (iDTO.Action) 
                    {
                        case DragShiftAction.Move:
                            #region Move

                            List<SoeScheduleWorkRules> rulesToSkip = new List<SoeScheduleWorkRules>();

                            foreach (var linkedSourceShedule in sourceScheduleBlocks)
                            {
                                if (linkedSourceShedule.ActualStartTime.HasValue && linkedSourceShedule.ActualStopTime.HasValue)
                                {
                                    zeroShift = linkedSourceShedule.ToTimeSchedulePlanningDayDTO();

                                    var (start, stop) = linkedSourceShedule.GetNewActualStartAndStopTime(destinationDate);
                                    dragedSourceShift = ConvertToTimeSchedulePlanningDayDTO(linkedSourceShedule, iDTO.DestinationEmployeeId, start, stop, sourceBreaks);
                                    dragedSourceShift.SourceTimeScheduleTemplateBlockId = linkedSourceShedule.TimeScheduleTemplateBlockId;
                                    dragedSourceShift.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift

                                    //if it is the SAME employee, we need to take the zero shift into consideration
                                    if (iDTO.DestinationEmployeeId == linkedSourceShedule.EmployeeId)
                                    {
                                        //Zero shift has to be created where the sourceshift was so we need to simulate that scenario                            
                                        zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                        zeroShift.StopTime = zeroShift.StartTime;
                                        plannedShifts.Add(zeroShift);
                                    }

                                    plannedShifts.Add(dragedSourceShift);
                                }
                            }
                            if (iDTO.FromQueue)
                                rulesToSkip.Add(SoeScheduleWorkRules.HoursBeforeAssignShift);

                            List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, rulesToSkip: rulesToSkip, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                            ruleResults.Clear();
                            plannedShifts.Clear();

                            if (sourceScheduleBlock != null && sourceScheduleBlock.EmployeeId.HasValue && iDTO.DestinationEmployeeId != sourceScheduleBlock.EmployeeId.Value)
                            {
                                foreach (var linkedSourceShedule in sourceScheduleBlocks)
                                {
                                    #region Simulate sourceshift as deleted on source

                                    zeroShift = linkedSourceShedule.ToTimeSchedulePlanningDayDTO();
                                    zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                    zeroShift.StopTime = zeroShift.StartTime;
                                    plannedShifts.Add(zeroShift);

                                    #endregion
                                }

                                ruleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, sourceScheduleBlock.EmployeeId.Value, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, new List<SoeScheduleWorkRules> { SoeScheduleWorkRules.AttestedDay }, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                                ruleResults.Clear();
                                plannedShifts.Clear();
                            }
                            #endregion

                            break;
                        case DragShiftAction.Copy:
                        case DragShiftAction.Absence:
                            #region Copy/Absence

                            //Copy:
                            //The owner (employee) of the sourceshift may or may not be the same as destination employeeid,
                            //but it does not matter. Destination employee should treat it as a new planned scheduleblock
                            //Absence:
                            //Destination employee should treat it as a new planned scheduleblock

                            foreach (var linkedSourceShedule in sourceScheduleBlocks)
                            {
                                if (linkedSourceShedule.ActualStartTime.HasValue && linkedSourceShedule.ActualStopTime.HasValue)
                                {
                                    var (start, stop) = linkedSourceShedule.GetNewActualStartAndStopTime(destinationDate);
                                    dragedSourceShift = ConvertToTimeSchedulePlanningDayDTO(linkedSourceShedule, iDTO.DestinationEmployeeId, start, stop, sourceBreaks);
                                    dragedSourceShift.TimeScheduleTemplateBlockId = 0;
                                    plannedShifts.Add(dragedSourceShift);
                                }
                            }

                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);

                            #endregion
                            break;
                        case DragShiftAction.Replace:
                        case DragShiftAction.ReplaceAndFree:
                            #region Replace/ReplaceAndFree

                            //Replace:
                            //TargetShifts and SourceShifts will be simulated as deleted on destinationEmployee(= target employee) and sourceemployee
                            //We also need to simulate the new shifts that destinationEmployeeId (= target employeeid) will recieve from sourceemployee, wich are copies of sourceshifts

                            //ReplaceAndFree:
                            //Same as above because "free" means hiddenemployee och schedules for hiddenemployee are not evaluated

                            foreach (var linkedTargetShedule in targetShedules)
                            {
                                #region Simulate targetshift as deleted

                                zeroShift = linkedTargetShedule.ToTimeSchedulePlanningDayDTO();
                                zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                zeroShift.StopTime = zeroShift.StartTime;
                                plannedShifts.Add(zeroShift);

                                #endregion
                            }

                            foreach (var linkedSourceShedule in sourceScheduleBlocks)
                            {
                                #region Simulate sourceshift as deleted

                                zeroShift = linkedSourceShedule.ToTimeSchedulePlanningDayDTO();
                                zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                zeroShift.StopTime = zeroShift.StartTime;
                                plannedShifts.Add(zeroShift);

                                #endregion

                                #region Simulate new shift for Destination employee

                                var (start, stop) = linkedSourceShedule.GetNewActualStartAndStopTime(destinationDate);
                                dragedSourceShift = ConvertToTimeSchedulePlanningDayDTO(linkedSourceShedule, iDTO.DestinationEmployeeId, start, stop, sourceBreaks);
                                dragedSourceShift.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift
                                plannedShifts.Add(dragedSourceShift);

                                #endregion
                            }

                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);

                            #endregion
                            break;
                        case DragShiftAction.SwapEmployee:
                            #region SwapEmployee

                            int sourceEmployeeId = sourceScheduleBlock.EmployeeId.Value;
                            DateTime sourceDate = sourceScheduleBlock.Date.Value;
                            int targetEmployeeId = iDTO.DestinationEmployeeId;

                            TimeSchedulePlanningDayDTO newShiftForSource = null;
                            TimeSchedulePlanningDayDTO newShiftForTarget = null;

                            #region Source

                            foreach (var linkedTargetShedule in targetShedules)
                            {
                                #region Simulate new shift for source employee

                                var (start, stop) = linkedTargetShedule.GetNewActualStartAndStopTime(destinationDate);
                                newShiftForSource = ConvertToTimeSchedulePlanningDayDTO(linkedTargetShedule, sourceEmployeeId, start, stop, targetBreaks);
                                newShiftForSource.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift
                                plannedShifts.Add(newShiftForSource);

                                #endregion
                            }

                            foreach (var linkedSourceShedule in sourceScheduleBlocks)
                            {
                                #region Simulate sourceshift as deleted on source

                                zeroShift = linkedSourceShedule.ToTimeSchedulePlanningDayDTO();
                                zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                zeroShift.StopTime = zeroShift.StartTime;
                                plannedShifts.Add(zeroShift);

                                #endregion
                            }

                            ruleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, sourceEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                            ruleResults.Clear();
                            plannedShifts.Clear();

                            #endregion

                            #region Target

                            foreach (var linkedTargetShedule in targetShedules)
                            {
                                #region Simulate targetshift as deleted on target

                                zeroShift = linkedTargetShedule.ToTimeSchedulePlanningDayDTO();
                                zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                zeroShift.StopTime = zeroShift.StartTime;
                                plannedShifts.Add(zeroShift);

                                #endregion
                            }

                            foreach (var linkedSourceShedule in sourceScheduleBlocks)
                            {
                                #region Simulate new shift for target employee

                                var (start, stop) = linkedSourceShedule.GetNewActualStartAndStopTime(sourceDate);
                                newShiftForTarget = ConvertToTimeSchedulePlanningDayDTO(linkedSourceShedule, targetEmployeeId, start, stop, sourceBreaks);
                                newShiftForTarget.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift
                                plannedShifts.Add(newShiftForTarget);

                                #endregion
                            }

                            ruleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, targetEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                            ruleResults.Clear();
                            plannedShifts.Clear();

                            #endregion

                            #endregion
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateScheduleSwapAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateScheduleSwapAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    if (!TimeScheduleManager.GetEmployeeSwapRequestRowWithShifts(this.entities, base.ActorCompanyId, iDTO.TimeScheduleSwapRequestId, out List<TimeScheduleTemplateBlock> initiatorShifts, out List<TimeScheduleTemplateBlock> swapwithShifts))
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8830, "Inga schemablock hittades"));
                        return oDTO;
                    }

                    List<TimeSchedulePlanningDayDTO> plannedShifts = new List<TimeSchedulePlanningDayDTO>();
                    List<TimeScheduleTemplateBlock> sourceScheduleBlocks = initiatorShifts.Where(w => !w.IsBreak && w.Date.HasValue).ToList();
                    List<TimeScheduleTemplateBlock> sourceBreaks = initiatorShifts.Where(w => w.IsBreak && w.Date.HasValue).ToList();
                    List<TimeScheduleTemplateBlock> targetShedules = swapwithShifts.Where(w => !w.IsBreak && w.Date.HasValue).ToList();
                    List<TimeScheduleTemplateBlock> targetBreaks = swapwithShifts.Where(w => w.IsBreak && w.Date.HasValue).ToList();
                    DateTime zero = new DateTime();
                    TimeSchedulePlanningDayDTO zeroShift = null;

                    //Sourcescheduleblocks are always required
                    if (sourceScheduleBlocks.IsNullOrEmpty() || targetShedules.IsNullOrEmpty())
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult((int)ActionResultSave.InsufficientInput, GetText(8830, "Inga schemablock hittades"));
                        return oDTO;
                    }

                    List<DateTime> sourceDates = sourceScheduleBlocks.Select(i => i.Date.Value).Distinct().OrderBy(i => i.Date).ToList();

                    #endregion

                    #region Perform

                    int sourceEmployeeId = sourceScheduleBlocks.FirstOrDefault()?.EmployeeId.Value ?? 0;
                    int targetEmployeeId = targetShedules.FirstOrDefault()?.EmployeeId.Value ?? 0;

                    TimeSchedulePlanningDayDTO newShiftForSource = null;
                    TimeSchedulePlanningDayDTO newShiftForTarget = null;

                    #region Source

                    foreach (var linkedTargetShedule in targetShedules)
                    {
                        #region Simulate new shift for source employee

                        var (start, stop) = linkedTargetShedule.GetNewActualStartAndStopTime(linkedTargetShedule.Date.Value);
                        newShiftForSource = ConvertToTimeSchedulePlanningDayDTO(linkedTargetShedule, sourceEmployeeId, start, stop, targetBreaks);
                        newShiftForSource.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift
                        plannedShifts.Add(newShiftForSource);

                        #endregion
                    }

                    foreach (var linkedSourceShedule in sourceScheduleBlocks)
                    {
                        #region Simulate sourceshift as deleted on source

                        zeroShift = linkedSourceShedule.ToTimeSchedulePlanningDayDTO();
                        zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                        zeroShift.StopTime = zeroShift.StartTime;
                        plannedShifts.Add(zeroShift);

                        #endregion
                    }

                    List<EvaluateWorkRuleResultDTO> ruleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, sourceEmployeeId, false, null, null);
                    oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                    ruleResults.Clear();
                    plannedShifts.Clear();

                    #endregion

                    #region Target

                    foreach (var linkedTargetShedule in targetShedules)
                    {
                        #region Simulate targetshift as deleted on target

                        zeroShift = linkedTargetShedule.ToTimeSchedulePlanningDayDTO();
                        zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                        zeroShift.StopTime = zeroShift.StartTime;
                        plannedShifts.Add(zeroShift);

                        #endregion
                    }

                    foreach (var linkedSourceShedule in sourceScheduleBlocks)
                    {
                        #region Simulate new shift for target employee

                        var (start, stop) = linkedSourceShedule.GetNewActualStartAndStopTime(linkedSourceShedule.Date.Value);
                        newShiftForTarget = ConvertToTimeSchedulePlanningDayDTO(linkedSourceShedule, targetEmployeeId, start, stop, sourceBreaks);
                        newShiftForTarget.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift
                        plannedShifts.Add(newShiftForTarget);

                        #endregion
                    }

                    ruleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, targetEmployeeId, false, null, null, sourceDates);
                    oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                    ruleResults.Clear();
                    plannedShifts.Clear();

                    #endregion

                    #endregion

                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateDragShiftAgainstWorkRulesMultipel()
        {
            var (iDTO, oDTO) = InitTask<EvaluateDragShiftMultipelAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if ((iDTO == null || iDTO.SourceShiftIds == null))
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }
            if ((iDTO.Action == DragShiftAction.CopyWithCycle || iDTO.Action == DragShiftAction.MoveWithCycle) && (!iDTO.StandbyCycleWeek.HasValue || !iDTO.StandbyCycleDateFrom.HasValue || !iDTO.StandbyCycleDateTo.HasValue))
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);

                    if (iDTO.IsStandByView && iDTO.Rules == null)
                    {
                        iDTO.Rules = new List<SoeScheduleWorkRules>
                        {
                            SoeScheduleWorkRules.AttestedDay,
                            SoeScheduleWorkRules.OverlappingShifts
                        };
                    }

                    //Dont evaluate for hiddenemployee
                    int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                    if (iDTO.DestinationEmployeeId == hiddenEmployeeId)
                    {
                        oDTO.Result.Success = true;
                        return oDTO;
                    }

                    TimeSchedulePlanningDayDTO sourceShift = null;
                    TimeSchedulePlanningDayDTO zeroShift = null;
                    List<TimeSchedulePlanningDayDTO> plannedShifts = new List<TimeSchedulePlanningDayDTO>();
                    List<TimeScheduleTemplateBlock> sourceScheduleBlocks = new List<TimeScheduleTemplateBlock>();

                    DateTime zero = new DateTime();
                    if (iDTO.Action == DragShiftAction.MoveWithCycle || iDTO.Action == DragShiftAction.CopyWithCycle)
                    {
                        sourceScheduleBlocks.AddRange(GetSourceShiftsAndShiftsInCycle(iDTO.SourceShiftIds, iDTO.StandbyCycleWeek.Value, iDTO.StandbyCycleDateFrom.Value, iDTO.StandbyCycleDateTo.Value, iDTO.TimeScheduleScenarioHeadId).Where(x => x.Date.HasValue));
                    }
                    else
                    {
                        sourceScheduleBlocks.AddRange(GetScheduleBlocks(iDTO.SourceShiftIds).Where(x => x.Date.HasValue));
                    }

                    sourceScheduleBlocks = sourceScheduleBlocks.OrderBy(x => x.Date.Value).ToList();
                    List<DateTime> sourceDates = sourceScheduleBlocks.Select(i => i.Date.Value).Distinct().OrderBy(i => i.Date).ToList();

                    #endregion

                    #region Perform

                    switch (iDTO.Action)
                    {
                        case DragShiftAction.Move:
                        case DragShiftAction.MoveWithCycle:
                            #region Move

                            foreach (var sourceScheduleBlock in sourceScheduleBlocks)
                            {
                                if (sourceScheduleBlock.ActualStartTime.HasValue && sourceScheduleBlock.ActualStopTime.HasValue)
                                {
                                    zeroShift = sourceScheduleBlock.ToTimeSchedulePlanningDayDTO();

                                    DateTime start = CalendarUtility.MergeDateAndTime(sourceScheduleBlock.Date.Value.AddDays(iDTO.OffsetDays), sourceScheduleBlock.StartTime);
                                    DateTime stop = start.Add(sourceScheduleBlock.StopTime - sourceScheduleBlock.StartTime);

                                    List<TimeScheduleTemplateBlock> sourceLinkedShifts = sourceScheduleBlock.EmployeeId.HasValue ? GetScheduleBlocksLinked(iDTO.TimeScheduleScenarioHeadId, sourceScheduleBlock.EmployeeId.Value, sourceScheduleBlock.Date.Value, sourceScheduleBlock.Link, (TermGroup_TimeScheduleTemplateBlockType)sourceScheduleBlock.Type, null).Where(x => x.TimeScheduleTemplateBlockId != sourceScheduleBlock.TimeScheduleTemplateBlockId).ToList() : new List<TimeScheduleTemplateBlock>();
                                    List<TimeScheduleTemplateBlock> sourceBreaks = sourceLinkedShifts.Where(x => x.BreakType != (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList();

                                    sourceShift = ConvertToTimeSchedulePlanningDayDTO(sourceScheduleBlock, iDTO.DestinationEmployeeId, start, stop, sourceBreaks);
                                    sourceShift.SourceTimeScheduleTemplateBlockId = sourceScheduleBlock.TimeScheduleTemplateBlockId;
                                    sourceShift.TimeScheduleTemplateBlockId = 0; //treat it as a new planned shift

                                    //if it is the SAME employee, we need to take the zero shift into consideration
                                    if (iDTO.DestinationEmployeeId == sourceScheduleBlock.EmployeeId)
                                    {
                                        //Zero shift has to be created where the sourceshift was so we need to simulate that scenario                            
                                        zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                                        zeroShift.StopTime = zeroShift.StartTime;
                                        plannedShifts.Add(zeroShift);
                                    }

                                    plannedShifts.Add(sourceShift);
                                }
                            }

                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);

                            #endregion
                            break;
                        case DragShiftAction.Copy:
                        case DragShiftAction.CopyWithCycle:
                            #region Copy
                            //The owner (employee) of the sourceshift may or may not be the same as destination employeeid,
                            //but it does not matter. Destination employee should treat it as a new planned scheduleblock

                            foreach (var sourceScheduleBlock in sourceScheduleBlocks)
                            {
                                if (sourceScheduleBlock.ActualStartTime.HasValue && sourceScheduleBlock.ActualStopTime.HasValue)
                                {
                                    DateTime start = CalendarUtility.MergeDateAndTime(sourceScheduleBlock.Date.Value.AddDays(iDTO.OffsetDays), sourceScheduleBlock.StartTime);
                                    DateTime stop = start.Add(sourceScheduleBlock.StopTime - sourceScheduleBlock.StartTime);

                                    List<TimeScheduleTemplateBlock> sourceLinkedShifts = sourceScheduleBlock.EmployeeId.HasValue ? GetScheduleBlocksLinked(iDTO.TimeScheduleScenarioHeadId, sourceScheduleBlock.EmployeeId.Value, sourceScheduleBlock.Date.Value, sourceScheduleBlock.Link, (TermGroup_TimeScheduleTemplateBlockType)sourceScheduleBlock.Type, null).Where(x => x.TimeScheduleTemplateBlockId != sourceScheduleBlock.TimeScheduleTemplateBlockId).ToList() : new List<TimeScheduleTemplateBlock>();
                                    List<TimeScheduleTemplateBlock> sourceBreaks = sourceLinkedShifts.Where(x => x.BreakType != (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList();

                                    sourceShift = ConvertToTimeSchedulePlanningDayDTO(sourceScheduleBlock, iDTO.DestinationEmployeeId, start, stop, sourceBreaks);
                                    sourceShift.TimeScheduleTemplateBlockId = 0;
                                    plannedShifts.Add(sourceShift);
                                }
                            }

                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);

                            #endregion
                            break;
                        case DragShiftAction.Delete:
                            #region Delete

                            foreach (var sourceScheduleBlock in sourceScheduleBlocks.Where(b => b.Type != (int)TimeScheduleBlockType.Booking).ToList())
                            {
                                if (sourceScheduleBlock.ActualStartTime.HasValue && sourceScheduleBlock.ActualStopTime.HasValue)
                                {
                                    DateTime start = CalendarUtility.MergeDateAndTime(sourceScheduleBlock.Date.Value.AddDays(iDTO.OffsetDays), CalendarUtility.DATETIME_DEFAULT);
                                    DateTime stop = CalendarUtility.MergeDateAndTime(sourceScheduleBlock.Date.Value.AddDays(iDTO.OffsetDays), CalendarUtility.DATETIME_DEFAULT);

                                    List<TimeScheduleTemplateBlock> sourceLinkedShifts = sourceScheduleBlock.EmployeeId.HasValue ? GetScheduleBlocksLinked(iDTO.TimeScheduleScenarioHeadId, sourceScheduleBlock.EmployeeId.Value, sourceScheduleBlock.Date.Value, sourceScheduleBlock.Link, (TermGroup_TimeScheduleTemplateBlockType)sourceScheduleBlock.Type, null).Where(x => x.TimeScheduleTemplateBlockId != sourceScheduleBlock.TimeScheduleTemplateBlockId).ToList() : new List<TimeScheduleTemplateBlock>();
                                    List<TimeScheduleTemplateBlock> sourceBreaks = sourceLinkedShifts.Where(x => x.BreakType != (int)SoeTimeScheduleTemplateBlockBreakType.None).ToList();

                                    sourceShift = ConvertToTimeSchedulePlanningDayDTO(sourceScheduleBlock, iDTO.DestinationEmployeeId, start, stop, sourceBreaks);
                                    sourceShift.SourceTimeScheduleTemplateBlockId = sourceScheduleBlock.TimeScheduleTemplateBlockId;
                                    sourceShift.TimeScheduleTemplateBlockId = 0;
                                    plannedShifts.Add(sourceShift);
                                }
                            }

                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, iDTO.Rules, sourceDates, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);

                            #endregion
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateDragTemplateShiftAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateDragTemplateShiftAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    TimeScheduleTemplateHead sourceTemplateHead = GetTimeScheduleTemplateHead(iDTO.SourceTemplateHeadId);
                    TimeScheduleTemplateHead targetTemplateHead = GetTimeScheduleTemplateHead(iDTO.TargetTemplateHeadId);
                    List<int> sourceChangedDayNumbers = new List<int>();
                    List<int> targetChangedDayNumbers = new List<int>();
                    if (sourceTemplateHead == null || targetTemplateHead == null)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false);
                        return oDTO;
                    }
                    if (!iDTO.TargetEmployeeId.HasValue && !iDTO.TargetEmployeePostId.HasValue)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false);
                        return oDTO;
                    }

                    #region Get target template 

                    //use targetdate a relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                    DateTime targetDateFrom = targetTemplateHead.GetCycleStartFromGivenDate(iDTO.TargetStart.Date);
                    List<TimeSchedulePlanningDayDTO> targetTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, targetTemplateHead.TimeScheduleTemplateHeadId, targetDateFrom, targetDateFrom.AddDays(targetTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                    #endregion

                    #region Get source template

                    //use sourcedate a relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                    DateTime sourceDateFrom = sourceTemplateHead.GetCycleStartFromGivenDate(iDTO.SourceDate.Date);
                    List<TimeSchedulePlanningDayDTO> sourceTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom, sourceDateFrom.AddDays(sourceTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                    #endregion

                    #endregion

                    #region Perform

                    switch (iDTO.Action)
                    {
                        case DragShiftAction.Move:
                            #region Move

                            oDTO.EvaluateWorkRulesResult.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, iDTO.SourceShiftId, iDTO.KeepSourceShiftsTogether, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, iDTO.TargetStart.Date, targetTemplateHead, targetDateFrom, Guid.NewGuid(), false, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                            if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                                return oDTO;

                            if (iDTO.TargetEmployeeId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeeId.Value, true, null, iDTO.Rules);
                            else if (iDTO.TargetEmployeePostId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeePostId.Value, iDTO.Rules);

                            #endregion
                            break;
                        case DragShiftAction.Copy:
                            #region Copy

                            oDTO.EvaluateWorkRulesResult.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, iDTO.SourceShiftId, iDTO.KeepSourceShiftsTogether, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, iDTO.TargetStart.Date, targetTemplateHead, targetDateFrom, Guid.NewGuid(), true, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                            if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                                return oDTO;

                            if (iDTO.TargetEmployeeId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeeId.Value, true, null, iDTO.Rules);
                            else if (iDTO.TargetEmployeePostId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeePostId.Value, iDTO.Rules);

                            #endregion
                            break;
                        case DragShiftAction.SwapEmployee:
                            List<EvaluateWorkRuleResultDTO> ruleResults = new List<EvaluateWorkRuleResultDTO>();
                            #region SwapEmployee

                            oDTO.Result = SwapEmployeesOnTemplateShifts(ref sourceTemplateShifts, ref targetTemplateShifts, iDTO.SourceShiftId, iDTO.KeepSourceShiftsTogether, sourceTemplateHead, iDTO.TargetShiftId, iDTO.KeepTargetShiftsTogether, iDTO.SourceDate, iDTO.TargetStart.Date, sourceDateFrom, targetDateFrom, targetTemplateHead, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);

                            if (iDTO.TargetEmployeeId.HasValue || iDTO.TargetEmployeePostId.HasValue)
                            {
                                if (iDTO.TargetEmployeeId.HasValue)
                                    ruleResults = EvaluatePlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeeId.Value, true, null, iDTO.Rules);
                                else if (iDTO.TargetEmployeePostId.HasValue)
                                    ruleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeePostId.Value, iDTO.Rules);

                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                                ruleResults.Clear();
                            }

                            if (sourceTemplateHead.EmployeeId.HasValue || sourceTemplateHead.EmployeePostId.HasValue)
                            {
                                if (sourceTemplateHead.EmployeeId.HasValue)
                                    ruleResults = EvaluatePlannedShiftsAgainstWorkRules(sourceTemplateShifts, sourceTemplateHead.EmployeeId.Value, true, null, iDTO.Rules);
                                else if (sourceTemplateHead.EmployeePostId.HasValue)
                                    ruleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(sourceTemplateShifts, sourceTemplateHead.EmployeePostId.Value, iDTO.Rules);

                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);
                                ruleResults.Clear();
                            }
                            #endregion
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateDragTemplateShiftAgainstWorkRulesMultipel()
        {
            var (iDTO, oDTO) = InitTask<EvaluateDragTemplateShiftMultipelAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    //Dont evaluate for hiddenemployee
                    int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                    if (iDTO.TargetEmployeeId.HasValue && iDTO.TargetEmployeeId.Value == hiddenEmployeeId)
                    {
                        oDTO.Result.Success = true;
                        return oDTO;
                    }

                    TimeScheduleTemplateHead sourceTemplateHead = GetTimeScheduleTemplateHead(iDTO.SourceTemplateHeadId);
                    TimeScheduleTemplateHead targetTemplateHead = GetTimeScheduleTemplateHead(iDTO.TargetTemplateHeadId);
                    List<int> sourceChangedDayNumbers = new List<int>();
                    List<int> targetChangedDayNumbers = new List<int>();
                    if (sourceTemplateHead == null || targetTemplateHead == null)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false);
                        return oDTO;
                    }
                    if (!iDTO.TargetEmployeeId.HasValue && !iDTO.TargetEmployeePostId.HasValue)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false);
                        return oDTO;
                    }

                    #region Get target template 

                    //use targetdate a relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                    DateTime targetDateFrom = targetTemplateHead.GetCycleStartFromGivenDate(iDTO.FirstTargetDate);
                    var targetTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, targetTemplateHead.TimeScheduleTemplateHeadId, targetDateFrom, targetDateFrom.AddDays(targetTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                    #endregion

                    #region Get source template

                    DateTime sourceDateFrom = sourceTemplateHead.GetCycleStartFromGivenDate(iDTO.FirstSourceDate.Date);
                    var sourceTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom, sourceDateFrom.AddDays(sourceTemplateHead.NoOfDays - 1), loadTasksAndDelivery: true, useNbrOfDaysFromTemplate: false);

                    #endregion

                    #endregion

                    #region Perform

                    switch (iDTO.Action)
                    {
                        case DragShiftAction.Move:
                            #region Move

                            foreach (var sourceShiftId in iDTO.SourceShiftIds)
                            {
                                var sourceShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
                                if (sourceShift == null)
                                    continue;

                                // (Drag shift from a slot to another slot)                                

                                // Move source shift to new date and possibly new employee                                                                    
                                oDTO.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, sourceShiftId, false, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays), targetTemplateHead, targetDateFrom, Guid.NewGuid(), false, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }


                            if (iDTO.TargetEmployeeId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeeId.Value, true, null, iDTO.Rules);
                            else if (iDTO.TargetEmployeePostId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeePostId.Value, iDTO.Rules);

                            #endregion
                            break;
                        case DragShiftAction.Copy:
                            #region Copy
                            //The owner (employee) of the sourceshift may or may not be the same as destination employeeid,
                            //but it does not matter. Destination employee should treat it as a new planned scheduleblock

                            foreach (var sourceShiftId in iDTO.SourceShiftIds)
                            {
                                var sourceShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
                                if (sourceShift == null)
                                    continue;

                                // (Drag shift from a slot to another slot)                                
                                // Move source shift to new date and possibly new employee                                                                    
                                oDTO.Result = CopyOrMoveTemplateShift(ref sourceTemplateShifts, ref targetTemplateShifts, sourceShiftId, false, iDTO.TargetEmployeeId, iDTO.TargetEmployeePostId, sourceShift.StartTime.Date.AddDays(iDTO.OffsetDays), targetTemplateHead, targetDateFrom, Guid.NewGuid(), true, ref sourceChangedDayNumbers, ref targetChangedDayNumbers);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }

                            if (iDTO.TargetEmployeeId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeeId.Value, true, null, iDTO.Rules);
                            else if (iDTO.TargetEmployeePostId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(targetTemplateShifts, iDTO.TargetEmployeePostId.Value, iDTO.Rules);

                            #endregion
                            break;
                        case DragShiftAction.Delete:
                            #region Delete

                            var plannedShifts = new List<TimeSchedulePlanningDayDTO>();
                            foreach (var sourceShiftId in iDTO.SourceShiftIds)
                            {
                                var sourceShift = sourceTemplateShifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == sourceShiftId);
                                if (sourceShift == null)
                                    continue;

                                sourceShift.SourceTimeScheduleTemplateBlockId = sourceShift.TimeScheduleTemplateBlockId;
                                sourceShift.TimeScheduleTemplateBlockId = 0;
                                if (sourceShift.TimeScheduleTemplateHeadId != 0)
                                    sourceShift.StopTime = sourceShift.StartTime;

                                plannedShifts.Add(sourceShift);
                            }

                            List<SoeScheduleWorkRules> rulesToSkip = new List<SoeScheduleWorkRules>()
                            {
                                SoeScheduleWorkRules.MinorsWorkTimeWeek,
                                SoeScheduleWorkRules.WorkTimeWeekMaxMin,
                                SoeScheduleWorkRules.WorkTimeWeekPartTimeWorkers,
                            };

                            if (iDTO.TargetEmployeeId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.TargetEmployeeId.Value, true, null, iDTO.Rules, rulesToSkip: rulesToSkip);
                            else if (iDTO.TargetEmployeePostId.HasValue)
                                oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluateEmployeePostPlannedShiftsAgainstWorkRules(plannedShifts, iDTO.TargetEmployeePostId.Value, iDTO.Rules);

                            #endregion
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateSplitShiftAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<SplitTimeScheduleShiftInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    bool useLeisureCodes = GetCompanyBoolSettingFromCache(CompanySettingType.UseLeisureCodes);

                    List<TimeSchedulePlanningDayDTO> plannedShifts = new List<TimeSchedulePlanningDayDTO>();

                    DateTime zero = new DateTime();
                    // Copy DTO
                    TimeSchedulePlanningDayDTO clone = new TimeSchedulePlanningDayDTO();
                    TimeSchedulePlanningDayDTO zeroShift = new TimeSchedulePlanningDayDTO();
                    EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(clone, iDTO.Shift);
                    EntityUtil.CopyDTO<TimeSchedulePlanningDayDTO>(zeroShift, iDTO.Shift);

                    //shift1: simulate the original shift as a zero shift
                    zeroShift.StartTime = CalendarUtility.MergeDateAndTime(zeroShift.StartTime.Date, zero);
                    zeroShift.StopTime = zeroShift.StartTime;


                    //shift 2: simulate as a new shift for employeeid1
                    iDTO.Shift.StopTime = iDTO.SplitTime;
                    iDTO.Shift.TimeScheduleTemplateBlockId = 0;
                    iDTO.Shift.EmployeeId = iDTO.EmployeeId1;

                    //shift 3: simulate as a new shift for employeeid2
                    clone.StartTime = iDTO.SplitTime;
                    clone.TimeScheduleTemplateBlockId = 0;
                    clone.EmployeeId = iDTO.EmployeeId2;


                    //Add shift1 (zeroshift is only needed if one part of the splitted shift will still belong to the originalemployee. i.e: the user splits the shift anf gives one part of it to another employee )
                    if (zeroShift.EmployeeId == iDTO.EmployeeId1 || (zeroShift.EmployeeId == iDTO.EmployeeId2))
                        plannedShifts.Add(zeroShift);

                    //Add shift2
                    plannedShifts.Add(iDTO.Shift);
                    //Add shift3
                    plannedShifts.Add(clone);

                    #endregion

                    #region Perform

                    var shiftsGroupedByEmployee = plannedShifts.GroupBy(x => x.EmployeeId).ToList();

                    foreach (var shiftsGroupedForEmployee in shiftsGroupedByEmployee)
                    {
                        int employeeId = shiftsGroupedForEmployee.Key;

                        List<TimeSchedulePlanningDayDTO> shiftsForEmployee = shiftsGroupedForEmployee.ToList();

                        var ruleResults = EvaluatePlannedShiftsAgainstWorkRules(shiftsForEmployee, employeeId, iDTO.IsPersonalScheduleTemplate, iDTO.TimeScheduleScenarioHeadId, useLeisureCodes: useLeisureCodes, planningPeriodStartDate: iDTO.PlanningPeriodStartDate, planningPeriodStopDate: iDTO.PlanningPeriodStopDate);
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(ruleResults);

                        if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateSplitTemplateShiftAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<SplitTemplateTimeScheduleShiftInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Init

                    if (iDTO.EmployeeId1 == 0)
                        iDTO.EmployeeId1 = null;
                    if (iDTO.EmployeeId2 == 0)
                        iDTO.EmployeeId2 = null;
                    if (iDTO.EmployeePostId1 == 0)
                        iDTO.EmployeePostId1 = null;
                    if (iDTO.EmployeePostId2 == 0)
                        iDTO.EmployeePostId2 = null;

                    TimeScheduleTemplateHead sourceTemplateHead = GetTimeScheduleTemplateHead(iDTO.SourceTemplateHeadId);
                    TimeScheduleTemplateHead target1TemplateHead = GetTimeScheduleTemplateHead(iDTO.TemplateHeadId1);
                    TimeScheduleTemplateHead target2TemplateHead = GetTimeScheduleTemplateHead(iDTO.TemplateHeadId2);
                    if (sourceTemplateHead == null || target1TemplateHead == null || target2TemplateHead == null)
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false);
                        return oDTO;
                    }

                    List<int> sourceChangedDayNumbers = new List<int>();
                    List<int> target1ChangedDayNumbers = new List<int>();
                    List<int> target2ChangedDayNumbers = new List<int>();

                    if ((!iDTO.EmployeeId1.HasValue && !iDTO.EmployeePostId1.HasValue) || (!iDTO.EmployeeId2.HasValue && !iDTO.EmployeePostId2.HasValue))
                    {
                        oDTO.EvaluateWorkRulesResult.Result = new ActionResult(false);
                        return oDTO;
                    }

                    int? sourceEmployeeId = iDTO.SourceShift.EmployeeId != 0 ? iDTO.SourceShift.EmployeeId : (int?)null;
                    int? sourceEmployeePostId = iDTO.SourceShift.EmployeePostId;

                    #region Get target template 

                    //use source shift actual date as relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                    DateTime target1DateFrom = target1TemplateHead.GetCycleStartFromGivenDate(iDTO.SourceShift.ActualDate.Date);
                    List<TimeSchedulePlanningDayDTO> target1TemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, target1TemplateHead.TimeScheduleTemplateHeadId, target1DateFrom, target1DateFrom, loadTasksAndDelivery: true);

                    DateTime target2DateFrom = target2TemplateHead.GetCycleStartFromGivenDate(iDTO.SourceShift.ActualDate.Date);
                    List<TimeSchedulePlanningDayDTO> target2TemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, target2TemplateHead.TimeScheduleTemplateHeadId, target2DateFrom, target2DateFrom, loadTasksAndDelivery: true);

                    #endregion

                    #region Get source template

                    //use sourcedate as relativ when datefrom is calculate, its needed so we can calculate the correct daynumber
                    DateTime sourceDateFrom = sourceTemplateHead.GetCycleStartFromGivenDate(iDTO.SourceShift.ActualDate.Date);
                    List<TimeSchedulePlanningDayDTO> sourceTemplateShifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, sourceTemplateHead.TimeScheduleTemplateHeadId, sourceDateFrom, sourceDateFrom, loadTasksAndDelivery: true);

                    #endregion

                    #region Split shift

                    #region Prereq

                    // Link
                    if (!iDTO.SourceShift.Link.HasValue)
                        iDTO.SourceShift.Link = GetNewShiftLink();

                    // Copy DTO
                    TimeSchedulePlanningDayDTO newShift = new TimeSchedulePlanningDayDTO();
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

                    List<TimeScheduleTemplateBlockTaskDTO> allTasks = GetTimeScheduleTemplateBlockTasks(iDTO.SourceShift.TimeScheduleTemplateBlockId).ToDTOs().ToList();
                    //Tasks that will get a new parent
                    List<TimeScheduleTemplateBlockTaskDTO> tasksToMove = allTasks.GetTaskThatStartsAfterGivenTime(iDTO.SplitTime);
                    List<TimeScheduleTemplateBlockTaskDTO> splittedTasks = new List<TimeScheduleTemplateBlockTaskDTO>();

                    foreach (var task in allTasks.Where(t => t.IsOverlapped(iDTO.SplitTime)))
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

                    iDTO.SourceShift.Tasks = splittedTasks;
                    newShift.Tasks = tasksToMove;

                    #endregion

                    #endregion

                    #endregion

                    #region Perform

                    //Move source shift
                    oDTO.EvaluateWorkRulesResult.Result = ApplyMoveTemplateShift(ref sourceTemplateShifts, ref target1TemplateShifts, iDTO.SourceShift, target1TemplateHead, iDTO.EmployeeId1, iDTO.EmployeePostId1, iDTO.SourceShift.ActualDate, target1DateFrom, iDTO.SourceShift.Link ?? Guid.NewGuid(), ref sourceChangedDayNumbers, ref target1ChangedDayNumbers);
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
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
                        oDTO.EvaluateWorkRulesResult.Result = ApplyCopyTemplateShift(ref target2TemplateShifts, newShift, target2TemplateHead, iDTO.EmployeeId2, iDTO.EmployeePostId2, iDTO.SourceShift.ActualDate, target2DateFrom, newShift.Link ?? Guid.NewGuid(), ref target2ChangedDayNumbers);
                        if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                            return oDTO;
                    }
                    else
                    {
                        //Copy new shift
                        oDTO.EvaluateWorkRulesResult.Result = ApplyCopyTemplateShift(ref target1TemplateShifts, newShift, target1TemplateHead, iDTO.EmployeeId1, iDTO.EmployeePostId1, iDTO.SourceShift.ActualDate, target1DateFrom, newShift.Link ?? Guid.NewGuid(), ref target1ChangedDayNumbers);
                        if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                            return oDTO;
                    }

                    if (iDTO.EmployeeId1.HasValue)
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(EvaluatePlannedShiftsAgainstWorkRules(target1TemplateShifts, iDTO.EmployeeId1.Value, true, null));
                    else if (iDTO.EmployeePostId1.HasValue)
                        oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(EvaluateEmployeePostPlannedShiftsAgainstWorkRules(target1TemplateShifts, iDTO.EmployeePostId1.Value));

                    if (iDTO.EmployeeId1 != iDTO.EmployeeId2 || iDTO.EmployeePostId1 != iDTO.EmployeePostId2)
                    {
                        if (iDTO.EmployeeId2.HasValue)
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(EvaluatePlannedShiftsAgainstWorkRules(target2TemplateShifts, iDTO.EmployeeId2.Value, true, null));
                        else if (iDTO.EmployeePostId2.HasValue)
                            oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults.AddRange(EvaluateEmployeePostPlannedShiftsAgainstWorkRules(target2TemplateShifts, iDTO.EmployeePostId2.Value));
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private EvaluateScheduleWorkRulesOutputDTO TaskEvaluateAssignTaskToEmployeeAgainstWorkRules()
        {
            var (iDTO, oDTO) = InitTask<EvaluateAssignTaskToEmployeeAgainstWorkRulesInputDTO, EvaluateScheduleWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Perform

                try
                {
                    #region Init

                    if (iDTO.TaskDTOs.Any(x => !x.StartTime.HasValue || !x.StopTime.HasValue))
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(8766, "En eller flera arbetsuppgifter saknar tider."));
                        return oDTO;
                    }

                    //Dont evaluate for hiddenemployee
                    int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
                    if (iDTO.DestinationEmployeeId == hiddenEmployeeId)
                    {
                        oDTO.Result.Success = true;
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    List<TimeSchedulePlanningDayDTO> plannedShifts = new List<TimeSchedulePlanningDayDTO>();

                    List<TimeScheduleTemplateBlock> existingShifts = GetScheduleBlocksForEmployeeWithTasks(null, iDTO.DestinationEmployeeId, iDTO.DestinationDate);
                    if (existingShifts.IsNullOrEmpty())
                    {
                        foreach (var staffingNeedsTaskDTO in iDTO.TaskDTOs)
                        {
                            TimeSchedulePlanningDayDTO newShift = CreateTimeSchedulePlanningDayDTO(iDTO.DestinationEmployeeId, iDTO.DestinationDate, staffingNeedsTaskDTO);
                            plannedShifts.Add(newShift);
                        }
                    }
                    else
                    {
                        //Try connected tasks to existing shifts. If no shift exists during a tasks start and stop then create a new shift
                        foreach (var staffingNeedsTaskDTO in iDTO.TaskDTOs)
                        {
                            DateTime taskStartTime = CalendarUtility.MergeDateAndTime(iDTO.DestinationDate, staffingNeedsTaskDTO.StartTime.Value.RemoveSeconds());
                            DateTime taskStopTime = taskStartTime.Add(staffingNeedsTaskDTO.StopTime.Value.RemoveSeconds() - staffingNeedsTaskDTO.StartTime.Value.RemoveSeconds());
                            TimeScheduleTemplateBlock overlappingShift = existingShifts.FirstOrDefault(x => x.ActualStartTime.Value <= taskStartTime && x.ActualStopTime.Value >= taskStopTime);

                            if (overlappingShift == null)
                            {
                                TimeSchedulePlanningDayDTO newShift = CreateTimeSchedulePlanningDayDTO(iDTO.DestinationEmployeeId, iDTO.DestinationDate, staffingNeedsTaskDTO);
                                plannedShifts.Add(newShift);
                            }
                        }
                    }

                    oDTO.EvaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(plannedShifts, iDTO.DestinationEmployeeId, false, null, iDTO.Rules);

                    #endregion

                }
                catch (Exception ex)
                {
                    oDTO.EvaluateWorkRulesResult.Result.Exception = ex;
                    oDTO.EvaluateWorkRulesResult.Result.IntegerValue = 0;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.EvaluateWorkRulesResult.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }

                #endregion
            }

            return oDTO;
        }

        private EvaluateDeviationsAgainstWorkRulesOutputDTO TaskEvaluateDeviationsAgainstWorkRulesAndSendXEMail()
        {
            var (iDTO, oDTO) = InitTask<EvaluateDeviationsAgainstWorkRulesInputDTO, EvaluateDeviationsAgainstWorkRulesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    oDTO.EvaluateDeviationsAgainstWorkRulesResult = EvaluateDeviationsAgainstWorkRulesAndSendXEMail(iDTO.EmployeeId, iDTO.Date);
                    return oDTO;

                }
                catch (Exception ex)
                {
                    oDTO.EvaluateDeviationsAgainstWorkRulesResult.Success = false;
                    oDTO.EvaluateDeviationsAgainstWorkRulesResult.ErrorMessage = ex.Message;
                    LogError(ex);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private IsDayAttestedOutputDTO TaskIsDayAttested()
        {
            var (iDTO, oDTO) = InitTask<IsDayAttestedInputDTO, IsDayAttestedOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    oDTO.IsDayAttestedResult = IsDayAttested(iDTO.EmployeeId, iDTO.Date);
                    return oDTO;

                }
                catch (Exception ex)
                {
                    //oDTO.IsDayAttestedResult.Success = false;
                    //oDTO.IsDayAttestedResult.ErrorMessage = ex.Message;
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

        #region WorkRule evaluation

        #region EmployeePost

        private List<EvaluateWorkRuleResultDTO> EvaluateEmployeePostPlannedShiftsAgainstWorkRules(List<TimeSchedulePlanningDayDTO> plannedShifts, int employeePostId, List<SoeScheduleWorkRules> rules = null, bool breakOnNotAllowedToOverride = true)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();
            plannedShifts.SetUniqueId();

            #region Prereq

            if (plannedShifts.IsNullOrEmpty())
            {
                evaluateResults.Add(new EvaluateWorkRuleResultDTO(true));
                return evaluateResults;
            }

            TimeSchedulePlanningDayDTO firstShift = plannedShifts.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO lastShift = plannedShifts.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (firstShift == null || lastShift == null)
                return evaluateResults;

            DateTime intervalToDate = CalendarUtility.GetLastDateOfWeek(lastShift.StartTime.Date);

            EmployeePost employeePost = GetEmployeePostWithScheduleCycleRuleTypeAndEmployeeGroup(employeePostId);
            if (employeePost == null || !employeePost.EmployeeGroupId.HasValue)
                return evaluateResults;

            EmployeeGroup employeeGroup = employeePost.EmployeeGroup;
            if (employeeGroup == null)
                return evaluateResults;

            if (rules.IsNullOrEmpty())
            {
                rules = new List<SoeScheduleWorkRules>()
                {
                    SoeScheduleWorkRules.OverlappingShifts,
                    SoeScheduleWorkRules.WorkTimeWeekMaxMin,
                    SoeScheduleWorkRules.RestDay,
                    SoeScheduleWorkRules.RestWeek,
                    SoeScheduleWorkRules.WorkTimeDay,
                    SoeScheduleWorkRules.ScheduleCycleRule,
                    SoeScheduleWorkRules.Breaks,
                };
            }

            #endregion

            #region Evaluate rules

            List<EvaluateWorkRuleResultDTO> ruleResults = new List<EvaluateWorkRuleResultDTO>();
            foreach (SoeScheduleWorkRules rule in rules)
            {
                ruleResults.Clear();
                switch (rule)
                {
                    case SoeScheduleWorkRules.OverlappingShifts:
                        ruleResults = EvaluateRuleOverlappingShifts(employeePost, plannedShifts);
                        break;
                    case SoeScheduleWorkRules.WorkTimeWeekMaxMin:
                        ruleResults = EvaluateRuleWorkTimeWeekMaxMin(employeePost, employeeGroup, plannedShifts);
                        break;
                    case SoeScheduleWorkRules.WorkTimeDay:
                        evaluateResults = EvaluateRuleWorkTimeDay(employeePost, employeeGroup, plannedShifts);
                        break;
                    case SoeScheduleWorkRules.RestDay:
                        ruleResults = EvaluateRuleRestTimeDay(employeePost, employeeGroup, plannedShifts);
                        break;
                    case SoeScheduleWorkRules.RestWeek:
                        ruleResults = EvaluateRuleRestTimeWeek(employeePost, employeeGroup, plannedShifts);
                        break;
                    case SoeScheduleWorkRules.ScheduleCycleRule:
                        ruleResults = EvaluateScheduleCycleRules(employeePost, intervalToDate);
                        break;
                    case SoeScheduleWorkRules.Breaks:
                        ruleResults = EvaluateRuleBreaks(employeePost, employeeGroup, plannedShifts);
                        break;
                    default:
                        break;
                }

                if (ruleResults.Any())
                {
                    foreach (var ruleResult in ruleResults)
                    {
                        ruleResult.EvaluatedWorkRule = rule;
                    }
                    evaluateResults.AddRange(ruleResults);

                    //No need to process other rules, because user is not allowed to override this rule if it fails
                    if (breakOnNotAllowedToOverride && ruleResults.Any(x => !x.Success) && rule == SoeScheduleWorkRules.OverlappingShifts)
                        break;
                }
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleOverlappingShifts(EmployeePost employeePost, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employeePost == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvalute = shiftsInput.Where(x => x.EmployeePostId.HasValue && x.EmployeePostId == employeePost.EmployeePostId).OrderBy(s => s.StartTime.Date).ToList();
            if (!shiftsToEvalute.Any())
                return evaluateResults;

            #endregion

            #region Evaluate planned shift against other planned shifts

            evaluateResults.AddRange(EvaluateRuleOverlappingShifts(employeePost.Name, shiftsToEvalute));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleRestTimeDay(EmployeePost employeePost, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            try
            {
                #region Prereq          

                if (employeePost == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                    return evaluateResults;

                if (employeeGroup.RuleRestTimeDay == 0)
                    return evaluateResults;

                //Create a local copy so that changes doesnt affect the input collection
                List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeePostId.HasValue && x.EmployeeId == employeePost.EmployeePostId).ToList();
                if (!shiftsToEvaluate.Any())
                    return evaluateResults;

                #endregion

                List<DateTime> plannedDays = shiftsToEvaluate.Select(x => x.ActualDate).Distinct().ToList();

                #region Decide which days to check

                var days = GetDaysForRestTimeDay(employeeGroup, plannedDays, null);

                #endregion

                #region Evaluate

                evaluateResults.AddRange(EvaluateRuleRestTimeDay(employeePost.Name, employeeGroup, days, shiftsToEvaluate, new List<TimeScheduleTemplateBlock>()));

                #endregion
            }
            finally
            {
                if (!evaluateResults.Any(x => !x.Success) && GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningRuleRestTimeDayMandatory))
                {
                    foreach (var evaluateResult in evaluateResults)
                    {
                        if (!evaluateResult.Success)
                            evaluateResult.IsRuleRestTimeDayMandatory = true;
                    }
                }
            }

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleRestTimeWeek(EmployeePost employeePost, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            try
            {
                #region Prereq

                if (employeePost == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                    return evaluateResults;

                if (employeeGroup.RuleRestTimeWeek == 0)
                    return evaluateResults;

                //Create a local copy so that changes doesnt affect the input collection
                List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeePostId.HasValue && x.EmployeePostId.Value == employeePost.EmployeePostId).ToList();
                if (!shiftsToEvaluate.Any())
                    return evaluateResults;

                #endregion

                List<DateTime> plannedDays = shiftsToEvaluate.Select(x => x.ActualDate).Distinct().ToList();

                #region Decide which weeks to check

                var weeks = GetWeeksForRestTimeWeek(employeeGroup, plannedDays, null);

                #endregion

                #region Evaluate

                evaluateResults.AddRange(EvaluateRuleRestTimeWeek(employeePost.Name, weeks, employeeGroup.RuleRestTimeWeek, shiftsToEvaluate, new List<TimeScheduleTemplateBlock>()));

                #endregion
            }
            finally
            {
                if (!evaluateResults.Any(x => !x.Success) && GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningRuleRestTimeWeekMandatory))
                {
                    foreach (var evaluateResult in evaluateResults)
                    {
                        if (!evaluateResult.Success)
                            evaluateResult.IsRuleRestTimeWeekMandatory = true;
                    }
                }
            }

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleBreaks(EmployeePost employeePost, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employeePost == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeePostId.HasValue && x.EmployeePostId.Value == employeePost.EmployeePostId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRuleBreaks(employeePost.Name, employeeGroup, shiftsToEvaluate, new List<TimeScheduleTemplateBlock>()));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleWorkTimeWeekMaxMin(EmployeePost employeePost, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employeePost == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvalute = shiftsInput.Where(x => x.EmployeePostId.HasValue && x.EmployeePostId.Value == employeePost.EmployeePostId).ToList();
            if (shiftsToEvalute.IsNullOrEmpty())
                return evaluateResults;

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRuleWorkTimeWeekMaxMin(employeePost.Name, null, employeeGroup, shiftsToEvalute, new List<TimeScheduleTemplateBlock>(), true));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleWorkTimeDay(EmployeePost employeePost, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employeePost == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeePostId.HasValue && x.EmployeePostId == employeePost.EmployeePostId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRuleWorkTimeDay(employeePost.Name, employeeGroup, shiftsToEvaluate, new List<TimeScheduleTemplateBlock>()));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateScheduleCycleRules(EmployeePost employeePost, DateTime date)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employeePost == null || employeePost.ScheduleCycle == null)
                return evaluateResults;

            var templateHead = GetEmployeePostTemplateHead(employeePost.EmployeePostId, date);
            if (templateHead == null)
                return evaluateResults;

            ScheduleCycleDTO scheduleCycle = employeePost.ScheduleCycle.ToDTO();

            #endregion

            List<TimeScheduleTemplateBlockDTO> cycleTemplateBlocks = GetTemplateScheduleBlocksForEmployeePostWithTemplatePeriods(null, templateHead, employeePost.EmployeePostId).ToDTOs();
            foreach (var rule in scheduleCycle.ScheduleCycleRuleDTOs)
            {
                if (rule.ScheduleCycleRuleTypeDTO == null)
                    continue;

                var ruleValidTemplateShifts = cycleTemplateBlocks.Where(x => !x.IsBreak && x.Date.HasValue && rule.Valid(x.Date.Value.DayOfWeek, x.StartTime, x.StopTime)).GroupBy(g => g.Date);
                if (ruleValidTemplateShifts.Count() < rule.MinOccurrences)
                {
                    String errormsg = String.Format(GetText(8772, "Minimum antal ({0}) för regel {1}, ej uppnådd för tjänst {2}."), rule.MinOccurrences, rule.ScheduleCycleRuleTypeDTO.Name, employeePost.Name);
                    evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.ScheduleCycleRuleMinOccurrencesNotReached, errormsg, employeePost.Name, date));
                }
                else if (ruleValidTemplateShifts.Count() > rule.MaxOccurrences)
                {
                    String errormsg = String.Format(GetText(8773, "Max antal ({0}) för regel {1}, har överskridits för  tjänst {2}."), rule.MaxOccurrences, rule.ScheduleCycleRuleTypeDTO.Name, employeePost.Name);
                    evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.ScheduleCycleRuleMaxOccurrencesViolated, errormsg, employeePost.Name, date));
                }
            }
            return evaluateResults;
        }

        #endregion

        #region Schedule/Template Schedule

        private ActionResult ValidateShiftAgainstGivenScenario(TimeSchedulePlanningDayDTO shift, int? timeScheduleScenarioHeadId)
        {
            return ValidateShiftsAgainstGivenScenario(new List<TimeSchedulePlanningDayDTO>() { shift }, timeScheduleScenarioHeadId);
        }

        private ActionResult ValidateShiftsAgainstGivenScenario(List<TimeSchedulePlanningDayDTO> shifts, int? timeScheduleScenarioHeadId)
        {
            if (shifts == null)
                return new ActionResult(true);

            bool inValid = false;
            if (timeScheduleScenarioHeadId.HasValue)
            {
                if (shifts.Any(x => !x.TimeScheduleScenarioHeadId.HasValue))
                    inValid = true;

                if (shifts.Any(x => x.TimeScheduleScenarioHeadId.HasValue && x.TimeScheduleScenarioHeadId.Value != timeScheduleScenarioHeadId.Value))
                    inValid = true;
            }
            else
            {
                if (shifts.Any(x => x.TimeScheduleScenarioHeadId.HasValue))
                    inValid = true;
            }

            if (inValid)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(9323, "Felaktiga inparametrar"));

            return new ActionResult(true);
        }

        private ActionResult ValidateShiftAgainstGivenScenario(TimeScheduleTemplateBlock shift, int? timeScheduleScenarioHeadId)
        {
            return ValidateShiftsAgainstGivenScenario(new List<TimeScheduleTemplateBlock>() { shift }, timeScheduleScenarioHeadId);
        }

        private ActionResult ValidateShiftsAgainstGivenScenario(List<TimeScheduleTemplateBlock> shifts, int? timeScheduleScenarioHeadId)
        {
            if (shifts == null)
                return new ActionResult(true);

            bool inValid = false;
            if (timeScheduleScenarioHeadId.HasValue)
            {
                if (shifts.Where(x => !x.IsBreak).Any(x => !x.TimeScheduleScenarioHeadId.HasValue))
                    inValid = true;

                if (shifts.Where(x => !x.IsBreak).Any(x => x.TimeScheduleScenarioHeadId.HasValue && x.TimeScheduleScenarioHeadId.Value != timeScheduleScenarioHeadId.Value))
                    inValid = true;
            }
            else
            {
                if (shifts.Where(x => !x.IsBreak).Any(x => x.TimeScheduleScenarioHeadId.HasValue))
                    inValid = true;
            }

            if (inValid)
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(9323, "Felaktiga inparametrar"));

            return new ActionResult(true);
        }

        private List<EvaluateWorkRuleResultDTO> EvaluatePlannedShiftsAgainstWorkRules(List<TimeSchedulePlanningDayDTO> plannedShifts, int employeeId, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules = null, List<DateTime> sourceDates = null, List<TimeScheduleTemplateBlock> scheduledBlocks = null, bool breakOnNotAllowedToOverride = true, List<SoeScheduleWorkRules> rulesToSkip = null, bool evaluateDeviationsOnPlannedDays = false, List<TimeSchedulePlanningDayDTO> adjacentShifts = null, bool all = false, bool evaluateScheduleWithTimeDeviationCauseSetting = false, bool useLeisureCodes = false, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null, List<TimeScheduleEmployeePeriodDetail> planningPeriodLeisureCodes = null, bool keepScheduledBlocks = false, bool useAnnualLeave = false)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();
            plannedShifts.SetUniqueId();

            #region Prereq

            ActionResult result = ValidateShiftsAgainstGivenScenario(plannedShifts, timeScheduleScenarioHeadId);
            if (result.Success)
                result = ValidateShiftsAgainstGivenScenario(scheduledBlocks, timeScheduleScenarioHeadId);

            if (!result.Success)
            {
                evaluateResults.Add(new EvaluateWorkRuleResultDTO(result.ErrorNumber, result.ErrorMessage));
                return evaluateResults;
            }

            if (plannedShifts.IsNullOrEmpty())
            {
                evaluateResults.Add(new EvaluateWorkRuleResultDTO(true));
                return evaluateResults;
            }

            foreach (TimeSchedulePlanningDayDTO plannedShift in plannedShifts.Where(i => i.IsDeleted))
            {
                plannedShift.StartTime = CalendarUtility.GetDateTime(plannedShift.StartTime.Date, CalendarUtility.DATETIME_DEFAULT);
                plannedShift.StopTime = plannedShift.StartTime;
            }

            if (!plannedShifts.Any(x => x.ActualDate.DayOfWeek == DayOfWeek.Saturday || x.ActualDate.DayOfWeek == DayOfWeek.Sunday))
            {
                if (rulesToSkip == null)
                    rulesToSkip = new List<SoeScheduleWorkRules>();

                rulesToSkip.Add(SoeScheduleWorkRules.ScheduleFreeWeekends);
            }

            TimeSchedulePlanningDayDTO first = plannedShifts.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = plannedShifts.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = CalendarUtility.GetFirstDateOfWeek(first.StartTime.Date);
            DateTime intervalToDate = CalendarUtility.GetLastDateOfWeek(last.StartTime.Date);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null && employeeId != Constants.NO_REPLACEMENT_EMPLOYEEID)
                return evaluateResults;

            EmployeeGroup employeeGroup = employee?.GetEmployeeGroup(intervalFromDate, intervalToDate, GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return evaluateResults;

            bool isOrderPlanning = plannedShifts.Any(x => x.IsBooking() || x.IsOrder());
            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            bool useAccountHierarchy = UseAccountHierarchy();

            #region LeisureCodes Prereq

            EmployeeGroupTimeLeisureCode firstEmployeeGroupTimeLeisureCodeVForPeriod = null;
            EmployeeGroupTimeLeisureCode firstEmployeeGroupTimeLeisureCodeXForPeriod = null;
            bool validateLeisureCodesWorkRules = false;
            List<TimeScheduleTemplateBlock> planningPeriodScheduledBlocks = new List<TimeScheduleTemplateBlock>();

            if (useLeisureCodes && planningPeriodStartDate.HasValue && planningPeriodStopDate.HasValue && planningPeriodStartDate <= planningPeriodStopDate)
            {
                #region Check settings

                List<EmployeeGroupTimeLeisureCode> leisureCodes = TimeScheduleManager.GetEmployeeGroupTimeLeisureCodes(actorCompanyId, null, true);
                if (employeeGroup != null && !leisureCodes.IsNullOrEmpty())
                {
                    List<EmployeeGroupTimeLeisureCode> employeeGroupLeisureCodes = leisureCodes.Where(x => x.EmployeeGroupId == employeeGroup.EmployeeGroupId || x.EmployeeGroupId == null).ToList();

                    // Should we create collection with all leisure codes for the planning period?
                    // For now, pick the first ones for this period
                    List<EmployeeGroupTimeLeisureCode> employeeGroupTimeLeisureCodesV = employeeGroupLeisureCodes.Where(x => x.DateFrom <= planningPeriodStartDate && x.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.V).ToList();
                    if (!employeeGroupTimeLeisureCodesV.IsNullOrEmpty())
                        firstEmployeeGroupTimeLeisureCodeVForPeriod = employeeGroupTimeLeisureCodesV.OrderBy(y => y.DateFrom).Last();

                    List<EmployeeGroupTimeLeisureCode> employeeGroupTimeLeisureCodesX = employeeGroupLeisureCodes.Where(x => x.DateFrom <= planningPeriodStartDate && x.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.X).ToList();
                    if (!employeeGroupTimeLeisureCodesX.IsNullOrEmpty())
                        firstEmployeeGroupTimeLeisureCodeXForPeriod = employeeGroupTimeLeisureCodesX.OrderBy(y => y.DateFrom).Last();

                    if (firstEmployeeGroupTimeLeisureCodeVForPeriod != null && firstEmployeeGroupTimeLeisureCodeXForPeriod != null)
                    {
                        if (planningPeriodLeisureCodes.IsNullOrEmpty())
                            planningPeriodLeisureCodes = TimeScheduleManager.GetTimeSchedulePlanningLeisureCodes(entities, planningPeriodStartDate.Value.AddDays(-7), planningPeriodStopDate.Value, new List<int> { employeeId });

                        validateLeisureCodesWorkRules = true;
                    }
                }

                #endregion

                #region Load scheduleBlocks for the planning period


                if (validateLeisureCodesWorkRules)
                {
                    // Maybe code can be optimized by setting scheduledBlocks below out from this collection, if intervalFromDate and intervalToDate (+/- extraDays) is within this collection
                    planningPeriodScheduledBlocks = GetScheduleBlocksForWorkRuleEvaluation(timeScheduleScenarioHeadId, employeeId, planningPeriodStartDate.Value.AddDays(-7), planningPeriodStopDate.Value.AddDays(1), isOrderPlanning);

                    if (!rules.IsNullOrEmpty())
                    {
                        rules.Remove(SoeScheduleWorkRules.RestWeek);
                        rules.Remove(SoeScheduleWorkRules.ScheduledDaysMaximum);
                        //rules.Remove(SoeScheduleWorkRules.MinorsRestWeek);
                        //rules.Remove(SoeScheduleWorkRules.CoherentSheduleTime);
                    }
                    else
                    {
                        if (rulesToSkip == null)
                            rulesToSkip = new List<SoeScheduleWorkRules>();
                        
                        rulesToSkip.Add(SoeScheduleWorkRules.RestWeek);
                        rulesToSkip.Add(SoeScheduleWorkRules.ScheduledDaysMaximum);
                        //rulesToSkip.Add(SoeScheduleWorkRules.MinorsRestWeek);
                        //rulesToSkip.Add(SoeScheduleWorkRules.CoherentSheduleTime);
                    }
                }

                #endregion
            }
            if (!validateLeisureCodesWorkRules && !rules.IsNullOrEmpty() && rules.Any(x => x == SoeScheduleWorkRules.LeisureCodes))
                rules.Remove(SoeScheduleWorkRules.LeisureCodes);

            #endregion

            #region AnnualLeave Prereq

            bool validateAnnualLeaveWorkRules = useAnnualLeave;
            bool isAddingAnnualLeaveDay = false;

            if (rules != null && rules.Count == 1 && rules.Contains(SoeScheduleWorkRules.AnnualLeave))
                isAddingAnnualLeaveDay = true;

            if (validateAnnualLeaveWorkRules && isAddingAnnualLeaveDay)
            {
                // Only validate one shift, the one currently being saved
                var annualLeaveShift = plannedShifts.FirstOrDefault();

                (DateTime, DateTime, int)? annualLeaveShiftTimes = AnnualLeaveManager.GetAnnualLeaveShiftTimes(annualLeaveShift.StartTime.Date, employeeId, actorCompanyId);
                if (!annualLeaveShiftTimes.HasValue)
                    validateAnnualLeaveWorkRules = false;
                else
                {
                    annualLeaveShift.StartTime = annualLeaveShiftTimes.Value.Item1;
                    annualLeaveShift.StopTime = annualLeaveShiftTimes.Value.Item2;
                }
            }
            else if (!validateAnnualLeaveWorkRules)
            {
                if (rules.IsNullOrEmpty())
                {
                    if (rulesToSkip == null)
                        rulesToSkip = new List<SoeScheduleWorkRules>();

                    rulesToSkip.Add(SoeScheduleWorkRules.AnnualLeave);
                }
                else if (rules != null && rules.Any(x => x == SoeScheduleWorkRules.AnnualLeave))
                    rules.Remove(SoeScheduleWorkRules.AnnualLeave);
            }
            
            #endregion

            //intervalFromDate is always monday
            int extraDaysBackward = 1;
            if ((employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek() == DayOfWeek.Monday && employeeGroup.GetRuleRestTimeWeekStartTime().TimeOfDay.TotalMinutes != 0) || employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek() != DayOfWeek.Monday)
                extraDaysBackward = employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek() == DayOfWeek.Sunday ? 1 : 8 - (int)employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek();

            //intervalToDate is always sunday
            int extraDaysForward = employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek() == DayOfWeek.Sunday ? 7 : (int)employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek();

            if (!isPersonalScheduleTemplate && scheduledBlocks == null)
                scheduledBlocks = GetScheduleBlocksForWorkRuleEvaluation(timeScheduleScenarioHeadId, employeeId, intervalFromDate.AddDays(-extraDaysBackward), intervalToDate.AddDays(extraDaysForward), isOrderPlanning);
            if (scheduledBlocks == null)
                scheduledBlocks = new List<TimeScheduleTemplateBlock>();

            if (isPersonalScheduleTemplate && adjacentShifts == null && plannedShifts.Any(x => x.EmployeeId == employeeId))
            {
                adjacentShifts = new List<TimeSchedulePlanningDayDTO>();
                TimeSchedulePlanningDayDTO firstShift = plannedShifts.Where(x => x.EmployeeId == employeeId).OrderBy(x => x.StartTime).FirstOrDefault();
                TimeSchedulePlanningDayDTO lastShift = plannedShifts.Where(x => x.EmployeeId == employeeId).OrderBy(x => x.StartTime).LastOrDefault();
                int? timeScheduleTemplateHeadId = plannedShifts.FirstOrDefault(x => x.TimeScheduleTemplateHeadId.HasValue && x.TimeScheduleTemplateHeadId.Value != 0)?.TimeScheduleTemplateHeadId;

                if (firstShift != null && timeScheduleTemplateHeadId.HasValue)
                {
                    DateTime from = firstShift.ActualDate.AddDays(-extraDaysBackward);
                    DateTime to = firstShift.ActualDate.AddDays(-1);
                    List<TimeSchedulePlanningDayDTO> shifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, timeScheduleTemplateHeadId.Value, from, to, loadTimeDeviationCause: false, loadStaffingNeeds: false, useNbrOfDaysFromTemplate: false);
                    adjacentShifts.AddRange(shifts.Where(x => x.ActualDate >= from && x.ActualDate <= to));
                }

                if (lastShift != null && timeScheduleTemplateHeadId.HasValue)
                {
                    DateTime from = lastShift.ActualDate.AddDays(1);
                    DateTime to = lastShift.ActualDate.AddDays(extraDaysForward);
                    List<TimeSchedulePlanningDayDTO> shifts = TimeScheduleManager.GetTimeSchedulePlanningTemplate(entities, actorCompanyId, 0, userId, 0, timeScheduleTemplateHeadId.Value, from, to, loadTimeDeviationCause: false, loadStaffingNeeds: false, useNbrOfDaysFromTemplate: false);
                    adjacentShifts.AddRange(shifts.Where(x => x.ActualDate >= from && x.ActualDate <= to));
                }
            }

            List<TimeScheduleTypeDTO> scheduleTypes = GetTimeScheduleTypesWithFactorFromCache().ToDTOs(true).ToList();

            List<TimeBlock> presenceTimeBlocks = null;
            if (!isPersonalScheduleTemplate && employeeGroup != null && !timeScheduleScenarioHeadId.HasValue && (employeeGroup.RuleRestDayIncludePresence || employeeGroup.RuleRestWeekIncludePresence || scheduleTypes.Any(x => x.IsBilagaJ)))
            {
                presenceTimeBlocks = GetTimeBlocksForWorkRuleEvaluation(employeeId, intervalFromDate.AddDays(-extraDaysBackward), intervalToDate.AddDays(extraDaysForward), employeeGroup, scheduledBlocks);
            }
            if (presenceTimeBlocks == null)
                presenceTimeBlocks = new List<TimeBlock>();

            if (employeeId != Constants.NO_REPLACEMENT_EMPLOYEEID)
            {
                if (rules.IsNullOrEmpty())
                {
                    rules = new List<SoeScheduleWorkRules>();

                    bool validateMinorsWorkrules = useWorkRulesForMinors && IsEmployeeYoungerThan18(employee);

                    foreach (SoeScheduleWorkRules rule in Enum.GetValues(typeof(SoeScheduleWorkRules)))
                    {
                        if (rule == SoeScheduleWorkRules.None)
                            continue;

                        if (rulesToSkip != null && rulesToSkip.Any() && rulesToSkip.Contains(rule))
                            continue;

                        if (rule == SoeScheduleWorkRules.ScheduleCycleRule)
                            continue;

                        if (rule == SoeScheduleWorkRules.HoursBeforeShiftRequest)
                            continue;

                        if (isPersonalScheduleTemplate && (rule == SoeScheduleWorkRules.AttestedDay || rule == SoeScheduleWorkRules.MinorsWorkAlone || rule == SoeScheduleWorkRules.HoursBeforeAssignShift))
                            continue;

                        if (timeScheduleScenarioHeadId != null && rule == SoeScheduleWorkRules.HoursBeforeAssignShift)
                            continue;

                        if (!isPersonalScheduleTemplate && rule == SoeScheduleWorkRules.ScheduleFreeWeekends)
                            continue;

                        // Remove these after validation instead if they overlap
                        if (validateMinorsWorkrules && (
                            //rule == SoeScheduleWorkRules.RestDay ||
                            //rule == SoeScheduleWorkRules.RestWeek ||
                            //rule == SoeScheduleWorkRules.WorkTimeWeekMaxMin ||
                            //rule == SoeScheduleWorkRules.WorkTimeWeekPartTimeWorkers ||
                            //rule == SoeScheduleWorkRules.WorkTimeDay ||
                            //rule == SoeScheduleWorkRules.Breaks ||
                            rule == SoeScheduleWorkRules.CoherentSheduleTime))
                            continue;

                        if ((!validateMinorsWorkrules || isOrderPlanning) && (
                            rule == SoeScheduleWorkRules.MinorsWorkTimeWeek ||
                            rule == SoeScheduleWorkRules.MinorsWorkTimeDay ||
                            rule == SoeScheduleWorkRules.MinorsWorkTimeSchoolDay ||
                            rule == SoeScheduleWorkRules.MinorsWorkingHours ||
                            rule == SoeScheduleWorkRules.MinorsRestWeek ||
                            rule == SoeScheduleWorkRules.MinorsRestDay ||
                            rule == SoeScheduleWorkRules.MinorsBreaks ||
                            rule == SoeScheduleWorkRules.MinorsSummerHoliday))
                            continue;

                        if (!validateLeisureCodesWorkRules && 
                            rule == SoeScheduleWorkRules.LeisureCodes)
                            continue;

                        if (!validateAnnualLeaveWorkRules &&
                            rule == SoeScheduleWorkRules.AnnualLeave)
                            continue;

                        rules.Add(rule);
                    }
                }
            }
            else
            {
                rules = new List<SoeScheduleWorkRules>()
                {
                    SoeScheduleWorkRules.MinorsWorkAlone,
                    SoeScheduleWorkRules.MinorsHandlingMoney,
                };
            }

            List<int> currentHierarchyAccountIds = null;
            List<Employee> currentHierarchyEmployees = null;
            if (useWorkRulesForMinors && (rules.Contains(SoeScheduleWorkRules.MinorsWorkAlone) || rules.Contains(SoeScheduleWorkRules.MinorsHandlingMoney)))
            {
                currentHierarchyAccountIds = GetAccountHierarchySettingAccountFromCache(useAccountHierarchy, intervalFromDate, intervalToDate);
                currentHierarchyEmployees = GetEmployeesForCompanyWithEmployment(currentHierarchyAccountIds, intervalFromDate).RemoveEmployeesWithoutEmployment(intervalFromDate);
            }

            #region Load TimeDeviationCauses

            List<TimeDeviationCause> timeDeviationCauses = null;
            if (evaluateScheduleWithTimeDeviationCauseSetting)
            {
                timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(actorCompanyId);
            }

            #endregion

            #endregion

            #region Evaluate rules

            List<EvaluateWorkRuleResultDTO> ruleResults = new List<EvaluateWorkRuleResultDTO>();
            foreach (SoeScheduleWorkRules rule in rules)
            {
                if (rule == SoeScheduleWorkRules.None)
                    continue;

                ruleResults.Clear();
                switch (rule)
                {
                    case SoeScheduleWorkRules.OverlappingShifts:
                        ruleResults = EvaluateRuleOverlappingShifts(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), isPersonalScheduleTemplate);
                        break;
                    case SoeScheduleWorkRules.AttestedDay:
                        ruleResults = EvaluateRuleAttestedDay(employee, employeeGroup, plannedShifts, timeScheduleScenarioHeadId);
                        break;
                    case SoeScheduleWorkRules.WorkTimeWeekMaxMin:
                        ruleResults = EvaluateRuleWorkTimeWeekMaxMin(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), isPersonalScheduleTemplate);
                        break;
                    case SoeScheduleWorkRules.WorkTimeDay:
                        ruleResults = EvaluateRuleWorkTimeDay(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList());
                        break;
                    case SoeScheduleWorkRules.RestDay:
                        ruleResults = EvaluateRuleRestTimeDay(employee, employeeGroup, plannedShifts, adjacentShifts, scheduledBlocks.Where(b => b.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && b.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), presenceTimeBlocks, isPersonalScheduleTemplate, evaluateDeviationsOnPlannedDays, timeDeviationCauses);
                        break;
                    case SoeScheduleWorkRules.RestWeek:
                        ruleResults = EvaluateRuleRestTimeWeek(employee, employeeGroup, plannedShifts, adjacentShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), presenceTimeBlocks, isPersonalScheduleTemplate, evaluateDeviationsOnPlannedDays, timeDeviationCauses);
                        break;
                    case SoeScheduleWorkRules.Breaks:
                        ruleResults = EvaluateRuleBreaks(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList());
                        break;
                    case SoeScheduleWorkRules.ScheduleFreeWeekends:
                        ruleResults = EvaluateScheduleFreeWeekends(employee, employeeGroup, plannedShifts, isPersonalScheduleTemplate);
                        break;
                    case SoeScheduleWorkRules.ScheduledDaysMaximum:
                        ruleResults = EvaluateScheduledDaysMaximum(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), intervalFromDate, intervalToDate);
                        break;
                    case SoeScheduleWorkRules.CoherentSheduleTime:
                        ruleResults = EvaluateCoherentScheduleTime(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), scheduleTypes, evaluateDeviationsOnPlannedDays, presenceTimeBlocks);
                        break;
                    case SoeScheduleWorkRules.MinorsWorkingHours:
                        ruleResults = EvaluateRuleMinorsWorkingHours(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(b => b.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && b.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList());
                        break;
                    case SoeScheduleWorkRules.MinorsWorkTimeDay:
                        ruleResults = EvaluateRuleMinorsWorkTimeDay(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList());
                        break;
                    case SoeScheduleWorkRules.MinorsWorkTimeSchoolDay:
                        ruleResults = EvaluateRuleMinorsWorkTimeSchoolDay(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList());
                        break;
                    case SoeScheduleWorkRules.MinorsWorkTimeWeek:
                        ruleResults = EvaluateRuleMinorsWorkTimeWeek(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), timeDeviationCauses);
                        break;
                    case SoeScheduleWorkRules.MinorsRestDay:
                        ruleResults = EvaluateRuleMinorsRestTimeDay(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(b => b.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && b.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), timeDeviationCauses);
                        break;
                    case SoeScheduleWorkRules.MinorsRestWeek:
                        ruleResults = EvaluateRuleMinorsRestTimeWeek(employee, employeeGroup, plannedShifts, CalendarUtility.GetFirstDateOfWeek(intervalFromDate), CalendarUtility.GetLastDateOfWeek(intervalToDate), timeScheduleScenarioHeadId, timeDeviationCauses);
                        break;
                    case SoeScheduleWorkRules.MinorsBreaks:
                        ruleResults = EvaluateRuleMinorsBreaks(employee, employeeGroup, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList());
                        break;
                    case SoeScheduleWorkRules.MinorsSummerHoliday:
                        ruleResults = EvaluateRuleMinorsSummerHoliday(employee, employeeGroup, plannedShifts, timeScheduleScenarioHeadId);
                        break;
                    case SoeScheduleWorkRules.MinorsWorkAlone:
                        ruleResults = EvaluateRuleMinorsWorkAlone(employee, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), currentHierarchyEmployees, currentHierarchyAccountIds, timeScheduleScenarioHeadId, sourceDates, isPersonalScheduleTemplate, all);
                        break;
                    case SoeScheduleWorkRules.MinorsHandlingMoney:
                        ruleResults = EvaluateRuleMinorsHandlingMoney(employee, plannedShifts, scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), currentHierarchyEmployees, currentHierarchyAccountIds, timeScheduleScenarioHeadId, sourceDates);
                        break;
                    case SoeScheduleWorkRules.HoursBeforeAssignShift:
                        ruleResults = EvaluateHoursBeforeAssignShift(employee, employeeGroup, plannedShifts);
                        break;
                    case SoeScheduleWorkRules.LeisureCodes:
                        ruleResults = EvaluateLeisureCodes(employee, plannedShifts, planningPeriodScheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList(), planningPeriodLeisureCodes, planningPeriodStartDate, planningPeriodStopDate, firstEmployeeGroupTimeLeisureCodeVForPeriod, firstEmployeeGroupTimeLeisureCodeXForPeriod, timeDeviationCauses, keepScheduledBlocks);
                        break;
                    case SoeScheduleWorkRules.AnnualLeave:
                        ruleResults = EvaluateAnnualLeave(employee, plannedShifts, scheduledBlocks, isAddingAnnualLeaveDay);
                        break;
                    default:
                        break;
                }
                if (ruleResults.Any())
                {
                    foreach (var ruleResult in ruleResults)
                    {
                        ruleResult.EmployeeId = employeeId;
                        ruleResult.EvaluatedWorkRule = rule;
                    }

                    evaluateResults.AddRange(ruleResults);
                }

                //No need to process other rules, because user is not allowed to override this rule if it fails
                if (breakOnNotAllowedToOverride && ruleResults.Any(x => !x.Success) && (rule == SoeScheduleWorkRules.AttestedDay || rule == SoeScheduleWorkRules.OverlappingShifts))
                    break;
            }

            #endregion
            RemoveWorkRuleResultsSupersededByMinor(evaluateResults);

            return evaluateResults;
        }

        private EvaluateDeviationsAgainstWorkRules EvaluateDeviationsAgainstWorkRulesAndSendXEMail(int employeeId, DateTime date)
        {
            EvaluateDeviationsAgainstWorkRules evaluateDeviationsAgainstWorkRules = new EvaluateDeviationsAgainstWorkRules();
            EvaluateWorkRulesActionResult evaluateWorkRulesResult = new EvaluateWorkRulesActionResult();

            try
            {
                evaluateDeviationsAgainstWorkRules.EvaluateWorkRulesResult = evaluateWorkRulesResult;
                evaluateDeviationsAgainstWorkRules.Success = true;

                #region Prereq

                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
                if (employee == null)
                {
                    evaluateWorkRulesResult.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));
                    return evaluateDeviationsAgainstWorkRules;
                }

                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache());
                if (employeeGroup == null)
                {
                    evaluateWorkRulesResult.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8539, "Tidavtal hittades inte"));
                    return evaluateDeviationsAgainstWorkRules;
                }

                List<TimeScheduleTypeDTO> scheduleTypes = GetTimeScheduleTypesWithFactorFromCache().ToDTOs(true).ToList();
                List<SoeScheduleWorkRules> rules = new List<SoeScheduleWorkRules>();

                if (employeeGroup.RuleRestWeekIncludePresence)
                    rules.Add(SoeScheduleWorkRules.RestWeek);
                if (employeeGroup.RuleRestDayIncludePresence)
                    rules.Add(SoeScheduleWorkRules.RestDay);
                if (scheduleTypes.Any(x => x.IsBilagaJ))
                    rules.Add(SoeScheduleWorkRules.CoherentSheduleTime);

                if (!rules.Any())
                    return evaluateDeviationsAgainstWorkRules;

                #endregion

                #region Perform

                List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksForWorkRuleEvaluation(null, employeeId, date, date, false);

                evaluateWorkRulesResult.EvaluatedRuleResults = EvaluatePlannedShiftsAgainstWorkRules(scheduleBlocks.ToTimeSchedulePlanningDayDTOs(), employeeId, false, null, rules: rules, evaluateDeviationsOnPlannedDays: true);

                if (!evaluateWorkRulesResult.Result.Success)
                {
                    evaluateDeviationsAgainstWorkRules.Success = false;
                    evaluateDeviationsAgainstWorkRules.EvaluateRulesFailed = true;
                    evaluateDeviationsAgainstWorkRules.ErrorMessage = GetText(10254, "Fel vid exekvering av arbetstidsregler");
                    return evaluateDeviationsAgainstWorkRules;
                }

                if (evaluateWorkRulesResult.AllRulesSucceded)
                    return evaluateDeviationsAgainstWorkRules;

                if (evaluateWorkRulesResult.Result.Success && !evaluateWorkRulesResult.AllRulesSucceded)
                {
                    bool sendXeMailNeeded = false;
                    bool sendXeMailSucceded = false;
                    string employeeInfoMessage = "";
                    HandleEvaluateDeviationsAgainstWorkRulesResult(employeeId, date, evaluateWorkRulesResult, ref sendXeMailNeeded, ref sendXeMailSucceded, ref employeeInfoMessage);

                    evaluateDeviationsAgainstWorkRules.SendXeMailNeeded = sendXeMailNeeded;
                    evaluateDeviationsAgainstWorkRules.SendXeMailSucceded = sendXeMailSucceded;
                    evaluateDeviationsAgainstWorkRules.InfoMessage = employeeInfoMessage;
                }

                #endregion
            }
            catch (Exception ex)
            {
                evaluateDeviationsAgainstWorkRules.Success = false;
                evaluateDeviationsAgainstWorkRules.ErrorMessage = ex.Message;
                LogError(ex);
            }

            return evaluateDeviationsAgainstWorkRules;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleAttestedDay(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, int? timeScheduleScenarioHeadId)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || timeScheduleScenarioHeadId.HasValue)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId).OrderBy(s => s.StartTime.Date).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            #endregion

            #region Evaluate

            foreach (var shiftsGroupedByDate in shiftsToEvaluate.GroupBy(x => x.ActualDate))
            {
                #region Result

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, shiftsGroupedByDate.Key);
                if (timeBlockDate != null)
                {
                    if (timeBlockDate.IsLocked)
                    {
                        String errormsg = String.Format(GetText(8290, "Otillåten ändring för {0}"), employee.Name);
                        errormsg += "\n" + String.Format(GetText(8925, "Dagen ({0}) är låst."), shiftsGroupedByDate.Key.ToShortDateString());
                        evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleDayIsLocked, errormsg, employee.Name, shiftsGroupedByDate.Key));
                    }
                    else
                    {
                        AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                        if (attestStateInitial != null)
                        {
                            List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsConnectedToTimeBlock(employee.EmployeeId, timeBlockDate.TimeBlockDateId);
                            if (timePayrollTransactions.Any(x => x.AttestStateId != attestStateInitial.AttestStateId))
                            {
                                String errormsg = String.Format(GetText(8290, "Otillåten ändring för {0}"), employee.Name);
                                errormsg += "\n" + String.Format(GetText(8291, "Dagen ({0}) innehåller attesterade tider."), shiftsGroupedByDate.Key.ToShortDateString());
                                evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleContainsAttestedTransactions, errormsg, employee.Name, shiftsGroupedByDate.Key));
                            }
                        }
                    }
                }

                #endregion                
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleOverlappingShifts(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, bool isPersonalScheduleTemplate)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId).OrderBy(s => s.StartTime.Date).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            bool ruleViolation = false;

            #endregion

            #region Evaluate planned shift against already scheduled shifts

            if (!isPersonalScheduleTemplate)
            {
                foreach (var shiftToEvalute in shiftsToEvaluate)
                {
                    // No overlapping test for on duty shift
                    if (shiftToEvalute.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty)
                        continue;

                    DateTime intervalFromDate = shiftToEvalute.StartTime.Date.AddDays(-1);
                    DateTime intervalToDate = shiftToEvalute.StartTime.Date.AddDays(1);

                    //isOrderPlanning should be called as false, see evaluate section in this method
                    List<TimeScheduleTemplateBlock> scheduledBlocksForDate = scheduledBlocksInput.Where(i => i.EmployeeId == employee.EmployeeId && (i.Date.HasValue && i.Date.Value >= intervalFromDate.Date && i.Date.Value <= intervalToDate.Date) && !i.IsBreak).ToList();

                    #region Remove shiftsToEvalute from scheduleBlocks (Unsaved changes have precedence)

                    RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocksForDate, false);

                    #endregion

                    #region Evaluate

                    foreach (var scheduledBlock in scheduledBlocksForDate)
                    {
                        bool skipCheck = false;
                        switch (shiftToEvalute.Type)
                        {
                            case TermGroup_TimeScheduleTemplateBlockType.Schedule:
                                skipCheck = scheduledBlock.IsOrder() || scheduledBlock.IsBooking() || scheduledBlock.IsOnDuty();
                                break;
                            case TermGroup_TimeScheduleTemplateBlockType.Order:
                                skipCheck = scheduledBlock.IsSchedule() || scheduledBlock.IsOnDuty();
                                break;
                            case TermGroup_TimeScheduleTemplateBlockType.Booking:
                                skipCheck = scheduledBlock.IsSchedule() || scheduledBlock.IsOnDuty();
                                break;
                            case TermGroup_TimeScheduleTemplateBlockType.Standby:
                                // TODO: skipCheck = scheduledBlock.IsSchedule();
                                break;
                            case TermGroup_TimeScheduleTemplateBlockType.OnDuty:
                                //skipCheck = scheduledBlock.IsSchedule();
                                break;
                        }

                        if (skipCheck)
                            continue;

                        if (shiftToEvalute.StartTime == shiftToEvalute.StopTime)
                            continue;

                        //planned shift starttime conflicts with scheduled block
                        if (scheduledBlock.ActualStartTime.Value <= shiftToEvalute.StartTime && shiftToEvalute.StartTime < scheduledBlock.ActualStopTime.Value)
                            ruleViolation = true;

                        //planned shift stoptime conflicts with scheduled block
                        if (scheduledBlock.ActualStartTime.Value < shiftToEvalute.StopTime && shiftToEvalute.StopTime <= scheduledBlock.ActualStopTime.Value)
                            ruleViolation = true;

                        //scheduled block is overlapped by planned shift 
                        if (CalendarUtility.IsCurrentOverlappedByNew(shiftToEvalute.StartTime, shiftToEvalute.StopTime, scheduledBlock.ActualStartTime.Value, scheduledBlock.ActualStopTime.Value))
                            ruleViolation = true;

                        //planned shift is overlapped by scheduled block
                        if (CalendarUtility.IsNewOverlappedByCurrent(shiftToEvalute.StartTime, shiftToEvalute.StopTime, scheduledBlock.ActualStartTime.Value, scheduledBlock.ActualStopTime.Value))
                            ruleViolation = true;

                        #region Result

                        if (ruleViolation)
                        {
                            bool isEvaluateOrder = shiftToEvalute.Order != null;
                            bool isTargetOrder = scheduledBlock.CustomerInvoiceId.GetValueOrDefault() > 0;
                            string invoiceNr = string.Empty;
                            if (isTargetOrder)
                            {
                                invoiceNr = $" [{InvoiceManager.GetInvoiceNr(scheduledBlock.CustomerInvoiceId.Value)}]";
                            }

                            String errormsg = String.Format(GetText(8285, "{0} {1} {2}-{3} krockar med {4} {5} {6}-{7} för {8}."),
                                StringUtility.CamelCaseWord(GetText(isEvaluateOrder ? 485 : 481, (int)TermGroup.TimeSchedulePlanning)),
                                shiftToEvalute.StartTime.Date.ToShortDateString(),
                                shiftToEvalute.StartTime.ToShortTimeString(),
                                shiftToEvalute.StopTime.ToShortTimeString(),
                                GetText(isTargetOrder ? 485 : 481, (int)TermGroup.TimeSchedulePlanning) + invoiceNr,
                                scheduledBlock.ActualStartTime.Value.ToShortDateString(),
                                scheduledBlock.ActualStartTime.Value.ToShortTimeString(),
                                scheduledBlock.ActualStopTime.Value.ToShortTimeString(),
                                employee.Name);

                            evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleShiftsOverlap, errormsg, employee.Name, shiftToEvalute.ActualDate));
                        }
                        ruleViolation = false;

                        #endregion
                    }

                    #endregion
                }
            }
            #endregion

            #region Evaluate planned shift against other planned shifts

            evaluateResults.AddRange(EvaluateRuleOverlappingShifts(employee.Name, shiftsToEvaluate));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleRestTimeDay(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeSchedulePlanningDayDTO> adjacentShiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeBlock> presenceTimeBlocksInput, bool isPersonalScheduleTemplate, bool evaluateDeviationsOnPlannedDays, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            try
            {
                #region Prereq          

                if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                    return evaluateResults;

                //Dont evaluate for hiddenemployee
                if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                    return evaluateResults;

                if (employeeGroup.RuleRestTimeDay == 0)
                    return evaluateResults;

                //Create a local copy so that changes doesnt affect the input collection
                List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                if (adjacentShiftsInput != null)
                    shiftsToEvaluate.AddRange(adjacentShiftsInput.Where(x => x.EmployeeId == employee.EmployeeId));

                if (!shiftsToEvaluate.Any())
                    return evaluateResults;

                List<TimeBlock> presenceTimeBlocks = null;
                List<DateTime> plannedDays = shiftsToEvaluate.Select(x => x.ActualDate).Distinct().ToList();
                if (employeeGroup.RuleRestDayIncludePresence)
                {

                    if (evaluateDeviationsOnPlannedDays)
                        presenceTimeBlocks = presenceTimeBlocksInput.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                    else
                        presenceTimeBlocks = presenceTimeBlocksInput.Where(x => x.EmployeeId == employee.EmployeeId && x.TimeBlockDate != null && !plannedDays.Contains(x.TimeBlockDate.Date)).ToList();
                }

                #endregion

                #region Decide which days to check

                var days = GetDaysForRestTimeDay(employeeGroup, plannedDays, employee);

                #endregion


                #region Evaluate

                evaluateResults.AddRange(EvaluateRuleRestTimeDay(employee.Name, employeeGroup, days, shiftsToEvaluate, scheduledBlocksInput, presenceTimeBlocks: presenceTimeBlocks, timeDeviationCauses: timeDeviationCauses));

                #endregion
            }
            finally
            {
                if (!evaluateResults.Any(x => !x.Success) && isPersonalScheduleTemplate && GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningRuleRestTimeDayMandatory))
                {
                    foreach (var evaluateResult in evaluateResults)
                    {
                        if (!evaluateResult.Success)
                            evaluateResult.IsRuleRestTimeDayMandatory = true;
                    }
                }
            }

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleRestTimeWeek(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeSchedulePlanningDayDTO> adjacentShiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeBlock> presenceTimeBlocksInput, bool isPersonalScheduleTemplate, bool evaluateDeviationsOnPlannedDays, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            try
            {
                #region Prereq

                if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                    return evaluateResults;

                //Dont evaluate for hiddenemployee
                if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                    return evaluateResults;

                if (employeeGroup.RuleRestTimeWeek == 0)
                    return evaluateResults;

                //Create a local copy so that changes doesnt affect the input collection
                List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                if (adjacentShiftsInput != null)
                    shiftsToEvaluate.AddRange(adjacentShiftsInput.Where(x => x.EmployeeId == employee.EmployeeId));

                if (!shiftsToEvaluate.Any())
                    return evaluateResults;

                List<DateTime> plannedDays = shiftsToEvaluate.Select(x => x.ActualDate).Distinct().ToList();
                List<TimeBlock> presenceTimeBlocks = null;
                if (employeeGroup.RuleRestWeekIncludePresence)
                {
                    if (evaluateDeviationsOnPlannedDays)
                        presenceTimeBlocks = presenceTimeBlocksInput.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                    else
                        presenceTimeBlocks = presenceTimeBlocksInput.Where(x => x.EmployeeId == employee.EmployeeId && x.TimeBlockDate != null && !plannedDays.Contains(x.TimeBlockDate.Date)).ToList();
                }

                #endregion

                #region Decide which weeks to check

                var weeks = GetWeeksForRestTimeWeek(employeeGroup, plannedDays, employee);

                #endregion

                #region Evaluate

                evaluateResults.AddRange(EvaluateRuleRestTimeWeek(employee.Name, weeks, employeeGroup.RuleRestTimeWeek, shiftsToEvaluate, scheduledBlocksInput, presenceTimeBlocks: presenceTimeBlocks, timeDeviationCauses: timeDeviationCauses));

                #endregion
            }
            finally
            {
                if (evaluateResults.Any(x => !x.Success) && isPersonalScheduleTemplate && GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningRuleRestTimeWeekMandatory))
                {
                    foreach (var evaluateResult in evaluateResults)
                    {
                        if (!evaluateResult.Success)
                            evaluateResult.IsRuleRestTimeWeekMandatory = true;
                    }
                }
            }

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleBreaks(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRuleBreaks(employee.Name, employeeGroup, shiftsToEvaluate, scheduledBlocks));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateCoherentScheduleTime(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeScheduleTypeDTO> scheduleTypes, bool evaluateDeviationsOnPlannedDays, List<TimeBlock> presenceTimeBlocksInput = null)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();
            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            #endregion

            DateTime intervalFromDate = shiftsToEvaluate.GetStartDate().AddDays(-2);
            DateTime intervalToDate = shiftsToEvaluate.GetStopDate().AddDays(2);

            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => x.Date.HasValue && x.EmployeeId == employee.EmployeeId && x.Date.Value >= intervalFromDate && x.Date.Value <= intervalToDate).ToList();

            List<TimeBlock> presenceTimeBlocks = null;
            List<DateTime> plannedDays = shiftsToEvaluate.Select(x => x.ActualDate).Distinct().ToList();
            if (evaluateDeviationsOnPlannedDays)
                presenceTimeBlocks = presenceTimeBlocksInput.Where(x => x.EmployeeId == employee.EmployeeId && x.TimeBlockDate != null && x.TimeBlockDate.Date >= intervalFromDate && x.TimeBlockDate.Date <= intervalToDate).ToList();
            else
                presenceTimeBlocks = presenceTimeBlocksInput.Where(x => x.EmployeeId == employee.EmployeeId && x.TimeBlockDate != null && x.TimeBlockDate.Date >= intervalFromDate && x.TimeBlockDate.Date <= intervalToDate && !plannedDays.Contains(x.TimeBlockDate.Date)).ToList();


            #region Evaluate

            evaluateResults.AddRange(EvaluateCoherentScheduleTime(employee.Name, shiftsToEvaluate, scheduledBlocks, scheduleTypes, presenceTimeBlocks));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateCoherentScheduleTime(string name, List<TimeSchedulePlanningDayDTO> shiftsToEvaluate, List<TimeScheduleTemplateBlock> scheduledBlocks, List<TimeScheduleTypeDTO> scheduleTypes, List<TimeBlock> presenceTimeBlocksInput = null)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (!scheduleTypes.Any(x => x.IsBilagaJ))
                return evaluateResults;

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocksInput (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion


            #region Evaluate

            List<WorkIntervalDTO> plannedShiftsWorkIntervals = shiftsToEvaluate.GetCoherentWorkIntervals(scheduleTypes).OrderBy(x => x.StartTime).ToList();
            List<WorkIntervalDTO> scheduledShiftsWorkIntervals = scheduledBlocks.ToTimeSchedulePlanningDayDTOs().GetCoherentWorkIntervals(scheduleTypes).OrderBy(x => x.StartTime).ToList();
            List<WorkIntervalDTO> presenceIntervals = presenceTimeBlocksInput.IsNullOrEmpty() ? new List<WorkIntervalDTO>() : presenceTimeBlocksInput.ToDTOs(false, false).ToList().GetCoherentWorkIntervals();
            List<WorkIntervalDTO> shiftIntervals = CalendarUtility.MergeIntervals(plannedShiftsWorkIntervals, scheduledShiftsWorkIntervals);
            List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(shiftIntervals, presenceIntervals, true);


            int maxCoherentLength = 20 * 60;
            int loopCount = workIntervals.Count - 1;

            for (int i = 0; i <= loopCount; i++)
            {
                WorkIntervalDTO workInterval = workIntervals[i];
                if (workInterval.TotalMinutes > maxCoherentLength)
                {
                    #region Result

                    string errormsg = string.Format(GetText(8948, "Sammanhängande schematid {0} {1}-{2} för {3} ({4}) överstiger 20 timmar."), workInterval.StartTime.ToShortDateString(), workInterval.StartTime.ToShortTimeString(), workInterval.StopTime.ToShortTimeString(), name, CalendarUtility.FormatMinutes(workInterval.TotalMinutes));
                    evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleCoherentSheduleTimeViolated, errormsg, name, workInterval.StartTime.Date));

                    #endregion

                }
                else if (workInterval.TotalMinutes <= maxCoherentLength && workInterval.HasBilagaJ && i < loopCount)
                {
                    WorkIntervalDTO nextInterval = workIntervals[i + 1];
                    int restToNextShift = (int)(nextInterval.StartTime - workInterval.StopTime).TotalMinutes;
                    if (workInterval.TotalMinutes > restToNextShift)
                    {

                        #region Result

                        string errormsg = string.Format(GetText(8949, "(Bilaga J) Sammanhängande schematid {0} {1}-{2} för {3} ({4}) måste efterföljas av minst lika lång vila. Nuvarande vila: {5}"), workInterval.StartTime.ToShortDateString(), workInterval.StartTime.ToShortTimeString(), workInterval.StopTime.ToShortTimeString(), name, CalendarUtility.FormatMinutes(workInterval.TotalMinutes), CalendarUtility.FormatMinutes(restToNextShift));
                        evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleCoherentSheduleTimeViolated, errormsg, name, workInterval.StartTime.Date));

                        #endregion
                    }
                }

            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateScheduledDaysMaximum(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, DateTime dateFrom, DateTime dateTo)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //For now, 
            if (employeeGroup.RuleScheduledDaysMaximumWeek == 0)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateScheduledDaysMaximum(employee.Name, employeeGroup, shiftsToEvaluate, scheduledBlocksInput, dateFrom, dateTo));

            #endregion
            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateScheduledDaysMaximum(string name, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsToEvaluate, List<TimeScheduleTemplateBlock> scheduledBlocksInput, DateTime dateFrom, DateTime dateTo)
        {
            #region Remove shiftsToEvalute from scheduleBlocksForWeek (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocksInput, true);

            #endregion

            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            if (name == null || employeeGroup == null || shiftsToEvaluate.IsNullOrEmpty())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            #region Calculate

            var weekOut = new Dictionary<int, int>();
            var weeks = CalendarUtility.GetWeeks(dateFrom, dateTo);

            shiftsToEvaluate = shiftsToEvaluate.Where(x => !x.IsZeroShift()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();
            shifts.AddRange(shiftsToEvaluate);
            shifts.AddRange(scheduledBlocks.ToTimeSchedulePlanningDayDTOs());

            foreach (var week in weeks)
            {
                int days = shifts.Where(w => w.ActualDate >= week.WeekStart && w.ActualDate <= week.WeekStop).Select(s => s.ActualDate).Distinct().Count();
                if (days > employeeGroup.RuleScheduledDaysMaximumWeek)
                {
                    int weekNr = CalendarUtility.GetWeekNr(week.WeekStart);
                    weekOut[weekNr] = days;
                }
            }

            if (weeks.Count == 0)
                return evaluateResults;

            #endregion

            #region Result

            foreach (var week in weekOut)
            {
                string errormsg = string.Format(GetText(9359, "Max schemalagda dagar per vecka uppnått för {2} på vecka {3}. Får inte schemalägga mer än {0} dagar per vecka ({1})."), employeeGroup.RuleScheduledDaysMaximumWeek, week.Value, name, week.Key);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleScheduledDaysMaximumWeek, errormsg, name);
                evaluateResults.Add(evaluateResult);
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateScheduleFreeWeekends(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, bool isPersonalScheduleTemplate)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //For now, 
            if (employeeGroup.RuleScheduleFreeWeekendsMinimumYear == 0 || !isPersonalScheduleTemplate)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            #endregion

            int? templateHeadId = shiftsToEvaluate.FirstOrDefault()?.TimeScheduleTemplateHeadId;
            if (!templateHeadId.HasValue)
                return evaluateResults;

            #region Calculate

            shiftsToEvaluate = shiftsToEvaluate.Where(x => !x.IsZeroShift()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            int scheduleFreeWeekendsInCycle = 0;
            List<int> weeksWithSchedule = new List<int>();
            foreach (var shiftsInWeek in shiftsToEvaluate.GroupBy(x => x.WeekNr))
            {
                weeksWithSchedule.Add(shiftsInWeek.Key);
                List<DateTime> workingDaysInWeekend = shiftsInWeek.Where(x => x.ActualDate.DayOfWeek == DayOfWeek.Saturday || x.ActualDate.DayOfWeek == DayOfWeek.Sunday).Select(x => x.ActualDate).Distinct().ToList();
                if (!workingDaysInWeekend.Any())
                    scheduleFreeWeekendsInCycle++;
            }

            //Only weeks that contains any shift (non zero shifts) will exist in weeksWithSchedule
            if (weeksWithSchedule.Count < shiftsToEvaluate[0].NbrOfWeeks)
                scheduleFreeWeekendsInCycle += shiftsToEvaluate[0].NbrOfWeeks - weeksWithSchedule.Count;

            int scheduleFreeWeekends = (52 / shiftsToEvaluate[0].NbrOfWeeks) * scheduleFreeWeekendsInCycle;

            #endregion

            #region Result

            if (scheduleFreeWeekends < employeeGroup.RuleScheduleFreeWeekendsMinimumYear)
            {
                string errormsg = string.Format(GetText(9357, "Minimum lediga helger per kalenderår ({0}), ej uppnådd för {1}. Anställd har endast {2} lediga helger."), employeeGroup.RuleScheduleFreeWeekendsMinimumYear, employee.Name, scheduleFreeWeekends);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleScheduleFreeWeekendsMinimumYear, errormsg, employee.Name);
                evaluateResults.Add(evaluateResult);
            }
            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleWorkTimeWeekMaxMin(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, bool isPersonalScheduleTemplate)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvalute = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvalute.Any())
                return evaluateResults;

            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            Employment employment = employee.GetEmployment(shiftsToEvalute.OrderBy(s => s.StartTime.Date).FirstOrDefault()?.StartTime.Date);

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRuleWorkTimeWeekMaxMin(employee.Name, employment, employeeGroup, shiftsToEvalute, scheduledBlocks, isPersonalScheduleTemplate));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleWorkTimeDay(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRuleWorkTimeDay(employee.Name, employeeGroup, shiftsToEvaluate, scheduledBlocks));

            #endregion

            return evaluateResults;
        }

        private void HandleEvaluateDeviationsAgainstWorkRulesResult(int employeeId, DateTime date, EvaluateWorkRulesActionResult evaluateWorkRulesResult, ref bool sendXeMailNeeded, ref bool sendXeMailSucceded, ref string employeeInfoMessage)
        {
            try
            {
                #region Prereq

                if (evaluateWorkRulesResult == null)
                    return;

                Employee employee = GetEmployeeWithContactPersonFromCache(employeeId);
                if (employee == null)
                    return;

                User currentUser = GetUserFromCache();
                if (currentUser == null)
                    return;

                #endregion

                bool workRulesViolated = false;
                bool workRulesFailed = false;
                if (evaluateWorkRulesResult.Result.Success)
                {
                    if (evaluateWorkRulesResult.AllRulesSucceded)
                    {
                        #region Success

                        // All rules succeeded

                        #endregion
                    }
                    else
                    {
                        #region Warning

                        // Work rules violated, check if they can be overridden
                        if (evaluateWorkRulesResult.CanUserOverrideRuleViolation)
                            workRulesViolated = true;
                        else
                            workRulesFailed = true;

                        #endregion
                    }
                }
                else if (evaluateWorkRulesResult.EvaluatedRuleResults != null && evaluateWorkRulesResult.EvaluatedRuleResults.Count > 0)
                {
                    #region Failure

                    // Work rules violated
                    workRulesFailed = true;

                    #endregion
                }

                if ((workRulesViolated || workRulesFailed) && evaluateWorkRulesResult.EvaluatedRuleResults.Any(r => !r.Success))
                {
                    sendXeMailNeeded = true;

                    #region Create subject

                    string subject = GetText(10251, "Autosvar gällande validering av arbetstidsregler vid rapportering av närvaro.");

                    #endregion

                    #region Create body

                    StringBuilder body = new StringBuilder();
                    body.Append(String.Format(GetText(10252, "Anställd {0} har rapporterat närvaroavvikelser {1}."), employee.Name, date.ToShortDateString()));

                    body.Append("{0}{0}" + String.Format("{0}", GetText(10253, "Observera följande varningar eller brott mot arbetstidsreglerna:")));
                    foreach (EvaluateWorkRuleResultDTO res in evaluateWorkRulesResult.EvaluatedRuleResults.Where(r => !r.Success))
                    {
                        body.Append("{0}{0}" + res.ErrorMessage);
                    }

                    #endregion

                    #region Send to receivers

                    List<UserDTO> receivers = UserManager.GetEmployeeNearestExecutives(entities, employee, date, date, actorCompanyId);
                    foreach (UserDTO receiver in receivers)
                    {
                        if (SendXEMail(receiver, currentUser, base.RoleId, subject, String.Format(body.ToString(), "<br/>"), String.Format(body.ToString(), "\n"), SoeEntityType.Employee, TermGroup_MessageType.AutomaticInformation, 0, receiver.EmailCopy).Success)
                            sendXeMailSucceded = true;
                    }

                    #endregion

                    employeeInfoMessage = GetText(10256, "Vid validering av arbetstidsreglerna har det upptäckts varningar. Du har fått ett meddelanden med mera information.");

                    #region Send XEMail to employee

                    #region Create subject

                    string subjectEmployee = GetText(10251, "Autosvar gällande validering av arbetstidsregler vid rapportering av närvaro.");

                    #endregion

                    #region Create body

                    StringBuilder bodyEmployee = new StringBuilder();
                    bodyEmployee.Append(String.Format(GetText(10261, "Validering av arbetstidsregler efter ändring av närvarotid {0}."), date.ToShortDateString()));

                    bodyEmployee.Append("{0}{0}" + String.Format("{0}", GetText(10253, "Observera följande varningar eller brott mot arbetstidsreglerna:")));
                    foreach (EvaluateWorkRuleResultDTO res in evaluateWorkRulesResult.EvaluatedRuleResults.Where(r => !r.Success))
                    {
                        bodyEmployee.Append("{0}{0}" + res.ErrorMessage);
                    }

                    #endregion

                    #region Send

                    SendXEMail(currentUser, currentUser, base.RoleId, subjectEmployee, String.Format(bodyEmployee.ToString(), "<br/>"), String.Format(bodyEmployee.ToString(), "\n"), SoeEntityType.Employee, TermGroup_MessageType.AutomaticInformation, 0, currentUser.EmailCopy);

                    #endregion

                    #endregion
                }

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private bool IsDayAttested(int employeeId, DateTime date)
        {
            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, date);
            if (timeBlockDate != null)
            {
                AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                if (attestStateInitial != null)
                {
                    List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsConnectedToTimeBlock(employeeId, timeBlockDate.TimeBlockDateId);
                    if (timePayrollTransactions.Any(x => x.AttestStateId != attestStateInitial.AttestStateId))
                        return true;
                }
            }

            return false;
        }

        #region Minors

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsWorkingHours(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate16To18limits = IsAgeBetween16To18(employee);
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate16To18limits && !validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, false);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Evaluate

            DateTime currentDayDate = intervalFromDate;
            while (currentDayDate <= intervalToDate)
            {
                // Skip if not minor this day
                if (!IsEmployeeYoungerThan18(employee, currentDayDate))
                {
                    currentDayDate = currentDayDate.AddDays(1);
                    continue;
                }

                bool validate16To18limitsToday = IsAgeBetween16To18(employee, currentDayDate);
                bool validate13To15limitsToday= IsAgeBetween13To15(employee, currentDayDate);
                bool validateYoungerThan13limitsToday = IsAgeYoungerThan13(employee, currentDayDate);

                #region Get Current day data

                List<TimeSchedulePlanningDayDTO> currentDayPlannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDayDate).OrderBy(b => b.StartTime).ToList();
                List<TimeScheduleTemplateBlock> currentDayScheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDayDate).OrderBy(b => b.StartTime).ToList();

                //Garanties that it always exist atleast one shift on the specified day
                if (!currentDayPlannedShifts.Any() && !currentDayScheduledShifts.Any())
                {
                    //There is no need to continue evaluating
                    currentDayDate = currentDayDate.AddDays(1);
                    continue;
                }

                var firstPlannedShiftCurrentDay = currentDayPlannedShifts.FirstOrDefault();
                var firstScheduledShiftCurrentDay = currentDayScheduledShifts.FirstOrDefault();

                var lastPlannedShiftCurrentDay = currentDayPlannedShifts.LastOrDefault();
                var lastScheduledhiftCurrentDay = currentDayScheduledShifts.LastOrDefault();

                #region Decide when current day starts

                DateTime currentDayStarts;
                if (firstPlannedShiftCurrentDay == null && firstScheduledShiftCurrentDay?.ActualStartTime != null)
                {
                    currentDayStarts = firstScheduledShiftCurrentDay.ActualStartTime.Value;
                }
                else if (firstScheduledShiftCurrentDay == null && firstPlannedShiftCurrentDay != null)
                {
                    currentDayStarts = firstPlannedShiftCurrentDay.StartTime;
                }
                else
                {
                    if (firstPlannedShiftCurrentDay != null && firstPlannedShiftCurrentDay.StartTime < firstScheduledShiftCurrentDay.ActualStartTime.Value)
                        currentDayStarts = firstPlannedShiftCurrentDay.StartTime;
                    else if (firstScheduledShiftCurrentDay?.ActualStartTime != null)
                        currentDayStarts = firstScheduledShiftCurrentDay.ActualStartTime.Value;
                    else
                        currentDayStarts = CalendarUtility.DATETIME_DEFAULT;
                }

                #endregion

                #region Decide when current day ends

                DateTime currentDayEnds;
                if (lastPlannedShiftCurrentDay == null && lastScheduledhiftCurrentDay?.ActualStopTime != null)
                {
                    currentDayEnds = lastScheduledhiftCurrentDay.ActualStopTime.Value;
                }
                else if (lastScheduledhiftCurrentDay == null && lastPlannedShiftCurrentDay != null)
                {
                    currentDayEnds = lastPlannedShiftCurrentDay.StopTime;
                }
                else
                {
                    if (lastPlannedShiftCurrentDay != null && lastPlannedShiftCurrentDay.StopTime > lastScheduledhiftCurrentDay.ActualStopTime.Value)
                        currentDayEnds = lastPlannedShiftCurrentDay.StopTime;
                    else if (lastScheduledhiftCurrentDay != null)
                        currentDayEnds = lastScheduledhiftCurrentDay.ActualStopTime.Value;
                    else
                        currentDayEnds = CalendarUtility.DATETIME_DEFAULT;
                }

                #endregion

                if ((currentDayStarts - currentDayEnds).TotalMinutes == 0)
                {
                    //Zero day, no need to continue evaluating
                    currentDayDate = currentDayDate.AddDays(1);
                    continue;
                }

                #endregion

                #region Evaluate working hour limits

                bool valid = true;

                if (validate16To18limitsToday)
                {
                    //Krav:Arbetstiden ska vara mellan klockan 06.00–22.00 eller 07.00–23.00. 
                    TimeSpan ts0600 = new TimeSpan(6, 0, 0);
                    TimeSpan ts0700 = new TimeSpan(7, 0, 0);
                    TimeSpan ts2200 = new TimeSpan(22, 0, 0);
                    TimeSpan ts2300 = new TimeSpan(23, 0, 0);

                    if (currentDayStarts.TimeOfDay < ts0600)
                        valid = false;
                    else if (currentDayEnds.TimeOfDay > ts2300)
                        valid = false;
                    else if (currentDayStarts.TimeOfDay < ts0700 && currentDayEnds.TimeOfDay > ts2200)
                        valid = false;
                }
                else if (validate13To15limitsToday || validateYoungerThan13limitsToday)
                {
                    //Krav: Du får inte jobba mellan 20.00–06.00.

                    TimeSpan ts0600 = new TimeSpan(6, 0, 0);
                    TimeSpan ts2000 = new TimeSpan(20, 0, 0);

                    if (currentDayStarts.TimeOfDay >= ts2000 || currentDayStarts.TimeOfDay < ts0600)
                        valid = false;
                    else if (currentDayEnds.TimeOfDay > ts2000 || currentDayEnds.TimeOfDay <= ts0600)
                        valid = false;
                }

                #region Result

                if (!valid)
                {
                    string errormsg = string.Format(GetText(8724, "Tillåtna arbetstider för anställda under 18 år ej uppfylld för {0}. Arbetsdag: {1} {2}-{3}."), employee.Name, currentDayDate.ToShortDateString(), currentDayStarts.ToShortTimeString(), currentDayEnds.ToShortTimeString());
                    var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsWorkingHoursViolated, errormsg, employee.Name, currentDayDate)
                    {
                        IsRuleForMinors = true,
                        CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                    };
                    evaluateResults.Add(evaluateResult);
                }

                #endregion

                #endregion

                currentDayDate = currentDayDate.AddDays(1);
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsWorkTimeDay(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate16To18limits = IsAgeBetween16To18(employee);
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate16To18limits && !validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            List<TimeScheduleTypeDTO> scheduleTypes = GetTimeScheduleTypesWithFactorFromCache().ToDTOs(true).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduleBlocksForWeek (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            #region Evaluate

            DateTime currentDayDate = intervalFromDate;
            while (currentDayDate <= intervalToDate)
            {
                // Skip day if adult
                if (!IsEmployeeYoungerThan18(employee, currentDayDate))
                {
                    currentDayDate = currentDayDate.AddDays(1);
                    continue;
                }

                bool validate16To18limitsToday = IsAgeBetween16To18(employee, currentDayDate);
                bool validate13To15limitsToday = IsAgeBetween13To15(employee, currentDayDate);
                bool validateYoungerThan13limitsToday = IsAgeYoungerThan13(employee, currentDayDate);

                int limitWorkTimeDay = 0;
                bool isWeekend = currentDayDate.DayOfWeek == DayOfWeek.Saturday || currentDayDate.DayOfWeek == DayOfWeek.Sunday;
                bool isSchoolWeek = IsSchoolWeek(currentDayDate);

                if (validate16To18limitsToday)
                {
                    limitWorkTimeDay = 8 * 60;
                }
                else if (validate13To15limitsToday)
                {
                    if (isWeekend)
                        limitWorkTimeDay = 7 * 60;
                    else if (isSchoolWeek)
                        limitWorkTimeDay = 2 * 60;
                    else
                        limitWorkTimeDay = 7 * 60;//schoolholiday
                }
                else if (validateYoungerThan13limitsToday && (!isSchoolWeek || isWeekend))
                {
                    limitWorkTimeDay = 7 * 60;
                }

                //Get Current day data                
                List<TimeSchedulePlanningDayDTO> currentDayPlannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDayDate).OrderBy(b => b.StartTime).ToList();
                List<TimeScheduleTemplateBlock> currentDayScheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDayDate).OrderBy(b => b.StartTime).ToList();
                int plannedTime = currentDayPlannedShifts.GetWorkMinutes(scheduleTypes);
                int scheduledTime = currentDayScheduledShifts.GetWorkMinutes(scheduleTypes);
                int totalWorkTime = scheduledTime + plannedTime;

                #region Result

                if (totalWorkTime > limitWorkTimeDay)
                {
                    string errormsg = string.Format(GetText(8726, "Max arbetstid per dag ({0}), {1}, kommer att överskridas för {2}."), CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(limitWorkTimeDay), false, false, false), currentDayDate.ToShortDateString(), employee.Name);
                    var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsWorkTimeDayReached, errormsg, employee.Name, currentDayDate)
                    {
                        IsRuleForMinors = true,
                        CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors)
                    };
                    evaluateResults.Add(evaluateResult);
                }

                #endregion

                currentDayDate = currentDayDate.AddDays(1);
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsWorkTimeSchoolDay(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
            if (employee.EmployeeId == hiddenEmployeeId)
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            GetMinorsSchoolDayMinutes(out int minorsSchoolDayStartMinutes, out int minorsSchoolDayStopMinutes);

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            bool isSchoolWeek = IsSchoolWeek(intervalFromDate);
            if (!isSchoolWeek)
                return evaluateResults;

            #endregion

            #region Remove shiftsToEvalute from scheduleBlocksForWeek (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            #region Evaluate

            DateTime currentDate = intervalFromDate;
            while (currentDate <= intervalToDate)
            {
                bool validate13To15limitsToday = IsAgeBetween13To15(employee, currentDate);
                bool validateYoungerThan13limitsToday = IsAgeYoungerThan13(employee, currentDate);

                if (validate13To15limitsToday || validateYoungerThan13limitsToday)
                {
                    if (IsSchoolDay(currentDate))
                    {
                        DateTime schoolDayStart = currentDate.AddMinutes(minorsSchoolDayStartMinutes);
                        DateTime schoolDayStop = currentDate.AddMinutes(minorsSchoolDayStopMinutes);
                        List<TimeSchedulePlanningDayDTO> plannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDate).OrderBy(b => b.StartTime).ToList();
                        List<TimeScheduleTemplateBlock> scheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDate).OrderBy(b => b.StartTime).ToList();

                        List<WorkIntervalDTO> plannedShiftsWorkIntervals = plannedShifts.GetWorkIntervals();
                        List<WorkIntervalDTO> scheduledShiftsWorkIntervals = scheduledShifts.GetWorkIntervals(hiddenEmployeeId);
                        List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(plannedShiftsWorkIntervals, scheduledShiftsWorkIntervals);

                        foreach (WorkIntervalDTO workInterval in workIntervals)
                        {
                            if (CalendarUtility.IsDatesOverlapping(workInterval.StartTime, workInterval.StopTime, schoolDayStart, schoolDayStop))
                            {
                                #region Result

                                string errormsg = string.Format(GetText(8729, "Arbetstid för anställda under 16 år kommer överskridas för {0}. Får inte jobba under skoltid {1}-{2}"), employee.Name, schoolDayStart.ToString("HH:mm"), schoolDayStop.ToString("HH:mm"));
                                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsBreaksViolated, errormsg, employee.Name, currentDate)
                                {
                                    IsRuleForMinors = true,
                                    CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                                };
                                evaluateResults.Add(evaluateResult);

                                #endregion
                            }
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsWorkTimeWeek(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate16To18limits = IsAgeBetween16To18(employee);
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate16To18limits && !validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            DateTime intervalFromDate = shiftsToEvaluate.GetStartDate();
            DateTime intervalToDate = shiftsToEvaluate.GetStopDate();

            List<TimeScheduleTypeDTO> scheduleTypes = GetTimeScheduleTypesWithFactorFromCache().ToDTOs(true).ToList();
            Employment employment = employee.GetEmployment();
            int employeeGroupWorkTimeWeek = employment != null ? employment.GetWorkTimeWeek(intervalFromDate) : employeeGroup.RuleWorkTimeWeek;

            #endregion

            #region Remove shiftsToEvalute from scheduleBlocksForWeek (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            #region Remove scheduledBlocks with deviation cause and setting

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                scheduledBlocks = scheduledBlocks.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
                shiftsToEvaluate = shiftsToEvaluate.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
            }

            #endregion

            #region Evaluate

            DateTime currentDate = intervalFromDate;
            while (currentDate <= intervalToDate)
            {
                DateTime firstDayOfWeek = CalendarUtility.GetBeginningOfDay(CalendarUtility.GetFirstDateOfWeek(currentDate));
                DateTime lastDayOfWeek = CalendarUtility.GetEndOfDay(CalendarUtility.GetLastDateOfWeek(currentDate));

                bool isAdultThisWeek = (!IsEmployeeYoungerThan18(employee, firstDayOfWeek));
                if (isAdultThisWeek) // Early skip week if adult this week
                {
                    currentDate = lastDayOfWeek.AddDays(1); //force the loop to change week 
                    continue;
                }


                List<TimeSchedulePlanningDayDTO> plannedShiftsForWeek = shiftsToEvaluate.Where(s => s.StartTime.Date >= firstDayOfWeek && s.StartTime.Date <= lastDayOfWeek).ToList();
                List<TimeScheduleTemplateBlock> scheduleBlocksForWeek = scheduledBlocks.Where(s => s.Date.HasValue && s.Date.Value.Date >= firstDayOfWeek && s.Date.Value.Date <= lastDayOfWeek).ToList();

                int plannedWeekTime = plannedShiftsForWeek.GetWorkMinutes(scheduleTypes);
                int scheduledWeekTime = scheduleBlocksForWeek.GetWorkMinutes(scheduleTypes);
                int totalWorkTimeWeek = plannedWeekTime + scheduledWeekTime;

                
                bool validate16To18limitsThisWeek = IsAgeBetween16To18(employee, firstDayOfWeek);
                bool validate13To15limitsThisWeek = IsAgeBetween13To15(employee, firstDayOfWeek);
                bool validateYoungerThan13limitsThisWeek = IsAgeYoungerThan13(employee, firstDayOfWeek);
                bool isSchoolWeek = IsSchoolWeek(firstDayOfWeek);

                int limitWorkTimeWeek = 0;
                bool limitWorkTimeWeekOriginIsAccourdingToWorkingEnvLaw = true;
                if (validate16To18limitsThisWeek)
                {
                    limitWorkTimeWeek = 40 * 60;
                }
                else if (validate13To15limitsThisWeek)
                {
                    if (isSchoolWeek)
                        limitWorkTimeWeek = 12 * 60;
                    else
                        limitWorkTimeWeek = 35 * 60;//schoolholiday
                }
                else if (validateYoungerThan13limitsThisWeek && !isSchoolWeek)
                {
                    limitWorkTimeWeek = 35 * 60;
                }

                if (employeeGroupWorkTimeWeek < limitWorkTimeWeek)
                {
                    limitWorkTimeWeek = employeeGroupWorkTimeWeek;
                    limitWorkTimeWeekOriginIsAccourdingToWorkingEnvLaw = false;
                }

                #region Result

                if (totalWorkTimeWeek > limitWorkTimeWeek)
                {
                    string accourdingToText = limitWorkTimeWeekOriginIsAccourdingToWorkingEnvLaw ? GetText(8755, "arbetsmiljölagen") : GetText(8756, "kontrakterad veckoarbetstid");
                    string errormsg = string.Format(GetText(8757, "Max antal arbetstimmar per vecka ({0}, enligt {1}), {2} - {3}, kommer att överskridas för {4}."),
                        CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(limitWorkTimeWeek), false, false, false),
                        accourdingToText,
                        firstDayOfWeek.ToShortDateString(),
                        lastDayOfWeek.ToShortDateString(),
                        employee.Name);

                    var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsWorkTimeWeekReached, errormsg, employee.Name, currentDate)
                    {
                        WorkTimeReachedDateFrom = firstDayOfWeek,
                        WorkTimeReachedDateTo = lastDayOfWeek,
                        IsRuleForMinors = true,
                        CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                    };
                    evaluateResults.Add(evaluateResult);
                }

                #endregion

                currentDate = lastDayOfWeek.AddDays(1); //force the loop to change week 
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsBreaks(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
            if (employee.EmployeeId == hiddenEmployeeId)
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate16To18limits = IsAgeBetween16To18(employee);
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate16To18limits && !validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            int RULE_DELIMETER_MAXLENGTHMINUTES = Convert.ToInt32(new TimeSpan(4, 30, 0).TotalMinutes);
            int RULE_DELIMETER_MINBREAKLENGHT = Convert.ToInt32(new TimeSpan(0, 30, 00).TotalMinutes);

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocksInput (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Evaluate

            DateTime currentDate = intervalFromDate;
            while (currentDate <= intervalToDate)
            {
                bool validate16To18limitsToday = IsAgeBetween16To18(employee, currentDate);
                bool validate13To15limitsToday = IsAgeBetween13To15(employee, currentDate);
                bool validateYoungerThan13limitsToday = IsAgeYoungerThan13(employee, currentDate);

                if (validate16To18limitsToday || validate13To15limitsToday  || validateYoungerThan13limitsToday)
                {

                    List<TimeSchedulePlanningDayDTO> plannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDate).OrderBy(b => b.StartTime).ToList();
                    List<TimeScheduleTemplateBlock> scheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDate).OrderBy(b => b.StartTime).ToList();

                    List<WorkIntervalDTO> plannedShiftsWorkIntervals = plannedShifts.GetWorkIntervals();
                    List<WorkIntervalDTO> scheduledShiftsWorkIntervals = scheduledShifts.GetWorkIntervals(hiddenEmployeeId);
                    List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(plannedShiftsWorkIntervals, scheduledShiftsWorkIntervals);
                    int workAmount = Convert.ToInt32(workIntervals.Sum(s => s.TotalMinutes));

                    if (workAmount >= RULE_DELIMETER_MAXLENGTHMINUTES)
                    {
                        bool breakOk = false;
                        int loopCount = workIntervals.Count - 1;
                        if (loopCount > 0)
                        {
                            for (int i = 0; i < loopCount; i++)
                            {
                                WorkIntervalDTO workInterval = workIntervals[i];
                                WorkIntervalDTO nextInterval = workIntervals[i + 1];
                                int restToNextShift = (int)(nextInterval.StartTime - workInterval.StopTime).TotalMinutes;
                                if (restToNextShift >= RULE_DELIMETER_MINBREAKLENGHT)
                                {
                                    breakOk = true;
                                    break;
                                }
                            }
                        }
                        if (!breakOk)
                        {
                            #region Result

                            string errormsg = string.Format(GetText(8728, "Raster och vila för anställda under 18 år ej uppfylld för {0}. Får inte jobba mer än 4,5 timmar utan 30 min rast"), employee.Name);
                            var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsBreaksViolated, errormsg, employee.Name, currentDate)
                            {
                                IsRuleForMinors = true,
                                CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                            };
                            evaluateResults.Add(evaluateResult);

                            #endregion
                        }
                    }
                }
                currentDate = currentDate.AddDays(1); //force the loop to change week 
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsSummerHoliday(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, int? timeScheduleScenarioHeadId)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            int RULE_DELIMETER_MINWEEKS = 4;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            List<SchoolHoliday> schoolHolidays = GetSchoolHolidaysInInterval(intervalFromDate, intervalToDate, true);
            if (schoolHolidays.IsNullOrEmpty())
                return evaluateResults;

            //Can only have one summer holiday per calendar year
            SchoolHoliday summerSchoolHoliday = schoolHolidays.FirstOrDefault();
            if (summerSchoolHoliday == null)
                return evaluateResults;

            intervalFromDate = CalendarUtility.GetBeginningOfDay(summerSchoolHoliday.DateFrom);
            intervalToDate = CalendarUtility.GetEndOfDay(summerSchoolHoliday.DateTo);

            // Don't check rule if employee already turned 16 when holiday begins,
            bool validate13To15BeginningHoliday = IsAgeBetween13To15(employee, intervalFromDate);
            bool validateYoungerThan13BeginningHoliday = IsAgeYoungerThan13(employee, intervalFromDate);
            if (!validate13To15BeginningHoliday && !validateYoungerThan13BeginningHoliday)
                return evaluateResults;

            List<TimeScheduleTemplateBlock> scheduledBlocks = GetScheduleBlocksForWorkRuleEvaluation(timeScheduleScenarioHeadId, employee.EmployeeId, intervalFromDate, intervalToDate, false).Where(x => !x.IsStandby()).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

            foreach (var item in shiftsToEvaluate)
            {
                if (item.TimeScheduleTemplateBlockId == 0)
                    continue;

                var scheduledBlock = scheduledBlocks.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == item.TimeScheduleTemplateBlockId);
                if (scheduledBlock != null)
                    scheduledBlocks.Remove(scheduledBlock);
            }

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Evaluate

            //Krav: Du måste ha 4 veckors sammanhängande ledighet under sommarlovet

            bool valid = false;
            List<DateTime> noneScheduleDays = new List<DateTime>();
            DateTime currentDate = intervalFromDate;


            while (currentDate <= intervalToDate && !valid)
            {
                List<TimeSchedulePlanningDayDTO> currentDayPlannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDate).OrderBy(b => b.StartTime).ToList();
                List<TimeScheduleTemplateBlock> currentDayScheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDate).OrderBy(b => b.StartTime).ToList();

                if (!currentDayPlannedShifts.Any() && !currentDayScheduledShifts.Any(i => i.StopTime > i.StartTime))
                {
                    DateTime? lastDate = noneScheduleDays.Any() ? noneScheduleDays.Last() : (DateTime?)null;
                    if (lastDate.HasValue && lastDate.Value.AddDays(1) != currentDate)
                        noneScheduleDays.Clear();

                    noneScheduleDays.Add(currentDate);

                    double weeks = (noneScheduleDays.Last() - noneScheduleDays.First()).TotalDays / 7;
                    if (weeks >= RULE_DELIMETER_MINWEEKS)
                        valid = true;
                }

                currentDate = currentDate.AddDays(1);
            }

            #region Result

            if (!valid)
            {
                string errormsg = string.Format(GetText(8725, "Ledighet för anställda under 16 år ej uppfylld för {0}. Ska ha minst fyra veckors sammanhängande ledighet under sommarlov"), employee.Name);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleSummerHolidayViolated, errormsg, employee.Name, currentDate)
                {
                    IsRuleForMinors = true,
                    CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors)
                };
                evaluateResults.Add(evaluateResult);
            }

            #endregion

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsRestTimeDay(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq          

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate16To18limits = IsAgeBetween16To18(employee);
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate16To18limits && !validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> allShifts = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();
            if (!allShifts.Any())
            {
                return evaluateResults;
            }

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            #endregion

            #region Remove scheduledBlocks with deviation cause and setting

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                scheduledBlocks = scheduledBlocks.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
                allShifts = allShifts.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
            }

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them
            
            allShifts = allShifts.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)
            RemovePlannedShiftsFromScheduledBlocks(allShifts, scheduledBlocks, false);

            #endregion

            #region Set shifts per age group
            List<TimeSchedulePlanningDayDTO> shiftsYoungerThan13 = allShifts.Where(x => IsAgeYoungerThan13(employee, x.ActualDate)).ToList();
            List<TimeSchedulePlanningDayDTO> shifts13To15 = allShifts.Where(x => IsAgeBetween13To15(employee, x.ActualDate)).ToList();
            List<TimeSchedulePlanningDayDTO> shifts16To18 = allShifts.Where(x => IsAgeBetween16To18(employee, x.ActualDate)).ToList();

            if (!shiftsYoungerThan13.Any() && !shifts13To15.Any() && !shifts16To18.Any())
                return evaluateResults;

            #endregion

            #region Decide limit

            int limitRestTimeDay16To18 = employeeGroup.RuleRestTimeDay > 12 * 60? employeeGroup.RuleRestTimeDay: 12 * 60;
            int limitRestTimeDay13To15 = employeeGroup.RuleRestTimeDay > 14 * 60? employeeGroup.RuleRestTimeDay: 14 * 60;
            int limitRestTimeDayUnder13 = employeeGroup.RuleRestTimeDay > 14 * 60? employeeGroup.RuleRestTimeDay: 14 * 60;

            #endregion

            List<DateTime> plannedDays16To18 = shifts16To18.Select(x => x.ActualDate).Distinct().ToList();
            List<DateTime> plannedDays13To15 = shifts13To15.Select(x => x.ActualDate).Distinct().ToList();
            List<DateTime> plannedDaysUnder13 = shiftsYoungerThan13.Select(x => x.ActualDate).Distinct().ToList();


            #region Decide which weeks to check

            var days16To18 = GetDaysForRestTimeDay(employeeGroup, plannedDays16To18, employee);
            var days13To15 = GetDaysForRestTimeDay(employeeGroup, plannedDays13To15, employee);
            var daysUnder13 = GetDaysForRestTimeDay(employeeGroup, plannedDaysUnder13, employee);

            #endregion

            #region Evaluate

            if (days16To18.Any()) evaluateResults.AddRange(EvaluateRestTimeDay(employee.Name, days16To18, shifts16To18, scheduledBlocks, limitRestTimeDay16To18, true));
            if (days13To15.Any()) evaluateResults.AddRange(EvaluateRestTimeDay(employee.Name, days13To15, shifts13To15, scheduledBlocks, limitRestTimeDay13To15, true));
            if (daysUnder13.Any()) evaluateResults.AddRange(EvaluateRestTimeDay(employee.Name, daysUnder13, shiftsYoungerThan13, scheduledBlocks, limitRestTimeDayUnder13, true));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsRestTimeWeek(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput, DateTime dateFrom, DateTime dateTo, int? timeScheduleScenarioHeadId, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;


            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;
            bool validate16To18limits = IsAgeBetween16To18(employee);
            bool validate13To15limits = IsAgeBetween13To15(employee);
            bool validateYoungerThan13limits = IsAgeYoungerThan13(employee);
            if (!validate16To18limits && !validate13To15limits && !validateYoungerThan13limits)
                return evaluateResults;


            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsStandby()).ToList();

            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            List<TimeScheduleTemplateBlock> scheduledBlocks = GetScheduleBlocksForWorkRuleEvaluation(timeScheduleScenarioHeadId, employee.EmployeeId, dateFrom, dateTo, false).Where(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && !x.IsStandby()).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, false);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Remove scheduledBlocks with deviation cause and setting

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                scheduledBlocks = scheduledBlocks.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
                shiftsToEvaluate = shiftsToEvaluate.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
            }

            #endregion

            #region Decide limit and weeks

            //int limitRuleRestTimeWeek = 0;
            var weeks = new List<(DateTime WeekStart, DateTime WeekStop)>();
            weeks.AddRange(CalendarUtility.GetWeeks(dateFrom, dateTo));

            var weeksU16 = new List<(DateTime WeekStart, DateTime WeekStop)>();
            var weeks16To18 = new List<(DateTime WeekStart, DateTime WeekStop)>();


            foreach (var week in weeks)
            {
                if (IsAgeBetween16To18(employee, week.WeekStart.Date))
                {
                    weeks16To18.Add(week);
                }
            }

            GetMinorsSchoolDayMinutes(out int minorsSchoolDayStartMinutes, out int minorsSchoolDayStopMinutes);
            List<Tuple<DateTime, DateTime>> schoolHolidayIntervals = new List<Tuple<DateTime, DateTime>>();
            List<SchoolHoliday> schoolHolidays = GetSchoolHolidaysInInterval(dateFrom, dateTo, null);
            foreach (SchoolHoliday schoolHoliday in schoolHolidays)
            {
                schoolHolidayIntervals.Add(Tuple.Create(schoolHoliday.DateFrom, schoolHoliday.DateTo));
            }

            weeksU16.AddRange(CalendarUtility.GetSchoolWeeks(dateFrom, dateTo, minorsSchoolDayStartMinutes, minorsSchoolDayStopMinutes, schoolHolidayIntervals));

            if (weeksU16.Any())
            {
                weeksU16.RemoveAll(w => !(IsAgeYoungerThan13(employee, w.WeekStart.Date) || IsAgeBetween13To15(employee, w.WeekStart.Date)));
            }



            int limitRuleRestTimeWeek = employeeGroup.RuleRestTimeWeek > 36 * 60? employeeGroup.RuleRestTimeWeek: 36 * 60;

            if (limitRuleRestTimeWeek == 0)
                return evaluateResults;

            #endregion

            #region Merge

            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();
            shifts.AddRange(shiftsToEvaluate);
            shifts.AddRange(scheduledBlocks.ToTimeSchedulePlanningDayDTOs());

            #endregion

            #region Evaluate

            evaluateResults.AddRange(EvaluateRestTimeWeek(employee.Name, shifts, weeksU16, limitRuleRestTimeWeek, true));
            evaluateResults.AddRange(EvaluateRestTimeWeek(employee.Name, shifts, weeks16To18, limitRuleRestTimeWeek, true));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsWorkAlone(Employee employee, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<Employee> employees, List<int> accountIds, int? timeScheduleScenarioHeadId, List<DateTime> sourceDates = null, bool isPersonalScheduleTemplate = false, bool all = false)
        {
            //OBS! employee and employeeGroup can be null (when evaluateing no replacement, only used with absence. In that case we need to evaluate EvaluateRuleMinorAloneCheckAll)

            //Minderåriga får inte jobba själva i butiken. De får jobba själva på en avdelning så länge det finns någon annan på en annan avdelning.

            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq          

            if (employees == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null || isPersonalScheduleTemplate)
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            DateTime intervalFromDate = shiftsToEvaluate.GetStartDate();
            DateTime intervalToDate = shiftsToEvaluate.GetStopDate();

            List<TimeScheduleTemplateBlock> scheduleBlocksForAccounts = GetScheduleBlocksForAccounts(timeScheduleScenarioHeadId, ref intervalFromDate, ref intervalToDate, sourceDates, accountIds);
            scheduledBlocks = scheduledBlocks.Where(i => i.Date >= intervalFromDate && i.Date <= intervalToDate).ToList();

            List<EmployeeAgeDTO> employeeAgeInfos = GetEmployeeAgeInfoFromCache(employees);
            List<EmployeeAgeDTO> seniors = employeeAgeInfos.GetSeniors();
            EmployeeAgeDTO employeeAgeInfo = employee != null ? employeeAgeInfos.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId) : null;

            #endregion

            #region Check Minor

            if (employeeAgeInfo != null && employeeAgeInfo.IsMinor)
            {
                #region Get all company shift for given dates (exclude given employee)

                scheduleBlocksForAccounts = scheduleBlocksForAccounts.ExcludeEmployeeId(employee.EmployeeId);

                #endregion

                #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

                RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduleBlocksForAccounts, false);
                RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, false);

                #endregion

                #region Zero days in shiftsToEvalute are no longer needed, remove them

                shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

                #endregion

                #region Evaluate

                DateTime currentDayDate = intervalFromDate;
                while (currentDayDate <= intervalToDate)
                {
                    bool isMinorToday = CalendarUtility.IsAgeYoungerThan18(employeeAgeInfo.BirthDate.Value, currentDayDate);
                    if (isMinorToday && IsAloneAtAnyTime(employeeAgeInfo, seniors, scheduledBlocks, scheduleBlocksForAccounts, shiftsToEvaluate, currentDayDate, out string invalidWorkInterval))
                    {
                        string errormsg = string.Format(GetText(8730, "Anställda under 18 år får inte jobba ensamma. Annan person måste vara schemalagd samtidigt. Anställd {0}, {1}"), employee.Name, invalidWorkInterval);
                        var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsWorkAloneViolated, errormsg, employee.Name, currentDayDate)
                        {
                            IsRuleForMinors = true,
                            CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors)
                        };
                        evaluateResults.Add(evaluateResult);
                    }

                    currentDayDate = currentDayDate.AddDays(1);
                }

                #endregion
            }

            #endregion

            #region Check other

            if (!all && (employeeAgeInfo == null || !employeeAgeInfo.IsMinor || sourceDates != null))
                evaluateResults.AddRange(EvaluateRuleMinorAloneCheckAll(employeeAgeInfos, shiftsToEvaluate, scheduleBlocksForAccounts, intervalFromDate, intervalToDate, false));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorsHandlingMoney(Employee employee, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<Employee> employees, List<int> accountIds, int? timeScheduleScenarioHeadId, List<DateTime> sourceDates = null)
        {
            //OBS! employee and employeeGroup can be null (when evaluateing no replacement, only used with absence. In that case we need to evaluate EvaluateRuleMinorAloneCheckAll)
            //16 - 18 år jobba med pengar så längde de inte är ensamma
            //övriga minderåriga får inte jobba med pengar överhuvudtaget

            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employees == null || shiftsInput.IsNullOrEmpty() || scheduledBlocksInput == null)
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => !x.IsStandby()).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            //Local copy
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            List<EmployeeAgeDTO> employeeAgeInfos = GetEmployeeAgeInfoFromCache(employees);
            List<EmployeeAgeDTO> seniors = employeeAgeInfos.GetSeniors();
            EmployeeAgeDTO employeeAgeInfo = employee != null ? employeeAgeInfos.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId) : null;

            List<int> shiftTypeIdsHandlingMoney = GetShiftTypeIdsHandlingMoneyFromCache();
            shiftsToEvaluate = shiftsToEvaluate.Where(x => shiftTypeIdsHandlingMoney.Contains(x.ShiftTypeId)).ToList();
            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            DateTime intervalFromDate = shiftsToEvaluate.GetStartDate();
            DateTime intervalToDate = shiftsToEvaluate.GetStopDate();

            List<TimeScheduleTemplateBlock> scheduleBlocksForAccounts = GetScheduleBlocksForAccounts(timeScheduleScenarioHeadId, ref intervalFromDate, ref intervalToDate, sourceDates, accountIds);
            scheduledBlocks = scheduledBlocks.Where(i => i.Date >= intervalFromDate && i.Date <= intervalToDate).ToList();

            #endregion

            #region Check Minor

            if (employeeAgeInfo != null && employeeAgeInfo.IsMinor)
            {
                if (!employeeAgeInfo.CheckHandleMoneyAlone && !employeeAgeInfo.CheckHanHandleMoney)
                    return evaluateResults;

                #region Get all company shift that involves hanling money for given dates (exclude given employee)

                scheduleBlocksForAccounts = scheduleBlocksForAccounts.ExcludeEmployeeId(employee.EmployeeId);
                scheduleBlocksForAccounts = scheduleBlocksForAccounts.ExcludeHandlingMoney(shiftTypeIdsHandlingMoney);

                #endregion

                #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

                RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduleBlocksForAccounts, false);
                RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, false);

                #endregion

                #region Zero days in shiftsToEvalute are no longer needed, remove them

                shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

                #endregion

                #region Remove shiftsToEvalute from companyShifts (Unsaved changes have precedence)

                RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduleBlocksForAccounts, false);

                #endregion

                #region Evaluate

                DateTime currentDayDate = intervalFromDate;
                while (currentDayDate <= intervalToDate)
                {
                    bool is16To18Today = CalendarUtility.IsAgeBetween16To18(employeeAgeInfo.BirthDate.Value, currentDayDate);
                    bool is13To15Today = CalendarUtility.IsAgeBetween13To15(employeeAgeInfo.BirthDate.Value, currentDayDate);
                    bool isUnder13Today = CalendarUtility.IsAgeYoungerThan18(employeeAgeInfo.BirthDate.Value, currentDayDate);

                    bool checkHandleMoneyToday = is13To15Today || isUnder13Today;
                    bool checkHandleMoneyAloneToday = is16To18Today;

                    if (checkHandleMoneyAloneToday)
                    {
                        if (IsAloneAtAnyTime(employeeAgeInfo, seniors, scheduledBlocks, scheduleBlocksForAccounts, shiftsToEvaluate, currentDayDate, out string invalidWorkInterval))
                        {
                            string errormsg = String.Format(GetText(8735, "Anställda mellan 16-18 år får inte jobba ensamma med pengar. Annan person måste vara schemalagd samtidigt. Anställd {0}, {1}"), employee.Name, invalidWorkInterval);
                            var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsHandlingMoneyViolated, errormsg, employee.Name, currentDayDate)
                            {
                                IsRuleForMinors = true,
                                CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                            };
                            evaluateResults.Add(evaluateResult);
                        }
                    }
                    else if (checkHandleMoneyToday && shiftsToEvaluate.Any(b => b.StartTime.Date == currentDayDate))
                    {
                        string errormsg = String.Format(GetText(8736, "Anställda yngre än 16 år får inte jobba med pengar. Anställd {0}, {1}."), employee.Name, currentDayDate.ToShortDateString());
                        var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsHandlingMoneyViolated, errormsg, employee.Name, currentDayDate)
                        {
                            IsRuleForMinors = true,
                            CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors)
                        };
                        evaluateResults.Add(evaluateResult);
                    }

                    currentDayDate = currentDayDate.AddDays(1);
                }

                #endregion
            }

            #endregion

            #region Check other

            if (employeeAgeInfo == null || !employeeAgeInfo.IsMinor || sourceDates != null)
                evaluateResults.AddRange(EvaluateRuleMinorAloneCheckAll(employeeAgeInfos, shiftsToEvaluate, scheduleBlocksForAccounts, intervalFromDate, intervalToDate, true));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleMinorAloneCheckAll(List<EmployeeAgeDTO> employeeAgeInfos, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> companyScheduleBlocks, DateTime intervalFromDate, DateTime intervalToDate, bool checkHandlingMoney)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq          

            if (employeeAgeInfos.IsNullOrEmpty() || companyScheduleBlocks.IsNullOrEmpty() || shiftsInput == null)
                return evaluateResults;

            bool useWorkRulesForMinors = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors);
            if (!useWorkRulesForMinors)
                return evaluateResults;

            List<EmployeeAgeDTO> minors = employeeAgeInfos.GetMinors();
            if (minors.IsNullOrEmpty())
                return evaluateResults;

            List<EmployeeAgeDTO> seniors = employeeAgeInfos.GetSeniors();

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsInput, companyScheduleBlocks, false);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsInput = shiftsInput.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Remove hidden Employee

            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
            companyScheduleBlocks = companyScheduleBlocks.ExcludeEmployeeId(hiddenEmployeeId);
            shiftsInput = shiftsInput.ExcludeEmployeeId(hiddenEmployeeId);

            #endregion

            #region Evaluate

            foreach (EmployeeAgeDTO minor in minors)
            {
                List<TimeScheduleTemplateBlock> scheduleBlocksMinor = companyScheduleBlocks.Where(i => i.EmployeeId == minor.EmployeeId).ToList();
                if (!scheduleBlocksMinor.Any())
                    continue;

                bool checkIsNotAloneWhenHandlingMoney = false;
                bool checkHandlingMoneyIsNotAllowed = false;
                bool checkWorkingAlone = false;
                if (checkHandlingMoney)
                {
                    if (minor.IsAgeBetween16To18)
                        checkIsNotAloneWhenHandlingMoney = true;
                    else if (minor.IsAgeBetween13To15)
                        checkHandlingMoneyIsNotAllowed = true;
                    else if (minor.IsAgeYoungerThan13)
                        checkHandlingMoneyIsNotAllowed = true;
                }
                else
                {
                    checkWorkingAlone = true;
                }

                if (!checkIsNotAloneWhenHandlingMoney && !checkHandlingMoneyIsNotAllowed && !checkWorkingAlone)
                    continue;

                #region Evaluate

                DateTime currentDayDate = intervalFromDate;
                while (currentDayDate <= intervalToDate)
                {
                    //bool validateUnder18Today = CalendarUtility.IsAgeYoungerThan18(minor.BirthDate.Value, currentDayDate);
                    bool is16To18Today = CalendarUtility.IsAgeBetween16To18(minor.BirthDate.Value, currentDayDate);
                    bool is13To15Today = CalendarUtility.IsAgeBetween13To15(minor.BirthDate.Value, currentDayDate);
                    bool isUnder13Today = CalendarUtility.IsAgeYoungerThan18(minor.BirthDate.Value, currentDayDate);

                    bool checkIsNotAloneWhenHandlingMoneyToday = false;
                    bool checkHandlingMoneyIsNotAllowedToday = false;
                    bool checkWorkingAloneToday = false;

                    if (checkHandlingMoney)
                    {
                        if (is16To18Today)
                            checkIsNotAloneWhenHandlingMoneyToday = true;
                        else if (is13To15Today)
                            checkHandlingMoneyIsNotAllowedToday = true;
                        else if (isUnder13Today)
                            checkHandlingMoneyIsNotAllowedToday = true;
                    }
                    else
                    {
                        checkWorkingAloneToday = true;
                    }

                    //if (!checkIsNotAloneWhenHandlingMoneyToday && !checkHandlingMoneyIsNotAllowedToday && !checkWorkingAloneToday)
                    //    continue;


                    if (checkWorkingAloneToday)
                    {
                        if (IsAloneAtAnyTime(minor, seniors, scheduleBlocksMinor, companyScheduleBlocks, shiftsInput, currentDayDate, hiddenEmployeeId, out string invalidWorkInterval))
                        {
                            string errormsg = String.Format(GetText(8730, "Anställda under 18 år får inte jobba ensamma. Annan person måste vara schemalagd samtidigt. Anställd {0}, {1}"), minor.Name, invalidWorkInterval);
                            var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsWorkAloneViolated, errormsg, minor.Name, currentDayDate)
                            {
                                IsRuleForMinors = true,
                                CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                            };
                            evaluateResults.Add(evaluateResult);
                        }
                    }
                    else if (checkIsNotAloneWhenHandlingMoneyToday)
                    {
                        if (IsAloneAtAnyTime(minor, seniors, scheduleBlocksMinor, companyScheduleBlocks, shiftsInput, currentDayDate, hiddenEmployeeId, out string invalidWorkInterval))
                        {
                            string errormsg = string.Format(GetText(8735, "Anställda mellan 16-18 år får inte jobba ensamma med pengar. Annan person måste vara schemalagd samtidigt. Anställd {0}, {1}"), minor.Name, invalidWorkInterval);
                            var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsHandlingMoneyViolated, errormsg, minor.Name, currentDayDate)
                            {
                                IsRuleForMinors = true,
                                CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors),
                            };
                            evaluateResults.Add(evaluateResult);
                        }
                    }
                    else if (checkHandlingMoneyIsNotAllowedToday && shiftsInput.Any(b => b.EmployeeId == minor.EmployeeId && b.StartTime.Date == currentDayDate)) //NOSONAR
                    {
                        string errormsg = string.Format(GetText(8736, "Anställda yngre än 16 år får inte jobba med pengar. Anställd {0}, {1}."), minor.Name, currentDayDate.ToShortDateString());
                        var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsHandlingMoneyViolated, errormsg, minor.Name, currentDayDate)
                        {
                            IsRuleForMinors = true,
                            CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors)
                        };
                        evaluateResults.Add(evaluateResult);
                    }

                    currentDayDate = currentDayDate.AddDays(1);
                }

                #endregion
            }

            #endregion

            return evaluateResults;
        }

        #endregion

        private List<EvaluateWorkRuleResultDTO> EvaluateHoursBeforeAssignShift(Employee employee, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsInput)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();
            int hours = GetCompanyIntSettingFromCache(CompanySettingType.TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift);
            DateTime checkDate = DateTime.Now.AddHours(hours);

            #region Prereq          

            if (hours == 0 || employee == null || employeeGroup == null || shiftsInput.IsNullOrEmpty())
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            //Create a local copy so that changes doesnt affect the input collection
            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsInput.Where(x => x.TimeScheduleTemplateBlockId == 0 && x.EmployeeId == employee.EmployeeId && x.StartTime >= DateTime.Now && x.StartTime < checkDate).ToList();

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Evaluate

            if (!shiftsToEvaluate.Any())
                return evaluateResults;

            foreach (TimeSchedulePlanningDayDTO shift in shiftsToEvaluate)
            {
                string errormsg = string.Format(GetText(8950, "Pass får inte tilldelas tidigare än {0} timmar före {1} {2}-{3} för {4}."), hours, shift.StartTime.ToShortDateString(), shift.StartTime.ToShortTimeString(), shift.StopTime.ToShortTimeString(), employee.Name);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleMinorsHandlingMoneyViolated, errormsg, employee.Name, shift.ActualDate);
                evaluateResults.Add(evaluateResult);
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO>  EvaluateLeisureCodes(Employee employee, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> planningPeriodScheduledBlocks, List<TimeScheduleEmployeePeriodDetail> planningPeriodLeisureCodes, DateTime? planningPeriodStartDate, DateTime? planningPeriodStopDate, EmployeeGroupTimeLeisureCode employeeGroupTimeLeisureCodeV, EmployeeGroupTimeLeisureCode employeeGroupTimeLeisureCodeX, List<TimeDeviationCause> timeDeviationCauses, bool keepScheduleBlocks = false)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region prereq

            if (employee == null || shiftsInput == null || planningPeriodScheduledBlocks == null || planningPeriodStartDate == null || planningPeriodStopDate == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            // Only if the period starts on a monday and ends on a sunday
            if (planningPeriodStartDate.Value.DayOfWeek != DayOfWeek.Monday && planningPeriodStopDate.Value.DayOfWeek != DayOfWeek.Sunday)
                return evaluateResults;

            #endregion

            #region Add or remove shiftsInput to/from planningPeriodScheduledBlocks

            if (!keepScheduleBlocks)
                RemovePlannedShiftsFromScheduledBlocks(shiftsInput, planningPeriodScheduledBlocks, true, true);

            #endregion

            #region Remove scheduledBlocks with deviation cause and setting

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                planningPeriodScheduledBlocks = planningPeriodScheduledBlocks.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
            }

            #endregion

            #region Group scheduledBlocks by week

            List<IGrouping<DateTime, TimeScheduleTemplateBlock>> planningPeriodScheduledBlocksGrouped = planningPeriodScheduledBlocks.Where(x => x.TotalMinutes > 0).GroupBy(x => CalendarUtility.GetFirstDateOfWeek(x.Date.Value, offset: DayOfWeek.Monday)).ToList();
            
            #endregion

            #region evaluate

            List<int> vDayWeeksMissing = new List<int>();
            List<int> vDayWeeksTooMany = new List<int>();
            int existingVDaysOnPeriod = 0;
            int maxWorkingDaysOnPeriod = 15;
            int maxWorkingDaysOnWeek = 6;

            for (int i = 0; i < planningPeriodScheduledBlocksGrouped.Count; i++)
            {
                List<TimeScheduleTemplateBlock> weekScheduledBlocks = planningPeriodScheduledBlocksGrouped[i].ToList();
                DateTime weekStartDate = planningPeriodScheduledBlocksGrouped[i].Key;
                DateTime weekStopDate = weekStartDate.AddDays(6);

                if (weekStartDate >= planningPeriodStartDate && weekStartDate < planningPeriodStopDate)
                {
                    // check if there is too many leisure codes V for the week
                    int existingVDays = planningPeriodLeisureCodes.Where(x => x.TimeScheduleEmployeePeriod.Date >= weekStartDate && x.TimeScheduleEmployeePeriod.Date <= weekStopDate && x.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.V).Select(x => x.TimeScheduleEmployeePeriod.Date).Distinct().Count();
                    if (existingVDays > 1)
                        vDayWeeksTooMany.Add(CalendarUtility.GetWeekNr(weekStartDate));
                    
                    // check if too many workingdays in the week (and no existing V-day)
                    if (PeriodExceedsWorkingdays(weekStartDate, weekStopDate, weekScheduledBlocks, maxWorkingDaysOnWeek) && existingVDays == 0)
                        vDayWeeksMissing.Add(CalendarUtility.GetWeekNr(weekStartDate));

                    existingVDaysOnPeriod += existingVDays;
                }
            }

            #region check weekly days off for the weeks

            if (vDayWeeksMissing.Any())
            {
                string errormsg = string.Format(GetText(13100, "{0}: Det saknas fridag ({1}) under vecka(or): {2}"), employee.Name, employeeGroupTimeLeisureCodeV.TimeLeisureCode.Code, String.Join(", ", vDayWeeksMissing));
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                evaluateResults.Add(evaluateResult);
            }
            if (vDayWeeksTooMany.Any())
            {
                string errormsg = string.Format(GetText(13103, "{0}: Det finns för många fridagar ({1}) under vecka(or): {2}"), employee.Name, employeeGroupTimeLeisureCodeV.TimeLeisureCode.Code, String.Join(", ", vDayWeeksTooMany));
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                evaluateResults.Add(evaluateResult);
            }

            #endregion

            #region check rules for the whole period

            #region check extra days off


            // check if there is too many leisure codes X for the period
            int existingXDays = planningPeriodLeisureCodes.Where(x => x.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.X && x.TimeScheduleEmployeePeriod.Date >= planningPeriodStartDate.Value && x.TimeScheduleEmployeePeriod.Date <= planningPeriodStopDate.Value).Select(x => x.TimeScheduleEmployeePeriod.Date).Distinct().Count();
            // TODO: Max nr of 3 X-days is hardcoded for now, should be a setting for that
            if (existingXDays > 3)
            {
                string errormsg = string.Format(GetText(13102, "{0}: Det finns för många fridagar ({1}) under perioden"), employee.Name, employeeGroupTimeLeisureCodeX.TimeLeisureCode.Code);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                evaluateResults.Add(evaluateResult);
            }

            // check if too many workingdays in the week (and less than 3 existing V-days and 3 existing X-days)
            if (PeriodExceedsWorkingdays(planningPeriodStartDate.Value, planningPeriodStopDate.Value, planningPeriodScheduledBlocks, maxWorkingDaysOnPeriod) && (existingVDaysOnPeriod < 3 || existingXDays < 3))
            {
                string errormsg = string.Format(GetText(13101, "{0}: För många arbetsdagar under perioden (max {1})"), employee.Name, maxWorkingDaysOnPeriod);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                evaluateResults.Add(evaluateResult);
            }

            #endregion

            // check consecutive working days and if leisure codes fit between surrounding shifts

            int maxConsecutiveWorkingDays = 6;
            int consecutiveWorkingDays = 0;
            bool warnAboutConsecutiveDays = false;
            bool haveCheckedBackwards = false;
            TimeScheduleTemplateBlock previousScheduledBlock = null;
            TimeScheduleTemplateBlock nextScheduledBlock = null;
            List<DateTime> schedulesOnDayOff = new List<DateTime>();

            for (DateTime currentDate = planningPeriodStartDate.Value; currentDate <= planningPeriodStopDate; currentDate = currentDate.AddDays(1))
            {
                previousScheduledBlock = planningPeriodScheduledBlocks.Where(x => !x.IsBreak && x.StartTime != x.StopTime && x.Date == currentDate.AddDays(-1)).OrderByDescending(x => x.StartTime).FirstOrDefault();
                nextScheduledBlock = planningPeriodScheduledBlocks.Where(x => !x.IsBreak && x.StartTime != x.StopTime && x.Date == currentDate.AddDays(1)).OrderBy(x => x.StartTime).FirstOrDefault();
                bool currentWorkingDay = planningPeriodScheduledBlocks.Any(x => !x.IsBreak && x.StartTime != x.StopTime && x.Date == currentDate);

                if (currentWorkingDay)
                    consecutiveWorkingDays++;
                else
                {
                    // when first dayoff is found, also look backwards on days before period startdate
                    int consecutiveWorkingDaysBackwards = 0;
                    if (!haveCheckedBackwards)
                    {
                        // check days backwards and add to current days
                        bool foundDayOffBackwards = false;
                        DateTime currentDateBackwards = planningPeriodStartDate.Value.AddDays(-1);

                        while (!foundDayOffBackwards && consecutiveWorkingDaysBackwards <= maxConsecutiveWorkingDays)
                        {
                            if (planningPeriodScheduledBlocks.Any(x => !x.IsBreak && x.StartTime != x.StopTime && x.Date == currentDateBackwards))
                                consecutiveWorkingDaysBackwards++;
                            else
                                foundDayOffBackwards = true;
                            
                            currentDateBackwards = currentDateBackwards.AddDays(-1);
                        }
                        haveCheckedBackwards = true;
                    }
                    
                    // check if we should warn about it
                    if (consecutiveWorkingDays + consecutiveWorkingDaysBackwards > maxConsecutiveWorkingDays)
                        warnAboutConsecutiveDays = true;
                    
                    consecutiveWorkingDays = 0;
                }

                // check if leisure codes fit between surrounding shifts
                TimeScheduleEmployeePeriodDetail leisureCodeDay = planningPeriodLeisureCodes.FirstOrDefault(x => (x.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.V || x.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.X) && x.TimeScheduleEmployeePeriod.Date == currentDate);
                if (leisureCodeDay != null)
                {
                    if (!currentWorkingDay)
                    {
                        EmployeeGroupTimeLeisureCodeSetting hoursSetting = null;
                        if (leisureCodeDay.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.V)
                            hoursSetting = employeeGroupTimeLeisureCodeV.EmployeeGroupTimeLeisureCodeSetting.FirstOrDefault(x => x.State == (int)SoeEntityState.Active && x.Type == (int)TermGroup_TimeLeisureCodeSettingType.LeisureHours);
                        else if (leisureCodeDay.TimeLeisureCode.Type == (int)TermGroup_TimeLeisureCodeType.X)
                            hoursSetting = employeeGroupTimeLeisureCodeX.EmployeeGroupTimeLeisureCodeSetting.FirstOrDefault(x => x.State == (int)SoeEntityState.Active && x.Type == (int)TermGroup_TimeLeisureCodeSettingType.LeisureHours);

                        if (hoursSetting != null)
                        {
                            if (previousScheduledBlock != null && nextScheduledBlock != null && hoursSetting.IntData != null)
                            {
                                if (previousScheduledBlock.ActualStopTime.Value.AddHours((double)hoursSetting.IntData) > nextScheduledBlock.ActualStartTime)
                                {
                                    string errormsg = string.Format(GetText(13105, "{0}: Fridag ({1}) den {2} är för kort (minst {3} timmar)"), employee.Name, leisureCodeDay.TimeLeisureCode.Code, currentDate.ToShortDateString(), hoursSetting.IntData);
                                    var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                                    evaluateResults.Add(evaluateResult);
                                }
                            }
                        }
                    }
                    else
                    {
                        schedulesOnDayOff.Add(currentDate);
                    }
                }
            }

            // check consecutive work days as a last step
            if (consecutiveWorkingDays > maxConsecutiveWorkingDays || warnAboutConsecutiveDays)
            {
                string errormsg = string.Format(GetText(13104, "{0}: Det finns för många arbetsdagar i rad (max {1})"), employee.Name, maxConsecutiveWorkingDays);
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                evaluateResults.Add(evaluateResult);
            }

            // there is schedule on a day off
            if (schedulesOnDayOff.Any())
            {
                string errormsg = string.Format(GetText(13106, "{0}: Det finns schema på en fridag ({1})"), employee.Name, string.Join(", ", schedulesOnDayOff.Select(d => d.ToShortDateString())));
                var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleLeisureCodes, errormsg, employee.Name, planningPeriodStartDate);
                evaluateResults.Add(evaluateResult);
            }

            #endregion

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateAnnualLeave(Employee employee, List<TimeSchedulePlanningDayDTO> shiftsInput, List<TimeScheduleTemplateBlock> scheduledBlocks, bool isAddingAnnualLeaveDay)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region prereq

            if (employee == null || shiftsInput == null || scheduledBlocks == null)
                return evaluateResults;

            //Dont evaluate for hiddenemployee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return evaluateResults;

            // If adding a new shift, don't warn about other existing annual leave shifts already placed
            if (isAddingAnnualLeaveDay)
                scheduledBlocks = scheduledBlocks.Where(x => x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).ToList();

            mergePlannedShiftsIntoScheduledBlocks(shiftsInput, scheduledBlocks);

            #endregion

            #region evaluate

            // check if annual leave shift fits between surrounding shifts

            foreach (TimeScheduleTemplateBlock annualLeaveShift in scheduledBlocks.Where(x => !x.IsBreak && x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.AnnualLeave))
            {
                AnnualLeaveGroup annualLeaveGroup = employee.GetEmployment(annualLeaveShift.ActualStartTime).GetAnnualLeaveGroup(annualLeaveShift.ActualStartTime);
                int minimumGapMinutes = annualLeaveGroup?.RuleRestTimeMinimum ?? 0;

                TimeScheduleTemplateBlock previousScheduledBlock = scheduledBlocks.Where(x => !x.IsBreak && x.StartTime != x.StopTime && x.ActualStartTime <= annualLeaveShift.ActualStartTime && x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).OrderByDescending(x => x.ActualStartTime).FirstOrDefault();
                TimeScheduleTemplateBlock nextScheduledBlock = scheduledBlocks.Where(x => !x.IsBreak && x.StartTime != x.StopTime && x.ActualStartTime > annualLeaveShift.ActualStartTime && x.AbsenceType == (int)TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard).OrderBy(x => x.ActualStartTime).FirstOrDefault();

                if (previousScheduledBlock == null)
                {
                    DateTime prevShiftDateTime = annualLeaveShift.ActualStartTime.Value.AddMinutes(-minimumGapMinutes);

                    // fake block far away
                    previousScheduledBlock = new TimeScheduleTemplateBlock() 
                    { 
                        StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT,  prevShiftDateTime),
                        StopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, prevShiftDateTime),
                        Date = prevShiftDateTime.Date
                    };
                }
                if (nextScheduledBlock == null)
                {
                    DateTime nextShiftDateTime = annualLeaveShift.ActualStopTime.Value.AddMinutes(minimumGapMinutes);

                    // fake block far away
                    nextScheduledBlock = new TimeScheduleTemplateBlock()
                    {
                        StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, nextShiftDateTime),
                        StopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, nextShiftDateTime),
                        Date = nextShiftDateTime.Date
                    };
                }

                TimeSpan diff = (DateTime)nextScheduledBlock.ActualStartTime - (DateTime)previousScheduledBlock.ActualStopTime;
                int diffMinutes = (int)diff.TotalMinutes;

                if (diffMinutes < minimumGapMinutes)
                {
                    string errormsg = string.Format(GetText(13125, "{0}: Ej tillräcklig ledig tid för placering av årsledighetsdag ({1})"), employee.Name, annualLeaveShift.ActualStartTime.ToShortDateString());
                    var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleAnnualLeave, errormsg, employee.Name, annualLeaveShift.ActualStartTime);
                    evaluateResults.Add(evaluateResult);
                }
            }

            #endregion

            return evaluateResults;
        }

        #endregion

        #region Common

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleOverlappingShifts(string name, List<TimeSchedulePlanningDayDTO> shiftsToEvalute)
        {
            bool ruleViolation = false;
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();
            List<string> violationPairs = new List<string>();

            foreach (var shiftToEvalute in shiftsToEvalute)
            {
                if (shiftToEvalute.StartTime == shiftToEvalute.StopTime)
                    continue;

                #region Make sur not to evaluate shift against it self

                List<TimeSchedulePlanningDayDTO> otherPlannedShifts = shiftsToEvalute.Where(x => x.UniqueId != shiftToEvalute.UniqueId).ToList();

                #endregion

                #region Evaluate

                foreach (TimeSchedulePlanningDayDTO otherPlannedShift in otherPlannedShifts)
                {
                    if (shiftToEvalute.TimeScheduleTemplateBlockId != 0 && otherPlannedShift.TimeScheduleTemplateBlockId != 0 && shiftToEvalute.TimeScheduleTemplateBlockId == otherPlannedShift.TimeScheduleTemplateBlockId)
                        continue;

                    if (otherPlannedShift.StartTime == otherPlannedShift.StopTime)
                        continue;

                    bool skipCheck = false;
                    switch (shiftToEvalute.Type)
                    {
                        case TermGroup_TimeScheduleTemplateBlockType.Schedule:
                            skipCheck = otherPlannedShift.IsOrder() || otherPlannedShift.IsBooking() || otherPlannedShift.IsOnDuty();
                            break;
                        case TermGroup_TimeScheduleTemplateBlockType.Order:
                            skipCheck = otherPlannedShift.IsSchedule() || otherPlannedShift.IsOnDuty();
                            break;
                        case TermGroup_TimeScheduleTemplateBlockType.Booking:
                            skipCheck = otherPlannedShift.IsSchedule() || otherPlannedShift.IsOnDuty();
                            break;
                        case TermGroup_TimeScheduleTemplateBlockType.Standby:
                            // TODO: skipCheck = otherPlannedShift.IsSchedule();
                            break;
                        case TermGroup_TimeScheduleTemplateBlockType.OnDuty:
                            skipCheck = true;
                            break;
                    }

                    if (skipCheck)
                        continue;

                    //planned shift starttime conflicts with scheduled block
                    if (otherPlannedShift.StartTime <= shiftToEvalute.StartTime && shiftToEvalute.StartTime < otherPlannedShift.StopTime)
                        ruleViolation = true;

                    //planned shift stoptime conflicts with scheduled block
                    if (otherPlannedShift.StartTime < shiftToEvalute.StopTime && shiftToEvalute.StopTime <= otherPlannedShift.StopTime)
                        ruleViolation = true;

                    //scheduled block is overlapped by planned shift 
                    if (CalendarUtility.IsCurrentOverlappedByNew(shiftToEvalute.StartTime, shiftToEvalute.StopTime, otherPlannedShift.StartTime, otherPlannedShift.StopTime))
                        ruleViolation = true;

                    //planned shift is overlapped by scheduled block
                    if (CalendarUtility.IsNewOverlappedByCurrent(shiftToEvalute.StartTime, shiftToEvalute.StopTime, otherPlannedShift.StartTime, otherPlannedShift.StopTime))
                        ruleViolation = true;

                    #region Result

                    if (ruleViolation)
                    {
                        string violationPairId = shiftToEvalute.UniqueId + " " + otherPlannedShift.UniqueId;
                        string violationPairIdReversed = otherPlannedShift.UniqueId + " " + shiftToEvalute.UniqueId;
                        if (!(violationPairs.Any(x => x == violationPairId) || violationPairs.Any(x => x == violationPairIdReversed)))
                        {
                            bool isOrder = shiftToEvalute.Order != null;

                            String errormsg = String.Format(GetText(8285, "{0} {1} {2}-{3} krockar med {4} {5} {6}-{7} för {8}."),
                                StringUtility.CamelCaseWord(GetText(isOrder ? 485 : 481, (int)TermGroup.TimeSchedulePlanning)),
                                shiftToEvalute.StartTime.Date.ToShortDateString(),
                                shiftToEvalute.StartTime.ToShortTimeString(),
                                shiftToEvalute.StopTime.ToShortTimeString(),
                                GetText(isOrder ? 485 : 481, (int)TermGroup.TimeSchedulePlanning),
                                otherPlannedShift.StartTime.ToShortDateString(),
                                otherPlannedShift.StartTime.ToShortTimeString(),
                                otherPlannedShift.StopTime.ToShortTimeString(),
                                name);

                            evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleShiftsOverlap, errormsg, name, shiftToEvalute.ActualDate));
                            violationPairs.Add(violationPairId);
                        }
                    }
                    ruleViolation = false;

                    #endregion
                }

                #endregion
            }
            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleRestTimeDay(string name, EmployeeGroup employeeGroup, List<(DateTime DayStart, DateTime DayStop)> days, List<TimeSchedulePlanningDayDTO> shiftsToEvaluateInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeBlock> presenceTimeBlocks = null, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluateInput, scheduledBlocksInput, false);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluateInput = shiftsToEvaluateInput.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Remove scheduledBlocks with deviation cause and setting

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                scheduledBlocksInput = scheduledBlocksInput.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
                shiftsToEvaluateInput = shiftsToEvaluateInput.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
            }

            #endregion

            evaluateResults.AddRange(EvaluateRestTimeDay(name, days, shiftsToEvaluateInput, scheduledBlocksInput, employeeGroup.RuleRestTimeDay, false, presenceTimeBlocks: presenceTimeBlocks));

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRestTimeDay(string name, List<(DateTime DayStart, DateTime DayStop)> days, List<TimeSchedulePlanningDayDTO> shiftsToEvaluateInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, int limitRestTimeDayMinutes, bool isEvaluatingMinor, List<TimeBlock> presenceTimeBlocks = null)
        {

            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            foreach (var day in days)
            {
                bool weekRestReached = false;

                List<TimeSchedulePlanningDayDTO> plannedshiftsForDay = shiftsToEvaluateInput.Where(shift => shift.StartTime != shift.StopTime && CalendarUtility.IsDatesOverlapping(shift.StartTime, shift.StopTime, day.DayStart, day.DayStop)).OrderBy(shift => shift.StartTime).ToList();
                List<TimeScheduleTemplateBlock> scheduleBlocksForDay = scheduledBlocksInput.Where(shift => shift.StartTime != shift.StopTime && shift.ActualStartTime.HasValue && shift.ActualStopTime.HasValue && CalendarUtility.IsDatesOverlapping(shift.ActualStartTime.Value, shift.ActualStopTime.Value, day.DayStart, day.DayStop)).OrderBy(shift => shift.ActualStartTime).ToList();
                List<TimeBlock> presenceTimeBlocksForDay = presenceTimeBlocks?.Where(timeBlock => timeBlock.ActualStartTime.HasValue && timeBlock.ActualStopTime.HasValue && timeBlock.ActualStartTime.Value != timeBlock.ActualStopTime.Value && CalendarUtility.IsDatesOverlapping(timeBlock.ActualStartTime.Value, timeBlock.ActualStopTime.Value, day.DayStart, day.DayStop)).OrderBy(timeBlock => timeBlock.StartTime).ToList() ?? new List<TimeBlock>();

                List<WorkIntervalDTO> plannedIntervals = plannedshiftsForDay.GetCoherentWorkIntervals(new List<TimeScheduleTypeDTO>()).OrderBy(x => x.StartTime).ToList();
                List<WorkIntervalDTO> scheduledIntervals = scheduleBlocksForDay.ToTimeSchedulePlanningDayDTOs().GetCoherentWorkIntervals(new List<TimeScheduleTypeDTO>()).OrderBy(x => x.StartTime).ToList();
                List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(plannedIntervals, scheduledIntervals);
                List<WorkIntervalDTO> presenceIntervals = presenceTimeBlocksForDay.IsNullOrEmpty() ? new List<WorkIntervalDTO>() : presenceTimeBlocksForDay.ToDTOs(false, false).ToList().GetCoherentWorkIntervals();
                workIntervals = CalendarUtility.MergeIntervals(workIntervals, presenceIntervals, true);
                if (workIntervals.Count == 0)
                    continue;

                int loopCount = workIntervals.Count - 1;

                for (int i = 0; i <= loopCount; i++)
                {
                    WorkIntervalDTO workInterval = workIntervals[i];
                    if (i == 0 && day.DayStart < workInterval.StartTime)
                    {
                        int restFromStartBoundryToFirstShift = (int)(workInterval.StartTime - day.DayStart).TotalMinutes;
                        weekRestReached = isLimitReached(restFromStartBoundryToFirstShift);
                        if (weekRestReached)
                            break;
                    }

                    if (i == loopCount && day.DayStop > workInterval.StopTime)
                    {
                        int restFromLastShiftToStopBoundry = (int)(day.DayStop - workInterval.StopTime).TotalMinutes;
                        weekRestReached = isLimitReached(restFromLastShiftToStopBoundry);
                        if (weekRestReached)
                            break;
                    }
                    if (i < loopCount)
                    {
                        WorkIntervalDTO nextInterval = workIntervals[i + 1];
                        int restToNextShift = (int)(nextInterval.StartTime - workInterval.StopTime).TotalMinutes;
                        weekRestReached = isLimitReached(restToNextShift);
                        if (weekRestReached)
                            break;
                    }
                }

                if (!weekRestReached)
                {
                    #region Result

                    string dayStartStr = "";
                    string dayStopStr = "";

                    if (day.DayStart.TimeOfDay.TotalMinutes > 0)
                    {
                        dayStartStr = day.DayStart.ToShortDateShortTimeString();
                        dayStopStr = day.DayStop.ToShortDateShortTimeString();
                    }
                    else
                    {
                        dayStartStr = day.DayStart.ToShortDateString();
                        dayStopStr = day.DayStop.ToShortDateString();
                    }

                    string errormsg = String.Format(GetText(8275, "Min. dygnsvila ({0}) mellan {1} - {2} ej uppfylld för {3}."), CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(limitRestTimeDayMinutes), false, false, false), dayStartStr, dayStopStr, name);
                    EvaluateWorkRuleResultDTO evaluateResult = new EvaluateWorkRuleResultDTO(isEvaluatingMinor ? (int)ActionResultSave.RuleMinorsRestDayReached : (int)ActionResultSave.RuleRestDayReached, errormsg, name, day.DayStart);
                    if (isEvaluatingMinor)
                    {
                        evaluateResult.IsRuleForMinors = true;
                        evaluateResult.CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors);

                    }
                    evaluateResults.Add(evaluateResult);

                    #endregion
                }
            }

            bool isLimitReached(int value)
            {
                return value >= limitRestTimeDayMinutes;
            }

            return evaluateResults;


        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleRestTimeWeek(string name, List<(DateTime WeekStart, DateTime WeekStop)> weeks, int ruleRestTimeWeek, List<TimeSchedulePlanningDayDTO> shiftsToEvaluateInput, List<TimeScheduleTemplateBlock> scheduledBlocksInput, List<TimeBlock> presenceTimeBlocks = null, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Remove shiftsToEvalute from scheduledBlocks (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluateInput, scheduledBlocksInput, false);

            #endregion

            #region Remove standby

            List<TimeSchedulePlanningDayDTO> shiftsToEvaluate = shiftsToEvaluateInput.Where(x => !x.IsStandby()).ToList();
            List<TimeScheduleTemplateBlock> scheduledBlocks = scheduledBlocksInput.Where(x => !x.IsStandby()).ToList();

            #endregion

            #region Remove scheduledBlocks with deviation cause and setting

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                scheduledBlocks = scheduledBlocks.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
                shiftsToEvaluate = shiftsToEvaluate.Where(x => !timeDeviationCauses.Any(c => c.TimeDeviationCauseId == x.TimeDeviationCauseId && c.ExcludeFromScheduleWorkRules)).ToList();
            }

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Merge

            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();
            shifts.AddRange(shiftsToEvaluate);
            shifts.AddRange(scheduledBlocks.ToTimeSchedulePlanningDayDTOs());

            #endregion            

            #region Evaluate

            evaluateResults.AddRange(EvaluateRestTimeWeek(name, shifts, weeks, ruleRestTimeWeek, false, presenceTimeBlocks: presenceTimeBlocks));

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRestTimeWeek(string name, List<TimeSchedulePlanningDayDTO> shiftsToEvaluate, List<(DateTime WeekStart, DateTime WeekStop)> weeks, int limitRestTimeWeekMinutes, bool isEvaluatingMinor, List<TimeBlock> presenceTimeBlocks = null)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            foreach (var week in weeks)
            {
                bool weekRestReached = false;

                List<TimeSchedulePlanningDayDTO> shiftsForWeek = shiftsToEvaluate.Where(shift => shift.StartTime != shift.StopTime && CalendarUtility.IsDatesOverlapping(shift.StartTime, shift.StopTime, week.WeekStart, week.WeekStop)).OrderBy(shift => shift.StartTime).ToList();
                List<TimeBlock> presenceTimeBlocksForWeek = presenceTimeBlocks?.Where(timeBlock => timeBlock.ActualStartTime.HasValue && timeBlock.ActualStopTime.HasValue && timeBlock.ActualStartTime.Value != timeBlock.ActualStopTime.Value && CalendarUtility.IsDatesOverlapping(timeBlock.ActualStartTime.Value, timeBlock.ActualStopTime.Value, week.WeekStart, week.WeekStop)).OrderBy(timeBlock => timeBlock.StartTime).ToList() ?? new List<TimeBlock>();

                List<WorkIntervalDTO> shiftIntervals = shiftsForWeek.GetCoherentWorkIntervals(new List<TimeScheduleTypeDTO>()).OrderBy(x => x.StartTime).ToList();
                List<WorkIntervalDTO> presenceIntervals = presenceTimeBlocksForWeek.IsNullOrEmpty() ? new List<WorkIntervalDTO>() : presenceTimeBlocksForWeek.ToDTOs(false, false).ToList().GetCoherentWorkIntervals();
                List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(shiftIntervals, presenceIntervals, true);
                if (workIntervals.Count == 0)
                    continue;

                int loopCount = workIntervals.Count - 1;

                for (int i = 0; i <= loopCount; i++)
                {
                    WorkIntervalDTO workInterval = workIntervals[i];
                    if (i == 0 && week.WeekStart < workInterval.StartTime)
                    {
                        int restFromStartBoundryToFirstShift = (int)(workInterval.StartTime - week.WeekStart).TotalMinutes;
                        weekRestReached = isLimitReached(restFromStartBoundryToFirstShift);
                        if (weekRestReached)
                            break;
                    }

                    if (i == loopCount && week.WeekStop > workInterval.StopTime)
                    {
                        int restFromLastShiftToStopBoundry = (int)(week.WeekStop - workInterval.StopTime).TotalMinutes;
                        weekRestReached = isLimitReached(restFromLastShiftToStopBoundry);
                        if (weekRestReached)
                            break;
                    }
                    if (i < loopCount)
                    {
                        WorkIntervalDTO nextInterval = workIntervals[i + 1];
                        int restToNextShift = (int)(nextInterval.StartTime - workInterval.StopTime).TotalMinutes;
                        weekRestReached = isLimitReached(restToNextShift);
                        if (weekRestReached)
                            break;
                    }
                }

                if (!weekRestReached)
                {
                    #region Result

                    string weekStartStr = "";
                    string weekStopStr = "";

                    if (week.WeekStart.TimeOfDay.TotalMinutes > 0)
                    {
                        weekStartStr = week.WeekStart.ToShortDateShortTimeString();
                        weekStopStr = week.WeekStop.ToShortDateShortTimeString();
                    }
                    else
                    {
                        weekStartStr = week.WeekStart.ToShortDateString();
                        weekStopStr = week.WeekStop.ToShortDateString();
                    }

                    string errormsg = String.Format(GetText(8471, "Min. veckovila ({0}) mellan {1} - {2} ej uppfylld för {3}."), CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(limitRestTimeWeekMinutes), false, false, false), weekStartStr, weekStopStr, name);
                    EvaluateWorkRuleResultDTO evaluateResult = new EvaluateWorkRuleResultDTO(isEvaluatingMinor ? (int)ActionResultSave.RuleMinorsRestWeekReached : (int)ActionResultSave.RuleRestWeekReached, errormsg, name, week.WeekStart);
                    if (isEvaluatingMinor)
                    {
                        evaluateResult.IsRuleForMinors = true;
                        evaluateResult.CanUserOverrideRuleForMinorsViolation = GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors);

                    }
                    evaluateResults.Add(evaluateResult);

                    #endregion
                }
            }

            bool isLimitReached(int value)
            {
                return value >= limitRestTimeWeekMinutes;
            }

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleBreaks(string name, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsToEvaluate, List<TimeScheduleTemplateBlock> scheduledBlocks)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            if (employeeGroup.MaxScheduleTimeWithoutBreaks == 0)
                return evaluateResults;

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();

            #endregion

            #region Remove shiftsToEvalute from scheduledBlocksInput (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            #region Zero days in shiftsToEvalute are no longer needed, remove them

            shiftsToEvaluate = shiftsToEvaluate.Where(x => x.StartTime != x.StopTime).ToList();

            #endregion

            #region Remove absence

            shiftsToEvaluate = shiftsToEvaluate.Where(x => !x.TimeDeviationCauseId.HasValue).ToList();

            #endregion

            #region Evaluate

            DateTime currentDate = intervalFromDate;
            while (currentDate <= intervalToDate)
            {
                List<TimeSchedulePlanningDayDTO> plannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDate).OrderBy(b => b.StartTime).ToList();
                List<TimeScheduleTemplateBlock> scheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDate).OrderBy(b => b.StartTime).ToList();

                List<WorkIntervalDTO> plannedShiftsWorkIntervals = plannedShifts.GetWorkIntervals();
                List<WorkIntervalDTO> scheduledShiftsWorkIntervals = scheduledShifts.GetWorkIntervals(hiddenEmployeeId);
                List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(plannedShiftsWorkIntervals, scheduledShiftsWorkIntervals);

                foreach (WorkIntervalDTO workInterval in workIntervals)
                {
                    if (workInterval.TotalMinutes > employeeGroup.MaxScheduleTimeWithoutBreaks)
                    {
                        #region Result

                        String errormsg = String.Format(GetText(11555, "Raster och vila ej uppfylld för {0}. Får inte jobba mer än {1} i sträck utan rast ({2})") + " " + currentDate.ToShortDateString(), name, CalendarUtility.FormatMinutes(employeeGroup.MaxScheduleTimeWithoutBreaks), CalendarUtility.FormatMinutes(workInterval.TotalMinutes));
                        evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleBreaksViolated, errormsg, name, currentDate));

                        #endregion
                    }
                }

                currentDate = currentDate.AddDays(1); //force the loop to change week 
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleWorkTimeWeekMaxMin(string name, Employment employment, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsToEvalute, List<TimeScheduleTemplateBlock> scheduledBlocks, bool isPersonalScheduleTemplate)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();
            if (shiftsToEvalute.IsNullOrEmpty())
                return evaluateResults;

            #region Prereq

            TimeSchedulePlanningDayDTO first = shiftsToEvalute.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvalute.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            if (!isPersonalScheduleTemplate && GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningRuleWorkTimeWeekDontEvaluateInSchedule))
                return evaluateResults;

            int maxScheduleTimeFullTime = employeeGroup.MaxScheduleTimeFullTime;
            int maxScheduleTimePartTime = employeeGroup.MaxScheduleTimePartTime;
            int minScheduleTimeFullTime = employeeGroup.MinScheduleTimeFullTime;
            int minScheduleTimePartTime = employeeGroup.MinScheduleTimePartTime;
            if (minScheduleTimeFullTime == 0 && minScheduleTimePartTime == 0 && maxScheduleTimePartTime == 0 && maxScheduleTimeFullTime == 0)
                return evaluateResults;

            int ruleWorkTimeWeek = employment != null ? employment.GetWorkTimeWeek(intervalFromDate) : employeeGroup.RuleWorkTimeWeek;
            if (ruleWorkTimeWeek == 0)
                return evaluateResults;

            var startInterval = ruleWorkTimeWeek;
            var stopInterval = ruleWorkTimeWeek;
            var fullTimeMinutes = employment != null ? employment.GetFullTimeWorkTimeWeek(employeeGroup, intervalFromDate) : employeeGroup.RuleWorkTimeWeek;

            bool isPartTime = ruleWorkTimeWeek < fullTimeMinutes;
            if (isPartTime)
            {
                startInterval += minScheduleTimePartTime;
                stopInterval += maxScheduleTimePartTime;
            }
            else
            {
                startInterval += minScheduleTimeFullTime;
                stopInterval += maxScheduleTimeFullTime;
            }

            List<TimeScheduleTypeDTO> scheduleTypes = GetTimeScheduleTypesWithFactorFromCache().ToDTOs(true).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduleBlocksForWeek (Unsaved changes have precedence)

            foreach (var item in shiftsToEvalute)
            {
                if (item.TimeScheduleTemplateBlockId == 0)
                    continue;

                #region Remove scheduleblock

                var scheduledBlock = scheduledBlocks.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == item.TimeScheduleTemplateBlockId);
                if (scheduledBlock != null)
                {
                    scheduledBlocks.Remove(scheduledBlock);

                    #region Remove breaks

                    //Remove all breaks on the day because shiftsToEvalute contains all breaks for the day
                    if (scheduledBlock.TimeScheduleEmployeePeriodId.HasValue)
                    {
                        var breaks = scheduledBlocks.Where(x => x.IsBreak && x.TimeScheduleEmployeePeriodId == scheduledBlock.TimeScheduleEmployeePeriodId.Value).ToList();
                        foreach (var breakBlock in breaks)
                        {
                            scheduledBlocks.Remove(breakBlock);
                        }
                    }

                    #endregion
                }

                #endregion
            }

            #endregion

            #region Evaluate

            while (intervalFromDate <= intervalToDate)
            {
                #region Calculate

                int plannedWeekTime = 0;
                int totalWeekWorkMinutes = 0;

                DateTime firstDayOfWeek = CalendarUtility.GetBeginningOfDay(CalendarUtility.GetFirstDateOfWeek(intervalFromDate));
                DateTime lastDayOfWeek = CalendarUtility.GetEndOfDay(CalendarUtility.GetLastDateOfWeek(intervalFromDate));
                List<TimeSchedulePlanningDayDTO> plannedShiftsForWeek = shiftsToEvalute.Where(s => s.StartTime.Date >= firstDayOfWeek && s.StartTime.Date <= lastDayOfWeek).ToList();
                List<TimeScheduleTemplateBlock> scheduleBlocksForWeek = scheduledBlocks.Where(s => s.Date.HasValue && s.Date.Value.Date >= firstDayOfWeek && s.Date.Value.Date <= lastDayOfWeek).ToList();

                //Calculate planned time for week
                plannedWeekTime = plannedShiftsForWeek.GetWorkMinutes(scheduleTypes);

                //Calculate scheduled work minutes
                totalWeekWorkMinutes = scheduleBlocksForWeek.GetWorkMinutes(scheduleTypes);

                #endregion

                #region Result

                var minutes = totalWeekWorkMinutes + plannedWeekTime;
                if (minutes < startInterval || minutes > stopInterval)
                {
                    string start = CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(startInterval), false, false);
                    string stop = CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(stopInterval), false, false);
                    string interval = $"{start}-{stop}";
                    string timePlusMinus = string.Format("(+{0}/{1})",
                            isPartTime ? CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(maxScheduleTimePartTime), false, false) : CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(maxScheduleTimeFullTime), false, false),
                            isPartTime ? CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(minScheduleTimePartTime), false, false) : CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(minScheduleTimeFullTime), false, false));

                    string errormsg = string.Format(GetText(8276, "Antal arbetstimmar per vecka kommer under perioden {0} - {1} att hamna utanför intervallet {2} {3} för anställd {4} ({5})"), firstDayOfWeek.ToShortDateString(), lastDayOfWeek.ToShortDateString(), interval, timePlusMinus, name, CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(minutes), false, false));
                    var evaluateResult = new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleWorkTimeWeekMaxMinReached, errormsg, name, intervalFromDate)
                    {
                        WorkTimeReachedDateFrom = firstDayOfWeek,
                        WorkTimeReachedDateTo = lastDayOfWeek
                    };
                    evaluateResults.Add(evaluateResult);
                }

                #endregion

                intervalFromDate = lastDayOfWeek.AddDays(1); //force the loop to change week 
            }

            #endregion

            return evaluateResults;
        }

        private List<EvaluateWorkRuleResultDTO> EvaluateRuleWorkTimeDay(string name, EmployeeGroup employeeGroup, List<TimeSchedulePlanningDayDTO> shiftsToEvaluate, List<TimeScheduleTemplateBlock> scheduledBlocks)
        {
            List<EvaluateWorkRuleResultDTO> evaluateResults = new List<EvaluateWorkRuleResultDTO>();

            #region Prereq

            TimeSchedulePlanningDayDTO first = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).FirstOrDefault();
            TimeSchedulePlanningDayDTO last = shiftsToEvaluate.OrderBy(s => s.StartTime.Date).LastOrDefault();
            if (first == null || last == null)
                return evaluateResults;

            DateTime intervalFromDate = first.StartTime.Date;
            DateTime intervalToDate = last.StartTime.Date;

            List<TimeScheduleTypeDTO> scheduleTypes = GetTimeScheduleTypesWithFactorFromCache().ToDTOs(true).ToList();

            #endregion

            #region Remove shiftsToEvalute from scheduleBlocksForWeek (Unsaved changes have precedence)

            RemovePlannedShiftsFromScheduledBlocks(shiftsToEvaluate, scheduledBlocks, true);

            #endregion

            if (employeeGroup.RuleWorkTimeDayMinimum > 0 || employeeGroup.RuleWorkTimeDayMaximumWeekend > 0 || employeeGroup.RuleWorkTimeDayMaximumWorkDay > 0)
            {
                DateTime currentDayDate = intervalFromDate;
                while (currentDayDate <= intervalToDate)
                {
                    //Get Current day data                
                    List<TimeSchedulePlanningDayDTO> currentDayPlannedShifts = shiftsToEvaluate.Where(b => b.StartTime.Date == currentDayDate).OrderBy(b => b.StartTime).ToList();
                    List<TimeScheduleTemplateBlock> currentDayScheduledShifts = scheduledBlocks.Where(b => b.Date.HasValue && b.Date.Value == currentDayDate).OrderBy(b => b.StartTime).ToList();
                    int plannedTime = currentDayPlannedShifts.GetWorkMinutes(scheduleTypes);
                    int scheduledTime = currentDayScheduledShifts.GetWorkMinutes(scheduleTypes);
                    int totalWorkTime = scheduledTime + plannedTime;

                    if (employeeGroup.RuleWorkTimeDayMinimum > 0 && totalWorkTime > 0 && totalWorkTime < employeeGroup.RuleWorkTimeDayMinimum)
                    {
                        #region Evaluate minimum rule

                        String errormsg = String.Format(GetText(91890, "Minimum arbetstid per dag ({0}), {1}, ej uppnådd för {2}."), CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(employeeGroup.RuleWorkTimeDayMinimum), false, false, false), currentDayDate.ToShortDateString(), name);
                        evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleWorkTimeDayMinimumNotReached, errormsg, name, currentDayDate));

                        #endregion
                    }

                    if (employeeGroup.RuleWorkTimeDayMaximumWorkDay > 0 && !currentDayDate.IsWeekendDay() && totalWorkTime > 0 && totalWorkTime > employeeGroup.RuleWorkTimeDayMaximumWorkDay)
                    {
                        #region Evaluate maximum rule work day

                        String errormsg = String.Format(GetText(8816, "Maximal arbetstid per dag ({0}), {1}, kommer att överstigas för {2}."), CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(employeeGroup.RuleWorkTimeDayMaximumWorkDay), false, false, false), currentDayDate.ToShortDateString(), name);
                        evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleWorkTimeDayMaximumWorkDayViolated, errormsg, name, currentDayDate));

                        #endregion
                    }

                    if (employeeGroup.RuleWorkTimeDayMaximumWeekend > 0 && currentDayDate.IsWeekendDay() && totalWorkTime > 0 && totalWorkTime > employeeGroup.RuleWorkTimeDayMaximumWeekend)
                    {
                        #region Evaluate maximum rule weekend

                        String errormsg = String.Format(GetText(8817, "Maximal arbetstid per helgdag ({0}), {1}, kommer att överstigas för {2}."), CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(employeeGroup.RuleWorkTimeDayMaximumWeekend), false, false, false), currentDayDate.ToShortDateString(), name);
                        evaluateResults.Add(new EvaluateWorkRuleResultDTO((int)ActionResultSave.RuleWorkTimeDayMaximumWeekendViolated, errormsg, name, currentDayDate));

                        #endregion
                    }
                    currentDayDate = currentDayDate.AddDays(1);
                }
            }

            return evaluateResults;
        }

        #endregion

        #region Help methods

        private void RemoveWorkRuleResultsSupersededByMinor(List<EvaluateWorkRuleResultDTO> evaluateResults)
        {
            // These rule-breaks shouldn't show if employee is still minor
            evaluateResults.RemoveAll(r =>
                r.EvaluatedWorkRule == SoeScheduleWorkRules.RestDay &&
                evaluateResults.Any(m =>
                    m.EvaluatedWorkRule == SoeScheduleWorkRules.MinorsRestDay &&
                    m.EmployeeId == r.EmployeeId &&
                    m.Date?.Date == r.Date?.Date
                )
                ||
                r.EvaluatedWorkRule == SoeScheduleWorkRules.RestWeek &&
                evaluateResults.Any(m =>
                    m.EvaluatedWorkRule == SoeScheduleWorkRules.MinorsRestWeek &&
                    m.EmployeeId == r.EmployeeId &&
                    m.Date?.Date == r.Date?.Date
                )
                ||
                r.EvaluatedWorkRule == SoeScheduleWorkRules.WorkTimeDay &&
                evaluateResults.Any(m =>
                    m.EvaluatedWorkRule == SoeScheduleWorkRules.MinorsWorkTimeDay &&
                    m.EmployeeId == r.EmployeeId &&
                    m.Date?.Date == r.Date?.Date
                )
                ||
                r.EvaluatedWorkRule == SoeScheduleWorkRules.Breaks &&
                evaluateResults.Any(m =>
                    m.EvaluatedWorkRule == SoeScheduleWorkRules.MinorsBreaks &&
                    m.EmployeeId == r.EmployeeId &&
                    m.Date?.Date == r.Date?.Date
                )
                ||
                r.EvaluatedWorkRule == (SoeScheduleWorkRules.WorkTimeWeekMaxMin) &&
                evaluateResults.Any(m =>
                    m.EvaluatedWorkRule == SoeScheduleWorkRules.MinorsWorkTimeWeek &&
                    m.EmployeeId == r.EmployeeId &&
                    m.Date?.Date == r.Date?.Date
                )
                ||
                r.EvaluatedWorkRule == (SoeScheduleWorkRules.WorkTimeWeekPartTimeWorkers) &&
                evaluateResults.Any(m =>
                    m.EvaluatedWorkRule == SoeScheduleWorkRules.MinorsWorkTimeWeek &&
                    m.EmployeeId == r.EmployeeId &&
                    m.Date?.Date == r.Date?.Date
                )
            );
        }

        private List<(DateTime DayStart, DateTime DayStop)> GetDaysForRestTimeDay(EmployeeGroup employeeGroup, List<DateTime> plannedDays, Employee employee = null)
        {
            DateTime defaultStartTime = employeeGroup.GetRuleRestTimeDayStartTime();

            List<EmployeeSetting> employeeSettings = employee != null ? EmployeeManager.GetEmployeeSettings(entities, actorCompanyId, employee.EmployeeId, plannedDays.OrderBy(x => x).FirstOrDefault(), plannedDays.OrderBy(x => x).LastOrDefault(), TermGroup_EmployeeSettingType.WorkTimeRule, TermGroup_EmployeeSettingType.WorkTimeRule_DailyRest) : new List<EmployeeSetting>();

            List<(DateTime DayStart, DateTime DayStop)> days = new List<(DateTime DayStart, DateTime DayStop)>();
            int blockSize = 1;

            foreach (var date in plannedDays)
            {
                DateTime restTimeDayStartTimeForCurrentDate = employee != null ? employee.GetRestDayStartTime(employeeGroup, employeeSettings, date) : defaultStartTime;
                DateTime plannedDay = CalendarUtility.MergeDateAndTime(date, restTimeDayStartTimeForCurrentDate);
                DateTime dateBefore = plannedDay.AddDays(-blockSize);
                DateTime dateAfter = plannedDay.AddDays(blockSize);

                if (!days.Any(x => x.DayStart == dateBefore && x.DayStop == plannedDay))
                    days.Add((dateBefore, plannedDay));

                if (!days.Any(x => x.DayStart == plannedDay && x.DayStop == dateAfter))
                    days.Add((plannedDay, dateAfter));
            }

            return days;
        }

        private List<(DateTime WeekStart, DateTime WeekStop)> GetWeeksForRestTimeWeek(EmployeeGroup employeeGroup, List<DateTime> plannedDays, Employee employee = null)
        {
            DayOfWeek defaultWeekStart = employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek();
            DateTime defaultStartTime = employeeGroup.GetRuleRestTimeWeekStartTime();

            List<EmployeeSetting> employeeSettings = employee != null ? EmployeeManager.GetEmployeeSettings(entities, actorCompanyId, employee.EmployeeId, plannedDays.OrderBy(x => x).FirstOrDefault(), plannedDays.OrderBy(x => x).LastOrDefault(), TermGroup_EmployeeSettingType.WorkTimeRule, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest) : new List<EmployeeSetting>();

            List<(DateTime WeekStart, DateTime WeekStop)> weeks = new List<(DateTime weekStart, DateTime weekStop)>();
            int blockSize = 7;

            foreach (var date in plannedDays)
            {
                DayOfWeek restTimeWeekWeekDayStartForCurrentDate = employee != null ? employee.GetRestTimeWeekWeekDayStart(employeeGroup, employeeSettings, date) : defaultWeekStart;
                DateTime restTimeWeektStartTimeForCurrentDate = employee != null ? employee.GetRestTimeWeektStartTime(employeeGroup, employeeSettings, date) : defaultStartTime;
                DateTime dateWeekFirst;
                DateTime dateWeekLast;

                if (date.DayOfWeek == restTimeWeekWeekDayStartForCurrentDate)
                {
                    dateWeekFirst = date;
                }
                else
                {
                    int currentDateDayNbr = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
                    int restTimeWeekWeekDayStartDayNbr = restTimeWeekWeekDayStartForCurrentDate == DayOfWeek.Sunday ? 7 : (int)restTimeWeekWeekDayStartForCurrentDate;
                    int dayDiff = restTimeWeekWeekDayStartDayNbr - currentDateDayNbr;
                    if (restTimeWeekWeekDayStartDayNbr > currentDateDayNbr)
                        dayDiff += -blockSize;

                    dateWeekFirst = date.AddDays(dayDiff);
                }

                dateWeekFirst = CalendarUtility.MergeDateAndTime(dateWeekFirst, restTimeWeektStartTimeForCurrentDate);
                dateWeekLast = dateWeekFirst.AddDays(blockSize);
                //if (restTimeWeektStartTimeForCurrentDate.TimeOfDay.TotalMinutes == 0)
                //    dateWeekLast = CalendarUtility.GetEndOfDay(dateWeekLast.AddDays(-1));

                if (!weeks.Any(x => x.WeekStart == dateWeekFirst && x.WeekStop == dateWeekLast))
                    weeks.Add((dateWeekFirst, dateWeekLast));
            }

            return weeks;
        }

        private void mergePlannedShiftsIntoScheduledBlocks(List<TimeSchedulePlanningDayDTO> plannedShifts, List<TimeScheduleTemplateBlock> scheduledBlocks)
        {
            foreach (var plannedShift in plannedShifts)
            {
                if (plannedShift.TimeScheduleTemplateBlockId == 0 && !plannedShift.SourceTimeScheduleTemplateBlockId.HasValue)
                    AddPlannedShiftIntoScheduledBlocks(plannedShift, scheduledBlocks);
            }
        }

        private void RemovePlannedShiftsFromScheduledBlocks(List<TimeSchedulePlanningDayDTO> plannedShifts, List<TimeScheduleTemplateBlock> scheduledBlocks, bool removeBreaks, bool addMissing = false)
        {
            foreach (var plannedShift in plannedShifts)
            {
                if (plannedShift.TimeScheduleTemplateBlockId == 0 && !plannedShift.SourceTimeScheduleTemplateBlockId.HasValue)
                {
                    if (addMissing)
                    {
                        AddPlannedShiftIntoScheduledBlocks(plannedShift, scheduledBlocks);
                    }
                    continue;
                }

                #region Remove scheduleblock

                var scheduledBlock = scheduledBlocks.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == plannedShift.TimeScheduleTemplateBlockId);
                if (scheduledBlock == null && plannedShift.SourceTimeScheduleTemplateBlockId.HasValue)
                    scheduledBlock = scheduledBlocks.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == plannedShift.SourceTimeScheduleTemplateBlockId.Value);

                if (scheduledBlock != null)
                {
                    scheduledBlocks.Remove(scheduledBlock);

                    #region Remove breaks

                    //Remove all breaks on the day because shiftsToEvalute contains all breaks for the day
                    if (removeBreaks && scheduledBlock.TimeScheduleEmployeePeriodId.HasValue)
                    {
                        var breaks = scheduledBlocks.Where(x => x.IsBreak && x.TimeScheduleEmployeePeriodId == scheduledBlock.TimeScheduleEmployeePeriodId.Value).ToList();
                        foreach (var breakBlock in breaks)
                        {
                            scheduledBlocks.Remove(breakBlock);
                        }
                    }

                    #endregion
                }
                
                #endregion
            }
        }

        private TimeScheduleTemplateBlock TimeScheduleTemplateBlockFromTimeSchedulePlanningDayDTO(int timeScheduleTemplateBlockId, DateTime startTime, DateTime stopTime, int timeScheduleTypeId, int timeScheduleEmployeePeriodId, int? timeDeviationCauseId, int employeeId, bool isBreak, TermGroup_TimeScheduleTemplateBlockAbsenceType absenceType)
        {
            return new TimeScheduleTemplateBlock()
            {
                TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId,
                StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, startTime),
                StopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, stopTime),
                TimeScheduleTypeId = timeScheduleTypeId,
                TimeScheduleEmployeePeriodId = timeScheduleEmployeePeriodId,
                TimeDeviationCauseId = timeDeviationCauseId,
                Date = startTime.Date,
                EmployeeId = employeeId,
                BreakType = isBreak ? (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak : (int)SoeTimeScheduleTemplateBlockBreakType.None,
                AbsenceType = (int)absenceType,
            };
        }

        private void AddPlannedShiftIntoScheduledBlocks(TimeSchedulePlanningDayDTO shift, List<TimeScheduleTemplateBlock> planningPeriodScheduledBlocks)
        {  
            //if (shift.TimeScheduleTemplateBlockId == 0)
            //    continue;
            var scheduledBlock = planningPeriodScheduledBlocks.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == shift.TimeScheduleTemplateBlockId);
            if (scheduledBlock == null && shift.SourceTimeScheduleTemplateBlockId.HasValue)
                scheduledBlock = planningPeriodScheduledBlocks.FirstOrDefault(s => s.TimeScheduleTemplateBlockId == shift.SourceTimeScheduleTemplateBlockId.Value);
            if (scheduledBlock == null)
                scheduledBlock = planningPeriodScheduledBlocks.FirstOrDefault(s => s.ActualStartTime == CalendarUtility.MergeDateAndTime(shift.ActualDate, shift.StartTime) && s.TotalMinutes == shift.NetTime);
            if (scheduledBlock == null && shift.StartTime != shift.StopTime)
            {
                planningPeriodScheduledBlocks.Add(TimeScheduleTemplateBlockFromTimeSchedulePlanningDayDTO(shift.TimeScheduleTemplateBlockId, shift.StartTime, shift.StopTime, shift.TimeScheduleTypeId, shift.TimeScheduleEmployeePeriodId, shift.TimeDeviationCauseId, shift.EmployeeId, false, shift.AbsenceType));

                // Add breaks
                if (shift.Break1Minutes > 0)
                    planningPeriodScheduledBlocks.Add(TimeScheduleTemplateBlockFromTimeSchedulePlanningDayDTO(0, shift.Break1StartTime, shift.Break1StartTime.AddMinutes(shift.Break1Minutes), shift.TimeScheduleTypeId, shift.TimeScheduleEmployeePeriodId, shift.TimeDeviationCauseId, shift.EmployeeId, true, shift.AbsenceType));
                if (shift.Break2Minutes > 0)
                    planningPeriodScheduledBlocks.Add(TimeScheduleTemplateBlockFromTimeSchedulePlanningDayDTO(0, shift.Break2StartTime, shift.Break2StartTime.AddMinutes(shift.Break2Minutes), shift.TimeScheduleTypeId, shift.TimeScheduleEmployeePeriodId, shift.TimeDeviationCauseId, shift.EmployeeId, true, shift.AbsenceType));
                if (shift.Break3Minutes > 0)
                    planningPeriodScheduledBlocks.Add(TimeScheduleTemplateBlockFromTimeSchedulePlanningDayDTO(0, shift.Break3StartTime, shift.Break3StartTime.AddMinutes(shift.Break3Minutes), shift.TimeScheduleTypeId, shift.TimeScheduleEmployeePeriodId, shift.TimeDeviationCauseId, shift.EmployeeId, true, shift.AbsenceType));
                if (shift.Break4Minutes > 0)
                    planningPeriodScheduledBlocks.Add(TimeScheduleTemplateBlockFromTimeSchedulePlanningDayDTO(0, shift.Break4StartTime, shift.Break4StartTime.AddMinutes(shift.Break4Minutes), shift.TimeScheduleTypeId, shift.TimeScheduleEmployeePeriodId, shift.TimeDeviationCauseId, shift.EmployeeId, true, shift.AbsenceType));
            }
        }

        private bool PeriodExceedsWorkingdays(DateTime periodStartDate, DateTime periodStopDate, List<TimeScheduleTemplateBlock> periodScheduledBlocks, int limit)
        {
            bool workingdaysExceeded = false;
            periodScheduledBlocks = periodScheduledBlocks.Where(x => !x.IsBreak && x.StartTime != x.StopTime && x.ActualStartTime >= periodStartDate && x.ActualStartTime <= CalendarUtility.GetEndOfDay(periodStopDate)).ToList();
            List<DateTime> scheduledDays = periodScheduledBlocks.Select(y => y.ActualStartTime.Value.Date).Distinct().ToList();

            if (scheduledDays.Count > limit)
                workingdaysExceeded = true;

            return workingdaysExceeded;
        }

        private void GetMinorsSchoolDayMinutes(out int minorsSchoolDayStartMinutes, out int minorsSchoolDayStopMinutes)
        {
            minorsSchoolDayStartMinutes = GetCompanyIntSettingFromCache(CompanySettingType.MinorsSchoolDayStartMinutes);
            if (minorsSchoolDayStartMinutes == 0)
                minorsSchoolDayStartMinutes = 8 * 60; //default 8 (if changed, also change instructiontext in setting page)
            minorsSchoolDayStopMinutes = GetCompanyIntSettingFromCache(CompanySettingType.MinorsSchoolDayStopMinutes);
            if (minorsSchoolDayStopMinutes == 0)
                minorsSchoolDayStopMinutes = 16 * 60; //default 16 (if changed, also change instructiontext in setting page)
        }

        private bool IsAloneAtAnyTime(EmployeeAgeDTO minor, List<EmployeeAgeDTO> seniors, List<TimeScheduleTemplateBlock> scheduleBlocksMinor, List<TimeScheduleTemplateBlock> companyScheduleBlocks, List<TimeSchedulePlanningDayDTO> shiftsInput, DateTime date, int hiddenEmployeeId, out string invalidWorkInterval)
        {
            bool isAloneAtAnyTime = false;
            invalidWorkInterval = String.Empty;

            if (seniors.IsNullOrEmpty())
                return true;

            if (minor != null && minor.EmployeeId != hiddenEmployeeId)
                companyScheduleBlocks = companyScheduleBlocks.ExcludeEmployeeId(hiddenEmployeeId);

            List<TimeScheduleTemplateBlock> scheduleBlocksMinorForDate = scheduleBlocksMinor.Where(b => b.Date.HasValue && b.Date.Value == date).OrderBy(b => b.StartTime).ToList();
            if (!scheduleBlocksMinorForDate.Any())
                return false;

            List<int> seniorEmployeeIdsWithEmployment = seniors.WithEmployment(date).Select(i => i.EmployeeId).ToList();
            List<TimeScheduleTemplateBlock> scheduleBlocksSeniorsForDate = companyScheduleBlocks.Where(i => i.Date == date && i.EmployeeId.HasValue && seniorEmployeeIdsWithEmployment.Contains(i.EmployeeId.Value)).ToList();
            List<TimeSchedulePlanningDayDTO> shiftsInputForDate = shiftsInput.Where(b => b.StartTime.Date == date).OrderBy(b => b.StartTime).ToList();
            List<WorkIntervalDTO> scheduleWorkIntervals = scheduleBlocksMinorForDate.GetWorkIntervals(hiddenEmployeeId);
            List<WorkIntervalDTO> otherEmployeeShiftWorkIntervals = shiftsInputForDate.GetWorkIntervals();
            List<WorkIntervalDTO> otherEmployeeScheduleWorkIntervals = scheduleBlocksSeniorsForDate.GetWorkIntervals(hiddenEmployeeId);
            List<WorkIntervalDTO> otherWorkIntervals = CalendarUtility.MergeIntervals(otherEmployeeShiftWorkIntervals, otherEmployeeScheduleWorkIntervals);

            foreach (WorkIntervalDTO scheduleWorkInterval in scheduleWorkIntervals)
            {
                DateTime currentTime = scheduleWorkInterval.StartTime;
                while (currentTime < scheduleWorkInterval.StopTime && !isAloneAtAnyTime)
                {
                    //Find workinterval that elapse longest
                    WorkIntervalDTO otherWorkInterval = otherWorkIntervals.Where(i => i.StartTime <= currentTime && i.StopTime > currentTime).OrderByDescending(i => i.StopTime).FirstOrDefault();
                    if (otherWorkInterval != null)
                    {
                        currentTime = otherWorkInterval.StopTime;
                    }
                    else
                    {
                        isAloneAtAnyTime = true;
                        WorkIntervalDTO nextWorkIterval = otherWorkIntervals.Where(i => i.StartTime > currentTime).OrderBy(i => i.StartTime).FirstOrDefault();
                        DateTime nextWorkIntervalStartTime = nextWorkIterval != null ? nextWorkIterval.StartTime : scheduleWorkInterval.StopTime;
                        invalidWorkInterval = String.Format("{0} {1}-{2}", date.ToShortDateString(), currentTime.ToString("HH:mm"), nextWorkIntervalStartTime.ToString("HH:mm"));
                    }
                }

                if (isAloneAtAnyTime)
                    break;
            }

            return isAloneAtAnyTime;
        }

        private bool IsAloneAtAnyTime(EmployeeAgeDTO minor, List<EmployeeAgeDTO> seniors, List<TimeScheduleTemplateBlock> scheduleBlocksMinor, List<TimeScheduleTemplateBlock> companyScheduleBlocks, List<TimeSchedulePlanningDayDTO> shiftsInput, DateTime date, out string invalidWorkInterval)
        {
            bool isAloneAtAnyTime = false;
            invalidWorkInterval = String.Empty;

            if (seniors.IsNullOrEmpty())
                return true;

            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();
            if (minor != null && minor.EmployeeId != hiddenEmployeeId)
                companyScheduleBlocks = companyScheduleBlocks.ExcludeEmployeeId(hiddenEmployeeId);
            shiftsInput = shiftsInput.ExcludeEmployeeId(GetHiddenEmployeeIdFromCache());

            List<TimeSchedulePlanningDayDTO> shiftsInputForDate = shiftsInput.Where(b => b.StartTime.Date == date).OrderBy(b => b.StartTime).ToList();
            List<TimeScheduleTemplateBlock> scheduleBlocksForDate = scheduleBlocksMinor.Where(b => b.Date.HasValue && b.Date.Value == date).OrderBy(b => b.StartTime).ToList();
            if (!shiftsInputForDate.Any() && !scheduleBlocksForDate.Any())
                return false;

            List<int> seniorEmployeeIdsWithEmployment = seniors.WithEmployment(date).Select(i => i.EmployeeId).ToList();
            List<TimeScheduleTemplateBlock> scheduleBlocksSeniorsForDate = companyScheduleBlocks.Where(i => i.Date == date && i.EmployeeId.HasValue && seniorEmployeeIdsWithEmployment.Contains(i.EmployeeId.Value)).ToList();
            List<WorkIntervalDTO> shiftWorkIntervals = shiftsInputForDate.GetWorkIntervals();
            List<WorkIntervalDTO> scheduleWorkIntervals = scheduleBlocksForDate.GetWorkIntervals(hiddenEmployeeId);
            List<WorkIntervalDTO> workIntervals = CalendarUtility.MergeIntervals(shiftWorkIntervals, scheduleWorkIntervals);
            List<WorkIntervalDTO> otherWorkIntervals = scheduleBlocksSeniorsForDate.GetWorkIntervals(hiddenEmployeeId);

            foreach (WorkIntervalDTO workInterval in workIntervals)
            {
                DateTime currentTime = workInterval.StartTime;
                while (currentTime < workInterval.StopTime && !isAloneAtAnyTime)
                {
                    //Find workinterval that elapse longest
                    WorkIntervalDTO otherWorkInterval = otherWorkIntervals.Where(i => i.StartTime <= currentTime && i.StopTime > currentTime).OrderByDescending(i => i.StopTime).FirstOrDefault();
                    if (otherWorkInterval != null)
                    {
                        currentTime = otherWorkInterval.StopTime;
                    }
                    else
                    {
                        isAloneAtAnyTime = true;
                        WorkIntervalDTO nextWorkIterval = otherWorkIntervals.Where(i => i.StartTime > currentTime).OrderBy(i => i.StartTime).FirstOrDefault();
                        DateTime nextWorkIntervalStartTime = nextWorkIterval != null ? nextWorkIterval.StartTime : workInterval.StopTime;
                        invalidWorkInterval = String.Format("{0} {1}-{2}", date.ToShortDateString(), currentTime.ToString("HH:mm"), nextWorkIntervalStartTime.ToString("HH:mm"));
                    }
                }

                if (isAloneAtAnyTime)
                    break;
            }

            return isAloneAtAnyTime;
        }

        private bool IsEmployeeYoungerThan18(Employee employee, DateTime? date = null)
        {
            if (employee == null)
                return false;

            DateTime? birthDate = GetEmployeeBirthDateFromCache(employee);
            if (birthDate.HasValue)
                return CalendarUtility.IsAgeYoungerThan18(birthDate.Value, date);
            return false;
        }

        private bool IsAgeBetween16To18(Employee employee, DateTime? date = null)
        {
            if (employee == null)
                return false;

            DateTime? birthDate = GetEmployeeBirthDateFromCache(employee);
            if (birthDate.HasValue)
                return CalendarUtility.IsAgeBetween16To18(birthDate.Value, date);
            return false;
        }

        private bool IsAgeBetween13To15(Employee employee, DateTime? date = null)
        {
            if (employee == null)
                return false;

            DateTime? birthDate = GetEmployeeBirthDateFromCache(employee);
            if (birthDate.HasValue)
                return CalendarUtility.IsAgeBetween13To15(birthDate.Value, date);
            return false;
        }

        private bool IsAgeYoungerThan13(Employee employee, DateTime? date = null)
        {
            if (employee == null)
                return false;

            DateTime? birthDate = GetEmployeeBirthDateFromCache(employee);
            if (birthDate.HasValue)
                return CalendarUtility.IsAgeYoungerThan13(birthDate.Value, date);
            return false;
        }

        #endregion

        #endregion
    }
}
