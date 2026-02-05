using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/VatCode")]
    public class VatCodeController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;

        #endregion

        #region Constructor

        public VatCodeController(AccountManager am)
        {
            this.am = am;
        }

        #endregion

        [HttpGet]
        [Route("Grid/{vatCodeId:int?}")]
        public IHttpActionResult GetVatCodesGrid(int? vatCodeId = null)
        {
            return Content(HttpStatusCode.OK, am.GetVatCodeGridDTOs(base.ActorCompanyId, vatCodeId));
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetVatCodesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetVatCodesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetVatCodes()
        {
            return Content(HttpStatusCode.OK, am.GetVatCodes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("{vatCodeId:int}")]
        public IHttpActionResult GetVatCode(int vatCodeId)
        {
            return Content(HttpStatusCode.OK, am.GetVatCode(vatCodeId).ToDTO());
        }

        [HttpPost]
        [Route("VatCode")]
        public IHttpActionResult SaveVatCode(VatCodeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveVatCode(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{vatCodeId:int}")]
        public IHttpActionResult DeleteVatCode(int vatCodeId)
        {
            return Content(HttpStatusCode.OK, am.DeleteVatCode(vatCodeId));
        }
    }
}