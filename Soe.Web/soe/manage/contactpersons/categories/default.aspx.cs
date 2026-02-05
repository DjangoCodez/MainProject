using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.contactpersons.categories
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Common_Categories_ContactPersons;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/categories/default.aspx");
        }
    }
}
