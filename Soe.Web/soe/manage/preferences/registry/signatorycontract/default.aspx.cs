using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.manage.preferences.registry.signatorycontract
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Preferences_Registry_SignatoryContract;
            base.Page_Init(sender, e);
        }
    }
}
