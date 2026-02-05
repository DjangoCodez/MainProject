using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.distribution.reports
{
    public partial class _default : PageBase
    {
        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        public SoeModule TargetSoeModule = SoeModule.None;
        public Feature FeatureEdit = Feature.None;
        public Feature FeatureSelection = Feature.None;
        public int ReportPackageId;
        public bool ReportSelectionPermission;

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            Int32.TryParse(QS["package"], out ReportPackageId);
            ReportSelectionPermission = HasRolePermission(FeatureSelection, Permission.Readonly);

            base.Page_Init(sender, e);
        }

         private void EnableModuleSpecifics()
        {
            int module;
            Int32.TryParse(QS["m"], out module);

            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Reports:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Reports_Edit;
                        FeatureSelection = Feature.Economy_Distribution_Reports_Selection;
                        break;
                    case Feature.Billing_Distribution_Reports:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Reports_Edit;
                        FeatureSelection = Feature.Billing_Distribution_Reports_Selection;
                        break;
                    case Feature.Time_Distribution_Reports:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Reports_Edit;
                        FeatureSelection = Feature.Time_Distribution_Reports_Selection;
                        break;
                }
            }
            else if (module != 0)
            {
                this.TargetSoeModule = (SoeModule)module;
                switch (this.TargetSoeModule)
                {
                    case SoeModule.Economy:
                        EnableEconomy = true;
                        this.Feature = Feature.Economy_Distribution_Reports;
                        FeatureEdit = Feature.Economy_Distribution_Reports_Edit;
                        FeatureSelection = Feature.Economy_Distribution_Reports_Selection;
                        break;
                    case SoeModule.Billing:
                        EnableBilling = true;
                        this.Feature = Feature.Billing_Distribution_Reports;
                        FeatureEdit = Feature.Billing_Distribution_Reports_Edit;
                        FeatureSelection = Feature.Billing_Distribution_Reports_Selection;
                        break;
                    case SoeModule.Time:
                        EnableTime = true;
                        this.Feature = Feature.Time_Distribution_Reports;
                        FeatureEdit = Feature.Time_Distribution_Reports_Edit;
                        FeatureSelection = Feature.Time_Distribution_Reports_Selection;
                        break;
                }
            }
        }
    }
}
