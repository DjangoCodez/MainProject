using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeLeisureCode : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeLeisureCode

        public static TimeLeisureCodeDTO ToDTO(this TimeLeisureCode e)
        {
            if (e == null)
                return null;

            TimeLeisureCodeDTO dto = new TimeLeisureCodeDTO()
            {
                TimeLeisureCodeId = e.TimeLeisureCodeId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_TimeLeisureCodeType)e.Type,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<TimeLeisureCodeDTO> ToDTOs(this IEnumerable<TimeLeisureCode> l)
        {
            var dtos = new List<TimeLeisureCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeLeisureCodeSmallDTO ToSmallDTO(this TimeLeisureCode e)
        {
            if (e == null)
                return null;

            TimeLeisureCodeSmallDTO dto = new TimeLeisureCodeSmallDTO()
            {
                TimeLeisureCodeId = e.TimeLeisureCodeId,
                Code = e.Code,
                Name = e.Name,
            };

            return dto;
        }

        public static IEnumerable<TimeLeisureCodeSmallDTO> ToSmallDTOs(this IEnumerable<TimeLeisureCode> l)
        {
            var dtos = new List<TimeLeisureCodeSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static TimeLeisureCodeGridDTO ToGridDTO(this TimeLeisureCode e)
        {
            if (e == null)
                return null;

            TimeLeisureCodeGridDTO dto = new TimeLeisureCodeGridDTO()
            {
                TimeLeisureCodeId = e.TimeLeisureCodeId,
                Type = (TermGroup_TimeLeisureCodeType)e.Type,
                TypeName = e.TypeName,
                Code = e.Code,
                Name = e.Name,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<TimeLeisureCodeGridDTO> ToGridDTOs(this IEnumerable<TimeLeisureCode> l)
        {
            var dtos = new List<TimeLeisureCodeGridDTO>();
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
