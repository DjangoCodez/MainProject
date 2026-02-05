using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Linq;
using SoftOne.Soe.Business.Util;
using System.Web.Http.Description;
using SoftOne.Soe.Common.DTO.ApiExternal;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Economy/Billing")]
    public class BillingController : WebApiExternalBase
    {

        #region Constructor

        public BillingController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            
        }

        #endregion

        #region Methods

        #region CustomerInvoices

        /// <summary>
        /// Search customer invoices
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="searchDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerInvoices/Search")]
        [ResponseType(typeof(List<CustomerInvoiceSearchResultIODTO>))]
        public IHttpActionResult SearchCustomerInvoices(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceSearchIODTO searchDTO, int maxNrRows)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Invoice_Invoices, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var searchResult = importExportManager.SearchCustomerInvoice(apiManager.ActorCompanyId, searchDTO, maxNrRows);
            return Content(HttpStatusCode.OK, searchResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerInvoices/Filter")]
        [ResponseType(typeof(List<CustomerInvoiceSmallIODTO>))]
        public IHttpActionResult GetCustomerInvoices(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceFilterIODTO filter)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Invoice_Invoices, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceSmallIODTOs(apiManager.ActorCompanyId, SoeOriginType.CustomerInvoice, filter);
            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTOs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerInvoices/")]
        [ResponseType(typeof(List<CustomerInvoiceIODTO>))]
        public IHttpActionResult GetCustomerInvoices(Guid companyApiKey, Guid connectApiKey, string token, DateTime? dateFrom, DateTime? dateTo, string numberFrom = "", string numberTo = "", int? invoiceId = null)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.CustomerInvoice, null, dateFrom, dateTo, numberFrom, numberTo, invoiceId);
            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTOs);
        }

        /// <summary>
        /// Get a single CustomerInvoice
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerInvoice/")]
        [ResponseType(typeof(CustomerInvoiceIODTO))]
        public IHttpActionResult GetCustomerInvoice(Guid companyApiKey, Guid connectApiKey, string token, string number, int? invoiceId)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (string.IsNullOrEmpty(number) && (!invoiceId.HasValue))
            {
                return Content(HttpStatusCode.OK, (CustomerInvoiceIODTO)null);
            }

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.CustomerInvoice, null, null, null, number, number, invoiceId);

            var customerInvoiceHeadIODTO = (customerInvoiceHeadIODTOs.Any()) ?
                                            customerInvoiceHeadIODTOs.FirstOrDefault() :
                                            new CustomerInvoiceIODTO();

            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTO);
        }

        /// <summary>
        /// Get a the Id from ExternalId
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>
        /// <param name="externalId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("InvoiceIdsFromExternalId/")]
        [ResponseType(typeof(List<int>))]
        public IHttpActionResult GetCustomerInvoiceIdsFromExternalId(Guid companyApiKey, Guid connectApiKey, string token, SoeOriginType type, string externalId)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var invoiceIds = importExportManager.GetCustomerInvoiceIdsFromExternalId(apiManager.ActorCompanyId, type, externalId);

            return Content(HttpStatusCode.OK, invoiceIds);
        }

        /// <summary>
        /// Save a list of Customerinvoices. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTOs"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerInvoices/")]
        public IHttpActionResult SaveCustomerInvoices(Guid companyApiKey, Guid connectApiKey, string token, List<CustomerInvoiceIODTO> customerInvoiceIODTOs, int? importId, string specialFunctionalities)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject() );

            var customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
            customerInvoiceIOItem.CustomerInvoices.AddRange(customerInvoiceIODTOs);
            
            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save a single Customerinvoices. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTO"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerInvoice/")]
        public IHttpActionResult SaveCustomerInvoice(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceIODTO customerInvoiceIODTO, int? importId, string specialFunctionalities)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            CustomerInvoiceIOItem customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
            customerInvoiceIOItem.CustomerInvoices.Add(customerInvoiceIODTO);
            
            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);

        }

        #endregion

        #region Customer Invoice Payments

        /// <summary>
        /// Get payments. If you want all use a long date interval.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>
        /// <param name="searchDTO"></param>
        /// 
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerInvoicePayments/Search")]
        [ResponseType(typeof(List<PaymentRowImportIODTO>))]
        public IHttpActionResult GetPaymentRowImportIODTOs(Guid companyApiKey, Guid connectApiKey, string token, PaymentSearchIODTO searchDTO)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            List<PaymentRowImportIODTO> paymentRowImportIODTO = importExportManager.GetPaymentRowImportIODTOs(apiManager.ActorCompanyId, SoeOriginType.CustomerPayment, searchDTO);

            return Content(HttpStatusCode.OK, paymentRowImportIODTO);
        }

        /// <summary>
        /// Get payments. If you want all use a long date interval.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerInvoicePayments/")]
        [ResponseType(typeof(List<PaymentRowImportIODTO>))]
        public IHttpActionResult GetPaymentRowImportIODTOs(Guid companyApiKey, Guid connectApiKey, string token, DateTime dateFrom, DateTime dateTo)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            List<PaymentRowImportIODTO> paymentRowImportIODTO = importExportManager.GetPaymentRowImportIODTOs(apiManager.ActorCompanyId, SoeOriginType.CustomerPayment, new PaymentSearchIODTO {PayDateFrom = dateFrom, PayDateTo = dateTo, IncludeAccountInformation = true });

            return Content(HttpStatusCode.OK, paymentRowImportIODTO);
        }

        /// <summary>
        /// Get payment information for specific customerinvoice).
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerInvoicePayments/PaymentInformationIODTO")]
        [ResponseType(typeof(PaymentInformationIODTO))]
        public IHttpActionResult GetPaymentInformationIODTO(Guid companyApiKey, Guid connectApiKey, string token, string invoiceNr, string externalId, int? invoiceId = null)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            InvoiceManager invoiceManager = new InvoiceManager(apiManager.GetParameterObject());
            var invoice = new Invoice();

            if (!invoiceId.HasValue && invoiceId != 0)
            {
                if (!string.IsNullOrEmpty(externalId))
                {
                    invoiceId = importExportManager.GetCustomerInvoiceIdsFromExternalId(apiManager.ActorCompanyId, SoeOriginType.CustomerInvoice, externalId).FirstOrDefault();
                }

                if (!invoiceId.HasValue && !string.IsNullOrEmpty(invoiceNr))
                {
                    invoiceId = invoiceManager.GetInvoiceId(invoiceNr, apiManager.ActorCompanyId, SoeOriginType.CustomerInvoice);
                }
            }

            if (invoiceId.HasValue)
            {
                invoice = invoiceManager.GetInvoice(invoiceId.Value, loadOrigin: true);
            }

            List<PaymentRowImportIODTO> paymentRowImportIODTOs = importExportManager.GetPaymentRowImportIODTOs(apiManager.ActorCompanyId, SoeOriginType.CustomerPayment, new PaymentSearchIODTO { InvoiceId = invoiceId, IncludeAccountInformation = true });

            var paymentInformationIODTO = new PaymentInformationIODTO();

            if (invoiceId != null)
            {
                paymentInformationIODTO.invoiceId = invoice.InvoiceId;
                paymentInformationIODTO.FullyPaid = invoice.FullyPayed;
                paymentInformationIODTO.PaidAmount = invoice.PaidAmount;
                paymentInformationIODTO.PaymentRowImportIODTOs = paymentRowImportIODTOs;
            }
            else
            {
                paymentInformationIODTO.PaymentRowImportIODTOs = new List<PaymentRowImportIODTO>();
            }

            return Content(HttpStatusCode.OK, paymentInformationIODTO);
        }

        /// <summary>
        /// Save Payments connected to CustomerInvoices.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="paymentRowImportIODTOs"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerInvoicePayments/")]
        [ResponseType(typeof(List<PaymentRowImportIODTO>))]
        public IHttpActionResult SavePaymentRowImportIODTOs(Guid companyApiKey, Guid connectApiKey, string token, List<PaymentRowImportIODTO> paymentRowImportIODTOs)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Customer_Payment_Payments_Edit, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            paymentRowImportIODTOs.ForEach(a =>
            {
                if (string.IsNullOrEmpty(a.InvoiceNr))
                    a.InvoiceNr = a.InvoiceExternalId;
            });

            var result = importExportManager.ImportPaymentFromIO(paymentRowImportIODTOs, apiManager.ActorCompanyId, TermGroup_IOType.WebAPI,false);

            return Content(HttpStatusCode.OK, result);
        }


        #endregion

        #region Orders


        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="orderNr"></param>
        /// <param name="definitive"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Order/CreateInvoice")]
        public IHttpActionResult CreateInvoiceFromOrder(Guid companyApiKey, Guid connectApiKey, string token, int orderNr, bool definitive)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Order_Status_OrderToInvoice, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var searchResult = importExportManager.CreateInvoiceFromOrder(orderNr, definitive, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, searchResult);
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="searchDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Orders/Search")]
        [ResponseType(typeof(List<CustomerOrderSearchResultIODTO>))]
        public IHttpActionResult SearchOrder(Guid companyApiKey, Guid connectApiKey, string token, CustomerOrderSearchIODTO searchDTO, int maxNrRows)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Order_Orders, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var searchResult = importExportManager.SearchCustomerOrder(apiManager.ActorCompanyId, searchDTO, maxNrRows);
            return Content(HttpStatusCode.OK, searchResult);
        }

        /// <summary>
        /// Get a list of Orders. Use the date and number intervals to get a more specific selection
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <param name="es"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Orders/")]
        [ResponseType(typeof(List<CustomerInvoiceIODTO>))]
        public IHttpActionResult GetOrders(Guid companyApiKey, Guid connectApiKey, string token, DateTime? dateFrom, DateTime? dateTo, string numberFrom = "", string numberTo = "")
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.Order, null, dateFrom, dateTo, numberFrom, numberTo,null,true);
            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTOs);
        }

        /// <summary>
        /// Get one Order.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Order/")]
        [ResponseType(typeof(CustomerInvoiceIODTO))]
        public IHttpActionResult GetOrder(Guid companyApiKey, Guid connectApiKey, string token, string number, int? invoiceId = null)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.Order, null, null, null, number, number, invoiceId,true);
            CustomerInvoiceIODTO customerInvoiceHeadIODTO = (customerInvoiceHeadIODTOs.Any() ) ? customerInvoiceHeadIODTOs.FirstOrDefault() : new CustomerInvoiceIODTO();

            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTO);
        }

        /// <summary>
        /// Save a list of Orders. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTOs"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Orders/")]
        public IHttpActionResult SaveOrders(Guid companyApiKey, Guid connectApiKey, string token, List<CustomerInvoiceIODTO> customerInvoiceIODTOs, int? importId, string specialFunctionalities)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            var customerInvoiceIOItem = new CustomerInvoiceIOItem { CustomerInvoices = new List<CustomerInvoiceIODTO>() };
            customerInvoiceIOItem.CustomerInvoices.AddRange(customerInvoiceIODTOs);
            
            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save a single Order. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTO"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Order/")]
        public IHttpActionResult SaveOrder(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceIODTO customerInvoiceIODTO, int? importId, string specialFunctionalities)
        {
            {
                #region Validation

                string validatationResult;
                var apiManager = new ApiManager(null);
                if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
                {
                    return Content(HttpStatusCode.Unauthorized, validatationResult);
                }

                #endregion

                if (importId == null)
                    importId = 0;

                ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
                CustomerInvoiceIOItem customerInvoiceIOItem = new CustomerInvoiceIOItem();
                customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
                customerInvoiceIOItem.CustomerInvoices.Add(customerInvoiceIODTO);
                var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
                return Content(HttpStatusCode.OK, result);
            }
        }


        /// <summary>
        /// Save a single Order. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTO"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Order/Update")]
        public IHttpActionResult UpdateOrder(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceOrderUpdateIODTO updateIODTO)
        {
            {
                #region Validation

                string validatationResult;
                var apiManager = new ApiManager(null);
                if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Order_Orders_Edit, out validatationResult))
                {
                    return Content(HttpStatusCode.Unauthorized, validatationResult);
                }

                #endregion
        
                var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
                var result = importExportManager.ImportUpdateCustomerInvoiceIO(updateIODTO, apiManager.ActorCompanyId, SoeOriginType.Order);
                return Content(HttpStatusCode.OK, result);
            }
        }

        #endregion

        #region Contracts

        /// <summary>
        /// Get a list of Contracts. Use the date and number intervals to get a more specific selection
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <param name="es"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Contracts/")]
        [ResponseType(typeof(List<CustomerInvoiceIODTO>))]
        public IHttpActionResult GetContracts(Guid companyApiKey, Guid connectApiKey, string token, DateTime? dateFrom, DateTime? dateTo, string numberFrom = "", string numberTo = "")
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.Contract, null, dateFrom, dateTo, numberFrom, numberTo);
            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTOs);
        }
        /// <summary>
        /// Get one Contract.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Contract/")]
        [ResponseType(typeof(CustomerInvoiceIODTO))]
        public IHttpActionResult GetContract(Guid companyApiKey, Guid connectApiKey, string token, string number, int? invoiceId = null)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            CustomerInvoiceIODTO customerInvoiceHeadIODTO = new CustomerInvoiceIODTO();
            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.Contract, null, null, null, number, number, invoiceId);

            if (customerInvoiceHeadIODTOs.Any() )
                customerInvoiceHeadIODTO = customerInvoiceHeadIODTOs.FirstOrDefault();

            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTO);
        }

        /// <summary>
        /// Save a list of Contracts. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTOs"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Contracts/")]
        public IHttpActionResult SaveContracts(Guid companyApiKey, Guid connectApiKey, string token, List<CustomerInvoiceIODTO> customerInvoiceIODTOs, int? importId, string specialFunctionalities)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            CustomerInvoiceIOItem customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
            customerInvoiceIOItem.CustomerInvoices.AddRange(customerInvoiceIODTOs);
            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save a single Contract. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTO"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Contract/")]
        public IHttpActionResult SaveContract(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceIODTO customerInvoiceIODTO, int? importId, string specialFunctionalities)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            CustomerInvoiceIOItem customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
            customerInvoiceIOItem.CustomerInvoices.Add(customerInvoiceIODTO);

            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);
        }


        #endregion

        #region Offers

        /// <summary>
        /// Get a list of Offers. Use the date and number intervals to get a more specific selection
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Offers/")]
        [ResponseType(typeof(List<CustomerInvoiceIODTO>))]
        public IHttpActionResult GetOffers(Guid companyApiKey, Guid connectApiKey, string token, DateTime? dateFrom, DateTime? dateTo, string numberFrom = "", string numberTo = "")
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.Offer, null, dateFrom, dateTo, numberFrom, numberTo);
            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTOs);
        }
        /// <summary>
        /// Get one Offer.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Offer/")]
        [ResponseType(typeof(CustomerInvoiceIODTO))]
        public IHttpActionResult GetOffer(Guid companyApiKey, Guid connectApiKey, string token, string number, int? invoiceId = null)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            CustomerInvoiceIODTO customerInvoiceHeadIODTO = new CustomerInvoiceIODTO();
            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );

            var customerInvoiceHeadIODTOs = importExportManager.GetCustomerInvoiceIODTOs(apiManager.ActorCompanyId, SoeOriginType.Offer, null, null, null, number, number, invoiceId);

            if (customerInvoiceHeadIODTOs.Any())
                customerInvoiceHeadIODTO = customerInvoiceHeadIODTOs.FirstOrDefault();

            return Content(HttpStatusCode.OK, customerInvoiceHeadIODTO);
        }

        /// <summary>
        /// Save a list of Offers. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>z
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTOs"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Offers/")]
        public IHttpActionResult SaveOffers(Guid companyApiKey, Guid connectApiKey, string token, List<CustomerInvoiceIODTO> customerInvoiceIODTOs, int? importId, string specialFunctionalities)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            CustomerInvoiceIOItem customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
            customerInvoiceIOItem.CustomerInvoices.AddRange(customerInvoiceIODTOs);
            
            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save a single Offer. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="customerInvoiceIODTO"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Offer/")]
        public IHttpActionResult SaveOffer(Guid companyApiKey, Guid connectApiKey, string token, CustomerInvoiceIODTO customerInvoiceIODTO, int? importId, string specialFunctionalities)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (importId == null)
                importId = 0;

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            CustomerInvoiceIOItem customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
            customerInvoiceIOItem.CustomerInvoices.Add(customerInvoiceIODTO);

            var result = importExportManager.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, (int)importId, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, specialFunctionalities, true);
            return Content(HttpStatusCode.OK, result);

        }


        #endregion

        #region Pricelists

        /// <summary>
        /// Get products/Articles
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Pricelists")]
        [ResponseType(typeof(List<PriceListTypeIODTO>))]
        public IHttpActionResult Pricelists(Guid companyApiKey, Guid connectApiKey, string token, bool? onlyStandard = null)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Preferences_InvoiceSettings_Pricelists, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var priceLists = importExportManager.GetPriceLists(apiManager.ActorCompanyId, onlyStandard.GetValueOrDefault());
            return Content(HttpStatusCode.OK, priceLists);
        }
        #endregion

        #region Products

        /// <summary>
        /// Get all product groups
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("Product/ProductGroups")]
        [ResponseType(typeof(List<ProductGroupIODTO>))]
        public IHttpActionResult GetProductGroups(Guid companyApiKey, Guid connectApiKey, string token)
        {

            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var productManager = new ProductGroupManager(apiManager.GetParameterObject());
            var vatCodes = productManager.GetProductGroups(apiManager.ActorCompanyId);

            var vatCodesIO = vatCodes.Select(x => new ProductGroupIODTO
            {
                Name = x.Name,
                Code = x.Code
            }).ToList();
            return Content(HttpStatusCode.OK, vatCodesIO);
        }

        /// <summary>
        /// Get all product units
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Product/Units")]
        [ResponseType(typeof(List<ProductUnitIODTO>))]
        public IHttpActionResult GetProductUnits(Guid companyApiKey, Guid connectApiKey, string token)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Product_Products, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var searchResult = importExportManager.GetProductUnits(apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, searchResult);
        }

        /// <summary>
        /// Get product price
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="customerId"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Products/Price")]
        [ResponseType(typeof(List<InvoiceProductPriceResultIODTO>))]
        public IHttpActionResult GetProductPrice(Guid companyApiKey, Guid connectApiKey, string token, int customerId, List<InvoiceProductPriceSearchIODTO> items)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Product_Products_ShowSalesPrice, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var searchResult = importExportManager.GetProductPrice(customerId, items, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, searchResult);
        }

        /// <summary>
        /// Get product price
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="customerId"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Products/Stockbalance")]
        [ResponseType(typeof(List<ProductStockBalanceIODTO>))]
        public IHttpActionResult GetProductStockbalance(Guid companyApiKey, Guid connectApiKey, string token, [FromUri] List<int> productIds, string stockCode)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Stock_Place, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var result = importExportManager.GetProductStockbalance(productIds, stockCode, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Search products/Articles
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="searchDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Products/Search")]
        [ResponseType(typeof(List<InvoiceProductSearchResultIODTO>))]
        public IHttpActionResult SearchProducts(Guid companyApiKey, Guid connectApiKey, string token, InvoiceProductSearchIODTO searchDTO, int maxNrRows)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Product_Products, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var searchResult = importExportManager.SearchInvoiceProduct(apiManager.ActorCompanyId, searchDTO, maxNrRows);
            return Content(HttpStatusCode.OK, searchResult);
        }

        /// <summary>
        /// Get products/Articles
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Products/")]
        [ResponseType(typeof(List<InvoiceProductIODTO>))]
        public IHttpActionResult GetProducts(Guid companyApiKey, Guid connectApiKey, string token, string numberFrom = "", string numberTo = "", int? productGroupId = null, DateTime? modifiedSince = null, bool? includeInactive = false)
        {
            
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject() );

            var productHeadIODTOs = importExportManager.GetInvoiceProductIODTOs(apiManager.ActorCompanyId, numberFrom, numberTo, productGroupId, modifiedSince, includeInactive ?? false);
            return Content(HttpStatusCode.OK, productHeadIODTOs);
        }

        /// <summary>
        /// Get products/Articles
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Products/Filter")]
        [ResponseType(typeof(List<InvoiceProductIODTO>))]
        public IHttpActionResult GetProducts(Guid companyApiKey, Guid connectApiKey, string token, InvoiceProductFilterIODTO filter)
        {

            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Product_Products, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());

            var productHeadIODTOs = importExportManager.GetInvoiceProductIODTOs(apiManager.ActorCompanyId, filter);
            return Content(HttpStatusCode.OK, productHeadIODTOs);
        }

        /// <summary>
        /// Get a single Product
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Product/")]
        [ResponseType(typeof(InvoiceProductIODTO))]
        public IHttpActionResult GetProduct(Guid companyApiKey, Guid connectApiKey, string token, string number)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            
            var productHeadIODTO = new InvoiceProductIODTO();
            var importExportManager = new ImportExportManager(apiManager.GetParameterObject() );

            var productIODTOs = importExportManager.GetInvoiceProductIODTOs(apiManager.ActorCompanyId, number, number, null, null,false);

            if (productIODTOs.Any())
                productHeadIODTO = productIODTOs.FirstOrDefault();

            return Content(HttpStatusCode.OK, productHeadIODTO);
        }

        /// <summary>
        /// Save a list of Customerinvoices. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="InvoiceProductIODTOs"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Products/")]
        public IHttpActionResult SaveProducts(Guid companyApiKey, Guid connectApiKey, string token, List<InvoiceProductIODTO> InvoiceProductIODTOs, int? importId, string specialFunctionalities)
        {
            #region Validation

            string validatationResult;
            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var result = importExportManager.ImportFromInvoiceProductIO(InvoiceProductIODTOs, apiManager.ActorCompanyId,false);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save a single Customerinvoices. ImportId should be used only when special functionality connected to an import is needed. You can also add special functionality in a string, contact SoftOne for more information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="InvoiceProductIODTO"></param>
        /// <param name="importId"></param>
        /// <param name="specialFunctionalities"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Product/")]
        public IHttpActionResult SaveProduct(Guid companyApiKey, Guid connectApiKey, string token, InvoiceProductIODTO InvoiceProductIODTO, int? importId, string specialFunctionalities)
        {
            {
                #region Validation

                string validatationResult;
                var apiManager = new ApiManager(null);
                if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
                {
                    return Content(HttpStatusCode.Unauthorized, validatationResult);
                }

                #endregion

                List<InvoiceProductIODTO> InvoiceProductIODTOs = new List<InvoiceProductIODTO>();
                InvoiceProductIODTOs.Add(InvoiceProductIODTO);

                var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
                var result = importExportManager.ImportFromInvoiceProductIO(InvoiceProductIODTOs, apiManager.ActorCompanyId,false);
                return Content(HttpStatusCode.OK, result);
            }
        }

        #endregion


        #endregion

    }
}