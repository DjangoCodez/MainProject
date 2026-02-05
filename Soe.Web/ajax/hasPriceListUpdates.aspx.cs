using SoftOne.Soe.Business.Core;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class hasPriceListUpdates : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int companyId = string.IsNullOrEmpty(QS["c"]) ? 0 : Convert.ToInt32(QS["c"]);

            WholeSellerManager wm = new WholeSellerManager(ParameterObject);
            if (companyId == 0)
            {
                ResponseObject = new { Success = false };
            }                
            else
            {
                bool result = wm.HasCompanyPriceListUpdates(companyId);
                ResponseObject = new { Success = result };
            }
        }
    }
}
