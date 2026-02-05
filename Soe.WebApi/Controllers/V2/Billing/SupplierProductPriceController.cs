using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Supplier/Product/Price")]
    public class SupplierProductPriceController : SoeApiController
    {
        #region Variables

        private readonly SupplierProductManager spm;

        #endregion

        #region Constructor

        public SupplierProductPriceController(SupplierProductManager spm)
        {
            this.spm = spm;

        }

        #endregion

        #region ProductPrice

        [HttpGet]
        [Route("List/{supplierProductId:int}")]
        public IHttpActionResult GetSupplierProductPrices(int supplierProductId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPrices(supplierProductId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("{supplierProductId:int}/{currentDate}/{quantity}/{currencyId}")]
        public IHttpActionResult GetSupplierInvoiceProductPrice(int supplierProductId, string currentDate, decimal quantity, int currencyId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPrice(supplierProductId, BuildDateTimeFromString(currentDate, true), quantity, currencyId));
        }

        [HttpGet]
        [Route("{productId:int}/{supplierId:int}/{currentDate}/{quantity}/{currencyId}")]
        public IHttpActionResult GetInvoiceProductPrice(int productId, int supplierId, string currentDate, decimal quantity, int currencyId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPrice(productId, supplierId, BuildDateTimeFromString(currentDate, true), quantity, currencyId));
        }

        #endregion
    }
}