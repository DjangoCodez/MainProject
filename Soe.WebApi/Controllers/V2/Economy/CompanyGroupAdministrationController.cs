using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/CompanyGroupAdministration")]
    public class CompanyGroupAdministrationController : SoeApiController
    {
        private readonly CompanyManager com;

        #region Constructor

        public CompanyGroupAdministrationController(CompanyManager com)
        {
            this.com = com;
        }

        #endregion

        #region CompanyGroupAdministration

        [HttpGet]
        [Route("Grid/{companyGroupAdministrationId:int?}")]
        public IHttpActionResult GetCompanyGroupAdministrationGrid(int? companyGroupAdministrationId = null)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupAdministrationList(base.ActorCompanyId, companyGroupAdministrationId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{companyGroupAdministrationId:int}")]
        public IHttpActionResult GetCompanyGroupAdministration(int companyGroupAdministrationId)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupAdministration(base.ActorCompanyId, companyGroupAdministrationId).ToDTO());
        }

        [HttpGet]
        [Route("ConsolidatingAccounting/ChildCompaniesDict/")]
        public IHttpActionResult GetGetChildCompaniesDict()
        {
            return Content(HttpStatusCode.OK, com.GetChildCompaniesByLicenseDict(base.LicenseId, base.ActorCompanyId, true, true));
        }

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupMappingHeadsDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetCompanyGroupMappingHeadsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupMappingHeadsDict(base.ActorCompanyId, addEmptyRow));
        }

        [HttpPost]
        [Route("CompanyGroupAdministration")]
        public IHttpActionResult SaveCompanyGroupAdministration(CompanyGroupAdministrationDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.SaveCompanyGroupAdministration(base.ActorCompanyId, model, true));
        }

        [HttpDelete]
        [Route("{companyGroupAdministrationId:int}")]
        public IHttpActionResult DeleteCompanyGroupAdministration(int companyGroupAdministrationId)
        {
            return Content(HttpStatusCode.OK, com.DeleteCompanyGroupAdministration(base.ActorCompanyId, companyGroupAdministrationId));
        }

        #endregion
    }
}
