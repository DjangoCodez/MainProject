using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.common.distribution.reports.selection.preview
{
    public partial class _default : PageBase
    {
        #region Variables

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Reports_Selection_Preview:
                        EnableEconomy = true;
                        break;
                    case Feature.Billing_Distribution_Reports_Selection_Preview:
                        EnableBilling = true;
                        break;
                    case Feature.Time_Distribution_Reports_Selection_Preview:
                        EnableTime = true;
                        break;
                }
            }
        }
    }
}
