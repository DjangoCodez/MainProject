using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.preferences.registry.paycondition
{
    public partial class _default : PageBase
    {
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
            this.Feature = Feature.Economy_Preferences_PayCondition;
            TargetSoeModule = SoeModule.Economy;
            FeatureEdit = Feature.Economy_Preferences_PayCondition;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}
