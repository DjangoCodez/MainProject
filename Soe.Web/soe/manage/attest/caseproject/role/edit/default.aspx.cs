using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.attest.caseproject.role.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Attest_CaseProject_AttestRoles_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/attest/role/default.aspx");
        }
    }
}
