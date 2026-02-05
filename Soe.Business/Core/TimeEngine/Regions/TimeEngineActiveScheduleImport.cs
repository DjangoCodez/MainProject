using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
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
        /// Import schedules from ActiveScheduleImport
        /// </summary>
        /// <returns></returns>
        private EmployeeActiveScheduleImportOutputDTO TaskEmployeeActiveScheduleImport()
        {
            var (iDTO, oDTO) = InitTask<EmployeeActiveScheduleImportInputDTO, EmployeeActiveScheduleImportOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.EmployeeActiveSchedules.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeActiveSchedules");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                TimeScheduleTemplateHead templateHead = TimeScheduleManager.GetCompanyZeroTemplateHead(entities, ActorCompanyId);
                if (templateHead == null)
                {
                    int timeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
                    if (timeCodeId == 0)
                        return new EmployeeActiveScheduleImportOutputDTO() { Result = new ActionResult(GetText(12102, "Företagsinställning för standard tidkod saknas")) };

                    templateHead = CreateEmptyTimeScheduleTemplateHead(CalendarUtility.GetFirstDateOfWeek(CalendarUtility.GetFirstDateOfYear().AddYears(-1)), timeCodeId);
                    if (templateHead == null)
                        return new EmployeeActiveScheduleImportOutputDTO() { Result = new ActionResult(GetText(12103, "En tom schemamall kunde inte hittas eller skapas")) };
                }

                var employeesByNumbers = EmployeeManager.GetAllEmployeesByNumbers(entities, actorCompanyId, iDTO.EmployeeActiveSchedules.Select(s => s.EmployeeNr ?? "").Distinct().ToList());
                var employeesByExternalCode = EmployeeManager.GetAllEmployeesByExternalCodes(entities, actorCompanyId, iDTO.EmployeeActiveSchedules.Select(s => s.EmployeeExternalCode ?? "").Distinct().ToList());

                List<int> employeeIds = employeesByNumbers
                    .Select(s => s.EmployeeId)
                    .Concat(employeesByExternalCode.Select(s => s.EmployeeId))
                    .Distinct()
                    .ToList();                

                employeesByNumbers = GetEmployeesWithEmployment(employeeIds, true);

                var timeCodeBreaks = TimeCodeManager.GetTimeCodes(taskEntities, actorCompanyId, SoeTimeCodeType.Break, loadPayrollProducts: false).OfType<TimeCodeBreak>().ToList();
                var timeCodeBreakGroups = TimeCodeManager.GetTimeCodeBreakGroups(taskEntities, actorCompanyId);
                var startDate = iDTO.EmployeeActiveSchedules.Min(m => m.ActiveScheduleIntervals.Min(mi => mi.StartDate));
                var stopDate = iDTO.EmployeeActiveSchedules.Max(m => m.ActiveScheduleIntervals.Max(mi => mi.StopDate));
                var existingScheduleByEmployee = GetScheduleBlocksForEmployeesWithTask(null, employeeIds, startDate, stopDate)
                    .Where(x => x.EmployeeId.HasValue)
                    .GroupBy(g => g.EmployeeId.Value)
                    .ToDictionary(k => k.Key, v => v.ToList());

                AddEmployeeSchedulesToCache(GetEmployeeSchedules(employeeIds));
                AddTimeScheduleTemplateHeadsToCache(GetPersonalTemplateHeads(employeeIds));

                #endregion

                #region Process

                try
                {
                    foreach (int employeeId in employeeIds)
                    {
                        using (TransactionScope transaction = CreateTransactionScope(TimeSpan.FromSeconds(1000), System.Transactions.IsolationLevel.ReadCommitted))
                        {
                            InitTransaction(transaction);

                            var employee = employeesByNumbers.FirstOrDefault(f => f.EmployeeId == employeeId);
                            if (employee == null)
                                continue;

                            var activeEmployeeSchedules = iDTO.EmployeeActiveSchedules.Where(f => f.EmployeeNr == employee.EmployeeNr || f.EmployeeExternalCode == employee.ExternalCode).ToList();
                            if (activeEmployeeSchedules.IsNullOrEmpty())
                                continue;

                            foreach (var activeEmployeeSchedule in activeEmployeeSchedules)
                            {
                                foreach (var interval in activeEmployeeSchedule.ActiveScheduleIntervals)
                                {
                                    oDTO.Result = ValidateDates(interval, out DateTime dateFrom, out DateTime dateTo);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    var scheduleDays = interval.ActiveScheduleDays;

                                    var firstEmployment = employee.GetFirstEmployment();
                                    if (firstEmployment.DateFrom > dateFrom)
                                        scheduleDays = scheduleDays.Where(w => w.Date >= firstEmployment.DateFrom).ToList();

                                    var (placementsFromImport, scheduleBlocksFromImport) = CreateEmployeeScheduleFromActiveScheduleImport(employee, templateHead, scheduleDays, dateFrom, dateTo, timeCodeBreaks, timeCodeBreakGroups);
                                    if (firstEmployment.DateFrom > dateFrom)
                                        scheduleBlocksFromImport = scheduleBlocksFromImport.Where(w => w.Date >= firstEmployment.DateFrom).ToList();

                                    oDTO.Result = DeleteActiveScheduleImportOverlappingSchedule(employeeId, existingScheduleByEmployee, scheduleBlocksFromImport);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    oDTO.Result = SaveActiveScheduleImportPlacement(placementsFromImport);
                                    if (!oDTO.Result.Success)
                                        return oDTO;

                                    //Must be done after placement is finished
                                    AddTimeScheduleTemplatePeriodsToCache(employeeId, dateFrom, dateTo);

                                    oDTO.Result = SaveActiveScheduleImportSchedules(scheduleBlocksFromImport);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                            }

                            TryCommit(oDTO);
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

                #endregion
            }

            return oDTO;
        }

        #endregion

        #region Create schedule from ActiveScheduleImport

        private (List<SaveEmployeeSchedulePlacementItem> placements, List<TimeScheduleTemplateBlockDTO> scheduleBlocks) CreateEmployeeScheduleFromActiveScheduleImport(Employee employee, TimeScheduleTemplateHead templateHead, List<ActiveScheduleDay> activeScheduleDays, DateTime startDate, DateTime stopDate, List<TimeCodeBreak> timeCodeBreaks, List<TimeCodeBreakGroup> timeCodeBreakGroups)
        {
            List<SaveEmployeeSchedulePlacementItem> placements = new List<SaveEmployeeSchedulePlacementItem>();
            List<TimeScheduleTemplateBlockDTO> scheduleBlocks = new List<TimeScheduleTemplateBlockDTO>();

            if (employee == null || templateHead == null)
                return (placements, scheduleBlocks);

            var employeeSchedules = GetEmployeeSchedulesForEmployee(employee.EmployeeId);
            var accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var shiftTypes = base.GetShiftTypesFromCache(entities, CacheConfig.Company(ActorCompanyId));

            DateTime firstEmployeeScheduleDate = startDate;
            DateTime currentDate = startDate;
            while (currentDate <= stopDate)
            {
                Guid link = Guid.NewGuid();

                var activeScheduleDay = activeScheduleDays?.FirstOrDefault(w => w.Date == currentDate);                           
                if (activeScheduleDay != null)
                {
                    if (!string.IsNullOrEmpty(activeScheduleDay.DayExternalCode))
                    {
                        var timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, currentDate, true);
                        timeBlockDate.ExternalCode = activeScheduleDay.DayExternalCode;
                    }

                    foreach (var breakBlock in activeScheduleDay.ActiveSceduleBreakBlocks ?? new List<ActiveSceduleBreakBlock>())
                    {
                        TimeCodeBreak timeCode = TimeCodeManager.GetTimeCodeBreakForLength(entities, actorCompanyId, employee.GetEmployeeGroupId(currentDate), Convert.ToInt32((breakBlock.StopTime - breakBlock.StartTime).TotalMinutes), timeCodeBreaks, timeCodeBreakGroups);
                        if (timeCode == null)
                            continue;
                        if (breakBlock.StartTime < currentDate.AddDays(-2) || breakBlock.StopTime > currentDate.AddDays(2))
                            continue;

                        scheduleBlocks.Add(new TimeScheduleTemplateBlockDTO()
                        {
                            ActualDate = activeScheduleDay.Date,
                            Date = activeScheduleDay.Date,
                            IsBreak = true,
                            StartTime = CalendarUtility.GetScheduleTime(breakBlock.StartTime, activeScheduleDay.Date, breakBlock.StartTime.Date),
                            StopTime = CalendarUtility.GetScheduleTime(breakBlock.StopTime, activeScheduleDay.Date, breakBlock.StopTime.Date),
                            TimeScheduleTemplatePeriodId = 0,
                            EmployeeId = employee.EmployeeId,
                            TimeCodeId = timeCode.TimeCodeId,
                            Link = link,
                        });
                    }

                    foreach (var block in activeScheduleDay.ActiveScheduleBlocks ?? new List<ActiveScheduleBlock>())
                    {
                        if (block.StartTime < currentDate.AddDays(-2) || block.StopTime > currentDate.AddDays(2))
                            continue;

                        var dto = new TimeScheduleTemplateBlockDTO()
                        {
                            EmployeeId = employee.EmployeeId,
                            TimeScheduleTemplatePeriodId = 0,
                            TimeCodeId = 0,
                            ActualDate = activeScheduleDay.Date,
                            Date = activeScheduleDay.Date,
                            StartTime = CalendarUtility.GetScheduleTime(block.StartTime, activeScheduleDay.Date, block.StartTime.Date),
                            StopTime = CalendarUtility.GetScheduleTime(block.StopTime, activeScheduleDay.Date, block.StopTime.Date),
                            IsBreak = false,
                            Link = link,
                        };

                        if (!string.IsNullOrEmpty(block.ShiftTypeExternalCode))
                            dto.ShiftTypeId = shiftTypes.GetShiftType(block.ShiftTypeExternalCode)?.ShiftTypeId;

                        if (base.UseAccountHierarchyOnCompanyFromCache(entities, ActorCompanyId) && !string.IsNullOrEmpty(block.HierarchicalAccountExternalCode))
                            dto.AccountId = accounts?.GetAccount(block.HierarchicalAccountExternalCode)?.AccountId;

                        if (block.AccountExternalCodeSIEDepartment != null)
                            dto.TryAddAccountInternal(accounts, TermGroup_SieAccountDim.Department, block.AccountExternalCodeSIEDepartment);
                        if (block.AccountExternalCodeSIECostUnit != null)
                            dto.TryAddAccountInternal(accounts, TermGroup_SieAccountDim.CostUnit, block.AccountExternalCodeSIECostUnit); 
                        if (block.AccountExternalCodeSIECostCenter != null)
                            dto.TryAddAccountInternal(accounts, TermGroup_SieAccountDim.CostCentre, block.AccountExternalCodeSIECostCenter);
                        if (block.AccountExternalCodeSIEProject != null)
                            dto.TryAddAccountInternal(accounts, TermGroup_SieAccountDim.Project, block.AccountExternalCodeSIEProject);
                        if (block.AccountExternalCodeSIEShop != null)
                            dto.TryAddAccountInternal(accounts, TermGroup_SieAccountDim.Shop, block.AccountExternalCodeSIEShop);

                        scheduleBlocks.Add(dto);
                    }
                }
                else
                {
                    scheduleBlocks.Add(new TimeScheduleTemplateBlockDTO()
                    {
                        EmployeeId = employee.EmployeeId,
                        TimeScheduleTemplatePeriodId = 0,
                        TimeCodeId = 0,
                        ActualDate = currentDate,
                        Date = currentDate,
                        StartTime = CalendarUtility.DATETIME_DEFAULT,
                        StopTime = CalendarUtility.DATETIME_DEFAULT,
                        IsBreak = false,
                        Link = link,
                    });
                }

                if (HasEmployeePlacement(employee.EmployeeId, currentDate, currentDate, employeeSchedules))
                    firstEmployeeScheduleDate = currentDate.AddDays(1);

                if (currentDate != stopDate)
                    currentDate = currentDate.AddDays(1);
                else
                    break;
            }

            if (firstEmployeeScheduleDate <= stopDate)
            {
                scheduleBlocks = scheduleBlocks.Where(w => w.Date >= firstEmployeeScheduleDate).ToList();

                DateTime dayBefore = firstEmployeeScheduleDate.AddDays(-1);
                EmployeeSchedule dockedEmployeeSchedule = entities.EmployeeSchedule.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId && f.StopDate == dayBefore && f.TimeScheduleTemplateHeadId == templateHead.TimeScheduleTemplateHeadId);

                ActivateScheduleGridDTO activateSchedule = new ActivateScheduleGridDTO()
                {
                    EmployeeScheduleId = dockedEmployeeSchedule?.EmployeeScheduleId ?? 0,
                    EmployeeScheduleStartDate = dockedEmployeeSchedule?.StartDate ?? firstEmployeeScheduleDate,
                    EmployeeScheduleStopDate = stopDate,
                    EmployeeScheduleStartDayNumber = dockedEmployeeSchedule?.StartDayNumber ?? 0,
                    EmployeeId = employee.EmployeeId,
                    EmployeeNr = employee.EmployeeNr,
                    EmployeeName = employee.Name,
                    TimeScheduleTemplateHeadId = templateHead.TimeScheduleTemplateHeadId,
                    TimeScheduleTemplateHeadName = templateHead.Name,
                    TemplateEmployeeId = 0,
                    TemplateStartDate = null,
                    EmployeeGroupId = 0,
                    EmployeeGroupName = "",
                    IsPlaced = false,
                    IsPreliminary = false,
                };

                SaveEmployeeSchedulePlacementItem placement = SaveEmployeeSchedulePlacementItem.Create(
                    activateSchedule,
                    dockedEmployeeSchedule == null ? TermGroup_TemplateScheduleActivateFunctions.NewPlacement : TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate,
                    templateHead.TimeScheduleTemplateHeadId,
                    0,
                    dockedEmployeeSchedule == null ? firstEmployeeScheduleDate : (DateTime?)null,
                    stopDate,
                    false,
                    activateSchedule.EmployeeId,
                    createTimeBlocksAndTransactionsAsync: false);
                placements.Add(placement);

                _ = GetTimeBlockDateFromCache(employee.EmployeeId, currentDate, true);
            }

            return (placements, scheduleBlocks);
        }

        #endregion

        #region Validation

        private ActionResult ValidateDates(ActiveScheduleInterval activeScheduleInterval, out DateTime validDateFrom, out DateTime validDateTo)
        {
            int maxDays = 365;
            DateTime? startDate = activeScheduleInterval.StartDate;
            DateTime? stopDate = activeScheduleInterval.StopDate;
            if ((stopDate.Value - startDate.Value).TotalDays > maxDays)
            {
                validDateFrom = CalendarUtility.DATETIME_DEFAULT;
                validDateTo = CalendarUtility.DATETIME_DEFAULT;
                return new ActionResult($"{GetText(12104, "Datumintervallet i filen överstiger max antal dagar: ")} {maxDays}");
            }
            else
            {
                validDateFrom = startDate.Value;
                validDateTo = stopDate.Value;
                return new ActionResult(true);
            }
        }


        #endregion

        #region Schedule

        private ActionResult DeleteActiveScheduleImportOverlappingSchedule(int employeeId, Dictionary<int, List<TimeScheduleTemplateBlock>> existingScheduleByEmployee, List<TimeScheduleTemplateBlockDTO> scheduleBlocksFromImport)
        {
            if (existingScheduleByEmployee.IsNullOrEmpty() || !existingScheduleByEmployee.ContainsKey(employeeId) || scheduleBlocksFromImport.IsNullOrEmpty())
                return new ActionResult(true);

            List<TimeScheduleTemplateBlock> existingScheduleForEmployee = existingScheduleByEmployee[employeeId].Where(s => s.Date.HasValue).ToList();
            foreach (var existingSchedulesOnEmployeeByDate in existingScheduleForEmployee.GroupBy(g => g.Date.Value))
            {
                DateTime date = existingSchedulesOnEmployeeByDate.Key;
                if (!scheduleBlocksFromImport.Any(a => a.Date == date))
                    continue;

                ActionResult result = SetScheduleToDeleted(existingSchedulesOnEmployeeByDate.ToList(), saveChanges: false);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        private ActionResult SaveActiveScheduleImportPlacement(List<SaveEmployeeSchedulePlacementItem> placementsFromImport)
        {
            if (placementsFromImport.IsNullOrEmpty())
                return new ActionResult(true);

            var validationResult = ValidateSchedulePlacement(placementsFromImport);
            if (!validationResult.Result.Success)
                return validationResult.Result;

            List<TimeScheduleTemplateBlock> asyncTemplateBlocks = null;
            return SaveEmployeeSchedulePlacement(validationResult, ref asyncTemplateBlocks);
        }

        private ActionResult SaveActiveScheduleImportSchedules(List<TimeScheduleTemplateBlockDTO> scheduleBlocksFromImport)
        {
            List<TimeSchedulePlanningDayDTO> planningDays = scheduleBlocksFromImport?.ToTimeSchedulePlanningDayDTOs(groupOnDateAndEmployeeInsteadOfPeriod: true);
            if (planningDays.IsNullOrEmpty())
                return new ActionResult(true);

            return SaveTimeScheduleShifts(TermGroup_ShiftHistoryType.ImportPayroll, planningDays, true, true, false, 0, null);
        }
        #endregion
    }
}
