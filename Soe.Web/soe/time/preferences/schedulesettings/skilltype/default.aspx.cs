using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.preferences.schedulesettings.skilltype
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_ScheduleSettings_SkillType;
            base.Page_Init(sender, e);
        }
    }
}
