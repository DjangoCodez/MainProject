using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Economy/Supplier")]
    public class SupplierController : WebApiExternalBase
    {

        #region Constructor

        public SupplierController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #endregion

        #region Suppliers

        /// <summary>
        /// Get Suppliers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Suppliers/")]
        [ResponseType(typeof(List<SupplierIODTO>))]
        public IHttpActionResult GetSuppliers(Guid companyApiKey, Guid connectApiKey, string token, string numberFrom = "", string numberTo = "")
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager( apiManager.GetParameterObject() );
            var supplierIODTOs = importExportManager.GetSupplierIODTOs(apiManager.ActorCompanyId, numberFrom, numberTo);

            return Content(HttpStatusCode.OK, supplierIODTOs);
        }

        [HttpGet]
        [Route("SuppliersFromExternalNr/")]
        [ResponseType(typeof(List<SupplierIODTO>))]
        public IHttpActionResult SuppliersFromExternalNr(Guid companyApiKey, Guid connectApiKey, string token, string externalNr)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var actorManager = new ActorManager(apiManager.GetParameterObject());

            var supplierIds = actorManager.GetInternalIdsListFromExternalNr(TermGroup_CompanyExternalCodeEntity.Supplier, externalNr, apiManager.ActorCompanyId);
            var supplierIODTO = importExportManager.GetSupplierIODTOs(apiManager.ActorCompanyId, "", "", supplierIds);

            return Content(HttpStatusCode.OK, supplierIODTO);
        }

        /// <summary>
        /// Get specific supplier with Id,
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Supplier/")]
        [ResponseType(typeof(SupplierIODTO))]
        public IHttpActionResult GetSupplier(Guid companyApiKey, Guid connectApiKey, string token, int? supplierId, string orgNr)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }
            
            #endregion

            if (supplierId == null && string.IsNullOrEmpty(orgNr))
            {
                return Content(HttpStatusCode.OK, "SupplierId and orgNr can not both be empty");
            }

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var supplierIds = supplierId == null ? null : new List<int> { (int)supplierId };
        
            var supplierIODTOs = importExportManager.GetSupplierIODTOs(apiManager.ActorCompanyId, "", "", supplierIds, orgNr);

            if (supplierIODTOs.Count > 1)
            {
                return Content(HttpStatusCode.OK, "More than 1 matching supplier was found");
            }

            return Content(HttpStatusCode.OK, supplierIODTOs.FirstOrDefault());
        }

        /// <summary>
        /// Save Suppliers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="suppliersIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Suppliers/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveSuppliers(Guid companyApiKey, Guid connectApiKey, string token, List<SupplierIODTO> suppliersIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var supplierIOItem = new SupplierIOItem { Suppliers = new List<SupplierIODTO>() };
            supplierIOItem.Suppliers.AddRange(suppliersIODTOs);

            var result = importExportManager.ImportSupplierIO(supplierIOItem, TermGroup_IOImportHeadType.Supplier, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, true);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region SupplierInvoice

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Invoice/Filter")]
        [ResponseType(typeof(List<SupplierInvoiceIODTO>))]
        public IHttpActionResult GetSupplierInvoices(Guid companyApiKey, Guid connectApiKey, string token, SupplierInvoiceFilterIODTO filter)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Supplier_Invoice, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var customerInvoiceHeadIODTOs = importExportManager.GetSupplierInvoiceIODTOs(apiManager.ActorCompanyId, filter);
            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTOs);
        }

        /// <summary>
        /// Get a single Supplier invoice
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Invoice/")]
        [ResponseType(typeof(SupplierInvoiceIODTO))]
        public IHttpActionResult GetSupplierInvoice(Guid companyApiKey, Guid connectApiKey, string token, string number = null, int? invoiceId = null)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Supplier_Invoice, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (string.IsNullOrEmpty(number) && (!invoiceId.HasValue))
            {
                return Content(HttpStatusCode.OK, (SupplierInvoiceIODTO)null);
            }

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var supplierInvoiceIODTO = importExportManager.GetSupplierInvoiceIODTOs(apiManager.ActorCompanyId, invoiceId);

            return Content(HttpStatusCode.OK, supplierInvoiceIODTO);
        }


        /// <summary>
        /// Get a single Supplier invoice image
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>

        /// <param name="invoiceId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Invoice/Image")]
        [ResponseType(typeof(SupplierInvoiceImageIODTO))]
        public IHttpActionResult GetSupplierInvoiceImage(Guid companyApiKey, Guid connectApiKey, string token, int invoiceId)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Supplier_Invoice, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (invoiceId == 0)
            {
                return Content(HttpStatusCode.OK, (SupplierInvoiceIODTO)null);
            }

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            return Content(HttpStatusCode.OK, importExportManager.GetSupplierInvoiceImageIODTO(apiManager.ActorCompanyId, invoiceId));
        }

        [HttpPost]
        [Route("Invoices/")]
        public IHttpActionResult SaveSupplerInvoices(Guid companyApiKey, Guid connectApiKey, string token, List<SupplierInvoiceIODTO> supplierInvoiceIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Supplier_Invoice, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var supplierInvoiceIOItem = new SupplierInvoiceIOItem();

            foreach (var invoice in supplierInvoiceIODTOs)
            {
                var raw = new SupplierInvoiceIORawData
                {
                    SupplierInvoiceNr = invoice.InvoiceNr,
                    ExternalId = invoice.ExternalId,
                    SupplierNr = invoice.SupplierNr,
                    SupplierOrgNr = invoice.SupplierOrgnr,
                    SupplierExternalNr = invoice.SupplierExternalNr,
                    DueDate = invoice.DueDate,
                    InvoiceDate = invoice.InvoiceDate,
                    VoucherDate = invoice.VoucherDate.HasValue ? invoice.VoucherDate :  invoice.InvoiceDate,
                    Currency = invoice.Currency,
                    CurrencyRate = invoice.CurrencyRate,
                    CurrencyDate = invoice.CurrencyDate,
                    ReferenceOur = invoice.ReferenceOur,
                    ReferenceYour = invoice.ReferenceYour,
                    TotalAmount = invoice.TotalAmount,
                    TotalAmountCurrency = invoice.TotalAmountCurrency,
                    VATAmount = invoice.VATAmount,
                    VATAmountCurrency = invoice.VATAmountCurrency,
                    BillingType = invoice.BillingType,
                    OriginStatus = invoice.OriginStatus
                };
                
                foreach (var row in invoice.InvoiceRows)
                {
                    var rawData = new SupplierInvoiceAccountingRowIORawData
                    {
                        AccountDim2Nr = row.AccountDim2Nr,
                        AccountDim3Nr = row.AccountDim3Nr,
                        AccountDim4Nr = row.AccountDim4Nr,
                        AccountDim5Nr = row.AccountDim5Nr,
                        AccountDim6Nr = row.AccountDim6Nr,
                        AccountNr = row.AccountNr,
                        Amount = row.Amount,
                        AmountCurrency = row.AmountCurrency,
                        Text = row.Text,
                        Quantity = row.Quantity,
                        CreditRow = row.Amount < 0,
                        DebitRow = row.Amount >= 0
                    };
                    raw.Accountingrows.Add(rawData);
                }
                supplierInvoiceIOItem.supplierInvoices.Add(raw);
            }

            var result = importExportManager.ImportSupplierInvoiceIO(supplierInvoiceIOItem, TermGroup_IOImportHeadType.SupplierInvoice,0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, true);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion
    }
}