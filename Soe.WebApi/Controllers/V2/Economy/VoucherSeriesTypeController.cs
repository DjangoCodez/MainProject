using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Util;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Account/VoucherSeriesType")]
    public class VoucherSeriesTypeController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;

        #endregion

        #region Constructor

        public VoucherSeriesTypeController(AccountManager am, SettingManager sm, VoucherManager vm)
        {
            this.vm = vm;
        }

        #endregion

        #region VoucherSeriesType

        [HttpGet]
        [Route("{voucherSeriesTypeId:int?}")]
        public IHttpActionResult GetVoucherSeriesTypes(int? voucherSeriesTypeId = null)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesTypes(base.ActorCompanyId, false, voucherSeriesTypeId).ToDTOs());
        }

        [HttpGet]
        [Route("ByCompany/{addEmptyRow:bool?}/{nameOnly:bool?}")]
        public IHttpActionResult GetVoucherSeriesTypesByCompany(bool? addEmptyRow = false, bool? nameOnly = false)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesTypesDict(base.ActorCompanyId, false, addEmptyRow, nameOnly).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{voucherSeriesTypeId:int}")]
        public IHttpActionResult GetVoucherSeriesType(int voucherSeriesTypeId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesType(voucherSeriesTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveVoucherSeriesType(VoucherSeriesTypeDTO voucherSeriesTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, vm.SaveVoucherSeriesType(voucherSeriesTypeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{voucherSeriesTypeId:int}")]
        public IHttpActionResult DeleteVoucherSeriesType(int voucherSeriesTypeId)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVoucherSeriesType(voucherSeriesTypeId, base.ActorCompanyId));
        }

        #endregion
    }
}