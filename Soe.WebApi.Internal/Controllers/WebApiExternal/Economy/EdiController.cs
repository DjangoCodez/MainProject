using Soe.Edi.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Economy/EDI")]
    public class EdiController : WebApiExternalBase
    {
        #region Variables

        private ConnectUtil connectUtil;

        #endregion

        #region Constructor

        public EdiController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.connectUtil = new ConnectUtil(null);
        }

        #endregion

        #region Methods

        #region Save

        /// <summary>
        /// Save Edi message according to SoftOnes format.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysEdiMessageHeadDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("edimessage/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEdiMessage(Guid companyApiKey, Guid connectApiKey, string token, SysEdiMessageHeadDTO sysEdiMessageHeadDTO)
        {
            #region Validation

            ParameterObject parameterObject = ParameterObject.Empty();

            int actorCompanyId = 0;
            string validatationResult = connectUtil.ValidateToken(companyApiKey, connectApiKey, token, out parameterObject, out actorCompanyId);

            if (!string.IsNullOrEmpty(validatationResult))
                return Content(HttpStatusCode.Unauthorized, validatationResult);

            #endregion

            var importExportManager = new ImportExportManager(parameterObject);
            var ediMessageItem = new EdiMessageItem
            {
                sysEdiMessageHeadDTOs = new List<SysEdiMessageHeadDTO>()
            };
            ediMessageItem.sysEdiMessageHeadDTOs.Add(sysEdiMessageHeadDTO);

            var result = importExportManager.ImportEdiMessage(ediMessageItem, TermGroup_IOImportHeadType.EdiMessage, TermGroup_IOSource.Connect, TermGroup_IOType.XEConnect, actorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }


        /// <summary>
        /// Save Edi messages according to SoftOnes format.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysEdiMessageHeadDTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("edimessages/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEdiMessages(Guid companyApiKey, Guid connectApiKey, string token, List<SysEdiMessageHeadDTO> sysEdiMessageHeadDTOs)
        {
            #region Validation

            ParameterObject parameterObject = ParameterObject.Empty();

            int actorCompanyId = 0;
            string validatationResult = connectUtil.ValidateToken(companyApiKey, connectApiKey, token, out parameterObject, out actorCompanyId);

            if (!string.IsNullOrEmpty(validatationResult))
                return Content(HttpStatusCode.Unauthorized, validatationResult);

            #endregion

            var importExportManager = new ImportExportManager(parameterObject);
            var ediMessageItem = new EdiMessageItem
            {
                sysEdiMessageHeadDTOs = new List<SysEdiMessageHeadDTO>()
            };
            
            ediMessageItem.sysEdiMessageHeadDTOs.AddRange(sysEdiMessageHeadDTOs);
            var result = importExportManager.ImportEdiMessage(ediMessageItem, TermGroup_IOImportHeadType.EdiMessage, TermGroup_IOSource.Connect, TermGroup_IOType.XEConnect, actorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Save Edi messages according to SoftOnes format.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sysEdiMessageHeadDTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("edimessage/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEdiMessage(Guid? companyApiKey, SysEdiMessageHeadDTO sysEdiMessageHeadDTO)
        {
            var importExportManager = new ImportExportManager(null);
            var result = importExportManager.ImportEdiMessage(companyApiKey, sysEdiMessageHeadDTO);
            return Content(HttpStatusCode.OK, result);
        }


        /// <summary>
        /// Send information where to fetch message, message will be fetch right away and validated and saved.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <param name="wholeSeller"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ediUrl/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveEdiMessageURL(Guid companyApiKey, Guid connectApiKey, string token, string url, EdiUrlSource source, SysWholesellerEdiIdEnum wholeSeller)
        {
            #region Validation

            ParameterObject parameterObject = ParameterObject.Empty();

            int actorCompanyId = 0;
            string validatationResult = connectUtil.ValidateToken(companyApiKey, connectApiKey, token, out parameterObject, out actorCompanyId);

            if (!string.IsNullOrEmpty(validatationResult))
                return Content(HttpStatusCode.Unauthorized, validatationResult);

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(parameterObject);
            EdiManager EdiManager = new EdiManager(parameterObject);
            EdiMessageItem ediMessageItem = new EdiMessageItem();
            ediMessageItem.sysEdiMessageHeadDTOs = new List<SysEdiMessageHeadDTO>();
            
            var result = importExportManager.ImportEdiMessage(ediMessageItem, TermGroup_IOImportHeadType.EdiMessage, TermGroup_IOSource.Connect, TermGroup_IOType.XEConnect, actorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }


        #endregion

        #endregion

    }
}