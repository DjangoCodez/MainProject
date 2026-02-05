using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EndReason : ICreatedModified, IState
    {
    }

    public static partial class EntityExtensions
    {
        public static EndReasonGridDTO ToGridDTO(this EndReasonDTO e)
        {
            if (e == null)
                return null;

            return new EndReasonGridDTO()
            {
                EndReasonId = e.EndReasonId,
                Name = e.Name,
                SystemEndReson = e.SystemEndReson,
                IsActive = e.SystemEndReson || e.State == SoeEntityState.Active,
            };
        }

        public static IEnumerable<EndReasonGridDTO> ToGridDTOs(this IEnumerable<EndReasonDTO> l)
        {
            var dtos = new List<EndReasonGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static EndReasonDTO ToDTO(this EndReason e)
        {
            if (e == null)
                return null;

            return new EndReasonDTO()
            {
                EndReasonId = e.EndReasonId,
                SystemEndReson = false,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Code = e.Code,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                IsActive = e.State == (int)SoeEntityState.Active,
            };
        }

        public static EndReasonDTO ToDTO(this KeyValuePair<int, string> e)
        {
            return new EndReasonDTO()
            {
                EndReasonId = e.Key,
                SystemEndReson = true,
                Name = e.Value,
                State = SoeEntityState.Active,
                IsActive = true,
            };
        }

        public static IEnumerable<EndReasonDTO> ToDTOs(this IEnumerable<EndReason> l, Dictionary<int, string> systemEndReasons)
        {
            var dtos = new List<EndReasonDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
                foreach (var e in systemEndReasons)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos.OrderBy(x => x.EndReasonId);
        }
    }
}
