using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/ProjectProduct")]
    public class ProjectProductController : SoeApiController
    {
        private readonly ProjectManager pm;
        private readonly TimeTransactionManager ttm;
        private readonly ProductManager prom;

        public ProjectProductController(ProjectManager pm, TimeTransactionManager ttm, ProductManager prom)
        {
            this.pm = pm;
            this.ttm = ttm;
            this.prom = prom;
        }

        #region Product
        [HttpGet]
        [Route("TimeCode")]
        public IHttpActionResult GetTimeCodes(HttpRequestMessage message)
        {
            int projectId = message.GetIntValueFromQS("projectId");
            return Content(HttpStatusCode.OK, ttm.GetProjectTimeCodeTransactionItems(ActorCompanyId, projectId));
        }

        [HttpGet]
        [Route("PriceList/{comparisonPriceListTypeId:int}/{priceListTypeId:int}/{loadAll:bool}/{priceDate}")]
        public IHttpActionResult GetPriceLists(int comparisonPriceListTypeId, int priceListTypeId, bool loadAll, string priceDate)
        {
            return Content(HttpStatusCode.OK, prom.GetProductComparisonDTOs(ActorCompanyId, comparisonPriceListTypeId, priceListTypeId, loadAll, BuildDateTimeFromString(priceDate, true)));
        }

        [HttpGet]
        [Route("ProductRows/{projectId:int}/{originType:int}/{includeChildProjects:bool}/{fromDate?}/{toDate?}")]
        public IHttpActionResult GetProductRows(int projectId, int originType, bool includeChildProjects, string fromDate = "", string toDate = "")
        {
            var projectCentral = new ProjectCentralManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, projectCentral.GetProjectProductRows(projectId, originType, includeChildProjects, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true)));
        }

        #endregion
    }
}
