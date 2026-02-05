using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimeWorkReduction")]
    public class TimeWorkReductionController : SoeApiController
    {
        #region Variables

        private readonly TimeAccumulatorManager tam;
        private readonly TimeWorkReductionManager twrm;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public TimeWorkReductionController(TimeAccumulatorManager tam, TimeWorkReductionManager twrm, TimeEngineManager tem)
        {
            this.tam = tam;
            this.twrm = twrm;
            this.tem = tem;
        }

        #endregion

        #region TimeWorkReduction

        [HttpGet]
        [Route("TimeAccumulatorsForReductionDict")]
        public IHttpActionResult GetTimeAccumulatorsForReductionDict()
        {
            return Content(HttpStatusCode.OK, tam.GetTimeAccumulatorsDict(base.ActorCompanyId, true, onlyTimeWorkAccountReduction: true).ToSmallGenericTypes());
        }

        #endregion

        #region TimeWorkReductionReconciliation

        [HttpGet]
        [Route("Reconciliation/Grid/{timeWorkReductionReconciliationId:int?}")]
        public IHttpActionResult GetTimeWorkReductionsGrid(int? timeWorkReductionReconciliationId = null)
        {
            return Content(HttpStatusCode.OK, twrm.GetTimeWorkReductionReconciliations(base.ActorCompanyId, timeWorkReductionReconciliationId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Reconciliation/{timeWorkReductionReconciliationId:int?}")]
        public IHttpActionResult GetTimeWorkReduction(int timeWorkReductionReconciliationId)
        {
            return Content(HttpStatusCode.OK, twrm.GetTimeWorkReductionReconciliation(base.ActorCompanyId, timeWorkReductionReconciliationId, includeYears: true).ToDTO());
        }


        [HttpPost]
        [Route("Reconciliation/")]
        public IHttpActionResult SaveTimeWorkReductionReconciliation(TimeWorkReductionReconciliationDTO timeWorkReductionReconciliation)
        {
            return Content(HttpStatusCode.OK, twrm.SaveTimeWorkReductionReconciliation(timeWorkReductionReconciliation, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Reconciliation/{timeWorkReductionReconciliationId:int?}")]
        public IHttpActionResult DeleteTimeWorkReductionReconciliation(int timeWorkReductionReconciliationId)
        {
            return Content(HttpStatusCode.OK, twrm.DeleteTimeWorkReductionReconciliation(timeWorkReductionReconciliationId, base.ActorCompanyId));
        }

        #endregion

        #region TimeWorkReductionReconciliationYear

        [HttpPost]
        [Route("Reconciliation/Year/")]
        public IHttpActionResult SaveTimeWorkReductionReconciliationYear(TimeWorkReductionReconciliationYearDTO timeWorkReductionReconciliationYear)
        {
            return Content(HttpStatusCode.OK, twrm.SaveTimeWorkReductionReconciliationYear(timeWorkReductionReconciliationYear));
        }
        [HttpDelete]
        [Route("Reconciliation/Year/{TimeWorkReductionYearId:int?}")]
        public IHttpActionResult DeleteTimeWorkReductionReconciliationYear(int timeWorkReductionReconciliationYearId)
        {
            return Content(HttpStatusCode.OK, twrm.DeleteTimeWorkReductionReconciliationYear(timeWorkReductionReconciliationYearId));
        }

        #endregion

        #region TimeWorkReductionReconciliationEmployee

        [HttpGet]
        [Route("Reconciliation/Year/Employee/{TimeWorkReconcilationYearId:int?}")]
        public IHttpActionResult GetTimeWorkReductionReconciliationEmployee(int timeWorkReductionReconciliationYearId)
        {
            return Content(HttpStatusCode.OK, twrm.GetTimeWorkReductionReconciliationEmployee(timeWorkReductionReconciliationYearId).ToDTOs());
        }

        [HttpPost]
        [Route("Reconciliation/Year/Employee/Calculate")]
        public IHttpActionResult CalculateYearEmployee(TimeWorkReductionReconciliationEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.CalculateTimeWorkReductionReconciliationYearEmployee(model.TimeWorkReductionReconciliationId, model.TimeWorkReductionReconciliationYearId, model.TimeWorkReductionReconciliationEmployeeIds, model.EmployeeIds));
        }
        [HttpPost]
        [Route("Reconciliation/Year/Employee/GenerateOutcome")]
        public IHttpActionResult GenerateOutcome(TimeWorkReductionReconciliationGenerateOutcomeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkReductionReconciliationYearEmployeeGenerateOutcome(model.TimeWorkReductionReconciliationId, model.TimeWorkReductionReconciliationYearId, model.OverrideChoosen, model.PaymentDate, model.TimeWorkReductionReconciliationEmployeeIds));
        }
        [HttpPost]
        [Route("Reconciliation/Year/Employee/ReverseTransactions")]
        public IHttpActionResult ReverseTransactions(TimeWorkReductionReconciliationGenerateOutcomeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkReductionReconciliationYearEmployeeReverseTransactions(model.TimeWorkReductionReconciliationId, model.TimeWorkReductionReconciliationYearId, model.OverrideChoosen, model.TimeWorkReductionReconciliationEmployeeIds));
        }
        [HttpPost]
        [Route("Reconciliation/Year/Employee/GetPensionExport")]
        public IHttpActionResult GetPensionExport(TimeWorkReductionReconciliationEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twrm.GetTimeWorkReductionYearPensionExport(model.TimeWorkReductionReconciliationId, model.TimeWorkReductionReconciliationYearId, model.TimeWorkReductionReconciliationEmployeeIds));
        }
      
        #endregion
    }
}