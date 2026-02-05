using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class PayrollLevel : ICreatedModified, IState
    {
        public string CodeAndName
        {
            get
            {
                return $"{this.Code ?? this.ExternalCode} ({this.Name})";
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region PayrollLevel

        public static PayrollLevelDTO ToDTO(this PayrollLevel e)
        {
            if (e == null)
                return null;

            return new PayrollLevelDTO()
            {
               PayrollLevelId = e.PayrollLevelId,
               ActorCompanyId = e.ActorCompanyId,
               ExternalCode = e.ExternalCode,
               Code = e.Code,
               Name = e.Name,
               Description = e.Description,
               Created = e.Created,
               CreatedBy = e.CreatedBy,
               Modified = e.Modified,
               ModifiedBy = e.ModifiedBy,
               State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<PayrollLevelDTO> ToDTOs(this IEnumerable<PayrollLevel> l)
        {
            var dtos = new List<PayrollLevelDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static PayrollLevelGridDTO ToGridDTO(this PayrollLevel e)
        {
            if (e == null)
                return null;

            return new PayrollLevelGridDTO()
            {
                PayrollLevelId = e.PayrollLevelId,                
                ExternalCode = e.ExternalCode,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,                
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<PayrollLevelGridDTO> ToGridDTOs(this IEnumerable<PayrollLevel> l)
        {
            var dtos = new List<PayrollLevelGridDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}

