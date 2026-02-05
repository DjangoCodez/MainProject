using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Currency")]
    public class CoreCurrencyController : SoeApiController
    {
        #region Variables

        private readonly CountryCurrencyManager ccm;

        #endregion

        #region Constructor

        public CoreCurrencyController(CountryCurrencyManager ccm)
        {
            this.ccm = ccm;
        }

        #endregion

        #region Currency
        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveCurrency(CurrencyDTO currency)
        {
            return Content(HttpStatusCode.OK, ccm.SaveCurrency(currency));
        }

        [HttpDelete]
        [Route("{currencyId:int}")]
        public IHttpActionResult DeleteCurrency(int currencyId)
        {
            return Content(HttpStatusCode.OK, ccm.DeleteCurrency(base.ActorCompanyId, currencyId));
        }

        [HttpGet]
        [Route("{currencyId:int}")]
        public IHttpActionResult GetCurrency(int currencyId)
        {
            return Content(HttpStatusCode.OK, ccm.GetCurrencyAndRateById(currencyId, this.ActorCompanyId, true).ToDTO());
        }

        [HttpGet]
        [Route("Grid/{currencyId:int}")]
        public IHttpActionResult GetCurrenciesGrid(int currencyId)
        {
            if (currencyId != 0)
            {
                var currency = ccm.GetCurrencyWithCode(currencyId);
                return Content(HttpStatusCode.OK, new List<CurrencyGridDTO>() { currency.ToGridDTO() });
            }
            return Content(HttpStatusCode.OK, ccm.GetCurrenciesWithSysCurrency(this.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Sys")]
        public IHttpActionResult GetSysCurrencies()
        {
            return Content(HttpStatusCode.OK, ccm.GetSysCurrencies(true).ToDTOs());
        }

        [HttpGet]
        [Route("Sys/Dict")]
        public IHttpActionResult GetSysCurrenciesDict()
        {
            bool addEmptyRow = Request.GetBoolValueFromQS("addEmptyRow");
            bool useCode = Request.GetBoolValueFromQS("useCode");

            List<SysCurrency> currencies = ccm.GetSysCurrencies(true);
            IEnumerable<SmallGenericType> result = currencies
                .ToSmallGenericTypes(addEmptyRow, useCode);

            return Content(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("Comp")]
        public IHttpActionResult GetCompCurrencies(bool loadRates)
        {
            return Content(HttpStatusCode.OK, ccm.GetCompCurrencies(base.ActorCompanyId, loadRates).ToDTOs(loadRates));
        }

        [HttpGet]
        [Route("Comp/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetCompCurrenciesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, ccm.GetCompCurrenciesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Comp/DictSmall")]
        public IHttpActionResult GetCompCurrenciesDictSmall()
        {
            return Content(HttpStatusCode.OK, ccm.GetCompCurrencies(base.ActorCompanyId, false).ToSmallDTOs());
        }

        [HttpGet]
        [Route("Comp/{sysCurrencyId:int}/{date}/{rateToBase:bool}")]
        public IHttpActionResult GetCompCurrencyRate(int sysCurrencyId, string date, bool rateToBase)
        {
            return Content(HttpStatusCode.OK, ccm.GetCurrencyRate(base.ActorCompanyId, sysCurrencyId, BuildDateTimeFromString(date, true).Value, rateToBase));
        }

        [HttpGet]
        [Route("Ledger/{actorId:int}")]
        public IHttpActionResult GetLedgerCurrency(int actorId)
        {
            return Content(HttpStatusCode.OK, ccm.GetLedgerCurrency(base.ActorCompanyId, actorId).ToDTO(false));
        }

        [HttpGet]
        [Route("Enterprise")]
        public IHttpActionResult GetEnterpriseCurrency()
        {
            return Content(HttpStatusCode.OK, ccm.GetCompanyBaseEntCurrency(base.ActorCompanyId).ToDTO(false));
        }

        [HttpGet]
        [Route("Comp/BaseCurrency")]
        public IHttpActionResult GetCompanyCurrency()
        {
            return Content(HttpStatusCode.OK, ccm.GetCompanyBaseCurrency(base.ActorCompanyId).ToDTO(false));
        }

        #endregion

    }
}