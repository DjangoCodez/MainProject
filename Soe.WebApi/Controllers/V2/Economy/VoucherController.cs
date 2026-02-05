using Soe.WebApi.Binders;
using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Voucher")]
    public class VoucherController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;
        private readonly AccountManager am;

        #endregion

        #region Constructor

        public VoucherController(VoucherManager vm,AccountManager am)
        {
            this.vm = vm;
            this.am = am;
        }

        #endregion

        #region Voucher

        [HttpGet]
        [Route("Voucher/BySeries/{accountYearId:int}/{voucherSeriesTypeId:int}/{voucherHeadId:int?}")]
        public IHttpActionResult GetVouchersBySeries(int accountYearId, int voucherSeriesTypeId,int? voucherHeadId =null)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherHeadsForGrid(accountYearId, base.ActorCompanyId, false, voucherSeriesTypeId, voucherHeadId));
        }

        [HttpGet]
        [Route("Voucher/Template/{accountYearId:int}")]
        public IHttpActionResult GetSmallVoucherTemplates(int accountYearId)
        {   
            return Content(HttpStatusCode.OK, vm.GetVoucherTemplatesByCompanyDict(accountYearId, base.ActorCompanyId).ToSmallGenericTypes());           
        }

        [HttpGet]
        [Route("Voucher/Template/Grid/{accountYearId:int}/{voucherHeadId:int?}")]
        public IHttpActionResult GetGridVoucherTemplates(int accountYearId, int? voucherHeadId = null)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherTemplatesByYear(accountYearId, base.ActorCompanyId, false, false, voucherHeadId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Voucher/{voucherHeadId:int}/{loadVoucherSeries:bool}/{loadVoucherRows:bool}/{loadVoucherRowAccounts:bool}/{loadAccountBalance:bool}")]
        public IHttpActionResult GetVoucher(int voucherHeadId, bool loadVoucherSeries, bool loadVoucherRows, bool loadVoucherRowAccounts, bool loadAccountBalance)
        {
            List<AccountDim> dims = am.GetAccountDimsByCompany(ActorCompanyId);
            return Content(HttpStatusCode.OK, vm.GetVoucherHead(voucherHeadId, loadVoucherSeries, loadVoucherRows, loadVoucherRowAccounts, loadAccountBalance).ToDTO(loadVoucherRows, loadVoucherRowAccounts, dims));
        }

        [HttpPost]
        [Route("Voucher")]
        public IHttpActionResult SaveVoucher(SaveVoucherModel model)
        {
            if (!ModelState.IsValid)
            return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, vm.SaveVoucher(model.VoucherHead, model.AccountingRows, model.HouseholdRowIds, model.Files, model.RevertVatVoucherId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Voucher/SuperSupport/EditVoucherNr/")]
        public IHttpActionResult EditVoucherNrOnlySuperSupport(EditVoucherNrModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, vm.UpdateVoucherNumberOnlySuperSupport(model.VoucherHeadId, model.NewVoucherNr));
        }

        [HttpDelete]
        [Route("Voucher/{voucherHeadId:int}")]
        public IHttpActionResult DeleteVoucher(int voucherHeadId)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVoucher(voucherHeadId));
        }

        [HttpDelete]
        [Route("Voucher/SuperSupport/{voucherHeadId:int}/{checkTransfer:bool}")]
        public IHttpActionResult DeleteVoucherOnlySuperSupport(int voucherHeadId, bool checkTransfer)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVoucherOnlySuperSupport(voucherHeadId, checkTransfer));
        }

        [HttpDelete]
        [Route("Voucher/SuperSupport/Multiple/")]
        public IHttpActionResult DeleteVouchersOnlySuperSupport([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] voucherHeadIds)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVouchersOnlySuperSupport(voucherHeadIds.ToList()));
        }

        #endregion

        #region VoucherRow

        [HttpGet]
        [Route("VoucherRow/{voucherHeadId:int}")]
        public IHttpActionResult GetVoucherRows(int voucherHeadId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherRows(voucherHeadId, true).ToDTOs(true));
        }

        #endregion

        #region VoucherHistory

        [HttpGet]
        [Route("Voucher/VoucherRowHistory/{voucherHeadId:int}")]
        public IHttpActionResult GetVoucherRowHistory(int voucherHeadId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherRowHistoryDTO(base.ActorCompanyId, voucherHeadId));
        }

        #endregion


    }
}