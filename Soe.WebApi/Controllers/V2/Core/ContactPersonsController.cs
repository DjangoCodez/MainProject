using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Core/ContactPerson")]
    public class ContactPersonsController : SoeApiController
    {
        #region Variables

        private readonly ContactManager com;

        #endregion

        #region Constructor

        public ContactPersonsController(ContactManager com)
        {
            this.com = com;
        }

        #endregion

        #region ContactPerson

        [HttpGet]
        [Route("ContactPersons/{contactpersonId:int?}")]
        public IHttpActionResult GetContactPersons(int? contactpersonId = null)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonsByActorIdsForGrid(contactpersonId: contactpersonId));
        }

        [HttpGet]
        [Route("ContactPersonsByActorId/{actorId}")]
        public IHttpActionResult GetContactPersonsByActorId(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonsByActorId(actorId));
        }

        [HttpGet]
        [Route("ContactPersonsByActorIds/{actorIds}")]
        public IHttpActionResult GetContactPersonsByActorIds(string actorIds)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonsByActorIdsForGrid(actorIds));
        }

        [HttpGet]
        [Route("ContactPerson/{actorContactPersonId}")]
        public IHttpActionResult GetContactPerson(int actorContactPersonId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonForExport(actorContactPersonId, true).ToDTO());
        }

        [HttpGet]
        [Route("Categories/{contactPersonId}")]
        public IHttpActionResult GetContactPersonCategories(int contactPersonId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonCategories(contactPersonId));
        }


        [HttpGet]
        [Route("ContactPerson/Export/{actorId}")]
        public IHttpActionResult GetContactPersonForExport(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonForExport(actorId, true).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveContactPerson(ContactPersonDTO contactPerson)
        {
            return Content(HttpStatusCode.OK, com.SaveContactPerson(base.ActorCompanyId, contactPerson));
        }

        [HttpPost]
        [Route("Delete/")]
        public IHttpActionResult DeleteContactPersons([FromBody]List<int> contactPersonIds)
        {
            return Content(HttpStatusCode.OK, com.DeleteContactPersons(contactPersonIds));
        }

        [HttpDelete]
        [Route("{contactPersonId:int}")]
        public IHttpActionResult DeleteContactPerson(int contactPersonId)
        {
            return Content(HttpStatusCode.OK, com.DeleteContactPerson(contactPersonId, true, true));
        }

        #endregion
    }
}