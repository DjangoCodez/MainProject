using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/SysWholeseller")]
    public class SysWholesellerController : SoeApiController
    {
        #region Variables

        private readonly WholeSellerManager wm;

        #endregion

        #region Constructor

        public SysWholesellerController(WholeSellerManager wm)
        {
            this.wm = wm;
        }

        #endregion


        #region SysWholeseller

        [HttpGet]
        [Route("SysWholeseller/{sysWholesellerId:int}/{loadSysWholesellerEdi:bool}/{loadSysEdiMsg:bool}/{loadSysEdiType:bool}")]
        public IHttpActionResult GetSysWholeseller(int sysWholesellerId, bool loadSysWholesellerEdi, bool loadSysEdiMsg, bool loadSysEdiType)
        {
            return Content(HttpStatusCode.OK, wm.GetSysWholeseller(sysWholesellerId, loadSysWholesellerEdi, loadSysEdiMsg, loadSysEdiType).ToDTO());
        }

        [HttpGet]
        [Route("SysWholesellers/Small/")]
        public IHttpActionResult GetSmallGenericSysWholesellers(bool addEmptyRow)
        {
                return Content(HttpStatusCode.OK, wm.GetWholesellerDictByCompany(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());

        }

        [HttpGet]
        [Route("SysWholesellers/Small/All")]
        public IHttpActionResult GetSmallGenericSysWholesellersAll()
        {
            return Content(HttpStatusCode.OK, wm.GetSysWholesellerDict().ToSmallGenericTypes());

        }

        [HttpGet]
        [Route("SysWholesellers/")]
        public IHttpActionResult GetSysWholesellers()
        {
            return Content(HttpStatusCode.OK, wm.GetSysWholesellersByCompany(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("SysWholesellersByCompany/{onlyNotUsed:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysWholesellersByCompany(bool onlyNotUsed, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, wm.GetSysWholesellersByCompanyDict(base.ActorCompanyId, onlyNotUsed, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

    }
}