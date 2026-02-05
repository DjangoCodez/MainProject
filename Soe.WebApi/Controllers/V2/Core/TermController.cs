using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class TermController : SoeApiController
    {
        #region Variables

        private readonly TermManager tm;

        #endregion

        #region Constructor

        public TermController(TermManager tm)
        {
            this.tm = tm;
        }

        #endregion

        #region Translation

        [Route("Translation/{lang}/{part}")]
        [HttpGet]
        public IHttpActionResult GetTranslationPart(string lang, string part)
        {
            try
            {
                Dictionary<string, string> terms = tm.GetAngularSysTermPart(lang, part);
                return Content(HttpStatusCode.OK, terms);
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, new WebApiError("error.default_error", ex.GetBaseException().Message)));
            }
        }


        [HttpGet]
        [Route("Translation/{recordType:int}/{recordId:int}/{loadLangName:bool}")]
        public IHttpActionResult GetTranslations(CompTermsRecordType recordType, int recordId, bool loadLangName)
        {
            return Content(HttpStatusCode.OK, tm.GetCompTermDTOs(recordType, recordId, loadLangName));
        }

        #endregion

        #region SysTermGroup

        [HttpGet]
        [Route("SysTermGroup/{sysTermGroupId:int}/{addEmptyRow:bool}/{skipUnknown:bool}/{sortById:bool}")]
        public IHttpActionResult GetTermGroupContent(int sysTermGroupId, bool addEmptyRow, bool skipUnknown, bool sortById)
        {
            return Content(HttpStatusCode.OK, base.GetTermGroupContent((TermGroup)sysTermGroupId, addEmptyRow, skipUnknown, sortById).ToSmallGenericTypes());
        }

        #endregion
    }
}