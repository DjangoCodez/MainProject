using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Report
{
    [RoutePrefix("V2/Report")]
    public class SalesEUController : SoeApiController
    {
        #region Variables
        private readonly EUSalesManager euSm;
        #endregion

        #region Constructor        

        public SalesEUController(EUSalesManager euSm)
        {
            this.euSm = euSm;
        }
        #endregion

        #region SalesEU

        [HttpGet]
        [Route("SalesEU/{startDate}/{stopDate}")]
        public IHttpActionResult SalesEU(string startDate, string stopDate)
        {
            return Content(HttpStatusCode.OK, euSm.GetSales(base.ActorCompanyId, BuildDateTimeFromString(startDate, true).Value,
                                                                        BuildDateTimeFromString(stopDate, true).Value));
        }
        [HttpGet]
        [Route("SalesEUDetails/{actorId}/{startDate}/{stopDate}")]
        public IHttpActionResult SalesEUDetails(int actorId, string startDate, string stopDate)
        {
            return Content(HttpStatusCode.OK, euSm.GetSalesDetails(actorId, BuildDateTimeFromString(startDate, true).Value,
                                                                        BuildDateTimeFromString(stopDate, true).Value));
        }
        [HttpGet]
        [Route("SalesEUExportFile/{periodType}/{startDate}/{stopDate}")]
        public IHttpActionResult SalesEUExportFile(int periodType, string startDate, string stopDate)
        {
            var result = euSm.GetExportFiles(base.ActorCompanyId, (DatePeriodType)periodType, BuildDateTimeFromString(startDate, true).Value, BuildDateTimeFromString(stopDate, true).Value);

            if (!result.Success)
            {
                List<string> error = new List<string>();
                error.Add(result.ErrorMessage.ToString());
                return Content(HttpStatusCode.OK, error);
            }

            return Content(HttpStatusCode.OK, result.Value);
        }

        #endregion
    }
}