using System.Collections.Generic;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Soe.WebApi.Binders;
using System;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Invoice")]
    public class InvoiceController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;
        private readonly PaymentManager pam;
        private readonly ProductManager prm;
        private readonly ProductPricelistManager pplm;
        private readonly ProjectManager projm;
        private readonly VoucherManager vm;
        private readonly WholeSellerManager wm;
        private readonly EdiManager em;
        private readonly SupplierAgreementManager sam;
        private readonly SupplierManager sm;
        private readonly SysPriceListManager spm;
        private readonly MarkupManager mm;
        private readonly PriceRuleManager rm;
        private readonly ElectronicInvoiceMananger eim;

        #endregion

        #region Constructor

        public InvoiceController(InvoiceManager im, PaymentManager pam, ProductManager prm, ProjectManager projm, VoucherManager vm, WholeSellerManager wm, EdiManager em, SupplierAgreementManager sam, SupplierManager sm, SysPriceListManager spm, MarkupManager mm, PriceRuleManager rm, ElectronicInvoiceMananger eim, ProductPricelistManager pplm)
        {
            this.im = im;
            this.pam = pam;
            this.prm = prm;
            this.vm = vm;
            this.wm = wm;
            this.projm = projm;
            this.em = em;
            this.sam = sam;
            this.sm = sm;
            this.spm = spm;
            this.mm = mm;
            this.rm = rm;
            this.eim = eim;
            this.pplm= pplm;
        }

        #endregion

        #region DeliveryCondition

        [HttpGet]
        [Route("DeliveryCondition/")]
        public IHttpActionResult GetDeliveryConditions(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, im.GetDeliveryConditionsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, im.GetDeliveryConditions(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("DeliveryCondition/{deliveryConditionId:int}")]
        public IHttpActionResult GetDeliveryCondition(int deliveryConditionId)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryCondition(deliveryConditionId).ToDTO());
        }

        [HttpPost]
        [Route("DeliveryCondition")]
        public IHttpActionResult SaveDeliveryCondition(DeliveryConditionDTO deliveryConditionDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveDeliveryCondition(deliveryConditionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("DeliveryCondition/{deliveryConditionId:int}")]
        public IHttpActionResult DeleteDeliveryCondition(int deliveryConditionId)
        {
            return Content(HttpStatusCode.OK, im.DeleteDeliveryCondition(deliveryConditionId));
        }

        #endregion

        #region DeliveryType

        [HttpGet]
        [Route("DeliveryType/")]
        public IHttpActionResult GetDeliveryTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, im.GetDeliveryTypesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, im.GetDeliveryTypes(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("DeliveryType/{deliveryTypeId:int}")]
        public IHttpActionResult GetDeliveryType(int deliveryTypeId)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryType(deliveryTypeId));
        }

        [HttpPost]
        [Route("DeliveryType")]
        public IHttpActionResult SaveDeliveryType(DeliveryTypeDTO deliveryType)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveDeliveryType(deliveryType, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("DeliveryType/{deliveryTypeId:int}")]
        public IHttpActionResult DeleteDeliveryType(int deliveryTypeId)
        {
            return Content(HttpStatusCode.OK, im.DeleteDeliveryType(deliveryTypeId));
        }

        #endregion

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
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionRows(base.ActorCompanyId, (SoeHouseholdClassificationGroup)classificationGroup, (TermGroup_HouseHoldTaxDeductionType)taxDeductionType));
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

        [HttpPost]
        [Route("HouseholdTaxDeductionRowForEdit/")]
        public IHttpActionResult GetHouseholdTaxDeductionRowForEdit(HouseholdTaxDeductionApplicantDTO item)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.UpdateHouseholdTaxDeductionRow(item));
        }

        [HttpPost]
        [Route("GetHouseholdTaxDeductionFileForEdit/")]
        public IHttpActionResult GetHouseholdTaxDeductionFileForEdit(List<int> ids)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionFileForEdit(ids));
        }

        [HttpPost]
        [Route("CreateHouseholdTaxDeductionFile/")]
        public IHttpActionResult CreateHouseholdTaxDeductionFile(GetHouseholdTaxDeductionFileModel model)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.GetHouseholdTaxDeductionFile(model.Applications, model.SeqNr, model.Type));
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

        [HttpDelete]
        [Route("HouseholdTaxDeduction/Delete/{rowId:int}")]
        public IHttpActionResult DeleteHouseholdTaxDeductionRow(int rowId)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.DeleteHouseholdTaxDeductionRow(rowId));
        }

        [HttpDelete]
        [Route("HouseholdTaxDeduction/WithdrawReceived/{rowId:int}")]
        public IHttpActionResult WithdrawHouseholdTaxReceived(int rowId)
        {
            var hm = new HouseholdTaxDeductionManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, hm.WithdrawHouseholdTaxReceived(rowId));
        }

        #endregion

        #region Invoice        

        [HttpGet]
        [Route("{invoiceId:int}/{includeCategories:bool}/{includeRows:bool}")]
        public IHttpActionResult GetInvoice(int invoiceId, bool includeCategories, bool includeRows)
        {
            return Content(HttpStatusCode.OK, im.GetBillingInvoice(invoiceId, includeCategories, includeRows));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveInvoice(SaveOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInvoice(model.ModifiedFields, model.NewRows, model.ModifiedRows, model.ChecklistHeads, model.ChecklistRows, model.OriginUsers, model.Files, model.DiscardConcurrencyCheck, model.RegenerateAccounting, model.SendXEMail, model.Crediting));
        }

        [HttpPost]
        [Route("CreateEInvoice")]
        public IHttpActionResult CreateEInvoice(CreateEInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, eim.CreateEInvoice( base.ActorCompanyId, base.UserId, model.InvoiceId, model.download, model.OverrideFinvoiceOperatorWarning));
        }

        [HttpGet]
        [Route("EInvoiceEntry/{invoiceId:int}")]
        public IHttpActionResult GetEInvoiceEntry(int invoiceId)
        {
            var idm = new InvoiceDistributionManager(null);
            return Content(HttpStatusCode.OK, idm.GetInExchangeEntry(ActorCompanyId, invoiceId));
        }

        [HttpDelete]
        [Route("{invoiceId:int}/{deleteProject:bool}/{restoreRowStatus:bool}/{isContract:bool}")]
        public IHttpActionResult DeleteInvoice(int invoiceId, bool deleteProject, bool restoreRowStatus, bool isContract)
        {
            return Content(HttpStatusCode.OK, im.DeleteInvoice(invoiceId, base.ActorCompanyId, deleteProject, restoreRowStatus, isContract));
        }

        [HttpGet]
        [Route("InvoiceFromOrderCheckLists/{invoiceId:int}")]
        public IHttpActionResult GetInvoiceFromOrderCheckLists(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetInvoiceFromOrderCheckLists(ActorCompanyId, invoiceId).ToCompactDTOs());
        }

        #endregion

        #region Markup

        [HttpGet]
        [Route("Markup/{isDiscount:bool}")]
        public IHttpActionResult GetMarkup(bool isDiscount)
        {
            return Content(HttpStatusCode.OK, mm.GetMarkup(base.ActorCompanyId, isDiscount));
        }

        [HttpGet]
        [Route("Markup/Discount/{sysWholesellerId:int}/{code}")]
        public IHttpActionResult GetDiscount(int sysWholesellerId, string code)
        {
            return Content(HttpStatusCode.OK, mm.GetDiscountBySysWholeseller(base.ActorCompanyId, sysWholesellerId, code == "null" ? String.Empty : code));
        }

        [HttpPost]
        [Route("Markup/")]
        public IHttpActionResult SaveMarkup(List<MarkupDTO> items)
        {
            return Content(HttpStatusCode.OK, mm.SaveMarkup(items, base.ActorCompanyId));
        }

        #endregion

        #region PriceBasedMarkup

        [HttpGet]
        [Route("Markup/PriceBased/")]
        public IHttpActionResult GetPriceBasedMarkup()
        {
            return Content(HttpStatusCode.OK, mm.GetPriceBasedMarkups(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Markup/PriceBased/")]
        public IHttpActionResult SavePriceBasedMarkup(PriceBasedMarkupDTO priceBasedMarkup)
        {
            return Content(HttpStatusCode.OK, mm.SavePriceBasedMarkup(priceBasedMarkup, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Markup/PriceBased/{priceBaseMarkupId:int}")]
        public IHttpActionResult DeletePriceBasedMarkup(int priceBaseMarkupId)
        {
            return Content(HttpStatusCode.OK, mm.DeletePriceBasedMarkup(priceBaseMarkupId));
        }

        #endregion

        #region Payment

        [HttpGet]
        [Route("Payment/GetPaymentTraceViews/{paymentRowId:int}")]
        public IHttpActionResult GetPaymentTraceViews(int paymentRowId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(ActorCompanyId);

            return Ok(pam.GetPaymentTraceViews(paymentRowId, baseSysCurrencyId));
        }

        [HttpPost]
        [Route("Payment/CashPayment/")]
        public IHttpActionResult SaveCashPayments(CashPaymentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pam.SaveCashPaymentsForCustomerInvoice(model.Payments, model.InvoiceId, model.MatchCodeId, model.RemainingAmount, model.SendEmail, model.Email, base.ActorCompanyId, model.UseRounding));
        }

        #endregion Payment

        #region PaymentCondition

        [HttpGet]
        [Route("PaymentCondition/")]
        public IHttpActionResult GetPaymentConditions(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, pam.GetPaymentConditions(base.ActorCompanyId).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, pam.GetPaymentConditionsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, pam.GetPaymentConditions(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("PaymentCondition/{paymentConditionId:int}")]
        public IHttpActionResult GetPaymentConditions(int paymentConditionId)
        {
            return Content(HttpStatusCode.OK, pam.GetPaymentCondition(paymentConditionId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("PaymentCondition")]
        public IHttpActionResult SavePaymentCondition(PaymentConditionDTO paymentConditionDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pam.SavePaymentCondition(paymentConditionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PaymentCondition/{paymentConditionId:int}")]
        public IHttpActionResult DeletePaymentCondition(int paymentConditionId)
        {
            return Content(HttpStatusCode.OK, pam.DeletePaymentCondition(paymentConditionId, base.ActorCompanyId));
        }

        #endregion

        #region PriceList

        [HttpGet]
        [Route("PriceList/")]
        public IHttpActionResult GetPriceLists(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, pplm.GetPriceListTypesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, pplm.GetPriceListTypesForGrid(base.ActorCompanyId));

            return Content(HttpStatusCode.OK, pplm.GetPriceListTypes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("PriceList/{priceListId:int}")]
        public IHttpActionResult GetPriceList(int priceListId)
        {
            return Content(HttpStatusCode.OK, pplm.GetPriceListType(priceListId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("PriceList")]
        public IHttpActionResult SavePriceList(PriceListTypeDTO priceListTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pplm.SavePriceListTypeDTO(priceListTypeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PriceList/{priceListTypeId:int}")]
        public IHttpActionResult DeletePriceList(int priceListTypeId)
        {
            return Content(HttpStatusCode.OK, pplm.DeletePriceListType(priceListTypeId, base.ActorCompanyId));
        }

        #endregion

        #region PriceRule

        [HttpGet]
        [Route("CompanyPriceRule/")]
        public IHttpActionResult GetCompanyPriceRules(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, rm.GetCompanyPriceRules(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PriceListTypeMarkups/")]
        public IHttpActionResult GetPriceListTypeMarkups()
        {
            return Content(HttpStatusCode.OK, rm.GetPriceListTypeMarkups(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PriceRule/{priceRuleId:int}")]
        public IHttpActionResult GetPriceRule(int priceRuleId)
        {
            return Content(HttpStatusCode.OK, rm.GetPriceRule(priceRuleId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("PriceRule")]
        public IHttpActionResult SavePriceRule(PriceRuleDTO priceRuleDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.AddPriceRule(priceRuleDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PriceRule/Validate")]
        public IHttpActionResult ValidatePriceRule(ValidatePriceRuleDTO input)
        {
            return Content(HttpStatusCode.OK, rm.ValidatePriceRule(input));
        }

        [HttpDelete]
        [Route("PriceRule/{priceRuleId:int}")]
        public IHttpActionResult DeletePriceRule(int priceRuleId)
        {
            return Content(HttpStatusCode.OK, rm.DeletePriceRule(priceRuleId, base.ActorCompanyId));
        }

        #endregion

        #region ProductGroup

        [HttpGet]
        [Route("ProductGroup/{addEmptyRow:bool}")]
        public IHttpActionResult GetProductGroups(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetProductGroups(base.ActorCompanyId, addEmptyRow));
        }

        [HttpGet]
        [Route("ProductGroup/{productGroupId:int}")]
        public IHttpActionResult GetProductGroup(int productGroupId)
        {
            return Content(HttpStatusCode.OK, im.GetProductGroup(productGroupId));
        }

        [HttpPost]
        [Route("ProductGroup")]
        public IHttpActionResult SaveProductGroup(ProductGroupDTO productGroupDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveProductGroup(productGroupDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ProductGroup/{productGroupId:int}")]
        public IHttpActionResult DeleteProductGroup(int productGroupId)
        {
            return Content(HttpStatusCode.OK, im.DeleteProductGroup(productGroupId));
        }

        #endregion

        #region Project        

        [HttpGet]
        [Route("ProjectList/{projectStatus:int}/{onlyMine:bool}")]
        public IHttpActionResult GetProjectList(int projectStatus, bool onlyMine)
        {
            return Content(HttpStatusCode.OK, projm.GetProjectList(base.ActorCompanyId, projectStatus, onlyMine));
        }

        [HttpGet]
        [Route("Project/{onlyActive:bool}/{hidden:bool}/{setStatusName:bool}/{includeManagerName:bool}/{loadOrders:bool}/{projectStatus:int}")]
        public IHttpActionResult GetProjects(bool onlyActive, bool hidden, bool setStatusName, bool includeManagerName, bool loadOrders, int projectStatus)
        {
            if (onlyActive)
                return Content(HttpStatusCode.OK, projm.GetProjects(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, onlyActive, hidden, setStatusName, includeManagerName, loadOrders, -1, projectStatus).ToGridDTOs(includeManagerName, loadOrders));
            else
                return Content(HttpStatusCode.OK, projm.GetProjects(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, null, hidden, setStatusName, includeManagerName, loadOrders, -1, projectStatus).ToGridDTOs(includeManagerName, loadOrders));
        }

        [HttpGet]
        [Route("Project/Small/{onlyActive:bool}/{hidden:bool}/{sortOnNumber:bool}")]
        public IHttpActionResult GetProjectsSmall(bool onlyActive, bool hidden, bool sortOnNumber)
        {
            if (onlyActive)
                return Content(HttpStatusCode.OK, projm.GetProjectsSmall(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, onlyActive, hidden, sortOnNumber));
            else
                return Content(HttpStatusCode.OK, projm.GetProjectsSmall(base.ActorCompanyId, TermGroup_ProjectType.TimeProject, null, hidden, sortOnNumber));
        }

        [HttpGet]
        [Route("Project/{projectId:int}")]
        public IHttpActionResult GetProject(int projectId)
        {
            return Content(HttpStatusCode.OK, projm.GetProject(projectId, false));
        }

        [HttpGet]
        [Route("Project/GridDTO/{projectId:int}")]
        public IHttpActionResult GetProjectGridDTO(int projectId)
        {
            return Content(HttpStatusCode.OK, projm.GetProject(projectId, true).ToGridDTO(true, false));
        }

        [HttpPost]
        [Route("Project/")]
        public IHttpActionResult ChangeProjectStatus(ChangeProjectStatusModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, projm.ChangeProjectStatus(model.Ids, model.NewState));
        }

        [HttpGet]
        [Route("Project/GetProjectTraceViews/{projectId:int}")]
        public IHttpActionResult GetProjectTraceViews(int projectId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(ActorCompanyId);

            return Ok(projm.GetProjectTraceViews(projectId, baseSysCurrencyId));
        }

        [HttpGet]
        [Route("Project/GetProjectCentralStatus/{projectId:int}/{includeChildProjects:bool}")]
        public IHttpActionResult GetProjectCentralStatus(int projectId, bool includeChildProjects)
        {
            var projectCentral = new ProjectCentralManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, projectCentral.GetProjectCentralStatusList(base.ActorCompanyId, projectId, null, null, includeChildProjects));
        }

        [HttpPost]
        [Route("Project/Search/")]
        public IHttpActionResult GetProjectsBySearch(ProjectSearchModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, projm.GetProjectsBySearch2(model.Number, model.Name, model.CustomerNr, model.CustomerName, model.ManagerName, model.OrderNr, model.OnlyActive, model.Hidden, model.ShowWithoutCustomer, model.LoadMine, model.CustomerId, model.ShowAllProjects));
        }

        #endregion

        #region SupplierAgreements

        [HttpGet]
        [Route("SupplierAgreementProviders/")]
        public IHttpActionResult GetSupplierAgreementProviders(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, sam.GetSupplierAgreementProviders(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("SupplierAgreements/{providerType:int}")]
        public IHttpActionResult GetSupplierAgreements(int providerType)
        {
            return Content(HttpStatusCode.OK, sam.GetSupplierAgreements(base.ActorCompanyId, providerType).ToDTOs());
        }

        [HttpDelete]
        [Route("SupplierAgreements/{wholesellerId:int}/{priceListTypeId:int}")]
        public IHttpActionResult DeleteSupplierAgreements(int wholesellerId, int priceListTypeId)
        {
            return Content(HttpStatusCode.OK, sam.DeleteSupplierAgreements(base.ActorCompanyId, (SoeSupplierAgreementProvider)wholesellerId, priceListTypeId));
        }

        [HttpPost]
        [Route("SupplierAgreement/Discount/")]
        public IHttpActionResult SaveSupplierAgreementDiscount(SupplierAgreementDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sam.SaveDiscount(model, base.ActorCompanyId));
        }

        #endregion

        #region SysPriceLists

        [HttpGet]
        [Route("SysPriceListCodes")]
        public IHttpActionResult GetSysPricelistCodeBySysWholesellerId([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] swIds)
        {
            return Content(HttpStatusCode.OK, spm.GetSysPricelistCodeBySysWholesellerId(swIds.ToList()));
        }

        #endregion

        #region SysWholeseller

        [HttpGet]
        [Route("SysWholeseller/{sysWholesellerId:int}/{loadSysWholesellerEdi:bool}/{loadSysEdiMsg:bool}/{loadSysEdiType:bool}")]
        public IHttpActionResult GetSysWholeseller(int sysWholesellerId, bool loadSysWholesellerEdi, bool loadSysEdiMsg, bool loadSysEdiType)
        {
            return Content(HttpStatusCode.OK, wm.GetSysWholeseller(sysWholesellerId, loadSysWholesellerEdi, loadSysEdiMsg, loadSysEdiType).ToDTO());
        }

        [HttpGet]
        [Route("SysWholesellers/")]
        public IHttpActionResult GetSysWholesellers(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, wm.GetWholesellerDictByCompany(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            // TODO: Replace with GridDTO when grid is created
            return Content(HttpStatusCode.OK, wm.GetSysWholesellersByCompany(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("SysWholesellersByCompany/{onlyNotUsed:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysWholesellersByCompany(bool onlyNotUsed, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, wm.GetSysWholesellersByCompanyDict(base.ActorCompanyId, onlyNotUsed, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region Templates

        [HttpGet]
        [Route("Templates/")]
        public IHttpActionResult GetTemplates()
        {
            return Content(HttpStatusCode.OK, im.GetInvoiceTemplates(base.ActorCompanyId, SoeInvoiceType.CustomerInvoice));
        }

        #endregion

        #region WholesellerSettings

        [HttpGet]
        [Route("Wholesellers/")]
        public IHttpActionResult GetWholesellers(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, wm.GetCompanyWholesellerDTOs(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("CompanySysWholeseller/{companyWholesellerId:int}")]
        public IHttpActionResult GetCompanySysWholeseller(int companyWholesellerId)
        {
            return Content(HttpStatusCode.OK, wm.GetCompanyWholesellerDTO(companyWholesellerId));
        }

        [HttpGet]
        [Route("SupplierBySysWholeseller/{sysWholesellerId:int}")]
        public IHttpActionResult GetSupplierBySysWholeseller(int sysWholesellerId)
        {
            return Content(HttpStatusCode.OK, sm.GetSupplierBySysWholeseller(base.ActorCompanyId, sysWholesellerId));
        }

        [HttpPost]
        [Route("CompanySysWholeseller")]
        public IHttpActionResult SaveCompanySysWholeseller(CompanyWholesellerSettingModel model)
        {
            return Content(HttpStatusCode.OK, wm.SaveCompanyWholesellerDTO(base.ActorCompanyId, model.CompanyWholesellerDTO, model.CustomerNbrs, model.ActorSupplierId));
        }

        [HttpDelete]
        [Route("WholesellerPriceList/{priceListImportedHeadId:int}")]
        public IHttpActionResult DeleteCompPriceList(int priceListImportedHeadId)
        {
            return Content(HttpStatusCode.OK, prm.DeleteCompPriceList(base.ActorCompanyId, priceListImportedHeadId));
        }

        #endregion

        #region WholesellerPriceLists

        [HttpGet]
        [Route("WholesellerPriceLists/{isUsed:bool}")]
        public IHttpActionResult GetWholesellerPriceLists(bool isUsed)
        {
            return Content(HttpStatusCode.OK, wm.GetCompanyWholesellerPriceLists(base.ActorCompanyId, isUsed ? isUsed : (bool?)null) );
        }

        [HttpGet]
        [Route("PriceListsToUpdate")]
        public IHttpActionResult GetPriceListsToUpdate()
        {
            return Content(HttpStatusCode.OK, wm.GetCompanyWholesellerPriceListsToUpdate(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("WholesellerPriceLists")]
        public IHttpActionResult SaveWholesellerPriceLists(List<CompanyWholesellerPriceListViewDTO> priceListViewDTO)
        {
            return Content(HttpStatusCode.OK, wm.SaveCompanyWholesellerPriceLists(priceListViewDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("WholesellerPriceLists/Upgrade")]
        public IHttpActionResult UpgradeWholesellerPriceLists(List<int> sysWholesellerIds)
        {
            return Content(HttpStatusCode.OK, wm.UpgradeCompanyWholeSellerPriceLists(base.ActorCompanyId, sysWholesellerIds));
        }

        [HttpDelete]
        [Route("CompanySysWholeseller/{companySysWholesellerId:int}")]
        public IHttpActionResult DeleteCompanyWholeseller(int companySysWholesellerId)
        {
            return Content(HttpStatusCode.OK, wm.DeleteCompanyWholesellerDTO(base.ActorCompanyId, companySysWholesellerId));
        }

        #endregion

        #region Voucher

        [HttpGet]
        [Route("Voucher/getVoucherTraceViews/{voucherHeadId:int}")]
        public IHttpActionResult GetVoucherTaceViews(int voucherHeadId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, vm.GetVoucherTraceViews(voucherHeadId, baseSysCurrencyId));
        }

        #endregion Voucher

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
    }
}