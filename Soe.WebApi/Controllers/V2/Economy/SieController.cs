using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Sie;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Sie")]
    public class SieController : SoeApiController
    {
        #region Variables
        private readonly SieManager sm;
        private readonly ImportExportManager iem;
        #endregion

        #region Constructor
        public SieController(SieManager sm, ImportExportManager iem)
        {
            this.sm = sm;
            this.iem = iem;
        }
        #endregion

        #region Export
        [HttpPost]
        [Route("Export")]
        public IHttpActionResult SieExport(SieExportDTO exportDTO)
        {
            exportDTO.LoginName = ParameterObject.LoginName;
            exportDTO.ActorCompanyId = this.ActorCompanyId;
            exportDTO.Program = Constants.APPLICATION_NAME;
            exportDTO.Version = Constants.APPLICATION_VERSION;

            return Content(HttpStatusCode.OK, sm.Export(exportDTO));
        }

        #endregion

        #region Import
        [HttpPost]
        [Route("Import")]
        public IHttpActionResult SieImport(SieImportDTO importDTO)
        {
            return Content(HttpStatusCode.OK, sm.Import(this.ActorCompanyId, this.UserId, importDTO));
        }
        
        [HttpPost]
        [Route("Import/ReadFile")]
        public IHttpActionResult SieImportReadFile(FileDTO file)
        {
            return Content(HttpStatusCode.OK, sm.SieImportPreview(this.ActorCompanyId, file));
        }

        [HttpGet]
        [Route("Import/History")]
        public IHttpActionResult GetSieImportHistory()
        {
            return Content(HttpStatusCode.OK, iem.GetFileImportGrid(this.ActorCompanyId));
        }

        [HttpPost]
        [Route("Import/Reverse")]
        public IHttpActionResult ReverseImport(SieReverseImportDTO importReverseRequest)
        { 
            return Content(HttpStatusCode.OK, sm.ReverseSieImport(this.ActorCompanyId, importReverseRequest));
        }
        #endregion
    }
}