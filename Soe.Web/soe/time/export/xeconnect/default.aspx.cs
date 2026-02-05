using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.export.xeconnect
{
    public partial class _default : PageBase
    {
        protected SoeModule TargetSoeModule = SoeModule.Time;
        protected Feature FeatureEdit = Feature.Time_Export_XEConnect;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Export_XEConnect;
            base.Page_Init(sender, e);
        }
    }
}