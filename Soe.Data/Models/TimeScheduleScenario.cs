using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleScenarioHead : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleScenarioAccount

        public static TimeScheduleScenarioAccountDTO ToDTO(this TimeScheduleScenarioAccount e)
        {
            if (e == null)
                return null;

            return new TimeScheduleScenarioAccountDTO
            {
                TimeScheduleScenarioAccountId = e.TimeScheduleScenarioAccountId,
                TimeScheduleScenarioHeadId = e.TimeScheduleScenarioHeadId,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<TimeScheduleScenarioAccountDTO> ToDTOs(this IEnumerable<TimeScheduleScenarioAccount> l)
        {
            var dtos = new List<TimeScheduleScenarioAccountDTO>();
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

        #region TimeScheduleScenarioHead

        public static TimeScheduleScenarioHeadDTO ToDTO(this TimeScheduleScenarioHead e)
        {
            if (e == null)
                return null;

            TimeScheduleScenarioHeadDTO dto = new TimeScheduleScenarioHeadDTO
            {
                TimeScheduleScenarioHeadId = e.TimeScheduleScenarioHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                SourceType = (TermGroup_TimeScheduleScenarioHeadSourceType)e.SourceType,
                SourceDateFrom = e.SourceDateFrom,
                SourceDateTo = e.SourceDateTo,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            dto.Employees = e.TimeScheduleScenarioEmployee.ToDTOs().ToList();
            dto.Accounts = e.TimeScheduleScenarioAccount.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<TimeScheduleScenarioHeadDTO> ToDTOs(this IEnumerable<TimeScheduleScenarioHead> l)
        {
            var dtos = new List<TimeScheduleScenarioHeadDTO>();
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

        #region TimeScheduleScenarioEmployee

        public static TimeScheduleScenarioEmployeeDTO ToDTO(this TimeScheduleScenarioEmployee e)
        {
            if (e == null)
                return null;

            return new TimeScheduleScenarioEmployeeDTO
            {
                TimeScheduleScenarioEmployeeId = e.TimeScheduleScenarioEmployeeId,
                TimeScheduleScenarioHeadId = e.TimeScheduleScenarioHeadId,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<TimeScheduleScenarioEmployeeDTO> ToDTOs(this IEnumerable<TimeScheduleScenarioEmployee> l)
        {
            var dtos = new List<TimeScheduleScenarioEmployeeDTO>();
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
