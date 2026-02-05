using SoftOne.Soe.Business.Billing.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using SoftOne.Soe.Business.Core.Template.Models.Time;
using SoftOne.Soe.Business.Template.Managers;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Template/Time")]
    public class TimeTemplateController : ApiBase
    {
        #region Constructor

        public TimeTemplateController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all DayTypeCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of DayTypeCopyItems</returns>
        [HttpGet]
        [Route("DayTypeCopyItems")]
        [ResponseType(typeof(List<DayTypeCopyItem>))]
        public IHttpActionResult GetDayTypeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<DayTypeCopyItem> dayTypeCopyItems = timeTemplateManager.GetDayTypeCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, dayTypeCopyItems);
        }

        /// <summary>
        /// Get all TimeHalfDayCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimeHalfDayCopyItems</returns>
        [HttpGet]
        [Route("TimeHalfDayCopyItems")]
        [ResponseType(typeof(List<TimeHalfDayCopyItem>))]
        public IHttpActionResult GetTimeHalfDayCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeHalfDayCopyItem> timeHalfDayCopyItems = timeTemplateManager.GetTimeHalfDayCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeHalfDayCopyItems);
        }

        /// <summary>
        /// Get all HolidayCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of HolidayCopyItems</returns>
        [HttpGet]
        [Route("HolidayCopyItems")]
        [ResponseType(typeof(List<HolidayCopyItem>))]
        public IHttpActionResult GetHolidayCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<HolidayCopyItem> holidayCopyItems = timeTemplateManager.GetHolidayCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, holidayCopyItems);
        }
        #endregion

        /// <summary>
        /// Get all TimePeriodHeadCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimePeriodHeadCopyItems</returns>
        [HttpGet]
        [Route("TimePeriodHeadCopyItems")]
        [ResponseType(typeof(List<TimePeriodHeadCopyItem>))]
        public IHttpActionResult GetTimePeriodHeadCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimePeriodHeadCopyItem> timePeriodHeadCopyItems = timeTemplateManager.GetTimePeriodHeadCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, timePeriodHeadCopyItems);
        }

        /// <summary>
        /// Get all PositionCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PositionCopyItems</returns>
        [HttpGet]
        [Route("PositionCopyItems")]
        [ResponseType(typeof(List<PositionCopyItem>))]
        public IHttpActionResult GetPositionCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<PositionCopyItem> positionCopyItems = timeTemplateManager.GetPositionCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, positionCopyItems);
        }

        /// <summary>
        /// Get all PayrollPriceTypeCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PayrollPriceTypeCopyItems</returns>
        [HttpGet]
        [Route("PayrollPriceTypeCopyItems")]
        [ResponseType(typeof(List<PayrollPriceTypeCopyItem>))]
        public IHttpActionResult GetPayrollPriceTypeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<PayrollPriceTypeCopyItem> payrollPriceTypeCopyItems = timeTemplateManager.GetPayrollPriceTypeCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, payrollPriceTypeCopyItems);
        }

        /// <summary>
        /// Get all PayrollPriceFormulaCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PayrollPriceFormulaCopyItems</returns>
        [HttpGet]
        [Route("PayrollPriceFormulaCopyItems")]
        [ResponseType(typeof(List<PayrollPriceFormulaCopyItem>))]
        public IHttpActionResult GetPayrollPriceFormulaCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<PayrollPriceFormulaCopyItem> payrollPriceFormulaCopyItems = timeTemplateManager.GetPayrollPriceFormulaCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, payrollPriceFormulaCopyItems);
        }


        /// <summary>
        /// Get all VacationGroupCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of VacationGroupCopyItems</returns>
        [HttpGet]
        [Route("VacationGroupCopyItems")]
        [ResponseType(typeof(List<VacationGroupCopyItem>))]
        public IHttpActionResult GetVacationGroupCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<VacationGroupCopyItem> vacationGroupCopyItems = timeTemplateManager.GetVacationGroupCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, vacationGroupCopyItems);
        }

        [HttpGet]
        [Route("TimeScheduleTypeCopyItems")]
        [ResponseType(typeof(List<TimeScheduleTypeCopyItem>))]
        public IHttpActionResult GetTimeScheduleTypeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeScheduleTypeCopyItem> timeScheduleTypeCopyItems = timeTemplateManager.GetTimeScheduleTypeCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, timeScheduleTypeCopyItems);
        }

        [HttpGet]
        [Route("ShiftTypeCopyItems")]
        [ResponseType(typeof(List<ShiftTypeCopyItem>))]
        public IHttpActionResult GetShiftTypeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<ShiftTypeCopyItem> shiftTypeCopyItems = timeTemplateManager.GetShiftTypeCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, shiftTypeCopyItems);
        }

        /// <summary>
        /// Get all SkillCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of SkillCopyItems</returns>
        [HttpGet]
        [Route("SkillCopyItems")]
        [ResponseType(typeof(List<SkillCopyItem>))]
        public IHttpActionResult GetSkillCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<SkillCopyItem> skillCopyItems = timeTemplateManager.GetSkillCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, skillCopyItems);
        }

        /// <summary>
        /// Get all ScheduleCycleCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of ScheduleCycleCopyItems</returns>
        [HttpGet]
        [Route("ScheduleCycleCopyItems")]
        [ResponseType(typeof(List<ScheduleCycleCopyItem>))]
        public IHttpActionResult GetScheduleCycleCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<ScheduleCycleCopyItem> scheduleCycleCopyItems = timeTemplateManager.GetScheduleCycleCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, scheduleCycleCopyItems);
        }

        /// <summary>
        /// Get all InvoiceProductCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of InvoiceProductCopyItems</returns>
        [HttpGet]
        [Route("InvoiceProductCopyItems")]
        [ResponseType(typeof(List<InvoiceProductCopyItem>))]
        public IHttpActionResult GetInvoiceProductCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<InvoiceProductCopyItem> invoiceProductCopyItems = timeTemplateManager.GetInvoiceProductCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, invoiceProductCopyItems);
        }

        /// <summary>
        /// Get all productUnitCopyItems for the specified actorCompanyId
        /// </summary>
        /// returns>List of ProductUnitCopyItem</returns>
        [HttpGet]
        [Route("ProductUnitCopyItems")]
        [ResponseType(typeof(List<ProductUnitCopyItem>))]
        public IHttpActionResult GetProductUnitCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(parameterObject);
            List<ProductUnitCopyItem> productUnitCopyItems = billingTemplateManager.GetProductUnitCopyItems(actorCompanyId);
            return Content(HttpStatusCode.OK, productUnitCopyItems);
        }



        /// <summary>
        /// Get all PayrollProductCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PayrollProductCopyItems</returns>
        [HttpGet]
        [Route("PayrollProductCopyItems")]
        [ResponseType(typeof(List<PayrollProductCopyItem>))]
        public IHttpActionResult GetPayrollProductCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<PayrollProductCopyItem> payrollProductCopyItems = timeTemplateManager.GetPayrollProductCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, payrollProductCopyItems);
        }

        /// <summary>
        /// Get all PayrollGroupCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of PayrollGroupCopyItems</returns>
        [HttpGet]
        [Route("PayrollGroupCopyItems")]
        [ResponseType(typeof(List<PayrollGroupCopyItem>))]
        public IHttpActionResult GetPayrollGroupCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<PayrollGroupCopyItem> payrollGroupCopyItems = timeTemplateManager.GetPayrollGroupCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, payrollGroupCopyItems);
        }

        [HttpGet]
        [Route("EmployeeGroupCopyItems")]
        [ResponseType(typeof(List<EmployeeGroupCopyItem>))]
        public IHttpActionResult GetEmployeeGroupCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<EmployeeGroupCopyItem> employeeGroupCopyItems = timeTemplateManager.GetEmployeeGroupCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, employeeGroupCopyItems);
        }


        /// <summary>
        /// Get all TimeCodeCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimeCodeCopyItems</returns>
        [HttpGet]
        [Route("TimeCodeCopyItems")]
        [ResponseType(typeof(List<TimeCodeCopyItem>))]
        public IHttpActionResult GetTimeCodeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeCodeCopyItem> timeCodeCopyItems = timeTemplateManager.GetTimeCodeCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeCodeCopyItems);
        }

        /// <summary>
        /// Get all TimeBreakTemplateCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimeBreakTemplateCopyItems</returns>
        [HttpGet]
        [Route("TimeBreakTemplateCopyItems")]
        [ResponseType(typeof(List<TimeBreakTemplateCopyItem>))]
        public IHttpActionResult GetTimeBreakTemplateCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeBreakTemplateCopyItem> timeBreakTemplateCopyItems = timeTemplateManager.GetTimeBreakTemplateCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeBreakTemplateCopyItems);
        }

        /// <summary>
        /// Get all TimeCodeBreakGroupCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimeCodeBreakGroupCopyItems</returns>
        [HttpGet]
        [Route("TimeCodeBreakGroupCopyItems")]
        [ResponseType(typeof(List<TimeCodeBreakGroupCopyItem>))]
        public IHttpActionResult GetTimeCodeBreakGroupCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeCodeBreakGroupCopyItem> timeCodeBreakGroupCopyItems = timeTemplateManager.GetTimeCodeBreakGroupCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeCodeBreakGroupCopyItems);
        }


        /// <summary>
        /// Get all TimeCodeRankingGroupCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimeCodeRankingGroupCopyItems</returns>
        [HttpGet]
        [Route("TimeCodeRankingGroupCopyItems")]
        [ResponseType(typeof(List<TimeCodeRankingGroupCopyItem>))]
        public IHttpActionResult GetTimeCodeRankingGroupCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeCodeRankingGroupCopyItem> timeCodeRankingGroupCopyItems = timeTemplateManager.GetTimeCodeRankingGroupCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeCodeRankingGroupCopyItems);
        }

        /// <summary>
        /// Get all TimeDeviationCauseCopyItems for the specified actorCompanyId
        /// </summary>
        /// <returns>List of TimeDeviationCauseCopyItems</returns>
        [HttpGet]
        [Route("TimeDeviationCauseCopyItems")]
        [ResponseType(typeof(List<TimeDeviationCauseCopyItem>))]
        public IHttpActionResult GetTimeDeviationCauseCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeDeviationCauseCopyItem> timeDeviationCauseCopyItems = timeTemplateManager.GetTimeDeviationCauseCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeDeviationCauseCopyItems);
        }

        [HttpGet]
        [Route("EmploymentTypeCopyItems")]
        [ResponseType(typeof(List<EmploymentTypeCopyItem>))]
        public IHttpActionResult GetEmploymentTypeCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<EmploymentTypeCopyItem> employmentTypeCopyItems = timeTemplateManager.GetEmploymentTypeCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, employmentTypeCopyItems);
        }


        [HttpGet]
        [Route("TimeAccumulatorCopyItems")]
        [ResponseType(typeof(List<TimeAccumulatorCopyItem>))]
        public IHttpActionResult GetTimeAccumulatorCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeAccumulatorCopyItem> timeAccumulatorCopyItems = timeTemplateManager.GetTimeAccumulatorCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeAccumulatorCopyItems);
        }

        [HttpGet]
        [Route("TimeRuleCopyItems")]
        [ResponseType(typeof(List<TimeRuleCopyItem>))]
        public IHttpActionResult GetTimeRuleCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeRuleCopyItem> timeRuleCopyItems = timeTemplateManager.GetTimeRuleCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeRuleCopyItems);
        }

        [HttpGet]
        [Route("TimeAbsenceRuleCopyItems")]
        [ResponseType(typeof(List<TimeAbsenceRuleCopyItem>))]
        public IHttpActionResult GetTimeAbsenceRuleCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeAbsenceRuleCopyItem> timeAbsenceRuleCopyItems = timeTemplateManager.GetTimeAbsenceRuleCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeAbsenceRuleCopyItems);
        }

        [HttpGet]
        [Route("TimeAttestRuleCopyItems")]
        [ResponseType(typeof(List<TimeAttestRuleCopyItem>))]
        public IHttpActionResult GetTimeAttestRuleCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<TimeAttestRuleCopyItem> timeAttestRuleCopyItems = timeTemplateManager.GetTimeAttestRuleCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, timeAttestRuleCopyItems);
        }


        [HttpGet]
        [Route("EmployeeCollectiveAgreementCopyItems")]
        [ResponseType(typeof(List<EmployeeCollectiveAgreementCopyItem>))]
        public IHttpActionResult GetEmployeeCollectiveAgreementCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<EmployeeCollectiveAgreementCopyItem> employeeCollectiveAgreementCopyItems = timeTemplateManager.GetEmployeeCollectiveAgreementCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, employeeCollectiveAgreementCopyItems);
        }

        [HttpGet]
        [Route("EmployeeTemplateCopyItems")]
        [ResponseType(typeof(List<EmployeeTemplateCopyItem>))]
        public IHttpActionResult GetEmployeeTemplateCopyItems(int actorCompanyId)
        {
            ParameterObject parameterObject = GetParameterObject(actorCompanyId, 0);
            TimeTemplateManager timeTemplateManager = new TimeTemplateManager(parameterObject);
            List<EmployeeTemplateCopyItem> employeeTemplateCopyItems = timeTemplateManager.GetEmployeeTemplateCopyItems(actorCompanyId);

            return Content(HttpStatusCode.OK, employeeTemplateCopyItems);
        }
    }
}