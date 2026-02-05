using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class OpeningHours : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region OpeningHours

        public static OpeningHoursDTO ToDTO(this OpeningHours e)
        {
            if (e == null)
                return null;

            return new OpeningHoursDTO()
            {
                OpeningHoursId = e.OpeningHoursId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                StandardWeekDay = e.StandardWeekDay,
                SpecificDate = e.SpecificDate,
                OpeningTime = e.OpeningTime,
                ClosingTime = e.ClosingTime,
                FromDate = e.FromDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };
        }

        public static List<OpeningHoursDTO> ToDTOs(this List<OpeningHours> l)
        {
            var dtos = new List<OpeningHoursDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static OpeningHoursGridDTO ToGridDTO(this OpeningHours e)
        {
            if (e == null)
                return null;

            return new OpeningHoursGridDTO()
            {
                OpeningHoursId = e.OpeningHoursId,
                Name = e.Name,
                Description = e.Description,
                StandardWeekDay = e.StandardWeekDay,
                SpecificDate = e.SpecificDate,
                OpeningTime = e.OpeningTime,
                ClosingTime = e.ClosingTime,
                FromDate = e.FromDate,
                State = e.State,
            };
        }

        public static IEnumerable<OpeningHoursGridDTO> ToGridDTOs(this IEnumerable<OpeningHours> l)
        {
            var dtos = new List<OpeningHoursGridDTO>();
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
