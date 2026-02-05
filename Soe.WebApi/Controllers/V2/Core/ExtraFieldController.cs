using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class ExtraFieldController : SoeApiController
    {
        #region Variables

        private readonly ExtraFieldManager efm;

        #endregion

        #region Constructor

        public ExtraFieldController(ExtraFieldManager efm)
        {
            this.efm = efm;
        }

        #endregion

        #region ExtraField

        [HttpGet]
        [Route("ExtraField/{extraFieldId:int}")]
        public IHttpActionResult GetExtraField(int extraFieldId)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraField(extraFieldId, loadValues: true).ToDTO());
        }

        [HttpGet]
        [Route("ExtraFields/{entity:int}")]
        public IHttpActionResult GetExtraFields(int entity)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFields(entity, base.ActorCompanyId, true).ToGridDTOs());
        }

        [HttpGet]
        [Route("ExtraFields/{entity:int}/{connectedEntity:int}/{connectedRecordId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetExtraFieldsDict(int entity, int connectedEntity, int connectedRecordId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldsDict(entity, base.ActorCompanyId, connectedEntity, connectedRecordId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ExtraFieldGrid/{entity:int}/{loadRecords:bool}/{connectedEntity:int}/{connectedRecordId:int}/{extraFieldId:int?}")]
        public IHttpActionResult GetExtraFieldGridDTOs(int entity, bool loadRecords, int connectedEntity, int connectedRecordId, int? extraFieldId = null)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldGridDTOs(entity, base.ActorCompanyId, loadRecords, connectedEntity, connectedRecordId, extraFieldId));
        }

        [HttpPost]
        [Route("ExtraField")]
        public IHttpActionResult SaveExtraField(ExtraFieldDTO extraField)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, efm.SaveExtraField(extraField, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ExtraField/{extraFieldId:int}")]
        public IHttpActionResult DeleteExtraField(int extraFieldId)
        {
            return Content(HttpStatusCode.OK, efm.DeleteExtraField(extraFieldId));
        }

        #endregion

        #region ExtraFieldRecord

        [HttpGet]
        [Route("ExtraFieldRecord/{extraFieldId:int}/{recordId:int}/{entity:int}")]
        public IHttpActionResult GetExtraFieldRecord(int extraFieldId, int recordId, int entity)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldRecord(extraFieldId, recordId, entity, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("ExtraFieldsWithRecords/{recordId:int}/{entity:int}/{langId:int}/{connectedEntity:int?}/{connectedRecordId:int?}")]
        public IHttpActionResult GetExtraFieldsWitRecords(int recordId, int entity, int langId, int connectedEntity = 0, int connectedRecordId = 0)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldWithRecords(recordId, entity, base.ActorCompanyId, langId, connectedEntity, connectedRecordId));
        }

        [HttpPost]
        [Route("ExtraFieldsWithRecords")]
        public IHttpActionResult SaveExtraFieldRecords(ExtraFieldRecordsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                return Content(HttpStatusCode.OK, efm.SaveExtraFieldRecords(model.records, model.entity, model.recordId, base.ActorCompanyId));
            }
        }

        #endregion

        #region SysExtraField

        [HttpGet]
        [Route("SysExtraFields/{entity:int}")]
        public IHttpActionResult GetSysExtraFields(int entity)
        {
            return Content(HttpStatusCode.OK, efm.GetSysExtraFields((SoeEntityType)entity).ToDTOs());
        }
        
        #endregion
    }
}