using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Economy
{
    [RoutePrefix("Economy/Customer")]
    public class CustomerController : SoeApiController
    {
        #region Variables

        private readonly CustomerManager cm;
        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public CustomerController(CustomerManager cm, InvoiceManager im)
        {
            this.cm = cm;
            this.im = im;
        }

        #endregion

        #region Matches

        [HttpPost]
        [Route("Invoice/Matches/Payments")]
        public IHttpActionResult SearchInvoicesPaymentsAndMatches(SearchInvoicesPaymentsAndMatchesDTO message)
        {
            return Ok(im.GetInvoicesPaymentsAndMatches(ActorCompanyId, message.ActorId, message.Type, message.AmountFrom, message.AmountTo, message.DateFrom, message.DateTo, message.OriginType));
        }

        [HttpGet]
        [Route("Invoice/Matches")]
        public IHttpActionResult GetMatches([FromUri]int recordId, [FromUri]int actorId, [FromUri]int type)
        {
            return Ok(im.GetMatches(ActorCompanyId, actorId, recordId, type));
        }       

        #endregion

        #region InsecureDebts

        [HttpGet]
        [Route("Invoice/InvoiceAndPaymentStatus/{type:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetInvoiceAndPaymentStatus(SoeOriginType type, bool addEmptyRow)
        {
            return Ok(im.GetInvoiceAndPaymentStatus(type, addEmptyRow));
        }

        [HttpGet]
        [Route("Invoice/InsecureDebts/")]
        public IHttpActionResult GetInsecureDebts()
        {
            return Ok(im.GetCustomerInsecureDebtDTO(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Invoice/InsecureDebts/")]
        public IHttpActionResult SaveInsecureDebts(List<int> customerInvoiceIds)
        {
            return Ok(im.SaveCustomerInvoiceInsecureStatus(base.ActorCompanyId, true, customerInvoiceIds));
        }

        [HttpPost]
        [Route("Invoice/NotInsecureDebts/")]
        public IHttpActionResult SaveNotInsecureDebts(List<int> customerInvoiceIds)
        {
            return Ok(im.SaveCustomerInvoiceInsecureStatus(base.ActorCompanyId, false, customerInvoiceIds));
        }

        #endregion

        [HttpGet]
        [Route("HouseholdTaxApplicants/{customerId:int}")]
        public IHttpActionResult GetHouseholdTaxDeductionApplicants(int customerId)
        {
            return Ok(cm.GetHouseholdTaxDeductionApplicants(customerId).ToDTO());
        }
    }
}