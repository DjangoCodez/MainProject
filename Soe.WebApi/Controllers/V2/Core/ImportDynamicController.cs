using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SoftOne.Soe.Common.DTO;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/ImportDynamic")]
    public class ImportDynamicController : SoeApiController
    {
        #region Variables
        private readonly ImportDynamicManager idm;
        #endregion

        #region Constructor
        public ImportDynamicController(ImportDynamicManager idm)
        {
            this.idm = idm;
        }
        #endregion

        #region Endponits
        [HttpPost]
        [Route("GetFileContent/{fileType:int}")]
        public IHttpActionResult GetFileContent(int fileType, [FromBody] ImportDynamicFileUploadDTO uploadFile)
        {
            if (uploadFile.File != null)
            {
                //var data = await Request.Content.ParseMultipartAsync();
                //Extensions.HttpPostedFile file = data?.Files["file"];
                var result = idm.GetFileContent(fileType, uploadFile?.File, uploadFile?.FileName);

                return Content(HttpStatusCode.OK, result);
            }
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("ParseRows")]
        public IHttpActionResult ParseRows(ParseRowsModel model)
        {
            return Content(HttpStatusCode.OK, idm.ParseRows(model.Fields, model.Options, model.Data));
        }
        #endregion
    }
}