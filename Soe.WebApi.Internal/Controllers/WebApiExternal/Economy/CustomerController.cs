using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Linq;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Economy/Customer")]
    public class CustomerController : WebApiExternalBase
    {
        
        #region Constructor

        public CustomerController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get Customers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Customers/")]
        [ResponseType(typeof(List<CustomerIODTO>))]
        public IHttpActionResult GetCustomers(Guid companyApiKey, Guid connectApiKey, string token, string numberFrom = "", string numberTo = "", DateTime? modifiedSince = null)
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
            var customerIODTOs = importExportManager.GetCustomerIODTOs(apiManager.ActorCompanyId, numberFrom, numberTo, null, null, modifiedSince);

            return Content(HttpStatusCode.OK, customerIODTOs);
        }

        /// <summary>
        /// Get Customers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="searchDTO"></param>
        /// <param name="maxNrRows"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Search")]
        [ResponseType(typeof(List<CustomerSearchResultIODTO>))]
        public IHttpActionResult SearchCustomers(Guid companyApiKey, Guid connectApiKey, string token, CustomerSearchIODTO searchDTO, int maxNrRows)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Customer_Customers, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var customerIODTOs = importExportManager.SearchCustomer(apiManager.ActorCompanyId, searchDTO, maxNrRows, false);

            return Content(HttpStatusCode.OK, customerIODTOs);
        }

        /// <summary>
        /// Get Customers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Search/All")]
        [ResponseType(typeof(List<CustomerSearchResultIODTO>))]
        public IHttpActionResult SearchCustomers(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Billing_Customer_Customers, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var customerIODTOs = importExportManager.SearchCustomer(apiManager.ActorCompanyId, new CustomerSearchIODTO(), 99999, true);

            return Content(HttpStatusCode.OK, customerIODTOs);
        }

        /// <summary>
        /// Get Customers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="numberFrom"></param>
        /// <param name="numberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomersFromExternalNr/")]
        [ResponseType(typeof(List<CustomerIODTO>))]
        public IHttpActionResult CustomersFromExternalNr(Guid companyApiKey, Guid connectApiKey, string token, string externalNr)
        {

            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }
            
            #endregion

            if (string.IsNullOrEmpty(externalNr))
            {
                return Content(HttpStatusCode.OK, "Parameter externalNr can not be empty");
            }
            
            var importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            var actorManager = new ActorManager(apiManager.GetParameterObject() );

            var customerIds = actorManager.GetInternalIdsListFromExternalNr(TermGroup_CompanyExternalCodeEntity.Customer, externalNr, apiManager.ActorCompanyId);
            var customerIODTOs = customerIds.Any() ? importExportManager.GetCustomerIODTOs(apiManager.ActorCompanyId, null, null, customerIds) : new List<CustomerIODTO>();

            return Content(HttpStatusCode.OK, customerIODTOs);
        }

        /// <summary>
        /// Get specific customer with Id,
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Customer/")]
        [ResponseType(typeof(CustomerIODTO))]
        public IHttpActionResult GetCustomer(Guid companyApiKey, Guid connectApiKey, string token, int? customerId, string orgNr)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (customerId == null && string.IsNullOrEmpty(orgNr))
            {
                return Content(HttpStatusCode.OK, "CustomerId and orgNr can not both be empty");
            }

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var customerIds = customerId == null ? null : new List<int> { (int)customerId };
            var customerIODTOs = importExportManager.GetCustomerIODTOs(apiManager.ActorCompanyId, "", "", customerIds, orgNr);

            if (customerIODTOs.Count > 1)
            {
                return Content(HttpStatusCode.OK, "More than 1 matching customer was found");
            }

            return Content(HttpStatusCode.OK, customerIODTOs.FirstOrDefault());
        }

        /// <summary>
        /// Save Customers
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="customersIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Customers/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveCustomers(Guid companyApiKey, Guid connectApiKey, string token, List<CustomerIODTO> customersIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject() );
            var customerIOItem = new CustomerIOItem { Customers = new List<CustomerIODTO>() };
            customerIOItem.Customers.AddRange(customersIODTOs);
            
            var result = importExportManager.ImportCustomerIO(customerIOItem, TermGroup_IOImportHeadType.Customer, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, "", true);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

    }
}