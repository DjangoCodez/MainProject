using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getSysCurrency : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["country"], out int sysCountryId) && Int32.TryParse(QS["actorCompanyId"], out int actorCompanyId))
            {
                CountryCurrencyManager ccm = new CountryCurrencyManager(ParameterObject);
                Currency currency = ccm.GetCurrencyFromCountry(actorCompanyId, sysCountryId);
                if (currency != null)
                {
                    ResponseObject = new
                    {
                        Found = true,
                        CurrencyId = currency.CurrencyId,
                    };
                }
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
