using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web
{
	public partial class setcompany : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
		{
            if (TryGetActorCompanyIdFromQS(out int actorCompanyIdFromQS))
			{
                bool updateUserSettings = !IsSupportLoggedInByClaims();

                int? actorCompanyIdFromClaim = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.ActorCompanyId);
                if (actorCompanyIdFromClaim != actorCompanyIdFromQS)
                {
                    if (!actorCompanyIdFromClaim.HasValue)
                    {
                        int userId = UserId;
                        if (userId == 0)
                            LogCollector.LogError($"UserId == 0 in setcompany. actorCompanyIdFromQS:{actorCompanyIdFromQS}, parameterObjectIsnull:{ParameterObject == null}, soeUserIsnull:{SoeUser == null}, licenseId:{ParameterObject?.LicenseId}");

                        int coreCompanyId = SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.CoreCompanyId, userId, 0, 0);
                        if (coreCompanyId > 0)
                            actorCompanyIdFromQS = coreCompanyId;

                        updateUserSettings = false;
                    }

                    Company newCompany = CompanyManager.GetCompany(actorCompanyIdFromQS, true);
                    if (newCompany != null)
                    {
                        if (IsSupportLoggedIn && !newCompany.IsSupportLoginAllowed())
                        {
                            LogCollector.LogInfo($"SupportuserId {SoeSupportUser?.UserId} with target userId {SoeUser?.UserId} tried to login to company {newCompany.Name} which is not allowed for support users.");
                            //Not allowed. Cannot redirect to error page cause it will cause circuclar references
                        }
                        else if (newCompany.LicenseId == SoeLicense.LicenseId)
                        {
                            if (IsSupportLoggedIn && ConfigurationSetupUtil.IsTestBasedOnMachine())
                                LogCollector.LogInfo($"SupportuserId {SoeSupportUser?.UserId} with target userId {SoeUser?.UserId} logged in to company {newCompany?.Name}.");

                            ChangeCurrentCompany(newCompany, updateUserSettings);
                        }                            
                        else if (newCompany.LicenseId != SoeLicense.LicenseId && IsSupportUserLoggedIn && TryRedirectToMultipleLicenses(newCompany, newCompany.License, SoeCompany, SoeLicense))
                        {
                            //Do nothing
                        }
                    }
                }
            }

            string prevousUrl = QS["prev"];
            if (!string.IsNullOrEmpty(prevousUrl))
                Response.Redirect(prevousUrl);
            else
                Response.Redirect(Request.UrlReferrer.ToString());
        }
	}
}
