using Soe.WebApi.Binders;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Employee")]
    public class VacationGroupController : SoeApiController
    {
        #region Variables

        private readonly PayrollManager pm;

        #endregion

        #region Constructor

        public VacationGroupController(PayrollManager pm)
        {
            this.pm = pm;
        }

        #endregion

        #region VacationGroup - Employee Conrtroller

        [HttpGet]
        [Route("VacationGroup/{loadTypeNames:bool}/{loadOnlyActive:bool}")]
        public IHttpActionResult GetVacationGroups(bool loadTypeNames, bool loadOnlyActive)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroups(base.ActorCompanyId, loadTypeNames, loadOnlyActive).ToGridDTOs());
        }

        [HttpGet]
        [Route("VacationGroup/{vacationGroupId:int}")]
        public IHttpActionResult GetVacationGroup(int vacationGroupId)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroup(vacationGroupId, setLatestVacationYearEnd: true, loadExternalCode: true).ToDTO());
        }

        [HttpGet]
        [Route("VacationGroup/Employee/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetVacationGroupForEmployee(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroupForEmployee(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true)).ToDTO());
        }

        [HttpDelete]
        [Route("DeleteVacationGroup/{vacationGroupId:int}")]
        public IHttpActionResult DeleteVacationGroup(int vacationGroupId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteVacationGroup(vacationGroupId));
        }

        [HttpPost]
        [Route("VacationGroup")]
        public IHttpActionResult SaveVacationGroup(VacationGroupDTO vacationGroup)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SaveVacationGroup(vacationGroup, vacationGroup.VacationGroupSE, base.ActorCompanyId));
        }

        #endregion

        #region VacationGroup - Payroll Conrtroller

        [HttpGet]
        [Route("VacationGroup/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetVacationGroupsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroupsDictV2(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("VacationGroup/EndDate")]
        public IHttpActionResult GetVacationGroupEndDates([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] vacationGroupIds)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroupEndDates(vacationGroupIds.ToList(), DateTime.Today));
        }

        #endregion
    }
}