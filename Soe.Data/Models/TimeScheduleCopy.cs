using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleCopyHead : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleCopyHead

        public static TimeScheduleCopyHeadDTO ToDTO(this TimeScheduleCopyHead e)
        {
            if (e == null)
                return null;

            TimeScheduleCopyHeadDTO dto = new TimeScheduleCopyHeadDTO
            {
                TimeScheduleCopyHeadId = e.TimeScheduleCopyHeadId,
                Type = (TermGroup_TimeScheduleCopyHeadType)e.Type,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                UserId = e.UserId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.TimeScheduleCopyRow != null)
                dto.Rows = e.TimeScheduleCopyRow.ToDTOs();

            return dto;
        }

        public static List<TimeScheduleCopyHeadDTO> ToDTOs(this IEnumerable<TimeScheduleCopyHead> l)
        {
            var dtos = new List<TimeScheduleCopyHeadDTO>();
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

        #region TimeScheduleCopyRow

        public static TimeScheduleCopyRowDTO ToDTO(this TimeScheduleCopyRow e)
        {
            if (e == null)
                return null;

            TimeScheduleCopyRowDTO dto = new TimeScheduleCopyRowDTO
            {
                TimeScheduleCopyRowId = e.TimeScheduleCopyRowId,
                EmployeeId = e.EmployeeId,
                Type = (TermGroup_TimeScheduleCopyRowType)e.Type,
                JsonData = e.JsonData,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static List<TimeScheduleCopyRowDTO> ToDTOs(this IEnumerable<TimeScheduleCopyRow> l)
        {
            var dtos = new List<TimeScheduleCopyRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleCopyRowJsonDataShiftDTO ToTimeScheduleCopyRowJsonDataDTO(this TimeScheduleTemplateBlock e)
        {
            if (e == null)
                return null;

            var dto = new TimeScheduleCopyRowJsonDataShiftDTO
            {
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                Type = (TimeScheduleBlockType)e.Type,
                Date = e.Date,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                ShiftTypeId = e.ShiftTypeId,
                IsBreak = e.IsBreak,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                AccountId = e.AccountId,
                AbsenceType = (TermGroup_TimeScheduleTemplateBlockAbsenceType)e.AbsenceType
            };

            return dto;
        }

        public static TimeScheduleCopyRowJsonDataShiftDTO ToTimeScheduleCopyRowJsonDataDTO(this TimeScheduleEmployeePeriodDetail e)
        {
            if (e == null)
                return null;

            var dto = new TimeScheduleCopyRowJsonDataShiftDTO
            {
                Type = TimeScheduleBlockType.Schedule,
                TimeScheduleEmployeePeriodDetailId = e.TimeScheduleEmployeePeriodDetailId,
                TimeLeisureCodeId = e.TimeLeisureCodeId,
                Date = e.TimeScheduleEmployeePeriod.Date.Date,
                StartTime = e.TimeScheduleEmployeePeriod.Date,
                StopTime = CalendarUtility.GetEndOfDay(e.TimeScheduleEmployeePeriod.Date),
            };

            return dto;
        }

        public static string ToTimeScheduleCopyRowJsonData(this TimeScheduleCopyRowJsonDataDTO dto)
        {
            if (dto == null)
                return null;

            return JsonConvert.SerializeObject(dto);
        }

        public static TimeScheduleCopyRowJsonDataDTO FromTimeScheduleCopyRowJsonData(this string json)
        {
            if (json == null)
                return null;

            return JsonConvert.DeserializeObject<TimeScheduleCopyRowJsonDataDTO>(json);
        }

        #endregion
    }
}
