using SoftOne.Soe.Common.Security;
using System;

namespace SoftOne.Soe.Web
{
	public partial class setrole : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            if (TryGetRoleIdFromQS(out int roleIdFromQS) && TryGetActorCompanyIdFromQS(out int actorCompanyIdFromQS))
            {
                int? roledFromClaim = ClaimsHelper.GetIntClaim(this.Context, SoeClaimType.RoleId);
                if (roledFromClaim != roleIdFromQS)
                    ChangeCurrentRole(roleIdFromQS, actorCompanyIdFromQS, updateSettings: !IsSupportLoggedInByClaims());
            }

            string prevousUrl = QS["prev"];
            if (!string.IsNullOrEmpty(prevousUrl))
                Response.Redirect(prevousUrl);
            else
                Response.Redirect(Request.UrlReferrer.ToString());
        }
	}
}
