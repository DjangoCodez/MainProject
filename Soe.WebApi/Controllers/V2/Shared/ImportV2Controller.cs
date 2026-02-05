using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Import;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Shared
{
    [RoutePrefix("V2/Shared/Import")]
    public class ImportV2Controller : SoeApiController
    {
        #region Variables

        private readonly ExcelImportManager eim;

        #endregion

        #region Constructor

        public ImportV2Controller(ExcelImportManager eim)
        {
            this.eim = eim;
        }

        #endregion

        #region Excel

        [HttpGet]
        [Route("ExcelGrid")]
        public IHttpActionResult GetExcelImportGrid()
        {
            return Content(HttpStatusCode.OK, eim.GetExcelImportTemplates());
        }

        [HttpPost]
        [Route("ExcelGrid")]
        public IHttpActionResult ImportExcelFile(ExcelImportDTO model)
        {
            return Content(HttpStatusCode.OK, eim.ImportExcelFromAngular(model));
        }


        #endregion
    }
}