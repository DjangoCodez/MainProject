using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Registry/OpeningHours")]
    public class OpeningHourController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;

        #endregion

        #region Constructor

        public OpeningHourController(CalendarManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region OpeningHours

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetOpeningHoursDict(bool addEmptyRow, bool includeDateInName)
        {
            return Content(HttpStatusCode.OK, cm.GetOpeningHoursDict(base.ActorCompanyId, addEmptyRow, includeDateInName).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetOpeningHours(string fromDate, string toDate, int? openingHoursId = null)
        {
            return Content(HttpStatusCode.OK, cm.GetOpeningHoursForCompany(base.ActorCompanyId, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true), openingHoursId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{openingHoursId:int}")]
        public IHttpActionResult GetOpeningHour(int openingHoursId)
        {
            return Content(HttpStatusCode.OK, cm.GetOpeningHours(openingHoursId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveOpeningHours(OpeningHoursDTO openingHoursDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.SaveOpeningHours(openingHoursDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{openingHoursId:int}")]
        public IHttpActionResult DeleteOpeningHours(int openingHoursId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteOpeningHours(openingHoursId, base.ActorCompanyId));
        }

        #endregion

    }
}