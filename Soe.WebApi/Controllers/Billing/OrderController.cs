using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Order")]
    public class OrderController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;
        private readonly EdiManager em;
        private readonly InvoiceManager im;
        private readonly TimeScheduleManager tsm;
        private readonly AccountManager acm;
        private readonly CustomerManager cm;
        private readonly ProjectManager pm;
        private readonly SupplierInvoiceManager sim;
        private readonly CommodityCodeManager ccm;
        private readonly ImportExportManager iem;

        #endregion

        #region Constructor

        public OrderController(AttestManager am, EdiManager em, InvoiceManager im, TimeScheduleManager tsm, AccountManager acm, CustomerManager cm, ProjectManager pm, SupplierInvoiceManager sim, CommodityCodeManager ccm, ImportExportManager iem)
        {
            this.am = am;
            this.em = em;
            this.im = im;
            this.tsm = tsm;
            this.acm = acm;
            this.cm = cm;
            this.pm = pm;
            this.sim = sim;
            this.ccm = ccm;
            this.iem = iem;
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
            return Content(HttpStatusCode.OK, em.GetEdiEntry(ediEntryId,base.ActorCompanyId, true).ToDTO(false,false));
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
                return Content(HttpStatusCode.OK, im.SaveOrder(model.ModifiedFields, model.NewRows, model.ModifiedRows, model.ChecklistHeads, model.ChecklistRows, model.OriginUsers, model.Files , model.DiscardConcurrencyCheck, model.RegenerateAccounting, model.SendXEMail, model.AutoSave));
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

            return Content(HttpStatusCode.OK, im.SendReminderForReadyState(base.UserId,orderId,ids,base.ActorCompanyId, orderNr));
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

        #region IntrastatTransactions

        [HttpGet]
        [Route("Intrastat/Transactions/{originId:int}")]
        public IHttpActionResult GetIntrastatTransactions(int originId)
        {
            return Content(HttpStatusCode.OK, ccm.GetIntrastatTransactions(originId));
        }

        [HttpGet]
        [Route("Intrastat/Transactions/ForExport/{intrastatReportingType:int}/{fromDate}/{toDate}")]
        public IHttpActionResult GetIntrastatTransactionsForExport(int intrastatReportingType, string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, ccm.GetIntrastatTransactionsForExport((IntrastatReportingType)intrastatReportingType, BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Intrastat/Transactions/")]
        public IHttpActionResult SaveIntrastatTransactions(SaveIntrastatTransactionModel model)
        {
            return Content(HttpStatusCode.OK, ccm.SaveIntrastatTransactions(model.Transactions, model.OriginId, (SoeOriginType)model.OriginType));
        }

        [HttpPost]
        [Route("Intrastat/Transactions/Export/")]
        public IHttpActionResult CreateIntrastatExport(EvaluatedSelection selection)
        {
            return Content(HttpStatusCode.OK, iem.CreateIntrastatStatisticsExport(selection));
        }

        #endregion

        #region SupplierInvoices

        [HttpGet]
        [Route("SupplierInvoices/LinkedToOrder/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoicesLinkedToOrder(int invoiceId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesLinkedToOrder(invoiceId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("SupplierInvoices/LinkedToProject/{invoiceId:int}/{projectId:int}")]
        public IHttpActionResult GetSupplierInvoicesLinkedToProject(int invoiceId, int projectId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesLinkedToProject(invoiceId, projectId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("SupplierInvoices/TransferedToOrder/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoicesTransferedOrder(int invoiceId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesTransferedToOrder(invoiceId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("SupplierInvoices/Items/{invoiceId:int}/{projectId:int}")]
        public IHttpActionResult GetSupplierInvoiceItemsForOrder(int invoiceId, int projectId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoiceItemsForOrder(invoiceId, projectId));
        }

        [HttpPost]
        [Route("SupplierInvoices/UpdateImageInclude/{id:int}/{type:int}/{include:bool}")]
        public IHttpActionResult UpdateOrderSupplierInvoiceImage(int id, int type, bool include)
        {
            return Content(HttpStatusCode.OK, sim.UpdateSupplierInvoiceImageOnOrder(id, (SupplierInvoiceOrderLinkType)type, include));
        }

        #endregion

        #region Shifts

        [HttpGet]
        [Route("OrderShifts/{invoiceId:int}")]
        public IHttpActionResult GetOrderShifts(int invoiceId)
        {
            return Content(HttpStatusCode.OK, tsm.GetOrderShifts(base.ActorCompanyId, invoiceId));
        }

        #endregion
    }
}