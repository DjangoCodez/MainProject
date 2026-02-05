using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Import")]
    public class ImportController : SoeApiController
    {
        #region Variables

        private readonly EdiManager em;

        #endregion

        #region Constructor

        public ImportController(EdiManager em)
        {
            this.em = em;
        }
        
        #endregion

        #region EDI

        [HttpGet]
        [Route("Edi/EdiEntryViews/{classification:int}/{originType:int}")]
        public IHttpActionResult GetEdiEntrysWithStateCheck(int classification, int originType)
        {
            return Content(HttpStatusCode.OK, em.GetEdiEntrysWithStateCheck(classification, originType));
        }

        [HttpPost]
        [Route("Edi/EdiEntryViews/Filtered/")]
        public IHttpActionResult GetFilteredEdiEntrys(GetFilteredEDIEntrysModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.GetFilteredEdiEntrys(base.ActorCompanyId, (SoeEntityState)model.Classification, model.OriginType, model.BillingTypes, model.BuyerId, model.DueDate, model.InvoiceDate, model.OrderNr, model.OrderStatuses, model.SellerOrderNr, model.EdiStatuses, model.Sum, model.SupplierNrName, model.AllItemsSelection));
        }

        #endregion
    }
}