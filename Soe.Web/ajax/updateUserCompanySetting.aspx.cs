using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class updateUserCompanySetting : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string setting = QS["setting"];
                if (!string.IsNullOrEmpty(setting))
                {
                    bool value = StringUtility.GetBool(QS["value"]);
                    switch (setting)
                    {
                        case Constants.COOKIE_MENU_COLLAPSED:
                            if (SettingManager.UpdateInsertBoolSetting(SettingMainType.User, (int)UserSettingType.UseCollapsedMenu, value, PageBase.UserId, 0, 0).Success)
                            {
                                AddToSessionAndCookie(Constants.COOKIE_MENU_COLLAPSED, value);
                                ResponseObject = new { Success = true };
                            }
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
                    ResponseObject = new { Success = false };
            }
        }
    }
}