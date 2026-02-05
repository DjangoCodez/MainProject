using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeMeeting : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeMeeting

        public static EmployeeMeetingDTO ToDTO(this EmployeeMeeting e, bool loadRelations)
        {
            if (e == null)
                return null;

            try
            {
                if (!e.IsAdded() && loadRelations)
                {
                    if (!e.FollowUpTypeReference.IsLoaded)
                    {
                        e.FollowUpTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeMeeting.cs e.FollowUpTypeReference");
                    }
                    if (!e.Participant.IsLoaded)
                    {
                        e.Participant.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeMeeting.cs e.Participant.IsLoaded");
                    }
                    if (!e.AttestRole.IsLoaded)
                    {
                        e.AttestRole.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeMeeting.cs e.AttestRole");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }


            EmployeeMeetingDTO dto = new EmployeeMeetingDTO()
            {
                EmployeeMeetingId = e.EmployeeMeetingId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                FollowUpTypeId = e.FollowUpTypeId,
                StartTime = e.StartTime,
                Reminder = e.Reminder,
                EmployeeCanEdit = e.EmployeeCanEdit,
                Note = e.Note,
                OtherParticipants = e.OtherParticipants,
                Completed = e.Completed,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            //Collections/Extensions
            if (e.Participant != null)
            {
                dto.ParticipantIds = e.Participant.Select(x => x.EmployeeId).ToList();
                dto.ParticipantNames = StringUtility.GetCommaSeparatedString(e.Participant.Select(x => x.Name).ToList(), false, true);
                if (!string.IsNullOrEmpty(dto.OtherParticipants))
                    dto.ParticipantNames += ";" + dto.OtherParticipants;
            }

            if (e.AttestRole != null)
                dto.AttestRoleIds = e.AttestRole.Select(x => x.AttestRoleId).ToList();
            if (e.FollowUpType != null)
                dto.FollowUpTypeName = e.FollowUpType.Name;
            dto.MeetingDateString = e.StartTime.ToShortDateShortTimeString();

            return dto;
        }

        public static IEnumerable<EmployeeMeetingDTO> ToDTOs(this IEnumerable<EmployeeMeeting> l, bool loadRelations)
        {
            var dtos = new List<EmployeeMeetingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(loadRelations));
                }
            }
            return dtos;
        }

        #endregion
    }
}
