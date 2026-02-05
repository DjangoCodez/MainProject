using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using SoftOne.Soe.Common.Util;

namespace Soe.Api.Internal.Controllers.ExternalMiddleware.SuperOffice
{
    [RoutePrefix("Internal/ExternalMiddleware/SuperOffice")]
    public class SuperOfficeController : ApiBase
    {
        #region Constructor

        public SuperOfficeController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("Company/")]
        [ResponseType(typeof(CompanyDTO))]
        public IHttpActionResult GetCompany(string companyApiKey)
        {
            var companyManager = new CompanyManager(null);
            return Content(HttpStatusCode.OK, companyManager.GetCompany(companyManager.GetActorCompanyIdFromApiKey(companyApiKey) ?? 0, true, true, true).ToCompanyDTO());
        }

        [HttpGet]
        [Route("Contracts/")]
        [ResponseType(typeof(List<InvoiceDTO>))]
        public IHttpActionResult GetContracts(string companyApiKey)
        {
            var companyManager = new CompanyManager(null);
            var invoiceManager = new InvoiceManager(null);
            return Content(HttpStatusCode.OK, invoiceManager.GetInvoices(companyManager.GetActorCompanyIdFromApiKey(companyApiKey) ?? 0, SoftOne.Soe.Common.Util.SoeOriginType.Contract).ToDTOs(false)); //TODO incorrect return type.
        }

        [HttpGet]
        [Route("ContractsForGrid/")]
        [ResponseType(typeof(List<CustomerInvoiceGridDTO>))]
        public IHttpActionResult GetContractsForGrid(string companyApiKey, int? daysBack)
        {
            var companyManager = new CompanyManager(null);
            var invoiceManager = new InvoiceManager(null);
            return Content(HttpStatusCode.OK, invoiceManager.GetChangedCustomerInvoices(SoeOriginStatusClassification.ContractsAll, (int)SoeOriginType.Contract, companyManager.GetActorCompanyIdFromApiKey(companyApiKey) ?? 0, 0, true, true, false, true, null, true, daysBack, forceHasPermissions: true)); //TODO incorrect return type.
        }

        [HttpGet]
        [Route("Customers/")]
        [ResponseType(typeof(List<CustomerDTO>))]
        public IHttpActionResult GetCustomers(string companyApiKey)
        {
            var companyManager = new CompanyManager(null);
            var customerManager = new CustomerManager(null);
            return Content(HttpStatusCode.OK, customerManager.GetCustomers(companyManager.GetActorCompanyIdFromApiKey(companyApiKey) ?? 0, true, true, true, true, true, true, true).ToDTOs(true, false, true)); //TODO incorrect return type.
        }

        [HttpGet]
        [Route("Categories/")]
        [ResponseType(typeof(List<CategoryDTO>))]
        public IHttpActionResult GetCustomerCategories(string companyApiKey, SoeCategoryType type)
        {
            var categoryManager = new CategoryManager(null);
            var companyManager = new CompanyManager(null);
            return Content(HttpStatusCode.OK, categoryManager.GetCategories(type, companyManager.GetActorCompanyIdFromApiKey(companyApiKey) ?? 0).ToDTOs(false));
        }

        [HttpGet]
        [Route("ContactEComs/")]
        [ResponseType(typeof(List<CategoryDTO>))]
        public IHttpActionResult GetCustomerCategories(int contactId, bool loadContact)
        {
            var contactManager = new ContactManager(null);
            return Content(HttpStatusCode.OK, contactManager.GetContactEComs(contactId, loadContact).ToDTOs());
        }

        [HttpGet]
        [Route("GetContactFromActor/")]
        [ResponseType(typeof(List<ContactDTO>))]
        public IHttpActionResult GetContactFromActor(int actorId, bool loadActor, bool loadAdresses)
        {
            var contactManager = new ContactManager(null);
            return Content(HttpStatusCode.OK, contactManager.GetContactFromActor(actorId, loadActor, loadAdresses).ToDTO(true, true));
        }
        #endregion
    }
}