using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using Soe.WebApi.Models;
using System.Collections.Generic;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Connect")]
    public class ConnectController : SoeApiController
    {
        #region Variables

        private readonly ImportExportManager iem;

        #endregion

        #region Constructor

        public ConnectController(ImportExportManager iem)
        {
            this.iem = iem;
        }

        #endregion


        #region Connect

        [HttpGet]
        [Route("Imports/{module:int}")]
        public IHttpActionResult GetImports(SoeModule module)
        {
            return Content(HttpStatusCode.OK, iem.GetImports(base.ActorCompanyId, module).ToDTOs());
        }

        [HttpGet]
        [Route("ImportEdit/{importId:int}")]
        public IHttpActionResult GetImport(int importId)
        {
            return Content(HttpStatusCode.OK, iem.GetImport(base.ActorCompanyId, importId, true).ToDTO());
        }

        [HttpGet]
        [Route("SysImportDefinitions/{module:int}")]
        public IHttpActionResult GetSysImportDefinitions(SoeModule module)
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportDefinitions(module).ToDTOs(false));
        }

        [HttpGet]
        [Route("SysImportHeads/")]
        public IHttpActionResult GetSysImportHeads()
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportHeads().ToDTOs(false, false));
        }
      
        [HttpGet]
        [Route("Batches/{importHeadType:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetImportBatches(int importHeadType, TermGroup_GridDateSelectionType allItemsSelection) 
        {
            if (importHeadType > 0)
            {
                return Content(HttpStatusCode.OK, iem.GetImportBatches(base.ActorCompanyId, (TermGroup_IOImportHeadType)importHeadType, allItemsSelection, null));
            }
            else
            {
                return Content(HttpStatusCode.OK, iem.GetImportBatches(base.ActorCompanyId, null, TermGroup_GridDateSelectionType.All, null));
            }
        }

        [HttpGet]
        [Route("ImportGridColumns/{importHeadType:int}")]
        public IHttpActionResult GetImportGridColumns(int importHeadType)
        {
            return Content(HttpStatusCode.OK, iem.GetImportGridColumnsDTOs(importHeadType));
        }

        [HttpGet]
        [Route("ImportIOResult/{importHeadType:int}/{batchId}")]
        public IHttpActionResult GetImportIOResult(int importHeadType, string batchId)
        {

            if (importHeadType == (int)TermGroup_IOImportHeadType.Customer)
                return Content(HttpStatusCode.OK, iem.GetCustomerIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.CustomerInvoice)
                return Content(HttpStatusCode.OK, iem.GetCustomerInvoiceHeadIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs(false));
            else if (importHeadType == (int)TermGroup_IOImportHeadType.CustomerInvoiceRow)
                return Content(HttpStatusCode.OK, iem.GetCustomerInvoiceRowIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.Supplier)
                return Content(HttpStatusCode.OK, iem.GetSupplierIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.SupplierInvoice ||
                     importHeadType == (int)TermGroup_IOImportHeadType.SupplierInvoiceAnsjo)
                return Content(HttpStatusCode.OK, iem.GetSupplierInvoiceHeadIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.Voucher)
                return Content(HttpStatusCode.OK, iem.GetVoucherHeadIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs(false));
            else if (importHeadType == (int)TermGroup_IOImportHeadType.Project)
                return Content(HttpStatusCode.OK, iem.GetProjectIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else
                return Ok();

        }

        [HttpPost]
        [Route("ImportEdit/")]
        public IHttpActionResult SaveImport(ImportDTO import)
        {
            return Content(HttpStatusCode.OK, iem.SaveImport(import, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ImportFile/")]
        public IHttpActionResult ImportFile(ImportModel model)
        {
            return Content(HttpStatusCode.OK, iem.XEConnectImport(base.ActorCompanyId, model.importId, model.dataStorageIds, model.accountYearId, model.voucherSeriesId, model.importDefinitionId));
        }

        [HttpPost]
        [Route("ImportIO/")]
        public IHttpActionResult ImportIO(ImportIOModel model)
        {
            IHttpActionResult httpActionResult = null;
            switch (model.importHeadType)
            {
                case TermGroup_IOImportHeadType.Customer:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportCustomerIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.CustomerInvoice:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportCustomerInvoiceHeadIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.CustomerInvoiceRow:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportCustomerInvoiceRowIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.Supplier:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportSupplierIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.SupplierInvoice:
                case TermGroup_IOImportHeadType.SupplierInvoiceAnsjo:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportSupplierInvoiceHeadIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.Voucher:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportVoucherHeadIO(model.ioIds, base.ActorCompanyId, model.UseAccountDistribution, model.useAccoungDims, model.defaultDim1AccountId,  model.defaultDim2AccountId, model.defaultDim3AccountId, model.defaultDim4AccountId, model.defaultDim5AccountId, model.defaultDim6AccountId));
                    break;
                case TermGroup_IOImportHeadType.Project:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportProjectIO(model.ioIds, base.ActorCompanyId));
                    break;
            }

            return httpActionResult;
        }

        [HttpPost]
        [Route("Connect/CustomerIODTO/")]
        public IHttpActionResult SaveCustomerIODTO(List<CustomerIODTO> customerIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateCustomerIO(customerIODTOs));
        }

        [HttpPost]
        [Route("Connect/CustomerInvoiceIODTO/")]
        public IHttpActionResult SaveCustomerInvoiceIODTO(List<CustomerInvoiceIODTO> customerInvoiceIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateCustomerInvoiceHeadIO(customerInvoiceIODTOs));
        }

        [HttpPost]
        [Route("Connect/CustomerInvoiceRowIODTO/")]
        public IHttpActionResult SaveCustomerInvoiceRowIODTO(List<CustomerInvoiceRowIODTO> customerInvoiceRowIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateCustomerInvoiceRowIO(customerInvoiceRowIODTOs));
        }

        [HttpPost]
        [Route("Connect/SupplierIODTO/")]
        public IHttpActionResult SaveSupplierIODTO(List<SupplierIODTO> supplierIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateSupplierIO(supplierIODTOs));
        }

        [HttpPost]
        [Route("Connect/SupplierInvoiceIODTO/")]
        public IHttpActionResult SaveSupplieInvoiceIODTO(List<SupplierInvoiceHeadIODTO> supplierInvoiceIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateSupplierInvoiceHeadIO(supplierInvoiceIODTOs));
        }

        [HttpPost]
        [Route("Connect/VoucherIODTO/")]
        public IHttpActionResult SaveVoucherIODTO(List<VoucherHeadIODTO> voucherIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateVoucherHeadIO(voucherIODTOs));
        }

        [HttpPost]
        [Route("Connect/ProjectIODTO/")]
        public IHttpActionResult SaveProjectIODTO(List<ProjectIODTO> projectIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateProjectIO(projectIODTOs));
        }

        [HttpPost]
        [Route("Connect/ImportSelectionGrid/")]
        public IHttpActionResult Import(FilesLookupDTO files)
        {
            return Content(HttpStatusCode.OK, iem.GetImportSelectionGrid(
                base.ActorCompanyId,
                files.Files));
        }

        [HttpDelete]
        [Route("Connect/ImportEdit/{importId:int}")]
        public IHttpActionResult DeleteImport(int importId)
        {
            return Content(HttpStatusCode.OK, iem.DeleteImport(importId, base.ActorCompanyId));
        }

        #endregion

    }
}