using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/ProjectExpense")]
    public class ProjectExpenseController : SoeApiController
    {
        #region Variables

        private readonly ExpenseManager em;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public ProjectExpenseController(ExpenseManager em, TimeEngineManager tem)
        {
            this.em = em;
            this.tem = tem;
        }

        #endregion

        #region ExpenseRow

        [HttpGet]
        [Route("Row/{expenseRowId:int}")]
        public IHttpActionResult GetExpenseRow(int expenseRowId)
        {
            return Content(HttpStatusCode.OK, em.GetExpenseRowForDialog(expenseRowId, true));
        }

        [HttpGet]
        [Route("Rows/{customerInvoiceId:int}/{customerInvoiceRowId:int}")]
        public IHttpActionResult GetExpenseRows(int customerInvoiceId, int customerInvoiceRowId)
        {
            return Content(HttpStatusCode.OK, em.GetExpenseRowsForGrid(customerInvoiceId, base.ActorCompanyId, base.UserId, base.RoleId, false, customerInvoiceRowId, true));
        }

        [HttpPost]
        [Route("Rows/Filtered")]
        public IHttpActionResult GetExpenseRowsForGridFiltered(FilterExpensesModel model)
        {
            return Content(HttpStatusCode.OK, em.GetExpenseRowsForGridFiltered(base.ActorCompanyId, base.UserId, base.RoleId, model.EmployeeId, model.From, model.To, model.Employees, model.Projects, model.Orders, model.EmployeeCategories));
        }

        [HttpPost]
        [Route("Rows/Validate")]
        public IHttpActionResult SaveExpenseRowsValidation(ExpenseRowsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveExpenseValidation(model.ExpenseRows[0]));
        }

        [HttpPost]
        [Route("Rows/")]
        public IHttpActionResult SaveExpenseRow(ExpenseRowsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveExpense(model.ExpenseRows[0], model.CustomerInvoiceId, false));
        }

        [HttpDelete]
        [Route("Row/{expenseRowId:int}")]
        public IHttpActionResult DeleteExpenseRow(int expenseRowId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteExpense(expenseRowId));
        }

        #endregion

        #region Expense
        [HttpGet]
        [Route("Report/{invoiceId:int}/{projectId:int}")]
        public IHttpActionResult getExpenseReportUrl(int invoiceId, int projectId)
        {
            //To be change to new expense report
            SettingManager sm = new SettingManager(null);
            int billingExpenseReportId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultExpenseReportTemplate, base.UserId, base.ActorCompanyId, 0);
            var reportItem = new BillingInvoiceExpenseReportDTO(base.ActorCompanyId, billingExpenseReportId, (int)SoeReportTemplateType.ExpenseReport, new BillingInvoiceReportDTO(base.ActorCompanyId, billingExpenseReportId, (int)SoeReportTemplateType.ExpenseReport, invoiceId, "", invoiceCopy: false, invoiceReminder: false, disableInvoiceCopies: false, includeProjectReport: true, includeOnlyInvoiced: false, reportLanguageId: 0));
            ReportManager rm = new ReportManager(null);
            rm.SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), billingExpenseReportId, (int)SoeReportTemplateType.ExpenseReport, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, reportItem.ToShortString(true));
        }
        #endregion
    }
}
