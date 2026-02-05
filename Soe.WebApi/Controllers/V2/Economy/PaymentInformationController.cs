using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;


namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/PaymentInformation")]
    public class PaymentInformationController : SoeApiController
    {
        #region Variables
        private readonly PaymentManager pm;
        #endregion

        #region Constructor
        public PaymentInformationController(PaymentManager _pm)
        {
            this.pm = _pm;
        }
        #endregion

        #region Utility 

        [HttpGet]
        [Route("BicFromIban/{iban}")]
        public IHttpActionResult GetBicFromIban(string iban)
        {
            return Content(HttpStatusCode.OK, pm.GetBicFromIban(iban));
        }

        [HttpGet]
        [Route("IsIbanValid/{iban}")]
        public IHttpActionResult IsIbanValid(string iban)
        {
            return Content(HttpStatusCode.OK, Validator.IsValidIBANNumber(iban));
        }
        #endregion
    }
}
