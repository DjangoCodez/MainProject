using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Holiday : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region Holiday

        public static HolidayDTO ToDTO(this Holiday e, bool includeDayType)
        {
            if (e == null)
                return null;

            HolidayDTO dto = new HolidayDTO()
            {
                HolidayId = e.HolidayId,
                ActorCompanyId = e.ActorCompanyId,
                DayTypeId = e.DayTypeId,
                SysHolidayId = e.SysHolidayId,
                SysHolidayTypeId = e.SysHolidayTypeId,
                Date = e.Date,
                Name = e.Name,
                IsRedDay = e.IsRedDay,
                Description = e.Description,
                DayTypeName = e.DayType?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (includeDayType && e.DayType != null)
                dto.DayType = e.DayType.ToDTO();

            return dto;
        }

        public static IEnumerable<HolidayDTO> ToDTOs(this IEnumerable<Holiday> l, bool includeDayType, bool addEmptyRow = false)
        {
            if (l.IsNullOrEmpty())
                return new List<HolidayDTO>();

            var dtos = new List<HolidayDTO>();

            if (addEmptyRow && l.Any())
                dtos.Add(new HolidayDTO() { HolidayId = 0, Name = String.Empty, Date = CalendarUtility.DATETIME_DEFAULT });

            foreach (var e in l)
            {
                dtos.Add(e.ToDTO(includeDayType));
            }

            return dtos;
        }

        public static HolidaySmallDTO ToSmallDTO(this HolidayDTO e)
        {
            if (e == null)
                return null;

            HolidaySmallDTO dto = new HolidaySmallDTO()
            {
                HolidayId = e.HolidayId,
                Date = e.Date,
                Name = e.Name,
                Description = e.Description,
                IsRedDay = e.IsRedDay
            };

            // Remove asterix
            if (dto.Name.EndsWith("*"))
                dto.Name = dto.Name.Left(dto.Name.Length - 1);

            return dto;
        }

        public static IEnumerable<HolidaySmallDTO> ToSmallDTOs(this IEnumerable<HolidayDTO> l)
        {
            var dtos = new List<HolidaySmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static Holiday FromDTO(this HolidayDTO dto)
        {
            if (dto == null)
                return null;

            Holiday e = new Holiday()
            {
                HolidayId = dto.HolidayId,
                ActorCompanyId = dto.ActorCompanyId,
                DayTypeId = dto.DayTypeId,
                SysHolidayId = dto.SysHolidayId,
                SysHolidayTypeId = dto.SysHolidayTypeId,
                Date = dto.Date,
                Name = dto.Name,
                IsRedDay = dto.IsRedDay,
                Description = dto.Description,
                Created = dto.Created,
                CreatedBy = dto.CreatedBy,
                Modified = dto.Modified,
                ModifiedBy = dto.ModifiedBy,
                State = (int)dto.State,
            };

            return e;
        }

        public static void CopyFrom(this Holiday e, Holiday source)
        {
            if (source == null || e == null)
                return;

            e.SysHolidayId = source.SysHolidayId;
            e.Date = source.Date;
            e.Name = source.Name;
            e.Description = source.Description;
            e.State = source.State;
            e.SysHolidayId = source.SysHolidayId;
            e.SysHolidayTypeId = source.SysHolidayTypeId;
        }

        public static IEnumerable<HolidayGridDTO> ToGridDTOs(this IEnumerable<HolidayDTO> l)
        {
            var dtos = new List<HolidayGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static HolidayGridDTO ToGridDTO(this HolidayDTO e)
        {
            if (e == null)
                return null;

            HolidayGridDTO dto = new HolidayGridDTO()
            {
                HolidayId = e.HolidayId,
                Date = e.Date,
                Name = e.Name,
                Description = e.Description,
                DayTypeName = e.DayTypeName,
                SysHolidayTypeName = e.SysHolidayTypeName,
                State = e.State
            };

            return dto;
        }

        #endregion
    }
}
