using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region AccrualAccountMapping
        public static AccrualAccountMappingDTO ToDTO(this AccrualAccountMapping e)
        {
            if (e == null)
                return null;

            return new AccrualAccountMappingDTO()
            {
                AccrualAccountMappingId = e.AccrualAccountMappingId,
                SourceAccountTypeSysTermId = e.SourceAccountTypeSysTermId.GetValueOrDefault(0),
                SourceAccountId = e.SourceAccountId.GetValueOrDefault(0),
                TargetAccrualAccountId = e.TargetAccrualAccountId,

                // Extention
                SourceAccountNr = e.Account.AccountNr,
                TargetAccrualAccountNr = e.Account1.AccountNr,
            };
        }

        public static List<AccrualAccountMappingDTO> ToDTOList(this IEnumerable<AccrualAccountMapping> entities)
        {
            if (entities == null)
                return null;
            return entities.Select(e => e.ToDTO()).ToList();
        }


        #endregion
    }
}
