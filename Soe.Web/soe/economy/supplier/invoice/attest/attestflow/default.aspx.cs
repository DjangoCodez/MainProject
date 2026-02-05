using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.Util;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web.soe.economy.supplier.invoice.attest.attestflow
{
    public partial class _default : PageBase
    {
        #region Variables
        
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Supplier_Invoice_AttestFlow;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}