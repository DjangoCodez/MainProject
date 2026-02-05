using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.errors
{
    public partial class RemoteLoginFailed : PageBase
    {
        protected string RemoteLoginFailedMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            if (SoeUser == null)
                RedirectToHome();

            #endregion

            RemoteLoginFailedType type = RemoteLoginFailedType.None;
            if (CTX != null && CTX.Contains("RemoteLoginFailedType"))
                type = (RemoteLoginFailedType)CTX["RemoteLoginFailedType"];

            string details = String.Empty;
            if (CTX != null && CTX.Contains("RemoteLoginFailedDetails"))
                details = (string)CTX["RemoteLoginFailedDetails"];

            switch (type)
            {
                case RemoteLoginFailedType.Failed:
                    RemoteLoginFailedMessage = ""; //No further description
                    break;
                case RemoteLoginFailedType.NotAllowed:
                    RemoteLoginFailedMessage = GetText(10035, "Företaget tillåter inte supportinlogg");
                    if (!String.IsNullOrEmpty(details))
                    {
                        RemoteLoginFailedMessage += ". ";
                        RemoteLoginFailedMessage += String.Format(GetText(10036, "Har endast tillåtits till {0}"), details);
                    }                        
                    break;
                case RemoteLoginFailedType.InvalidLicense:
                    RemoteLoginFailedMessage = GetText(8854, "Supportinlogg inte tillåtet");
                    break;
                case RemoteLoginFailedType.InvalidServer:
                    RemoteLoginFailedMessage = String.Format("{0} {1}", GetText(5488, "Licensen är flyttad till"), details);
                    LinkTo2ndServer.Value = String.Format(GetText(1697, "Klicka här för att komma till {0}"), details);
                    LinkTo2ndServer.Href = details + "/soe/manage/companies/edit/remote/" + Request.Url.Query + String.Format("&forceout=1");
                    break;
            }
        }
    }
}
