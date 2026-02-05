using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/SysPriceList")]
    public class SysPriceListController : SoeApiController
    {
        #region Variables

        private readonly SysPriceListManager splm;

        #endregion

        #region Constructor

        public SysPriceListController(SysPriceListManager splm)
        {
            this.splm = splm;
        }

        #endregion

        [HttpGet]
        [Route("SysPriceListPrivider")]
        public IHttpActionResult GetSysPriceProvider()
        {
            return Content(HttpStatusCode.OK, SysPriceListManager.GetSysPriceListProviders());
        }

        [HttpGet]
        [Route("SysPriceList")]
        public IHttpActionResult GetSysPriceListHeads()
        {
            return Content(HttpStatusCode.OK, splm.GetSysPriceListHeadGridDTOs());
        }

        [HttpPost]
        [Route("Import")]
        [SupportUserAuthorize]
        public IHttpActionResult SysPriceListImport(SysPriceListImportDTO importDTO)
        {
            return Content(HttpStatusCode.OK, splm.SysPriceListImport(this.ActorCompanyId, this.UserId, importDTO));
        }

    }
}