using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.attest.registry.transition.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Attest_Time_AttestTransitions_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/attest/transition/default.aspx");
        }
    }
}
