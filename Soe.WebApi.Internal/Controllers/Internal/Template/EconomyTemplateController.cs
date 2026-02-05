using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models.Economy;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Template/Economy")]
    public class EconomyTemplateController : ApiBase
    {
        #region Constructor

        public EconomyTemplateController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        #region AccountDim

        /// Get all AccountDimCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AccountDimCopyItems/")]
        [ResponseType(typeof(List<AccountDimCopyItem>))]
        public IHttpActionResult GetAccountDimCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetAccountDimCopyItems(actorCompanyId));
        }
        #endregion

        #region AccountStd

        /// Get all AccountStdCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AccountStdCopyItems/")]
        [ResponseType(typeof(List<AccountStdCopyItem>))]
        public IHttpActionResult GetAccountStdCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetAccountStdCopyItems(actorCompanyId));
        }
        #endregion

        #region AccountInternal

        /// Get all AccountInternalCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AccountInternalCopyItems/")]
        [ResponseType(typeof(List<AccountInternalCopyItem>))]
        public IHttpActionResult GetAccountInternalCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetAccountInternalCopyItems(actorCompanyId));
        }
        #endregion

        #region AccountYear

        /// <summary>
        /// Get all AccountYearCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of AccountYearCopyItem</returns>
        [HttpGet]
        [Route("AccountYearCopyItems")]
        [ResponseType(typeof(List<AccountYearCopyItem>))]
        public IHttpActionResult GetAccountYearCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<AccountYearCopyItem> accountYearCopyItems = economyTemplateManager.GetAccountYearCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, accountYearCopyItems);
        }

        /// <summary>
        /// Get all VoucherSeriesTypeCopyItem for the specified actorCompanyId
        /// </summary>
        /// <returns>List of VoucherSeriesTypeCopyIte</returns>
        [HttpGet]
        [Route("VoucherSeriesTypeCopyItems")]
        [ResponseType(typeof(List<VoucherSeriesTypeCopyItem>))]
        public IHttpActionResult GetVoucherSeriesTypeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<VoucherSeriesTypeCopyItem> voucherSeriesTypeCopyItems = economyTemplateManager.GetVoucherSeriesTypeCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, voucherSeriesTypeCopyItems);
        }

        /// <summary>
        /// Get all GetPaymentMethodCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PaymentMethodCopyItem</returns>
        [HttpGet]
        [Route("PaymentMethodCopyItems")]
        [ResponseType(typeof(List<PaymentMethodCopyItem>))]
        public IHttpActionResult GetPaymentMethodCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<PaymentMethodCopyItem> paymentMethodCopyItems = economyTemplateManager.GetPaymentMethodCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, paymentMethodCopyItems);
        }

        /// <summary>
        /// Get all GrossProfitCodeCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of GrossProfitCodeCopyItem</returns>
        [HttpGet]
        [Route("GrossProfitCodeCopyItems")]
        [ResponseType(typeof(List<GrossProfitCodeCopyItem>))]
        public IHttpActionResult GetGrossProfitCodeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<GrossProfitCodeCopyItem> grossProfitCodeCopyItems = economyTemplateManager.GetGrossProfitCodeCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, grossProfitCodeCopyItems);
        }

        /// <summary>
        /// Get all InventoryWriteOffMethodCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of InventoryWriteOffMethodCopyItem</returns>
        [HttpGet]
        [Route("InventoryWriteOffMethodCopyItems")]
        [ResponseType(typeof(List<InventoryWriteOffMethodCopyItem>))]
        public IHttpActionResult GetInventoryWriteOffMethodCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<InventoryWriteOffMethodCopyItem> InventoryWriteOffMethodCopyItems = economyTemplateManager.GetInventoryWriteOffMethods(actorCompanyId);

            return Content(HttpStatusCode.OK, InventoryWriteOffMethodCopyItems);
        }

        /// <summary>
        /// Get all InventoryWriteOffTemplateCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of InventoryWriteOffTemplateCopyItem</returns>
        [HttpGet]
        [Route("InventoryWriteOffTemplateCopyItems")]
        [ResponseType(typeof(List<InventoryWriteOffTemplateCopyItem>))]
        public IHttpActionResult GetInventoryWriteOffTemplateCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<InventoryWriteOffTemplateCopyItem> InventoryWriteOffTemplateCopyItems = economyTemplateManager.GetInventoryWriteOffTemplateCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, InventoryWriteOffTemplateCopyItems);
        }


        /// <summary>
        /// Get all PaymentConditionCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PaymentConditionCopyItem</returns>
        [HttpGet]
        [Route("PaymentConditionCopyItems")]
        [ResponseType(typeof(List<PaymentConditionCopyItem>))]
        public IHttpActionResult GetPaymentConditionCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<PaymentConditionCopyItem> paymentConditionCopyItems = economyTemplateManager.GetPaymentConditionCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, paymentConditionCopyItems);
        }

        /// <summary>
        /// Get all VatCodeCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of VatCodeCopyItems</returns>
        [HttpGet]
        [Route("VatCodeCopyItems")]
        [ResponseType(typeof(List<PaymentConditionCopyItem>))]
        public IHttpActionResult GetVatCodeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            List<VatCodeCopyItem> paymentConditionCopyItems = economyTemplateManager.GetVatCodeCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, paymentConditionCopyItems);
        }


        #endregion

        #region AccountDistribution

        /// Get all AccountInternalCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AccountDistributionCopyItems")]
        [ResponseType(typeof(List<AccountDistribitionCopyItem>))]
        public IHttpActionResult GetAccountDistributionCopyItems(int actorCompanyId, int distributionType)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetAccountDistribitionCopyItems(actorCompanyId, (SoeAccountDistributionType)distributionType));
        }

        #endregion

        #region Supplier

        /// Get all SupplierCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]  
        [Route("SupplierCopyItems")]
        [ResponseType(typeof(List<SupplierCopyItem>))]
        public IHttpActionResult GetSupplierCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, new List<SupplierCopyItem>() { economyTemplateManager.GetSupplierCopyItems(actorCompanyId) });
        }

        #endregion

        #region DistributionCodes

        /// Get all AccountInternalCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DistributionCodeCopyItems/")]
        [ResponseType(typeof(List<DistributionCodeHeadCopyItem>))]
        public IHttpActionResult GetDistributinCodeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetDistribitionCodeHeadCopyItems(actorCompanyId));
        }

        #endregion

        #region VoucherTemplates

        /// Get all VoucherTemplatesCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("VoucherTemplatesCopyItems/")]
        [ResponseType(typeof(List<VoucherTemplatesCopyItem>))]
        public IHttpActionResult GetVoucherTemplatesCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetVoucherTemplatesCopyItems(actorCompanyId));
        }

        #endregion

        #region ResidualCodes

        /// Get all GetResidualCodeCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ResidualCodeCopyItems/")]
        [ResponseType(typeof(List<ResidualCodeCopyItem>))]
        public IHttpActionResult GetResidualCodeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, economyTemplateManager.GetResidualCodeCopyItems(actorCompanyId));
        }        

        #endregion

        #endregion
    }
}