using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/InvoiceSupplierAgreements")]
    public class SupplierAgreementsController : SoeApiController
    {
        #region Variables

        private readonly SupplierAgreementManager sam;

        #endregion

        #region Constructor

        public SupplierAgreementsController(SupplierAgreementManager sam)
        {
            this.sam = sam;
        }

        #endregion

        #region SupplierAgreements

        [HttpGet]
        [Route("Providers/")]
        public IHttpActionResult GetSupplierAgreementProviders()
        {
            return Content(HttpStatusCode.OK, sam.GetSupplierAgreementProviders(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("{providerType:int}")]
        public IHttpActionResult GetSupplierAgreements(int providerType)
        {
            return Content(HttpStatusCode.OK, sam.GetSupplierAgreements(base.ActorCompanyId, providerType).ToDTOs());
        }

        [HttpPost]
        [Route("Discount/")]
        public IHttpActionResult SaveSupplierAgreementDiscount(SupplierAgreementDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sam.SaveDiscount(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Import/")]
        public IHttpActionResult SaveSupplierAgreements(SupplierAgreementModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sam.Import(model.Files[0].Bytes, model.Files[0].Name, (SoeSupplierAgreementProvider)model.WholesellerId, model.PriceListTypeId, base.ActorCompanyId, model.GeneralDiscount));
        }

        [HttpDelete]
        [Route("{wholesellerId:int}/{priceListTypeId:int}")]
        public IHttpActionResult DeleteSupplierAgreements(int wholesellerId, int priceListTypeId)
        {
            return Content(HttpStatusCode.OK, sam.DeleteSupplierAgreements(base.ActorCompanyId, (SoeSupplierAgreementProvider)wholesellerId, priceListTypeId));
        }

        #endregion
    }
}