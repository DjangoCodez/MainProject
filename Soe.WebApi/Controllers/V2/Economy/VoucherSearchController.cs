using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/VoucherSearch")]
    public class VoucherSearchController: SoeApiController
    {
        #region Variables
        private readonly VoucherManager vm;
        #endregion

        #region Constructor
        public VoucherSearchController(VoucherManager vm)
        {
            this.vm = vm;
        }
        #endregion

        #region VoucherSearch
        [HttpPost]
        [Route("")]
        public IHttpActionResult getSearchedVoucherRows(SearchVoucherRowsAngDTO dto)
        {
            return Content(HttpStatusCode.OK, vm.SearchVoucherRowsDto(base.ActorCompanyId, dto));
        }
        #endregion

    }
}