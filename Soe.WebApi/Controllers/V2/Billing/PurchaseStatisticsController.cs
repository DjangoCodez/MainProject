using SoftOne.Soe.Business.Core;
using System.Net;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;

using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Purchase")]
    public class PurchaseStatisticsController : SoeApiController
    {

        #region Variables

        private readonly PurchaseManager pm;


        #endregion

        #region Constructor
        public PurchaseStatisticsController(PurchaseManager pm)
        {
            this.pm = pm;

        }
        #endregion

        #region Statistics
        [HttpPost]
        [Route("Statistics/")]
        public IHttpActionResult GetPurchaseStatistics(GeneralProductStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseStatistics(model.FromDate, model.ToDate));
        }
        #endregion

    }
}