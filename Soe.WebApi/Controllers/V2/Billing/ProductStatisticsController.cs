using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Product")]
    public class ProductStatisticsController : SoeApiController
    {
        #region Variables

        private readonly ProductManager pm;

        #endregion

        #region Constructor

        public ProductStatisticsController(ProductManager pm)
        {
            this.pm = pm;
        }

        #endregion

        [HttpPost]
        [Route("Statistics/")]
        public IHttpActionResult GetProductStatistics(ProductStatisticsRequest model)
        {
            return Content(HttpStatusCode.OK, pm.GetProductStatistics(model.ProductIds, model.FromDate, model.ToDate, model.OriginType, model.IncludeServiceProducts));
        }
    }
}