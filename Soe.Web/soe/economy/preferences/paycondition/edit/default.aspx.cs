using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.preferences.registry.paycondition.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_PayCondition_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/paycondition/default.aspx");
        }
    }
}
