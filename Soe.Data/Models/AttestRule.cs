using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AttestRuleHead : ICreatedModified, IState
    {
    }

    public static partial class EntityExtensions
    {
        #region AttestRuleHead

        public static AttestRuleHeadDTO ToDTO(this AttestRuleHead e, bool includeRows, bool includeEmployeeGroups, bool setDayTypeName)
        {
            if (e == null)
                return null;

            AttestRuleHeadDTO dto = new AttestRuleHeadDTO()
            {
                AttestRuleHeadId = e.AttestRuleHeadId,
                ActorCompanyId = e.ActorCompanyId,
                DayTypeId = e.DayTypeId,
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (includeRows)
                dto.AttestRuleRows = e.AttestRuleRow?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<AttestRuleRowDTO>();
            if (includeEmployeeGroups)
                dto.EmployeeGroupIds = e.EmployeeGroup?.Select(eg => eg.EmployeeGroupId).ToList() ?? new List<int>();
            if (setDayTypeName && e.DayTypeId != null)
                dto.DayTypeName = e.DayType.Name;

            return dto;
        }
        
        public static IEnumerable<AttestRuleHeadDTO> ToDTOs(this IEnumerable<AttestRuleHead> l, bool includeRows, bool includeEmployeeGroups, bool setDayTypeName)
        {
            var dtos = new List<AttestRuleHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, includeEmployeeGroups, setDayTypeName));
                }
            }
            return dtos;
        }

        public static AttestRuleHeadGridDTO ToGridDTO(this AttestRuleHead e)
        {
            if (e == null)
                return null;

            AttestRuleHeadGridDTO dto = new AttestRuleHeadGridDTO()
            {
                AttestRuleHeadId = e.AttestRuleHeadId,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State,
                DayTypeName = e.DayTypeId.HasValue ? e.DayType.Name : string.Empty
            };

            //Extensions
            dto.EmployeeGroupNames = e.EmployeeGroup?.OrderBy(eg => eg.Name).Select(eg => eg.Name).ToCommaSeparated(addWhiteSpace: true) ?? string.Empty;

            return dto;
        }

        public static IEnumerable<AttestRuleHeadGridDTO> ToGridDTOs(this IEnumerable<AttestRuleHead> l)
        {
            var dtos = new List<AttestRuleHeadGridDTO>();
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

        #region AttestRuleRow

        public static AttestRuleRowDTO ToDTO(this AttestRuleRow e)
        {
            if (e == null)
                return null;

            return new AttestRuleRowDTO()
            {
                AttestRuleRowId = e.AttestRuleRowId,
                AttestRuleHeadId = e.AttestRuleHeadId,
                LeftValueType = (TermGroup_AttestRuleRowLeftValueType)e.LeftValueType,
                LeftValueId = e.LeftValueId,
                RightValueType = (TermGroup_AttestRuleRowRightValueType)e.RightValueType,
                RightValueId = e.RightValueId,
                Minutes = e.Minutes,
                ComparisonOperator = (WildCard)e.ComparisonOperator,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static List<AttestRuleRow> FromDTOs(this List<AttestRuleRowDTO> l)
        {
            List<AttestRuleRow> rows = new List<AttestRuleRow>();
            if (l != null)
            {
                foreach (AttestRuleRowDTO e in l)
                {
                    rows.Add(e.FromDTO());
                }
            }
            return rows;
        }

        public static AttestRuleRow FromDTO(this AttestRuleRowDTO e)
        {
            return new AttestRuleRow()
            {
                AttestRuleRowId = e.AttestRuleRowId,
                LeftValueType = (int)(e.LeftValueType ?? 0),
                LeftValueId = e.LeftValueId ?? 0,
                ComparisonOperator = (int)e.ComparisonOperator,
                RightValueType = (int)(e.RightValueType ?? 0),
                RightValueId = e.RightValueId ?? 0,
                Minutes = e.Minutes
            };
        }

        public static IEnumerable<AttestRuleRowDTO> ToDTOs(this IEnumerable<AttestRuleRow> l)
        {
            var dtos = new List<AttestRuleRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool ContainsAnyRowWithType(this List<AttestRuleHead> l, TermGroup_AttestRuleRowLeftValueType leftValueType, TermGroup_AttestRuleRowRightValueType rightValueType)
        {
            if (l.IsNullOrEmpty())
                return false;

            foreach (var e in l)
            {
                if (e.AttestRuleRow.Any(row => row.LeftValueType == (int)leftValueType || row.RightValueType == (int)rightValueType))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
