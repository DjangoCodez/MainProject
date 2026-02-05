using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class ExtendedAbsenceSetting
    {
        public bool IsWholeDay
        {
            get { return !this.AbsenceFirstAndLastDay && !this.AdjustAbsencePerWeekDay/* && !this.PercentalAbsence*/; }
        }
    }

    public static partial class EntityExtensions
    {
        public static IEnumerable<ExtendedAbsenceSettingDTO> ToDTOs(this IEnumerable<ExtendedAbsenceSetting> l)
        {
            var dtos = new List<ExtendedAbsenceSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ExtendedAbsenceSetting FromDTO(this ExtendedAbsenceSettingDTO dto)
        {
            if (dto == null)
                return null;

            return new ExtendedAbsenceSetting
            {
                ExtendedAbsenceSettingId = dto.ExtendedAbsenceSettingId,
                AbsenceFirstAndLastDay = dto.AbsenceFirstAndLastDay,
                AbsenceWholeFirstDay = dto.AbsenceWholeFirstDay,
                AbsenceFirstDayStart = dto.AbsenceFirstDayStart,
                AbsenceWholeLastDay = dto.AbsenceWholeLastDay,
                AbsenceLastDayStart = dto.AbsenceLastDayStart,
                PercentalAbsence = dto.PercentalAbsence,
                PercentalValue = dto.PercentalValue,
                PercentalAbsenceOccursStartOfDay = dto.PercentalAbsenceOccursStartOfDay,
                PercentalAbsenceOccursEndOfDay = dto.PercentalAbsenceOccursEndOfDay,
                AdjustAbsencePerWeekDay = dto.AdjustAbsencePerWeekDay,
                AdjustAbsenceAllDaysStart = dto.AdjustAbsenceAllDaysStart,
                AdjustAbsenceAllDaysStop = dto.AdjustAbsenceAllDaysStop,
                AdjustAbsenceMonStart = dto.AdjustAbsenceMonStart,
                AdjustAbsenceMonStop = dto.AdjustAbsenceMonStop,
                AdjustAbsenceTueStart = dto.AdjustAbsenceTueStart,
                AdjustAbsenceTueStop = dto.AdjustAbsenceTueStop,
                AdjustAbsenceWedStart = dto.AdjustAbsenceWedStart,
                AdjustAbsenceWedStop = dto.AdjustAbsenceWedStop,
                AdjustAbsenceThuStart = dto.AdjustAbsenceThuStart,
                AdjustAbsenceThuStop = dto.AdjustAbsenceThuStop,
                AdjustAbsenceFriStart = dto.AdjustAbsenceFriStart,
                AdjustAbsenceFriStop = dto.AdjustAbsenceFriStop,
                AdjustAbsenceSatStart = dto.AdjustAbsenceSatStart,
                AdjustAbsenceSatStop = dto.AdjustAbsenceSatStop,
                AdjustAbsenceSunStart = dto.AdjustAbsenceSunStart,
                AdjustAbsenceSunStop = dto.AdjustAbsenceSunStop,
            };
        }

        public static ExtendedAbsenceSettingDTO ToDTO(this ExtendedAbsenceSetting e)
        {
            if (e == null)
                return null;

            return new ExtendedAbsenceSettingDTO()
            {
                ExtendedAbsenceSettingId = e.ExtendedAbsenceSettingId,
                AbsenceFirstAndLastDay = e.AbsenceFirstAndLastDay,
                AbsenceWholeFirstDay = e.AbsenceWholeFirstDay,
                AbsenceFirstDayStart = e.AbsenceFirstDayStart,
                AbsenceWholeLastDay = e.AbsenceWholeLastDay,
                AbsenceLastDayStart = e.AbsenceLastDayStart,
                PercentalAbsence = e.PercentalAbsence,
                PercentalValue = e.PercentalValue,
                PercentalAbsenceOccursStartOfDay = e.PercentalAbsenceOccursStartOfDay,
                PercentalAbsenceOccursEndOfDay = e.PercentalAbsenceOccursEndOfDay,
                AdjustAbsencePerWeekDay = e.AdjustAbsencePerWeekDay,
                AdjustAbsenceAllDaysStart = e.AdjustAbsenceAllDaysStart,
                AdjustAbsenceAllDaysStop = e.AdjustAbsenceAllDaysStop,
                AdjustAbsenceMonStart = e.AdjustAbsenceMonStart,
                AdjustAbsenceMonStop = e.AdjustAbsenceMonStop,
                AdjustAbsenceTueStart = e.AdjustAbsenceTueStart,
                AdjustAbsenceTueStop = e.AdjustAbsenceTueStop,
                AdjustAbsenceWedStart = e.AdjustAbsenceWedStart,
                AdjustAbsenceWedStop = e.AdjustAbsenceWedStop,
                AdjustAbsenceThuStart = e.AdjustAbsenceThuStart,
                AdjustAbsenceThuStop = e.AdjustAbsenceThuStop,
                AdjustAbsenceFriStart = e.AdjustAbsenceFriStart,
                AdjustAbsenceFriStop = e.AdjustAbsenceFriStop,
                AdjustAbsenceSatStart = e.AdjustAbsenceSatStart,
                AdjustAbsenceSatStop = e.AdjustAbsenceSatStop,
                AdjustAbsenceSunStart = e.AdjustAbsenceSunStart,
                AdjustAbsenceSunStop = e.AdjustAbsenceSunStop,
            };
        }
    }
}
