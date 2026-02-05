using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Personell
{
    [RoutePrefix("Personell/Time")]
    public class TimeController : WebApiExternalBase
    {

        #region Constructor

        public TimeController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        #region TimeRegistrationInformation

        /// <summary>
        /// Get TimeRegistrationInformations
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="employeeNrsString"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TimeRegistrationInformation/")]
        [ResponseType(typeof(List<TimeRegistrationInformation>))]
        public IHttpActionResult GetTimeRegistrationInformations(Guid companyApiKey, Guid connectApiKey, string token, string employeeNrsString, DateTime fromDate, DateTime toDate)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var payrollManager = new PayrollManager(apiManager.GetParameterObject());
            var employeeNumbers = employeeNrsString == null || employeeNrsString == "NULL" ? null : employeeNrsString.Split(',').ToList();
            var timeRegistrationInformations = payrollManager.GetTimeRegistrationInformation(apiManager.ActorCompanyId, employeeNumbers, fromDate, toDate);

            return Content(HttpStatusCode.OK, timeRegistrationInformations);
        }

        /// <summary>
        /// Save TimeregistrationInformation batch
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="timeRegistrationInformations"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TimeRegistrationInformation/")]
        public IHttpActionResult SaveTimeRegistrationInformation(Guid companyApiKey, Guid connectApiKey, string token, List<TimeRegistrationInformation> timeRegistrationInformations)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var payrollManager = new PayrollManager(apiManager.GetParameterObject());
            var result = payrollManager.SaveTimeRegistrationInformation(timeRegistrationInformations, apiManager.ActorCompanyId, true);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region TimeStampEntry


        /// <summary>
        /// Get TimeStampEntries
        /// </summary>

        [HttpGet]
        [Route("TimeStampEntry/Get")]
        [ResponseType(typeof(List<TimeStampEntryIODTO>))]
        public IHttpActionResult GetTimeStampEntries(Guid companyApiKey, Guid connectApiKey, string token, DateTime dateFrom, DateTime dateTo, string employeeNrsString, string code = "")
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
            var employeeNumbers = employeeNrsString == null ? new List<string>() : employeeNrsString.Split(',').ToList();
            var timeScheduleBlockIODTOs = importExportManager.GetTimeStampEntryIODTOs(apiManager.ActorCompanyId, dateFrom, dateTo, employeeNumbers);

            return Content(HttpStatusCode.OK, timeScheduleBlockIODTOs);
        }

        /// <summary>
        /// Save TimeStampEntries
        /// </summary>
        [HttpPost]
        [Route("TimeStampEntry/Save")]
        public IHttpActionResult SaveTimeStampEntries(Guid companyApiKey, Guid connectApiKey, string token, List<TimeStampEntryIODTO> timeStampEntryIODTOs)
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
            var result = importExportManager.SaveTimeStampEntries(apiManager.ActorCompanyId, timeStampEntryIODTOs);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region TimeBalance

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
        [Route("TimeBalance/")]
        [ResponseType(typeof(List<TimeBalanceIODTO>))]
        public IHttpActionResult GetTimeBalance(Guid companyApiKey, Guid connectApiKey, string token, DateTime date, string employeeNrsString, string code = "")
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
            var employeeNumbers = employeeNrsString == null ? new List<string>() : employeeNrsString.Split(',').ToList();
            var timeScheduleBlockIODTOs = importExportManager.GetTimeBalance(apiManager.ActorCompanyId, date, employeeNumbers, null, code);

            return Content(HttpStatusCode.OK, new List<TimeBalanceIODTO>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="timeBalanceIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TimeBalance/")]
        public IHttpActionResult SaveTimeBalances(Guid companyApiKey, Guid connectApiKey, string token, List<TimeBalanceIODTO> timeBalanceIODTOs)
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
            TimeBalanceIOItem item = new TimeBalanceIOItem();

            item.timeBalanceIOs = timeBalanceIODTOs;
            ActionResult result = new ActionResult();
            result = importExportManager.ImportTimeBalanceIO(item, TermGroup_IOImportHeadType.TimeBalance, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion


        #region TimeCodeTransactionSimple


        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="timeCodeTransactionSimpleIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TimeCodeTransactionSimple/")]
        public IHttpActionResult SaveTimeCodeTransactionSimples(Guid companyApiKey, Guid connectApiKey, string token, List<TimeCodeTransactionSimpleIODTO> timeCodeTransactionSimpleIODTOs)
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
            TimeCodeTransactionSimpleIOItem item = new TimeCodeTransactionSimpleIOItem();

            item.timeCodeTransactionSimpleIOs = timeCodeTransactionSimpleIODTOs;
            ActionResult result = new ActionResult();
            result = importExportManager.ImportTimeCodeTransactionSimpleIO(item, TermGroup_IOImportHeadType.TimeCodeTransactionSimple, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region CompanyTimeInformation
        [HttpPost]
        [Route("CompanyTimeInformationValidation/")]
        public IHttpActionResult GetCompanyTimeInformationValidation(Guid companyApiKey, Guid connectApiKey, CompanyTimeInformationRequestKey companyTimeInformationRequest)
        {
            ImportExportManager importExportManager = new ImportExportManager(null);
            SettingManager settingManager = new SettingManager(null);
            var company = importExportManager.GetCompany(companyTimeInformationRequest, out CompanyTimeInformationDTO dto);
            return Content(HttpStatusCode.OK, new ActionResult() { StringValue = (company != null ? settingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, 0, company.ActorCompanyId, 0) : "") });
        }
        [HttpPost]
        [Route("CompanyTimeInformation/")]
        public IHttpActionResult GetCompanyTimeInformation(Guid companyApiKey, Guid connectApiKey, CompanyTimeInformationRequest companyTimeInformationRequest)
        {
            #region Validation

            var companyManager = new CompanyManager(null);
            if (!companyManager.GetActorCompanyIdFromApiKey(companyApiKey.ToString()).HasValue)
                return Content(HttpStatusCode.OK, "Company not found. ApiKey do not match requirements.");

            if (connectApiKey != Guid.Parse("4536b84f-b1cd-4a75-9234-6d9a7928c9e1"))
                return Content(HttpStatusCode.OK, "Invalid connectApiKey for this request");

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(null);
            var result = importExportManager.GetCompanyTimeInformation(companyTimeInformationRequest);
            return Content(HttpStatusCode.OK, result);
        }
        #endregion

        #region MultiCompanyEndpoint
        //MultiCompanyEndpoint
        [HttpPost]
        [Route("MultiCompanyEndpoint/")]
        public IHttpActionResult GetMultiCompanyEndpoint(CompanyBatchRequest request)
        {
            //validate keys TODO

            var result = new CompanyBatchAccountAggregatedTime();
            return Content(HttpStatusCode.OK, result);
        }
        #endregion
    }


#endregion
}