using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeGroupTimeLeisureCodeSetting : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string SettingValue { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeGroupTimeLeisureCodeSetting

        public static EmployeeGroupTimeLeisureCodeSettingDTO ToDTO(this EmployeeGroupTimeLeisureCodeSetting e)
        {
            if (e == null)
                return null;

            EmployeeGroupTimeLeisureCodeSettingDTO dto = new EmployeeGroupTimeLeisureCodeSettingDTO()
            {
                EmployeeGroupTimeLeisureCodeSettingId = e.EmployeeGroupTimeLeisureCodeSettingId,
                Type = (TermGroup_TimeLeisureCodeSettingType)e.Type,
                DataType = (SettingDataType)e.DataType,
                Name = e.Name,
                BoolData = e.BoolData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                StrData = e.StrData,
                DateData = e.DateData,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupTimeLeisureCodeSettingDTO> ToDTOs(this IEnumerable<EmployeeGroupTimeLeisureCodeSetting> l)
        {
            var dtos = new List<EmployeeGroupTimeLeisureCodeSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeGroupTimeLeisureCodeSettingGridDTO ToGridDTO(this EmployeeGroupTimeLeisureCodeSetting e)
        {
            if (e == null)
                return null;

            EmployeeGroupTimeLeisureCodeSettingGridDTO dto = new EmployeeGroupTimeLeisureCodeSettingGridDTO()
            {
                EmployeeGroupTimeLeisureCodeSettingId = e.EmployeeGroupTimeLeisureCodeSettingId,
                Name = e.Name,
                Type = (TermGroup_TimeLeisureCodeSettingType)e.Type,
                TypeName = e.TypeName,
                DataType = (SettingDataType)e.DataType,
                SettingValue = e.SettingValue,
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupTimeLeisureCodeSettingGridDTO> ToGridDTOs(this IEnumerable<EmployeeGroupTimeLeisureCodeSetting> l)
        {
            var dtos = new List<EmployeeGroupTimeLeisureCodeSettingGridDTO>();
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
