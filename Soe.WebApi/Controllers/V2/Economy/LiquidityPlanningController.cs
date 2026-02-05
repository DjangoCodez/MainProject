using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.DTO;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/LiquidityPlanning")]
    public class LiquidityPlanningController : SoeApiController
    {
        #region Variables

        private readonly LiquidityPlanningManager lpm;

        #endregion

        #region Constructor

        public LiquidityPlanningController(LiquidityPlanningManager lpm)
        {
            this.lpm = lpm;
        }

        #endregion

        #region LiquidityPlanning

        [HttpPost]
        [Route("LiquidityPlanning/Get")]
        public IHttpActionResult GetLiquidityPlanning(GetLiquidityPlanningModel model)
        {
            return Content(HttpStatusCode.OK, lpm.GetLiquidityPlanning(base.ActorCompanyId, model.From, model.To, model.Exclusion, model.Balance, model.Unpaid, model.PaidUnchecked, model.PaidChecked));
        }

        [HttpPost]
        [Route("LiquidityPlanning/Get/new")]
        public IHttpActionResult GetLiquidityPlanningv2(GetLiquidityPlanningModel model)
        {
            return Content(HttpStatusCode.OK, lpm.GetLiquidityPlanningv2(base.ActorCompanyId, model.From, model.To, model.Exclusion, model.Balance, model.Unpaid, model.PaidUnchecked, model.PaidChecked));
        }

        [HttpPost]
        [Route("LiquidityPlanning")]
        public IHttpActionResult SaveLiquidityPlanningTransaction(LiquidityPlanningDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, lpm.SaveLiquidityPlanningTransaction(model));
        }

        [HttpDelete]
        [Route("LiquidityPlanning/{liquidityPlanningTransactionId:int}")]
        public IHttpActionResult DeleteLiquidityPlanningTransaction(int liquidityPlanningTransactionId)
        {
            return Content(HttpStatusCode.OK, lpm.DeleteLiquidityPlanningTransaction(liquidityPlanningTransactionId));
        }

        #endregion

    }
}