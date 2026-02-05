using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Product")]
    public class ProductController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;
        private readonly ProductManager pm;
        private readonly InvoiceManager im;
        private readonly CommodityCodeManager cm;

        #endregion

        #region Constructor

        public ProductController(AccountManager am, ProductManager pm, InvoiceManager im, CommodityCodeManager cm)
        {
            this.am = am;
            this.pm = pm;
            this.im = im;
            this.cm = cm;
        }

        #endregion

        #region Intrastat - CommodityCodes

        [HttpGet]
        [Route("CommodityCodes/{onlyActive:bool}/{ignoreCodeStateCheck:bool}")]
        public IHttpActionResult GetCustomerCommodyCodes(bool onlyActive, bool? ignoreCodeStateCheck)
        {
            return Content(HttpStatusCode.OK, cm.GetCustomerCommodityCodes(base.ActorCompanyId, onlyActive, ignoreCodeStateCheck ?? false));
        }

        [HttpGet]
        [Route("CommodityCodes/Dict/{addEmpty:bool}")]
        public IHttpActionResult GetCustomerCommodyCodesDict(bool addEmpty)
        {
            return Content(HttpStatusCode.OK, cm.GetCustomerCommodityCodesDict(base.ActorCompanyId, addEmpty));
        }

        [HttpPost]
        [Route("CommodityCodes/")]
        public IHttpActionResult SaveCustomerCommodityCodes(UpdateEntityStatesModel model)
        {
            return Content(HttpStatusCode.OK, cm.SaveCustomerCommodityCodes(model.Dict, base.ActorCompanyId));
        }

        #endregion

        #region InvoiceProduct

        [HttpGet]
        [Route("InvoiceProduct/{invoiceProductVatType:int}/{addEmptyRow:bool}")]
        public IHttpActionResult getInvoiceProducts(int invoiceProductVatType, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProductsDict(base.ActorCompanyId, (TermGroup_InvoiceProductVatType)invoiceProductVatType, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("InvoiceProduct/{excludeExternal:bool}")]
        public IHttpActionResult GetInvoiceProductsSmall(bool excludeExternal)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProductsSmall(base.ActorCompanyId, excludeExternal));
        }

        #endregion

        #region Product
        [HttpGet]
        [Route("Products/{active:bool}/{loadProductUnitAndGroup:bool}/{loadAccounts:bool}/{loadCategories:bool}/{loadTimeCode:bool}")]
        public IHttpActionResult GetProducts(bool active, bool loadProductUnitAndGroup, bool loadAccounts, bool loadCategories, bool loadTimeCode)
        {
            if (Request.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
            {
                return Content(HttpStatusCode.OK, pm.GetProductsSmall(base.ActorCompanyId, true));
            } 
            else if (Request.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
            {
                return Content(HttpStatusCode.OK, pm.GetInvoiceProducts(base.ActorCompanyId, active, loadProductUnitAndGroup, loadAccounts, loadCategories, 0, loadTimeCode).ToGridDTOs());
            }
            return Content(HttpStatusCode.OK, pm.GetInvoiceProducts(base.ActorCompanyId, active, loadProductUnitAndGroup, loadAccounts, loadCategories, 0, loadTimeCode).ToDTOs());
        }
        [HttpGet]
        [Route("Products/ForSelect")]
        public IHttpActionResult GetProductForSelect()
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProductsTiny(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Products/{productId}")]
        public IHttpActionResult GetProduct(int productId)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProduct(productId, true, true, true, true).ToDTO(true, true));
        }


        [HttpGet]
        [Route("Search/{number}/{name}")]
        public IHttpActionResult SearchInvoiceProducts(string number, string name)
        {
            List<InvoiceProductSearchViewDTO>  dtos = pm.SearchInvoiceProducts(base.ActorCompanyId, number.ToLower() != "null" ? number : string.Empty, name.ToLower() != "null" ? name : string.Empty);
            return Content(HttpStatusCode.OK, dtos);
        }


        [HttpGet]
        [Route("Search/Extended/{number}/{name}/{group}/{text}")]
        public IHttpActionResult SearchInvoiceProductsExtended(string number, string name, string group, string text)
        {
            List<InvoiceProductSearchViewDTO> dtos = pm.SearchInvoiceProducts(base.ActorCompanyId, number.ToLower() != "null" ? number : string.Empty, name.ToLower() != "null" ? name : string.Empty, group.ToLower() != "null" ? group : string.Empty, text.ToLower() != "null" ? text : string.Empty, 200);
            return Content(HttpStatusCode.OK, dtos);
        }

        [HttpGet]
        [Route("ProductRows/{productId:int}")]
        public IHttpActionResult GetProductRowsProduct(int productId)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProduct(productId, loadProductUnitAndGroup: true).ToProductRowsProductDTO());
        }

        [HttpPost]
        [Route("ProductRows/List/")]
        public IHttpActionResult GetProductRowsProducts(ProductsSimpleModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProducts(base.ActorCompanyId, model.ProductIds, true, true, null).ToProductRowsProductDTOs());
        }

        [HttpPost]
        [Route("ExternalUrls/")]
        public IHttpActionResult GetProductExternalUrls(ProductsSimpleModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetProductExternalUrls(base.ActorCompanyId, model.ProductIds));
        }

        [HttpGet]
        [Route("Accounts/{rowId:int}/{productId:int}/{projectId:int}/{customerId:int}/{employeeId:int}/{vatType:int}/{getSalesAccounts:bool}/{getPurchaseAccounts:bool}/{getVatAccounts:bool}/{getInternalAccounts:bool}/{isTimeProjectRow:bool}/{tripartiteTrade:bool}")]
        public IHttpActionResult GetProductAccounts(int rowId, int productId, int projectId, int customerId, int employeeId, TermGroup_InvoiceVatType vatType, bool getSalesAccounts, bool getPurchaseAccounts, bool getVatAccounts, bool getInternalAccounts, bool isTimeProjectRow, bool tripartiteTrade)
        {
            return Content(HttpStatusCode.OK, am.GetInvoiceProductAccounts(base.ActorCompanyId, productId, projectId, customerId, employeeId, vatType, getSalesAccounts, getPurchaseAccounts, getVatAccounts, getInternalAccounts, isTimeProjectRow));
        }

        [HttpGet]
        [Route("HouseholdDeductionType/{addEmptyRow:bool}")]
        public IHttpActionResult GetHouseholdDeductionTypes(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetSysHouseholdTypeDict(addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("LiftProducts")]
        public IHttpActionResult GetLiftProducts()
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProductsByCalculationType(base.ActorCompanyId,TermGroup_InvoiceProductCalculationType.Lift));
        }

        [HttpPost]
        [Route("CopyInvoiceProduct")]
        public IHttpActionResult CopyExternalInvoiceProduct(CopyInvoiceProductModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                var result = new InvoiceProductCopyResult();

                decimal outPurchasePrice = model.PurchasePrice;
                decimal outSalesPrice = model.SalesPrice;
                InvoiceProductDTO product = pm.CopyExternalInvoiceProduct(model.ProductId, model.PurchasePrice, model.SalesPrice, model.ProductUnit, model.PriceListTypeId, model.PriceListHeadId, model.SysWholesellerName, model.CustomerId, base.ActorCompanyId, true, model.Origin, out outPurchasePrice, out outSalesPrice).ToDTO(false, false);
                if (product != null)
                {
                    product.PurchasePrice = outPurchasePrice;
                    product.SalesPrice = outSalesPrice;

                    result = new InvoiceProductCopyResult
                    {
                        Product = product,
                        SysWholesellerName = model.SysWholesellerName
                    };
                }

                return Content(HttpStatusCode.OK, result);
            }
        }

        [HttpPost]
        [Route("SaveInvoiceProduct")]
        public IHttpActionResult SaveInvoiceProduct(SaveInvoiceProductModel model)
        {
            return Content(HttpStatusCode.OK, pm.SaveInvoiceProduct(model.invoiceProduct, model.priceLists, model.categoryRecords, base.ActorCompanyId, model.stocks, model.translations, model.extrafields));
        }

        [HttpPost]
        [Route("Products/UpdateState")]
        public IHttpActionResult UpdateProductState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.UpdateProductsState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Products/{productId}")]
        public IHttpActionResult DeleteProduct(int productId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.DeleteInvoiceProduct(productId, base.ActorCompanyId));
        }

        #endregion

        #region Product prices
        [HttpPost]
        [Route("Prices/Search")]
        public IHttpActionResult SearchInvoiceProductPrices(SearchProductPricesModel model)
        {
            return Content(HttpStatusCode.OK, pm.SearchInvoiceProductPricesByNumber(base.ActorCompanyId, model.PriceListTypeId, model.CustomerId, new List<string>() { model.Number }, true, (SoeSysPriceListProviderType)model.ProviderType));
        }

        [HttpGet]
        [Route("Prices/{priceListTypeId:int}/{productId:int}/{customerId:int}/{currencyId:int}/{wholesellerId:int}/{quantity:int}/{returnFormula:bool}/{copySysProduct:bool}")]
        public IHttpActionResult GetProductPrice(int priceListTypeId, int productId, int customerId, int currencyId, int wholesellerId, int quantity, bool returnFormula, bool copySysProduct)
        {
            return Content(HttpStatusCode.OK, pm.GetProductPrice(base.ActorCompanyId, new ProductPriceRequestDTO { PriceListTypeId = priceListTypeId, ProductId = productId, Quantity = quantity, CustomerId = customerId, CurrencyId = currencyId, WholesellerId = wholesellerId, ReturnFormula = returnFormula, CopySysProduct = copySysProduct }, null, true));
        }

        [HttpPost]
        [Route("Prices/Collection")]
        public IHttpActionResult GetProductPrices(ProductPricesRequestDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.GetProductPrices(base.ActorCompanyId, model));
            }
        }

        [HttpGet]
        [Route("Prices/{priceListTypeId:int}/{productId:int}")]
        public IHttpActionResult GetProductPriceDecimal(int priceListTypeId, int productId)
        {
            return Content(HttpStatusCode.OK, pm.GetProductPriceDecimal(productId, priceListTypeId));
        }

        [HttpGet]
        [Route("Prices/CustomerInvoice/{productId:int}/{customerInvoiceId:int}/{quantity}")]
        public IHttpActionResult GetProductPriceForCustomerInvoice(int productId, int customerInvoiceId, decimal quantity)
        {
            return Content(HttpStatusCode.OK, pm.GetProductPriceForCustomerInvoice(productId, customerInvoiceId, base.ActorCompanyId, quantity));
        }

        [HttpGet]
        [Route("Prices/PriceList/{priceListTypeId:int}/{loadAll:bool}")]
        public IHttpActionResult GetPriceListPrices(int priceListTypeId, bool loadAll)
        {
            var pplm = new ProductPricelistManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, pplm.GetPriceListPrices(ActorCompanyId, priceListTypeId, loadAll));
        }

        [HttpPost]
        [Route("Prices/PriceList/")]
        public IHttpActionResult SavePriceListPrices(SavePriceListsModel model)
        {
            var pplm = new ProductPricelistManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, pplm.SavePriceListPrices(ActorCompanyId, model.priceListTypeId, model.priceLists, model.deletedPriceLists));
        }

        #endregion

        #region ProductUnit

        [HttpGet]
        [Route("ProductUnit")]
        public IHttpActionResult GetProductUnits()
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnits(base.ActorCompanyId).ToSmallDTOs());
        }

        [HttpGet]
        [Route("ProductUnit/{productUnitId:int}")]
        public IHttpActionResult GetProductUnit(int productUnitId)
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnit(productUnitId).ToDTO());
        }

        [HttpPost]
        [Route("ProductUnit/")]
        public IHttpActionResult SaveProductUnit(ProductUnitModel model)
        {
            return Content(HttpStatusCode.OK, pm.SaveProductUnit(model.ProductUnit, model.Translations));
        }

        [HttpDelete]
        [Route("ProductUnit/{productUnitId:int}")]
        public IHttpActionResult DeleteProductUnit(int productUnitId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteProductUnit(new ProductUnit() { ProductUnitId = productUnitId }));
        }

        #endregion

        #region ProductUnitConverts

        [HttpGet]
        [Route("ProductUnitConvert/{productId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetProductUnitConverts(int productId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnitConverts(productId, addEmptyRow).ToDTOs());
        }

        [HttpPost]
        [Route("SaveProductUnitConvert")]
        public IHttpActionResult SaveProductUnitConvert(List<ProductUnitConvertDTO> unitConvertDTOs)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.AddUpdateProductUnitConverts(unitConvertDTOs));
        }

        [HttpPost]
        [Route("ProductUnitConvert/Parse")]
        public IHttpActionResult ParseProductUnitConversionFile(ProductUnitFileModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.ParseProductUnitConversionFile(model.ProductIds, model.FileData));
        }

        #endregion

        #region ProductStatistics

        [HttpPost]
        [Route("Statistics/")]
        public IHttpActionResult GetProductStatistics(ProductStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceRowsByProductForAngular(base.ActorCompanyId, model.ProductId, model.OriginType, model.AllItemSelection));
        }

        #endregion

        #region SysProductGroups

        [HttpGet]
        [Route("VVSGroupsForSearch")]
        public IHttpActionResult GetVVSProductGroupsForSearch()
        {
            return Content(HttpStatusCode.OK, pm.GetVVSProductGroupsForSearch());
        }
        #endregion
    }
}