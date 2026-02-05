using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using SoftOne.Soe.Common.DTO;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/HandleBilling")]
    public class HandleBillingController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;
        private readonly ProjectManager pm;
        private readonly CustomerManager cm;
        private readonly ExpenseManager em;

        #endregion

        #region Constructor

        public HandleBillingController(InvoiceManager im, ProjectManager pm, CustomerManager cm, ExpenseManager em)
        {
            this.im = im;
            this.pm = pm;
            this.cm = cm;
            this.em = em;
        }

        #endregion

        #region HandleBilling

        [HttpPost]
        [Route("Search/")]
        public IHttpActionResult SearchCustomerInvoiceRows(SearchCustomerInvoiceRowModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SearchCustomerInvoiceRows(base.ActorCompanyId, model.projects, model.orders, model.customers, model.orderTypes, model.orderContractTypes, model.From, model.To, model.onlyValid, model.onlyMine, model.customerInvoiceRowId));
        }

        [HttpPost]
        [Route("ChangeAttestState")]
        public IHttpActionResult OrderRowChangeAttestState(OrderRowChangeAttestStateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.ChangeAttestStateOnOrderRows(model.Items, model.AttestStateId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TransferOrdersToInvoice")]
        public IHttpActionResult TransferOrdersToInvoice(TransferOrdersToInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.CreateInvoiceFromOrders(model.Ids, model.Merge, model.SetStatusToOrigin, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("BatchSplitTimeRows")]
        public IHttpActionResult BatchSplitTimeRows(BatchSplitTimeRowsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.MoveTimeRowsToNewCustomerInvoiceRow(model.Items, model.From, model.To));
        }

        [HttpGet]
        [Route("ExpenseRows/{customerInvoiceId:int}/{customerInvoiceRowId:int}")]
        public IHttpActionResult GetExpenseRows(int customerInvoiceId, int customerInvoiceRowId)
        {
            return Content(HttpStatusCode.OK, em.GetExpenseRowsForGrid(customerInvoiceId, base.ActorCompanyId, base.UserId, base.RoleId, false, customerInvoiceRowId, true));
        }

        [HttpGet]
        [Route("ProjectTimeBlock/{invoiceId:int}/{customerInvoiceRowId:int}")]
        public IHttpActionResult GetProjectTimeBlocksForInvoiceRow(int invoiceId, int customerInvoiceRowId)
        {
            return Content(HttpStatusCode.OK, pm.GetProjectTimeBlocksForInvoiceRow(invoiceId, customerInvoiceRowId, null, null));
        }

        #endregion

        #region Project

        [HttpGet]
        [Route("Projects/")]
        public IHttpActionResult GetProjects()
        {
            return Content(HttpStatusCode.OK, pm.GetProjects(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, true, true, false, false, false).Select(p => new SmallGenericType() { Id = p.ProjectId, Name = p.Number + " " + p.Name }).ToList());
        }

        #endregion

        #region Customer

        [HttpGet]
        [Route("Customers/")]
        public IHttpActionResult GetCustomers()
        {
            return Content(HttpStatusCode.OK, cm.GetCustomersByCompany(base.ActorCompanyId, true).Select(c => new SmallGenericType() { Id = c.ActorCustomerId, Name = c.CustomerNr + " " + c.Name }).ToList());
        }

        #endregion

        #region Orders

        [HttpGet]
        [Route("Orders/")]
        public IHttpActionResult GetOrders()
        {
            return Content(HttpStatusCode.OK, im.GetOpenOrders());
        }

        #endregion
    }
}