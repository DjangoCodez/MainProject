using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/TrackChanges")]
    public class TrackChangesController : SoeApiController
    {
        #region Variables
        private readonly TrackChangesManager tcm;
        #endregion

        #region Constructor
        public TrackChangesController(TrackChangesManager tcm)
        {
            this.tcm = tcm;
        }
        #endregion

        #region TrackChanges
        [HttpGet]
        [Route("TrackChangesLog/{entity:int}/{recordId:int}/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetTrackChangesLog(SoeEntityType entity, int recordId, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, tcm.GetTrackChangesLog(base.ActorCompanyId, entity, recordId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value));
        }
        #endregion

    }
}