using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EmployeeSchedule")]
    public class EmployeeScheduleController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public EmployeeScheduleController(TimeEngineManager tem, TimeScheduleManager tsm)
        {
            this.tsm = tsm;
            this.tem = tem;
        }

        #endregion

        #region EmployeeSchedule

        //[HttpGet]
        //[Route("EmployeeSchedule/GetEmployeeScheduleForEmployee/{dateString}/{employeeId:int}")]
        //public IHttpActionResult GetEmployeeScheduleForEmployee(string dateString, int employeeId)
        //{
        //    return Content(HttpStatusCode.OK, tsm.GetPlacementForEmployee(BuildDateTimeFromString(dateString, true).Value, employeeId, base.ActorCompanyId).ToDTO());
        //}

        //[HttpGet]
        //[Route("EmployeeSchedule/GetLastEmployeeScheduleForEmployee/{employeeId:int}/{timeScheduleTemplateHeadId:int}")]
        //public IHttpActionResult GetLastEmployeeScheduleForEmployee(int employeeId, int timeScheduleTemplateHeadId)
        //{
        //    return Content(HttpStatusCode.OK, tsm.GetLastPlacementForEmployee(employeeId, base.ActorCompanyId, timeScheduleTemplateHeadId != 0 ? timeScheduleTemplateHeadId : (int?)null).ToDTO());
        //}

        [HttpGet]
        [Route("Grid/{onlyLatest:bool}/{addEmptyPlacement:bool}")]
        public IHttpActionResult getPlacementsForGrid(bool onlyLatest, bool addEmptyPlacement)
        {
            //     public bool OnlyLatest { get; set; }
            //public bool AddEmptyPlacement { get; set; }
            //public List<int> EmployeeIds { get; set; }
            //public DateTime? DateFrom { get; set; }
            //public DateTime? DateTo { get; set; }
            //BuildDateTimeFromString(dateString, true)
            return Content(HttpStatusCode.OK, tsm.GetPlacementsForGrid(base.RoleId, onlyLatest, addEmptyPlacement));
        }

        //[HttpPost]
        //[Route("EmployeeSchedule/ValidateShortenEmployment")]
        //public IHttpActionResult ValidateShortenEmployment(HasEmployeeOverlappingPlacementModel model)
        //{
        //    return Content(HttpStatusCode.OK, tsm.ValidateShortenEmployment(model.EmployeeId, model.OldDateFrom, model.OldDateTo, model.NewDateFrom, model.NewDateTo, !model.ApplyFinalSalary, model.ChangedEmployment, model.EmployeePlacements, model.ScheduledEmployeePlacements, model.Employments));
        //}

        [Route("ControlActivations")]
        public IHttpActionResult ControlActivations(ControlActivationsModel model)
        {
            return Content(HttpStatusCode.OK, tem.ControlEmployeeSchedulePlacements(model.Items, model.StartDate, model.StopDate, model.IsDelete));
        }

        [Route("ControlActivation")]
        public IHttpActionResult ControlActivation(ControlActivationModel model)
        {
            return Content(HttpStatusCode.OK, tem.ControlEmployeeSchedulePlacement(model.EmployeeId, model.EmployeeScheduleStartDate, model.EmployeeScheduleStopDate, model.StartDate, model.StopDate, model.IsDelete));
        }

        [HttpPost]
        [Route("EmployeeSchedule/Activate")]
        public IHttpActionResult EmployeeSchedule(SaveEmployeeScheduleModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveEmployeeSchedulePlacement(model.Control, model.Items, model.Function, model.StartDate, model.StopDate, model.TimeScheduleTemplateHeadId, model.TimeScheduleTemplatePeriodId, model.Preliminary));
        }

        [HttpPost]
        [Route("Delete")]
        public IHttpActionResult DeletePlacement(DeletePlacementModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteEmployeeSchedulePlacement(model.Item, model.Control));
        }

        #endregion

        #region TimeScheduleTemplateHead

        [HttpGet]
        [Route("TimeScheduleTemplateHead/Activate/")]
        public IHttpActionResult GetTimeScheduleTemplateHeadsForActivate()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHeads(base.ActorCompanyId, false, false, true, true).ToSmallDTOs());
        }
        #endregion

        #region TimeScheduleTemplatePeriod

        [HttpGet]
        [Route("TimeScheduleTemplatePeriod/Activate/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult GetTimeScheduleTemplatePeriodsForActivate(int timeScheduleTemplateHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplatePeriods(timeScheduleTemplateHeadId, false).ToSmallDTOs());
        }

        #endregion
    }
}