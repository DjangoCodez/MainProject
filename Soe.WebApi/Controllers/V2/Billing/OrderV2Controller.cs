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

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Order")]
    public class OrderV2Controller : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;
        private readonly ProjectManager pm;
        private readonly EdiManager em;
        private readonly AccountManager acm;
        private readonly AttestManager am;
        private readonly CustomerManager cm;


        #endregion

        #region Constructor

        public OrderV2Controller(InvoiceManager im, ProjectManager pm, EdiManager em, AccountManager acm, AttestManager am,CustomerManager cm)
        {
            this.im = im;
            this.pm = pm;
            this.em = em;
            this.acm = acm;
            this.am = am;
            this.cm = cm;
        }

        #endregion


        #region Order

        [HttpGet]
        [Route("{invoiceId:int}/{includeCategories:bool}/{includeRows:bool}")]
        public IHttpActionResult GetOrder(int invoiceId, bool includeCategories, bool includeRows)
        {
            return Content(HttpStatusCode.OK, im.GetOrder(invoiceId, includeCategories, includeRows, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountRows/{invoiceId:int}")]
        public IHttpActionResult GetAccountRows(int invoiceId)
        {
            List<AccountDim> dims = acm.GetAccountDimsByCompany(ActorCompanyId);
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceAccountRows(invoiceId).ToDTOs(dims));
        }


        [HttpGet]
        [Route("SplitAccountingRows/{customerInvoiceRowId:int}/{excludeVatRows:bool}")]
        public IHttpActionResult GetSplitAccountingRows(int customerInvoiceRowId, bool excludeVatRows)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceAccountRowsForCustomerInvoiceRow(customerInvoiceRowId, excludeVatRows).ToSplitDTOs());
        }

        [HttpGet]
        [Route("Template")]
        public IHttpActionResult GetOrderTemplates()
        {
            return Content(HttpStatusCode.OK, im.GetInvoiceTemplatesDict(base.ActorCompanyId, SoeOriginType.Order, SoeInvoiceType.CustomerInvoice).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Templates/{originType:int}")]
        public IHttpActionResult GetOrderTemplates(int originType)
        {
            return Content(HttpStatusCode.OK, im.GetInvoiceTemplatesDict(base.ActorCompanyId, (SoeOriginType)originType, SoeInvoiceType.CustomerInvoice).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Edi")]
        public IHttpActionResult UseEdi()
        {
            return Content(HttpStatusCode.OK, em.CompanyUsesEdi(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("GetEdiEntryInfo/{ediEntryId:int}")]
        public IHttpActionResult GetEdiEntryInfo(int ediEntryId)
        {
            return Content(HttpStatusCode.OK, em.GetEdiEntry(ediEntryId, base.ActorCompanyId, true).ToDTO(false, false));
        }

        [HttpGet]
        [Route("OriginUsers/{invoiceId:int}")]
        public IHttpActionResult OriginUsers(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetOriginUsers(ActorCompanyId, invoiceId).ToDTOs());
        }

        [HttpGet]
        [Route("CanUserCreateInvoice/{currentAttestStateId:int}")]
        public IHttpActionResult CanUserCreateInvoice(int currentAttestStateId)
        {
            return Content(HttpStatusCode.OK, am.CanUserCreateInvoice(base.ActorCompanyId, base.UserId, currentAttestStateId));
        }

        [HttpGet]
        [Route("GetOrderTraceViews/{orderId:int}")]
        public IHttpActionResult GetOrderTraceViews(int orderId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, im.GetOrderTraceViews(orderId, baseSysCurrencyId));
        }

        [HttpGet]
        [Route("CreditLimit/{customerId:int}/{creditLimit:int}")]
        public IHttpActionResult CheckCustomerCreditLimit(int customerId, int creditLimit)
        {
            return Content(HttpStatusCode.OK, cm.CheckCustomerCreditLimit(base.ActorCompanyId, customerId, creditLimit));
        }

        [HttpGet]
        [Route("OpenDict/")]
        public IHttpActionResult GetOpenOrdersDict()
        {
            return Content(HttpStatusCode.OK, im.GetOpenOrders());
        }

        [HttpGet]
        [Route("Summary/{invoiceId:int}/{projectId:int}")]
        public IHttpActionResult GetOrderSummary(int invoiceId, int projectId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceSummary(invoiceId, projectId > 0 ? projectId : (int?)null));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveOrder(SaveOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveOrder(model.ModifiedFields, model.NewRows, model.ModifiedRows, model.ChecklistHeads, model.ChecklistRows, model.OriginUsers, model.Files, model.DiscardConcurrencyCheck, model.RegenerateAccounting, model.SendXEMail, model.AutoSave));
        }

        [HttpPost]
        [Route("Unlock/{orderId:int}")]
        public IHttpActionResult UnlockOrder(int orderId)
        {
            return Content(HttpStatusCode.OK, im.UnlockCustomerInvoice(orderId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Close/{orderId:int}")]
        public IHttpActionResult CloseOrder(int orderId)
        {
            return Content(HttpStatusCode.OK, im.CloseOrderOffer(orderId));
        }

        [HttpPost]
        [Route("UpdateReadyState/{orderId:int}/{userId:int}")]
        public IHttpActionResult UpdateReadyState(int orderId, int userId)
        {
            return Content(HttpStatusCode.OK, im.UpdateOrderReadyState(orderId, userId, false));
        }

        [HttpPost]
        [Route("SendReminderForReadyState/{orderId:int}/{orderNr}/{userIds}")]
        public IHttpActionResult SendReminderForReadyState(int orderId, string orderNr, string userIds)
        {
            var ids = string.IsNullOrEmpty(userIds) ? new List<int>() : userIds.Split(',').Select(Int32.Parse).ToList();

            return Content(HttpStatusCode.OK, im.SendReminderForReadyState(base.UserId, orderId, ids, base.ActorCompanyId, orderNr));
        }

        [HttpPost]
        [Route("ClearReadyState/{orderId}/{userIds}")]
        public IHttpActionResult ClearReadyState(int orderId, string userIds)
        {
            var ids = string.IsNullOrEmpty(userIds) ? new List<int>() : userIds.Split(',').Select(Int32.Parse).ToList();

            return Content(HttpStatusCode.OK, im.ClearReadyState(orderId, ids, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SetOrderKeepAsPlanned/{orderId:int}/{keepAsPlanned:bool}")]
        public IHttpActionResult SetOrderKeepAsPlanned(int orderId, bool keepAsPlanned)
        {
            return Content(HttpStatusCode.OK, im.SetOrderKeepAsPlanned(orderId, base.ActorCompanyId, keepAsPlanned));
        }

        [HttpPost]
        [Route("HandleBilling/Search/")]
        public IHttpActionResult SearchCustomerInvoiceRows(SearchCustomerInvoiceRowModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SearchCustomerInvoiceRows(base.ActorCompanyId, model.projects, model.orders, model.customers, model.orderTypes, model.orderContractTypes, model.From, model.To, model.onlyValid, model.onlyMine));
        }

        [HttpPost]
        [Route("HandleBilling/ChangeAttestState")]
        public IHttpActionResult OrderRowChangeAttestState(OrderRowChangeAttestStateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.ChangeAttestStateOnOrderRows(model.Items, model.AttestStateId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("HandleBilling/TransferOrdersToInvoice")]
        public IHttpActionResult TransferOrdersToInvoice(TransferOrdersToInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.CreateInvoiceFromOrders(model.Ids, model.Merge, model.SetStatusToOrigin, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("HandleBilling/BatchSplitTimeRows")]
        public IHttpActionResult BatchSplitTimeRows(BatchSplitTimeRowsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.MoveTimeRowsToNewCustomerInvoiceRow(model.Items, model.From, model.To));
        }

        [HttpDelete]
        [Route("{invoiceId:int}/{deleteProject:bool}")]
        public IHttpActionResult DeleteOrder(int invoiceId, bool deleteProject)
        {
            return Content(HttpStatusCode.OK, im.DeleteInvoice(invoiceId, base.ActorCompanyId, deleteProject));
        }

        [HttpPost]
        [Route("RecalculateTimeRow/{customerInvoiceRowId:int}")]
        public IHttpActionResult RecalculateTimeRows(int customerInvoiceRowId)
        {
            return Content(HttpStatusCode.OK, pm.RecalculateTimeRow(customerInvoiceRowId, base.ActorCompanyId));
        }
        #endregion

    }
}