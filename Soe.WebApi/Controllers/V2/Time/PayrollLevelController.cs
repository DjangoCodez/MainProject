using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/PayrollLevel")]
    public class PayrollLevelController : SoeApiController
    {
        #region Variables

        private readonly PayrollManager pm;

        #endregion

        #region Constructor

        public PayrollLevelController(PayrollManager tsm)
        {
            this.pm = tsm;
        }

        #endregion

        #region PayrollLevel

        [HttpGet]
        [Route("Grid/{payrollLevelId:int?}")]
        public IHttpActionResult GetPayrollLevelsGrid(int? payrollLevelId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollLevels(base.ActorCompanyId, payrollLevelId).ToGridDTOs());
        }

        [HttpGet]        
        [Route("{payrollLevelId:int}")]
        public IHttpActionResult GetPayrollLevel(int payrollLevelId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollLevel(base.ActorCompanyId, payrollLevelId).ToDTO());
        }
        [HttpPost]
        [Route("")]
        public IHttpActionResult SavePayrollLevel(PayrollLevelDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollLevel(model, base.ActorCompanyId));
        }
        [HttpDelete]
        [Route("{payrollLevelId:int}")]
        public IHttpActionResult DeletePayrollLevel(int payrollLevelId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollLevel(payrollLevelId));
        }
        #endregion
    }
}