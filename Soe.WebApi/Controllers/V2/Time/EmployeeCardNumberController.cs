using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EmployeeCardNumber")]
    public class EmployeeCardNumberController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public EmployeeCardNumberController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion
        #region CardNumber

        [HttpGet]
        [Route("CardNumber/Grid")]
        public IHttpActionResult GetCardNumbers(HttpRequestMessage message)
        {
            var lst = em.GetCardNumbers(base.ActorCompanyId, base.RoleId, base.UserId);
            return Content(HttpStatusCode.OK, lst);
        }

        [HttpGet]
        [Route("CardNumber/Exists/{cardNumber}/{excludeEmployeeId:int}")]
        public IHttpActionResult CardNumberExists(string cardNumber, int excludeEmployeeId)
        {
            return Content(HttpStatusCode.OK, em.CardNumberExists(base.ActorCompanyId, cardNumber, excludeEmployeeId));
        }

        [HttpDelete]
        [Route("CardNumber/{employeeId:int}")]
        public IHttpActionResult DeleteCardNumber(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.ClearCardNumber(employeeId, base.ActorCompanyId));
        }

        #endregion
    }
}