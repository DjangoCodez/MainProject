using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeePosition
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeePosition

        public static EmployeePositionDTO ToDTO(this EmployeePosition e)
        {
            if (e == null)
                return null;

            return new EmployeePositionDTO()
            {
                EmployeePositionId = e.EmployeePositionId,
                EmployeeId = e.EmployeeId,
                PositionId = e.PositionId,
                EmployeePositionName = e.Position.Name,
                Default = e.Default,
                SysPositionCode = e.Position?.SysPositionCode ?? string.Empty,
                SysPositionName = e.Position?.SysPositionName ?? string.Empty,
                SysPositionDescription = e.Position?.SysPositionDescription ?? string.Empty,
            };
        }

        public static IEnumerable<EmployeePositionDTO> ToDTOs(this IEnumerable<EmployeePosition> l)
        {
            var dtos = new List<EmployeePositionDTO>();
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
