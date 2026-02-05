using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/HouseholdTaxDeduction")]
    public class HouseholdTaxDeductionController : SoeApiController
    {
        #region Household tax deduction

        [HttpGet]
        [Route("HouseholdTaxDeduction/Customer/{customerId:int}/{addEmptyRow:bool}/{showAllApplicants:bool}")]
        public IHttpActionResult GetHouseholdTaxDeductionRowsByCustomer(int customerId, bool addEmptyRow, bool showAllApplicants)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionApplicants(base.ActorCompanyId, customerId, addEmptyRow, showAllApplicants));
        }

        [HttpGet]
        [Route("HouseholdTaxDeduction/{classificationGroup:int}/{taxDeductionType:int}")]
        public IHttpActionResult GetHouseholdTaxDeductionRows(int classificationGroup, int taxDeductionType)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRows_v2(base.ActorCompanyId, (SoeHouseholdClassificationGroup)classificationGroup, (TermGroup_HouseHoldTaxDeductionType)taxDeductionType));
        }

        [HttpGet]
        [Route("HouseholdTaxDeduction/Apply/")]
        public IHttpActionResult GetHouseholdTaxDeductionRowsApply()
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRows(base.ActorCompanyId, SoeHouseholdClassificationGroup.Apply));
        }

        [HttpGet]
        [Route("HouseholdTaxDeduction/Applied/")]
        public IHttpActionResult GetHouseholdTaxDeductionRowsApplied()
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRows(base.ActorCompanyId, SoeHouseholdClassificationGroup.Applied));
        }

        [HttpGet]
        [Route("HouseholdTaxDeduction/Denied/")]
        public IHttpActionResult GetHouseholdTaxDeductionRowsDenied()
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRows(base.ActorCompanyId, SoeHouseholdClassificationGroup.Denied));
        }

        [HttpGet]
        [Route("HouseholdTaxDeduction/Received/")]
        public IHttpActionResult GetHouseholdTaxDeductionRowsReceived()
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRows(base.ActorCompanyId, SoeHouseholdClassificationGroup.Received));
        }

        [HttpGet]
        [Route("HouseholdTaxRowInfo/{invoiceId:int}/{customerInvoiceRowId:int}")]
        public IHttpActionResult GetHouseholdTaxDeductionRowInfo(int invoiceId, int customerInvoiceRowId)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRowInfo(invoiceId, customerInvoiceRowId));
        }

        [HttpGet]
        [Route("HouseholdTaxDeductionRowForEdit/{customerInvoiceRowId:int}")]
        public IHttpActionResult GetHouseholdTaxDeductionRowForEdit(int customerInvoiceRowId)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRowForEdit(customerInvoiceRowId));
        }

        [HttpGet]
        [Route("HouseholdSequenceNumber/{entityName}")]
        public IHttpActionResult GetLastUsedSequenceNumber(string entityName)
        {
            var sqm = new SequenceNumberManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, sqm.GetLastUsedSequenceNumber(base.ActorCompanyId, entityName));
        }

        [HttpPost]
        [Route("HouseholdTaxDeductionRowForEdit/")]
        public IHttpActionResult SaveHouseholdTaxDeductionRowForEdit(HouseholdTaxDeductionApplicantDTO item)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.UpdateHouseholdTaxDeductionRow(item));
        }

        [HttpPost]
        [Route("HouseholdTaxDeduction/SaveReceived")]
        public IHttpActionResult SaveHouseholdTaxReceived(UpdateHouseholdDeductionModel model)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.SaveHouseholdTaxReceived(model.idsToUpdate, model.bulkDate));
        }

        [HttpPost]
        [Route("HouseholdTaxDeduction/SaveReceived/Partially")]
        public IHttpActionResult SaveHouseholdTaxPartiallyApproved(UpdateHouseholdDeductionModel model)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.SaveHouseholdTaxReceived(model.idsToUpdate, model.bulkDate, model.amount));
        }

        [HttpPost]
        [Route("HouseholdTaxDeduction/SaveApplied")]
        public IHttpActionResult SaveHouseholdTaxApplied(UpdateHouseholdDeductionModel model)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.SaveHouseholdTaxApplied(model.idsToUpdate));
        }

        [HttpPost]
        [Route("HouseholdTaxDeduction/SaveWithdrawApplied")]
        public IHttpActionResult SaveHouseholdTaxWithdrawApplied(UpdateHouseholdDeductionModel model)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.WithdrawHouseholdApplied(model.idsToUpdate));
        }

        [HttpPost]
        [Route("HouseholdTaxDeduction/SaveDenied")]
        public IHttpActionResult SaveHouseholdTaxDenied(UpdateHouseholdDeductionModel model)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.SaveHouseholdTaxDenied(model.customerInvoiceId, model.customerInvoiceRowId, model.bulkDate));
        }

        [HttpPost]
        [Route("Print/HouseholdTaxDeduction/")]
        public IHttpActionResult GetHouseholdTaxDeductionPrintUrl(HouseholdTaxDeductionPrintUrlModel model)
        {
            var rm = new ReportManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, rm.GetHouseholdPrintUrl(model.CustomerInvoiceRowIds, model.ReportId, model.SysReportTemplateTypeId, model.NextSequenceNumber, model.UseGreen));
        }

        [HttpDelete]
        [Route("HouseholdTaxDeduction/Delete/{rowId:int}")]
        public IHttpActionResult DeleteHouseholdTaxDeductionRow(int rowId)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.DeleteHouseholdTaxDeductionRow(rowId));
        }

        #endregion
    }
}
