using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Template/Core")]
    public class CoreTemplateController : ApiBase
    {
        #region Constructor

        public CoreTemplateController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        #region TemplateCompanyItem

        /// Get all ImportCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("TemplateCompanyItems/")]
        [ResponseType(typeof(List<TemplateCompanyItem>))]
        public IHttpActionResult GetTemplateCompanyItem()
        {
            ParameterObject parameterObject = GetParameterObject(0, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetGlobalTemplateCompanyItems());
        }



        #endregion

        #region Import

        /// Get all ImportCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ImportCopyItems/")]
        [ResponseType(typeof(List<ImportCopyItem>))]
        public IHttpActionResult GetImportCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetImportCopyItems(actorCompanyId));
        }
        #endregion

        #region User

        /// Get all UserCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UserCopyItems/")]
        [ResponseType(typeof(List<UserCopyItem>))]
        public IHttpActionResult GetUserCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetUserCopyItems(actorCompanyId));
        }
        #endregion

        #region CompanyFieldSettings
        [HttpGet]
        [Route("CompanyFieldSettingCopyItems/")]
        [ResponseType(typeof(List<CompanyFieldSetting>))]
        public IHttpActionResult GetCompanyFieldSettings(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetCompanyFieldSettingCopyItems(actorCompanyId));
        }

        #endregion

        #region UserCompanySettings
        [HttpGet]
        [Route("CompanySettingCopyItems/")]
        [ResponseType(typeof(List<UserCompanySetting>))]
        public IHttpActionResult GetUserCompanySettings(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetCompanySettingCopyItems(actorCompanyId));
        }


        #endregion

        #region RoleAndFeature
        [HttpGet]
        [Route("RoleAndFeatureCopyItems/")]
        [ResponseType(typeof(List<RoleAndFeatureCopyItem>))]
        public IHttpActionResult GetRoleAndFeatureCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetRoleAndFeatureCopyItems(actorCompanyId));
        }

        [HttpGet]
        [Route("CompanyAndFeatureCopyItems/")]
        [ResponseType(typeof(List<CompanyAndFeatureCopyItem>))]
        public IHttpActionResult GetCompanyAndFeatureCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetCompanyAndFeatureCopyItems(actorCompanyId));
        }

        #endregion

        #region Report

        /// <summary>
        /// Get all ReportCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ReportCopyItems/")]
        [ResponseType(typeof(List<ReportCopyItem>))]
        public IHttpActionResult GetReportCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetReportCopyItems(actorCompanyId));
        }

        /// <summary>
        /// Get all ReportTemplateCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ReportTemplateCopyItems/")]
        [ResponseType(typeof(List<ReportTemplateCopyItem>))]
        public IHttpActionResult GetReportTemplateCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetReportTemplateCopyItems(actorCompanyId));
        }

        /// <summary>
        /// Get all ExternalCodeCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ExternalCodeCopyItems/")]
        [ResponseType(typeof(List<ExternalCodeCopyItem>))]
        public IHttpActionResult GetExternalCodeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, coreTemplateManager.GetExternalCodeCopyItems(actorCompanyId));
        }


        #endregion

        #endregion
    }
}