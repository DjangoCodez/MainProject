using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<MatchCode, MatchCodeGridDTO>> GetMatchCodeGridDTO =
            (matchCode) => new MatchCodeGridDTO
            {
                AccountNr = matchCode.Account.AccountNr ?? string.Empty,
                Description = matchCode.Description,
                MatchCodeId = matchCode.MatchCodeId,
                Name = matchCode.Name,
                VatAccountNr = matchCode.VatAccount.AccountNr ?? string.Empty,
                TypeId = matchCode.Type
            };
    }
}
