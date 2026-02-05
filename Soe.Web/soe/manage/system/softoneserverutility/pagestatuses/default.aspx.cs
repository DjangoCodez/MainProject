using System;
using System.Collections.Generic;
using System.Web;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;

namespace SoftOne.Soe.Web.soe.manage.system.softoneserverutility.pagestatuses
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }
    }
}
