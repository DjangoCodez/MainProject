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
using Soe.WebApi.Binders;
using Soe.WebApi.Models;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Account")]
    public class SysAccountStdController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;
        #endregion

        #region Constructor

        public SysAccountStdController(AccountManager am)
        {
            this.am = am;
        }

        #endregion
        #region SysAccountStd

        [HttpGet]
        [Route("SysAccountStd/{sysAccountStdTypeId:int}/{accountNr}")]
        public IHttpActionResult GetSysAccountStd(int sysAccountStdTypeId, string accountNr)
        {
            if (sysAccountStdTypeId == 0)
                sysAccountStdTypeId = am.GetSysAccountStdTypeParentIdForStandardDim() ?? 0;

            return Content(HttpStatusCode.OK, am.GetSysAccountStd(sysAccountStdTypeId, accountNr, false).ToDTO(false));
        }

        [HttpGet]
        [Route("SysAccountStd/Copy/{sysAccountStdId:int}")]
        public IHttpActionResult CopySysAccountStd(int sysAccountStdId)
        {
            return Content(HttpStatusCode.OK, am.ImportSysAccountStd(base.ActorCompanyId, sysAccountStdId).ToDTO(includeAccountDim: true));
        }

        #endregion
    }
}