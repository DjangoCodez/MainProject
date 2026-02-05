using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.invoicesettings.templates
{
    public partial class _default : PageBase
    {
        protected AccountManager am;
        public bool isTemplateRegistration;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_InvoiceSettings_Templates;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            isTemplateRegistration = true;            
        }
    }
}
