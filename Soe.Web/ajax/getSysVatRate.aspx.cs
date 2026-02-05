using SoftOne.Soe.Business.Core;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getSysVatRate : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AccountManager am = new AccountManager(ParameterObject);
            if (Int32.TryParse(QS["vatAccId"], out int vatAccountId))
            {
                decimal rate = am.GetSysVatRateValue(vatAccountId, false);
                ResponseObject = new
                {
                    Found = true,
                    Value = rate
                };
            }
            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}
