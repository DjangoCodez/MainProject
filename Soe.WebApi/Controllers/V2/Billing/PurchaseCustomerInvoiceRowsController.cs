using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Purchase")]
    public class PurchaseCustomerInvoiceRowsController : SoeApiController
    {
        #region Variables

        private readonly PurchaseManager pm;

        #endregion

        #region Constructor

        public PurchaseCustomerInvoiceRowsController(PurchaseManager pm)
        {
            this.pm = pm;
        }

        #endregion


        #region CustomerInvoiceRows
        [HttpGet]
        [Route("CustomerInvoiceRows/{viewType:int}/{id:int}")]
        public IHttpActionResult GetCustomerInvoiceRows(int viewType, int id)
        {
            if (viewType == 0 || id == 0)
                return Error(HttpStatusCode.BadRequest, null, null, null);
            else
                return Content(HttpStatusCode.OK, pm.GetCustomerInvoiceRows(this.ActorCompanyId, viewType, id));
        }
        #endregion

    }
}