using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/MatchCode")]
    public class MatchCodeController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public MatchCodeController(InvoiceManager _im)
        {
            this.im = _im;
        }

        #endregion

        #region MatchCode

        [HttpGet]
        [Route("Grid/{matchCodeId:int?}")]
        public IHttpActionResult GetMatchCodesGrid(int? matchCodeId = null)
        {
            return Content(HttpStatusCode.OK, im.GetMatchCodesForGrid(base.ActorCompanyId, null, matchCodeId));
        }

        [HttpGet]
        [Route("ByType/{matchCodeType:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetMatchCodes(SoeInvoiceMatchingType matchCodeType, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetMatchCodes(base.ActorCompanyId, matchCodeType, addEmptyRow).ToDTOs());
        }

        [HttpGet]
        [Route("Dict/{type:int}")]
        public IHttpActionResult GetMatchCodesDict(int type)
        {
            SoeInvoiceMatchingType mt = (SoeInvoiceMatchingType)Enum.ToObject(typeof(SoeInvoiceMatchingType), type);
            return Content(HttpStatusCode.OK, im.GetMatchCodesDict(base.ActorCompanyId, mt, true).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{matchCodeId:int}")]
        public IHttpActionResult GetMatchCode(int matchCodeId)
        {
            return Content(HttpStatusCode.OK, im.GetMatchCodeDto(matchCodeId, true));
        }

        [HttpPost]
        [Route("MatchCode")]
        public IHttpActionResult SaveMatchCode(MatchCodeDTO matchCodeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveMatchCode(matchCodeDTO));
        }

        [HttpDelete]
        [Route("{matchCodeId:int}")]
        public IHttpActionResult DeleteMatchCode(int matchCodeId)
        {
            return Content(HttpStatusCode.OK, im.DeleteMatchCode(matchCodeId));
        }

        #endregion
    }
}