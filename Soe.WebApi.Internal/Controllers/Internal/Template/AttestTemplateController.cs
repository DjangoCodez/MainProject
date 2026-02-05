using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Attest;
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
    [RoutePrefix("Internal/Template/Attest")]
    public class AttestTemplateController : ApiBase
    {
        #region Constructor

        public AttestTemplateController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all CategoryCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("CategoryCopyItems/")]
        [ResponseType(typeof(List<CategoryCopyItem>))]
        public IHttpActionResult GetCategoryCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            AttestTemplateManager attestTemplateManager = new AttestTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, attestTemplateManager.GetCategoryCopyItems(actorCompanyId));
        }

        /// Get all AttestRoleCopyItems for actorCompanyId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AttestRoleCopyItems/")]
        [ResponseType(typeof(List<AttestRoleCopyItem>))]
        public IHttpActionResult GetAttestRoleCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            AttestTemplateManager attestTemplateManager = new AttestTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, attestTemplateManager.GetAttestRoleCopyItems(actorCompanyId));
        }

        /// <summary>
        /// Get all AttestWorkFlowTemplateHeadCopyItems for actorCompanyId
        /// </summary>
        [HttpGet]
        [Route("AttestWorkFlowTemplateHeadCopyItems/")]
        [ResponseType(typeof(List<AttestWorkFlowTemplateHeadCopyItem>))]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            AttestTemplateManager attestTemplateManager = new AttestTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, attestTemplateManager.GetAttestWorkFlowTemplateHeadCopyItems(actorCompanyId));
        }

        /// <summary>
        /// Get all AttestTransitionCopyItems for actorCompanyId
        /// </summary>
        [HttpGet]
        [Route("AttestTransitionCopyItems/")]
        [ResponseType(typeof(List<AttestTransitionCopyItem>))]
        public IHttpActionResult GetAttestTransitionCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            AttestTemplateManager attestTemplateManager = new AttestTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, attestTemplateManager.GetAttestTransitionCopyItems(actorCompanyId));
        }

        /// <summary>
        /// Get all AttestStateCopyItems for actorCompanyId
        /// </summary>
        [HttpGet]
        [Route("AttestStateCopyItems/")]
        [ResponseType(typeof(List<AttestStateCopyItem>))]
        public IHttpActionResult GetAttestStateCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            CompanyManager companyManager = new CompanyManager(parameterObject);
            AttestTemplateManager attestTemplateManager = new AttestTemplateManager(parameterObject);
            return Content(HttpStatusCode.OK, attestTemplateManager.GetAttestStateCopyItems(actorCompanyId));
        }

        #endregion
    }
}