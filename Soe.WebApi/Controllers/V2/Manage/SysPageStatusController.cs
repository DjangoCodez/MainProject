using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/SysPageStatus")]
    public class SysPageStatusController : SoeApiController
    {
        #region Variables

        private readonly GeneralManager gm;

        #endregion

        #region Constructor

        public SysPageStatusController(GeneralManager gm)
        {
            this.gm = gm;
        }

        #endregion

        [HttpGet]
        [Route("AvailableSpaModules")]
        public IHttpActionResult GetAvailableSpaModules()
        {
            return Content(HttpStatusCode.OK, gm.GetMigratedFeaturesFromCache());
        }
    }
}