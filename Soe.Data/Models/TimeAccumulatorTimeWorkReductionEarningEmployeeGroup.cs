using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeAccumulatorTimeWorkReductionEarningEmployeeGroup : ICreatedModified, IState
    {
    }

    public static partial class EntityExtensions
    {
        public static TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO ToDTO(this TimeAccumulatorTimeWorkReductionEarningEmployeeGroup e)
        {
            if (e == null)
                return null;

            TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO dto = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO()
            {
                TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId = e.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId,
                EmployeeGroupId = e.EmployeeGroupId,
                TimeWorkReductionEarningId = e.TimeWorkReductionEarningId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                State = (SoeEntityState)e.State,
            };

            if (e.EmployeeGroup != null)
                dto.EmployeeGroup = e.EmployeeGroup.ToDTO();

            if (e.TimeWorkReductionEarning != null)
                dto.TimeWorkReductionEarning = e.TimeWorkReductionEarning.ToDTO(false);

            return dto;
        }

        public static List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO> ToDTOs(this IEnumerable<TimeAccumulatorTimeWorkReductionEarningEmployeeGroup> l)
        {
            var dtos = new List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO>();
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
