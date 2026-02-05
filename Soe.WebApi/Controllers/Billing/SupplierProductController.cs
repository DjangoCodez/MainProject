using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Supplier/Product")]
    public class SupplierProductController : SoeApiController
    {
        #region Variables

        private readonly SupplierProductManager spm;
        
        #endregion

        #region Constructor

        public SupplierProductController(SupplierProductManager spm)
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

        [HttpGet]
        [Route("Products/Small/{supplierId:int}")]
        public IHttpActionResult GetSupplierProductsSmall(int supplierId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductsSmall(supplierId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("{supplierProductId:int}")]
        public IHttpActionResult GetSupplierProduct(int supplierProductId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProduct(supplierProductId, base.ActorCompanyId).ToDTO() );
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
        public IHttpActionResult SaveProduct(SupplierProductSaveDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.SaveSupplierProduct(model.Product, model.PriceRows));
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

        #endregion

        #region ProductPrice

        [HttpGet]
        [Route("Price/List/{supplierProductId:int}")]
        public IHttpActionResult GetSupplierProductPrices(int supplierProductId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPrices(supplierProductId, base.ActorCompanyId));
        }

        
        [HttpGet]
        [Route("Price/{supplierProductId:int}/{currentDate}/{quantity}/{currencyId}")]
        public IHttpActionResult GetSupplierInvoiceProductPrices(int supplierProductId, string currentDate, decimal quantity, int currencyId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPrice(supplierProductId, BuildDateTimeFromString(currentDate, true), quantity, currencyId));
        }

        [HttpGet]
        [Route("Price/{productId:int}/{supplierId:int}/{currentDate}/{quantity}/{currencyId}")]
        public IHttpActionResult GetSupplierInvoiceProductPrices(int productId, int supplierId, string currentDate, decimal quantity, int currencyId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPrice(productId, supplierId, BuildDateTimeFromString(currentDate, true), quantity, currencyId));
        }

        #endregion

        #region Pricelist

        [HttpGet]
        [Route("Pricelist/List/{supplierId:int}")]
        public IHttpActionResult GetSupplierPricelists(int supplierId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelists(base.ActorCompanyId, supplierId));
        }

        [HttpGet]
        [Route("Pricelist/{pricelistId:int}")]
        public IHttpActionResult GetSupplierPricelist(int pricelistId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelist(base.ActorCompanyId, pricelistId));
        }

        [HttpDelete]
        [Route("Pricelist/{pricelistId:int}")]
        public IHttpActionResult DeleteSupplierPricelist(int pricelistId)
        {
            return Content(HttpStatusCode.OK, spm.DeleteSupplierProductPricelist(pricelistId));
        }

        [HttpGet]
        [Route("Pricelist/Prices/{pricelistId:int}/{includeComparison:bool}")]
        public IHttpActionResult GetSupplierPricelist(int pricelistId, bool includeComparison)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelistPrices(pricelistId, includeComparison));
        }

        [HttpPost]
        [Route("Pricelist/Compare")]
        public IHttpActionResult GetSupplierPricelists(SupplierProductPriceSearchDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
               return Content(HttpStatusCode.OK, spm.GetSupplierProductPriceCompare(base.ActorCompanyId, model.SupplierId, model.currencyId, model.CompareDate, model.IncludePricelessProducts));
        }

        [HttpPost]
        [Route("Pricelist")]
        public IHttpActionResult SaveSupplierPricelist(SupplierProductPriceListSaveDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.SaveSupplierProductPricelist(model.PriceList, model.PriceRows));
        }
        [HttpGet]
        [Route("Pricelist/Import/{importToPriceList:bool}/{importPrices:bool}/{multipleSuppliers:bool}")]
        public IHttpActionResult GetSupplierPricelistImport(bool importToPriceList, bool importPrices, bool multipleSuppliers)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelistImportFields(true, importToPriceList, importPrices, multipleSuppliers));
        }
        [HttpPost]
        [Route("Pricelist/Import/Perform")]
        public IHttpActionResult PerformPricelistImport(SupplierProductImportDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.PerformSupplierPriceListImport(model.ImportToPriceList, model.SupplierId, model.PriceListId, model.Rows, model.Options));
        }
        #endregion

    }
}