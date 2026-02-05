using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/SupplierInvoiceArrival")]
    public class SupplierInvoiceArrivalController : SoeApiController
    {
        #region Variables

        private readonly SupplierInvoiceManager sim;

        #endregion
        public SupplierInvoiceArrivalController(SupplierInvoiceManager sim)
        {
            this.sim = sim;
        }

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetSupplierInvoiceArrivalHall()
        {
            return Ok(sim.GetSupplierInvoiceIncomingHallGrid(ActorCompanyId));
        }


		[HttpGet]
		[Route("Invoice/SupplierInvoiceHistory/{supplierId:int}")]
		public IHttpActionResult GetSupplierInvoice20Latest(int supplierId)
		{
			return Content(HttpStatusCode.OK, sim.GetSupplierInvoiceHistory(supplierId, 20));
		}
	}
}