using System.Net;
using System.Linq;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using Soe.WebApi.Models;
using Soe.WebApi.Controllers;
using System.Net.Http;
using Soe.WebApi.Extensions;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/CustomerPaymentMethod")]
    public class CustomerPaymentMethodController : SoeApiController
    {
        #region Variables

        private readonly PaymentManager pm;
        private readonly InvoiceManager im;
        private readonly OriginManager om;
        private readonly SoeOriginType originalPaymentType;
        private readonly ImportExportManager iem;
        private readonly SupplierInvoiceManager sim;

        #endregion

        #region Constructor

        public CustomerPaymentMethodController(PaymentManager pm, InvoiceManager im, OriginManager om, SupplierInvoiceManager sim, ImportExportManager iem)
        {
            this.pm = pm;
            originalPaymentType = SoeOriginType.CustomerPayment;
            this.im = im;
            this.om = om;
            this.sim = sim;
            this.iem = iem;
        }

        #endregion


        #region Customer

        [HttpGet]
        [Route("CustomerLedger/{invoiceId:int}")]
        public IHttpActionResult GetCustomerLedger(int invoiceId)
        {
            var invoiceRows = im.GetCustomerInvoiceRowsForCustomerInvoiceEdit(invoiceId, true, true);
            var accountRows = im.GetCustomerInvoiceAccountRowsForCustomerInvoiceEdit(invoiceId);
            var invoice = im.GetCustomerInvoice(invoiceId, loadOrigin: true, loadActor: true, loadOriginInvoiceMapping: true);

            return Content(HttpStatusCode.OK, invoice.ToCustomerInvoiceDTO(invoiceRows, accountRows, true, true));
        }

        [HttpPost]
        [Route("CustomerLedger/")]
        public IHttpActionResult SaveCustomerLedger(SaveCustomerLedgerModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveCustomerLedger(model.invoice, model.accountingRows, base.ActorCompanyId, null, false, false, model.files));
        }

        [HttpDelete]
        [Route("CustomerLedger/{invoiceId:int}")]
        public IHttpActionResult DeleteCustomerLedger(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.DeleteInvoice(invoiceId, base.ActorCompanyId, false));
        }

        [HttpGet]
        [Route("Invoice/Payment/{invoiceId:int}")]
        public IHttpActionResult GetInvoiceForPayment(int invoiceId)
        {
            var invoice = im.GetInvoiceForPayment(invoiceId);
            var claimAccountId = im.GetCustomerInvoiceClaimAccountId((TermGroup_BillingType)invoice.BillingType, invoiceId);
            var invoiceRows = im.GetCustomerInvoiceRows(invoiceId, false);
            return Content(HttpStatusCode.OK, invoice.ToCustomerInvoiceDTO(invoiceRows, null, false, false, claimAccountId));
        }

        [HttpGet]
        [Route("Invoice/Unpaid/{customerId:int}")]
        public IHttpActionResult GetUnpaidInvoices(int customerId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoices(SoeOriginStatusClassification.CustomerPaymentsUnpayed, (int)SoeOriginType.CustomerInvoice, customerId, false, false, false, TermGroup_ChangeStatusGridAllItemsSelection.All).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Invoice/Unpaid/Dialog/{customerId:int}")]
        public IHttpActionResult GetUnpaidInvoicesForDialog(int customerId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoices(SoeOriginStatusClassification.CustomerPaymentsUnpayed, (int)SoeOriginType.CustomerInvoice, customerId, false, false, false, TermGroup_ChangeStatusGridAllItemsSelection.All).ToSmallDialogDTOs());
        }

        [HttpPost]
        [Route("CustomerCentralCountersAndBalance/")]
        public List<ChangeStatusGridViewBalanceDTO> GetCustomerCentralCountersAndBalance(GetCustomerCentralCountersAndBalanceModel model)
        {
            return im.GetChangeStatusGridViewsCountersAndBalanceForCustomerCentral(model.CounterTypes, model.CustomerId, base.ActorCompanyId);
        }

        [HttpPost]
        [Route("TransferCustomerInvoicesToDefinitive/")]
        public IHttpActionResult TransferCustomerInvoicesToDefinitive(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, om.UpdateInvoiceOriginStatusFromAngular(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TransferCustomerInvoicesToVoucher/")]
        public IHttpActionResult TransferCustomerInvoicesToVoucher(TransferModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sim.TransferCustomerInvoicesToVoucherFromAngular(model.IdsToTransfer, model.AccountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        //[Route("ExportInvoicesToSOP/{model}")]
        [Route("ExportInvoicesToSOP/")]
        public IHttpActionResult ExportInvoicesToSOP(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, iem.CreateSOPCustomerInvoiceExportFileFromAngular(model.Numbers, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("ExportInvoicesToUniMicro/")]
        public IHttpActionResult ExportInvoicesToUniMicro(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, iem.CreateUniMicroCustomerInvoiceExportFileFromAngular(model.Numbers, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("ExportInvoicesToDIRegnskap/")]
        public IHttpActionResult ExportInvoicesToDIRegnskap(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, iem.CreateDIRegnCustomerInvoiceExportFileFromAngular(model.Numbers, base.ActorCompanyId, base.UserId, false));
        }

        #endregion


        #region PaymentInformation

        [HttpGet]
        [Route("PaymentInformation/{addEmptyRow:bool}")]
        public IHttpActionResult GetPaymentInformationViewsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationViewsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("PaymentInformation/{supplierId:int}")]
        public IHttpActionResult GetPaymentInformationViews(int supplierId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationViews(supplierId, true));
        }

        #endregion

        #region PaymentMethod

        [HttpGet]
        [Route("PaymentMethodCustomerGrid/{addEmptyRow:bool}/{includePaymentInformationRows:bool}/{includeAccount:bool}/{onlyCashSales:bool}/{paymentMethodId:int?}")]
        public IHttpActionResult GetPaymentMethodsCustomerGrid(bool addEmptyRow, bool includePaymentInformationRows, bool includeAccount, bool onlyCashSales, int? paymentMethodId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethods(originalPaymentType, base.ActorCompanyId, addEmptyRow, onlyCashSales, includeAccount, paymentMethodId).ToCustomerGridDTOs(includePaymentInformationRows, includeAccount));
        }

        [HttpGet]
        [Route("PaymentMethod/{addEmptyRow:bool}/{includePaymentInformationRows:bool}/{includeAccount:bool}/{onlyCashSales:bool}")]
        public IHttpActionResult GetPaymentMethods(bool addEmptyRow, bool includePaymentInformationRows, bool includeAccount, bool onlyCashSales)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethods(originalPaymentType, base.ActorCompanyId, addEmptyRow, onlyCashSales).ToDTOs(includePaymentInformationRows, includeAccount));
        }

        [HttpGet]
        [Route("PaymentMethod/{paymentMethodId:int}/{loadAccount:bool}/{loadPaymentInformationRow:bool}")]
        public IHttpActionResult GetPaymentMethod(int paymentMethodId, bool loadAccount, bool loadPaymentInformationRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethod(paymentMethodId, base.ActorCompanyId, loadAccount).ToDTO(loadPaymentInformationRow, loadAccount));
        }

        [HttpPost]
        [Route("PaymentMethod")]
        public IHttpActionResult SavePaymentMethod(PaymentMethodDTO paymentMethod)
        {
            paymentMethod.PaymentType = originalPaymentType;
            return Content(HttpStatusCode.OK, pm.SavePaymentMethod(paymentMethod, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PaymentMethod/{paymentMethodId:int}")]
        public IHttpActionResult DeletePaymentMethod(int paymentMethodId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePaymentMethod(paymentMethodId, base.ActorCompanyId));
        }

        #endregion

        #region SysPaymentMethod

        [HttpGet]
        [Route("SysPaymentMethod/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysPaymentMethodsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetSysPaymentMethodsDict(originalPaymentType, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion
    }
}