using SoftOne.Soe.Business.Billing.Template.Managers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Template/Billing")]
    public class BillingTemplateController : ApiBase
    {
        #region Constructor

        public BillingTemplateController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods


        /// <summary>
        /// Get all InvoiceProductCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of InvoiceProductCopyItem</returns>
        [HttpGet]
        [Route("InvoiceProductCopyItems")]
        [ResponseType(typeof(List<InvoiceProductCopyItem>))]
        public IHttpActionResult GetInvoiceProductCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<InvoiceProductCopyItem> invoiceProductCopyItems = billingTemplateManager.GetInvoiceProductCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, invoiceProductCopyItems);
        }

        /// <summary>
        /// Get all productUnitCopyItems for the specified actorCompanyId
        /// </summary>
        /// returns>List of ProductUnitCopyItem</returns>
        [HttpGet]
        [Route("ProductUnitCopyItems")]
        [ResponseType(typeof(List<ProductUnitCopyItem>))]
        public IHttpActionResult GetProductUnitCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<ProductUnitCopyItem> productUnitCopyItems = billingTemplateManager.GetProductUnitCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, productUnitCopyItems);
        }

        /// <summary>
        /// Get all PriceListCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PriceListCopyItem</returns>
        [HttpGet]
        [Route("PriceListCopyItems")]
        [ResponseType(typeof(List<PriceListCopyItem>))]
        public IHttpActionResult GetPriceListCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<PriceListCopyItem> PriceListCopyItems = billingTemplateManager.GetPriceListCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, PriceListCopyItems);
        }

        /// <summary>
        /// Get all SupplierAgreementCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of SupplierAgreementCopyItem</returns>
        [HttpGet]
        [Route("SupplierAgreementCopyItems")]
        [ResponseType(typeof(List<SupplierAgreementCopyItem>))]
        public IHttpActionResult GetSupplierAgreementCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<SupplierAgreementCopyItem> SupplierAgreementCopyItems = billingTemplateManager.GetSupplierAgreementCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, SupplierAgreementCopyItems);
        }

        /// <summary>
        /// Get all ChecklistCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of ChecklistCopyItem</returns>
        [HttpGet]
        [Route("ChecklistCopyItems")]
        [ResponseType(typeof(List<ChecklistCopyItem>))]
        public IHttpActionResult GetChecklistCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<ChecklistCopyItem> ChecklistCopyItems = billingTemplateManager.GetChecklistCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, ChecklistCopyItems);
        }

        /// <summary>
        /// Get all EmailTemplateCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of EmailTemplateCopyItem</returns>
        [HttpGet]
        [Route("EmailTemplateCopyItems")]
        [ResponseType(typeof(List<EmailTemplateCopyItem>))]
        public IHttpActionResult GetEmailTemplateCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<EmailTemplateCopyItem> EmailTemplateCopyItems = billingTemplateManager.GetEmailTemplateCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, EmailTemplateCopyItems);
        }

        /// <summary>
        /// Get all CompanyWholesellerPriceListCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of CompanyWholesellerPriceListCopyItem</returns>
        [HttpGet]
        [Route("CompanyWholesellerPriceListCopyItems")]
        [ResponseType(typeof(List<CompanyWholesellerPriceListCopyItem>))]
        public IHttpActionResult GetCompanyWholesellerPriceListCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<CompanyWholesellerPriceListCopyItem> CompanyWholesellerPriceListCopyItems = billingTemplateManager.GetCompanyWholesellerPriceListCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, CompanyWholesellerPriceListCopyItems);
        }

        /// <summary>
        /// Get all PriceRuleCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>PriceRuleCopyItems</returns>
        [HttpGet]
        [Route("PriceRuleCopyItems")]
        [ResponseType(typeof(PriceRuleCopyItem))]
        public IHttpActionResult GetPriceRuleCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);

            return Content(HttpStatusCode.OK, billingTemplateManager.GetPriceRuleCopyItem(actorCompanyId));
        }

        /// <summary>
        /// Get all PriceRuleCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>PriceRuleCopyItems</returns>
        [HttpGet]
        [Route("ProjectSettingsCopyItem")]
        [ResponseType(typeof(ProjectSettingsCopyItem))]
        public IHttpActionResult GetProjectSettingsCopyItem(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);

            return Content(HttpStatusCode.OK, billingTemplateManager.GetProjectSettingsCopyItem(actorCompanyId));
        }

        #endregion
    }
}