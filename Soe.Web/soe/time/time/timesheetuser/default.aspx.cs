using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.time.timesheetuser
{
    public partial class _default : PageBase
    {
        #region Variables
        
        protected string subtitle;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Time_TimeSheetUser;
            base.Page_Init(sender, e);
        }
    }
}
