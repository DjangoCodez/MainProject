using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.supplier
{
    public partial class _default : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Feature = Feature.Economy_Supplier;
        }
    }
}
