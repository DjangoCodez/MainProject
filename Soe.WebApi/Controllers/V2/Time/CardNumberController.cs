using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/CardNumber")]
    public class CardNumberController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public CardNumberController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region CardNumber

        [HttpGet]
        [Route("Grid/")]
        public IHttpActionResult GetCardNumbers()
        {
            return Content(HttpStatusCode.OK, em.GetCardNumbers(base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("Exists/{cardNumber}/{excludeEmployeeId:int}")]
        public IHttpActionResult CardNumberExists(string cardNumber, int excludeEmployeeId)
        {
            return Content(HttpStatusCode.OK, em.CardNumberExists(base.ActorCompanyId, cardNumber, excludeEmployeeId));
        }

        [HttpDelete]
        [Route("{employeeId:int}")]
        public IHttpActionResult DeleteCardNumber(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.ClearCardNumber(employeeId, base.ActorCompanyId));
        }

        #endregion

    }
}