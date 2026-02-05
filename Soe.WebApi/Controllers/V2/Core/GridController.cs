using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class GridController: SoeApiController
    {
        #region Variables

        private readonly SettingManager sm;

        #endregion

        #region Constructor

        public GridController(SettingManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region SysGridState

        [HttpGet]
        [Route("SysGridState/{grid}")]
        public IHttpActionResult GetSysGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.GetSysGridStateValue(grid));
        }

        [HttpPost]
        [Route("SysGridState")]
        public IHttpActionResult SaveSysGridState(SaveUserGridStateModel model)
        {
            return Content(HttpStatusCode.OK, sm.SaveSysGridState(model.Grid, model.GridState, base.UserId));
        }

        [HttpDelete]
        [Route("SysGridState/{grid}")]
        public IHttpActionResult DeleteSysGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.DeleteSysGridState(grid, base.UserId));
        }

        #endregion

        #region UserGridState

        [HttpGet]
        [Route("UserGridState/{grid}")]
        public IHttpActionResult GetUserGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.GetUserGridStateValue(grid));
        }

        [HttpPost]
        [Route("UserGridState")]
        public IHttpActionResult SaveUserGridState(SaveUserGridStateModel model)
        {
            return Content(HttpStatusCode.OK, sm.SaveUserGridState(model.Grid, model.GridState));
        }

        [HttpDelete]
        [Route("UserGridState/{grid}")]
        public IHttpActionResult DeleteUserGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.DeleteUserGridState(grid));
        }

        #endregion
    }
}