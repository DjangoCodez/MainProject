using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Supplier/Product")]
    public class SupplierPurchaseProductController : SoeApiController
    {
        #region Variables
        private readonly SupplierProductManager spm;
        #endregion

        #region ctor
        public SupplierPurchaseProductController(SupplierProductManager spm)
        {
            this.spm = spm;
        }
        #endregion

        #region SupplierProduct

        [HttpPost]
        [Route("Products/")]
        public IHttpActionResult GetSupplierProductList(SupplierProductSearchDTO model)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductsForGrid(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Products/Dict")]
        public IHttpActionResult GetSupplierProductListDict(SupplierProductSearchDTO model)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductsForGridDict(model, base.ActorCompanyId).ToSmallGenericTypes()); 
        }

        [HttpGet]
        [Route("Products/Small/{supplierId:int}")]
        public IHttpActionResult GetSupplierProductsSmall(int supplierId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductsSmall(supplierId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Products/Dict/{supplierId:int}")]
        public IHttpActionResult GetSupplierProductsDict(int supplierId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductsDict(supplierId, base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{supplierProductId:int}")]
        public IHttpActionResult GetSupplierProduct(int supplierProductId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProduct(supplierProductId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("Suppliers/{invoiceProductId:int}")]
        public IHttpActionResult GetSupplierByInvoiceProduct(int invoiceProductId)
        {
            return Content(HttpStatusCode.OK, spm.GetSuppliersByInvoiceProduct(this.ActorCompanyId, invoiceProductId));
        }

        [HttpGet]
        [Route("{invoiceProductId:int}/{supplierId:int}")]
        public IHttpActionResult GetSupplierProductByInvoiceProduct(int invoiceProductId, int supplierId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductByInvoiceProduct(invoiceProductId, supplierId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveProduct(SupplierProductDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.SaveSupplierProduct(model, model.PriceRows ?? new List<SupplierProductPriceDTO>()));
        }

        [HttpDelete]
        [Route("{supplierProductId:int}")]
        public IHttpActionResult DeleteProduct(int supplierProductId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.DeleteSupplierProduct(supplierProductId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Products/PriceUpdate")]
        public IHttpActionResult PerformPriceUpdate(SupplierProductPriceUpdateModel model)
        {
            return Content(HttpStatusCode.OK, spm.UpdateSupplierProductPrices(base.ActorCompanyId, model.SupplierProductIds, model.PriceUpdate, model.UpdateExisting, model.DateFrom, model.DateTo, model.PriceComparisonDate, model.CurrencyId, model.QuantityFrom, model.QuantityTo));
        }

        #endregion
    }
}