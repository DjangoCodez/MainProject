using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class AccountDimMock
    {
        private static int id = 1;

        public static List<AccountDimDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             *  select top 5 'l.Add(Create("' + Account.AccountNr + '","' + Account.Name + '","' + isnull(Account.ExternalCode, '') + '",' + CAST(AccountDim.AccountDimNr as nvarchar) + '));'  from Account inner join AccountDim on Account.AccountDimId = AccountDim.AccountDimId where AccountDim.ActorCompanyId = 701609 and AccountDim.State = 0 and AccountDim.AccountDimId = 2986 order by Account.AccountNr
             *  select top 5 'l.Add(Create("' + Account.AccountNr + '","' + Account.Name + '","' + isnull(Account.ExternalCode, '') + '",' + CAST(AccountDim.AccountDimNr as nvarchar) + '));'  from Account inner join AccountDim on Account.AccountDimId = AccountDim.AccountDimId where AccountDim.ActorCompanyId = 701609 and AccountDim.State = 0 and AccountDim.AccountDimId = 3051 order by Account.AccountNr
             *  select top 5 'l.Add(Create("' + Account.AccountNr + '","' + Account.Name + '","' + isnull(Account.ExternalCode, '') + '",' + CAST(AccountDim.AccountDimNr as nvarchar) + '));'  from Account inner join AccountDim on Account.AccountDimId = AccountDim.AccountDimId where AccountDim.ActorCompanyId = 701609 and AccountDim.State = 0 and AccountDim.AccountDimId = 3090 order by Account.AccountNr
             *  select top 5 'l.Add(Create("' + Account.AccountNr + '","' + Account.Name + '","' + isnull(Account.ExternalCode, '') + '",' + CAST(AccountDim.AccountDimNr as nvarchar) + '));'  from Account inner join AccountDim on Account.AccountDimId = AccountDim.AccountDimId where AccountDim.ActorCompanyId = 701609 and AccountDim.State = 0 and AccountDim.AccountDimId = 3024 order by Account.AccountNr
             *  select top 5 'l.Add(Create("' + Account.AccountNr + '","' + Account.Name + '","' + isnull(Account.ExternalCode, '') + '",' + CAST(AccountDim.AccountDimNr as nvarchar) + '));'  from Account inner join AccountDim on Account.AccountDimId = AccountDim.AccountDimId where AccountDim.ActorCompanyId = 701609 and AccountDim.State = 0 and AccountDim.AccountDimId = 3057 order by Account.AccountNr
             *  select top 5 'l.Add(Create("' + Account.AccountNr + '","' + Account.Name + '","' + isnull(Account.ExternalCode, '') + '",' + CAST(AccountDim.AccountDimNr as nvarchar) + '));'  from Account inner join AccountDim on Account.AccountDimId = AccountDim.AccountDimId where AccountDim.ActorCompanyId = 701609 and AccountDim.State = 0 and AccountDim.AccountDimId = 5426 order by Account.AccountNr 
             */
            #endregion

            id = 1;
            var l = new List<AccountDimDTO>();

            l.Add(Create(1, "Kontoplan", "Std", null));
            l.Add(Create(2, "Avdelning", "Avd", 50));
            l.Add(Create(3, "Kostnadsställe", "Kst", 1));
            l.Add(Create(4, "Grupp", "Grupp", 40));
            l.Add(Create(5, "Passtyp", "Passtyp", 8));
            l.Add(Create(100, "Område", "Omr", 30));

            return l;
        }

        private static AccountDimDTO Create(int accountDimNr, string name, string shortName, int? sysSieDimNr)
        {
            return new AccountDimDTO()
            {
                AccountDimId = id++,
                AccountDimNr = accountDimNr,
                Name = name,
                ShortName = shortName,
                SysSieDimNr = sysSieDimNr
            };
        }
    }
}
