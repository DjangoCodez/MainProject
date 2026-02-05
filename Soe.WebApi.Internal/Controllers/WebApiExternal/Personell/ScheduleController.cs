using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Employee
{
    [RoutePrefix("Personell/Schedule")]
    public class ScheduleController : WebApiExternalBase
    {

        #region Constructor

        public ScheduleController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        #region Schedule

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeNumbers"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ActiveSchedules/")]
        [ResponseType(typeof(List<TimeScheduleBlockIODTO>))]
        public IHttpActionResult GetActiveSchedules(Guid companyApiKey, Guid connectApiKey, string token, DateTime? dateFrom, DateTime? dateTo, [FromUri] List<string> employeeNumbers)
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
            var timeScheduleBlockIODTOs = importExportManager.GetTimeScheduleBlockIODTOs(apiManager.ActorCompanyId, null, dateFrom, dateTo, employeeNumbers);

            return Content(HttpStatusCode.OK, timeScheduleBlockIODTOs);
        }

        /// <summary>
        /// Get schedule and transaction information
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeNrsString"></param>
        /// <param name="addAmounts"></param>
        /// <param name="loadSchedule"></param>
        /// <param name="loadPayroll"></param>
        /// <param name="includeEmployeeName"></param>
        /// <param name="includeEmployeeNr"></param>
        /// <param name="includeEmployeeExternalCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TimeScheduleInfo/")]
        [ResponseType(typeof(TimeScheduleInfoIODTO))]
        public IHttpActionResult GetTimeScheduleInfos(Guid companyApiKey, Guid connectApiKey, string token, DateTime dateFrom, DateTime dateTo, string employeeNrsString, bool addAmounts, bool loadSchedule, bool loadPayroll, bool includeEmployeeName = true, bool includeEmployeeNr = true, bool includeEmployeeExternalCode = true)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (!string.IsNullOrEmpty(employeeNrsString) && employeeNrsString.ToLower() == "null")
                employeeNrsString = string.Empty;

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var employeeNumbers = employeeNrsString?.Trim().Split(',').ToList() ?? new List<string>();
            var dto = importExportManager.GetTimeScheduleInfo(apiManager.ActorCompanyId, dateFrom, dateTo, employeeNumbers, addAmounts, loadSchedule, loadPayroll, includeEmployeeName, includeEmployeeNr, includeEmployeeExternalCode);

            return Content(HttpStatusCode.OK, dto);
        }

        [HttpPost]
        [Route("EmployeeActiveSchedules/")]
        public IHttpActionResult SaveEmployeeActiveSchedules(Guid companyApiKey, Guid connectApiKey, string token, List<EmployeeActiveScheduleIO> timeScheduleBlockIODTOs)
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
            ActionResult result = importExportManager.SaveEmployeeActiveScheduleIOs(apiManager.ActorCompanyId, timeScheduleBlockIODTOs);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="timeScheduleBlockIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ActiveSchedules/")]
        public IHttpActionResult SaveActiveSchedules(Guid companyApiKey, Guid connectApiKey, string token, List<TimeScheduleBlockIODTO> timeScheduleBlockIODTOs)
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
            TimeScheduleBlockIOItem timeScheduleBlockIOItem = new TimeScheduleBlockIOItem();

            timeScheduleBlockIOItem.TimeScheduleBlockIODTOs = new List<TimeScheduleBlockIODTO>();
            timeScheduleBlockIOItem.TimeScheduleBlockIODTOs.AddRange(timeScheduleBlockIODTOs);
            ActionResult result = importExportManager.ImportTimeScheduleBlockIO(timeScheduleBlockIOItem, TermGroup_IOImportHeadType.Schedule, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region FrequencyData

        /// <summary>
        /// Send information about sales and frequency data in order to calculate need.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="staffingNeedsFrequencys"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("StaffingNeedsFrequencys/")]
        public IHttpActionResult SaveStaffingNeedsFrequencys(Guid companyApiKey, Guid connectApiKey, string token, List<StaffingNeedsFrequencyIODTO> staffingNeedsFrequencys)
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

            foreach (var item in staffingNeedsFrequencys)
                item.ActorCompanyId = apiManager.ActorCompanyId;

            StaffingNeedsFrequencyIOItem staffingNeedsFrequencyIOItem = new StaffingNeedsFrequencyIOItem();
            staffingNeedsFrequencyIOItem.frequencies = staffingNeedsFrequencys;

            ActionResult result = importExportManager.ImportStaffingNeedsFrequencyIO(staffingNeedsFrequencyIOItem, TermGroup_IOImportHeadType.StaffingNeedsFrequency, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, true);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region ShiftType

        /// <summary>
        /// Get ShiftTypes
        /// </summary>
        [HttpGet]
        [Route("ShiftTypes/")]
        public IHttpActionResult GetShiftTypes(Guid companyApiKey, Guid connectApiKey, string token)
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
            List<ShiftTypeIODTO> shiftTypeIODTOs = importExportManager.GetShiftTypeIODTOs(apiManager.ActorCompanyId);

            return Content(HttpStatusCode.OK, shiftTypeIODTOs);
        }

        /// <summary>
        /// Save ShiftTypes
        /// </summary>
        [HttpPost]
        [Route("ShiftTypes/")]
        public IHttpActionResult SaveShiftTypes(Guid companyApiKey, Guid connectApiKey, string token, List<ShiftTypeIODTO> shiftTypeIODTOs)
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
            ActionResult result = importExportManager.SaveShiftTypeIODTOs(apiManager.ActorCompanyId, shiftTypeIODTOs);

            return Content(HttpStatusCode.OK, result);
        }
        #endregion

        #region LeaveRequest
        /// <summary>
        /// Get leave request information
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeNrsString"></param>
        /// <param name="employeeRequestIds"></param>
        /// <param name="createdOrModifiedAfter"></param>
        /// <param name="createdAfter"></param>
        /// <param name="modifiedAfter"></param>
        /// <param name="includeAffectedShifts"></param>
        /// <param name="includeAffectedPeriods"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("leaveRequests/")]
        [ResponseType(typeof(List<LeaveRequest>))]
        public IHttpActionResult GetLeaveRequests(
            Guid companyApiKey, 
            Guid connectApiKey, 
            string token, 
            DateTime dateFrom, 
            DateTime dateTo, 
            string employeeNrsString,
            string employeeRequestIds = null,
            DateTime? createdOrModifiedAfter = null,
            DateTime? createdAfter = null,
            DateTime? modifiedAfter = null,
            bool includeAffectedShifts = false,
            bool includeAffectedPeriods = false
            )
        {
            #region Validation

            var apiManager = new ApiManager(null);
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out string validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            var parameterObject = apiManager.GetParameterObject();
            var featureManager = new FeatureManager(parameterObject);
            var requiredFeature = Feature.Time_Schedule_AbsenceRequests;
            if (!featureManager.HasRolePermission(requiredFeature, Permission.Readonly, parameterObject.RoleId, parameterObject.ActorCompanyId))
            {
                return Content(HttpStatusCode.Forbidden, $"The user does not have required permission [{(int)requiredFeature}].");
            }

            #endregion

            if (!string.IsNullOrEmpty(employeeNrsString) && employeeNrsString.ToLower() == "null")
                employeeNrsString = null;
            if (!string.IsNullOrEmpty(employeeRequestIds) && employeeRequestIds.ToLower() == "null")
                employeeRequestIds = null;

            var employeeNumbers = employeeNrsString?.Trim().Split(',').ToList();
            
            List<int> employeeRequestIdList = null;
            var valueArray = employeeRequestIds?.Split(',');
            if (valueArray != null && valueArray.Length > 0)
            {
                employeeRequestIdList = new List<int>();
                foreach (var stringValue in valueArray)
                {
                    if (int.TryParse(stringValue, out int integerValue))
                        employeeRequestIdList.Add(integerValue);
                    else
                        return Content(HttpStatusCode.BadRequest, $"An invalid id found in employeeRequestIds. '{stringValue}' is not a valid integer.");
                }
            }

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var leaveRequests = importExportManager.GetLeaveRequests(apiManager.ActorCompanyId, dateFrom, dateTo, employeeNumbers, employeeRequestIdList, createdOrModifiedAfter, createdAfter, modifiedAfter, includeAffectedShifts, includeAffectedPeriods);

            return Content(HttpStatusCode.OK, leaveRequests);
        }
        #endregion

        #endregion
    }
}