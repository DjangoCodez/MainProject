using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmploymentVacationGroup : IEmploymentVacationGroup, ICreatedModified, IState
    {
        public static EmploymentVacationGroup Create(Employment employment, int vacationGroupId, DateTime? fromDate, string createdBy, DateTime? created = null)
        {
            if (employment == null)
                return null;

            EmploymentVacationGroup employmentVacationGroup = new EmploymentVacationGroup()
            {
                FromDate = fromDate,
                VacationGroupId = vacationGroupId,
            };
            employmentVacationGroup.SetCreated(created ?? DateTime.Now, createdBy);
            employment.EmploymentVacationGroup.Add(employmentVacationGroup);
            return employmentVacationGroup;
        }
        public void Update(int vacationGroupId, DateTime? fromDate, string modifiedBy, DateTime? modified = null)
        {
            if (vacationGroupId != this.VacationGroupId || fromDate != this.FromDate)
            {
                this.VacationGroupId = vacationGroupId;
                this.FromDate = fromDate;
                this.SetModified(modified ?? DateTime.Now, modifiedBy);
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region EmploymentVacationGroup

        public static EmploymentVacationGroupDTO ToDTO(this EmploymentVacationGroup e, bool includeVacationGroupSE, List<VacationGroup> vacationGroups = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeVacationGroupSE && !e.IsAdded())
                {
                    if (!e.VacationGroupReference.IsLoaded)
                    {
                        e.VacationGroupReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmploymentVacationGroup.cs e.VacationGroupReference");
                    }
                    if (!e.VacationGroup.VacationGroupSE.IsLoaded)
                    {
                        e.VacationGroup.VacationGroupSE.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmploymentVacationGroup.cs e.acationGroup.VacationGroupSE");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            EmploymentVacationGroupDTO dto = new EmploymentVacationGroupDTO()
            {
                EmploymentVacationGroupId = e.EmploymentVacationGroupId,
                EmploymentId = e.EmploymentId,
                VacationGroupId = e.VacationGroupId,
                FromDate = e.FromDate,
                Name = e.VacationGroup.Name,
                Type = e.VacationGroup.Type,
            };

            if (!vacationGroups.IsNullOrEmpty() && vacationGroups.Any(v => v.VacationGroupId == dto.VacationGroupId))
            {
                var group = vacationGroups.FirstOrDefault(v => v.VacationGroupId == dto.VacationGroupId);
                if (group != null && !group.VacationGroupSE.IsLoaded)
                {
                    group.VacationGroupSE.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("EmploymentVacationGroup.cs group.VacationGroupSE");
                }

                var se = group?.VacationGroupSE.FirstOrDefault();
                if (se != null)
                {
                    dto.CalculationType = (TermGroup_VacationGroupCalculationType)se.CalculationType;
                    dto.VacationHandleRule = (TermGroup_VacationGroupVacationHandleRule)se.VacationHandleRule;
                    dto.VacationDaysHandleRule = (TermGroup_VacationGroupVacationDaysHandleRule)se.VacationDaysHandleRule;
                }
            }
            else if (includeVacationGroupSE && e.VacationGroup?.VacationGroupSE != null && e.VacationGroup.VacationGroupSE.Count > 0)
            {
                VacationGroupSE se = e.VacationGroup.VacationGroupSE.First();
                dto.CalculationType = (TermGroup_VacationGroupCalculationType)se.CalculationType;
                dto.VacationHandleRule = (TermGroup_VacationGroupVacationHandleRule)se.VacationHandleRule;
                dto.VacationDaysHandleRule = (TermGroup_VacationGroupVacationDaysHandleRule)se.VacationDaysHandleRule;
            }

            return dto;
        }

        public static IEnumerable<EmploymentVacationGroupDTO> ToDTOs(this IEnumerable<EmploymentVacationGroup> l, bool includeVacationGroupSE, List<VacationGroup> vacationGroups = null)
        {
            var dtos = new List<EmploymentVacationGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeVacationGroupSE, vacationGroups: vacationGroups));
                }
            }
            return dtos;
        }

        public static int? GetLastVacationGroupId(this IEnumerable<IEmploymentVacationGroup> l)
        {
            return l?.OrderBy(i => i.FromDate ?? DateTime.MinValue).LastOrDefault()?.VacationGroupId;
        }

        #endregion
    }
}
