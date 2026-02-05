using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Inventory")]
    public class InventoryAccountController : SoeApiController
    {
        #region Variables

        private readonly SettingManager sm;

        #endregion

        #region Constructor

        public InventoryAccountController(SettingManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region Accounts

        [HttpGet]
        [Route("Accounts/InventoryTriggerAccounts")]
        public IHttpActionResult GetInventoryTriggerAccounts()
        {
            return Content(HttpStatusCode.OK, sm.GetInventoryEditTriggerAccountsFromSettings(base.ActorCompanyId).ToList());
        }

        #endregion
    }
}