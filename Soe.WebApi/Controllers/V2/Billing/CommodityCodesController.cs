using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using static Soe.WebApi.Controllers.CoreController;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/CommodityCodes")]
    public class CommodityCodesController : SoeApiController
    {
        #region Variables

        private readonly CommodityCodeManager cm;
        private readonly ExcelImportManager eim;

        #endregion

        #region Constructor

        public CommodityCodesController(CommodityCodeManager cm, ExcelImportManager eim)
        {
            this.cm = cm;
            this.eim= eim;
        }

        #endregion

        #region CommodityCodes

        [HttpGet]
        [Route("{onlyActive:bool}")]
        public IHttpActionResult GetCustomerCommodyCodes(bool onlyActive)
        {
            return Content(HttpStatusCode.OK, cm.GetCustomerCommodityCodes(base.ActorCompanyId, onlyActive));
        }

        [HttpGet]
        [Route("Dict/{addEmpty:bool}")]
        public IHttpActionResult GetCustomerCommodyCodesDict(bool addEmpty)
        {
            return Content(HttpStatusCode.OK, cm.GetCustomerCommodityCodesDict(base.ActorCompanyId, addEmpty));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveCustomerCommodityCodes(UpdateEntityStatesModel model)
        {
            return Content(HttpStatusCode.OK, cm.SaveCustomerCommodityCodes(model.Dict, base.ActorCompanyId));
        }

        #endregion
    }
}