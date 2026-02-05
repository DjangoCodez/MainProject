using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;


namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("System")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SystemController : WebApiExternalBase
    {
        #region Constructor

        public SystemController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
           
        }

        #endregion

        #region Methods

        #region SysTerms

        #region SystermGroup

        /// <summary>
        /// Get all SystermGroups
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SysTermGroup/")]
        [ResponseType(typeof(List<SysTermGroupDTO>))]
        public IHttpActionResult GetSysTermGroups(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var termManager = new TermManager(apiManager.GetParameterObject() );
            var sysTermGroups = termManager.GetSysTermGroupDTOs();
            return Content(HttpStatusCode.OK, sysTermGroups);
        }

        /// <summary>
        /// Compare that all systermgroups are the same, get a list of changed items back
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysTermGroupDTOs"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SysTermGroup/Compare")]
        [ResponseType(typeof(List<SysTermGroupDTO>))]
        public IHttpActionResult CompareSysTermGroups(Guid companyApiKey, Guid connectApiKey, string token, List<SysTermGroupDTO> sysTermGroupDTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var termManager = new TermManager( apiManager.GetParameterObject() );
            List<SysTermGroupDTO> originalSysTermGroups = termManager.GetSysTermGroupDTOs();
            List<SysTermGroupDTO> comparedSysTermGroups = termManager.CompareSysTermGroupDTOs(originalSysTermGroups, sysTermGroupDTOs);
            return Content(HttpStatusCode.OK, comparedSysTermGroups);
        }

        /// <summary>
        /// Update SystermGroups, make sure PostChange has value
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysTermGroupDTOs"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SysTermGroup/Update")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult UpdateSysTermGroups(Guid companyApiKey, Guid connectApiKey, string token, List<SysTermGroupDTO> sysTermGroupDTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            TermManager termManager = new TermManager(apiManager.GetParameterObject());
            ActionResult result = termManager.UpdateSysTermGroups(sysTermGroupDTOs);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region Systerm

        /// <summary>
        /// Get all SystermGroups, leave langid to null is you want all languages
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="langId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SysTerm/")]
        [ResponseType(typeof(List<SysTermDTO>))]
        public IHttpActionResult GetSysterms(Guid companyApiKey, Guid connectApiKey, string token, int? langId = null)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            TermManager termManager = new TermManager(apiManager.GetParameterObject() );
            var sysTermDTOs = termManager.GetSysTermDTOsFromDatabase();
            return Content(HttpStatusCode.OK, sysTermDTOs);
        }

        /// <summary>
        /// Compare that all systerms are the same, get a list of changed items back
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysTermDTOs"></param>
        /// <param name="langId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SysTerm/Compare")]
        [ResponseType(typeof(List<SysTermDTO>))]
        public IHttpActionResult CompareSysterms(Guid companyApiKey, Guid connectApiKey, string token, List<SysTermDTO> sysTermDTOs, int? langId = null)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            TermManager termManager = new TermManager(apiManager.GetParameterObject() );
            List<SysTermDTO> oldsysTermDTOs = termManager.GetSysTermDTOsFromDatabase();
            List<SysTermDTO> changedItems = termManager.CompareSystermDTOs(oldsysTermDTOs, sysTermDTOs);
            return Content(HttpStatusCode.OK, changedItems);
        }

        /// <summary>
        /// Update SystermGroups, make sure PostChange has value
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysTermDTOs"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SysTerm/Update")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult UpdateSysTerms(Guid companyApiKey, Guid connectApiKey, string token, List<SysTermDTO> sysTermDTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            TermManager termManager = new TermManager(apiManager.GetParameterObject() );
            ActionResult result = termManager.UpdateSysTerms(sysTermDTOs);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #endregion

        #endregion
    }
}