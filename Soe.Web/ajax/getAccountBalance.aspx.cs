using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAccountBalance : JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string accountNr = QS["acc"];
			if (!string.IsNullOrEmpty(accountNr))
			{
                Account account = AccountManager.GetAccountByDimNr(accountNr, 1, SoeCompany.ActorCompanyId);
				if (account != null)
				{
					AccountBalanceManager abm = new AccountBalanceManager(ParameterObject, SoeCompany.ActorCompanyId);
					abm.CalculateAccountBalanceForAccountInAccountYears(SoeCompany.ActorCompanyId, account.AccountId);

					ResponseObject = new
					{
						Found = true,
					};
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
