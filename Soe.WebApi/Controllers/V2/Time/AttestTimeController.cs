using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Core.TimeTree;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Attest")]
    public class AttestTimeController : SoeApiController
    {
        #region Variables

        private readonly TimeTreeAttestManager ttam;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public AttestTimeController(TimeTreeAttestManager ttam, TimeEngineManager tem)
        {
            this.ttam = ttam;
            this.tem = tem;
        }

        #endregion

        #region AttestTransactions

        [HttpPost]
        [Route("Attest/Transactions")]
        public IHttpActionResult SaveAttestForTransactions(SaveAttestForTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveAttestForTransactions(model.Items, model.AttestStateToId, model.IsMySelf));
        }

        [HttpPost]
        [Route("Transactions/Validation")]
        public IHttpActionResult SaveAttestForTransactionsValidation(SaveAttestForTransactionsValidationModel model)
        {
            return Content(HttpStatusCode.OK, ttam.SaveAttestForTransactionsValidation(model.Items, model.AttestStateToId, model.IsMySelf, base.ActorCompanyId, base.UserId));
        }

        #endregion

    }
}