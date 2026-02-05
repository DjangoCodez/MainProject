using System.Net;
using System.Linq;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using Soe.WebApi.Models;
using Soe.WebApi.Controllers;


namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("Billing/InvoicePayment")]
    public class InvoicePaymentController : SoeApiController
    {
        #region Variables

        private readonly PaymentManager pam;

        #endregion

        #region Constructor

        public InvoicePaymentController(PaymentManager pam)
        {
            this.pam = pam;
        }

        #endregion

        #region Payment

        [HttpGet]
        [Route("Payment/GetPaymentTraceViews/{paymentRowId:int}")]
        public IHttpActionResult GetPaymentTraceViews(int paymentRowId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(ActorCompanyId);

            return Ok(pam.GetPaymentTraceViews(paymentRowId, baseSysCurrencyId));
        }

        [HttpPost]
        [Route("Payment/CashPayment/")]
        public IHttpActionResult SaveCashPayments(CashPaymentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pam.SaveCashPaymentsForCustomerInvoice(model.Payments, model.InvoiceId, model.MatchCodeId, model.RemainingAmount, model.SendEmail, model.Email, base.ActorCompanyId, model.UseRounding));
        }

        #endregion Payment

    }
}