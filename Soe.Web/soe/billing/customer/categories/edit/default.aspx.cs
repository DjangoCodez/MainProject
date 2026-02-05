using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.customer.categories.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Common_Categories_Customer_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/categories/edit/default.aspx");
        }
    }
}
