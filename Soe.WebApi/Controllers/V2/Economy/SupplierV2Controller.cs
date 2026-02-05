using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Supplier")]
    public class SupplierV2Controller : SoeApiController
    {
        #region Variables

        private readonly SupplierManager sm;
        private readonly ReportDataManager rm;
        private readonly InvoiceManager im;
        private readonly EdiManager edim;
        #endregion

        #region Constructor
        public SupplierV2Controller(SupplierManager sm, ReportDataManager rm, InvoiceManager im, EdiManager edim)
        {
            this.sm = sm;
            this.rm = rm;
            this.im = im;
            this.edim = edim;
        }

        #endregion

        #region Supplier

        [HttpGet]
        [Route("Supplier/")]
        public IHttpActionResult GetSuppliers([FromUri] bool onlyActive, [FromUri] int? supplierId = null)
        {
            return Content(HttpStatusCode.OK, sm.GetSuppliersByCompanyExtended(base.ActorCompanyId, onlyActive, supplierId));
        }

        [HttpGet]
        [Route("Supplier/Dict/")]
        public IHttpActionResult GetSuppliersDict([FromUri] bool onlyActive, [FromUri] bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, sm.GetSuppliersByCompanyDict(base.ActorCompanyId, onlyActive, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("Supplier/BySearch/")]
        public IHttpActionResult GetSuppliersBySearch(SearchSuppliersDTO dto)
        {
            return Content(HttpStatusCode.OK, sm.GetSuppliersBySearch(base.ActorCompanyId, dto.SupplierNumber, dto.SupplierName).ToGridDTOs());
        }

        [HttpGet]
        [Route("Supplier/{supplierId:int}/{loadActor:bool}/{loadAccount:bool}/{loadContactAddresses:bool}/{loadCategories:bool}")]
        public IHttpActionResult GetSupplier(int supplierId, bool loadActor, bool loadAccount, bool loadContactAddresses, bool loadCategories)
        {
            var supplier = sm.GetSupplier(supplierId, loadActor, false, loadContactAddresses, loadCategories)
                .ToDTO(loadContactAddresses);
            if (loadAccount)
                supplier.AccountingSettings = sm.GetSupplierAccountSettings(this.ActorCompanyId, supplierId);
            return Content(HttpStatusCode.OK, supplier); 
        }

        [HttpGet]
        [Route("Supplier/Export/{supplierId:int}")]
        public IHttpActionResult GetSupplierForExport(int supplierId)
        {
            var supplier = sm.GetSupplier(supplierId, true, false, true, true)
                .ToDTO(true);
            supplier.AccountingSettings = sm.GetSupplierAccountSettings(this.ActorCompanyId, supplierId);
            return Content(HttpStatusCode.OK, supplier);
        }

        [HttpGet]
        [Route("Supplier/NextSupplierNr/")]
        public IHttpActionResult GetNextSupplierNr()
        {
            return Content(HttpStatusCode.OK, sm.GetNextSupplierNr(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Supplier")]
        public IHttpActionResult SaveSupplier(SaveSupplierModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveSupplier(model.Supplier, model.Supplier.ContactPersons, model.Files, base.ActorCompanyId, model.ExtraFields));
        }

        [HttpDelete]
        [Route("Supplier/{supplierId:int}")]
        public IHttpActionResult DeleteSupplier(int supplierId)
        {
            return Content(HttpStatusCode.OK, sm.DeleteSupplier(supplierId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Supplier/UpdateState")]
        public IHttpActionResult UpdateSuppliersState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.UpdateSuppliersState(model.Dict));
        }

        [HttpPost]
        [Route("Supplier/UpdateIsPrivatePerson")]
        public IHttpActionResult UpdateIsPrivatePerson(List<UpdateIsPrivatePerson> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.UpdateSuppliersIsPrivatePerson(items.ToDictionary(k => k.id, v => v.isPrivatePerson)));
        }

        #endregion

        #region GenerateReportForEdi
                
        [HttpPost]
        [Route("GenerateReportForEdi/")]
        public IHttpActionResult GenerateReportForEdi(List<int> ediEntryIds)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.GenerateReportForEdi(ediEntryIds, ActorCompanyId));
        }

        #endregion

        #region Invoice


        [HttpGet]
        [Route("GetOrdersForSupplierInvoiceEdit/")]
        public IHttpActionResult GetOrdersForSupplierInvoiceEdit()
        {
            return Content(HttpStatusCode.OK, im.GetOrdersForSupplierInvoiceEdit());
        }
      

        #region Invoice
        [HttpPost]
        [Route("UpdateEdiEntries/")]
        public IHttpActionResult UpdateEdiEntrys(List<UpdateEdiEntryDTO> ediEntries)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edim.UpdateEdiEntrys(ediEntries, base.ActorCompanyId));
        }


        [HttpPost]
        [Route("GenerateReportForFinvoice/")]
        public IHttpActionResult GenerateReportForFinvoice(List<int> ediEntryIds)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.GenerateReportForFinvoice(ediEntryIds, ActorCompanyId));
        }


        [HttpPost]
        [Route("TransferEdiToInvoices/")]
        public IHttpActionResult TransferEdiToInvoices(ListIntModel itemsToTransfer)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edim.TransferToSupplierInvoicesFromEdiDict(itemsToTransfer.Numbers, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("TransferEdiToOrder/")]
        public IHttpActionResult TransferEdiToOrder(ListIntModel itemsToTransfer)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edim.TransferToOrdersFromEdi(itemsToTransfer.Numbers, base.ActorCompanyId, false));
        }


        [HttpPost]
        [Route("TransferEdiState/")]
        public IHttpActionResult TransferEdiState(TransferEdiStateModel model)
        {
            return Content(HttpStatusCode.OK, edim.ChangeEdiEntriesState(model.IdsToTransfer, model.StateTo, base.ActorCompanyId));
        }
        #endregion
    }
    #endregion
}