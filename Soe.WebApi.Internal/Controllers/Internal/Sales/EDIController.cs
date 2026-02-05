using Soe.Edi.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.Sales
{
    [RoutePrefix("Internal/Sales/EDI")]
    public class EdiMessageController : ApiBase
    {
        const string LoginName = "SoftOne (EDI)";

        #region Constructor

        public EdiMessageController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        #region Save

        /// <summary>
        /// Save Edi messages according to SoftOnes format.
        /// </summary>
        /// <param name="connectApiKey"></param>
        /// <param name="sysEdiMessageHeadDTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("EdiMessages")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEdiMessages(string connectApiKey, int actorCompanyId, List<SysEdiMessageHeadDTO> sysEdiMessageHeadDTOs)
        {
            var importExportManager = new ImportExportManager(GetParameterObject(actorCompanyId, 0, null, LoginName));
            var ediMessageItem = new EdiMessageItem
            {
                sysEdiMessageHeadDTOs = new List<SysEdiMessageHeadDTO>()
            };

            ediMessageItem.sysEdiMessageHeadDTOs.AddRange(sysEdiMessageHeadDTOs);
            var result = importExportManager.ImportEdiMessage(ediMessageItem, TermGroup_IOImportHeadType.EdiMessage, TermGroup_IOSource.Connect, TermGroup_IOType.XEConnect, actorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }
       
        #endregion

        #endregion

    }
}