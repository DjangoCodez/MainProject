using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Accounting")]
    public class BalanceController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;
        private readonly AccountBalanceManager abm;

        #endregion

        #region Constructor

        public BalanceController(AccountManager am, AccountBalanceManager abm)
        {
            this.am = am;
            this.abm = abm;
        }

        #endregion

        #region Balance

        [HttpGet]
        [Route("Balance/{accountYearId:int}")]
        public IHttpActionResult GetAccountYearBalance(int accountYearId)
        {
            var accountDims = am.GetAccountDimsByCompany(onlyInternal: true, loadInternalAccounts: true);
            return Content(HttpStatusCode.OK, abm.GetAccountYearBalanceHeads(accountYearId, base.ActorCompanyId).ToFlatDTOs(accountDims, false));
        }

        [HttpGet]
        [Route("Balance/Transfer/{accountYearId:int}")]
        public IHttpActionResult GetAccountYearBalanceFromPreviousYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, abm.GetAccountYearBalanceHeadsForPreviousYear(accountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Balance")]
        public IHttpActionResult SaveAccountYearBalances(SaveAccountYearBalanceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, abm.SaveAccountYearBalances(model.items, model.AccountYearId, base.ActorCompanyId));
        }

        #endregion

    }
}