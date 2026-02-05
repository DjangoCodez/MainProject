using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/UserSelection")]
    public class UserSelectionController : SoeApiController
    {
        #region Variables
        
        private readonly UserManager um;

        #endregion

        #region Constructor

        public UserSelectionController(UserManager um)
        {
            this.um = um;
        }

        #endregion

        #region UserSelection

        [HttpGet]
        [Route("List/{type:int}")]
        public IHttpActionResult GetUserSelections(int type)
        {
            return Content(HttpStatusCode.OK, um.GetUserSelections((UserSelectionType)type, base.UserId, base.RoleId, base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("Dict/{type:int}")]
        public IHttpActionResult GetUserSelectionsDict(int type)
        {
            return Content(HttpStatusCode.OK, um.GetUserSelectionsDict((UserSelectionType)type, base.UserId, base.RoleId, base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{userSelectionId:int}")]
        public IHttpActionResult GetUserSelection(int userSelectionId)
        {
            return Content(HttpStatusCode.OK, um.GetUserSelection(userSelectionId, loadAccess: true).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveUserSelection(UserSelectionDTO dto)
        {
            return Content(HttpStatusCode.Accepted, um.SaveUserSelection(dto));
        }

        [HttpDelete]
        [Route("{userSelectionId:int}")]
        public IHttpActionResult DeleteUserSelection(int userSelectionId)
        {
            return Content(HttpStatusCode.Accepted, um.DeleteUserSelection(userSelectionId));
        }

        #endregion
    }
}