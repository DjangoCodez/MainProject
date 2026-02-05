using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getSysAccountStdParent : JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string accountNr = QS["acc"];
			if (!string.IsNullOrEmpty(accountNr))
			{
				AccountDim accountDim = AccountManager.GetAccountDimStd(SoeCompany.ActorCompanyId);
				if(accountDim != null)
				{
					SysAccountStd sysAccountStd = AccountManager.GetSysAccountStdParent(accountDim.AccountDimId, accountNr, SoeCompany.ActorCompanyId);
					if (sysAccountStd != null)
					{
						int sysAccountSruCode1 = 0;
						int sysAccountSruCode2 = 0;
						if(sysAccountStd.SysAccountSruCode != null)
						{
							int counter = 1;
							foreach (var sysAccountSruCode in sysAccountStd.SysAccountSruCode)
							{
								if(counter == 1)
									sysAccountSruCode1 = sysAccountSruCode.SysAccountSruCodeId;
								else if(counter == 2)
									sysAccountSruCode2 = sysAccountSruCode.SysAccountSruCodeId;
								else if(counter > 2)
									break;
								counter++;
							}
						}

						ResponseObject = new
						{
							Found = true,
							AccountNr = sysAccountStd.AccountNr,
							Name = sysAccountStd.Name,
							AccountType = sysAccountStd.AccountTypeSysTermId,
							AmountStop = sysAccountStd.AmountStop,
							Unit = sysAccountStd.Unit != null ? sysAccountStd.Unit : String.Empty,
							UnitStop = Convert.ToBoolean(sysAccountStd.UnitStop) ? Boolean.TrueString : Boolean.FalseString,
							VatAccount = sysAccountStd.SysVatAccount != null ? sysAccountStd.SysVatAccount.SysVatAccountId : 0, 
							SruCode1 = sysAccountSruCode1,
							SruCode2 = sysAccountSruCode2,
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
