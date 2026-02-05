using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class SchoolHoliday : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region SchoolHoliday

        public static SchoolHolidayDTO ToDTO(this SchoolHoliday e)
        {
            if (e == null)
                return null;

            return new SchoolHolidayDTO()
            {
                SchoolHolidayId = e.SchoolHolidayId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                IsSummerHoliday = e.IsSummerHoliday,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty
            };
        }

        public static IEnumerable<SchoolHolidayDTO> ToDTOs(this IEnumerable<SchoolHoliday> l)
        {
            var dtos = new List<SchoolHolidayDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SchoolHolidayGridDTO ToGridDTO(this SchoolHoliday e)
        {
            if (e == null)
                return null;

            return new SchoolHolidayGridDTO()
            {
                SchoolHolidayId = e.SchoolHolidayId,
                Name = e.Name,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<SchoolHolidayGridDTO> ToGridDTOs(this IEnumerable<SchoolHoliday> l)
        {
            var dtos = new List<SchoolHolidayGridDTO>();
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
