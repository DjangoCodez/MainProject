using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using System.Collections.Generic;
using SoftOne.Soe.Common.DTO;
using System;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using Soe.WebApi.Binders;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Project")]
    public class ProjectTimeController : SoeApiController
    {
        #region Variables

        private readonly ProjectManager pm;
        private readonly UserManager um;

        #endregion

        #region Constructor

        public ProjectTimeController(ProjectManager pm, UserManager um)
        {
            this.pm = pm;
            this.um = um;
        }

        #endregion

        #region TimeTransactions

        [HttpPost]
        [Route("TimeBlocksForTimeSheetFiltered")]
        public IHttpActionResult GetTimeBlocksForTimeSheetFiltered(GetProjectTimeBlocksForTimesheetModel model)
        {
            return Content(HttpStatusCode.OK, pm.LoadProjectTimeBlockForTimeSheet(model.From, model.To, model.EmployeeId, model.Employees ?? new List<int>(), model.Projects ?? new List<int>(), model.Orders ?? new List<int>(), model.groupByDate, model.incPlannedAbsence, true, model.incInternOrderText, model.EmployeeCategories, model.TimeDeviationCauses));
        }

        [HttpGet]
        [Route("TimeBlocksForTimeSheetFilteredByProject/{fromDate}/{toDate}/{projectId:int}/{includeChildProjects:bool}/{employeeId:int}")]
        public IHttpActionResult GetTimeBlocksForTimeSheetFilteredByProject(string fromDate, string toDate, int projectId, bool includeChildProjects, int employeeId)
        {
            return Content(HttpStatusCode.OK, pm.LoadProjectTimeBlockForTimeSheetByProjectId(BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true), projectId, includeChildProjects, employeeId));
        }

        [HttpGet]
        [Route("Employees/Small/{projectId:int}/{fromDateString}/{toDateString}")]
        public IHttpActionResult GetEmployeesForTimeProjectRegistrationSmall(int projectId, string fromDateString, string toDateString)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeesForTimeProjectRegistrationSmall(base.RoleId, projectId, BuildDateTimeFromString(fromDateString, true), BuildDateTimeFromString(toDateString, true)));
        }

        [HttpGet]
        [Route("EmployeeScheduleAndTransactionInfo/{employeeId}/{date}")]
        public IHttpActionResult GetEmployeeScheduleAndTransactionInfo(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, pm.LoadEmployeeScheduleAndTransactionInfo(employeeId, (DateTime)BuildDateTimeFromString(date, true)));
        }

        [HttpGet]
        [Route("EmployeeFirstTime/{employeeId}/{date}")]
        public IHttpActionResult GetEmployeeFirstEligibleTime(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeeFirstEligableTime(employeeId, (DateTime)BuildDateTimeFromString(date, true), base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("RecalculateWorkTime")]
        public IHttpActionResult RecalculateWorkTime(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.RecalculateWorkTime(projectTimeBlockSaveDTOs));
            }
        }


       [HttpPost]
        [Route("MoveTimeRowsToDate")]
        public IHttpActionResult MoveTimeRowsToDate(MoveProjectTimeBlocksToDateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.MoveTimeRowsToDate(BuildDateTimeFromString(model.SelectedDate, true), model.ProjectTimeBlockIds, false));
            }
        }

        [HttpPost]
        [Route("MoveTimeRowsToOrder")]
        public IHttpActionResult MoveTimeRowsToOrder(MoveProjectTimeBlocksToOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.MoveTimeRowsToOrder(model.CustomerInvoiceId, model.ProjectTimeBlockIds));
            }
        }

        [HttpPost]
        [Route("MoveTimeRowsToOrderRow")]
        public IHttpActionResult MoveTimeRowsToOrderRow(MoveProjectTimeBlocksToOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.MoveTimeRowsToOrderRow(model.CustomerInvoiceId, model.CustomerInvoiceRowId, model.ProjectTimeBlockIds));
            }
        }

        [HttpPost]
        [Route("SaveNotesForProjectTimeBlock")]
        public IHttpActionResult SaveNotesForProjectTimeBlock(ProjectTimeBlockSaveDTO projectTimeBlockSaveDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.SaveNotesForProjectTimeBlock(projectTimeBlockSaveDTO));
            }
        }

        [HttpPost]
        [Route("ValidateProjectTimeBlockSaveDTO")]
        public IHttpActionResult ValidateProjectTimeBlockSaveDTO(List<ValidateProjectTimeBlockSaveDTO> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.ValidateSaveProjectTimeBlocks(items, false));
            }
        }

        [HttpPost]
        [Route("ProjectTimeBlockSaveDTO")]
        public IHttpActionResult SaveProjectTimeBlockSaveDTO(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pm.SaveProjectTimeBlocks(projectTimeBlockSaveDTOs, false));
            }
        }


        #endregion

        #region TimeSheet

        [HttpPost]
        [Route("Employees")]
        public IHttpActionResult GetEmployeesForProject(GetProjectEmployeesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.GetEmployeesForProjectWithTimeCode(base.ActorCompanyId, base.UserId, base.RoleId, model.AddEmptyRow, model.GetHidden, model.AddNoReplacementEmployee, model.IncludeEmployeeId, BuildDateTimeFromString(model.FromDateString, true), BuildDateTimeFromString(model.ToDateString, true), model.EmployeeCategories));
        }

        [HttpGet]
        [Route("EmployeeChilds/{employeeId:int}")]
        public IHttpActionResult GetEmployeeChildren(int employeeId)
        {
            var em = new EmployeeManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, em.GetEmployeeChildsDict(employeeId, false).ToSmallGenericTypes());
        }

        #region weekReport
        [HttpGet]
        [Route("TimeBlocksForMatrix/{employeeId:int}/{selectedEmployeeId:int}/{dateFrom}/{dateTo}/{isCopying}")]
        public IHttpActionResult LoadProjectTimeBlockForMatrix(int employeeId, int selectedEmployeeId, string dateFrom, string dateTo, bool isCopying)
        {
            return Content(HttpStatusCode.OK, pm.LoadProjectTimeBlockForMatrix(employeeId, selectedEmployeeId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), isCopying));
        }

        [HttpPost]
        [Route("TimeBlocksForMatrix")]
        public IHttpActionResult SaveProjectMatrix(List<ProjectTimeMatrixSaveDTO> projectTimeMatrixBlockDTOs)
        {
            return Content(HttpStatusCode.OK, pm.SaveProjectTimeMatrixBlocks(projectTimeMatrixBlockDTOs, base.ActorCompanyId));
        }
        #endregion

        [HttpGet]
        [Route("Project/ProjectsForTimeSheet/{employeeId:int}")]
        public IHttpActionResult GetProjectsForTimeSheet(int employeeId)
        {   // Get user connected to specified employee
            User user = um.GetUserByEmployeeId(employeeId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, pm.GetProjectsForTimeSheetWithCustomer(employeeId, base.ActorCompanyId, user != null ? user.UserId : (int?)null).ToSmallDTOs(user != null ? user.UserId : (int?)null, setCustomer: true));
        }

        [HttpGet]
        [Route("Project/ProjectsForTimeSheet/Employees/{projectId:int?}")]
        public IHttpActionResult GetProjectsForTimeSheetEmployees([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] empIds, int? projectId = 0)
        {
            return Content(HttpStatusCode.OK, pm.GetProjectsForTimeSheetEmployees(empIds, projectId));
        }

        #endregion
    }
}