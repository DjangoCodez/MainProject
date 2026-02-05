using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.preferences.timesettings.timeabsencerule
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_TimeSettings_TimeAbsenceRule;
            base.Page_Init(sender, e);
        }
    }
}
