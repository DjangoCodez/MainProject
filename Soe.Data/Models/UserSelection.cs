using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class UserSelection : ICreatedModified, IState
    {

    }

    public partial class UserSelectionAccess : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region UserSelection

        public static UserSelectionDTO ToDTO(this UserSelection e)
        {
            if (e == null)
                return null;

            bool schedulePlanning = e.Type == (int)UserSelectionType.SchedulePlanningView_Day || e.Type == (int)UserSelectionType.SchedulePlanningView_Schedule;

            UserSelectionDTO dto = new UserSelectionDTO()
            {
                UserSelectionId = e.UserSelectionId,
                ActorCompanyId = e.ActorCompanyId,
                UserId = e.UserId,
                Type = (UserSelectionType)e.Type,
                Default = e.Default,
                Name = e.Name,
                Description = e.Description,
                Access = e.UserSelectionAccess?.Where(a => a.State == (int)SoeEntityState.Active).ToDTOs(),
                State = (SoeEntityState)e.State
            };

            if (schedulePlanning)
                dto.Selection = e.Selection;
            else
                dto.Selections = ReportDataSelectionDTO.FromJSON(e.Selection);

            return dto;
        }

        public static List<UserSelectionDTO> ToDTOs(this IEnumerable<UserSelection> l)
        {
            List<UserSelectionDTO> dtos = new List<UserSelectionDTO>();
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

        #region UserSelectionAccess

        public static UserSelectionAccessDTO ToDTO(this UserSelectionAccess e)
        {
            if (e == null)
                return null;

            return new UserSelectionAccessDTO()
            {
                UserSelectionAccessId = e.UserSelectionAccessId,
                UserSelectionId = e.UserSelectionId,
                Type = (TermGroup_ReportUserSelectionAccessType)e.Type,
                RoleId = e.RoleId,
                MessageGroupId = e.MessageGroupId,
                State = (SoeEntityState)e.State
            };
        }

        public static List<UserSelectionAccessDTO> ToDTOs(this IEnumerable<UserSelectionAccess> l)
        {
            List<UserSelectionAccessDTO> dtos = new List<UserSelectionAccessDTO>();
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
