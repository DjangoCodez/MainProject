using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Payroll/PayrollGroup")]
    public class PayrollGroupController : SoeApiController
    {
        #region Variables

        private readonly PayrollManager pm;

        #endregion

        #region Constructor

        public PayrollGroupController(PayrollManager pm)
        {
            this.pm = pm;
        }

        #endregion

        #region PayrollGroup

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetPayrollGroupsGrid()
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroups(base.ActorCompanyId, loadTimePeriods: true, onlyActive: false).ToGridDTOs());
        }

        [HttpGet]
        [Route("SmallDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetPayrollGroupsSmall(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroupsSmall(base.ActorCompanyId, addEmptyRow, true));
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetPayrollGroupsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroupsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetPayrollGroups()
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroups(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("{payrollGroupId:int}/{includePriceTypes:bool}/{includePriceFormulas:bool}/{includeSettings:bool}/{includePayrollGroupReports:bool}/{includeTimePeriod:bool}/{includeAccounts:bool}/{includePayrollGroupVacationGroup:bool}/{includePayrollGroupPayrollProduct:bool}")]
        public IHttpActionResult GetPayrollGroup(int payrollGroupId, bool includePriceTypes, bool includePriceFormulas, bool includeSettings, bool includePayrollGroupReports, bool includeTimePeriod, bool includeAccounts, bool includePayrollGroupVacationGroup, bool includePayrollGroupPayrollProduct)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroup(payrollGroupId, false, includePriceTypes, includePriceFormulas, includeSettings, includePayrollGroupReports, includeTimePeriod, includeAccounts, includePayrollGroupVacationGroup, includePayrollGroupPayrollProduct, loadExternalCode: true).ToDTO(includePriceTypes, includePriceFormulas, includeSettings, includePayrollGroupReports, includeTimePeriod, includeAccounts, includePayrollGroupVacationGroup, includePayrollGroupPayrollProduct));
        }

        [HttpGet]
        [Route("Reports/{checkRolePermission:bool}")]
        public IHttpActionResult GetCompanyPayrollGroupReports(bool checkRolePermission)
        {
            return Content(HttpStatusCode.OK, pm.GetCompanyPayrollGroupReports(base.ActorCompanyId, checkRolePermission, base.RoleId));
        }

        [HttpGet]
        [Route("PriceTypesExists/{payrollGroupId:int}/{priceTypeIds}")]
        public IHttpActionResult PriceTypesExistsInPayrollGroup(int payrollGroupId, string priceTypeIds)
        {
            return Content(HttpStatusCode.OK, pm.PriceTypesExistsInPayrollGroup(payrollGroupId, StringUtility.SplitNumericList(priceTypeIds, nullIfEmpty: true)));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SavePayrollGroup(PayrollGroupDTO payrollGroup)
        {
            return Content(HttpStatusCode.OK, pm.SavePayrollGroup(payrollGroup));
        }

        [HttpDelete]
        [Route("{payrollGroupId:int}")]
        public IHttpActionResult DeletePayrollGroup(int payrollGroupId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollGroup(payrollGroupId));
        }

        #endregion
    }
}