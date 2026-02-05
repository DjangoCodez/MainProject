using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimeLeisureCode")]
    public class TimeLeisureCodeController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public TimeLeisureCodeController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region TimeLeisureCode

        [HttpGet]
        [Route("Grid/{timeLeisureCodeId:int?}")]
        public IHttpActionResult GetTimeLeisureCodesGrid(int? timeLeisureCodeId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeLeisureCodes(base.ActorCompanyId, timeLeisureCodeId, true).ToGridDTOs());
        }

        [HttpGet]
        [Route("{timeLeisureCodeId:int}")]
        public IHttpActionResult GetTimeLeisureCode(int timeLeisureCodeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeLeisureCode(timeLeisureCodeId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveTimeLeisureCode(TimeLeisureCodeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeLeisureCode(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{timeLeisureCodeId:int}")]
        public IHttpActionResult DeleteTimeLeisureCode(int timeLeisureCodeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeLeisureCode(timeLeisureCodeId));
        }

        #endregion

        #region EmployeeGroupTimeLeisureCode

        [HttpGet]
        [Route("EmployeeGroup/Grid/{employeeGroupTimeLeisureCodeId:int?}")]
        public IHttpActionResult GetEmployeeGroupTimeLeisureCodesGrid(int? employeeGroupTimeLeisureCodeId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeeGroupTimeLeisureCodes(base.ActorCompanyId, employeeGroupTimeLeisureCodeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("EmployeeGroup/{employeeGroupTimeLeisureCodeId:int}")]
        public IHttpActionResult GetEmployeeGroupTimeLeisureCode(int employeeGroupTimeLeisureCodeId)
        {
            EmployeeGroupTimeLeisureCodeDTO employeeGroupTimeLeisureCodeDTO = tsm.GetEmployeeGroupTimeLeisureCode(employeeGroupTimeLeisureCodeId).ToDTO();
            employeeGroupTimeLeisureCodeDTO.Settings = tsm.GetEmployeeGroupTimeLeisureCodeSettings(employeeGroupTimeLeisureCodeId).ToDTOs();

            return Content(HttpStatusCode.OK, employeeGroupTimeLeisureCodeDTO);
        }

        [HttpPost]
        [Route("EmployeeGroup")]
        public IHttpActionResult SaveEmployeeGroupTimeLeisureCode(EmployeeGroupTimeLeisureCodeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveEmployeeGroupTimeLeisureCode(model));
        }

        [HttpDelete]
        [Route("EmployeeGroup/{employeeGroupTimeLeisureCodeId:int}")]
        public IHttpActionResult DeleteEmployeeGroupTimeLeisureCode(int employeeGroupTimeLeisureCodeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteEmployeeGroupTimeLeisureCode(employeeGroupTimeLeisureCodeId));
        }

        #endregion
    }



}