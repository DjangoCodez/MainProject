using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodeBreakGroup
    {
    }

    public static partial class EntityExtensions
    {
        public static TimeCodeBreakGroupDTO ToDTO(this TimeCodeBreakGroup e)
        {
            if (e == null)
                return null;

            TimeCodeBreakGroupDTO dto = new TimeCodeBreakGroupDTO()
            {
                TimeCodeBreakGroupId = e.TimeCodeBreakGroupId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.TimeCodeBreak != null)
                dto.TimeCodeBreaks = e.TimeCodeBreak.ToDTOs(false).ToList();

            return dto;
        }

        public static TimeCodeBreakGroupGridDTO ToGridDTO(this TimeCodeBreakGroup e)
        {
            if (e == null)
                return null;

            return new TimeCodeBreakGroupGridDTO()
            {
                TimeCodeBreakGroupId = e.TimeCodeBreakGroupId,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static IEnumerable<TimeCodeBreakGroupDTO> ToDTOs(this IEnumerable<TimeCodeBreakGroup> l)
        {
            var dtos = new List<TimeCodeBreakGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<TimeCodeBreakGroupGridDTO> ToGridDTOs(this IEnumerable<TimeCodeBreakGroup> l)
        {
            var dtos = new List<TimeCodeBreakGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }
    }
}
