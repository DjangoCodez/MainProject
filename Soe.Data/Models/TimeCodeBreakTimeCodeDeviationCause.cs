using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodeBreakTimeCodeDeviationCause
    {
    }

    public static partial class EntityExtensions
    {
        public static TimeCodeBreakTimeCodeDeviationCauseDTO ToDTO(this TimeCodeBreakTimeCodeDeviationCause e)
        {
            if (e == null)
                return null;

            return new TimeCodeBreakTimeCodeDeviationCauseDTO()
            {
                TimeCodeBreakTimeCodeDeviationCauseId = e.TimeCodeBreakTimeCodeDeviationCauseId,
                TimeCodeBreakId = e.TimeCodeBreak?.TimeCodeId ?? 0,
                TimeCodeDeviationCauseId = e.TimeDeviationCause?.TimeDeviationCauseId ?? 0,
                TimeCodeId = e.TimeCode?.TimeCodeId ?? 0
            };
        }

        public static List<TimeCodeBreakTimeCodeDeviationCauseDTO> ToDTOs(this IEnumerable<TimeCodeBreakTimeCodeDeviationCause> l)
        {
            List<TimeCodeBreakTimeCodeDeviationCauseDTO> dtos = new List<TimeCodeBreakTimeCodeDeviationCauseDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
    }
}
