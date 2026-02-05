using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using Soe.WebApi.Models;
using System.Dynamic;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/CustomerInvoices")]
    public class CustomerInvoicesController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;


        #endregion

        #region Constructor

        public CustomerInvoicesController(InvoiceManager im)
        {
            this.im = im;
        }

        #endregion

        #region CustomerInvoices

        [HttpGet]
        [Route("{classification:int}/{originType:int}/{loadOpen:bool}/{loadClosed:bool}/{onlyMine:bool}/{loadActive:bool}/{allItemsSelection:int}/{billing:bool}")]
        public IHttpActionResult GetInvoices(int classification, int originType, bool loadOpen, bool loadClosed, bool onlyMine, bool loadActive, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, bool billing)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)classification, originType, base.ActorCompanyId, base.UserId, loadOpen, loadClosed, onlyMine, loadActive, allItemsSelection, billing));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult GetInvoicesForProjectCentral(CustomerInvoicesGridModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)model.Classification, model.OriginType, base.ActorCompanyId, base.UserId, model.LoadOpen, model.LoadClosed, model.OnlyMine, model.LoadActive, (TermGroup_ChangeStatusGridAllItemsSelection)model.AllItemsSelection, model.Billing, invoiceIds: model.ModifiedIds));
        }

        [HttpPost]
        [Route("ProjectCentral/")]
        public IHttpActionResult GetInvoicesForProjectCentral(InvoicesForProjectCentralModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)model.Classification, model.OriginType, base.ActorCompanyId, base.UserId, true, true, false, true, TermGroup_ChangeStatusGridAllItemsSelection.All, false, model.ProjectId, model.LoadChildProjects, model.InvoiceIds));
        }

        [HttpPost]
        [Route("CustomerCentral/")]
        public IHttpActionResult GetInvoicesForCustomerCentral(InvoicesForCustomerCentralModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)model.Classification, model.OriginType, base.ActorCompanyId, base.UserId, true, true, model.OnlyMine, true, TermGroup_ChangeStatusGridAllItemsSelection.All, false, actorCustomerId: model.ActorCustomerId));
        }

        [HttpPost]
        [Route("Filtered/")]
        public IHttpActionResult GetFilteredCustomerInvoices(ExpandoObject filterModels)
        {
            return Content(HttpStatusCode.OK, im.GetFilteredCustomerInvoicesForGrid(filterModels));
        }

        [HttpPost]
        [Route("Transfer")]
        public IHttpActionResult TransferCustomerInvoices(TransferCustomerInvoiceAndPaymentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.TransferCustomerInvoices(model.Items, model.originStatusChange, model.PaymentMethodId, model.MergeInvoices, model.ClaimLevel, model.bulkPayDate, model.bulkInvoiceDate, model.bulkDueDate, model.bulkVoucherDate, model.KeepFixedPriceOrderOpen, model.CheckPartialInvoicing, model.CreateCopiesOfTransferedContractRows, model.SetStatusToOrigin, model.EmailTemplateId, model.ReportId, model.LanguageId, model.MergePdfs));
        }

        [HttpGet]
        [Route("ReminderInformation/{invoiceId:int}")]
        public IHttpActionResult GetReminderPrintedInformation(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicePrintedRemindersMessage(invoiceId));
        }

        [HttpGet]
        [Route("NumbersDict/{customerId:int}/{originType:int}/{classification:int}/{registrationType:int}/{orderByNumber:bool}")]
        public IHttpActionResult GetReminderPrintedInformation(int customerId, int originType, int classification, int registrationType, bool orderByNumber)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceContractsDict(base.ActorCompanyId, customerId, (SoeOriginType)originType, (SoeOriginStatusClassification)classification, orderByNumber));
        }

        [HttpGet]
        [Route("Rows/{invoiceId:int}")]
        public IHttpActionResult GetCustomerInvoiceRowsForInvoice(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceRows(invoiceId, true).ToProductRowDTOs());
        }

        [HttpGet]
        [Route("RowsSmall/{invoiceId:int}")]
        public IHttpActionResult GetCustomerInvoiceRowsSmallForInvoice(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceDetailsRowsForInvoice(invoiceId));
        }

        [HttpGet]
        [Route("ServiceOrdersForAgreement/{invoiceId:int}")]
        public IHttpActionResult GetServiceOrdersForAgreementDetails(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetServiceOrdersForAgreementDetails(base.ActorCompanyId, invoiceId));
        }

        [HttpPost]
        [Route("CopyRows/")]
        public IHttpActionResult CopyCustomerInvoiceRows(CopyCustomerInvoiceRowsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.CopyCustomerInvoiceRows(model.RowsToCopy, model.OriginType, base.ActorCompanyId, model.TargetId, model.OriginId.HasValue && model.OriginId.Value > 0 ? model.OriginId.Value : (int?)null, model.UpdateOrigin.HasValue && model.UpdateOrigin.Value, model.Recalculate.HasValue && model.Recalculate.Value));
        }

        [HttpGet]
        [Route("PendingReminders/{customerId:int}/{loadCustomer:bool}/{loadProduct:bool}")]
        public IHttpActionResult GetPendingCustomerInvoiceReminders(int customerId, bool loadCustomer, bool loadProduct)
        {
            return Content(HttpStatusCode.OK, im.GetPendingCustomerInvoiceReminders(customerId, loadCustomer, loadProduct).ToDTOs());
        }

        [HttpGet]
        [Route("PendingInterests/{customerId:int}/{loadCustomer:bool}/{loadProduct:bool}")]
        public IHttpActionResult GetPendingCustomerInvoiceInterests(int customerId, bool loadCustomer, bool loadProduct)
        {
            return Content(HttpStatusCode.OK, im.GetPendingCustomerInvoiceInterests(customerId, loadCustomer, loadProduct).ToDTOs());
        }

        [HttpPost]
        [Route("SearchSmall/")]
        public IHttpActionResult GetInvoicesBySearch(SearchCustomerInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.GetInvoicesBySearch(base.ActorCompanyId, model.OriginType, new CustomerInvoiceSearchParamsDTO
                {
                    CustomerId = model.CustomerId,
                    ProjectId = model.ProjectId,
                    Number = model.Number,
                    ExternalNr = model.ExternalNr,
                    CustomerName = model.CustomerName,
                    CustomerNr = model.CustomerNr,
                    InternalText = model.InternalText,
                    ProjectNr = model.ProjectNr,
                    ProjectName = model.ProjectName,
                    IgnoreChildren = model.IgnoreChildren,
                    IgnoreInvoiceId = model.IgnoreInvoiceId,
                    IncludePreliminary = model.IncludePreliminary == true,
                    IncludeVoucher = model.IncludeVoucher == true,
                    UserId = model.UserId,
                    FullyPaid = model.FullyPaid,
                }, 0));
        }

        [HttpDelete]
        [Route("PendingReminders/{customerId:int}")]
        public IHttpActionResult DeletePendingCustomerInvoiceReminders(int customerId)
        {
            return Content(HttpStatusCode.OK, im.DeletePendingCustomerInvoiceReminders(customerId));
        }

        [HttpDelete]
        [Route("PendingInterests/{customerId:int}")]
        public IHttpActionResult DeletePendingCustomerInvoiceInterests(int customerId)
        {
            return Content(HttpStatusCode.OK, im.DeletePendingCustomerInvoiceInterests(customerId));
        }

        #endregion
    }
}