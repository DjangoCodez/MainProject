using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class UnionFee : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region UnionFee

        public static IEnumerable<UnionFeeDTO> ToDTOs(this IEnumerable<UnionFee> l)
        {
            var dtos = new List<UnionFeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static UnionFeeDTO ToDTO(this UnionFee e)
        {
            if (e == null)
                return null;

            return new UnionFeeDTO()
            {
                UnionFeeId = e.UnionFeeId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                PayrollPriceTypeIdPercent = e.PayrollPriceTypeIdPercent,
                PayrollPriceTypeIdPercentCeiling = e.PayrollPriceTypeIdPercentCeiling,
                PayrollPriceTypeIdFixedAmount = e.PayrollPriceTypeIdFixedAmount,
                PayrollProductId = e.PayrollProductId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Association = e.Association,
            };
        }

        public static UnionFeeGridDTO ToGridDTO(this UnionFee e)
        {
            if (e == null)
                return null;

            return new UnionFeeGridDTO()
            {
                UnionFeeId = e.UnionFeeId,
                Name = e.Name,
                PayrollPriceTypeIdPercentName = e.PayrollPriceTypePercent?.Name ?? string.Empty,
                PayrollPriceTypeIdPercentCeilingName = e.PayrollPriceTypePercentCeiling?.Name ?? string.Empty,
                PayrollPriceTypeIdFixedAmountName = e.PayrollPriceTypeFixedAmount?.Name ?? string.Empty,
                PayrollProductName = e.PayrollProduct?.Name ?? string.Empty,
                State = (SoeEntityState)e.State,
                Association = e.Association,
            };
        }

        public static IEnumerable<UnionFeeGridDTO> ToGridDTOs(this IEnumerable<UnionFee> l)
        {
            var dtos = new List<UnionFeeGridDTO>();
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
