using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ScheduleCycle : ICreatedModified, IState
    {

    }

    public partial class ScheduleCycleRule : ICreatedModified, IState
    {

    }

    public partial class ScheduleCycleRuleType : ICreatedModified, IState
    {
        public List<int> DayOfWeekIds { get; set; }
        public string DayOfWeeksGridString { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region ScheduleCycle

        public static ScheduleCycleDTO ToDTO(this ScheduleCycle e)
        {
            if (e == null)
                return null;

            return new ScheduleCycleDTO()
            {
                ScheduleCycleId = e.ScheduleCycleId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                NbrOfWeeks = e.NbrOfWeeks,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                ScheduleCycleRuleDTOs = e.ScheduleCycleRule?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<ScheduleCycleDTO> ToDTOs(this IEnumerable<ScheduleCycle> l)
        {
            var dtos = new List<ScheduleCycleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region ScheduleCycleRule

        public static ScheduleCycleRuleDTO ToDTO(this ScheduleCycleRule e)
        {
            if (e == null)
                return null;

            return new ScheduleCycleRuleDTO()
            {
                ScheduleCycleRuleId = e.ScheduleCycleRuleId,
                ScheduleCycleId = e.ScheduleCycleId,
                ScheduleCycleRuleTypeId = e.ScheduleCycleRuleTypeId,
                MinOccurrences = e.MinOccurrences,
                MaxOccurrences = e.MaxOccurrences,
                ScheduleCycleRuleTypeDTO = e.ScheduleCycleRuleType.ToDTO(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<ScheduleCycleRuleDTO> ToDTOs(this IEnumerable<ScheduleCycleRule> l)
        {
            var dtos = new List<ScheduleCycleRuleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ScheduleCycleGridDTO ToGridDTO(this ScheduleCycle scheduleCycle)
        {
            if (scheduleCycle == null)
                return null;

            return new ScheduleCycleGridDTO
            {
                ScheduleCycleId = scheduleCycle.ScheduleCycleId,
                Name = scheduleCycle.Name,
                Description = scheduleCycle.Description,
                NbrOfWeeks = scheduleCycle.NbrOfWeeks
            };
        }

        public static List<ScheduleCycleGridDTO> ToGridDTOs(this IEnumerable<ScheduleCycle> e)
        {
            var dtos=new List<ScheduleCycleGridDTO>();
            if (e != null)
            {
                foreach (var scheduleCycle in e)
                {
                    dtos.Add(scheduleCycle.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region ScheduleCycleRuleType

        public static ScheduleCycleRuleTypeDTO ToDTO(this ScheduleCycleRuleType e)
        {
            if (e == null)
                return null;

            ScheduleCycleRuleTypeDTO dto = new ScheduleCycleRuleTypeDTO()
            {
                ScheduleCycleRuleTypeId = e.ScheduleCycleRuleTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                DayOfWeeks = e.DayOfWeeks,
                DayOfWeeksGridString = e.DayOfWeeksGridString,
                DayOfWeekIds = e.DayOfWeekIds,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (dto.DayOfWeekIds == null && !String.IsNullOrEmpty(dto.DayOfWeeks))
            {
                dto.DayOfWeekIds = new List<int>();
                foreach (string sDayOfWeek in dto.DayOfWeeks.Split(','))
                {
                    if (Int32.TryParse(sDayOfWeek, out int iDayOfWeek))
                        dto.DayOfWeekIds.Add(iDayOfWeek);
                }
            }

            return dto;
        }

        public static IEnumerable<ScheduleCycleRuleTypeDTO> ToDTOs(this IEnumerable<ScheduleCycleRuleType> l)
        {
            var dtos = new List<ScheduleCycleRuleTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ScheduleCycleRuleTypeGridDTO ToGridDTO(this ScheduleCycleRuleType e)
        {
            if (e == null)
                return null;

            return new ScheduleCycleRuleTypeGridDTO()
            {
                ScheduleCycleRuleTypeId = e.ScheduleCycleRuleTypeId,
                Name = e.Name,
                DayOfWeeksGridString = e.DayOfWeeksGridString,
                StartTime = e.StartTime,
                StopTime = e.StopTime
            };
        }

        public static IEnumerable<ScheduleCycleRuleTypeGridDTO> ToGridDTOs(this IEnumerable<ScheduleCycleRuleType> l)
        {
            var dtos = new List<ScheduleCycleRuleTypeGridDTO>();
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
