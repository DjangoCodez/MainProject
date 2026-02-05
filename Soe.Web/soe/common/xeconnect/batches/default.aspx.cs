using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.xeconnect.batches
{
    public partial class _default : PageBase
    {
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
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}
