using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeUnionFee : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeUnionFee

        public static EmployeeUnionFeeDTO ToDTO(this EmployeeUnionFee e)
        {
            if (e == null)
                return null;

            return new EmployeeUnionFeeDTO()
            {
                EmployeeUnionFeeId = e.EmployeeUnionFeeId,
                EmployeeId = e.EmployeeId,
                UnionFeeId = e.UnionFeeId,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                UnionFeeName = e.UnionFee?.Name,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<EmployeeUnionFeeDTO> ToDTOs(this IEnumerable<EmployeeUnionFee> l)
        {
            var dtos = new List<EmployeeUnionFeeDTO>();
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
