using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleSwapRequest : ICreatedModified, IState
    {   
        
    }

    public partial class TimeScheduleSwapRequestRow : ICreatedModified, IState
    {
        public bool IsOngoing => this.Status == (int)TermGroup_TimeScheduleSwapRequestRowStatus.Initiated || this.Status == (int)TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByEmployee;
        public bool IsApproved => this.Status == (int)TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByAdmin;
    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleType

        public static TimeScheduleSwapRequestDTO ToDTO(this TimeScheduleSwapRequest e, bool includeRows)
        {
            if (e == null)
                return null;

            TimeScheduleSwapRequestDTO dto = new TimeScheduleSwapRequestDTO()
            {
               TimeScheduleSwapRequestId = e.TimeScheduleSwapRequestId,
                InitiatorUserId = e.InitiatorUserId,
                InitiatorEmployeeId = e.InitiatorEmployeeId,
                Comment = e.Comment,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Status = (TermGroup_TimeScheduleSwapRequestStatus)e.Status,
                State = (SoeEntityState)e.State,
                InitiatedDate = e.InitiatedDate,
                AcceptorUserId = e.AcceptorUserId,
                AcceptorUserName = e.AcceptorUser?.Name ?? string.Empty,
                AcceptorEmployeeId = e.AcceptorEmployeeId,
                AcceptedDate = e.AcceptedDate,
                ApprovedDate = e.ApprovedDate,
                ApprovedBy = e.ApprovedBy
            };

            if (includeRows)
            {
                dto.Rows = new List<TimeScheduleSwapRequestRowDTO>();
                if (!e.TimeScheduleSwapRequestRow.IsNullOrEmpty())
                    dto.Rows = e.TimeScheduleSwapRequestRow.Where(f => f.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            return dto;
        }

        public static IEnumerable<TimeScheduleSwapRequestDTO> ToDTOs(this IEnumerable<TimeScheduleSwapRequest> l, bool includeRows)
        {
            var dtos = new List<TimeScheduleSwapRequestDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        public static TimeScheduleSwapRequestRowDTO ToDTO(this TimeScheduleSwapRequestRow e)
        {
            if (e == null)
                return null;

            return new TimeScheduleSwapRequestRowDTO()
            {
                TimeScheduleSwapRequestRowId = e.TimeScheduleSwapRequestRowId,
                TimeScheduleSwapRequestId = e.TimeScheduleSwapRequestId,
                EmployeeId = e.EmployeeId,
                Date = e.Date,
                ShiftsInfo = e.ShiftsInfo,
                ScheduleStart = e.ScheduleStart,
                ScheduleStop = e.ScheduleStop,
                Status = (TermGroup_TimeScheduleSwapRequestRowStatus)e.Status,                
                State = (SoeEntityState)e.State,
                ShiftLength = e.ShiftLength,
            };
        }

        public static IEnumerable<TimeScheduleSwapRequestRowDTO> ToDTOs(this IEnumerable<TimeScheduleSwapRequestRow> l)
        {
            var dtos = new List<TimeScheduleSwapRequestRowDTO>();
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
