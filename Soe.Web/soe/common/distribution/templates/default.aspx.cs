using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.distribution.templates
{
    public partial class _default : PageBase
    {
        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        public SoeModule TargetSoeModule = SoeModule.None;
        public Feature FeatureEdit = Feature.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Templates:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Templates_Edit;
                        break;
                    case Feature.Billing_Distribution_Templates:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Templates_Edit;
                        break;
                    case Feature.Time_Distribution_Templates:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Templates_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}
