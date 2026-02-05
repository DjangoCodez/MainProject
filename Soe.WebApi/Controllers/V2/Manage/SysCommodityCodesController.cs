using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using static Soe.WebApi.Controllers.CoreController;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/System")]
    public class SysCommodityCodesController : SoeApiController
    {
        #region Variables

        private readonly CommodityCodeManager cm;
        private readonly ExcelImportManager eim;

        #endregion

        #region Constructor

        public SysCommodityCodesController(CommodityCodeManager cm, ExcelImportManager eim)
        {
            this.cm = cm;
            this.eim = eim; 
        }

        #endregion


        #region CommodityCodes

        [HttpGet]
        [Route("CommodityCodes/{onlyActive:bool}")]
        public IHttpActionResult GetCustomerCommodyCodes(bool onlyActive)
        {
            return Content(HttpStatusCode.OK, cm.GetCustomerCommodityCodes(base.ActorCompanyId, onlyActive));
        }

        [HttpGet]
        [Route("CommodityCodes/Dict/{addEmpty:bool}")]
        public IHttpActionResult GetCustomerCommodyCodesDict(bool addEmpty)
        {
            return Content(HttpStatusCode.OK, cm.GetCustomerCommodityCodesDict(base.ActorCompanyId, addEmpty));
        }

        [HttpPost]
        [Route("CommodityCodes/")]
        public IHttpActionResult SaveCustomerCommodityCodes(UpdateEntityStatesModel model)
        {
            return Content(HttpStatusCode.OK, cm.SaveCustomerCommodityCodes(model.Dict, base.ActorCompanyId));
        }

        //#endregion

        //#region Intrastat - CommodityCodes

        [HttpGet]
        [Route("CommodityCodes/{langId:int}")]
        public IHttpActionResult GetCommodyCodes(int langId)
        {
            return Content(HttpStatusCode.OK, cm.GetSysIntrastatCodesDTOs(langId));
        }


        [HttpPost]
        [SupportUserAuthorize]
        [Route("Files/Intrastat/CommodityCodes")]
        public IHttpActionResult UploadCommodityCodesFile(CommodityCodeUploadDTO model)
        {
            Byte[] bytes = Convert.FromBase64String(model.FileString);
            string fileName = "";
            string pathOnServer = "";
            //var file = await UploadedFileHandler.HandleAsync(Request);
            var result = new ActionResult();
            if (bytes.Length > 0)
            {
                //has content
                var startingDate = new DateTime(model.Year, 1, 1);

                //Validate
                fileName = eim.ValidatePostedFile(model.FileName, true);

                var extention = Path.GetExtension(model.FileName);
                if (!(extention == ".csv" || extention == ".xlsx" || extention == ".xls"))
                {
                    result.Success = false;
                    result.ErrorMessage = eim.GetFileNotSupportedMessage();
                    return Ok(result);
                }
                if (!(extention == ".csv"))
                {
                    fileName = Path.ChangeExtension(fileName, ".csv");
                }
                //Save temp-file
                pathOnServer = eim.SaveTempFileToServer(bytes, fileName);

                result = eim.ImportCommodityCodes(pathOnServer, startingDate, extention);
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = eim.GetFileIsEmptyMessage();
            }

            result.StringValue = model.FileName;
            return Ok(result);
            
        }

        #endregion

        
    }
}