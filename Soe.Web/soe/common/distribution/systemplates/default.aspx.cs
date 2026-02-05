using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.distribution.systemplates
{
    public partial class _default : PageBase
    {
        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        public Feature FeatureEdit = Feature.None;
        public SoeModule TargetSoeModule = SoeModule.None;

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
                    case Feature.Economy_Distribution_SysTemplates:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_SysTemplates_Edit;
                        break;
                    case Feature.Billing_Distribution_SysTemplates:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_SysTemplates_Edit;
                        break;
                    case Feature.Time_Distribution_SysTemplates:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_SysTemplates_Edit;
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
