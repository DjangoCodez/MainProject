using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/PayrollPriceType")]
    public class PayrollPriceTypeController : SoeApiController
    {
        #region Variables

        private readonly PayrollManager pm;

        #endregion

        #region Constructor

        public PayrollPriceTypeController(PayrollManager pm)
        {
            this.pm = pm;
        }

        #endregion

        #region PayrollPriceType

        [HttpGet]
        [Route("Grid/{payrollPriceTypeId:int?}")]
        public IHttpActionResult GetPayrollPriceTypesGrid(int? payrollPriceTypeId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceTypes(base.ActorCompanyId, null, false, payrollPriceTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{payrollPriceTypeId:int}")]
        public IHttpActionResult GetPayrollPriceType(int payrollPriceTypeId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceType(payrollPriceTypeId).ToDTO(true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SavePayrollPriceType(PayrollPriceTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollPriceType(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{payrollPriceTypeId:int}")]
        public IHttpActionResult DeletePayrollPriceType(int payrollPriceTypeId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollPriceType(payrollPriceTypeId));
        }

        #endregion
    }
}