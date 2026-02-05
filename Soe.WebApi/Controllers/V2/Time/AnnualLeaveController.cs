using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/AnnualLeave")]
    public class AnnualLeaveController : SoeApiController
    {
        #region Variables

        private readonly AnnualLeaveManager alm;

        #endregion

        #region Constructor

        public AnnualLeaveController(AnnualLeaveManager alm)
        {
            this.alm = alm;
        }

        #endregion

        #region Annual leave calculation

        [HttpPost]
        [Route("CalculateTransactions")]
        public IHttpActionResult CalculateAnnualLeaveTransactions(AnnualLeaveCalculationModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, alm.CalculateAnnualLeaveTransactions(base.ActorCompanyId, model.EmployeeIds, model.DateFrom, model.DateTo));
        }

        #endregion

        #region Annual leave group

        [HttpGet]
        [Route("Grid/{annualLeaveGroupId:int?}")]
        public IHttpActionResult GetAnnualLeaveGroupsGrid(int? annualLeaveGroupId = null)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveGroups(base.ActorCompanyId, annualLeaveGroupId, true, true).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetAnnualLeaveGroupsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveGroupsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{annualLeaveGroupId:int}")]
        public IHttpActionResult GetAnnualLeaveGroup(int annualLeaveGroupId)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveGroup(annualLeaveGroupId).ToDTO());
        }

        [HttpGet]
        [Route("AnnualLeaveGroup/Limits/{type:int}")]
        public IHttpActionResult GetAnnualLeaveGroupLimits(int type)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveGroupLimits((TermGroup_AnnualLeaveGroupType)type));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveAnnualLeaveGroup(AnnualLeaveGroupDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, alm.SaveAnnualLeaveGroup(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{annualLeaveGroupId:int}")]
        public IHttpActionResult DeleteAnnualLeaveGroup(int annualLeaveGroupId)
        {
            return Content(HttpStatusCode.OK, alm.DeleteAnnualLeaveGroup(annualLeaveGroupId));
        }

        #endregion

        #region Annual leave transaction/balance

        [HttpGet]
        [Route("Transaction/{annualLeaveTransactionId:int}")]
        public IHttpActionResult GetAnnualLeaveTransaction(int annualLeaveTransactionId)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveTransaction(annualLeaveTransactionId, true).ToEditDTO());
        }

        [HttpPost]
        [Route("Transaction/Grid")]
        public IHttpActionResult getAnnualLeaveTransactionGridData(SearchAnnualLeaveTransactionModel model)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveTransactions(base.ActorCompanyId, model.EmployeeIds, model.dateFrom, model.dateTo, true).ToGridDTOs(true));
        }

        [HttpPost]
        [Route("Transaction")]
        public IHttpActionResult SaveAnnualLeaveTransaction(AnnualLeaveTransactionEditDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, alm.SaveAnnualLeaveTransaction(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Transaction/{annualLeaveTransactionId:int}")]
        public IHttpActionResult DeleteAnnualLeaveTransaction(int annualLeaveTransactionId)
        {
            return Content(HttpStatusCode.OK, alm.DeleteAnnualLeaveTransaction(annualLeaveTransactionId, false));
        }

        #endregion
    }
}