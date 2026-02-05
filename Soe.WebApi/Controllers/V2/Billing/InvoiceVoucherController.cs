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
    [RoutePrefix("Billing/InvoiceVoucher")]
    public class InvoiceVoucherController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;

        #endregion

        #region Constructor

        public InvoiceVoucherController(VoucherManager vm)
        {
            this.vm = vm;
        }

        #endregion

        #region Voucher

        [HttpGet]
        [Route("GetVoucherTraceViews/{voucherHeadId:int}")]
        public IHttpActionResult GetVoucherTaceViews(int voucherHeadId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, vm.GetVoucherTraceViews(voucherHeadId, baseSysCurrencyId));
        }

        #endregion Voucher
    }
}