using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.preferences.needssettings.locations
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_NeedsSettings_Locations;
            base.Page_Init(sender, e);
        }
    }
}
