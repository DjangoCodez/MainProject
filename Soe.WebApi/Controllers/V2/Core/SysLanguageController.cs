using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class SystemLanguageController : SoeApiController
    {
        #region Variables

        private readonly LanguageManager lm;

        #endregion

        #region Constructor

        public SystemLanguageController(LanguageManager lm)
        {
            this.lm = lm;
        }

        #endregion

        #region SysLanguage

        [HttpGet]
        [Route("SysLanguage/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysLanguages(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, lm.GetSysLanguageDict(addEmptyRow).ToSmallGenericTypes());
        }

        #endregion
    }
}