using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmploymentType : ICreatedModified, IState
    {
    }

    public static partial class EntityExtensions
    {
        #region EmploymentType

        public static EmploymentTypeDTO ToDTO(this EmploymentType e)
        {
            if (e == null)
                return null;

            EmploymentTypeDTO dto = new EmploymentTypeDTO()
            {
                EmploymentTypeId = e.EmploymentTypeId,
                Type = e.Type,
                Name = e.Name,
                Description = e.Description,
                Code = e.Code,
                State = (SoeEntityState)e.State,
                ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = e.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment,
                SettingOnly = e.SettingOnly,
                ExternalCode = e.ExternalCode,
                Active = e.State == (int)SoeEntityState.Active
            };

            return dto;
        }

        public static IEnumerable<EmploymentTypeDTO> ToDTOs(this IEnumerable<EmploymentType> l)
        {
            var dtos = new List<EmploymentTypeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmploymentTypeGridDTO ToGridDTO(this EmploymentTypeDTO e)
        {
            if (e == null)
                return null;

            return new EmploymentTypeGridDTO()
            {
                EmploymentTypeId = e.EmploymentTypeId,
                GridId = e.GetEmploymentType(),
                Type = e.Type,
                TypeName = e.TypeName,
                Name = e.Name,
                Description = e.Description,
                Code = e.Code,
                AllowEdit = !e.HideEdit,
                ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = e.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment,
                SettingOnly = e.SettingOnly,
                ExternalCode = e.ExternalCode,
                State = (SoeEntityState)e.State,
                Standard = e.Standard
            };
        }

        public static IEnumerable<EmploymentTypeGridDTO> ToGridDTOs(this IEnumerable<EmploymentTypeDTO> l)
        {
            var dtos = new List<EmploymentTypeGridDTO>();

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
