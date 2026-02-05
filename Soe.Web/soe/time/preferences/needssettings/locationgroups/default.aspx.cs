using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.preferences.needssettings.locationgroups
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_NeedsSettings_LocationGroups;
            base.Page_Init(sender, e);
        }
    }
}
