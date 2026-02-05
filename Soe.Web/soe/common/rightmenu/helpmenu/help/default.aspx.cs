using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.rightmenu.helpmenu.help
{
    public partial class _default : PageBase
    {
        protected SoeModule TargetSoeModule = SoeModule.None;
        protected Feature FeatureEdit = Feature.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            int module;
            int featureId;
            if (int.TryParse(QS["module"], out module))
                this.TargetSoeModule = (SoeModule)module;
            if (int.TryParse(QS["feature"], out featureId))
                this.FeatureEdit = (Feature)featureId;

            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }
    }
}