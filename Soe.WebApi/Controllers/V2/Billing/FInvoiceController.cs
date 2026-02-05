using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using static Soe.WebApi.Controllers.CoreController;
using System.Threading.Tasks;
using System;
using System.IO;
using Soe.WebApi.Extensions;
using System.Net.Http;
using System.Collections.Generic;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/FInvoice")]
    public class FInvoiceController : SoeApiController
    {
        #region Variables

        private readonly GeneralManager gm;
        private readonly EdiManager em;
        

        #endregion

        #region Constructor

        public FInvoiceController(GeneralManager gm, EdiManager em)
        {
            this.gm = gm;
            this.em = em;
        }

        #endregion

        #region EDI_TEMP

        [HttpGet]
        [Route("Edi/EdiEntryViews/{classification:int}/{originType:int}")]

        public IHttpActionResult GetEdiEntrysWithStateCheck(int classification, int originType)
        {
            return Content(HttpStatusCode.OK, em.GetEdiEntrysWithStateCheck(classification, originType));
        }

        [HttpGet]
        [Route("Edi/EdiEntryViews/Count/{classification:int}/{originType:int}")]
        public IHttpActionResult GetEdiEntrysCountWithStateCheck(int classification, int originType)
        {
            return Content(HttpStatusCode.OK, em.GetEdiEntrysWithStateCheck(classification, originType).Count);
        }

        [HttpPost]
        [Route("Edi/EdiEntryViews/Filtered/")]
        public IHttpActionResult GetFilteredEdiEntrys(GetFilteredEDIEntrysModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.GetFilteredEdiEntrys(base.ActorCompanyId, (SoeEntityState)model.Classification, model.OriginType, model.BillingTypes, model.BuyerId, model.DueDate, model.InvoiceDate, model.OrderNr, model.OrderStatuses, model.SellerOrderNr, model.EdiStatuses, model.Sum, model.SupplierNrName, model.AllItemsSelection));
        }

        [HttpGet]
        [Route("Edi/FinvoiceEntryViews/{classification:int}/{allItemsSelection:int}/{onlyUnHandled:bool}")]
        public IHttpActionResult GetFinvoiceEntrys(int classification, int allItemsSelection, bool onlyUnHandled)
        {
            return Content(HttpStatusCode.OK, em.GetFinvoiceEntrys(base.ActorCompanyId, (SoeEntityState)classification, allItemsSelection, onlyUnHandled));
        }

        #endregion

        #region FInvoice
        [HttpPost]
        [Route("FinvoiceImport/Attachments/")]
        public  IHttpActionResult ImportFinvoiceAttachments(FInvoiceModel model)
        {
            byte[] bytes = Convert.FromBase64String(model.FileString);
            var edi = new EdiManager(ParameterObject);
            return Content(HttpStatusCode.OK, edi.AddFinvoiceAttachment(model.FileName, base.ActorCompanyId, new MemoryStream(bytes)));

        }

        private SoeDataStorageRecordType GetDataStorageType(string type)
        {
            switch (type.ToString().ToUpper())
            {
                case ".JPG":

                case ".PNG":
                    return SoeDataStorageRecordType.InvoiceBitmap;
                case ".PDF":
                    return SoeDataStorageRecordType.InvoicePdf;
                default:
                    return SoeDataStorageRecordType.Unknown;
            }
        }

        [HttpPost]
        [Route("Files/Invoice")]
        public IHttpActionResult UploadInvoiceFile(FInvoiceModel model)
        {
            byte[] bytes = Convert.FromBase64String(model.FileString);

            var record = new DataStorageRecordExtendedDTO
            {
                Data = bytes,
                Type = GetDataStorageType(model.Extention.ToUpper()),
                Entity = model.Entity,
                Description = model.FileName,
                RecordNumber = model.FileName,
                RecordId = 0
                };
            var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
            result.StringValue = model.FileName;
            return Ok(result);
        }

        #endregion
    }
}