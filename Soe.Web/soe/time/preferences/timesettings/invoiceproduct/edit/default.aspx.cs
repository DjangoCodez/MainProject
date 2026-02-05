using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.preferences.timesettings.invoiceproduct.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_TimeSettings_InvoiceProduct_Edit;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/invoiceproduct/default.aspx");
        }
    }
}
