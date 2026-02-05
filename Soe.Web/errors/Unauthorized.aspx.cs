using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.errors
{
	public partial class Unauthorized : PageBase
	{
        protected string UnauthorizedMessage;

		protected void Page_Load(object sender, EventArgs e)
		{
            #region Init

            if (SoeUser == null)
                RedirectToHome();

            #endregion

            UnauthorizationType type = UnauthorizationType.None;
            if(CTX.Contains("UnauthorizationType"))
                type = (UnauthorizationType)CTX["UnauthorizationType"];

            switch(type)
            {
                case UnauthorizationType.FeaturePermissionMissing:
                    UnauthorizedMessage = GetText(1959, "Rollen saknar behörighet för aktuell sida");
                    break;
                case UnauthorizationType.DataAuthorityMissing:
                    UnauthorizedMessage = GetText(1960, "Aktuell sida kräver särskilda rättigheter eller administratörsroll");
                    break;
                case UnauthorizationType.ReportPermissionMissing:
                    UnauthorizedMessage = GetText(5670, "Rollen saknar behörighet för att visa rapporten");
                    break;
                case UnauthorizationType.UnknownLogin:
                    UnauthorizedMessage = GetText(11017, "Obehörigt inlogg");
                    break;
            }
		}
	}
}
