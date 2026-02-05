using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.PushNotifications
{
    public abstract class SoePushNotification
    {
        protected string recieverId = string.Empty;
        protected String message = String.Empty;
        protected int recordId = 0;
        protected bool releaseMode = false;
        protected PushNotificationType notificationType = PushNotificationType.None;
        protected TermGroup_BrandingCompanies brandingCompany = TermGroup_BrandingCompanies.SoftOne;

        public virtual ActionResult Send(SoeMobileAppType appType)
        {
            return new ActionResult(false);
        }

        public virtual string GetUri()
        {
            return string.Empty;
        }

        public virtual string GetParamStr()
        {
            return string.Empty;
        }

        public virtual ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (String.IsNullOrEmpty(recieverId))
            {
                result.Success = false;
                result.ErrorNumber = (int)ActionResultSave.PushNotificationNoReciever;
                return result;

            }

            if (String.IsNullOrEmpty(message))
            {
                result.Success = false;
                result.ErrorNumber = (int)ActionResultSave.PushNotificationNoMessage;
                return result;

            }

            return result;
        }

    }
}
