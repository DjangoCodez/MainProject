using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Project")]
    public class ProjectController : SoeApiController
    {
        #region Variables

        private readonly ProjectManager pm;
        private readonly ProductManager prom;
        private readonly TimeTransactionManager ttm;

        #endregion

        #region Constructor
        public ProjectController(ProjectManager pm, TimeTransactionManager ttm, ProductManager prom)
        {
            this.pm = pm;
            this.ttm = ttm;
            this.prom = prom;
        }
        #endregion

        #region Product
        [HttpGet]
        [Route("TimeCode")]
        public IHttpActionResult GetTimeCodes(HttpRequestMessage message)
        {
            int projectId = message.GetIntValueFromQS("projectId");
            return Content(HttpStatusCode.OK, ttm.GetProjectTimeCodeTransactionItems(ActorCompanyId, projectId));
        }

        [HttpGet]
        [Route("PriceList/{comparisonPriceListTypeId:int}/{priceListTypeId:int}/{loadAll:bool}/{priceDate}")]
        public IHttpActionResult GetPriceLists(int comparisonPriceListTypeId, int priceListTypeId, bool loadAll, string priceDate)
        {
            return Content(HttpStatusCode.OK, prom.GetProductComparisonDTOs(ActorCompanyId, comparisonPriceListTypeId, priceListTypeId, loadAll, BuildDateTimeFromString(priceDate, true)));        
        }

        [HttpGet]
        [Route("ProductRows/{projectId:int}/{originType:int}/{includeChildProjects:bool}/{fromDate}/{toDate}")]
        public IHttpActionResult GetProductRows(int projectId, int originType, bool includeChildProjects, string fromDate, string toDate)
        {
            var projectCentral = new ProjectCentralManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, projectCentral.GetProjectProductRows(projectId, originType, includeChildProjects, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true)));
        }

        #endregion

        #region User
        [HttpGet]
        [Route("Users")]
        public IHttpActionResult GetProjectUsers(HttpRequestMessage message)
        {
            int projectId = message.GetIntValueFromQS("projectId");
            bool loadTypeNames = message.GetBoolValueFromQS("loadTypeNames");
            return Content(HttpStatusCode.OK, pm.GetProjectUsersForAngular(projectId, base.ActorCompanyId, loadTypeNames));
        }
        #endregion

        #region TimeSheet

        [HttpPost]
        [Route("Employees/")]
        public IHttpActionResult GetEmployeesForProject(GetProjectEmployeesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.GetEmployeesForProjectWithTimeCode(base.ActorCompanyId, base.UserId, base.RoleId, model.AddEmptyRow, model.GetHidden, model.AddNoReplacementEmployee, model.IncludeEmployeeId, BuildDateTimeFromString(model.FromDateString, true), BuildDateTimeFromString(model.ToDateString, true), model.EmployeeCategories));
        }

        [HttpGet]
        [Route("EmployeeChilds/{employeeId:int}")]
        public IHttpActionResult GetEmployeeChilds(int employeeId)
        {
            var em = new EmployeeManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, em.GetEmployeeChildsDict(employeeId, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeBlocksForMatrix/{employeeId:int}/{selectedEmployeeId:int}/{dateFrom}/{dateTo}/{isCopying}")]
        public IHttpActionResult LoadProjectTimeBlockForMatrix(int employeeId, int selectedEmployeeId, string dateFrom, string dateTo, bool isCopying)
        {
            return Content(HttpStatusCode.OK, pm.LoadProjectTimeBlockForMatrix(employeeId, selectedEmployeeId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), isCopying));
        }

        [HttpPost]
        [Route("TimeBlocksForMatrix/")]
        public IHttpActionResult SaveProject(List<ProjectTimeMatrixSaveDTO> projectTimeMatrixBlockDTOs)
        {
            return Content(HttpStatusCode.OK, pm.SaveProjectTimeMatrixBlocks(projectTimeMatrixBlockDTOs, base.ActorCompanyId));
        }
        #endregion

        #region Project

        [HttpGet]
        [Route("{projectId:int}")]
        public IHttpActionResult GetProject(int projectId)
        {
            return Content(HttpStatusCode.OK, pm.GetTimeProject(projectId, true).ToTimeProjectDTO(true, true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveProject(SaveInvoiceProjectModel project)
        {
            var dict = new Dictionary<int, decimal>();
            foreach (var pricelist in project.priceLists)
            {
                dict.Add(pricelist.Key, pricelist.Value);
            }
            return Content(HttpStatusCode.OK, pm.SaveTimeProject(project.invoiceProject, project.categoryRecords, project.accountSettings, project.projectUsers, dict, project.newPricelist, project.pricelistName, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("UpdateCustomer/{invoiceId:int}/{projectId:int}/{customerId:int}")]
        public IHttpActionResult UpdateProjectCustomer(int invoiceId, int projectId, int customerId)
        {
            return Content(HttpStatusCode.OK, pm.UpdateProjectCustomer(base.ActorCompanyId, projectId, invoiceId, customerId));
        }

        [HttpDelete]
        [Route("{projectId:int}")]
        public IHttpActionResult DeleteProject(int projectId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.DeleteProject(projectId, base.ActorCompanyId));
        }

        #endregion

        #region ProjectCentral

        [HttpGet]
        [Route("ProjectCentralStatus/{projectId:int}/{includeChildProjects:bool}/{from}/{to}/{loadDetails:bool}")]
        public IHttpActionResult GetProjectCentralStatus(int projectId, bool includeChildProjects, string from, string to, bool loadDetails)
        {
            var projectCentral = new ProjectCentralManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, projectCentral.GetProjectCentralStatus(base.ActorCompanyId, projectId, BuildDateTimeFromString(from, true), BuildDateTimeFromString(to, true), includeChildProjects, loadDetails));
        }

        #endregion

        #region Migration


        [HttpGet]
        [Route("Migrate/{key}")]
        public IHttpActionResult MigrateProjectInvoiceDays(string key)
        {
            Guid guid = Guid.Parse(key);
            int actorCompanyId = base.ActorCompanyId;
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            var workingThread = new Thread(() => StartMigrateProjectInvoiceDays(cultureInfo, guid, actorCompanyId));
            workingThread.Start();
            return Content(HttpStatusCode.OK, new SoeProgressInfo(guid));
        }

        [HttpGet]
        [Route("Migrate/Result/{key}")]
        public IHttpActionResult GetMigrationResult(Guid key)
        {
            return Content(HttpStatusCode.OK, monitor.GetResult(key));
        }

        private void StartMigrateProjectInvoiceDays(CultureInfo cultureInfo, Guid key, int actorCompanyId)
        {
            SetLanguage(cultureInfo);

            SoeProgressInfo info = monitor.RegisterNewProgressProcess(key);
            pm.MigrateProjectInvoiceDaysToProjectTimeBlocks(key, actorCompanyId, ref info, monitor);
        }

        #endregion
    }
}