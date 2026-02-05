using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class ContactAddressController : SoeApiController
    {
        #region Variables
        private readonly ContactManager com;
        private readonly UserManager um;
        #endregion

        #region Constructor

        public ContactAddressController(ContactManager com, UserManager um)
        {
            this.com = com;
            this.um = um;
        }

        #endregion

        #region API Endpoints

        [HttpGet]
        [Route("ContactAddress/{actorId:int}/{type:int}/{addEmptyRow:bool}/{includeRows:bool}/{includeCareOf:bool}")]
        public IHttpActionResult GetContactAddresses(int actorId, TermGroup_SysContactAddressType type, bool addEmptyRow, bool includeRows, bool includeCareOf)
        {
            int contactId = com.GetContactIdFromActorId(actorId);
            return Content(HttpStatusCode.OK, com.GetContactAddresses(contactId, type, addEmptyRow, includeCareOf).ToDTOs(includeRows));
        }

        [HttpGet]
        [Route("ContactAddressDict/{contactPersonId:int}")]
        public IHttpActionResult GetContactAddressItemsDict(int contactPersonId)
        {
            return Content(HttpStatusCode.OK, com.GetContactAddressItemsDict(contactPersonId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ContactAddressItem/{actorId:int}")]
        public IHttpActionResult GetContactAddressItems(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactAddressItems(actorId));
        }

        [HttpGet]
        [Route("ContactAddressItem/ByUser/{userId:int}")]
        public IHttpActionResult GetContactAddressItemsByUser(int userId)
        {
            return Content(HttpStatusCode.OK, com.GetContactAddressItems(um.GetActorContactPersonId(userId)));
        }

        [HttpGet]
        [Route("Address/AddressRowType/{sysContactTypeId:int}")]
        public IHttpActionResult GetSysContactAddressRowTypeIds(int sysContactTypeId)
        {
            return Content(HttpStatusCode.OK, com.GetSysContactAddressRowTypesWithAddressTypes(sysContactTypeId));
        }

        [HttpGet]
        [Route("Address/AddressType/{sysContactTypeId:int}")]
        public IHttpActionResult GetSysContactAddressTypeIds(int sysContactTypeId)
        {
            return Content(HttpStatusCode.OK, com.GetSysContactAddressTypeIds(sysContactTypeId));
        }

        [HttpGet]
        [Route("Address/EComType/{sysContactTypeId:int}")]
        public IHttpActionResult GetSysContactEComTypeIds(int sysContactTypeId)
        {
            return Content(HttpStatusCode.OK, com.GetSysContactEComsTypeIds(sysContactTypeId));
        }

        #endregion
    }
}