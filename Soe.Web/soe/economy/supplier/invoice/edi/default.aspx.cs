using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.supplier.invoice.edi
{
    public partial class _default : PageBase
    {
        #region Variables
        
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            EnableFeatureSpecifics();
            base.Page_Init(sender, e);
        }

        private void EnableFeatureSpecifics()
        {
            if (QS["feature"] != null)
            {
                int featureId = Convert.ToInt32(QS["feature"]);
                switch (featureId)
                {
                    case (int)Feature.Economy_Supplier_Invoice_Scanning:
                        this.Feature = Feature.Economy_Supplier_Invoice_Scanning;
                        //ediType = TermGroup_EDISourceType.Scanning;
                        break;
                    case (int)Feature.Economy_Supplier_Invoice_Finvoice:
                        this.Feature = Feature.Economy_Supplier_Invoice_Finvoice;
                        //ediType = TermGroup_EDISourceType.Finvoice;
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
