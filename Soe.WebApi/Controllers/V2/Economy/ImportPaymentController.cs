using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Util;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Windows.Forms;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/ImportPayment")]
    public class ImportPaymentController : SoeApiController
    {
        #region Variables

        private readonly ImportExportManager iem;
        private readonly PaymentManager pm;
        private readonly InvoiceManager im;
        #endregion

        #region Constructor

        public ImportPaymentController(ImportExportManager iem, PaymentManager pm, InvoiceManager im)
        {
            this.iem = iem;
            this.pm = pm;
            this.im = im;
        }

        #endregion

        #region ImportFiles

        [HttpGet]
        [Route("PaymentImports")]
        public IHttpActionResult GetPaymentImports(int allItemsSelection,int? paymentImportId = null)
        {
            int[] importTypes = new int[] 
            { 
                (int)ImportPaymentType.CustomerPayment, 
                (int)ImportPaymentType.SupplierPayment
            };
            return Content(HttpStatusCode.OK, iem.GetPaymentImports(
                base.ActorCompanyId, 
                importTypes, 
                (TermGroup_ChangeStatusGridAllItemsSelection)allItemsSelection, 
                paymentImportId));
        }

        [HttpPost]
        [Route("PaymentImportHeader/")]
        public IHttpActionResult SavePaymentImportHeader(PaymentImportDTO model)
        {
            return Content(HttpStatusCode.OK, pm.SavePaymentImportHeader(base.ActorCompanyId, model));
        }

        [HttpPost]
        [Route("PaymentImport/")]
        public IHttpActionResult StartPaymentImport(PaymentImportRowsDto model)
        {
            byte[] Contents = System.Convert.FromBase64String(model.Base64String);            
            model.Contents = new List<byte[]> { Contents };
            return Content(HttpStatusCode.OK, pm.StartPaymentImport(model.PaymentIOType, model.PaymentMethodId, model.Contents, model.FileName, ActorCompanyId, base.UserId, model.BatchId, model.PaymentImportId, model.ImportType));
        }

        [HttpDelete]
        [Route("ImportedIoInvoices/{batchId:int}/{importType:int}")]
        public IHttpActionResult DeletePaymentImportHeader(int batchId, int importType)
        {
            return Content(HttpStatusCode.OK, iem.DeleteImportedIOInvoices(base.ActorCompanyId, batchId, (ImportPaymentType)importType));
        }

        [HttpGet]
        [Route("ImportedIoInvoices/{batchId:int}/{importType:int}")]
        public IHttpActionResult GetImportedIoInvoices(int batchId, ImportPaymentType importType)
        {
            return Ok(iem.GetImportedIOInvoices(ActorCompanyId, batchId, importType, true));
        }

        [HttpGet]
        [Route("PaymentImport/{importId:int}")]
        public IHttpActionResult GetPaymentImport(int importId)
        {
            return Content(HttpStatusCode.OK, iem.GetPaymentImport(base.ActorCompanyId, importId).ToDTO());
        }
        [HttpPost]
        [Route("SavePaymentImportIOs/")]
        public IHttpActionResult SavePaymentImportIOs(List<PaymentImportIODTO> model)
        {
            return Content(HttpStatusCode.OK, pm.SavePaymentImportIOs(model));
        }

        [HttpPost]
        [Route("PaymentImportIO/")]
        public IHttpActionResult UpdatePaymentImportIO(PaymentImportIODTO model)
        {
            return Content(HttpStatusCode.OK, im.UpdatePaymentImport(model));
        }

        [HttpPost]
        [Route("PaymentImportIODTOsUpdate/")]
        public IHttpActionResult UpdatePaymentImportIODTOS(SavePaymentImportIODTOModel savePaymentItems)
        {
            return Content(HttpStatusCode.OK, pm.SaveImportPaymentFromSupplierInvoice(savePaymentItems.items, savePaymentItems.bulkPayDate, SoeOriginType.SupplierPayment, false, savePaymentItems.accountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("CustomerPaymentImportIODTOsUpdate/")]
        public IHttpActionResult UpdateCustomerPaymentImportIODTOS(SaveCustomerPaymentImportIODTOModel savePaymentItems)
        {
            return Content(HttpStatusCode.OK, pm.SaveImportPaymentFromCustomerInvoice(savePaymentItems.items, savePaymentItems.bulkPayDate, savePaymentItems.paymentMethodId, savePaymentItems.accountYearId, base.ActorCompanyId, false, true));
        }

        [HttpPost]
        [Route("PaymentImportIODTOsUpdateStatus/")]
        public IHttpActionResult UpdatePaymentImportIODTOSStatus(SavePaymentImportIODTOModel savePaymentItems)
        {
            return Content(HttpStatusCode.OK, pm.UpdatePaymentImportIOStatus(savePaymentItems.items));
        }

        [HttpPost]
        [Route("PaymentFileImport")]
        public async Task<IHttpActionResult> PaymentImport()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                {
                    ActionResult result = new ActionResult();
                    try
                    {
                        var contents = FileUtil.ConvertToStream(file.File, false);
                        result.Value = contents;
                        result.Value2 = file.Filename;
                    }
                    catch (Exception exception)
                    {
                        result.Success = false;
                        result.Exception = exception;
                    }

                    return Content(HttpStatusCode.OK, result);
                }
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("FinvoiceImport/")]
        public async Task<IHttpActionResult> ImportFinvoiceFiles(List<int> dataStorageIds)
        {
            var result = await iem.ImportFinvoiceFiles(dataStorageIds, base.ActorCompanyId).ConfigureAwait(false);
			return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("FinvoiceImport/Attachments/")]
        public async Task<IHttpActionResult> ImportFinvoiceAttachments()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var edi = new EdiManager(ParameterObject);
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                {
                    return Content(HttpStatusCode.OK, edi.AddFinvoiceAttachment(file.Filename, base.ActorCompanyId, new MemoryStream(file.File)));
                }
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpDelete]
        [Route("PaymentImportIO/{paymentImportIOId:int}")]
        public IHttpActionResult DeletePaymentImportIO(int paymentImportIOId)
        {
            return Content(HttpStatusCode.OK, iem.DeletePaymentImportRow(base.ActorCompanyId, paymentImportIOId));
        }

        #endregion

        #region Paymentmethods

        [HttpPost]
        [Route("PaymentMethods")]
        public IHttpActionResult GetPaymentMethodsDict(PaymentMethodsGetModel model)
        {
            return Content(
                HttpStatusCode.OK, 
                pm.GetPaymentMethodsDict(
                    model.OriginTypeIds, 
                    base.ActorCompanyId, 
                    model.AddEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("PaymentMethods/ForImport/{originTypeId:int}")]
        public IHttpActionResult GetPaymentMethodsForImport(int originTypeId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethodsForImport(originTypeId, base.ActorCompanyId).ToDTOs(false));
        }

        [HttpGet]
        [Route("PaymentTypes/")]
        public IHttpActionResult GetSysPaymentTypes()
        {
            return Content(HttpStatusCode.OK, iem.GetSysPaymentTypeDict().ToSmallGenericTypes());
        }

        #endregion
    }
}