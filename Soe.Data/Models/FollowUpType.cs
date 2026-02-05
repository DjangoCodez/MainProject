using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class FollowUpType : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region FollowUpType

        public static FollowUpTypeDTO ToDTO(this FollowUpType e)
        {
            if (e == null)
                return null;

            return new FollowUpTypeDTO()
            {
                FollowUpTypeId = e.FollowUpTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_FollowUpTypeType)e.Type,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<FollowUpTypeDTO> ToDTOs(this IEnumerable<FollowUpType> l)
        {
            var dtos = new List<FollowUpTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }

            }
            return dtos;
        }

        public static FollowUpTypeGridDTO ToGridDTO(this FollowUpType e)
        {
            if (e == null)
                return null;

            return new FollowUpTypeGridDTO()
            {
                FollowUpTypeId = e.FollowUpTypeId,
                Type = (TermGroup_FollowUpTypeType)e.Type,
                Name = e.Name,
                State = (SoeEntityState)e.State,

            };
        }

        public static IEnumerable<FollowUpTypeGridDTO> ToGridDTOs(this IEnumerable<FollowUpType> l)
        {
            var dtos = new List<FollowUpTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }

            }
            return dtos;
        }

        public static void CopyFrom(this FollowUpType e, FollowUpType source)
        {
            if (source == null || e == null)
                return;

            e.Type = source.Type;
            e.Name = source.Name;
            e.State = source.State;
        }

        #endregion
    }
}
