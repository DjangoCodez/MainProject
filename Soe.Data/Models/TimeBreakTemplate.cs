using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeBreakTemplate : ICreatedModified, IState
    {

    }

    public partial class TimeBreakTemplateRow : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeBreakTemplate

        public static IEnumerable<TimeBreakTemplateDTO> ToDTOs(this IEnumerable<TimeBreakTemplate> l)
        {
            var dtos = new List<TimeBreakTemplateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeBreakTemplateDTO ToDTO(this TimeBreakTemplate e)
        {
            if (e == null)
                return null;

            TimeBreakTemplateDTO dto = new TimeBreakTemplateDTO()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                ShiftStartFromTime = e.ShiftStartFromTime,
                ShiftLength = e.ShiftLength,
                UseMaxWorkTimeBetweenBreaks = e.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,

                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            dto.TimeBreakTemplateRows = e.TimeBreakTemplateRow?.ToDTOs().ToList();

            if (!e.DayTypes.IsNullOrEmpty())
            {
                if (dto.DayTypeIds == null)
                    dto.DayTypeIds = new List<int>();
                dto.DayTypeIds.AddRange(e.DayTypes.Select(i => i.DayTypeId));
            }
            if (!e.ShiftTypes.IsNullOrEmpty())
            {
                if (dto.ShiftTypeIds == null)
                    dto.ShiftTypeIds = new List<int>();
                dto.ShiftTypeIds.AddRange(e.ShiftTypes.Select(i => i.ShiftTypeId));
            }

            return dto;
        }

        public static IEnumerable<TimeBreakTemplateDTO> ToDTOs(this IEnumerable<TimeBreakTemplateGridDTO> l)
        {
            var dtos = new List<TimeBreakTemplateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeBreakTemplateDTO ToDTO(this TimeBreakTemplateGridDTO e)
        {
            if (e == null)
                return null;

            TimeBreakTemplateDTO dto = new TimeBreakTemplateDTO()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                ShiftStartFromTime = e.ShiftStartFromTimeMinutes >= 0 ? CalendarUtility.DATETIME_DEFAULT.AddMinutes(e.ShiftStartFromTimeMinutes) : CalendarUtility.DATETIME_DEFAULT,
                ShiftLength = e.ShiftLength,
                UseMaxWorkTimeBetweenBreaks = e.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                State = e.State,
            };

            if (e.DayTypes != null && e.DayTypes.Count > 0)
            {
                if (dto.DayTypeIds == null)
                    dto.DayTypeIds = new List<int>();
                dto.DayTypeIds.AddRange(e.DayTypes.Where(i => i != null).Select(i => i.DayTypeId));
            }
            if (e.ShiftTypes != null && e.ShiftTypes.Count > 0)
            {
                if (dto.ShiftTypeIds == null)
                    dto.ShiftTypeIds = new List<int>();
                dto.ShiftTypeIds.AddRange(e.ShiftTypes.Where(i => i != null).Select(i => i.ShiftTypeId));
            }

            dto.TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>();
            for (int i = 1; i <= e.MajorNbrOfBreaks; i++)
            {
                dto.TimeBreakTemplateRows.Add(new TimeBreakTemplateRowDTO()
                {
                    Guid = Guid.NewGuid(),
                    TimeCodeBreakGroupId = e.MajorTimeCodeBreakGroupId,
                    MinTimeAfterStart = e.MajorMinTimeAfterStart,
                    MinTimeBeforeEnd = e.MajorMinTimeBeforeEnd,
                    Type = SoeTimeBreakTemplateType.Major,
                    State = SoeEntityState.Active,
                });
            }

            for (int i = 1; i <= e.MinorNbrOfBreaks; i++)
            {
                dto.TimeBreakTemplateRows.Add(new TimeBreakTemplateRowDTO()
                {
                    Guid = Guid.NewGuid(),
                    TimeCodeBreakGroupId = e.MinorTimeCodeBreakGroupId,
                    MinTimeAfterStart = e.MinorMinTimeAfterStart,
                    MinTimeBeforeEnd = e.MinorMinTimeBeforeEnd,
                    Type = SoeTimeBreakTemplateType.Minor,
                    State = SoeEntityState.Active,
                });
            }

            return dto;
        }

        public static TimeBreakTemplateGridDTO ToGridDTO(this TimeBreakTemplateDTO e)
        {
            if (e == null)
                return null;

            var majorBreaks = e.TimeBreakTemplateRows?.Where(r => r.Type == SoeTimeBreakTemplateType.Major && r.State == SoeEntityState.Active).ToList() ?? new List<TimeBreakTemplateRowDTO>();
            var minorBreaks = e.TimeBreakTemplateRows?.Where(r => r.Type == SoeTimeBreakTemplateType.Minor && r.State == SoeEntityState.Active).ToList() ?? new List<TimeBreakTemplateRowDTO>();

            var firstMajor = majorBreaks.FirstOrDefault();
            var firstMinor = minorBreaks.FirstOrDefault();

            return new TimeBreakTemplateGridDTO()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                ShiftStartFromTimeMinutes = e.ShiftStartFromTime.HasValue
                    ? CalendarUtility.TimeToMinutes(e.ShiftStartFromTime.Value, e.ShiftStartFromTime.Value.Date)
                    : 0,
                ShiftLength = e.ShiftLength,
                UseMaxWorkTimeBetweenBreaks = e.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                State = e.State,

                MajorNbrOfBreaks = majorBreaks.Count,
                MajorTimeCodeBreakGroupId = firstMajor?.TimeCodeBreakGroupId ?? 0,
                MajorMinTimeAfterStart = firstMajor?.MinTimeAfterStart ?? 0,
                MajorMinTimeBeforeEnd = firstMajor?.MinTimeBeforeEnd ?? 0,

                MinorNbrOfBreaks = minorBreaks.Count,
                MinorTimeCodeBreakGroupId = firstMinor?.TimeCodeBreakGroupId ?? 0,
                MinorMinTimeAfterStart = firstMinor?.MinTimeAfterStart ?? 0,
                MinorMinTimeBeforeEnd = firstMinor?.MinTimeBeforeEnd ?? 0,

            };
        }

        public static TimeBreakTemplateGridDTO ToGridDTO(this TimeBreakTemplateDTONew e)
        {
            if (e == null)
                return null;

            return new TimeBreakTemplateGridDTO()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                ShiftStartFromTimeMinutes = e.ShiftStartFromTime,
                ShiftLength = e.ShiftLength,
                UseMaxWorkTimeBetweenBreaks = e.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                State = e.State,

                MajorNbrOfBreaks = e.MajorNbrOfBreaks,
                MajorTimeCodeBreakGroupId = e.MajorTimeCodeBreakGroupId,
                MajorMinTimeAfterStart = e.MajorMinTimeAfterStart,
                MajorMinTimeBeforeEnd = e.MajorMinTimeBeforeEnd,

                MinorNbrOfBreaks = e.MinorNbrOfBreaks,
                MinorTimeCodeBreakGroupId = e.MinorTimeCodeBreakGroupId,
                MinorMinTimeAfterStart = e.MinorMinTimeAfterStart,
                MinorMinTimeBeforeEnd = e.MinorMinTimeBeforeEnd,

            };
        }

        public static TimeBreakTemplateDTO ToDTO(this TimeBreakTemplateDTONew e)
        {
            if (e == null)
                return null;

            TimeBreakTemplateDTO dto = new TimeBreakTemplateDTO()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                ShiftTypeIds = e.ShiftTypeIds,
                DayTypeIds = e.DayTypeIds,
                DayOfWeeks = e.DayOfWeeks,
                ShiftStartFromTime = e.ShiftStartFromTime >= 0 ? CalendarUtility.DATETIME_DEFAULT.AddMinutes(e.ShiftStartFromTime) : CalendarUtility.DATETIME_DEFAULT,
                ShiftLength = e.ShiftLength,
                UseMaxWorkTimeBetweenBreaks = e.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>()
            };

            for (int i = 0; i < e.MajorNbrOfBreaks; i++)
            {
                dto.TimeBreakTemplateRows.Add(new TimeBreakTemplateRowDTO()
                {
                    TimeCodeBreakGroupId = e.MajorTimeCodeBreakGroupId,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = e.MajorMinTimeAfterStart,
                    MinTimeBeforeEnd = e.MajorMinTimeBeforeEnd,
                    State = SoeEntityState.Active
                });
            }

            for (int i = 0; i < e.MinorNbrOfBreaks; i++)
            {
                dto.TimeBreakTemplateRows.Add(new TimeBreakTemplateRowDTO()
                {
                    TimeCodeBreakGroupId = e.MinorTimeCodeBreakGroupId,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = e.MinorMinTimeAfterStart,
                    MinTimeBeforeEnd = e.MinorMinTimeBeforeEnd,
                    State = SoeEntityState.Active
                });
            }

            return dto;
        }

        public static List<TimeBreakTemplateRow> GetRows(this TimeBreakTemplate e, SoeTimeBreakTemplateType type)
        {
            return e.TimeBreakTemplateRow?.Where(i => i.State == (int)SoeEntityState.Active && i.Type == (int)type).ToList() ?? new List<TimeBreakTemplateRow>();
        }

        public static List<TimeBreakTemplateGridDTONew> ToGridDTOsNew(this IEnumerable<TimeBreakTemplate> templates)
        {
            var dtos = new List<TimeBreakTemplateGridDTONew>();
            if (templates == null)
                return dtos;

            int rowNr = 1;
            foreach (var template in templates.OrderBy(t => t.TimeBreakTemplateId))
            {
                dtos.Add(template.ToGridDTONew(rowNr++));
            }
            return dtos;
        }

        public static TimeBreakTemplateGridDTONew ToGridDTONew(this TimeBreakTemplate e, int rowNr)
        {
            if (e == null)
                return null;

            var majorRows = e.GetRows(SoeTimeBreakTemplateType.Major);
            var minorRows = e.GetRows(SoeTimeBreakTemplateType.Minor);
            var firstMajor = majorRows.FirstOrDefault();
            var firstMinor = minorRows.FirstOrDefault();

            return new TimeBreakTemplateGridDTONew()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                RowNr = rowNr,
                ShiftTypeNames = string.Join(", ", e.ShiftTypes?.Select(st => st.Name) ?? new List<string>()),
                DayTypeNames = string.Join(", ", e.DayTypes?.Select(dt => dt.Name) ?? new List<string>()),
                DayOfWeekNames = GetDayOfWeekNamesFromString(e.DayOfWeeks),
                ShiftLength = e.ShiftLength,
                ShiftStartFromTimeMinutes = e.ShiftStartFromTime.HasValue
                    ? CalendarUtility.TimeToMinutes(e.ShiftStartFromTime.Value, e.ShiftStartFromTime.Value.Date)
                    : 0,

                MajorNbrOfBreaks = majorRows.Count,
                MajorTimeCodeBreakGroupName = firstMajor?.TimeCodeBreakGroup?.Name ?? string.Empty,
                MajorMinTimeAfterStart = firstMajor?.MinTimeAfterStart ?? 0,
                MajorMinTimeBeforeEnd = firstMajor?.MinTimeBeforeEnd ?? 0,

                MinorNbrOfBreaks = minorRows.Count,
                MinorTimeCodeBreakGroupName = firstMinor?.TimeCodeBreakGroup?.Name ?? string.Empty,
                MinorMinTimeAfterStart = firstMinor?.MinTimeAfterStart ?? 0,
                MinorMinTimeBeforeEnd = firstMinor?.MinTimeBeforeEnd ?? 0,

                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                State = (SoeEntityState)e.State
            };
        }

        public static TimeBreakTemplateDTONew ToDTONew(this TimeBreakTemplate e)
        {
            if (e == null)
                return null;

            var majorRows = e.GetRows(SoeTimeBreakTemplateType.Major);
            var minorRows = e.GetRows(SoeTimeBreakTemplateType.Minor);
            var firstMajor = majorRows.FirstOrDefault();
            var firstMinor = minorRows.FirstOrDefault();

            TimeBreakTemplateDTONew dto = new TimeBreakTemplateDTONew()
            {
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                ShiftStartFromTime = e.ShiftStartFromTime.HasValue
                    ? CalendarUtility.TimeToMinutes(e.ShiftStartFromTime.Value, e.ShiftStartFromTime.Value.Date)
                    : 0,
                ShiftLength = e.ShiftLength,
                UseMaxWorkTimeBetweenBreaks = e.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = e.MinTimeBetweenBreaks,
                StartDate = e.StartDate,
                StopDate = e.StopDate,

                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,

                MajorNbrOfBreaks = majorRows.Count,
                MajorTimeCodeBreakGroupId = firstMajor?.TimeCodeBreakGroupId ?? 0,
                MajorMinTimeAfterStart = firstMajor?.MinTimeAfterStart ?? 0,
                MajorMinTimeBeforeEnd = firstMajor?.MinTimeBeforeEnd ?? 0,

                MinorNbrOfBreaks = minorRows.Count,
                MinorTimeCodeBreakGroupId = firstMinor?.TimeCodeBreakGroupId ?? 0,
                MinorMinTimeAfterStart = firstMinor?.MinTimeAfterStart ?? 0,
                MinorMinTimeBeforeEnd = firstMinor?.MinTimeBeforeEnd ?? 0,
            };

            if (!e.ShiftTypes.IsNullOrEmpty())
            {
                dto.ShiftTypeIds = new List<int>();
                dto.ShiftTypeIds.AddRange(e.ShiftTypes.Select(i => i.ShiftTypeId));
            }

            if (!e.DayTypes.IsNullOrEmpty())
            {
                dto.DayTypeIds = new List<int>();
                dto.DayTypeIds.AddRange(e.DayTypes.Select(i => i.DayTypeId));
            }

            if (!string.IsNullOrEmpty(e.DayOfWeeks))
            {
                dto.DayOfWeeks = new List<DayOfWeek>();
                foreach (string dayOfWeek in e.DayOfWeeks.Split(','))
                {
                    if (int.TryParse(dayOfWeek, out int dayOfWeekId))
                    {
                        dto.DayOfWeeks.Add((DayOfWeek)dayOfWeekId);
                    }
                }
            }

            return dto;
        }

        private static string GetDayOfWeekNamesFromString(string dayOfWeeks)
        {
            if (string.IsNullOrEmpty(dayOfWeeks))
                return string.Empty;

            var names = new List<string>();
            foreach (string dayOfWeek in dayOfWeeks.Split(','))
            {
                if (int.TryParse(dayOfWeek, out int dayOfWeekId))
                {
                    string name = ((DayOfWeek)dayOfWeekId).ToString();
                    names.Add(name);
                }
            }
            return string.Join(", ", names);
        }

        #endregion

        #region TimeBreakTemplateRow

        public static IEnumerable<TimeBreakTemplateRowDTO> ToDTOs(this IEnumerable<TimeBreakTemplateRow> l)
        {
            var dtos = new List<TimeBreakTemplateRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeBreakTemplateRowDTO ToDTO(this TimeBreakTemplateRow e)
        {
            if (e == null)
                return null;

            return new TimeBreakTemplateRowDTO()
            {
                Guid = Guid.NewGuid(),
                TimeBreakTemplateRowId = e.TimeBreakTemplateRowId,
                TimeBreakTemplateId = e.TimeBreakTemplateId,
                TimeCodeBreakGroupId = e.TimeCodeBreakGroupId,
                Type = (SoeTimeBreakTemplateType)e.Type,
                MinTimeAfterStart = e.MinTimeAfterStart,
                MinTimeBeforeEnd = e.MinTimeBeforeEnd,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        #endregion
    }
}
