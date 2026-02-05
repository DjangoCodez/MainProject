using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Document")]
    public class DocumentController: SoeApiController
    {
        #region Variables

        private readonly GeneralManager gm;

        #endregion

        #region Constructor

        public DocumentController(GeneralManager gm)
        {
            this.gm = gm;
        }

        #endregion

        #region Document

        [HttpGet]
        [Route("NewSince/{time}")]
        public IHttpActionResult HasNewDocuments(string time)
        {
            return Content(HttpStatusCode.OK, gm.HasNewCompanyDocuments(base.ActorCompanyId, BuildDateTimeFromString(time, false).Value));
        }

        [HttpGet]
        [Route("Company/")]
        public IHttpActionResult GetCompanyDocuments()
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyDocuments(base.ActorCompanyId, base.RoleId, base.UserId, includeUserUploaded: true).ToDocumentDTOs());
        }

        [HttpGet]
        [Route("Company/UnreadCount/")]
        public IHttpActionResult GetNbrOfUnreadCompanyDocuments()
        {
            return Content(HttpStatusCode.OK, gm.GetNbrOfUnreadCompanyDocuments(base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("My/")]
        public IHttpActionResult GetMyDocuments()
        {
            return Content(HttpStatusCode.OK, gm.GetMyDocuments(base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("{dataStorageId:int}")]
        public IHttpActionResult GetDocument(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDataStorage(dataStorageId, base.ActorCompanyId, true).ToDocumentDTO());
        }

        [HttpGet]
        [Route("Url/{dataStorageId:int}")]
        public IHttpActionResult GetDocumentUrl(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentUrl(dataStorageId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Data/{dataStorageId:int}")]
        public IHttpActionResult GetDocumentData(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentData(dataStorageId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Folders")]
        public IHttpActionResult GetDocumentFolders()
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentFolders(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("RecipientInfo/{dataStorageId:int}")]
        public IHttpActionResult GetDocumentRecipientInfo(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentRecipientInfo(dataStorageId, base.ActorCompanyId, base.RoleId, base.UserId, true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveDocument(SaveDocumentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gm.SaveDocument(model.Document, model.FileData, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SetAsRead/")]
        public IHttpActionResult SetDocumentAsRead(SetDocumentAsReadModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gm.SetDocumentAsRead(model.DataStorageId, base.UserId, model.Confirmed));
        }

        [HttpDelete]
        [Route("{dataStorageId:int}")]
        public IHttpActionResult DeleteDocument(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteDocument(dataStorageId, base.ActorCompanyId));
        }

        #endregion
    }
}