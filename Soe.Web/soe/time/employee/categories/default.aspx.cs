using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.employee.categories
{
    public partial class _default : PageBase
    {
        protected SoeModule TargetSoeModule = SoeModule.None;
        protected Feature FeatureEdit = Feature.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Common_Categories_Employee;
            this.FeatureEdit = Feature.Common_Categories_Employee_Edit;

            base.Page_Init(sender, e);
        }
    }
}
