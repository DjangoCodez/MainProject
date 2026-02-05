using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleEvent : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleEvent

        public static TimeScheduleEventDTO ToDTO(this TimeScheduleEvent e)
        {
            if (e == null)
                return null;

            TimeScheduleEventDTO dto = new TimeScheduleEventDTO
            {
                TimeScheduleEventId = e.TimeScheduleEventId,
                ActorCompanyId = e.ActorCompanyId,
                Date = e.Date,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.TimeScheduleEventMessageGroup != null)
                dto.TimeScheduleEventMessageGroups = e.TimeScheduleEventMessageGroup.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<TimeScheduleEventDTO> ToDTOs(this IEnumerable<TimeScheduleEvent> l)
        {
            var dtos = new List<TimeScheduleEventDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleEventGridDTO ToGridDTO(this TimeScheduleEvent entity)
        {
            if (entity == null) return null;
            var recipientGroupNames = string.Empty;
            if (entity.TimeScheduleEventMessageGroup != null && entity.TimeScheduleEventMessageGroup.Any())
            {
                var groupNames = entity.TimeScheduleEventMessageGroup
                    .Where(temg => temg.MessageGroup != null)
                    .Select(temg => temg.MessageGroup.Name)
                    .OrderBy(name => name);
                recipientGroupNames = string.Join(", ", groupNames);
            }
            return new TimeScheduleEventGridDTO
            {
                TimeScheduleEventId = entity.TimeScheduleEventId,
                Date = entity.Date,
                Name = entity.Name,
                Description = entity.Description,
                RecipientGroupNames = recipientGroupNames
            };
        }


        public static IEnumerable<TimeScheduleEventGridDTO> ToGridDTOs(this IEnumerable<TimeScheduleEvent> entities)
        {
            var dtos = new List<TimeScheduleEventGridDTO>();
            if (entities != null)
            {
                foreach (var e in entities)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }
        #endregion

        #region TimeScheduleEventMessageGroup

        public static TimeScheduleEventMessageGroupDTO ToDTO(this TimeScheduleEventMessageGroup e)
        {
            if (e == null)
                return null;

            return new TimeScheduleEventMessageGroupDTO
            {
                TimeScheduleEventMessageGroupId = e.TimeScheduleEventMessageGroupId,
                TimeScheduleEventId = e.TimeScheduleEventId,
                MessageGroupId = e.MessageGroupId,
            };
        }

        public static IEnumerable<TimeScheduleEventMessageGroupDTO> ToDTOs(this IEnumerable<TimeScheduleEventMessageGroup> l)
        {
            var dtos = new List<TimeScheduleEventMessageGroupDTO>();
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
    }
}
