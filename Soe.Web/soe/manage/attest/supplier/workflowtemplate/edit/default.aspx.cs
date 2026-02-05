using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.attest.supplier.workflowtemplate.edit
{
    public partial class Default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Attest_Supplier_WorkFlowTemplate_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/attest/workFlowTemplate/default.aspx");
        }
    }
}
