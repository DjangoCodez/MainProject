using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Economy
{
    [RoutePrefix("Economy/Export")]
    public class EconomyExportController : SoeApiController
    {
        #region Variables

        private readonly SAFTManager saftMgr;

        #endregion

        #region Constructor

        public EconomyExportController(SAFTManager saftMgr)
        {
            this.saftMgr = saftMgr;
        }

        [HttpGet]
        [Route("Saft/Transactions/{fromDate}/{toDate}")]
        public IHttpActionResult GetSAFTTransactionsForExport(string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, saftMgr.GetTransactions(BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Saft/Export/{fromDate}/{toDate}")]
        public IHttpActionResult CreateSAFTExport(string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, saftMgr.Export(BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value, base.ActorCompanyId));
        }

        #endregion
    }
}