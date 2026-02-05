using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAccountStd : JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string accountNr = QS["acc"];
			if (!string.IsNullOrEmpty(accountNr))
			{
				AccountDim accountDim = AccountManager.GetAccountDimStd(SoeCompany.ActorCompanyId);
                if (accountDim != null)
                {
                    if (Int32.TryParse(QS["ay"], out int accountYearId))
                    {
                        Account account = AccountManager.GetAccountByNr(accountNr, accountDim.AccountDimId, SoeCompany.ActorCompanyId, loadAccount: true);
                        if (account != null)
                        {
                            var accstd = account.AccountStd;
                            if (accstd != null)
                            {
                                if (!accstd.AccountBalance.IsLoaded)
                                    accstd.AccountBalance.Load();

                                decimal balance = 0;
                                foreach (AccountBalance bal in accstd.AccountBalance)
                                {
                                    if (bal.AccountYearId == accountYearId)
                                    {
                                        balance = bal.Balance;
                                        break;
                                    }
                                }
                                ResponseObject = new
                                {
                                    Found = true,
                                    AccountId = account.AccountId,
                                    AccountNr = account.AccountNr,
                                    Name = account.Name,
                                    UnitStop = accstd.UnitStop,
                                    AmountStop = accstd.AmountStop,
                                    Balance = balance
                                };
                            }
                        }
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
