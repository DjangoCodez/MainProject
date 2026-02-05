using System.Net;
using System.Web.Http;
using Soe.Sys.Common.DTO;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/System")]
    [SupportUserAuthorize]
    public class SysCompanyController : SoeApiController
    {
        #region Variables

        private readonly SysServiceManager ssm;

        #endregion

        #region Constructor

        public SysCompanyController(SysServiceManager ssm)
        {
            this.ssm = ssm;
        }

        #endregion

        #region SysCompany

        [HttpGet]
        [Route("SysCompany/Grid/{sysCompanyId:int?}")]
        public IHttpActionResult GetSysCompanies(int? sysCompanyId=null)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompanies(sysCompanyId));
        }
        [HttpGet]
        [Route("SysCompany/SysCompanyDict")]
        public IHttpActionResult GetSysCompanyDict()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompanyDict());
        }

        [HttpGet]
        [Route("SysCompany/{sysCompanyId}")]
        public IHttpActionResult GetSysCompany(int sysCompanyId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompany(sysCompanyId, true, true, true));
        }

        [HttpGet]
        [Route("SysCompany/{companyApiKey}/{sysCompDbId}")]
        public IHttpActionResult GetSysCompanyByApiKey(string companyApiKey, int sysCompDbId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompany(companyApiKey, sysCompDbId));
        }

        [HttpPost]
        [Route("SysCompany/")]
        public IHttpActionResult SaveSysCompany(SysCompanyDTO sysCompanyDTO)
        {
            return Content(HttpStatusCode.OK, ssm.SaveSysCompany(sysCompanyDTO, 0));
        }

        #endregion

    }
}