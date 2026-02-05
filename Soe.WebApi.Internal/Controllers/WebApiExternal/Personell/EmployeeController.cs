using Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Employee
{
    [RoutePrefix("Personell/Employee")]
    public class EmployeeController : WebApiExternalBase
    {
        #region Constructor

        public EmployeeController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #endregion

        #region Methods

        #region Employee

        /// <summary>
        /// Save new Employees or update existing
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employeeIODTOs"></param>
        /// <param name="discardLicenseCheckes"></param>
        /// <param name="doNotModifyWithEmpty"></param>
        /// <param name="doNotModifyRolesWithEmpty"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Employees/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEmployees(Guid companyApiKey, Guid connectApiKey, string token, List<EmployeeIODTO> employeeIODTOs, bool discardLicenseCheckes = false, bool doNotModifyWithEmpty = false, bool doNotModifyRolesWithEmpty = false)
        {
            doNotModifyWithEmpty = true;

            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
                return Content(HttpStatusCode.Unauthorized, validatationResult);

            #endregion

            EmployeeIOItem employeeIOItem = new EmployeeIOItem(employeeIODTOs, TermGroup_IOSource.XE, TermGroup_IOType.WebService, TermGroup_IOImportHeadType.Employee, apiManager.ActorCompanyId);

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            ActionResult result = importExportManager.ImportEmployees(employeeIOItem.EmployeeIOs, apiManager.ActorCompanyId, discardLicenseCheckes, doNotModifyWithEmpty, doNotModifyRolesWithEmpty);

            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Get employee based on employee number
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employeeNr"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Employee/")]
        [ResponseType(typeof(List<EmployeeIODTO>))]
        public IHttpActionResult GetEmployee(Guid companyApiKey, Guid connectApiKey, string token, string employeeNr)
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
            var employeeIO = importExportManager.ExportEmployee(TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, employeeNr);
            return Content(HttpStatusCode.OK, employeeIO.GetValidEmployeeIOs().FirstOrDefault());
        }

        /// <summary>
        /// Get all employees. Will only retrieve employees connected to the attestrole of token user.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="changedAfter"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Employees/")]
        [ResponseType(typeof(List<EmployeeIODTO>))]
        public IHttpActionResult GetEmployees(Guid companyApiKey, Guid connectApiKey, string token, DateTime? changedAfter = null, bool includeInactive = false)
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
            var employeeIO = importExportManager.ExportEmployees(TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, changedAfter, apiManager.ActorCompanyId, includeInactive);
            return Content(HttpStatusCode.OK, employeeIO.GetValidEmployeeIOs());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employeeNr"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExistingEmployees/")]
        [ResponseType(typeof(List<ExistingEmployeeIO>))]
        public IHttpActionResult GetExistingEmployees(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation
            if (base.ValidateToken(companyApiKey, connectApiKey, token, out string validatationResult, out ParameterObject parameterObject))
                return Content(HttpStatusCode.Unauthorized, validatationResult);

            #endregion

            var importExportManager = new ImportExportManager(parameterObject);
            var existingEmployeeIOs = importExportManager.GetExistingEmployeeIOs(parameterObject.ActorCompanyId);
            return Content(HttpStatusCode.OK, existingEmployeeIOs);
        }

        #region EmployeeChange

        /// <summary>
        /// Add one or more employeeChangeIODTOs. Set null or empty on value on EmployeeChangeRowIODTO if it should be deleted. 
        /// Deprecated. Use Employees/ImportChanges instead. That endpoint contains different result object wich contains more detailed information.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employeeChangeIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Employees/Changes")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEmployee(Guid companyApiKey, Guid connectApiKey, string token, List<EmployeeChangeIODTO> employeeChangeIODTOs)
        {
            ApiMessageDTO dto = new ApiMessageDTO(companyApiKey, connectApiKey, SoeEntityType.Employee, TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.API);

            var am = new ApiManager(null);
            if (am.ValidateToken(companyApiKey, connectApiKey, token, out string validatationResult))
            {
                ActionResult result = am.ImportEmployeeChanges(dto, employeeChangeIODTOs, out _, isLegacyMode: true);
                return Content(HttpStatusCode.OK, result);
            }
            else
            {
                am.ApiMessageFailedValidation(dto, employeeChangeIODTOs);
                return Content(HttpStatusCode.OK, new ActionResult(validatationResult));
            }
        }

        [HttpPost]
        [Route("Employees/ImportChanges")]
        [ResponseType(typeof(EmployeeChangeResult))]
        public IHttpActionResult ImportEmployeeChanges(Guid companyApiKey, Guid connectApiKey, string token, List<EmployeeChangeIODTO> employeeChanges)
        {
            ApiMessageDTO dto = new ApiMessageDTO(companyApiKey, connectApiKey, SoeEntityType.Employee, TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.API);

            var am = new ApiManager(null);
            if (am.ValidateToken(companyApiKey, connectApiKey, token, out string validatationResult))
            {
                return Content(HttpStatusCode.OK, am.ImportEmployeeChangesExtensive(dto, employeeChanges, out ActionResult _));
            }
            else
            {
                am.ApiMessageFailedValidation(dto, employeeChanges);
                return Content(HttpStatusCode.OK, new EmployeeChangeResult(validatationResult));
            }
        }

        #endregion

        #endregion

        #region Employment

        /// <summary>
        /// Import Employments
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employmentIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Employment/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEmployments(Guid companyApiKey, Guid connectApiKey, string token, List<EmploymentIODTO> employmentIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            EmploymentIOItem employmentIOItem = new EmploymentIOItem();

            employmentIOItem.employmentIOs = new List<EmploymentIODTO>();
            employmentIOItem.employmentIOs.AddRange(employmentIODTOs);
            var result = importExportManager.ImportEmploymentIO(employmentIOItem, TermGroup_IOImportHeadType.Employment, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region FixedPayrollRows

        /// <summary>
        /// Import FixedPayrollRows
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="fixedPayrollRowIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("FixedPayrollRow/")]
        public IHttpActionResult SaveFixedPayrollRows(Guid companyApiKey, Guid connectApiKey, string token, List<FixedPayrollRowIODTO> fixedPayrollRowIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            FixedPayrollRowIOItem fixedPayrollRowIOItem = new FixedPayrollRowIOItem();

            fixedPayrollRowIOItem.fixedPayrollRowIOs = new List<FixedPayrollRowIODTO>();
            fixedPayrollRowIOItem.fixedPayrollRowIOs.AddRange(fixedPayrollRowIODTOs);
            ActionResult result = importExportManager.ImportFixedPayrollRowIO(fixedPayrollRowIOItem, TermGroup_IOImportHeadType.FixedPayrollRow, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }


        #endregion

        #region EmployeeVacationSE

        /// <summary>
        /// Import EmployeeVacationDays (Swedish)
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employeeVacationSEIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("EmployeeVacationSE/")]
        public IHttpActionResult SaveemployeeVacationSEs(Guid companyApiKey, Guid connectApiKey, string token, List<EmployeeVacationSEIODTO> employeeVacationSEIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            EmployeeVacationSEIOItem employeeVacationSEIOItem = new EmployeeVacationSEIOItem();

            employeeVacationSEIOItem.employeeVacationSEIOs = new List<EmployeeVacationSEIODTO>();
            employeeVacationSEIOItem.employeeVacationSEIOs.AddRange(employeeVacationSEIODTOs);

            var result = importExportManager.ImportEmployeeVacationSEIO(employeeVacationSEIOItem, TermGroup_IOImportHeadType.VactionDays, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }


        #endregion

        #region AccountHierarchyTree

        /// <summary>
        /// Get Account Hierarchy tree
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AccountHierarchyTree/")]
        [ResponseType(typeof(AccountHierarchyTreeDTO))]
        public IHttpActionResult GetAccountHierarchyTree(Guid companyApiKey, Guid connectApiKey, string token)
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
            var dto = importExportManager.GetAccountHierarchyTree(apiManager.ActorCompanyId);

            return Content(HttpStatusCode.OK, dto);
        }

        #endregion

        #region EmployeeOrganisationInformation

        [HttpGet]
        [Route("EmployeeOrganisationInformations/")]
        [ResponseType(typeof(List<EmployeeOrganisationInformation>))]
        public IHttpActionResult GetEmployeeOrganisationInformations(Guid companyApiKey, Guid connectApiKey, string token, DateTime fromDate, DateTime toDate)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var employeeManager = new EmployeeManager(apiManager.GetParameterObject());
            var dto = employeeManager.GetEmployeeOrganisationInformations(apiManager.ActorCompanyId, fromDate, toDate);

            return Content(HttpStatusCode.OK, dto);
        }

        #endregion

        #region EmployeeDayInformation
        [HttpPost]
        [Route("EmployeeDayInformations/")]
        [ResponseType(typeof(ActionResultDto))]
        public IHttpActionResult SaveEmployeeDayInformations(Guid companyApiKey, Guid connectApiKey, string token, List<EmployeeDayInformation> employeeDayInformations)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var employeeManager = new EmployeeManager(apiManager.GetParameterObject());
            var result = new ActionResultDto() { ErrorMessage = "Not implemented yet" }; // employeeManager.SaveEmployeeDayInformations(apiManager.ActorCompanyId, employeeDayInformations);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region EmloyeeInformation

        [HttpPost]
        [Route("EmployeeInformations/")]
        [ResponseType(typeof(List<EmployeeInformation>))]
        public IHttpActionResult SaveEmployeeInformations(Guid companyApiKey, Guid connectApiKey, string token, FetchEmployeeInformation fetchEmployeeInformationModel)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            var param = apiManager.GetParameterObject();
            var featureManager = new FeatureManager(param);
            var requiredFeature = Feature.Time_Employee_Employees_Edit_OtherEmployees;
            if (!featureManager.HasRolePermission(requiredFeature, Permission.Readonly, param.RoleId, param.ActorCompanyId))
            {
                return Content(HttpStatusCode.Forbidden, $"The user does not have required permission [{(int)requiredFeature}].");
            }

            #endregion
            var parameterObject = apiManager.GetParameterObject();
            var importexportManager = new ImportExportManager(parameterObject);
            var result = importexportManager.GetEmployeeInformations(parameterObject.ActorCompanyId, fetchEmployeeInformationModel);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region Absence

        [HttpPost]
        [Route("Absence/")]
        [ResponseType(typeof(LongTermAbsenceOutput))]
        public IHttpActionResult SaveLongTermAbsence(Guid companyApiKey, Guid connectApiKey, string token, LongTermAbsenceInput longTermAbsenceInput)
        {
            #region Validation
            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }
            #endregion
            var parameterObject = apiManager.GetParameterObject();
            var importexportManager = new ImportExportManager(parameterObject);
            var result = importexportManager.GetLongTermAbsence(longTermAbsenceInput, parameterObject.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }


        #endregion

        #endregion
    }

}