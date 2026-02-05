using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/RecalculateTime")]
    public class RecalculateTimeController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public RecalculateTimeController( TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region RecalculateTimeHead

        [HttpGet]
        [Route("RecalculateTimeHead")]
        public IHttpActionResult GetRecalculateTimeHeads(SoeRecalculateTimeHeadAction recalculateAction, bool loadRecords, bool showHistory, bool setExtensionNames, string dateFromString, string dateToString = null, int? limitNbrOfHeads = null)
        {
            var limit = (limitNbrOfHeads.HasValue && limitNbrOfHeads.Value > 0) ? limitNbrOfHeads : null;
            return Content(HttpStatusCode.OK, tsm.GetRecalculateTimeHeads(base.ActorCompanyId, base.UserId, base.RoleId, recalculateAction, loadRecords, showHistory, setExtensionNames, base.BuildDateTimeFromString(dateFromString, true), base.BuildDateTimeFromString(dateToString, true), limit).ToDTOs());
        }

        [HttpGet]
        [Route("RecalculateTimeHead/{recalculateTimeHeadId:int}/{loadRecords:bool}/{setExtensionNames:bool}")]
        public IHttpActionResult GetRecalculateTimeHead(int recalculateTimeHeadId, bool loadRecords, bool setExtensionNames)
        {
            return Content(HttpStatusCode.OK, tsm.GetRecalculateTimeHead(base.ActorCompanyId, recalculateTimeHeadId, loadRecords, setExtensionNames).ToDTO());
        }

        [HttpGet]
        [Route("RecalculateTimeHeadId/{key}")]
        public IHttpActionResult GetRecalculateTimeHeadId(string key)
        {
            Guid recalculateGuid = Guid.Parse(key);
            return Content(HttpStatusCode.OK, tsm.GetRecalculateTimeHeadId(base.ActorCompanyId, base.UserId, recalculateGuid));
        }

        [HttpPost]
        [Route("RecalculateTimeHead/SetToProcessed")]
        public IHttpActionResult SetRecalculateTimeHeadToProcessed(IntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SetRecalculateTimeHeadToProcessed(base.ActorCompanyId, base.UserId, base.ParameterObject.IsSupportLoggedIn, model.Id));
        }

        [HttpDelete]
        [Route("RecalculateTimeHead/{recalculateTimeHeadId:int}")]
        public IHttpActionResult CancelRecalculateTimeHead(int recalculateTimeHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.CancelRecalculateTimeHead(base.ActorCompanyId, base.UserId, base.ParameterObject.IsSupportLoggedIn, recalculateTimeHeadId));
        }

        #endregion

        #region RecalculateTimeRecord

        [HttpDelete]
        [Route("RecalculateTimeRecord/{recalculateTimeRecordId:int}")]
        public IHttpActionResult CancelRecalculateTimeRecord(int recalculateTimeRecordId)
        {
            return Content(HttpStatusCode.OK, tsm.CancelRecalculateTimeRecord(base.ActorCompanyId, base.UserId, base.ParameterObject.IsSupportLoggedIn, recalculateTimeRecordId));
        }

        #endregion
    }
}