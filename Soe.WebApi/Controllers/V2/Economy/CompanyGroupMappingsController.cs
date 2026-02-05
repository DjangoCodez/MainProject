
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Util;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Accounting/ConsolidatingAccounting/CompanyGroupMappingHeads")]
    public class CompanyGroupMappingsController : SoeApiController
    {
        private readonly CompanyManager com;

        #region Constructor

        public CompanyGroupMappingsController(CompanyManager com)
        {
            this.com = com;
        }

        #endregion


        #region CompanyGroupMappings

        [HttpGet]
        [Route("Grid/{companyGroupMappingHeadId:int?}")]
        public IHttpActionResult GetCompanyGroupMappingHeads(int? companyGroupMappingHeadId = null)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupMappingHeadList(base.ActorCompanyId, companyGroupMappingHeadId).ToDTOs(false));
        }

        [HttpGet]
        [Route("{companyGroupMappingHeadId:int}")]
        public IHttpActionResult GetCompanyGroupMappingHead(int companyGroupMappingHeadId)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupMapping(companyGroupMappingHeadId, true).ToDTO(true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveCompanyGroupMappingHead(CompanyGroupMappingHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.SaveCompanyGroupMapping(model, model.Rows, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{companyGroupMappingHeadId:int}")]
        public IHttpActionResult DeleteCompanyGroupMappingHead(int companyGroupMappingHeadId)
        {
            return Content(HttpStatusCode.OK, com.DeleteCompanyGroupMapping(companyGroupMappingHeadId));
        }

        [HttpGet]
        [Route("Exists/CompanyGroupMappingHeadNumber/{companyGroupMappingHeadId:int}/{companyGroupMappingHeadNumber:int}")]
        public IHttpActionResult CheckCompanyGroupMappingHeadNumberIsExists(int companyGroupMappingHeadId,int companyGroupMappingHeadNumber)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.IsCompanyGroupMappingHeadNumberExists(companyGroupMappingHeadId, companyGroupMappingHeadNumber, base.ActorCompanyId));
        }

        #endregion
    }
}
