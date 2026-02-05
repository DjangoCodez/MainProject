using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getUserCompanySetting : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string setting = QS["setting"];
                if (!String.IsNullOrEmpty(setting))
                {
                    switch (setting)
                    {
                        case Constants.COOKIE_MENU_COLLAPSED:
                            var sessionMenuCollpsed = GetSessionAndCookie(Constants.COOKIE_MENU_COLLAPSED);
                            bool settingUseCollapsedMenu = sessionMenuCollpsed != null ? StringUtility.GetBool(sessionMenuCollpsed) : SettingManager.GetBoolSetting(SettingMainType.User, (int)UserSettingType.UseCollapsedMenu, PageBase.SoeUser?.UserId ?? 0, 0, 0);
                            ResponseObject = new { Found = true, Value = settingUseCollapsedMenu };
                            break;
                    }
                }
            }
            catch (Exception)
            {
                ResponseObject = null;
            }
            finally
            {
                if (ResponseObject == null)
                    ResponseObject = new { Found = false };
            }
        }
    }
}