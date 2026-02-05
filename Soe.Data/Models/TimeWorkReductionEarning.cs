using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class TimeWorkReductionEarning : ICreatedModified, IState
    {
        public static TimeWorkReductionEarning Create(int minutesWeight, TermGroup_TimeWorkReductionPeriodType periodType, TimeAccumulator timeAccumulator)
        {
            return Create(minutesWeight, (int)periodType, timeAccumulator);
        }
        public static TimeWorkReductionEarning Create(int minutesWeight, int periodType, TimeAccumulator timeAccumulator)
        {
            if (timeAccumulator == null)
                return null;

            TimeWorkReductionEarning timeWorkReductionEarning = new TimeWorkReductionEarning()
            {
                MinutesWeight = minutesWeight,
                PeriodType = periodType,
            };
            timeAccumulator.TimeWorkReductionEarning = timeWorkReductionEarning;
            return timeWorkReductionEarning;
        }

        public void Update(int minutesWeight, int periodType)
        {
            this.MinutesWeight = minutesWeight;
            this.PeriodType = periodType;
        }
        public void Update(int minutesWeight, TermGroup_TimeWorkReductionPeriodType periodType)
        {
            this.MinutesWeight = minutesWeight;
            this.PeriodType = (int)periodType;
        }
    }

    public static partial class EntityExtensions
    {
        public static TimeWorkReductionEarningDTO ToDTO(this TimeWorkReductionEarning e, bool useTimeAccumulatorTimeWorkReductionEarningEmployeeGroup = true)
        {
            if (e == null)
                return null;

            TimeWorkReductionEarningDTO dto = new TimeWorkReductionEarningDTO()
            {

                TimeWorkReductionEarningId = e.TimeWorkReductionEarningId,
                MinutesWeight = e.MinutesWeight,
                PeriodType = e.PeriodType,
                State = (SoeEntityState)e.State,
            };

            if (useTimeAccumulatorTimeWorkReductionEarningEmployeeGroup && e.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup != null)
                dto.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup = e.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs();

            if (e.TimeAccumulator != null)
                dto.TimeAccumulatorName = e.TimeAccumulator.FirstOrDefault(w => w.TimeWorkReductionEarningId == e.TimeWorkReductionEarningId)?.Name ?? "";

            return dto;
        }

        public static List<TimeWorkReductionEarningDTO> ToDTOs(this IEnumerable<TimeWorkReductionEarning> l, bool useTimeAccumulatorTimeWorkReductionEarningEmployeeGroup = true)
        {
            var dtos = new List<TimeWorkReductionEarningDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(useTimeAccumulatorTimeWorkReductionEarningEmployeeGroup));
                }
            }
            return dtos;
        }
    }
}
