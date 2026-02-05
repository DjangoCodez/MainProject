using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class SystemCountryController : SoeApiController
    {
        #region Variables

        private readonly CountryCurrencyManager ccm;

        #endregion

        #region Constructor

        public SystemCountryController(CountryCurrencyManager ccm)
        {
            this.ccm = ccm;
        }

        #endregion

        #region SysCountry

        [HttpGet]
        [Route("SysCountry/{addEmptyRow:bool}/{onlyUsedLanguages:bool}")]
        public IHttpActionResult GetSysCountries(bool addEmptyRow, bool onlyUsedLanguages)
        {
            return Content(HttpStatusCode.OK, ccm.GetSysCountriesDict(addEmptyRow, onlyUsedLanguages).ToSmallGenericTypes());
        }

        #endregion
    }
}