using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class DayType : ICreatedModified, IState
    {
        public bool Import { get; set; }
    }

    public static partial class EntityExtensions
    {
        public static DayTypeDTO ToDTO(this DayType e)
        {
            if (e == null)
                return null;

            return new DayTypeDTO()
            {
                DayTypeId = e.DayTypeId,
                ActorCompanyId = e.ActorCompanyId,
                SysDayTypeId = e.SysDayTypeId,
                Type = (TermGroup_SysDayType)e.Type,
                Name = e.Name,
                Description = e.Description,
                StandardWeekdayFrom = e.StandardWeekdayFrom,
                StandardWeekdayTo = e.StandardWeekdayTo,
                Import = e.Import,
                TimeHalfdays = e.TimeHalfday?.ToDTOs(true).ToList() ?? new List<TimeHalfdayDTO>(),
                EmployeeGroups = e.EmployeeGroup?.ToDTOs(false).ToList() ?? new List<EmployeeGroupDTO>(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                WeekendSalary = e.WeekendSalary
            };
        }

        public static IEnumerable<DayTypeDTO> ToDTOs(this IEnumerable<DayType> l)
        {
            var dtos = new List<DayTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static DayTypeGridDTO ToGridDTO(this DayType e)
        {
            if (e == null)
                return null;

            return new DayTypeGridDTO()
            {
                DayTypeId = e.DayTypeId,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<DayTypeGridDTO> ToGridDTOs(this IEnumerable<DayType> l)
        {
            var dtos = new List<DayTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static void CopyFrom(this DayType e, DayType source)
        {
            if (source == null || e == null)
                return;

            e.SysDayTypeId = source.SysDayTypeId;
            e.Name = source.Name;
            e.Description = source.Description;
            e.StandardWeekdayFrom = source.StandardWeekdayFrom;
            e.StandardWeekdayTo = source.StandardWeekdayTo;
            e.Type = source.Type;
            e.State = source.State;
        }
    }
}
