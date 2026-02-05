using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Accounting")]
    public class AccountReconciliationController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;

        #endregion

        #region Constructor

        public AccountReconciliationController(VoucherManager vm)
        {
            this.vm = vm;
        }

        #endregion

        #region Reconciliation

        [HttpGet]
        [Route("ReconciliationRows/{dim1Id:int}/{fromDim1}/{toDim1}/{fromDate}/{toDate}/")]
        public IHttpActionResult GetReconciliationRows(int dim1Id, string fromDim1, string toDim1, string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, vm.GetReconciliationRows(base.ActorCompanyId, dim1Id, fromDim1, toDim1, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true)));
        }

        [HttpGet]
        [Route("ReconciliationPerAccount/{accountId:int}/{fromDate}/{toDate}/")]
        public IHttpActionResult GetReconciliationPerAccount(int accountId, string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, vm.GetReconciliationPerAccount(base.ActorCompanyId, accountId, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true)));
        }

        #endregion
    }
}