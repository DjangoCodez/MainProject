using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.preferences.licensesettings
{
    public partial class _default : PageBase
    {
        protected SettingMainType settingMainType = SettingMainType.License;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Preferences_LicenseSettings;
            base.Page_Init(sender, e);
        }
    }
}
