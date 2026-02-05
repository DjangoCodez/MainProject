using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/BreakTemplate")]
    public class TimeBreakTemplateController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public TimeBreakTemplateController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region TimeBreakTemplate

        [HttpGet]
        [Route("Grid/{timeBreakTemplateId:int?}")]
        public IHttpActionResult GetTimeBreakTemplatesGrid(int? timeBreakTemplateId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeBreakTemplateGridsNew(base.ActorCompanyId, timeBreakTemplateId));
        }

        [HttpGet]
        [Route("{timeBreakTemplateId:int}")]
        public IHttpActionResult GetTimeBreakTemplate(int timeBreakTemplateId)
        {
            var result = tsm.GetTimeBreakTemplateForEditNew(timeBreakTemplateId, base.ActorCompanyId);
            if (result == null)
                return NotFound();

            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("Validate")]
        public IHttpActionResult ValidateTimeBreakTemplate(TimeBreakTemplateDTONew model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            var validationResult = tsm.ValidateBreakTemplateDTONew(model);
            return Content(HttpStatusCode.OK, validationResult);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveTimeBreakTemplate(TimeBreakTemplateDTONew model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            var saveResult = tsm.SaveTimeBreakTemplateNew(model);
            if (!saveResult.Success)
                return Error(HttpStatusCode.BadRequest, null, null, saveResult.ErrorMessage);

            return Content(HttpStatusCode.OK, saveResult);
        }

        [HttpDelete]
        [Route("{timeBreakTemplateId:int}")]
        public IHttpActionResult DeleteTimeBreakTemplate(int timeBreakTemplateId)
        {
            var result = tsm.DeleteTimeBreakTemplate(timeBreakTemplateId, base.ActorCompanyId);
            if (!result.Success)
                return Error(HttpStatusCode.BadRequest, null, null, result.ErrorMessage);

            return Content(HttpStatusCode.OK, result);
        }

        #endregion
    }
}


