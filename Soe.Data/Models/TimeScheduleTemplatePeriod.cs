using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleTemplatePeriod : ICreatedModified, IState
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public string HolidayName { get; set; }
        public bool IsHoliday { get; set; }
        public bool HasAttestedTransactions { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleTemplatePeriod

        public static TimeScheduleTemplatePeriodDTO ToDTO(this TimeScheduleTemplatePeriod e, bool includeBlocks = false, bool includeAccounts = false)
        {
            if (e == null)
                return null;

            TimeScheduleTemplatePeriodDTO dto = new TimeScheduleTemplatePeriodDTO()
            {
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                DayNumber = e.DayNumber,
                Date = e.Date,
                DayName = e.DayName,
                HolidayName = e.HolidayName,
                IsHoliday = e.IsHoliday,
                HasAttestedTransactions = e.HasAttestedTransactions,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includeBlocks)
                dto.TimeScheduleTemplateBlocks = (e.TimeScheduleTemplateBlock != null && e.TimeScheduleTemplateBlock.Count > 0) ? e.TimeScheduleTemplateBlock.Where(b => b.State == (int)SoeEntityState.Active).ToDTOs(includeAccounts: includeAccounts) : new List<TimeScheduleTemplateBlockDTO>();

            return dto;
        }

        public static IEnumerable<TimeScheduleTemplatePeriodDTO> ToDTOs(this IEnumerable<TimeScheduleTemplatePeriod> l, bool includeBlocks = false, bool includeAccounts = false)
        {
            var dtos = new List<TimeScheduleTemplatePeriodDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeBlocks, includeAccounts));
                }
            }
            return dtos.OrderBy(d => d.DayNumber);
        }

        public static TimeScheduleTemplatePeriodSmallDTO ToSmallDTO(this TimeScheduleTemplatePeriod e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTemplatePeriodSmallDTO()
            {
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                DayNumber = e.DayNumber
            };
        }

        public static IEnumerable<TimeScheduleTemplatePeriodSmallDTO> ToSmallDTOs(this IEnumerable<TimeScheduleTemplatePeriod> l)
        {
            var dtos = new List<TimeScheduleTemplatePeriodSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
