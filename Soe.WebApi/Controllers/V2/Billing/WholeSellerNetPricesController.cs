using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("Billing/WholesellerNetPrices")]
    public class WholeSellerNetPricesController: SoeApiController
    {
        #region Variables

        private readonly WholsellerNetPriceManager wpm;

        #endregion

        #region Constructor

        public WholeSellerNetPricesController(WholsellerNetPriceManager wpm)
        {
            this.wpm = wpm;
        }

        #endregion

        #region NetPrices

        [HttpGet]
        [Route("Wholesellers/{onlyCurrentCountry:bool}/{onlySeparateFile:bool}")]
        public IHttpActionResult GetWholeSellers(bool onlyCurrentCountry, bool onlySeparateFile)
        {
            return Content(HttpStatusCode.OK, wpm.WholesellersWithNetPricesDict(onlyCurrentCountry, onlySeparateFile).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{sysWholeSellerId:int}")]
        public IHttpActionResult GetNetPrices(int sysWholeSellerId)
        {
            return Content(HttpStatusCode.OK, wpm.GetNetPrices(base.ActorCompanyId, sysWholeSellerId));
        }

        [HttpPost]
        [Route("Rows/Delete")]
        public IHttpActionResult DeleteNetPriceRows(SupplierNetPricesDeleteModel model)
        {
            return Content(HttpStatusCode.OK, wpm.DeleteRows(model.WholsellerNetPriceRowIds, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Import/")]
        public IHttpActionResult SaveNetPrices(SupplierAgreementModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, wpm.Import(model.Files,model.WholesellerId, model.PriceListTypeId,base.ActorCompanyId));
        }

        #endregion
    }
}