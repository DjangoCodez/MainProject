using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;

namespace Soe.WebApi.Controllers
{
    public class TranslationController : ApiController
    {
        [Route("translation/{lang}/{part}")]
        [HttpGet]
        public IHttpActionResult GetPart(string lang, string part)
        {
            try
            {
                TermManager tm = new TermManager(null);
                Dictionary<string, string> terms = tm.GetAngularSysTermPart(lang, part);
                return Content(HttpStatusCode.OK, terms);
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, new WebApiError("error.default_error", ex.GetBaseException().Message)));
            }

        }
    }
}
