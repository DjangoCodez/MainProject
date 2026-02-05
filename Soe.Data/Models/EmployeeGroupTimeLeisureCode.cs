using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeGroupTimeLeisureCode : ICreatedModified, IState
    {
        public List<EmployeeGroupTimeLeisureCodeSetting> ActiveSettings
        {
            get
            {
                return EmployeeGroupTimeLeisureCodeSetting?
                    .Where(x => x.State == (int)SoeEntityState.Active)
                    .ToList() ?? null;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeGroupTimeLeisureCode

        public static EmployeeGroupTimeLeisureCodeDTO ToDTO(this EmployeeGroupTimeLeisureCode e)
        {
            if (e == null)
                return null;

            EmployeeGroupTimeLeisureCodeDTO dto = new EmployeeGroupTimeLeisureCodeDTO()
            {
                EmployeeGroupTimeLeisureCodeId = e.EmployeeGroupTimeLeisureCodeId,
                TimeLeisureCodeId = e.TimeLeisureCodeId,
                EmployeeGroupId = e.EmployeeGroupId,
                DateFrom = e.DateFrom,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Settings = e.EmployeeGroupTimeLeisureCodeSetting.ToDTOs()
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupTimeLeisureCodeDTO> ToDTOs(this IEnumerable<EmployeeGroupTimeLeisureCode> l)
        {
            var dtos = new List<EmployeeGroupTimeLeisureCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeGroupTimeLeisureCodeGridDTO ToGridDTO(this EmployeeGroupTimeLeisureCode e)
        {
            if (e == null)
                return null;

            EmployeeGroupTimeLeisureCodeGridDTO dto = new EmployeeGroupTimeLeisureCodeGridDTO()
            {
                EmployeeGroupTimeLeisureCodeId = e.EmployeeGroupTimeLeisureCodeId,
                TimeLeisureCodeId = e.TimeLeisureCodeId,
                TimeLeisureCodeName = e.TimeLeisureCode?.Name,
                EmployeeGroupId = e.EmployeeGroupId,
                EmployeeGroupName = e.EmployeeGroup?.Name,
                DateFrom = e.DateFrom,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupTimeLeisureCodeGridDTO> ToGridDTOs(this IEnumerable<EmployeeGroupTimeLeisureCode> l)
        {
            var dtos = new List<EmployeeGroupTimeLeisureCodeGridDTO>();
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
