using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimeStamp")]
    public class TimeStampV2Controller : SoeApiController
    {
        #region Variables

        private readonly TimeStampManager tsm;
        #endregion

        #region Constructor

        public TimeStampV2Controller(TimeStampManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region TimeStamp
        [HttpGet]
        [Route("TimeStamp/{timeStampEntryId:int}")]
        public IHttpActionResult GetTimeStamp(int timeStampEntryId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeStampEntry(timeStampEntryId).ToDTO());
        }
        [HttpGet]
        [Route("TimeStamp/UserAgentClientInfo/{timeStampEntryId:int}")]
        public IHttpActionResult GetTimeStampEntryUserAgentClientInfo(int timeStampEntryId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeStampEntryUserAgentClientInfo(base.ActorCompanyId, timeStampEntryId));
        }

        [HttpPost]
        [Route("Search")]
        public IHttpActionResult SearchTimeStamps(SearchTimeStampModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeStampEntriesDTO(model.dateFrom, model.dateTo, model.EmployeeIds, base.ActorCompanyId));
        }
        [HttpPost]
        [Route("TimeStamp/Save/")]
        public IHttpActionResult SaveTimeStamps(List<TimeStampEntryDTO> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveAdjustedTimeStampEntries(items));
        }
        #endregion

    }
}