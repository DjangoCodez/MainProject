using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.preferences.schedulesettings.daytypes.import
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_ScheduleSettings_DayTypes_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/daytypes/import/default.aspx");
        }
    }
}
