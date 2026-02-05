using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.common.xeconnect
{
    public partial class _default : PageBase
    {
        protected AccountManager am;
        protected int accountYearId;

        //Module specifics       
        protected SoeModule TargetSoeModule = SoeModule.None;        

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
                    case Feature.Economy_Import_XEConnect:
                        TargetSoeModule = SoeModule.Economy;
                        this.Feature = Feature.Economy_Import_XEConnect;
                        break;                        
                    case Feature.Billing_Import_XEConnect:
                        TargetSoeModule = SoeModule.Billing;
                        this.Feature = Feature.Billing_Import_XEConnect;
                        break;
                    case Feature.Time_Import_XEConnect:
                        TargetSoeModule = SoeModule.Time;
                        this.Feature = Feature.Time_Import_XEConnect;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out _);
        }
    }
}
