using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.signing.document.state
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Signing_Document_States;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
