using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class AccountBalance : ICreatedModified
    {
    }

    public static partial class EntityExtensions
    {
        #region AccountBalance


        public static IEnumerable<DecimalKeyValue> ToDecimalKeyValues(this Dictionary<int, decimal> l) {
            var dtos = new List<DecimalKeyValue>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(new DecimalKeyValue(e.Key,e.Value));
                }
            }
            return dtos;
        }

        public static IEnumerable<AccountBalanceDTO> ToDTOs(this IEnumerable<AccountBalance> l, bool includeYear, bool includeAccount)
        {
            var dtos = new List<AccountBalanceDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeYear, includeAccount));
                }
            }
            return dtos;
        }

        public static AccountBalanceDTO ToDTO(this AccountBalance e, bool includeYear, bool includeAccount)
        {
            if (e == null)
                return null;

            if (includeYear && !e.AccountYearReference.IsLoaded)
            {
                e.AccountYearReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("AccountBalance.cs e.AccountYearReference");
            }


            if (includeAccount && !e.AccountStdReference.IsLoaded)
            {
                e.AccountStdReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("AccountBalance.cs e.AccountStdReference");
            }

            AccountBalanceDTO dto = new AccountBalanceDTO()
            {
                AccountYearId = e.AccountYearId,
                AccountId = e.AccountId,
                Balance = e.Balance,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };

            dto.AccountYearStr = e.AccountYear != null ? e.AccountYear.From.ToString("yyyyMM") + "-" + e.AccountYear.To.ToString("yyyyMM") : "";
            dto.BalanceStr = e.Balance.ToString();
            if (e.Modified.HasValue)
                dto.ModifiedCreatedStr = e.Modified.Value.ToString();
            else if (e.Created.HasValue)
                dto.ModifiedCreatedStr = e.Created.Value.ToString();

            return dto;
        }

        #endregion
    }
}
