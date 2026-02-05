using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Registry/MessageGroup")]
    public class MessageGroupController : SoeApiController
    {
        #region Variables
        private readonly CommunicationManager com;
        private readonly UserManager um;

        #endregion

        #region Constructor

        public MessageGroupController(CommunicationManager com, UserManager um)
        {  
            this.com = com;
            this.um = um;
        }

        #endregion

        #region MessageGroups

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetMessageGroups()
        {
            return Content(HttpStatusCode.OK, com.GetMessageGroups(base.ActorCompanyId, base.UserId));
        }

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetMessageGroupsGrid()
        {
            return Content(HttpStatusCode.OK, com.GetMessageGroupsForGrid(base.ActorCompanyId, base.UserId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetMessageGroupsDict()
        {
            return Content(HttpStatusCode.OK, com.GetMessageGroupsDict(base.ActorCompanyId, base.UserId));
        }

        [HttpGet]
        [Route("UsersByAccount/{accountId:int}")]
        public IHttpActionResult GetMessageGroupUsersByAccount(int accountId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByAccounts(base.ActorCompanyId, accountId.ObjToList()).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UsersByCategory/{categoryId:int}")]
        public IHttpActionResult GetMessageGroupUsersByCategory(int categoryId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByCategory(base.ActorCompanyId, base.RoleId, base.UserId, categoryId).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UsersByEmployeeGroup/{employeeGroupId:int}")]
        public IHttpActionResult GetMessageGroupUsersByEmployeeGroup(int employeeGroupId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByEmployeeGroup(base.ActorCompanyId, base.RoleId, base.UserId, employeeGroupId).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UsersByRole/{roleId:int}")]
        public IHttpActionResult GetMessageGroupUsersByRole(int roleId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByRole(base.ActorCompanyId, roleId, base.UserId).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{messageGroupId:int}")]
        public IHttpActionResult GetMessageGroup(int messageGroupId)
        {
            return Content(HttpStatusCode.OK, com.GetMessageGroup(messageGroupId).ToDTO(true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveMessageGroup(MessageGroupDTO messageGroupDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.SaveMessageGroup(messageGroupDTO));
        }

        [HttpDelete]
        [Route("{messageGroupId:int}")]
        public IHttpActionResult DeleteMessageGroup(int messageGroupId)
        {
            return Content(HttpStatusCode.OK, com.DeleteMessageGroup(messageGroupId));
        }

        #endregion
    }
}