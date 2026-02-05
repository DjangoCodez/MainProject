using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class SessionLogoutWarning : PageBase
    {
        protected string timeoutTime = "";
         
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            DateTime time = GetTimeoutLogoutTime();
            if (time > CalendarUtility.DATETIME_DEFAULT)
                this.timeoutTime = GetText(11717, "Automatisk utloggning sker") + " " + time.ToString("HH:mm:ss");

            #endregion

            #region Content

            //GetText not working due to Chris Dependency Injection that doesnt work in modalforms when request comes from javascript...
            if (base.IsLanguageSwedish())
            {
                ((ModalFormMaster)Master).HeaderText = "Sessionen håller på att ta slut";
                ((ModalFormMaster)Master).InfoText = "Du kommer loggas ut om 2 minuter pga av inaktivitet";
                ((ModalFormMaster)Master).ActionButtonText = "Fortsätt vara inloggad";
            }
            else
            {
                ((ModalFormMaster)Master).HeaderText = "The session is ending";
                ((ModalFormMaster)Master).InfoText = "You will be logged out in 2 minutes due to inactivity";
                ((ModalFormMaster)Master).ActionButtonText = "Continue logged in";
            }
            ((ModalFormMaster)Master).ActionJs = "refreshSession()";
            ((ModalFormMaster)Master).showSubmitButton = false;
            ((ModalFormMaster)Master).showCancelButton = false;
            ((ModalFormMaster)Master).showActionButton = false;
            ((ModalFormMaster)Master).showActionJsButton = true;

            #endregion
        }
    }
}