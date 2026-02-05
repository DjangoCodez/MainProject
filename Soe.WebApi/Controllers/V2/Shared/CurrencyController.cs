using Soe.WebApi.Controllers;

using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Shared
{
    [RoutePrefix("V2/Shared/Currency")]
    public class CurrencyController : SoeApiController
    {
        #region Variables

        private readonly CountryCurrencyManager ccm;

        #endregion

        #region Constructor
        public CurrencyController(CountryCurrencyManager ccm)
        {
            this.ccm = ccm;
        }
        #endregion

        [Route("CompCurrencies/GenericType")]
        public IHttpActionResult GetCompCurrenciesGenericType(bool loadRates, bool nameAsDisplay = true)
        {
            return Content(HttpStatusCode.OK, ccm.GetCompCurrenciesDict(base.ActorCompanyId, loadRates, nameAsDisplay).ToSmallGenericTypes());
        }

    }
}
