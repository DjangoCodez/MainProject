using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.preferences.schedulesettings.holiday.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_ScheduleSettings_Holidays_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/holidays/edit/default.aspx");
        }
    }
}
