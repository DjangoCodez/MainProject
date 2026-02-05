using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Supplier/Product/Pricelist")]
    public class SupplierProductPriceListController : SoeApiController
    {
        #region Variables

        private readonly SupplierProductManager spm;

        #endregion

        #region Constructor

        public SupplierProductPriceListController(SupplierProductManager spm)
        {
            this.spm = spm;

        }

        #endregion

        #region Pricelist

        [HttpGet]
        [Route("Grid/{supplierId:int?}")]
        public IHttpActionResult GetSupplierPricelistsBySupplier(int? supplierId = null)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelists(base.ActorCompanyId, supplierId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{pricelistId:int}")]
        public IHttpActionResult GetSupplierPricelistById(int pricelistId)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelist(base.ActorCompanyId, pricelistId));
        }

        [HttpDelete]
        [Route("{pricelistId:int}")]
        public IHttpActionResult DeleteSupplierPricelist(int pricelistId)
        {
            return Content(HttpStatusCode.OK, spm.DeleteSupplierProductPricelist(pricelistId));
        }

        [HttpGet]
        [Route("Prices/{pricelistId:int}/{includeComparison:bool}")]
        public IHttpActionResult GetSupplierPricelist(int pricelistId, bool includeComparison)
        {
            return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelistPrices(pricelistId, includeComparison));
        }

        [HttpPost]
        [Route("Compare")]
        public IHttpActionResult GetSupplierProductPriceCompare(SupplierProductPriceSearchDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.GetSupplierProductPriceCompare(base.ActorCompanyId, model.SupplierId, model.currencyId, model.CompareDate, model.IncludePricelessProducts));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveSupplierPricelist(SupplierProductPriceListSaveDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.SaveSupplierProductPricelist(model.PriceList, model.PriceRows));
        }
        [HttpGet]
        [Route("Import/{importToPriceList:bool}/{importPrices:bool}/{multipleSuppliers:bool}")]
        public IHttpActionResult GetSupplierPricelistImport(bool importToPriceList, bool importPrices, bool multipleSuppliers)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, spm.GetSupplierProductPricelistImportFields(true, importToPriceList, importPrices, multipleSuppliers));
        }
        [HttpPost]
        [Route("Import/Perform")]
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