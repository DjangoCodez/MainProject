using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.import.edi
{
    public partial class _default : PageBase
    {
        #region Variables
        
        private AccountManager am;

        protected SoeModule TargetSoeModule = SoeModule.Billing;
        protected Feature FeatureEdit = Feature.Billing_Import_EDI;
        public int accountYearId;
        public bool accountYearIsOpen;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            EnableModuleSpecifics();
            base.Page_Init(sender, e);
        }

        private void EnableModuleSpecifics()
        {
            if (QS["type"] != null)
            {
                int type = Convert.ToInt32(QS["type"]);
                switch (type)
                {
                    case (int)TermGroup_EDISourceType.EDI:
                        this.Feature = Feature.Billing_Import_EDI;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);            
        }
    }
}