using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodeRanking : ICreatedModified, IState
    {
        public static TimeCodeRanking Create(int actorCompanyId, int leftTimeCodeId, int rightTimeCodeId, int operatorType, int timeCodeRankingGroup)
        {
            return new TimeCodeRanking
            {
                ActorCompanyId = actorCompanyId,
                LeftTimeCodeId = leftTimeCodeId,
                RightTimeCodeId = rightTimeCodeId,
                OperatorType = operatorType,
                TimeCodeRankingGroupId = timeCodeRankingGroup,
                State = (int)SoeEntityState.Active
            };
        }

        public void Update(int operatorType)
        {
            this.OperatorType = operatorType;
        }

        public void Delete()
        {
            this.State = (int)SoeEntityState.Deleted;
        }
    }
    public partial class TimeCodeRankingGroup : ICreatedModified, IState
    {

    }
    public static partial class EntityExtensions
    {
        #region TimeCodeRankingGroupGridDTO
        public static IEnumerable<TimeCodeRankingGroupGridDTO> ToGridDTOs(this IEnumerable<TimeCodeRankingGroup> l)
        {
            var dtos = new List<TimeCodeRankingGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }

            }
            return dtos;
        }

        public static TimeCodeRankingGroupGridDTO ToGridDTO(this TimeCodeRankingGroup e)
        {
            if (e == null)
                return null;

            TimeCodeRankingGroupGridDTO dto = new TimeCodeRankingGroupGridDTO()
            {
                TimeCodeRankingGroupId = e.TimeCodeRankingGroupId,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Description = e.Description,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        #endregion

        #region TimeCodeRankingGroupDTO
        public static IEnumerable<TimeCodeRankingGroupDTO> ToDTOs(this IEnumerable<TimeCodeRankingGroup> l)
        {
            var dtos = new List<TimeCodeRankingGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }

            }
            return dtos;
        }

        public static TimeCodeRankingGroupDTO ToDTO(this TimeCodeRankingGroup e, List<TimeCodeRankingDTO> timeCodeRankings = null)
        {
            if (e == null)
                return null;

            return new TimeCodeRankingGroupDTO()
            {
                TimeCodeRankingGroupId = e.TimeCodeRankingGroupId,
                ActorCompanyId = e.ActorCompanyId,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Description = e.Description,
                State = (SoeEntityState)e.State,
                TimeCodeRankings = timeCodeRankings?.OrderBy(o => o.LeftTimeCodeName).ToList(),
            };
        }

        #endregion


    }
}


