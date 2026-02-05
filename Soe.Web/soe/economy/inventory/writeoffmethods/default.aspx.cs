using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.inventory.writeoffmethods
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Inventory_WriteOffMethods;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing        
        }
    }
}
