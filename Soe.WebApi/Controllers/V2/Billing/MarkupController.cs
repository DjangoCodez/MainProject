using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using System.Collections.Generic;
using System;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Invoice/Markup")]
    public class MarkupController : SoeApiController
    {
        #region Variables

        private readonly MarkupManager mm;
        private readonly ExpenseManager em;

        #endregion

        #region Constructor

        public MarkupController(MarkupManager mm, ExpenseManager em)
        {
            this.mm = mm;
            this.em = em;
        }
        #endregion

        #region Markup

        [HttpGet]
        [Route("{isDiscount:bool}")]
        public IHttpActionResult GetMarkup(bool isDiscount)
        {
            return Content(HttpStatusCode.OK, mm.GetMarkup(base.ActorCompanyId, isDiscount));
        }

        [HttpGet]
        [Route("Discount/{sysWholesellerId:int}/{code}")]
        public IHttpActionResult GetDiscount(int sysWholesellerId, string code)
        {
            return Content(HttpStatusCode.OK, mm.GetDiscountBySysWholeseller(base.ActorCompanyId, sysWholesellerId, code == "null" ? String.Empty : code));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveMarkup(List<MarkupDTO> items)
        {
            return Content(HttpStatusCode.OK, mm.SaveMarkup(items, base.ActorCompanyId));
        }

        #endregion

   
    }
}
