using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region AnnualLeaveGroup

        public static AnnualLeaveGroupDTO ToDTO(this AnnualLeaveGroup e)
        {
            if (e == null)
                return null;

            AnnualLeaveGroupDTO dto = new AnnualLeaveGroupDTO()
            {
                AnnualLeaveGroupId = e.AnnualLeaveGroupId,
                Type = (TermGroup_AnnualLeaveGroupType)e.Type,
                Name = e.Name,
                Description = e.Description,
                QualifyingDays = e.QualifyingDays,
                QualifyingMonths = e.QualifyingMonths,
                GapDays = e.GapDays,
                RuleRestTimeMinimum = e.RuleRestTimeMinimum,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static List<AnnualLeaveGroupDTO> ToDTOs(this IEnumerable<AnnualLeaveGroup> l)
        {
            var dtos = new List<AnnualLeaveGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static AnnualLeaveGroupGridDTO ToGridDTO(this AnnualLeaveGroup e)
        {
            if (e == null)
                return null;

            AnnualLeaveGroupGridDTO dto = new AnnualLeaveGroupGridDTO()
            {
                AnnualLeaveGroupId = e.AnnualLeaveGroupId,
                TypeName = e.TypeName,
                Name = e.Name,
                Description = e.Description,
                QualifyingDays = e.QualifyingDays,
                QualifyingMonths = e.QualifyingMonths,
                GapDays = e.GapDays,
                TimeDeviationCauseName = e.TimeDeviationCause?.Name ?? "",
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static List<AnnualLeaveGroupGridDTO> ToGridDTOs(this IEnumerable<AnnualLeaveGroup> l)
        {
            var dtos = new List<AnnualLeaveGroupGridDTO>();
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
