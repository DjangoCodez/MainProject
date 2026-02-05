using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Data;


namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Account")]
    public class GrossProfitCodeController : SoeApiController
    {
        #region Variables

        private readonly GrossProfitManager gpm;

        #endregion

        #region Constructor

        public GrossProfitCodeController(GrossProfitManager gpm)
        {
            this.gpm = gpm;
        }

        #endregion


        #region GrossProfitCode

        [HttpGet]
        [Route("GrossProfitCode/Grid")]
        public IHttpActionResult GetGrossProfitCodesGrid(int? grossProfitCodeId = null)
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCodes(base.ActorCompanyId,null, grossProfitCodeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("GrossProfitCode/")]
        public IHttpActionResult GetGrossProfitCodes()
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCodes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("GrossProfitCode/ByYear/{accountYearId:int}")]
        public IHttpActionResult GetGrossProfitCodesByYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCodes(base.ActorCompanyId, accountYearId).ToDTOs());
        }

        [HttpGet]
        //[Route("GrossProfitCode/{grossProfitCodeId:int}")]
        [Route("GrossProfitCode/{grossProfitCodeId}")]
        public IHttpActionResult GetGrossProfitCode(int grossProfitCodeId)
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCode(base.ActorCompanyId, grossProfitCodeId).ToDTO());
        }

        [HttpPost]
        [Route("GrossProfitCode")]
        public IHttpActionResult SaveGrossProfitCode(GrossProfitCodeDTO grossProfitCodeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gpm.SaveGrossProfitCode(base.ActorCompanyId, grossProfitCodeDTO));
        }

        [HttpDelete]
        [Route("GrossProfitCode/{grossProfitCodeId:int}")]
        public IHttpActionResult DeleteGrossProfitCode(int grossProfitCodeId)
        {
            return Content(HttpStatusCode.OK, gpm.DeleteGrossProfitCode(base.ActorCompanyId, grossProfitCodeId));
        }

        #endregion
    }
}