using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ReportUserSelection : ICreatedModified, IState
    {

    }

    public partial class ReportUserSelectionAccess : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportUserSelection

        public static ReportUserSelectionDTO ToDTO(this ReportUserSelection e)
        {
            if (e == null)
                return null;

            ReportUserSelectionDTO dto = new ReportUserSelectionDTO()
            {
                ReportUserSelectionId = e.ReportUserSelectionId,
                ReportId = e.ReportId,
                ActorCompanyId = e.ActorCompanyId,
                UserId = e.UserId,
                Type = (ReportUserSelectionType)e.Type,
                Name = e.Name,
                Description = e.Description,
                Selections = ReportDataSelectionDTO.FromJSON(e.Selection),
                State = (SoeEntityState)e.State,
                ScheduledJobHeadId = e.ScheduledJobHeadId
            };

            if (e.ReportUserSelectionAccess != null)
                dto.Access = e.ReportUserSelectionAccess.Where(a => a.State == (int)SoeEntityState.Active).ToDTOs();
            if (e.Report?.SysReportTemplateTypeId != null)
                dto.ValidForScheduledJobHead = e.Report.ValidForScheduledJob;

            return dto;
        }

        public static List<ReportUserSelectionDTO> ToDTOs(this IEnumerable<ReportUserSelection> l)
        {
            List<ReportUserSelectionDTO> dtos = new List<ReportUserSelectionDTO>();
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

        #region ReportUserSelectionAccess

        public static ReportUserSelectionAccessDTO ToDTO(this ReportUserSelectionAccess e)
        {
            if (e == null)
                return null;

            return new ReportUserSelectionAccessDTO()
            {
                ReportUserSelectionAccessId = e.ReportUserSelectionAccessId,
                ReportUserSelectionId = e.ReportUserSelectionId,
                Type = (TermGroup_ReportUserSelectionAccessType)e.Type,
                RoleId = e.RoleId,
                MessageGroupId = e.MessageGroupId,
                State = (SoeEntityState)e.State
            };
        }

        public static List<ReportUserSelectionAccessDTO> ToDTOs(this IEnumerable<ReportUserSelectionAccess> l)
        {
            List<ReportUserSelectionAccessDTO> dtos = new List<ReportUserSelectionAccessDTO>();
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
