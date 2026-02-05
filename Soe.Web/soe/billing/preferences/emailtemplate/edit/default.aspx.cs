using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.emailtemplate.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_EmailTemplate_Edit;
            
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/emailtemplate/default.aspx");
        }
    }
}
