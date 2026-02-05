using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class AttestRoleMapping : ICreatedModified, IState
    {
       
    }

    public static partial class EntityExtensions
    {
        public static IEnumerable<AttestRoleMappingDTO> ToDTOs(this IEnumerable<AttestRoleMapping> l)
        {
            var dtos = new List<AttestRoleMappingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #region AttestRoleMapping

        public static AttestRoleMappingDTO ToDTO(this AttestRoleMapping e)
        {
            if (e == null)
                return null;

            AttestRoleMappingDTO dto = new AttestRoleMappingDTO()
            {
                AttestRoleMappingId = e.AttestRoleMappingId,
                ParentAttestRoleId = e.ParentAttestRoleId,
                ChildtAttestRoleId = e.ChildAttestRoleId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                Entity = (TermGroup_AttestEntity)e.Entity,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        #endregion
    }
}
