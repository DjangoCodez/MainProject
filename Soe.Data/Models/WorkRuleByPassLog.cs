using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class WorkRuleBypassLog : ICreatedModified, IState
    {
        public string ActionText { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region WorkRuleBypassLog

        public static WorkRuleBypassLogDTO ToDTO(this WorkRuleBypassLog e)
        {
            if (e == null)
                return null;

            return new WorkRuleBypassLogDTO
            {
                WorkRuleBypassLogId = e.WorkRuleBypassLogId,
                ActorCompanyId = e.ActorCompanyId,
                UserId = e.UserId,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee?.Name ?? string.Empty,
                WorkRule = (SoeScheduleWorkRules)e.WorkRule,
                Action = (TermGroup_ShiftHistoryType)e.Action,
                Date = e.Date,
                Message = e.Message,
                ActionText = e.ActionText,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<WorkRuleBypassLogDTO> ToDTOs(this IEnumerable<WorkRuleBypassLog> l)
        {
            var dtos = new List<WorkRuleBypassLogDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static WorkRuleBypassLogGridDTO ToGridDTO(this WorkRuleBypassLog e)
        {
            if (e == null)
                return null;

            return new WorkRuleBypassLogGridDTO
            {
                WorkRuleBypassLogId = e.WorkRuleBypassLogId,
                Date = e.Date,
                EmployeeNrAndName = e.Employee?.NumberAndName ?? string.Empty,
                Message = e.Message,
                ActionText = e.ActionText,
                CreatedBy = e.CreatedBy,
            };
        }

        public static IEnumerable<WorkRuleBypassLogGridDTO> ToGridDTOs(this IEnumerable<WorkRuleBypassLog> l)
        {
            var dtos = new List<WorkRuleBypassLogGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static WorkRuleBypassLogGridDTO ToGridDTO(this WorkRuleBypassLogDTO dto)
        {
            if (dto == null)
                return null;

            return new WorkRuleBypassLogGridDTO
            {
                WorkRuleBypassLogId = dto.WorkRuleBypassLogId,
                Date = dto.Date,
                EmployeeNrAndName = dto.EmployeeNrAndName,
                Message = dto.Message,
                ActionText = dto.ActionText,
                CreatedBy = dto.CreatedBy,
            };
        }

        public static IEnumerable<WorkRuleBypassLogGridDTO> ToGridDTOs(this IEnumerable<WorkRuleBypassLogDTO> l)
        {
            var dtos = new List<WorkRuleBypassLogGridDTO>();
            if (l != null)
            {
                foreach (var dto in l)
                {
                    dtos.Add(dto.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
