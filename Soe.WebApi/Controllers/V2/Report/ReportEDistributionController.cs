using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Soe.WebApi.V2.Report
{
    [RoutePrefix("V2/ReportEDistributionItems")]
    public class ReportEDistributionController : SoeApiController
    {
        private readonly InvoiceDistributionManager idm;

        public ReportEDistributionController(InvoiceDistributionManager idm)
        {
            this.idm = idm;
        }

        [HttpGet]
        [Route("EDistributionItems/{originType:int}/{type:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetEDistributionItems(int originType, int type, int allItemsSelection)
        {
            return Content(HttpStatusCode.OK, idm.GetDistributionItems(base.ActorCompanyId, (SoeOriginType)originType, (TermGroup_EDistributionType)type, (TermGroup_GridDateSelectionType)allItemsSelection));
        }
    }
}