namespace Soe.WebApi.V2.Economy
{
    using Soe.WebApi.Controllers;
    using SoftOne.Soe.Business.Core;
    using SoftOne.Soe.Common.DTO;
    using System.Collections.Generic;
    using System.Web.Http;

    [RoutePrefix("V2/Economy/Invoice/Matches")]
    public class InvoiceMatchesController : SoeApiController
    {
        #region Variables
        private readonly InvoiceManager im;
        #endregion

        #region Constructor
        public InvoiceMatchesController(
            InvoiceManager im)
        {
            this.im = im;
        }
        #endregion


        #region Methods

        [HttpPost]
        [Route("Payments")]
        public IHttpActionResult SearchInvoicesPaymentsAndMatches(
            SearchInvoicesPaymentsAndMatchesDTO message)
        {
            List<InvoiceMatchingDTO> invoiceMatches = im.GetInvoicesPaymentsAndMatches(
                actorCompanyId: ActorCompanyId,
                actorId: message.ActorId,
                type: message.Type,
                aFrom: message.AmountFrom,
                aTo: message.AmountTo,
                dtFrom: message.DateFrom,
                dtTo: message.DateTo,
                originType: message.OriginType);
            return Ok(invoiceMatches);
        }

        #endregion

    }
}
