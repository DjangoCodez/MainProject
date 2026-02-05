
using Newtonsoft.Json;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Net;
using System.Web.Http;


namespace Soe.Api.Internal.Controllers.WebApiExternal.Employee
{
    [RoutePrefix("Personell/Report")]
    public class PersonellMatrixController : WebApiExternalBase
    {

        #region Constructor

        public PersonellMatrixController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        #region MatrixReports
        /// <summary>
        /// Get different types of output and the fields/columns available for each report
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MatrixReports/Types")]
        public IHttpActionResult GetApiMatrixReports(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion
            try
            {
                var reportManager = new ReportManager(apiManager.GetParameterObject());
                var reports = reportManager.GetApiMatrixReports((int)SoeModule.Time);
                return Content(HttpStatusCode.OK, reports);
            }
            catch (Exception ex)
            {
                LogCollector.LogError($"GetApiMatrixReports. companyApiKey:{companyApiKey} connectApiKey{connectApiKey} exception:{ex}");
            }

            return Content(HttpStatusCode.InternalServerError, "Internal Server Error 46545132");
        }

        /// <summary>
        /// Post information about selection and recieve information as a result.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="apiMatrixDataSelection"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MatrixReports/Output")]
        public IHttpActionResult GetApiReportOutput(Guid companyApiKey, Guid connectApiKey, string token, ApiMatrixDataSelection apiMatrixDataSelection)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion
            try
            {
                LogCollector.LogInfo($"GetApiReportOutput. companyApiKey:{companyApiKey} connectApiKey{connectApiKey}");

                var param = apiManager.GetParameterObject();
                var reportDataManager = new ReportDataManager(param);
                var trackGuid = Guid.NewGuid();
                var output = reportDataManager.CreateApiMatrixDataResult(apiMatrixDataSelection, param.ActorCompanyId, trackGuid);

                if (output == null)
                {
                    LogCollector.LogError($"GetApiReportOutput. companyApiKey:{companyApiKey} connectApiKey{connectApiKey} trackGuid:{trackGuid} output is null");
                    return Content(HttpStatusCode.InternalServerError, $"Internal Server Error 223477: TrackGuid {trackGuid} output is null");
                }

                LogCollector.LogInfo($"GetApiReportOutput. companyApiKey:{companyApiKey} connectApiKey{connectApiKey} trackGuid:{trackGuid} done output: {JsonConvert.SerializeObject(output)}");
                if (string.IsNullOrEmpty(output.ResultMessage))
                {
                    LogCollector.LogInfo($"GetApiReportOutput. companyApiKey:{companyApiKey} connectApiKey{connectApiKey} trackGuid:{trackGuid} output does not contain ResultMessage");
                    return Content(HttpStatusCode.OK, output);
                }
                else
                    LogCollector.LogError($"GetApiReportOutput. companyApiKey:{companyApiKey} connectApiKey{connectApiKey} trackGuid:{trackGuid} output is null {output == null} message:{output.ResultMessage}");

                return Content(HttpStatusCode.InternalServerError, $"Internal Server Error 2234: TrackGuid {trackGuid} message: {output.ResultMessage}");
            }
            catch (Exception ex)
            {
                LogCollector.LogError($"GetApiReportOutput. companyApiKey:{companyApiKey} connectApiKey{connectApiKey} exception:{ex}");
            }

            return Content(HttpStatusCode.InternalServerError, "Internal Server Error 46577132");
        }

        #endregion

        #endregion
    }
}
