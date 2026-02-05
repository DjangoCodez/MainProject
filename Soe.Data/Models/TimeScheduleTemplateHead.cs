using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleTemplateHead : ICreatedModified, IState
    {
        public bool IsPersonalTemplate
        {
            get { return this.EmployeeId.HasValue && this.EmployeeId.Value > 0 && this.StartDate.HasValue; }
        }
        public DateTime LastPlacementStartDate { get; set; }
        public DateTime LastPlacementStopDate { get; set; }
        public List<int> TimeScheduleTemplatePeriodIds
        {
            get
            {
                List<int> timeScheduleTemplatePeriodIds = new List<int>();
                if (this.TimeScheduleTemplatePeriod != null)
                    timeScheduleTemplatePeriodIds.AddRange(this.TimeScheduleTemplatePeriod.Where(i => i.State == (int)SoeEntityState.Active).Select(i => i.TimeScheduleTemplatePeriodId));
                return timeScheduleTemplatePeriodIds;
            }
        }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleTemplateHead

        public static TimeScheduleTemplateHeadDTO ToDTO(this TimeScheduleTemplateHead e, bool includePeriods, bool includeBlocks, bool includeEmployeeSchedule, bool includeAccounts, bool includeEmployeeName, bool includeLastPlacement)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includePeriods && !e.TimeScheduleTemplatePeriod.IsLoaded)
                    {
                        e.TimeScheduleTemplatePeriod.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTemplateHead.cs e.TimeScheduleTemplatePeriod");
                    }
                    if (includeEmployeeSchedule && !e.EmployeeSchedule.IsLoaded)
                    {
                        e.EmployeeSchedule.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTemplateHead.cs e.EmployeeSchedule");
                    }
                    if (includeEmployeeName)
                    {
                        if (!e.EmployeeReference.IsLoaded)
                        {
                            e.EmployeeReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTemplateHead.cs e.EmployeeReference");
                        }
                        if (e.Employee != null && !e.Employee.ContactPersonReference.IsLoaded)
                        {
                            e.Employee.ContactPersonReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTemplateHead.cs e.Employee.ContactPersonReference");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeScheduleTemplateHeadDTO dto = new TimeScheduleTemplateHeadDTO()
            {
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                EmployeePostId = e.EmployeePostId,
                Name = e.Name,
                Description = e.Description,
                NoOfDays = e.NoOfDays,
                StartOnFirstDayOfWeek = e.StartOnFirstDayOfWeek,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                FirstMondayOfCycle = e.FirstMondayOfCycle,
                SimpleSchedule = e.SimpleSchedule,
                FlexForceSchedule = e.FlexForceSchedule,
                Locked = e.Locked,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includePeriods)
                dto.TimeScheduleTemplatePeriods = e.TimeScheduleTemplatePeriod?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(includeBlocks, includeAccounts).ToList() ?? new List<TimeScheduleTemplatePeriodDTO>();
            if (includeEmployeeSchedule)
            {
                dto.EmployeeSchedules = new List<EmployeeScheduleDTO>();
                if (e.EmployeeSchedule != null)
                    dto.EmployeeSchedules.AddRange(e.EmployeeSchedule.Where(s => s.State == (int)SoeEntityState.Active).ToDTOs());
            }
            if (includeEmployeeName)
                dto.EmployeeName = e.Employee?.Name ?? string.Empty;
            if (includeLastPlacement)
            {
                dto.LastPlacementStartDate = e.LastPlacementStartDate;
                dto.LastPlacementStopDate = e.LastPlacementStopDate;
            }

            return dto;
        }

        public static List<TimeScheduleTemplateHeadDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateHead> l, bool includePeriods, bool includeBlocks, bool includeEmployeeSchedule, bool includeAccounts, bool includeEmployeeName, bool includeLastPlacement)
        {
            var dtos = new List<TimeScheduleTemplateHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePeriods, includeBlocks, includeEmployeeSchedule, includeAccounts, includeEmployeeName, includeLastPlacement));
                }
            }
            return dtos;
        }

        public static TimeScheduleTemplateHead FromDTO(this TimeScheduleTemplateHeadDTO dto)
        {
            if (dto == null)
                return null;

            TimeScheduleTemplateHead e = new TimeScheduleTemplateHead()
            {
                TimeScheduleTemplateHeadId = dto.TimeScheduleTemplateHeadId,
                ActorCompanyId = dto.ActorCompanyId,
                EmployeeId = dto.EmployeeId,
                Name = dto.Name,
                Description = dto.Description,
                NoOfDays = dto.NoOfDays,
                StartOnFirstDayOfWeek = dto.StartOnFirstDayOfWeek,
                StartDate = dto.StartDate,
                StopDate = dto.StopDate,
                FirstMondayOfCycle = dto.FirstMondayOfCycle,
                SimpleSchedule = dto.SimpleSchedule,
                FlexForceSchedule = dto.FlexForceSchedule,
                Locked = dto.Locked,
                Created = dto.Created,
                CreatedBy = dto.CreatedBy,
                Modified = dto.Modified,
                ModifiedBy = dto.ModifiedBy,
                State = (int)dto.State
            };

            return e;
        }

        public static TimeScheduleTemplateHeadSmallDTO ToSmallDTO(this TimeScheduleTemplateHead e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTemplateHeadSmallDTO()
            {
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                Name = e.Name,
                NoOfDays = e.NoOfDays,
                EmployeeId = e.EmployeeId,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                FirstMondayOfCycle = e.FirstMondayOfCycle,
                SimpleSchedule = e.SimpleSchedule,
                Locked = e.Locked,
                AccountId = e.AccountId,
                AccountName = e.AccountName
            };
        }

        public static IEnumerable<TimeScheduleTemplateHeadSmallDTO> ToSmallDTOs(this IEnumerable<TimeScheduleTemplateHead> l)
        {
            var dtos = new List<TimeScheduleTemplateHeadSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static DateTime GetCycleStartFromGivenDate(this TimeScheduleTemplateHead template, DateTime date)
        {
            if (template != null && (template.FirstMondayOfCycle.HasValue || template.StartDate.HasValue))
            {
                DateTime currentDate = (template.FirstMondayOfCycle.HasValue ? template.FirstMondayOfCycle.Value.Date : template.StartDate.Value.Date);
                if (!template.StopDate.HasValue || template.StopDate.Value > date)
                {
                    while (currentDate.AddDays(template.NoOfDays) <= date)
                    {
                        currentDate = currentDate.AddDays(template.NoOfDays);
                    }
                }
                return currentDate; //see SetDayNumberFromTemplate in TimeSchedulePlanningViewModel
            }
            else
                return date; //see SetDayNumberFromTemplate in TimeSchedulePlanningViewModel
        }

        #endregion
    }
}
