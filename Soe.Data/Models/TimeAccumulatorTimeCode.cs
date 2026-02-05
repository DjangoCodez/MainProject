using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeAccumulatorTimeCode
    {
        public static TimeAccumulatorTimeCode Create(TimeAccumulator timeAccumulator, TimeCode timeCode, decimal factor, bool importDefault = false, bool isHeadTimeCode = false)
        {
            if (timeAccumulator == null || timeCode == null)
                return null;

            var timeAccumulatorTimeCode = new TimeAccumulatorTimeCode()
            {
                TimeAccumulator = timeAccumulator,
                TimeCode = timeCode,
                ImportDefault = importDefault,
                IsHeadTimeCode = isHeadTimeCode,
            };
            timeAccumulatorTimeCode.SetFactor(factor);
            return timeAccumulatorTimeCode;
        }

        public void Update(TimeCode timeCode, decimal factor, bool importDefault = false, bool isHeadTimeCode = false)
        {
            this.TimeCode = timeCode;
            this.ImportDefault = importDefault;
            this.IsHeadTimeCode = isHeadTimeCode;
            this.SetFactor(factor);
        }

        private void SetFactor(decimal factor)
        {
            this.Factor = Decimal.Round(factor, 5, MidpointRounding.AwayFromZero);
        }
    }

    public static partial class EntityExtensions
    {
        public static TimeAccumulatorTimeCodeDTO ToDTO(this TimeAccumulatorTimeCode e)
        {
            if (e == null)
                return null;

            return new TimeAccumulatorTimeCodeDTO()
            {
                TimeCodeId = e.TimeCodeId,
                Factor = e.Factor,
                IsHeadTimeCode = e.IsHeadTimeCode,
                ImportDefault = e.ImportDefault
            };
        }

        public static List<TimeAccumulatorTimeCodeDTO> ToDTOs(this IEnumerable<TimeAccumulatorTimeCode> l)
        {
            var dtos = new List<TimeAccumulatorTimeCodeDTO>();
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
