using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/ScheduledJob")]
    public class ScheduledJobController : SoeApiController
    {
        #region Variables

        private readonly ScheduledJobManager sjm;

        #endregion

        #region Constructor

        public ScheduledJobController(ScheduledJobManager sjm)
        {
            this.sjm = sjm;
        }

        #endregion

        #region ScheduledJobHead

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}/{includeSharedOnLicense:bool}")]
        public IHttpActionResult GetScheduledJobHeadsDict(bool addEmptyRow, bool includeSharedOnLicense)
        {
            return Content(HttpStatusCode.OK, sjm.GetScheduledJobHeadsDict(base.ActorCompanyId, addEmptyRow, includeSharedOnLicense).ToSmallGenericTypes());
        }

        #endregion
    }
}