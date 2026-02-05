using Soe.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Core

{
    [RoutePrefix("V2/Core/Process")]
    public class ProcessController : SoeApiController
    {
        public ProcessController() { }

        #region ProcessInfo

        [HttpGet]
        [Route("ProgressInfo/{key}")]
        public IHttpActionResult GetProgressInfo(string key)
        {
            Guid guid = Guid.Parse(key);
            return Content(HttpStatusCode.OK, monitor.GetInfo(guid));
        }

        #endregion
    }
}
