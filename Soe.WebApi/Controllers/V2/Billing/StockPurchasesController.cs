using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Stock/Purchase")]
    public class StockPurchaseController : SoeApiController
    {
        #region Variables

        private readonly StockManager sm;

        #endregion

        #region Constructor

        public StockPurchaseController(StockManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region Purchase
        [HttpPost]
        [Route("GenerateSuggestion")]
        public IHttpActionResult GeneratePurchaseSuggestion(GenerateStockPurchaseSuggestionDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.GetStockPurchaseSugggestion(this.ActorCompanyId, model));
        }
        #endregion
    }
}