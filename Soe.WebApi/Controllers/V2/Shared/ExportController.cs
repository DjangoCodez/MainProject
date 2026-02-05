using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Shared
{
    [RoutePrefix("V2/Shared/Export")]
    public class ExportController : SoeApiController
    {
        #region Variables

        private readonly ImportExportManager iem;

        #endregion

        #region Constructor

        public ExportController(ImportExportManager iem)
        {
            this.iem = iem;
        }

        #endregion

        #region Export

        [HttpGet]
        [Route("Grid/{module:int}")]
        public IHttpActionResult GetExportsGrid(int module, [FromUri] int? exportId = null)
        {
            return Content(HttpStatusCode.OK, iem.GetExports(base.ActorCompanyId, (SoeModule)module, exportId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{exportId:int}")]
        public IHttpActionResult GetExport(int exportId)
        {
            return Content(HttpStatusCode.OK, iem.GetExport(base.ActorCompanyId, exportId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveExport(ExportDTO model)
        {
            return Content(HttpStatusCode.OK, iem.SaveExport(base.ActorCompanyId, model));
        }

        [HttpDelete]
        [Route("{exportId:int}")]
        public IHttpActionResult DeleteExport(int exportId)
        {
            return Content(HttpStatusCode.OK, iem.DeleteExport(base.ActorCompanyId, exportId));
        }

        #endregion

        #region ExportDefinition

        [HttpGet]
        [Route("ExportDefinition/Grid")]
        public IHttpActionResult GetExportDefinitionsGrid(int? exportDefinitionId = null)
        {
            return Content(HttpStatusCode.OK, iem.GetExportDefinitions(base.ActorCompanyId, exportDefinitionId).ToGridDTOs(null));
        }

        [HttpGet]
        [Route("ExportDefinition/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetExportDefinitionsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, iem.GetExportDefinitionsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ExportDefinition/{exportDefinitionId:int}")]
        public IHttpActionResult GetExportDefinition(int exportDefinitionId)
        {
            return Content(HttpStatusCode.OK, iem.GetExportDefinition(base.ActorCompanyId, exportDefinitionId, true).ToDTO(true));
        }


        [HttpPost]
        [Route("ExportDefinition")]
        public IHttpActionResult SaveExportDefinition(ExportDefinitionDTO model)
        {
            return Content(HttpStatusCode.OK, iem.SaveExportDefinition(base.ActorCompanyId, model));
        }

        //[HttpDelete]
        //[Route("ExportDefinition/{exportDefinitionId:int}")]
        //public IHttpActionResult DeleteExportDefinition(int exportDefinitionId)
        //{
        //    return Content(HttpStatusCode.OK, iem.DeleteExportDefinition(exportDefinitionId));
        //}

        #endregion
    }
}