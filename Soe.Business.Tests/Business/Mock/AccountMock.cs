using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class AccountMock
    {
        private static int id = 1;

        public static List<AccountDTO> Mock()
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
            var l = new List<AccountDTO>();

            //Dim 1
            l.Add(Create("1613", "Förskott", "", 1));
            l.Add(Create("1941", "Bank", "", 1));
            l.Add(Create("2641", "Ingående Moms", "", 1));
            l.Add(Create("2710", "Personalskatt", "", 1));
            l.Add(Create("2730", "Lagstadgade sociala avgifter", "", 1));
            //Dim 2
            l.Add(Create("1", "VD", "", 2));
            l.Add(Create("10", "Customer Service OH", "", 2));
            l.Add(Create("100", "OC Ved (Anv ej)", "", 2));
            l.Add(Create("101", "Koordinator Ved (Anv ej)", "", 2));
            l.Add(Create("102", "Fordon OH", "", 2));
            //Dim 3
            l.Add(Create("100", "Finans", "", 3));
            l.Add(Create("105", "HR", "", 3));
            l.Add(Create("106", "HR Operations", "", 3));
            l.Add(Create("110", "Analysis", "", 3));
            l.Add(Create("120", "Revenue B2B", "", 3));
            //Dim 4
            l.Add(Create("1", "VM Kolonial Bma", "", 4));
            l.Add(Create("10", "Matkassar Gbg (Anv ej)", "", 4));
            l.Add(Create("100", "Finans", "", 4));
            l.Add(Create("1000", "Stf. VM Kolonial Bma", "", 4));
            l.Add(Create("1001", "Stf. VM Kyl Bma", "", 4));
            //Dim 5
            l.Add(Create("", "CTO", "", 5));
            l.Add(Create("1", "Koordinator VM Kolonial Bma", "", 5));
            l.Add(Create("10", "Koordinator VM Kyl Bma", "", 5));
            l.Add(Create("100", "Koordinator Kolonial Gbg", "", 5));
            l.Add(Create("1000", "Utbildare VM Gbg", "", 5));
            //Dim 6
            l.Add(Create("1", "Finans", "", 100));
            l.Add(Create("10", "Creative", "", 100));
            l.Add(Create("100", "Driftledare Bma", "", 100));
            l.Add(Create("101", "Inhyrda Gbg", "", 100));
            l.Add(Create("102", "Development OH C", "", 100));

            return l;
        }

        private static AccountDTO Create(string accountNr, string name, string externalCode, int accountDimNr)
        {
            return new AccountDTO()
            {
                AccountId = id++,
                AccountNr = accountNr,
                Name = name,
                ExternalCode = externalCode.EmptyToNull(),
                AccountDimNr = accountDimNr,
            };
        }
    }
}
