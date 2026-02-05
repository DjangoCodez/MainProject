using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/Company")]
    public class CompanyController : SoeApiController
    {        
        #region Variables

        private readonly CompanyManager cm;

        #endregion

        #region Constructor

        public CompanyController(CompanyManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region Company

        [HttpGet]        
        [Route("CompaniesByUser/")]
        public IHttpActionResult GetCompaniesByUser()
        {
            return Content(HttpStatusCode.OK, cm.GetCompaniesByUserDict(base.UserId, base.LicenseId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Company/{actorCompanyId:int}")]
        public IHttpActionResult GetCompany(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, cm.GetCompanyEdit(actorCompanyId));
        }

        [HttpGet]
        [Route("ByLicense/{licenseId:int}")]
        public IHttpActionResult GetCompaniesByLicense(int licenseId)
        {
            return Content(HttpStatusCode.OK, cm.GetCompaniesByLicense(licenseId).ToCompanyDTOs());
        }

        [HttpGet]
        [Route("ByLicense/{licenseId:int}/{onlyTemplates:bool}")]
        public IHttpActionResult GetCompaniesByLicense(string licenseId,bool onlyTemplates)
        {
            return Content(HttpStatusCode.OK, cm.GetCompaniesByLicense(licenseId, onlyTemplates).ToCompanyDTOs());
        }

        [HttpPost]
        [Route("Company/")]
        public IHttpActionResult SaveCompany(CompanyEditDTO dto)
        {
            return Content(HttpStatusCode.OK, cm.SaveCompany(dto, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Company/{actorCompanyId:int}")]
        public IHttpActionResult DeleteCompany(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteCompany(actorCompanyId));
        }

        #endregion

        #region Template

        [HttpGet]
        [Route("Template/Companies/{licenseId:int}")]
        public IHttpActionResult GetTemplateCompanies(int licenseId)
        {
            return Content(HttpStatusCode.OK, cm.GetTemplateCompaniesDict(licenseId, true, true));
        }

        [HttpGet]
        [Route("Template/GlobalCompanies")]
        public IHttpActionResult GetGlobalTemplateCompanies()
        {
            return Content(HttpStatusCode.OK, cm.GetGlobalTemplateCompanies().ToCompanyDTOs());
        }

        [HttpPost]
        [Route("Template/Copy/")]
        public IHttpActionResult CopyFromTemplateCompany(CopyFromTemplateCompanyInputDTO dto)
        {
            return Content(HttpStatusCode.OK, cm.CopyFromTemplateCompany(dto));
        }

        #endregion
    }
}