using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/UnionFee")]
    public class UnionFeeController : SoeApiController
    {
        #region Variables

        private readonly PayrollManager pm;
        private readonly ProductManager prm;

        #endregion

        #region Constructor

        public UnionFeeController(PayrollManager pm, ProductManager prm)
        {
            this.pm = pm;
            this.prm = prm;
        }

        #endregion

        #region UnionFee

        [HttpGet]
        [Route("Grid/{unionFeeId:int?}")]
        public IHttpActionResult GetUnionFeesGrid(int? unionFeeId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetUnionFees(base.ActorCompanyId, loadPriceTypes: true, loadPayrollProducts: true, unionFeeId: unionFeeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{unionFeeId:int}")]
        public IHttpActionResult GetUnionFee(int unionFeeId)
        {
            return Content(HttpStatusCode.OK, pm.GetUnionFee(unionFeeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveUnionFee(UnionFeeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SaveUnionFee(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{unionFeeId:int}")]
        public IHttpActionResult DeleteUnionFee(int unionFeeId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteUnionFee(unionFeeId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PayrollPriceTypesDict")]
        public IHttpActionResult GetPayrollPriceTypesDict()
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceTypesDict(base.ActorCompanyId, null, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UnionFeePayrollProducts")]
        public IHttpActionResult GetUnionFeePayrollProducts()
        {
            List<PayrollProduct> payrollProducts = prm.GetPayrollProducts(base.ActorCompanyId, null);
            payrollProducts = payrollProducts.Where(p => p.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction && p.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_UnionFee).ToList();
            return Content(HttpStatusCode.OK, payrollProducts.ToSmallDTOs());
        }

        #endregion
    }
}