using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.attest.supplier.workflowtemplate
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Attest_Supplier_WorkFlowTemplate;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}
