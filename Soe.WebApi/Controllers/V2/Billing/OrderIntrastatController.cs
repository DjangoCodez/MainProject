using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using SoftOne.Soe.Common.DTO;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Order")]
    public class OrderIntrastatController : SoeApiController
    {
        #region Variables

        private readonly CommodityCodeManager ccm;
        private readonly ImportExportManager iem;

        #endregion

        #region Constructor

        public OrderIntrastatController(CommodityCodeManager ccm, ImportExportManager  iem)
        {
            this.ccm = ccm;
            this.iem = iem;
        }

        #endregion

        #region IntrastatTransactions

        [HttpGet]
            [Route("Intrastat/Transactions/{originId:int}")]
            public IHttpActionResult GetIntrastatTransactions(int originId)
            {
                return Content(HttpStatusCode.OK, ccm.GetIntrastatTransactions(originId));
            }

            [HttpGet]
            [Route("Intrastat/Transactions/ForExport/{intrastatReportingType:int}/{fromDate}/{toDate}")]
            public IHttpActionResult GetIntrastatTransactionsForExport(int intrastatReportingType, string fromDate, string toDate)
            {
                return Content(HttpStatusCode.OK, ccm.GetIntrastatTransactionsForExport((IntrastatReportingType)intrastatReportingType, BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value, base.ActorCompanyId));
            }

            [HttpPost]
            [Route("Intrastat/Transactions/")]
            public IHttpActionResult SaveIntrastatTransactions(SaveIntrastatTransactionModel model)
            {
                return Content(HttpStatusCode.OK, ccm.SaveIntrastatTransactions(model.Transactions, model.OriginId, (SoeOriginType)model.OriginType));
            }

            [HttpPost]
            [Route("Intrastat/Transactions/Export/")]
            public IHttpActionResult CreateIntrastatExport(EvaluatedSelection selection)
            {
                return Content(HttpStatusCode.OK, iem.CreateIntrastatStatisticsExport(selection));
            }

            #endregion

    }
}