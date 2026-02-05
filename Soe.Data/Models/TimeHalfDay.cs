using Newtonsoft.Json.Linq;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeHalfday : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeHalfday

        public static TimeHalfdayDTO ToDTO(this TimeHalfday e, bool includeDayType)
        {
            if (e == null)
                return null;

            TimeHalfdayDTO dto = new TimeHalfdayDTO()
            {
                TimeHalfdayId = e.TimeHalfdayId,
                DayTypeId = e.DayTypeId,
                Type = (SoeTimeHalfdayType)e.Type,
                Name = e.Name,
                Description = e.Description,
                Value = e.Value,
                TypeName = e.TypeName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (!e.TimeCodeBreak.IsNullOrEmpty())
                dto.TimeCodeBreaks = e.TimeCodeBreak.ToDTOs(false).ToList();
            if (includeDayType && e.DayType != null)
                dto.DayTypeName = e.DayType.Name;

            return dto;
        }

        public static IEnumerable<TimeHalfdayDTO> ToDTOs(this IEnumerable<TimeHalfday> l, bool includeDayType)
        {
            var dtos = new List<TimeHalfdayDTO>();
            if (l != null)
            {
                foreach (var e in l.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    dtos.Add(e.ToDTO(includeDayType));
                }
            }
            return dtos;
        }

        public static void CopyFrom(this TimeHalfday e, TimeHalfday source)
        {
            if (e == null || source == null)
                return;

            e.Name = source.Name;
            e.Description = source.Description;
            e.Type = source.Type;
            e.Value = source.Value;
            e.State = source.State;
        }

        public static TimeHalfdayGridDTO ToGridDTO(this TimeHalfday e)
        {
            if (e == null)
                return null;

            return new TimeHalfdayGridDTO()
            {
                TimeHalfdayId = e.TimeHalfdayId,
                Name = e.Name,
                Description = e.Description,
                Value = e.Value,
                TypeName = e.TypeName ?? "",
                DayTypeName = e.DayType?.Name ?? ""
            };
        }

        public static IEnumerable<TimeHalfdayGridDTO> ToGridDTOs(this IEnumerable<TimeHalfday> l)
        {
            var dtos = new List<TimeHalfdayGridDTO>();
            if (l != null)
            {
                foreach (var e in l.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static TimeHalfdayEditDTO ToEditDTO(this TimeHalfday e)
        {
            if (e == null)
                return null;

            TimeHalfdayEditDTO dto = new TimeHalfdayEditDTO()
            {
                TimeHalfdayId = e.TimeHalfdayId,
                DayTypeId = e.DayTypeId,
                Type = (SoeTimeHalfdayType)e.Type,
                Name = e.Name,
                Description = e.Description,
                Value = e.Value,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            dto.TimeCodeBreakIds = e.TimeCodeBreak.Any() ? e.TimeCodeBreak.Select(b => b.TimeCodeId).ToList() : new List<int>();

            return dto;
        }

        #endregion
    }
}
