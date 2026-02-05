using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimeWorkAccount")]
    public class TimeWorkAccountController : SoeApiController
    {
        #region Variables

        private readonly ReportManager rm;
        private readonly ProductManager prm;
        private readonly TimeAccumulatorManager tam;
        private readonly TimeWorkAccountManager twam;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public TimeWorkAccountController(ReportManager rm, ProductManager prm, TimeAccumulatorManager tam, TimeWorkAccountManager twam, TimeEngineManager tem)
        {
            this.rm = rm;
            this.prm = prm;
            this.tam = tam;
            this.twam = twam;
            this.tem = tem;
        }

        #endregion

        #region TimeWorkAccount

        [HttpGet]
        [Route("Grid/{timeWorkAccountId:int?}")]
        public IHttpActionResult GetTimeWorkAccountsGrid(int? timeWorkAccountId = null)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccounts(timeWorkAccountId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{timeWorkAccountId:int}/{includeYears:bool}")]
        public IHttpActionResult GetTimeWorkAccount(int timeWorkAccountId, bool includeYears)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccount(timeWorkAccountId, loadYears: includeYears).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveTimeWorkAccount(TimeWorkAccountDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twam.SaveTimeWorkAccount(model));
        }

        [HttpDelete]
        [Route("{timeWorkAccountId:int}")]
        public IHttpActionResult DeleteTimeWorkAccount(int timeWorkAccountId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twam.DeleteTimeWorkAccount(timeWorkAccountId));
        }

        #endregion

        #region TimeWorkAccountYear

        [HttpGet]
        [Route("TimeWorkAccountYear/{timeWorkAccountYearId:int}/{timeWorkAccountId:int}/{includeEmployees:bool}/{loadWorkTimeWeek:bool}")]
        public IHttpActionResult GetTimeWorkAccountYear(int timeWorkAccountYearId, int timeWorkAccountId, bool includeEmployees, bool loadWorkTimeWeek)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccountYear(timeWorkAccountYearId, timeWorkAccountId, loadEmployees: includeEmployees, loadWorkTimeWeek).ToDTO());
        }

        [HttpGet]
        [Route("TimeWorkAccountLastYear/{timeWorkAccountId:int}/{addYear:bool}")]
        public IHttpActionResult GetTimeWorkAccountLastYear(int timeWorkAccountId, bool addYear)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccountLastYear(timeWorkAccountId).ToDTO(addYear));
        }

        [HttpPost]
        [Route("TimeWorkAccountYear")]
        public IHttpActionResult SaveTimeWorkAccountYear(TimeWorkAccountYearDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twam.SaveTimeWorkAccountYear(model));
        }

        [HttpDelete]
        [Route("TimeWorkAccountYear/{timeWorkAccountYearId:int}/{timeWorkAccountId:int}")]
        public IHttpActionResult DeleteTimeWorkAccountYear(int timeWorkAccountYearId, int timeWorkAccountId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twam.DeleteTimeWorkAccountYear(timeWorkAccountYearId, timeWorkAccountId));
        }


        [HttpPost]
        [Route("TimeWorkAccountYear/CalculateYearEmployee")]
        public IHttpActionResult CalculateYearEmployee(TimeWorkAccountYearEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.CalculateTimeWorkAccountYearEmployee(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.TimeWorkAccountYearEmployeeIds, model.EmployeeIds));
        }

        [HttpPost]
        [Route("TimeWorkAccountYear/SendSelection")]
        public IHttpActionResult SendSelection(TimeWorkAccountYearEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkAccountChoiceSendXEMail(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.TimeWorkAccountYearEmployeeIds));
        }

        [HttpGet]
        [Route("TimeWorkAccountYear/CalculationBasis/{timeWorkAccountYearEmployeeId:int}/{employeeId:int}")]
        public IHttpActionResult GetCalculationBasis(int timeWorkAccountYearEmployeeId, int employeeId)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccountYearEmployeeCalculationBasis(timeWorkAccountYearEmployeeId, employeeId));
        }

        [HttpPost]
        [Route("TimeWorkAccountYear/GetPensionExport")]
        public IHttpActionResult GetPensionExport(TimeWorkAccountYearEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twam.GetTimeWorkAccountYearPensionExport(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.TimeWorkAccountYearEmployeeIds, base.RoleId));
        }

        [HttpPost]
        [Route("TimeWorkAccountYear/GenerateOutcome")]
        public IHttpActionResult GenerateOutcome(TimeWorkAccountGenerateOutcomeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkAccountYearGenerateOutcome(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.OverrideChoosen, model.PaymentDate, model.TimeWorkAccountYearEmployeeIds));
        }
        [HttpPost]
        [Route("TimeWorkAccountYear/GenerateUnusedPaidBalance")]
        public IHttpActionResult GenerateUnPaidBalance(TimeWorkAccountGenerateOutcomeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkAccountGenerateUnusedPaidBalance(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.PaymentDate, model.TimeWorkAccountYearEmployeeIds));
        }
        [HttpGet]
        [Route("TimeWorkAccountYear/TimePeriods")]
        public IHttpActionResult GetPayrollTimePeriods()
        {
            return Content(HttpStatusCode.OK, rm.GetReportPayrollTimePeriods(base.ActorCompanyId, true).OrderBy(t => t.PaymentDate));
        }

        [HttpGet]
        [Route("TimeWorkAccountYear/GetPayrollProductIdsByType/{level1:int}/{level2:int}")]
        public IHttpActionResult GetPayrollProductIdsByType(int level1, int level2)
        {
            return Content(HttpStatusCode.OK, prm.GetPayrollProductIdsByType(base.ActorCompanyId, level1, level2, null, null));
        }
        [HttpGet]
        [Route("TimeWorkAccountYear/GetPayrollProductsSmall")]
        public IHttpActionResult GetPayrollProductsSmall()
        {
            return Content(HttpStatusCode.OK, prm.GetPayrollProducts(base.ActorCompanyId, true).ToSmallDTOs());
        }
        [HttpGet]
        [Route("TimeWorkAccountYear/GetPaymentDate/{timeWorkAccountYearId:int}/{timeWorkAccountYearEmployeeId:int}/")]
        public IHttpActionResult GetPaymentDate(int timeWorkAccountYearId, int timeWorkAccountYearEmployeeId)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccountYearEmployeePaymentDate(timeWorkAccountYearId, timeWorkAccountYearEmployeeId));
        }
        [HttpGet]
        [Route("TimeWorkAccountYear/GetTimeAccumulators")]
        public IHttpActionResult GetTimeAccumulators()
        {
            return Content(HttpStatusCode.OK, tam.GetTimeAccumulators(base.ActorCompanyId, onlyActive: true, timeWorkAccount: true).ToDTOs());
        }

        [HttpPost]
        [Route("TimeWorkAccountYear/ReverseTransaction")]
        public IHttpActionResult ReverseTransaction(TimeWorkAccountYearEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkAccountYearReverseTransaction(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.TimeWorkAccountYearEmployeeIds));
        }

        [HttpPost]
        [Route("TimeWorkAccountYear/ReversePaidBalance")]
        public IHttpActionResult ReversePaidBalance(TimeWorkAccountYearEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.TimeWorkAccountYearReversePaidBalance(model.TimeWorkAccountId, model.TimeWorkAccountYearId, model.TimeWorkAccountYearEmployeeIds));
        }

        [HttpDelete]
        [Route("TimeWorkAccountYear/DeleteTimeWorkAccountYearEmployeeRow/{timeWorkAccountYearId:int}/{timeWorkAccountYearEmployeeId:int}/{employeeId:int}")]
        public IHttpActionResult DeleteTimeWorkAccountYearEmployeeRow(int timeWorkAccountYearId, int timeWorkAccountYearEmployeeId, int employeeId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, twam.DeleteTimeWorkAccountYearEmployeeRow(timeWorkAccountYearId, timeWorkAccountYearEmployeeId, employeeId));
        }

        #endregion

    }
}