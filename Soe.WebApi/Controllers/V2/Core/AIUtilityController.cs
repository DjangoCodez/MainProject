using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneAI;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/AI-utility")]
    public class AIUtilityController : SoeApiController
    {
        AIManager aim;
        public AIUtilityController(AIManager aim)
        {

            this.aim = aim;
        }

        [HttpGet]
        [Route("translations")]
        public IHttpActionResult GetTranslationSuggestions(string originalText, string languages)
        {
            var languagesArray = languages.Split(',').Select(l => int.Parse(l)).ToList();
            return Content(HttpStatusCode.OK, aim.TranslateText(originalText, languagesArray.Select(l => (TermGroup_Languages)l).ToList()));
        }

        [HttpGet]
        [Route("professionalized")]
        public IHttpActionResult GetProfessionalizedText(string text)
        {
            return Content(HttpStatusCode.OK, aim.ProfessionalizeText(text));
        }

    }
}