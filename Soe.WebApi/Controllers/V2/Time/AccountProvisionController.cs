using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Payroll/AccountProvision")]
    public class AccountProvisionController : SoeApiController
    {
        #region Variables

        private readonly TimeEngineManager tem;
        private readonly PayrollManager pm;
        #endregion

        #region Constructor

        public AccountProvisionController(TimeEngineManager tem, PayrollManager pm)
        {
            this.tem = tem;
            this.pm = pm;
        }

        #endregion

        #region AccountProvisionBase

        [HttpGet]
        [Route("AccountProvisionBase/Columns/{timePeriodId:int}")]
        public IHttpActionResult GetAccountProvisionBaseColumns(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetAccountProvisionBaseColumns(timePeriodId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountProvisionBase/{timePeriodId:int}")]
        public IHttpActionResult GetAccountProvisionBase(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetAccountProvisionBase(timePeriodId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountProvisionBase/Lock/{timePeriodId:int}")]
        public IHttpActionResult LockAccountProvisionBase(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.LockAccountProvisionBase(timePeriodId));
        }

        [HttpGet]
        [Route("AccountProvisionBase/Unlock/{timePeriodId:int}")]
        public IHttpActionResult UnLockAccountProvisionBase(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.UnLockAccountProvisionBase(timePeriodId));
        }

        [HttpPost]
        [Route("AccountProvisionBase")]
        public IHttpActionResult SaveAccountProvisionBase(List<AccountProvisionBaseDTO> provisions)
        {
            return Content(HttpStatusCode.OK, tem.SaveAccountProvisionBase(provisions));
        }

        #endregion

        #region AccountProvisionTransaction

        [HttpGet]
        [Route("AccountProvisionTransaction/{timePeriodId:int}")]
        public IHttpActionResult GetAccountProvisionTransactions(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetAccountProvisionTransactions(timePeriodId, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("AccountProvisionTransaction/Update")]
        public IHttpActionResult UpdateAccountProvisionTransactions(AccountProvisionTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.UpdateAccountProvisionTransactions(model.Transactions));
        }

        [HttpPost]
        [Route("AccountProvisionTransaction/Attest")]
        public IHttpActionResult SaveAttestForAccountProvision(AccountProvisionTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveAttestForAccountProvision(model.Transactions));
        }

        #endregion
    }
}