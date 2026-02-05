using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.distribution.groups
{
    public partial class _default : PageBase
    {
        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        protected SoeModule TargetSoeModule = SoeModule.None;
        protected Feature FeatureEdit = Feature.None;

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
                    case Feature.Economy_Distribution_Groups:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Groups_Edit;
                        break;
                    case Feature.Billing_Distribution_Groups:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Groups_Edit;
                        break;
                    case Feature.Time_Distribution_Groups:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Groups;
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
