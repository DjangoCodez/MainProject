using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AccountPeriod : ICreatedModified
    {
    }

    public static partial class EntityExtensions
    {
        #region AccountPeriod

        public static AccountPeriodDTO ToDTO(this AccountPeriod e, bool doVoucherCheck = false)
        {
            if (e == null)
                return null;

            AccountPeriodDTO dto = new AccountPeriodDTO()
            {
                AccountPeriodId = e.AccountPeriodId,
                AccountYearId = e.AccountYearId,
                PeriodNr = e.PeriodNr,
                From = e.From,
                To = e.To,
                Status = (TermGroup_AccountStatus)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                StartValue = e.From.ToString("yyyyMM", CultureInfo.CurrentCulture),
            };

            if (doVoucherCheck)
                dto.HasExistingVouchers = e.HasVouchers ?? e.VoucherHead.Any();

            return dto;
        }

        public static IEnumerable<AccountPeriodDTO> ToDTOs(this IEnumerable<AccountPeriod> l)
        {
            var dtos = new List<AccountPeriodDTO>();
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
