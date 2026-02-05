using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleTemplateGroup : ICreatedModified, IState
    {

    }

    public partial class TimeScheduleTemplateGroupEmployee : ICreatedModified, IState
    {

    }

    public partial class TimeScheduleTemplateGroupRow : ICreatedModified, IState
    {
        public DateTime? NextStartDate { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleTemplateGroup

        public static TimeScheduleTemplateGroupDTO ToDTO(this TimeScheduleTemplateGroup e, bool skipEmployees = false)
        {
            if (e == null)
                return null;

            TimeScheduleTemplateGroupDTO dto = new TimeScheduleTemplateGroupDTO()
            {
                TimeScheduleTemplateGroupId = e.TimeScheduleTemplateGroupId,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            dto.TemplateNames = e.TimeScheduleTemplateGroupRow?.Where(r => r.TimeScheduleTemplateHead != null && r.State == (int)SoeEntityState.Active).Select(i => i.TimeScheduleTemplateHead.Name).OrderBy(name => name).ToCommaSeparated();
            dto.Rows = e.TimeScheduleTemplateGroupRow?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs() ?? new List<TimeScheduleTemplateGroupRowDTO>();
            dto.Employees = !skipEmployees && e.TimeScheduleTemplateGroupEmployee != null ? e.TimeScheduleTemplateGroupEmployee.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs() : new List<TimeScheduleTemplateGroupEmployeeDTO>();

            return dto;
        }

        public static List<TimeScheduleTemplateGroupDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateGroup> l)
        {
            var dtos = new List<TimeScheduleTemplateGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleTemplateGroupGridDTO ToGridDTO(this TimeScheduleTemplateGroup e)
        {
            if (e == null)
                return null;

            TimeScheduleTemplateGroupGridDTO dto = new TimeScheduleTemplateGroupGridDTO()
            {
                TimeScheduleTemplateGroupId = e.TimeScheduleTemplateGroupId,
                Name = e.Name,
                Description = e.Description,
            };

            dto.NbrOfRows = e.TimeScheduleTemplateGroupRow?.Count(r => r.State == (int)SoeEntityState.Active) ?? 0;
            dto.NbrOfEmployees = e.TimeScheduleTemplateGroupEmployee?.Count(r => r.State == (int)SoeEntityState.Active) ?? 0;

            return dto;
        }

        public static List<TimeScheduleTemplateGroupGridDTO> ToGridDTOs(this IEnumerable<TimeScheduleTemplateGroup> l)
        {
            var dtos = new List<TimeScheduleTemplateGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleTemplateGroupRowDTO ToDTO(this TimeScheduleTemplateGroupRow e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTemplateGroupRowDTO()
            {
                TimeScheduleTemplateGroupRowId = e.TimeScheduleTemplateGroupRowId,
                TimeScheduleTemplateGroupId = e.TimeScheduleTemplateGroupId,
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                RecurrencePattern = e.RecurrencePattern,
                NextStartDate = e.NextStartDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<TimeScheduleTemplateGroupRowDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateGroupRow> l)
        {
            var dtos = new List<TimeScheduleTemplateGroupRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool SetNextStartDate(this TimeScheduleTemplateGroupRow row, DateTime visibleDateFrom, DateTime visibleDateTo)
        {
            if (row.StopDate.HasValue && visibleDateTo > row.StopDate.Value)
                visibleDateTo = row.StopDate.Value;

            DailyRecurrenceDatesOutput output = DailyRecurrencePatternDTO.GetDatesFromPattern(row.RecurrencePattern, row.StartDate, visibleDateFrom, visibleDateTo, null);
            if (output.RecurrenceDates.Any())
            {
                row.NextStartDate = output.RecurrenceDates.First();
                return true;
            }

            return false;
        }

        public static TimeScheduleTemplateGroupEmployeeDTO ToDTO(this TimeScheduleTemplateGroupEmployee e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTemplateGroupEmployeeDTO()
            {
                TimeScheduleTemplateGroupEmployeeId = e.TimeScheduleTemplateGroupEmployeeId,
                TimeScheduleTemplateGroupId = e.TimeScheduleTemplateGroupId,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.Employee?.EmployeeNr,
                EmployeeName = e.Employee?.Name,
                Group = e.TimeScheduleTemplateGroup?.ToDTO(true)
            };
        }

        public static List<TimeScheduleTemplateGroupEmployeeDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateGroupEmployee> l)
        {
            var dtos = new List<TimeScheduleTemplateGroupEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
