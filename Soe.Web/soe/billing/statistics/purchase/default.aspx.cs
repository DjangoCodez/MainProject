using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.statistics.purchase
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        public int accountYearId;
        public bool accountYearIsOpen;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Statistics_Purchase;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
        }
    }
}