using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Export/Invoices")]
    public class ExportFilesController : SoeApiController
    {
        private readonly ImportExportManager iem;
        private readonly InvoiceManager im;
        private readonly SAFTManager saftMgr;

        #region Constructor

        public ExportFilesController(ImportExportManager iem, InvoiceManager im, SAFTManager saftMgr)
        {
            this.iem = iem;
            this.im = im;
            this.saftMgr = saftMgr;
        }

        #endregion

        #region ExportFiles

        [HttpGet]
        [Route("GetPaymentServiceRecordsGrid/{invoiceExportId:int?}")]
        public IHttpActionResult GetPaymentServiceRecords(int? invoiceExportId = null)
        {
            return Content(HttpStatusCode.OK, iem.GetInvoiceExports(base.ActorCompanyId, invoiceExportId).ToDTOs());
        }

        [HttpGet]
        [Route("GetExportedIOInvoices/{invoiceExportId:int}")]
        public IHttpActionResult GetExportedIOInvoices(int invoiceExportId)
        {
            return Ok(iem.GetExportedIOInvoices(base.ActorCompanyId, invoiceExportId).ToDTOs());
        }

        [HttpGet]
        [Route("PaymentService/GetInvoicesForPaymentService/{paymentService:int}")]
        public IHttpActionResult GetInvoicesForPaymentService(int paymentService)
        {
            return Ok(im.GetInvoicesForPaymentService(ActorCompanyId, paymentService));
        }

        [HttpGet]
        [Route("Saft/Transactions/{fromDate}/{toDate}")]
        public IHttpActionResult GetSAFTTransactionsForExport(string fromDate, string toDate)
        {
          return Content(HttpStatusCode.OK, saftMgr.GetTransactions(BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value, base.ActorCompanyId));
        }

        /// <summary>
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns>Action result of XML file content as UTF-8 encoded string</returns>
        [HttpGet]
        [Route("Saft/Export/{fromDate}/{toDate}")]
        public IHttpActionResult CreateSAFTExport(string fromDate, string toDate)
        {
          return Content(HttpStatusCode.OK, saftMgr.Export(BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PaymentService/Invoices/{paymentService:int}")]
        public IHttpActionResult SaveCustomerInvoicePaymentService(List<InvoiceExportIODTO> items, int paymentService)
        {
            return Ok(im.SaveCustomerInvoicePaymentService(items, ActorCompanyId, UserId, paymentService));
        }


        #endregion
    }
}
