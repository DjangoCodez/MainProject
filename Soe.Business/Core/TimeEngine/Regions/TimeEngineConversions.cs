using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region ApplyAbsenceDay

        private List<ApplyAbsenceDTO> ConvertToApplyAbsenceDayDTOs(List<ApplyAbsenceDay> applyAbsenceDays)
        {
            List<ApplyAbsenceDTO> dtos = new List<ApplyAbsenceDTO>();
            if (applyAbsenceDays != null)
            {
                foreach (ApplyAbsenceDay item in applyAbsenceDays)
                {
                    dtos.Add(item.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeEngineDay

        private List<TimeEngineDay> ConvertToTimeEngineDays(int employeeId, List<TimeEngineRestoreDay> restoreDays)
        {
            if (restoreDays.IsNullOrEmpty())
                return new List<TimeEngineDay>();

            List<TimeEngineDay> days = new List<TimeEngineDay>();
            foreach (TimeEngineRestoreDay restoreDay in restoreDays.Where(i => i.EmployeeId == employeeId).OrderBy(i => i.Date))
            {
                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, restoreDay.Date, createIfNotExists: true);
                if (timeBlockDate == null)
                    continue;

                days.AddDay(
                    templatePeriodId: restoreDay.TemplatePeriodId,
                    timeBlockDate: timeBlockDate
                    );
            }
            return days;
        }

        private TimeEnginePeriod ConvertToTimeEnginePeriod(Employee employee, List<AttestEmployeeDaySmallDTO> items, List<EmployeeGroup> employeeGroups = null)
        {
            return new TimeEnginePeriod(employee.EmployeeId, ConvertToTimeEngineDays(employee, items, employeeGroups));
        }

        private List<TimeEngineDay> ConvertToTimeEngineDays(Employee employee, List<AttestEmployeeDaySmallDTO> items, List<EmployeeGroup> employeeGroups = null)
        {
            if (items.IsNullOrEmpty())
                return new List<TimeEngineDay>();

            employeeGroups = employeeGroups ?? GetEmployeeGroupsFromCache();

            List<TimeEngineDay> days = new List<TimeEngineDay>();
            foreach (AttestEmployeeDaySmallDTO item in items.Where(i => i.EmployeeId == employee.EmployeeId).OrderBy(i => i.Date))
            {
                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, item.Date, createIfNotExists: true);
                if (timeBlockDate == null)
                    continue;

                days.AddDay(
                    templatePeriodId: item.TimeScheduleTemplatePeriodId ?? 0,
                    timeBlockDate: timeBlockDate, 
                    employeeGroup: employee.GetEmployeeGroup(item.Date, employeeGroups)
                    );
            }
            return days;
        }

        private List<TimeEngineDay> ConvertToTimeEngineDaysWithoutDeviations(int employeeId, List<TimeEngineDay> days)
        {
            if (days.IsNullOrEmpty())
                return new List<TimeEngineDay>();

            List<int> timeBlockDateIds = days.GetTimeBlockDateIds(skipNew: true).Distinct().ToList();

            var timeBlocks = timeBlockDateIds.Any() ?
                            (from tb in entities.TimeBlock
                             where tb.EmployeeId == employeeId &&
                             tb.State == (int)SoeEntityState.Active &&
                             timeBlockDateIds.Contains(tb.TimeBlockDateId)
                             select new
                             {
                                 tb.TimeBlockDateId,
                                 tb.StartTime,
                                 tb.StopTime,
                             }).ToList() : null;

            List<TimeEngineDay> validDays = new List<TimeEngineDay>();
            foreach (TimeEngineDay day in days)
            {
                if (day.IsNew || timeBlocks == null || !timeBlocks.Any(tb => tb.TimeBlockDateId == day.TimeBlockDateId && tb.StartTime < tb.StopTime))
                    validDays.Add(day);
            }

            return validDays;
        }

        #endregion

        #region TimeScheduleTemplateBlock

        private List<TimeScheduleTemplateBlockDTO> ConvertToScheduleBlockItems(List<TimeScheduleTemplateBlock> templateBlocks, TimeHalfdayDTO timeHalfDay)
        {
            var scheduleBlockItems = new List<TimeScheduleTemplateBlockDTO>();

            if (templateBlocks.IsNullOrEmpty())
                return scheduleBlockItems;

            // Do not add blocks if only breaks
            if (templateBlocks.Count(t => t.State == (int)SoeEntityState.Active) == templateBlocks.GetBreaks().Count(t => t.State == (int)SoeEntityState.Active))
                return scheduleBlockItems;

            foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks.Where(t => t.State == (int)SoeEntityState.Active))
            {
                //Make sure it is only schedule blocks
                if (templateBlock.Date.HasValue)
                    continue;

                if (CanEntityLoadReferences(entities, templateBlock))
                {
                    if (!templateBlock.AccountInternal.IsLoaded)
                        templateBlock.AccountInternal.Load();
                    if (!templateBlock.TimeScheduleTemplateBlockTask.IsLoaded)
                        templateBlock.TimeScheduleTemplateBlockTask.Load();

                    SynchTimeScheduleTemplateBlockToCache(templateBlock);
                }

                scheduleBlockItems.Add(templateBlock.ToDTO(true));
            }

            // Reduce length of schedule according to halfday settings
            if (scheduleBlockItems.Any() && timeHalfDay != null)
                scheduleBlockItems = AdjustScheduleBlocksAccordingToHalfday(scheduleBlockItems, timeHalfDay);

            return scheduleBlockItems;
        }

        private List<TimeScheduleTemplateBlockDTO> ConvertToTemplateBlockItems(List<TimeSchedulePlanningDayDTO> shifts)
        {
            List<TimeScheduleTemplateBlockDTO> templateBlockItems = new List<TimeScheduleTemplateBlockDTO>();
            foreach (TimeSchedulePlanningDayDTO shift in shifts.OrderBy(i => i.StartTime))
            {
                templateBlockItems.Add(ConvertToTemplateBlockItem(shift));
            }
            return templateBlockItems;
        }

        private TimeScheduleTemplateBlockDTO ConvertToTemplateBlockItem(TimeSchedulePlanningDayDTO shift)
        {
            TimeScheduleTemplateBlockDTO item = new TimeScheduleTemplateBlockDTO();
            if (shift != null)
            {
                item.TimeScheduleTemplateBlockId = shift.TimeScheduleTemplateBlockId;
                item.TimeScheduleTemplatePeriodId = shift.TimeScheduleTemplatePeriodId;
                item.TimeScheduleEmployeePeriodId = shift.TimeScheduleEmployeePeriodId;
                item.TimeScheduleTypeId = shift.TimeScheduleTypeId;
                item.EmployeeId = shift.EmployeeId;
                item.DayNumber = shift.DayNumber;
                item.Description = shift.Description;
                item.StartTime = CalendarUtility.GetScheduleTime(shift.StartTime);
                item.StopTime = CalendarUtility.GetScheduleTime(shift.StopTime, shift.StartTime, shift.StopTime);
                item.Type = shift.Type;
                if (shift.BelongsToPreviousDay)
                {
                    item.StartTime = item.StartTime.AddDays(1);
                    item.StopTime = item.StopTime.AddDays(1);
                }
                else if (shift.BelongsToNextDay)
                {
                    item.StartTime = item.StartTime.AddDays(-1);
                    item.StopTime = item.StopTime.AddDays(-1);
                }
                item.Date = null;
                item.ActualDate = shift.ActualDate;
                item.State = (int)SoeEntityState.Active;

                //Breaks
                item.Break1Id = shift.Break1Id;
                item.Break1StartTime = shift.Break1StartTime;
                item.Break1Minutes = shift.Break1Minutes;
                item.Break1Link = shift.Break1Link;
                item.Break1IsPreliminary = shift.Break1IsPreliminary;
                item.Break2Id = shift.Break2Id;
                item.Break2StartTime = shift.Break2StartTime;
                item.Break2Minutes = shift.Break2Minutes;
                item.Break2Link = shift.Break2Link;
                item.Break2IsPreliminary = shift.Break2IsPreliminary;
                item.Break3Id = shift.Break3Id;
                item.Break3StartTime = shift.Break3StartTime;
                item.Break3Minutes = shift.Break3Minutes;
                item.Break3Link = shift.Break3Link;
                item.Break3IsPreliminary = shift.Break3IsPreliminary;
                item.Break4Id = shift.Break4Id;
                item.Break4StartTime = shift.Break4StartTime;
                item.Break4Minutes = shift.Break4Minutes;
                item.Break4Link = shift.Break4Link;
                item.Break4IsPreliminary = shift.Break4IsPreliminary;
                item.HasBreakTimes = true;

                //TimeCode
                item.TimeCodeId = shift.TimeCodeId;
                item.Break1TimeCodeId = shift.Break1TimeCodeId;
                item.Break2TimeCodeId = shift.Break2TimeCodeId;
                item.Break3TimeCodeId = shift.Break3TimeCodeId;
                item.Break4TimeCodeId = shift.Break4TimeCodeId;

                //Accounting
                item.AccountId = shift.AccountId;
                item.Dim2Id = 0;
                item.Dim3Id = 0;
                item.Dim4Id = 0;
                item.Dim5Id = 0;
                item.Dim6Id = 0;

                //Shift
                item.ShiftTypeId = shift.ShiftTypeId;
                item.ShiftTypeName = shift.ShiftTypeName;
                item.ShiftTypeDescription = shift.Description;
                item.ShiftTypeTimeScheduleTypeId = shift.TimeScheduleTypeId;
                item.Link = shift.Link;
                item.IsPreliminary = shift.IsPreliminary;
                item.ExtraShift = shift.ExtraShift;
                item.SubstituteShift = shift.SubstituteShift;
                item.StaffingNeedsRowId = shift.StaffingNeedsRowId;
                item.StaffingNeedsRowPeriodId = shift.StaffingNeedsRowPeriodId;

                item.Tasks = new List<TimeScheduleTemplateBlockTaskDTO>();
                if (shift.Tasks != null)
                {
                    foreach (TimeScheduleTemplateBlockTaskDTO task in shift.Tasks)
                    {
                        item.Tasks.Add(new TimeScheduleTemplateBlockTaskDTO()
                        {
                            TimeScheduleTemplateBlockId = task.TimeScheduleTemplateBlockId,
                            StartTime = task.StartTime,
                            StopTime = task.StopTime,
                            TimeScheduleTaskId = task.TimeScheduleTaskId,
                            IncomingDeliveryRowId = task.IncomingDeliveryRowId,
                            State = task.State,
                        });
                    }
                }
                else if (shift.TimeScheduleTaskId.HasValue || shift.IncomingDeliveryRowId.HasValue)
                {
                    item.Tasks.Add(new TimeScheduleTemplateBlockTaskDTO()
                    {
                        TimeScheduleTemplateBlockId = shift.TimeScheduleTemplateBlockId,
                        StartTime = shift.StartTime,
                        StopTime = shift.StopTime,
                        TimeScheduleTaskId = shift.TimeScheduleTaskId,
                        IncomingDeliveryRowId = shift.IncomingDeliveryRowId,
                        State = SoeEntityState.Active,
                    });
                }
            }

            return item;
        }

        private TimeScheduleTemplateBlock ConvertToTimeScheduleTemplateBlock(TimeSchedulePlanningDayDTO dayDTO, Employee employee)
        {
            TimeScheduleTemplateBlock templateBlock = new TimeScheduleTemplateBlock()
            {
                Type = (int)dayDTO.Type,
                StartTime = CalendarUtility.GetScheduleTime(dayDTO.StartTime),
                StopTime = CalendarUtility.GetScheduleTime(dayDTO.StopTime, dayDTO.StartTime.Date, dayDTO.StopTime.Date),
                Date = dayDTO.StartTime.Date,
                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                Description = dayDTO.Description,
                Link = dayDTO.Link.HasValue ? dayDTO.Link.ToString() : String.Empty,
                ExtraShift = dayDTO.ExtraShift,
                SubstituteShift = dayDTO.SubstituteShift,
                IsPreliminary = dayDTO.IsPreliminary,
                AbsenceType = (int)dayDTO.AbsenceType,

                //Set FK
                TimeScheduleTemplatePeriodId = dayDTO.TimeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriodId = dayDTO.TimeScheduleEmployeePeriodId,
                TimeScheduleScenarioHeadId = dayDTO.TimeScheduleScenarioHeadId,
                TimeCodeId = dayDTO.TimeCodeId,
                EmployeeId = dayDTO.EmployeeId,
                TimeScheduleTypeId = dayDTO.TimeScheduleTypeId != 0 ? dayDTO.TimeScheduleTypeId : (int?)null,
                ShiftTypeId = dayDTO.ShiftTypeId != 0 ? (int?)dayDTO.ShiftTypeId : null,
                TimeDeviationCauseId = dayDTO.TimeDeviationCauseId,
                EmployeeChildId = dayDTO.EmployeeChildId,
                TimeHalfdayId = null,
                AccountId = dayDTO.AccountId != 0 ? dayDTO.AccountId : (int?)null,
            };
            SetCreatedProperties(templateBlock);
            entities.TimeScheduleTemplateBlock.AddObject(templateBlock);

            if (dayDTO.BelongsToPreviousDay)
            {
                templateBlock.Date = templateBlock.Date.Value.AddDays(-1).Date;
                templateBlock.StartTime = templateBlock.StartTime.AddDays(1);
                templateBlock.StopTime = templateBlock.StopTime.AddDays(1);
            }
            else if (dayDTO.BelongsToNextDay)
            {
                templateBlock.Date = templateBlock.Date.Value.AddDays(1).Date;
                templateBlock.StartTime = templateBlock.StartTime.AddDays(-1);
                templateBlock.StopTime = templateBlock.StopTime.AddDays(-1);
            }

            if (dayDTO.Type == TermGroup_TimeScheduleTemplateBlockType.Booking)
            {
                //save orgiginal booking dates to be able to handle booking spanning multiple days
                templateBlock.StartTime = dayDTO.StartTime;
                templateBlock.StopTime = dayDTO.StopTime;
            }

            // Staffing needs
            templateBlock.StaffingNeedsRowId = dayDTO.StaffingNeedsRowId;
            templateBlock.StaffingNeedsRowPeriodId = dayDTO.StaffingNeedsRowPeriodId;

            //Connect employeeschedule
            if (!dayDTO.TimeScheduleScenarioHeadId.HasValue)
                templateBlock.EmployeeSchedule = GetEmployeeScheduleFromCache(templateBlock.EmployeeId.Value, templateBlock.Date.Value);

            // Order planning
            if (dayDTO.Order != null)
            {
                OrderListDTO order = dayDTO.Order;
                templateBlock.CustomerInvoiceId = order.OrderId;
                templateBlock.ProjectId = order.ProjectId;
            }

            //Set accounting
            ApplyAccountingOnTimeScheduleTemplateBlock(templateBlock, employee, templateBlock.ShiftTypeId, null, null);

            return templateBlock;
        }

        private TimeSchedulePlanningDayDTO ConvertToTimeSchedulePlanningDayDTO(TimeScheduleTemplateBlock templateBlock, int newEmployeeId, DateTime newStartTime, DateTime newStopTime, List<TimeScheduleTemplateBlock> breakBlocks = null)
        {
            TimeSchedulePlanningDayDTO dayDTO = templateBlock.ToTimeSchedulePlanningDayDTO();
            if (dayDTO != null)
            {
                dayDTO.Type = (TermGroup_TimeScheduleTemplateBlockType)templateBlock.Type;
                dayDTO.StartTime = newStartTime;
                dayDTO.StopTime = newStopTime;
                dayDTO.EmployeeId = newEmployeeId;
            }

            if (breakBlocks != null && templateBlock.Date.HasValue)
            {
                DateTime newDate = newStartTime.Date;
                DateTime oldDate = templateBlock.Date.Value;
                int daysOffset = (newDate - oldDate).Days;

                if (dayDTO != null)
                {
                    int breakNr = 0;
                    foreach (var breakBlock in breakBlocks.OrderBy(b => b.Date).ThenBy(b => b.StartTime))
                    {
                        breakNr++;
                        switch (breakNr)
                        {
                            case 1:
                                dayDTO.Break1Id = breakBlock.TimeScheduleTemplateBlockId;
                                dayDTO.Break1TimeCodeId = breakBlock.TimeCodeId;
                                dayDTO.Break1StartTime = CalendarUtility.MergeDateAndTime(breakBlock.Date.Value.AddDays((breakBlock.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days + daysOffset), breakBlock.StartTime);
                                dayDTO.Break1Minutes = (int)(breakBlock.StopTime - breakBlock.StartTime).TotalMinutes;
                                dayDTO.Break1Link = (!String.IsNullOrEmpty(breakBlock.Link)) ? new Guid(breakBlock.Link) : (Guid?)null;
                                dayDTO.Break1IsPreliminary = breakBlock.IsPreliminary;
                                break;
                            case 2:
                                dayDTO.Break2Id = breakBlock.TimeScheduleTemplateBlockId;
                                dayDTO.Break2TimeCodeId = breakBlock.TimeCodeId;
                                dayDTO.Break2StartTime = CalendarUtility.MergeDateAndTime(breakBlock.Date.Value.AddDays((breakBlock.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days + daysOffset), breakBlock.StartTime);
                                dayDTO.Break2Minutes = (int)(breakBlock.StopTime - breakBlock.StartTime).TotalMinutes;
                                dayDTO.Break2Link = (!String.IsNullOrEmpty(breakBlock.Link)) ? new Guid(breakBlock.Link) : (Guid?)null;
                                dayDTO.Break2IsPreliminary = breakBlock.IsPreliminary;
                                break;
                            case 3:
                                dayDTO.Break3Id = breakBlock.TimeScheduleTemplateBlockId;
                                dayDTO.Break3TimeCodeId = breakBlock.TimeCodeId;
                                dayDTO.Break3StartTime = CalendarUtility.MergeDateAndTime(breakBlock.Date.Value.AddDays((breakBlock.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days + daysOffset), breakBlock.StartTime);
                                dayDTO.Break3Minutes = (int)(breakBlock.StopTime - breakBlock.StartTime).TotalMinutes;
                                dayDTO.Break3Link = (!String.IsNullOrEmpty(breakBlock.Link)) ? new Guid(breakBlock.Link) : (Guid?)null;
                                dayDTO.Break3IsPreliminary = breakBlock.IsPreliminary;
                                break;
                            case 4:
                                dayDTO.Break4Id = breakBlock.TimeScheduleTemplateBlockId;
                                dayDTO.Break4TimeCodeId = breakBlock.TimeCodeId;
                                dayDTO.Break4StartTime = CalendarUtility.MergeDateAndTime(breakBlock.Date.Value.AddDays((breakBlock.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days + daysOffset), breakBlock.StartTime);
                                dayDTO.Break4Minutes = (int)(breakBlock.StopTime - breakBlock.StartTime).TotalMinutes;
                                dayDTO.Break4Link = (!String.IsNullOrEmpty(breakBlock.Link)) ? new Guid(breakBlock.Link) : (Guid?)null;
                                dayDTO.Break4IsPreliminary = breakBlock.IsPreliminary;
                                break;
                        }
                    }
                }
            }

            return dayDTO;
        }

        private TimeSchedulePlanningDayDTO CreateTimeSchedulePlanningDayDTO(int employeeId, DateTime date, StaffingNeedsTaskDTO taskDTO)
        {
            if (!taskDTO.StartTime.HasValue || !taskDTO.StopTime.HasValue)
                return null;

            TimeSchedulePlanningDayDTO dto = new TimeSchedulePlanningDayDTO()
            {
                EmployeeId = employeeId,
                ShiftTypeId = taskDTO.ShiftTypeId ?? 0,
                StartTime = CalendarUtility.MergeDateAndTime(date, taskDTO.StartTime.Value.RemoveSeconds()),
                AccountId = taskDTO.AccountId
            };
            dto.StopTime = dto.StartTime.Add(taskDTO.StopTime.Value.RemoveSeconds() - taskDTO.StartTime.Value.RemoveSeconds());

            return dto;
        }

        #endregion

        #region TimeTransactionItem

        private List<TimeTransactionItem> ConvertToTimeTransactionItems(TimeEngineTemplate template)
        {
            List<TimeTransactionItem> timeTransactionItems = new List<TimeTransactionItem>();

            List<TimeCodeTransaction> timeCodeTransactions = template?.Outcome?.TimeCodeTransactions ?? new List<TimeCodeTransaction>();
            foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions)
            {
                TimeTransactionItem item = ConvertToTimeTransactionItem(timeCodeTransaction);
                if (item != null)
                    timeTransactionItems.Add(item);
            }

            return timeTransactionItems;
        }

        private TimeTransactionItem ConvertToTimeTransactionItem(TimeCodeTransaction timeCodeTransaction)
        {
            //Do not return schedule transactions from TimeCodeTransactions, only from TimePayrollTransaction
            if (timeCodeTransaction.IsScheduleTransaction)
                return null;

            TimeCode timeCode = GetTimeCodeFromCache(timeCodeTransaction.TimeCodeId);
            if (timeCode == null)
                return null;

            TimeTransactionItem timeTransactionItem = new TimeTransactionItem
            {
                //Keys
                GuidInternalPK = timeCodeTransaction.Guid,
                GuidTimeBlockFK = timeCodeTransaction.GuidTimeBlock,

                //Transaction
                TimeTransactionId = timeCodeTransaction.TimeCodeTransactionId,
                TransactionType = SoeTimeTransactionType.TimeCode,
                Quantity = timeCodeTransaction.Quantity,
                InvoiceQuantity = 0,
                Comment = String.Empty,
                ManuallyAdded = false,
                IsAdded = false,
                IsFixed = false,
                IsReversed = false,
                ReversedDate = null,
                TransactionSysPayrollTypeLevel1 = 0,
                TransactionSysPayrollTypeLevel2 = 0,
                TransactionSysPayrollTypeLevel3 = 0,
                TransactionSysPayrollTypeLevel4 = 0,
                IsScheduleTransaction = false,
                IsVacationReplacement = false,
                ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,

                //Employee
                EmployeeId = 0,
                EmployeeName = String.Empty,
                EmployeeChildId = null,
                EmployeeChildName = String.Empty,

                //Product
                ProductId = 0,
                ProductNr = String.Empty,
                ProductName = String.Empty,
                ProductVatType = TermGroup_InvoiceProductVatType.None,
                PayrollProductSysPayrollTypeLevel1 = 0,
                PayrollProductSysPayrollTypeLevel2 = 0,
                PayrollProductSysPayrollTypeLevel3 = 0,
                PayrollProductSysPayrollTypeLevel4 = 0,

                //TimeCode
                TimeCodeId = timeCodeTransaction.TimeCodeId,
                Code = timeCode.Code,
                CodeName = timeCode.Name,
                TimeCodeStart = timeCodeTransaction.Start,
                TimeCodeStop = timeCodeTransaction.Stop,
                TimeCodeType = SoeTimeCodeType.None,
                TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,

                //TimeBlock
                TimeBlockId = timeCodeTransaction.TimeBlockId,

                //TimeBlockDate
                TimeBlockDateId = timeCodeTransaction.TimeBlockDateId,
                Date = timeCodeTransaction.TimeBlockDate != null ? timeCodeTransaction.TimeBlockDate.Date : (DateTime?)null,

                //TimeRule
                TimeRuleId = timeCodeTransaction.TimeRuleId ?? 0,
                TimeRuleName = timeCodeTransaction.TimeRuleName,
                TimeRuleSort = timeCodeTransaction.TimeRuleSort,

                //Attest
                AttestStateId = 0,
                AttestStateName = String.Empty,
                AttestStateInitial = false,
                AttestStateColor = String.Empty,
                AttestStateSort = 0,

                //Accounting
                //..
            };

            return timeTransactionItem;
        }

        #endregion
    }
}
