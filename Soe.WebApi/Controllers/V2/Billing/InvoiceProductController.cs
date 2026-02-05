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
using Soe.WebApi.Extensions;


namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/InvoiceProduct")]
    public class InvoiceProductController : SoeApiController
    {
        #region Variables

        private readonly ProductManager pm;

        #endregion

        #region Constructor

        public InvoiceProductController(ProductManager pm)
        {
            this.pm = pm;
        }

        #endregion

        #region InvoiceProduct

        [HttpGet]
        [Route("Small/{excludeExternal:bool}")]
        public IHttpActionResult GetInvoiceProductsSmall(bool excludeExternal)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProductsSmall(base.ActorCompanyId, excludeExternal));
        }

        [HttpGet]
        [Route("Small/{invoiceProductVatType:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetInvoiceProducts(int invoiceProductVatType, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetInvoiceProductsDict(base.ActorCompanyId, (TermGroup_InvoiceProductVatType)invoiceProductVatType, addEmptyRow).ToSmallGenericTypes());
        }

      
        #endregion
    }
}