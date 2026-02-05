using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/AccountBalance")]
    public class AccountBalanceController : SoeApiController
    {
        #region Variables

        private readonly AccountBalanceManager abm;

        #endregion

        #region Constructor

        public AccountBalanceController(AccountBalanceManager abm)
        {
            this.abm = abm;
        }

        #endregion

        #region AccountBalance

        [HttpGet]
        [Route("ByAccount/{accountId:int}/{loadYear:bool}")]
        public IHttpActionResult GetAccountBalanceByAccount(int accountId, bool loadYear)
        {
            return Content(HttpStatusCode.OK, abm.GetAccountBalanceByAccount(accountId, loadYear: loadYear).ToDTOs(loadYear, false));
        }

        [HttpPost]
        [Route("CalculateForAccounts/{accountYearId:int}")]
        public IHttpActionResult CalculateAccountBalanceForAccounts(int accountYearId)
        {
            return Content(HttpStatusCode.OK, abm.CalculateAccountBalanceForAccounts(base.ActorCompanyId, accountYearId));
        }

        [HttpPost]
        [Route("CalculateForAccountsAllYears")]
        public IHttpActionResult CalculateForAccountsAllYears()
        {
            return Content(HttpStatusCode.OK, abm.CalculateAccountBalanceForAccounts(base.ActorCompanyId, null));
        }

        [HttpPost]
        [Route("CalculateForAccountInAccountYears/{accountId:int}")]
        public IHttpActionResult CalculateAccountBalanceForAccountInAccountYears(int accountId)
        {
            return Content(HttpStatusCode.OK, abm.CalculateAccountBalanceForAccountInAccountYears(base.ActorCompanyId, accountId));
        }

        [HttpPost]
        [Route("CalculateAccountBalanceForAccountsFromVoucher")]
        public IHttpActionResult CalculateAccountBalanceForAccountsFromVoucher(CalculateAccountBalanceForAccountsFromVoucherModel model)
        {
            abm.CalculateAccountBalanceForAccountsFromVoucher(base.ActorCompanyId, model.accountYearId, 3);
            return Content(HttpStatusCode.OK, "");
        }

        [HttpPost]
        [Route("GetAccountBalances/{accountYearId:int}")]
        public IHttpActionResult GetAccountBalances(int accountYearId)
        {
            //AccountBalanceManager abm = new AccountBalanceManager(null, actorCompanyId);
            return Content(HttpStatusCode.OK, abm.GetAccountBalancesByYearDict(accountYearId).ToDecimalKeyValues());
        }

        #endregion

    }
}