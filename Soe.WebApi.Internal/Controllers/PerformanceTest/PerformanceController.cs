using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.PerformanceTest
{
    [RoutePrefix("PerformanceTest/Performance")]
    public class PerformanceController : ApiBase
    {
        #region Variables

        private EmployeeManager employeeManager;

        #endregion

        #region Constructor

        public PerformanceController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.employeeManager = new EmployeeManager(null);
        }

        #endregion

        [HttpGet]
        [Route("AllEmployees")]
        [ResponseType(typeof(List<EmployeeDTO>))]
        public IHttpActionResult GetAllEmployees(int actorCompanyId, string superKey, Guid superKeyGuid)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                return Content(HttpStatusCode.OK, employeeManager.GetAllEmployees(actorCompanyId, loadEmployment: true).ToDTOs());
            }
            else return Content(HttpStatusCode.Unauthorized, new List<EmployeeDTO>());
        }

        [HttpGet]
        [Route("GetScheduleJobs")]
        [ResponseType(typeof(ReportPrintoutDTO))]
        public IHttpActionResult GetScheduleJobs(string superKey, Guid superKeyGuid)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                SysScheduledJobManager jm = new SysScheduledJobManager(null);
                return Content(HttpStatusCode.OK, jm.GetScheduledJobs(null).ToDTOs(false, false, false).ToList());
            }
            else return Content(HttpStatusCode.Unauthorized, new List<SysScheduledJobDTO>());
        }

        [HttpGet]
        [Route("GetCompanies")]
        [ResponseType(typeof(List<CompanyDTO>))]
        public IHttpActionResult GetCompanies(string superKey, Guid superKeyGuid)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                CompanyManager cm = new CompanyManager(null);
                return Content(HttpStatusCode.OK, cm.GetCompanies(loadLicense: true).ToCompanyDTOs().ToList());
            }
            else return Content(HttpStatusCode.Unauthorized, new List<SysScheduledJobDTO>());
        }


        [HttpPost]
        [Route("GetTimeSchedulePlanningMonthDTO/")]
        [ResponseType(typeof(IEnumerable<TimeSchedulePlanningMonthDTO>))]
        public IHttpActionResult GetTimeSchedulePlanningMonthDTO(string superKey, Guid superKeyGuid, GetShiftPeriodsModel model, int actorCompanyId, int userId, int roleId)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                TimeScheduleManager tsm = new TimeScheduleManager(GetParameterObject(actorCompanyId, userId));
                return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningPeriods_ByProcedure(actorCompanyId, userId, roleId, model.DateFrom, model.DateTo, model.EmployeeId, model.DisplayMode, model.BlockTypes, model.EmployeeIds, model.ShiftTypeIds, null, null, model.DeviationCauseIds, model.IncludeGrossNetAndCost, false, model.IncludePreliminary, model.IncludeEmploymentTaxAndSupplementChargeCost));

            }
            else return Content(HttpStatusCode.Unauthorized,"");
        }

        [HttpGet]
        [Route("GetTimeAttestTree")]
        [ResponseType(typeof(TimeEmployeeTreeDTO))]
        public IHttpActionResult GetTimeAttestTree(string superKey, Guid superKeyGuid, DateTime startDate, DateTime stopDate, int actorCompanyId, int userId)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                TimeTreeAttestManager ttam = new TimeTreeAttestManager(GetParameterObject(actorCompanyId, userId));
                return Content(HttpStatusCode.OK, ttam.GetAttestTree(TermGroup_AttestTreeGrouping.EmployeeAuthModel, TermGroup_AttestTreeSorting.EmployeeNr, startDate, stopDate, null));
            }
            else return Content(HttpStatusCode.Unauthorized, (TimeEmployeeTreeDTO)null);
        }

        [HttpGet]
        [Route("GetInvoices")]
        [ResponseType(typeof(List<CustomerInvoiceGridDTO>))]
        public IHttpActionResult GetInvoices(string superKey, Guid superKeyGuid, int actorCompanyId)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                InvoiceManager am = new InvoiceManager(GetParameterObject(actorCompanyId, 0));
                return Content(HttpStatusCode.OK, am.GetCustomerInvoicesForGrid(SoeOriginStatusClassification.CustomerInvoicesAll,(int)SoeOriginType.CustomerInvoice, actorCompanyId,0,true,true,false,false,TermGroup_ChangeStatusGridAllItemsSelection.All,false));
            }
            else return Content(HttpStatusCode.Unauthorized, new List<CustomerInvoiceGridDTO>());
        }

        [HttpGet]
        [Route("GetEmployeesTimeStamp")]
        [ResponseType(typeof(List<ChangeStatusGridViewDTO>))]
        public IHttpActionResult GetEmployeesTimeStamp(string superKey, Guid superKeyGuid, int actorCompanyId)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                TimeStampManager am = new TimeStampManager(GetParameterObject(actorCompanyId, 0));
                return Content(HttpStatusCode.OK, am.SyncEmployee(actorCompanyId, DateTime.Now.AddYears(-10)));
            }
            else return Content(HttpStatusCode.Unauthorized, new List<ChangeStatusGridViewDTO>());
        }

        [HttpPost]
        [Route("PrintReport")]
        [ResponseType(typeof(ReportPrintoutDTO))]
        public IHttpActionResult PrintReportGetData(string superKey, Guid superKeyGuid, EvaluatedSelection es, int actorCompanyId, int userId, string culture)
        {
            if (SoftOneIdConnector.ValidateSuperKey(superKeyGuid, superKey))
            {
                SetLanguage(culture);

                ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId));
                return Content(HttpStatusCode.OK, rdm.PrintReportDTO(es));
            }
            else return Content(HttpStatusCode.Unauthorized, new ReportPrintoutDTO());
        }
    }

    public class GetShiftPeriodsModel
    {
        public int EmployeeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<int> EmployeeIds { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public List<int> DeviationCauseIds { get; set; }
        public List<SoftOne.Soe.Common.Util.TermGroup_TimeScheduleTemplateBlockType> BlockTypes { get; set; }
        public SoftOne.Soe.Common.Util.TimeSchedulePlanningDisplayMode DisplayMode { get; set; }
        public bool IncludeGrossNetAndCost { get; set; }
        public bool IncludePreliminary { get; set; }
        public bool IncludeEmploymentTaxAndSupplementChargeCost { get; set; }
    }
}