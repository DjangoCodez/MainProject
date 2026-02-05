using Soe.WebApi.Binders;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;


namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/InvoiceProject")]
    public class InvoiceProjectController : SoeApiController
    {
        #region Variables

        private readonly ProjectManager projm;
        private readonly BudgetManager bm;

        #endregion

        #region Constructor

        public InvoiceProjectController(ProjectManager projm, BudgetManager bm)
        {
            this.projm = projm;
            this.bm = bm;
        }

        #endregion

        #region Project  

        [HttpGet]
        [Route("ProjectList")]
        public IHttpActionResult GetProjectList(int projectId, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] projectStatuses, bool onlyMine)
        {
            var projects = projm.GetProjectList(base.ActorCompanyId, projectStatuses, onlyMine);
            if (projectId != 0)
                projects = projects.Where(p => p.ProjectId == projectId).ToList();

            return Content(HttpStatusCode.OK, projects);
        }

        [HttpGet]
        [Route("{onlyActive:bool}/{hidden:bool}/{setStatusName:bool}/{includeManagerName:bool}/{loadOrders:bool}/{projectStatus:int}")]
        public IHttpActionResult GetProjects(bool onlyActive, bool hidden, bool setStatusName, bool includeManagerName, bool loadOrders, int projectStatus)
        {
            if (onlyActive)
                return Content(HttpStatusCode.OK, projm.GetProjects(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, onlyActive, hidden, setStatusName, includeManagerName, loadOrders, -1, projectStatus).ToGridDTOs(includeManagerName, loadOrders));
            else
                return Content(HttpStatusCode.OK, projm.GetProjects(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, null, hidden, setStatusName, includeManagerName, loadOrders, -1, projectStatus).ToGridDTOs(includeManagerName, loadOrders));
        }

        [HttpGet]
        [Route("{projectId:int}")]
        public IHttpActionResult GetProject(int projectId)
        {
            return Content(HttpStatusCode.OK, projm.GetProject(projectId, false));
        }

        [HttpGet]
        [Route("GetTimeProject/{projectId:int}")]
        public IHttpActionResult GetTimeProject(int projectId)
        {
            return Content(HttpStatusCode.OK, projm.GetTimeProject(projectId, true).ToTimeProjectDTO(true, true));
        }

        [HttpGet]
        [Route("GridDTO/{projectId:int}")]
        public IHttpActionResult GetProjectGridDTO(int projectId)
        {
            return Content(HttpStatusCode.OK, projm.GetProject(projectId, true).ToGridDTO(true, false));
        }

        [HttpGet]
        [Route("Project/Small/{onlyActive:bool}/{hidden:bool}/{sortOnNumber:bool}")]
        public IHttpActionResult GetProjectsSmall(bool onlyActive, bool hidden, bool sortOnNumber)
        {
            if (onlyActive)
                return Content(HttpStatusCode.OK, projm.GetProjectsSmall(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, onlyActive, hidden, sortOnNumber));
            else
                return Content(HttpStatusCode.OK, projm.GetProjectsSmall(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, null, hidden, sortOnNumber));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult ChangeProjectStatus(ChangeProjectStatusModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, projm.ChangeProjectStatus(model.Ids, model.NewState));
        }

        [HttpGet]
        [Route("GetProjectTraceViews/{projectId:int}")]
        public IHttpActionResult GetProjectTraceViews(int projectId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(ActorCompanyId);

            return Ok(projm.GetProjectTraceViews(projectId, baseSysCurrencyId));
        }

        [HttpGet]
        [Route("ProjectCentralStatus/{projectId:int}/{includeChildProjects:bool}/{loadDetails:bool}/{from?}/{to?}")]
        public IHttpActionResult GetProjectCentralStatus(int projectId, bool includeChildProjects, bool loadDetails, string from = "", string to = "")
        {
            var projectCentral = new ProjectCentralManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, projectCentral.GetProjectCentralStatus(base.ActorCompanyId, projectId, BuildDateTimeFromString(from, true), BuildDateTimeFromString(to, true), includeChildProjects, loadDetails));
        }

        [HttpGet]
        [Route("BudgetHead/{projectId:int}/{actorCompanyId:int}")]
        public IHttpActionResult GetBudgetHeadGridForProject(int projectId, int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, bm.GetProjectBudgetHeadForGrid(actorCompanyId != 0 ? actorCompanyId : base.ActorCompanyId, projectId));
        }

        [HttpPost]
        [Route("Search/")]
        public IHttpActionResult GetProjectsBySearch(ProjectSearchModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, projm.GetProjectsBySearch2(model.Number, model.Name, model.CustomerNr, model.CustomerName, model.ManagerName, model.OrderNr, model.OnlyActive, model.Hidden, model.ShowWithoutCustomer, model.LoadMine, model.CustomerId, model.ShowAllProjects));
        }

        [HttpPost]
        [Route("Save")]
        public IHttpActionResult SaveProject(SaveInvoiceProjectModel project)
        {
            var dict = new Dictionary<int, decimal>();
            foreach (var pricelist in project.priceLists)
            {
                dict.Add(pricelist.Key, pricelist.Value);
            }
            return Content(HttpStatusCode.OK, projm.SaveTimeProject(project.invoiceProject, project.categoryRecords, project.accountSettings, project.projectUsers, dict, project.newPricelist, project.pricelistName, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{projectId:int}")]
        public IHttpActionResult DeleteProject(int projectId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, projm.DeleteProject(projectId, base.ActorCompanyId));
        }

        #endregion

        #region

        [HttpGet]
        [Route("Users/{projectId:int}/{loadTypeNames:bool}")]
        public IHttpActionResult GetProjectUsers(int projectId, bool loadTypeNames)
        {
            return Content(HttpStatusCode.OK, projm.GetProjectUsersForAngular(projectId, base.ActorCompanyId, loadTypeNames));
        }
        #endregion

    }
}