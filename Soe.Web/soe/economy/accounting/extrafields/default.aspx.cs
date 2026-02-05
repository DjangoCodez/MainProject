using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.extrafields
{
    public partial class _default : PageBase
    {
        protected SoeEntityType Entity = SoeEntityType.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Entity = SoeEntityType.Account;
            this.Feature = Feature.Common_ExtraFields_Account_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
