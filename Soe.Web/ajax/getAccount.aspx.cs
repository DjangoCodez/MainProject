using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAccount : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string accountNr = QS["acc"];
            if (!string.IsNullOrEmpty(accountNr))
            {
                if (Int32.TryParse(QS["dim"], out int accountDimId) && Int32.TryParse(QS["ay"], out int accountYearId))
                {
                    Account account = AccountManager.GetAccountByNr(accountNr, accountDimId, SoeCompany.ActorCompanyId, loadAccount: true, loadAccountMapping: true);
                    if (account != null)
                    {
                        decimal balance = 0;
                        Dictionary<int, List<Object>> dict = new Dictionary<int, List<Object>>();

                        var accountStd = account.AccountStd;
                        if (accountStd != null)
                        {
                            foreach (AccountMapping accountMapping in account.AccountMapping)
                            {
                                if (accountMapping.AccountInternal == null)
                                    continue;
                                    
                                //Make sure Account is loaded
                                if (!accountMapping.AccountInternal.AccountReference.IsLoaded)
                                    accountMapping.AccountInternal.AccountReference.Load();
                                //Make sure AccountStd is loaded
                                if (!accountMapping.AccountInternal.Account.AccountStdReference.IsLoaded)
                                    accountMapping.AccountInternal.Account.AccountStdReference.Load();

                                List<Object> accountMappings = new List<Object>();
                                accountMappings.Add(accountMapping.MandatoryLevel);
                                accountMappings.Add(accountMapping.AccountInternal.Account.AccountNr);
                                accountMappings.Add(accountMapping.AccountInternal.Account.Name);
                                accountMappings.Add(accountMapping.AccountDimId);

                                dict.Add(accountMapping.AccountDim.AccountDimNr, accountMappings);
                            }

                            //Make sure AccountBalance is loaded
                            if (!accountStd.AccountBalance.IsLoaded)
                                accountStd.AccountBalance.Load();

                            foreach (AccountBalance accountBalance in accountStd.AccountBalance)
                            {
                                if (accountBalance.AccountYearId == accountYearId)
                                {
                                    balance = accountBalance.Balance;
                                    break;
                                }
                            }
                        }

                        ResponseObject = new
                        {
                            Found = true,
                            AccountId = account.AccountId,
                            AccountNr = account.AccountNr,
                            Name = account.Name,
                            UnitStop = accountStd?.UnitStop ?? false,
                            AmountStop = accountStd?.AmountStop ?? 1,
                            Balance = balance,
                            Mapping = dict
                        };
                    }
                }
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}
