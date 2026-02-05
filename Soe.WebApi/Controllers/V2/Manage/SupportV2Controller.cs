using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Support")]
    [SupportUserAuthorize]
    public class SupportV2Controller : SoeApiController
    {
        #region Variables

        private readonly SysLogManager slm;

        #endregion

        #region Constructor

        public SupportV2Controller(SysLogManager slm)
        {
            this.slm = slm;
        }

        #endregion

        #region SysLog

        [HttpGet]
        [Route("SysLog/LogType/{logType:int}/{showUnique:bool}")]
        public IHttpActionResult GetSysLogsGrid(int logType, bool showUnique)
        {
            return Content(HttpStatusCode.OK, slm.GetSysLogs((SoeLogType)logType, showUnique).ToGridDTOs());
        }

        [HttpGet]
        [Route("SysLog/{sysLogId:int}")]
        public IHttpActionResult GetSysLog(int sysLogId)
        {
            return Content(HttpStatusCode.OK, slm.GetSysLog(sysLogId).ToDTO());
        }

        [HttpPost]
        [Route("SysLog/Search/")]
        public IHttpActionResult SearchSysLogs(SearchSysLogsDTO dto)
        {
            return Content(HttpStatusCode.OK, slm.SearchLogEntries(dto).ToGridDTOs());
        }

        #endregion
    }
}