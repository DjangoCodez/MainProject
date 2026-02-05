using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Project/Project")]
    public class ProjectController : WebApiExternalBase
    {

        #region Constructor
        public ProjectController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get projects. No selection will retrieve all project.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="search"></param>
        /// <param name="projectNumberFrom"></param>
        /// <param name="projectNumberTo"></param>
        /// <param name="onlyActive"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Projects/")]
        [ResponseType(typeof(List<ProjectIODTO>))]
        public IHttpActionResult GetProjects(Guid companyApiKey, Guid connectApiKey, string token, string search, string projectNumberFrom = "", string projectNumberTo = "", bool onlyActive = true)
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
            var projectIODTOs = importExportManager.GetProjectIODTOs(apiManager.ActorCompanyId, search, projectNumberFrom, projectNumberTo, onlyActive);

            return Content(HttpStatusCode.OK, projectIODTOs);
        }

        /// <summary>
        /// Save projects from list.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="projectIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Projects/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveProjects(Guid companyApiKey, Guid connectApiKey, string token, List<ProjectIODTO> projectIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var projectIOItem = new ProjectIOItem
            {
                Projects = projectIODTOs
            };

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var result = importExportManager.ImportProjectIO(projectIOItem, TermGroup_IOImportHeadType.Project, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, true, 0);

            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save Project
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="projectIODTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Project/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveProject(Guid companyApiKey, Guid connectApiKey, string token, ProjectIODTO projectIODTO)
        {

            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var projectIOItem = new ProjectIOItem
            {
                Projects = new List<ProjectIODTO>() { projectIODTO }
            };

            var importExportManager = new ImportExportManager( apiManager.GetParameterObject() );
            var result = importExportManager.ImportProjectIO(projectIOItem, TermGroup_IOImportHeadType.Project, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, true, 0);

            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Get TimeCodeTransactions. Empty or null EmployeeNrs list will return transactions for allEmployees.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeNrs"></param>
        /// <param name="projectNumberFrom"></param>
        /// <param name="projectNumberTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TimeCodeTransactions/")]
        [ResponseType(typeof(List<TimeCodeTransactionIODTO>))]
        public IHttpActionResult GetTimeCodeTransactions(Guid companyApiKey, Guid connectApiKey, string token, DateTime dateFrom, DateTime dateTo, List<string> employeeNrs, string projectNumberFrom = "", string projectNumberTo = "")
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
            var employeeManager = new EmployeeManager(apiManager.GetParameterObject());
            List<TimeCodeTransactionIODTO> timeCodeTransactionIODTOs = importExportManager.GetTimeCodeTransactions(apiManager.ActorCompanyId, dateFrom, dateTo, employeeManager.GetAllEmployeeIdsByEmployeeNr(apiManager.ActorCompanyId, employeeNrs));

            return Content(HttpStatusCode.OK, timeCodeTransactionIODTOs);
        }

        /// <summary>
        /// Get TimeCodeTransactions2. Empty or null EmployeeNrs list will return transactions for allEmployees. Employee separted with comma
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeNrsString"></param>
        /// <param name="projectNumberFrom"></param>
        /// <param name="projectNumberTo"></param>
        [HttpGet]
        [Route("TimeCodeTransactions2/")]
        [ResponseType(typeof(List<TimeCodeTransactionIODTO>))]
        public IHttpActionResult GetTimeCodeTransactions2(Guid companyApiKey, Guid connectApiKey, string token, DateTime dateFrom, DateTime dateTo, string employeeNrsString, string projectNumberFrom = "", string projectNumberTo = "", bool includeInActive=false)
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
            var employeeManager = new EmployeeManager(apiManager.GetParameterObject() );

            List<int> employeeNbrs = null;
            if (!string.IsNullOrEmpty(employeeNrsString) && employeeNrsString.ToLower() == "null")
                employeeNrsString = string.Empty;

            if (!string.IsNullOrEmpty(employeeNrsString))
            {
                employeeNbrs = employeeManager.GetAllEmployeeIdsByEmployeeNr(apiManager.ActorCompanyId, employeeNrsString.Split(',').ToList());
                if (employeeNbrs.IsNullOrEmpty())
                {
                    return Content(HttpStatusCode.OK, new List<TimeCodeTransactionIODTO>());
                }
            }
            var timeCodeTransactionIODTOs = importExportManager.GetTimeCodeTransactions(apiManager.ActorCompanyId, dateFrom, dateTo, employeeNbrs, includeInActive);
            return Content(HttpStatusCode.OK, timeCodeTransactionIODTOs);
        }

        #endregion

    }
}