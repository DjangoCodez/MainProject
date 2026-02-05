using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.distribution.reports.selection
{
    public partial class _default : PageBase
    {
        #region Variables
        
        protected string subtitle;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Distribution_Reports_Selection;

            if (String.IsNullOrEmpty(QS["exporttype"]))
            {
                base.Page_Init(sender, e);
            }
            else
            {
                //Add parameters
                Context.Items["Feature"] = this.Feature;

                Server.Transfer("/soe/common/distribution/reports/selection/default.aspx");
            }
        }
    }
}
