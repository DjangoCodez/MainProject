using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;

namespace SoftOne.Soe.Web.soe.economy.customer.customers
{
    public partial class _default : PageBase
    {
        #region Variables

        
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Customer_Customers;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}
