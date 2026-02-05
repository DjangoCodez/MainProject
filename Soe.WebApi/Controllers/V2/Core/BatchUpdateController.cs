using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Category/BatchUpdate")]
    public class BatchUpdateController : SoeApiController
    {
        #region Variables
        private readonly BatchUpdateManager bum;
        #endregion

        #region Constructor
        public BatchUpdateController(BatchUpdateManager bum)
        {
            this.bum = bum;
        }
        #endregion

        #region BatchUpdate API endpoints
        [HttpGet]
        [Route("GetBatchUpdateForEntity/{entityType:int}")]
        public IHttpActionResult GetBatchUpdateForEntity(SoeEntityType entityType)
        {
            return Content(HttpStatusCode.OK, bum.GetBatchUpdate(entityType));
        }
        [HttpPost]
        [Route("RefreshBatchUpdateOptions")]
        public IHttpActionResult RefreshBatchUpdateOptions(RefreshBatchUpdateOptionsModel model)
        {
            return Content(HttpStatusCode.OK, bum.RefreshBatchUpdateOptions(model.EntityType, model.BatchUpdate));
        }
        [HttpGet]
        [Route("FilterOptions/{entityType:int}")]
        public IHttpActionResult GetContactAddressItemsDict(SoeEntityType entityType)
        {
            return Content(HttpStatusCode.OK, bum.GetBatchUpdateFilterOptions(entityType));
        }
        [HttpPost]
        [Route("PerformBatchUpdate")]
        public IHttpActionResult PerformBatchUpdate(PerformBatchUpdateModel model)
        {
            return Content(HttpStatusCode.OK, bum.PerformBatchUpdate(model.EntityType, model.BatchUpdates, model.Ids, model.FilterIds));
        }
        #endregion
    }
}