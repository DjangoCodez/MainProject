using SoftOne.Soe.Business.Core.Template.Models.Attest;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using SoftOne.Soe.Business.Core.Template.Models.Core;
using SoftOne.Soe.Business.Core.Template.Models.Economy;
using SoftOne.Soe.Business.Core.Template.Models.Time;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Template.Models
{
    public class TemplateCompanyDataItem
    {
        public TemplateCompanyDataItem(int sysCompDbId)
        {
            TemplateCompanyCoreDataItem = new TemplateCompanyCoreDataItem();
            TemplateCompanyEconomyDataItem = new TemplateCompanyEconomyDataItem();
            TemplateCompanyAttestDataItem = new TemplateCompanyAttestDataItem();
            TemplateCompanyBillingDataItem = new TemplateCompanyBillingDataItem();
            TemplateCompanyTimeDataItem = new TemplateCompanyTimeDataItem();
            SysCompDbId = sysCompDbId;
        }
        public CopyFromTemplateCompanyInputDTO InputDTO { get; set; }
        public int SysCompDbId { get; set; }
        public int UserId { get; set; }
        public bool Update { get; set; }
        public int SourceActorCompanyId { get; set; }
        public int DestinationActorCompanyId { get; set; }
        public int DestinationLicenseId { get; set; }

        public TemplateCompanyCoreDataItem TemplateCompanyCoreDataItem { get; set; }
        public TemplateCompanyEconomyDataItem TemplateCompanyEconomyDataItem { get; set; }
        public TemplateCompanyAttestDataItem TemplateCompanyAttestDataItem { get; set; }
        public TemplateCompanyBillingDataItem TemplateCompanyBillingDataItem { get; set; }
        public TemplateCompanyTimeDataItem TemplateCompanyTimeDataItem { get; set; }
        public ChildCopyItemRequest ChildCopyItemRequest { get; set; }

        public bool HasValidChildCopyItemRequest(ChildCopyItemRequestType type)
        {
            if (ChildCopyItemRequest == null)
                return false;

            if (ChildCopyItemRequest.ChildCopyItemRequestType == ChildCopyItemRequestType.None)
                return false;

            if (ChildCopyItemRequest.Ids == null || ChildCopyItemRequest.Ids.Count == 0)
                return false;

            if (ChildCopyItemRequest.ChildCopyItemRequestType != type)
                return false;

            return true;
        }

        public List<int> GetIdsFromChildCopyItemRequest(ChildCopyItemRequestType type)
        {
            if (!HasValidChildCopyItemRequest(type))
                return null;

            if (!ChildCopyItemRequest.Ids.Any())
                return null;

            return ChildCopyItemRequest.Ids;
        }
    }

    public class TemplateCompanyEconomyDataItem
    {
        public TemplateCompanyEconomyDataItem()
        {
            AccountStdCopyItems = new List<AccountStdCopyItem>();
            AccountInternalCopyItems = new List<AccountInternalCopyItem>();
            AccountDimCopyItems = new List<AccountDimCopyItem>();
            VoucherSeriesTypeCopyItems = new List<VoucherSeriesTypeCopyItem>();
            PaymentMethodCopyItems = new List<PaymentMethodCopyItem>();
            PaymentConditionCopyItems = new List<PaymentConditionCopyItem>();
            GrossProfitCodeCopyItems = new List<GrossProfitCodeCopyItem>();
            InventoryWriteOffMethodCopyItems = new List<InventoryWriteOffMethodCopyItem>();
            InventoryWriteOffTemplateCopyItems = new List<InventoryWriteOffTemplateCopyItem>();
            VatCodeCopyItems = new List<VatCodeCopyItem>();
            AutoAccountDistributionCopyItems = new List<AccountDistribitionCopyItem>();
            PeriodAccountDistributionCopyItems = new List<AccountDistribitionCopyItem>();
            DistributionCodeHeadCopyItems = new List<DistributionCodeHeadCopyItem>();
            VoucherTemplatesCopyItems = new List<VoucherTemplatesCopyItem>();
            SupplierCopyItem = new SupplierCopyItem();
        }

        public List<AccountStdCopyItem> AccountStdCopyItems { get; set; }
        public List<AccountInternalCopyItem> AccountInternalCopyItems { get; set; }
        public List<AccountDimCopyItem> AccountDimCopyItems { get; set; }
        public List<VoucherSeriesTypeCopyItem> VoucherSeriesTypeCopyItems { get; set; }
        public List<AccountYearCopyItem> AccountYearCopyItems { get; set; }
        public List<PaymentMethodCopyItem> PaymentMethodCopyItems { get; set; }
        public List<PaymentConditionCopyItem> PaymentConditionCopyItems { get; set; }
        public List<GrossProfitCodeCopyItem> GrossProfitCodeCopyItems { get; set; }
        public List<InventoryWriteOffMethodCopyItem> InventoryWriteOffMethodCopyItems { get; set; }
        public List<InventoryWriteOffTemplateCopyItem> InventoryWriteOffTemplateCopyItems { get; set; }
        public List<VatCodeCopyItem> VatCodeCopyItems { get; set; }
        public List<AccountDistribitionCopyItem> AutoAccountDistributionCopyItems { get; set; }
        public List<AccountDistribitionCopyItem> PeriodAccountDistributionCopyItems { get; set; }
        public List<DistributionCodeHeadCopyItem> DistributionCodeHeadCopyItems { get; set; }

        public List<VoucherTemplatesCopyItem> VoucherTemplatesCopyItems { get; set; }
        public List<ResidualCodeCopyItem> ResidualCodesCopyItems { get; set; }
        public SupplierCopyItem SupplierCopyItem { get; set; }



        #region AccountYears
        private Dictionary<int, AccountYear> AccountYearMappings { get; set; }

        public void AddAccountYearMapping(int templateAccountYearId, AccountYear item)
        {
            if (AccountYearMappings == null)
                AccountYearMappings = new Dictionary<int, AccountYear>();
            if (!AccountYearMappings.ContainsKey(templateAccountYearId))
                AccountYearMappings.Add(templateAccountYearId, item);
        }

        public AccountYear GetAccountYear(int accountYearId)
        {
            if (AccountYearMappings == null)
                return null;

            if (AccountYearMappings.ContainsKey(accountYearId))
                return AccountYearMappings[accountYearId];

            return null;
        }

        #endregion


        #region AccountDims
        private Dictionary<int, AccountDim> AccountDimMappings { get; set; }

        public void AddAccountDimMapping(int templateAccountDimId, AccountDim item)
        {
            if (AccountDimMappings == null)
                AccountDimMappings = new Dictionary<int, AccountDim>();
            if (!AccountDimMappings.ContainsKey(templateAccountDimId))
                AccountDimMappings.Add(templateAccountDimId, item);
        }

        public AccountDim GetAccountDim(int accountDimId)
        {
            if (AccountDimMappings == null)
                return null;

            if (AccountDimMappings.ContainsKey(accountDimId))
                return AccountDimMappings[accountDimId];

            return null;
        }

        #endregion

        #region Accounts
        private Dictionary<int, Account> AccountMappings { get; set; }


        public void AddAccountMapping(int templateAccountId, Account item)
        {
            if (AccountMappings == null)
                AccountMappings = new Dictionary<int, Account>();
            if (!AccountMappings.ContainsKey(templateAccountId))
                AccountMappings.Add(templateAccountId, item);
        }

        public Account GetAccount(int accountId)
        {
            if (AccountMappings == null)
                return null;

            if (AccountMappings.ContainsKey(accountId))
                return AccountMappings[accountId];

            return null;
        }

        #endregion

        #region VoucherSeriesTypes
        private Dictionary<int, VoucherSeriesType> VoucherSeriesTypeMappings { get; set; }


        public void AddVoucherSeriesTypeMapping(int templateVoucherSeriesTypeId, VoucherSeriesType item)
        {
            if (VoucherSeriesTypeMappings == null)
                VoucherSeriesTypeMappings = new Dictionary<int, VoucherSeriesType>();
            if (!VoucherSeriesTypeMappings.ContainsKey(templateVoucherSeriesTypeId))
                VoucherSeriesTypeMappings.Add(templateVoucherSeriesTypeId, item);
        }

        public VoucherSeriesType GetVoucherSeriesType(int VoucherSeriesTypeId)
        {
            if (VoucherSeriesTypeMappings == null)
                return null;

            if (VoucherSeriesTypeMappings.ContainsKey(VoucherSeriesTypeId))
                return VoucherSeriesTypeMappings[VoucherSeriesTypeId];

            return null;
        }

        #endregion

        #region PaymentConditions
        private Dictionary<int, PaymentCondition> PaymentConditionMappings { get; set; }


        public void AddPaymentConditionMapping(int templatePaymentConditionId, PaymentCondition item)
        {
            if (PaymentConditionMappings == null)
                PaymentConditionMappings = new Dictionary<int, PaymentCondition>();
            if (!PaymentConditionMappings.ContainsKey(templatePaymentConditionId))
                PaymentConditionMappings.Add(templatePaymentConditionId, item);
        }

        public PaymentCondition GetPaymentCondition(int PaymentConditionId)
        {
            if (PaymentConditionMappings == null)
                return null;

            if (PaymentConditionMappings.ContainsKey(PaymentConditionId))
                return PaymentConditionMappings[PaymentConditionId];

            return null;
        }

        #endregion

        #region VatCodes
        private Dictionary<int, VatCode> VatCodeMappings { get; set; }


        public void AddVatCodeMapping(int templateVatCodeId, VatCode item)
        {
            if (VatCodeMappings == null)
                VatCodeMappings = new Dictionary<int, VatCode>();
            if (!VatCodeMappings.ContainsKey(templateVatCodeId))
                VatCodeMappings.Add(templateVatCodeId, item);
        }

        public VatCode GetVatCode(int vatCodeId)
        {
            if (VatCodeMappings == null)
                return null;

            if (VatCodeMappings.ContainsKey(vatCodeId))
                return VatCodeMappings[vatCodeId];

            return null;
        }

        internal void AddVatCodeMapping(object vatCodeId, VatCode vatCode)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region VoucherTemplates

        private Dictionary<int, VoucherHead> VoucherTemplateMappings { get; set; }


        public void AddVoucherTemplateMapping(int voucherHeadId, VoucherHead item)
        {
            if (VoucherTemplateMappings == null)
                VoucherTemplateMappings = new Dictionary<int, VoucherHead>();
            if (!VoucherTemplateMappings.ContainsKey(voucherHeadId))
                VoucherTemplateMappings.Add(voucherHeadId, item);
        }

        public VoucherHead GetVoucherTemplate(int voucherHeadId)
        {
            if (VoucherTemplateMappings == null)
                return null;

            if (VoucherTemplateMappings.ContainsKey(voucherHeadId))
                return VoucherTemplateMappings[voucherHeadId];

            return null;
        }

        #endregion

    }

    public class TemplateCompanyAttestDataItem
    {
        public TemplateCompanyAttestDataItem()
        {
            AttestRoleCopyItems = new List<AttestRoleCopyItem>();
            AttestStateCopyItems = new List<AttestStateCopyItem>();
            AttestTransitionCopyItems = new List<AttestTransitionCopyItem>();
            CategoryCopyItems = new List<CategoryCopyItem>();
            AttestWorkFlowTemplateHeadCopyItems = new List<AttestWorkFlowTemplateHeadCopyItem>();
        }

        public List<AttestRoleCopyItem> AttestRoleCopyItems { get; set; }
        public List<AttestStateCopyItem> AttestStateCopyItems { get; set; }
        public List<AttestTransitionCopyItem> AttestTransitionCopyItems { get; set; }
        public List<CategoryCopyItem> CategoryCopyItems { get; set; }
        public List<AttestWorkFlowTemplateHeadCopyItem> AttestWorkFlowTemplateHeadCopyItems { get; set; }

        private Dictionary<int, AttestRole> AttestRoleMappings { get; set; }

        public void AddAttestRoleMapping(int templateAttestRoleId, AttestRole item)
        {
            if (AttestRoleMappings == null)
                AttestRoleMappings = new Dictionary<int, AttestRole>();
            if (!AttestRoleMappings.ContainsKey(templateAttestRoleId))
                AttestRoleMappings.Add(templateAttestRoleId, item);
        }

        public AttestRole GetAttestRole(int RoleId)
        {
            if (AttestRoleMappings == null)
                return null;

            if (AttestRoleMappings.ContainsKey(RoleId))
                return AttestRoleMappings[RoleId];

            return null;
        }

        public Dictionary<int, AttestRole> GetAttestRoleMappings()
        {
            if (AttestRoleMappings == null)
                AttestRoleMappings = new Dictionary<int, AttestRole>();

            return AttestRoleMappings;
        }

        private Dictionary<int, AttestState> AttestStateMappings { get; set; }

        public void AddAttestStateMapping(int templateAttestStateId, AttestState item)
        {
            if (AttestStateMappings == null)
                AttestStateMappings = new Dictionary<int, AttestState>();
            if (!AttestStateMappings.ContainsKey(templateAttestStateId))
                AttestStateMappings.Add(templateAttestStateId, item);
        }

        public AttestState GetAttestState(int StateId)
        {
            if (AttestStateMappings == null)
                return null;

            if (AttestStateMappings.ContainsKey(StateId))
                return AttestStateMappings[StateId];

            return null;
        }

        private Dictionary<int, Category> CategoryMappings { get; set; }

        public void AddCategoryMapping(int templateCategoryId, Category item)
        {
            if (CategoryMappings == null)
                CategoryMappings = new Dictionary<int, Category>();
            if (!CategoryMappings.ContainsKey(templateCategoryId))
                CategoryMappings.Add(templateCategoryId, item);
        }

        public Category GetCategory(int StateId)
        {
            if (CategoryMappings == null)
                return null;

            if (CategoryMappings.ContainsKey(StateId))
                return CategoryMappings[StateId];

            return null;
        }

        private Dictionary<int, AttestTransition> AttestTransitionMappings { get; set; }

        public void AddAttestTransitionMapping(int templateAttestTransitionId, AttestTransition item)
        {
            if (AttestTransitionMappings == null)
                AttestTransitionMappings = new Dictionary<int, AttestTransition>();
            if (!AttestTransitionMappings.ContainsKey(templateAttestTransitionId))
                AttestTransitionMappings.Add(templateAttestTransitionId, item);
        }

        public AttestTransition GetAttestTransition(int TransitionId)
        {
            if (AttestTransitionMappings == null)
                return null;

            if (AttestTransitionMappings.ContainsKey(TransitionId))
                return AttestTransitionMappings[TransitionId];

            return null;
        }
    }

    public class TemplateCompanyBillingDataItem
    {
        public TemplateCompanyBillingDataItem()
        {
            InvoiceProductCopyItems = new List<InvoiceProductCopyItem>();
            ProductGroupCopyItems = new List<ProductGroupCopyItem>();
            ProductUnitCopyItems = new List<ProductUnitCopyItem>();
            PriceListCopyItems = new List<PriceListCopyItem>();
            SupplierAgreementCopyItems = new List<SupplierAgreementCopyItem>();
            ChecklistCopyItems = new List<ChecklistCopyItem>();
            EmailTemplateCopyItems = new List<EmailTemplateCopyItem>();
            CompanyWholesellerPricelistCopyItems = new List<CompanyWholesellerPriceListCopyItem>();
        }
        public List<InvoiceProductCopyItem> InvoiceProductCopyItems { get; set; }
        public List<ProductGroupCopyItem> ProductGroupCopyItems { get; set; }
        public List<ProductUnitCopyItem> ProductUnitCopyItems { get; set; }
        public List<PriceListCopyItem> PriceListCopyItems { get; set; }
        public List<SupplierAgreementCopyItem> SupplierAgreementCopyItems { get; set; }
        public List<ChecklistCopyItem> ChecklistCopyItems { get; set; }
        public List<EmailTemplateCopyItem> EmailTemplateCopyItems { get; set; }
        public List<CompanyWholesellerPriceListCopyItem> CompanyWholesellerPricelistCopyItems { get; set; }
        public PriceRuleCopyItem PriceRuleCopyItem { get; set; }
        public ProjectSettingsCopyItem ProjectSettingsCopyItem { get; set; }

        #region ProjectSettings

        private Dictionary<int, TimeCode> TimeCodeMappings { get; set; }

        public void AddTimeCodeMapping(int timeCodeId, TimeCode item)
        {
            if (TimeCodeMappings == null)
                TimeCodeMappings = new Dictionary<int, TimeCode>();
            if (!TimeCodeMappings.ContainsKey(timeCodeId))
                TimeCodeMappings.Add(timeCodeId, item);
        }

        public TimeCode GetTimeCode(int timeCodeId)
        {
            if (TimeCodeMappings == null)
                return null;

            if (TimeCodeMappings.ContainsKey(timeCodeId))
                return TimeCodeMappings[timeCodeId];

            return null;
        }

        #endregion

        #region EmailTemplates
        private Dictionary<int, EmailTemplate> EmailTemplateMappings { get; set; }


        public void AddEmailTemplateMapping(int emailTemplateId, EmailTemplate item)
        {
            if (EmailTemplateMappings == null)
                EmailTemplateMappings = new Dictionary<int, EmailTemplate>();
            if (!EmailTemplateMappings.ContainsKey(emailTemplateId))
                EmailTemplateMappings.Add(emailTemplateId, item);
        }

        public EmailTemplate GetEmailTemplate(int emailTemplateId)
        {
            if (EmailTemplateMappings == null)
                return null;

            if (EmailTemplateMappings.ContainsKey(emailTemplateId))
                return EmailTemplateMappings[emailTemplateId];

            return null;
        }

        #endregion

        #region InvoiceProducts
        private Dictionary<int, InvoiceProduct> InvoiceProductMappings { get; set; }

        public void AddInvoiceProductMapping(int templateInvoiceProductId, InvoiceProduct item)
        {
            if (InvoiceProductMappings == null)
                InvoiceProductMappings = new Dictionary<int, InvoiceProduct>();
            if (!InvoiceProductMappings.ContainsKey(templateInvoiceProductId))
                InvoiceProductMappings.Add(templateInvoiceProductId, item);
        }

        public InvoiceProduct GetInvoiceProduct(int InvoiceProductId)
        {
            if (InvoiceProductMappings == null)
                return null;

            if (InvoiceProductMappings.ContainsKey(InvoiceProductId))
                return InvoiceProductMappings[InvoiceProductId];

            return null;
        }

        #endregion

        #region PriceLists
        private Dictionary<int, PriceListType> PriceListMappings { get; set; }


        public void AddPriceListMapping(int templatePriceListId, PriceListType item)
        {
            if (PriceListMappings == null)
                PriceListMappings = new Dictionary<int, PriceListType>();
            if (!PriceListMappings.ContainsKey(templatePriceListId))
                PriceListMappings.Add(templatePriceListId, item);
        }

        public PriceListType GetPriceList(int PriceListId)
        {
            if (PriceListMappings == null)
                return null;

            if (PriceListMappings.ContainsKey(PriceListId))
                return PriceListMappings[PriceListId];

            return null;
        }

        #endregion

        #region DeliveryConditions

        private Dictionary<int, DeliveryType> DeliveryTypeMappings { get; set; }

        public void AddDeliveryTypeMapping(int templateDeliveryTypeId, DeliveryType item)
        {
            if (DeliveryTypeMappings == null)
                DeliveryTypeMappings = new Dictionary<int, DeliveryType>();
            if (!DeliveryTypeMappings.ContainsKey(templateDeliveryTypeId))
                DeliveryTypeMappings.Add(templateDeliveryTypeId, item);
        }

        public DeliveryType GetDeliveryType(int DeliveryTypeId)
        {
            if (DeliveryTypeMappings == null)
                return null;
            if (DeliveryTypeMappings.ContainsKey(DeliveryTypeId))
                return DeliveryTypeMappings[DeliveryTypeId];
            return null;
        }


        private Dictionary<int, DeliveryCondition> DeliveryConditionMappings { get; set; }

        public void AddDeliveryConditionMapping(int templateDeliveryConditionId, DeliveryCondition item)
        {
            if (DeliveryConditionMappings == null)
                DeliveryConditionMappings = new Dictionary<int, DeliveryCondition>();
            if (!DeliveryConditionMappings.ContainsKey(templateDeliveryConditionId))
                DeliveryConditionMappings.Add(templateDeliveryConditionId, item);
        }

        public DeliveryCondition GetDeliveryCondition(int DeliveryConditionId)
        {
            if (DeliveryConditionMappings == null)
                return null;
            if (DeliveryConditionMappings.ContainsKey(DeliveryConditionId))
                return DeliveryConditionMappings[DeliveryConditionId];
            return null;
        }

        #endregion

        #region ProductUnit

        private Dictionary<int, ProductUnit> ProductUnitMappings { get; set; }

        public void AddProductUnitMapping(int templateProductUnitId, ProductUnit item)
        {
            if (ProductUnitMappings == null)
                ProductUnitMappings = new Dictionary<int, ProductUnit>();
            if (!ProductUnitMappings.ContainsKey(templateProductUnitId))
                ProductUnitMappings.Add(templateProductUnitId, item);
        }

        public ProductUnit GetProductUnit(int ProductUnitId)
        {
            if (ProductUnitMappings == null)
                return null;
            if (ProductUnitMappings.ContainsKey(ProductUnitId))
                return ProductUnitMappings[ProductUnitId];
            return null;
        }

        internal object GetInvoiceProduct(object productId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region

        #endregion
    }
    public class TemplateCompanyTimeDataItem
    {
        public TemplateCompanyTimeDataItem()
        {
            DayTypeCopyItems = new List<DayTypeCopyItem>();
            TimeHalfDayCopyItems = new List<TimeHalfDayCopyItem>();
            HolidayCopyItems = new List<HolidayCopyItem>();
            TimePeriodHeadCopyItems = new List<TimePeriodHeadCopyItem>();
            PositionCopyItems = new List<PositionCopyItem>();
            PayrollPriceTypeCopyItems = new List<PayrollPriceTypeCopyItem>();
            PayrollPriceFormulaCopyItems = new List<PayrollPriceFormulaCopyItem>();
            VacationGroupCopyItems = new List<VacationGroupCopyItem>();
            TimeScheduleTypeCopyItems = new List<TimeScheduleTypeCopyItem>();
            ShiftTypeCopyItems = new List<ShiftTypeCopyItem>();
            SkillCopyItems = new List<SkillCopyItem>();
            ScheduleCycleCopyItems = new List<ScheduleCycleCopyItem>();
            FollowUpTypeCopyItems = new List<FollowUpTypeCopyItem>();
            PayrollGroupCopyItems = new List<PayrollGroupCopyItem>();
            PayrollProductCopyItems = new List<PayrollProductCopyItem>();
            InvoiceProductCopyItems = new List<InvoiceProductCopyItem>();
            TimeCodeBreakGroupCopyItems = new List<TimeCodeBreakGroupCopyItem>();
            TimeCodeCopyItems = new List<TimeCodeCopyItem>();
            TimeBreakTemplateCopyItems = new List<TimeBreakTemplateCopyItem>();
            TimeDeviationCauseCopyItems = new List<TimeDeviationCauseCopyItem>();
            EmployeeGroupCopyItems = new List<EmployeeGroupCopyItem>();
            EmploymentTypeCopyItems = new List<EmploymentTypeCopyItem>();
            TimeAccumulatorCopyItems = new List<TimeAccumulatorCopyItem>();
            TimeRuleCopyItems = new List<TimeRuleCopyItem>();
            TimeAbsenceRuleCopyItems = new List<TimeAbsenceRuleCopyItem>();
            TimeAttestRuleCopyItems = new List<TimeAttestRuleCopyItem>();
            EmployeeCollectiveAgreementCopyItems = new List<EmployeeCollectiveAgreementCopyItem>();
            EmployeeTemplateCopyItems = new List<EmployeeTemplateCopyItem>();
        }
        public List<DayTypeCopyItem> DayTypeCopyItems { get; set; }
        public List<TimeHalfDayCopyItem> TimeHalfDayCopyItems { get; set; }
        public List<HolidayCopyItem> HolidayCopyItems { get; set; }
        public List<TimePeriodHeadCopyItem> TimePeriodHeadCopyItems { get; set; }
        public List<PositionCopyItem> PositionCopyItems { get; internal set; }
        public List<PayrollPriceTypeCopyItem> PayrollPriceTypeCopyItems { get; set; }
        public List<PayrollPriceFormulaCopyItem> PayrollPriceFormulaCopyItems { get; set; }
        public List<PayrollGroupCopyItem> PayrollGroupCopyItems { get; set; }
        public List<VacationGroupCopyItem> VacationGroupCopyItems { get; set; }
        public List<TimeScheduleTypeCopyItem> TimeScheduleTypeCopyItems { get; set; }
        public List<SkillCopyItem> SkillCopyItems { get; set; }
        public List<ShiftTypeCopyItem> ShiftTypeCopyItems { get; set; }
        public List<ScheduleCycleCopyItem> ScheduleCycleCopyItems { get; set; }
        public List<FollowUpTypeCopyItem> FollowUpTypeCopyItems { get; set; }
        public List<PayrollProductCopyItem> PayrollProductCopyItems { get; set; }
        public List<InvoiceProductCopyItem> InvoiceProductCopyItems { get; set; }
        public List<TimeCodeBreakGroupCopyItem> TimeCodeBreakGroupCopyItems { get; set; }
        public List<TimeCodeRankingGroupCopyItem> TimeCodeRankingGroupCopyItems { get; set; }
        public List<TimeCodeCopyItem> TimeCodeCopyItems { get; set; }
        public List<TimeDeviationCauseCopyItem> TimeDeviationCauseCopyItems { get; set; }
        public List<EmployeeGroupCopyItem> EmployeeGroupCopyItems { get; set; }
        public List<TimeBreakTemplateCopyItem> TimeBreakTemplateCopyItems { get; set; }
        public List<EmploymentTypeCopyItem> EmploymentTypeCopyItems { get; set; }
        public List<TimeAccumulatorCopyItem> TimeAccumulatorCopyItems { get; set; }
        public List<TimeRuleCopyItem> TimeRuleCopyItems { get; set; }
        public List<TimeAbsenceRuleCopyItem> TimeAbsenceRuleCopyItems { get; set; }
        public List<TimeAttestRuleCopyItem> TimeAttestRuleCopyItems { get; set; }
        public List<EmployeeCollectiveAgreementCopyItem> EmployeeCollectiveAgreementCopyItems { get; set; }
        public List<EmployeeTemplateCopyItem> EmployeeTemplateCopyItems { get; set; }

        #region DayTypes
        private Dictionary<int, DayType> DayTypeMappings { get; set; }

        public void AddDayTypeMapping(int templateDayTypeId, DayType item)
        {
            if (DayTypeMappings == null)
                DayTypeMappings = new Dictionary<int, DayType>();
            if (!DayTypeMappings.ContainsKey(templateDayTypeId))
                DayTypeMappings.Add(templateDayTypeId, item);
        }

        public DayType GetDayType(int DayTypeId)
        {
            if (DayTypeMappings == null)
                return null;

            if (DayTypeMappings.ContainsKey(DayTypeId))
                return DayTypeMappings[DayTypeId];

            return null;
        }

        #endregion

        #region PriceTypes
        private Dictionary<int, PayrollPriceType> PriceTypeMappings { get; set; }

        public void AddPriceTypeMapping(int templatePriceTypeId, PayrollPriceType item)
        {
            if (PriceTypeMappings == null)
                PriceTypeMappings = new Dictionary<int, PayrollPriceType>();
            if (!PriceTypeMappings.ContainsKey(templatePriceTypeId))
                PriceTypeMappings.Add(templatePriceTypeId, item);
        }

        public PayrollPriceType GetPriceType(int PriceTypeId)
        {
            if (PriceTypeMappings == null)
                return null;

            if (PriceTypeMappings.ContainsKey(PriceTypeId))
                return PriceTypeMappings[PriceTypeId];

            return null;
        }

        #endregion

        #region PriceFormulas
        private Dictionary<int, PayrollPriceFormula> PriceFormulaMappings { get; set; }

        public void AddPriceFormulaMapping(int templatePriceFormulaId, PayrollPriceFormula item)
        {
            if (PriceFormulaMappings == null)
                PriceFormulaMappings = new Dictionary<int, PayrollPriceFormula>();
            if (!PriceFormulaMappings.ContainsKey(templatePriceFormulaId))
                PriceFormulaMappings.Add(templatePriceFormulaId, item);
        }

        public PayrollPriceFormula GetPriceFormula(int formulaId)
        {
            if (PriceFormulaMappings == null)
                return null;

            if (PriceFormulaMappings.ContainsKey(formulaId))
                return PriceFormulaMappings[formulaId];

            return null;
        }

        #endregion

        #region  PayrollGroups
        private Dictionary<int, PayrollGroup> PayollGroupMappings { get; set; }

        public void AddPayrollGroupMapping(int templatePayrollGroupId, PayrollGroup item)
        {
            if (PayollGroupMappings == null)
                PayollGroupMappings = new Dictionary<int, PayrollGroup>();
            if (!PayollGroupMappings.ContainsKey(templatePayrollGroupId))
                PayollGroupMappings.Add(templatePayrollGroupId, item);
        }

        public PayrollGroup GetPayrollGroup(int GroupId)
        {
            if (PayollGroupMappings == null)
                return null;

            if (PayollGroupMappings.ContainsKey(GroupId))
                return PayollGroupMappings[GroupId];

            return null;
        }

        #endregion

        #region VacationGroups
        private Dictionary<int, VacationGroup> VacationGroupMappings { get; set; }

        public void AddVacationGroupMapping(int templateVacationGroupId, VacationGroup item)
        {
            if (VacationGroupMappings == null)
                VacationGroupMappings = new Dictionary<int, VacationGroup>();
            if (!VacationGroupMappings.ContainsKey(templateVacationGroupId))
                VacationGroupMappings.Add(templateVacationGroupId, item);
        }

        public VacationGroup GetVacationGroup(int VacationGroupId)
        {
            if (VacationGroupMappings == null)
                return null;

            if (VacationGroupMappings.ContainsKey(VacationGroupId))
                return VacationGroupMappings[VacationGroupId];

            return null;
        }

        #endregion

        #region  InvoiceProducts
        private Dictionary<int, InvoiceProduct> InvoiceProductMappings { get; set; }

        public void AddInvoiceProductMapping(int templateInvoiceProductId, InvoiceProduct item)
        {
            if (InvoiceProductMappings == null)
                InvoiceProductMappings = new Dictionary<int, InvoiceProduct>();
            if (!InvoiceProductMappings.ContainsKey(templateInvoiceProductId))
                InvoiceProductMappings.Add(templateInvoiceProductId, item);
        }
        public InvoiceProduct GetInvoiceProduct(int ProductId)
        {
            if (InvoiceProductMappings == null)
                return null;

            if (InvoiceProductMappings.ContainsKey(ProductId))
                return InvoiceProductMappings[ProductId];

            return null;
        }

        #endregion

        #region  PayrollProducts
        private Dictionary<int, PayrollProduct> PayollProductMappings { get; set; }

        public void AddPayrollProductMapping(int templatePayrollProductId, PayrollProduct item)
        {
            if (PayollProductMappings == null)
                PayollProductMappings = new Dictionary<int, PayrollProduct>();
            if (!PayollProductMappings.ContainsKey(templatePayrollProductId))
                PayollProductMappings.Add(templatePayrollProductId, item);
        }

        public PayrollProduct GetPayrollProduct(int ProductId)
        {
            if (PayollProductMappings == null)
                return null;

            if (PayollProductMappings.ContainsKey(ProductId))
                return PayollProductMappings[ProductId];

            return null;
        }

        #endregion

        #region TimePeriodHeads
        private Dictionary<int, TimePeriodHead> TimePeriodHeadMappings { get; set; }

        public void AddTimePeriodHeadMapping(int templateTimePeriodHeadId, TimePeriodHead item)
        {
            if (TimePeriodHeadMappings == null)
                TimePeriodHeadMappings = new Dictionary<int, TimePeriodHead>();
            if (!TimePeriodHeadMappings.ContainsKey(templateTimePeriodHeadId))
                TimePeriodHeadMappings.Add(templateTimePeriodHeadId, item);
        }

        public TimePeriodHead GetTimePeriodHead(int TimePeriodHeadId)
        {
            if (TimePeriodHeadMappings == null)
                return null;

            if (TimePeriodHeadMappings.ContainsKey(TimePeriodHeadId))
                return TimePeriodHeadMappings[TimePeriodHeadId];

            return null;
        }

        #endregion

        #region TimeScheduleTypes
        private Dictionary<int, TimeScheduleType> TimeScheduleTypeMappings { get; set; }

        public void AddTimeScheduleTypeMapping(int templateTimeScheduleTypeId, TimeScheduleType item)
        {
            if (TimeScheduleTypeMappings == null)
                TimeScheduleTypeMappings = new Dictionary<int, TimeScheduleType>();
            if (!TimeScheduleTypeMappings.ContainsKey(templateTimeScheduleTypeId))
                TimeScheduleTypeMappings.Add(templateTimeScheduleTypeId, item);
        }

        public TimeScheduleType GetTimeScheduleType(int TimeScheduleTypeId)
        {
            if (TimeScheduleTypeMappings == null)
                return null;

            if (TimeScheduleTypeMappings.ContainsKey(TimeScheduleTypeId))
                return TimeScheduleTypeMappings[TimeScheduleTypeId];

            return null;
        }

        #endregion

        #region ShiftTypes
        private Dictionary<int, ShiftType> ShiftTypeMappings { get; set; }


        public void AddShiftTypeMapping(int templateShiftTypeId, ShiftType item)
        {
            if (ShiftTypeMappings == null)
                ShiftTypeMappings = new Dictionary<int, ShiftType>();
            if (!ShiftTypeMappings.ContainsKey(templateShiftTypeId))
                ShiftTypeMappings.Add(templateShiftTypeId, item);
        }

        public ShiftType GetShiftType(int ShiftTypeId)
        {
            if (ShiftTypeMappings == null)
                return null;

            if (ShiftTypeMappings.ContainsKey(ShiftTypeId))
                return ShiftTypeMappings[ShiftTypeId];

            return null;
        }

        #endregion

        #region TimeCodes
        private Dictionary<int, TimeCode> TimeCodeMappings { get; set; }


        public void AddTimeCodeMapping(int templateTimeCodeId, TimeCode item)
        {
            if (TimeCodeMappings == null)
                TimeCodeMappings = new Dictionary<int, TimeCode>();
            if (!TimeCodeMappings.ContainsKey(templateTimeCodeId))
                TimeCodeMappings.Add(templateTimeCodeId, item);
        }

        public TimeCode GetTimeCode(int TimeCodeId)
        {
            if (TimeCodeMappings == null)
                return null;

            if (TimeCodeMappings.ContainsKey(TimeCodeId))
                return TimeCodeMappings[TimeCodeId];

            return null;
        }

        #endregion

        #region TimeCodeBreakGroups
        private Dictionary<int, TimeCodeBreakGroup> TimeCodeBreakGroupMappings { get; set; }


        public void AddTimeCodeBreakGroupMapping(int templateTimeCodeBreakGroupId, TimeCodeBreakGroup item)
        {
            if (TimeCodeBreakGroupMappings == null)
                TimeCodeBreakGroupMappings = new Dictionary<int, TimeCodeBreakGroup>();
            if (!TimeCodeBreakGroupMappings.ContainsKey(templateTimeCodeBreakGroupId))
                TimeCodeBreakGroupMappings.Add(templateTimeCodeBreakGroupId, item);
        }

        public TimeCodeBreakGroup GetTimeCodeBreakGroup(int TimeCodeBreakGroupId)
        {
            if (TimeCodeBreakGroupMappings == null)
                return null;

            if (TimeCodeBreakGroupMappings.ContainsKey(TimeCodeBreakGroupId))
                return TimeCodeBreakGroupMappings[TimeCodeBreakGroupId];

            return null;
        }

        #endregion

        #region TimeDeviationCauses
        private Dictionary<int, TimeDeviationCause> TimeDeviationCauseMappings { get; set; }


        public void AddTimeDeviationCauseMapping(int templateTimeDeviationCauseId, TimeDeviationCause item)
        {
            if (TimeDeviationCauseMappings == null)
                TimeDeviationCauseMappings = new Dictionary<int, TimeDeviationCause>();
            if (!TimeDeviationCauseMappings.ContainsKey(templateTimeDeviationCauseId))
                TimeDeviationCauseMappings.Add(templateTimeDeviationCauseId, item);
        }

        public TimeDeviationCause GetTimeDeviationCause(int TimeDeviationCauseId, List<TimeDeviationCause> existingTimeDeviationCauses = null)
        {
            if (TimeDeviationCauseMappings == null)
                return null;

            if (TimeDeviationCauseMappings.ContainsKey(TimeDeviationCauseId))
                return TimeDeviationCauseMappings[TimeDeviationCauseId];

            if (existingTimeDeviationCauses != null)
            {
                var templateTimeDeviationCause = TimeDeviationCauseCopyItems.FirstOrDefault(x => x.TimeDeviationCauseId == TimeDeviationCauseId);

                if (templateTimeDeviationCause != null)
                {
                    var existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(x => x.Name.ToLower() == templateTimeDeviationCause.Name.ToLower() || (!string.IsNullOrEmpty(x.ExtCode) && !string.IsNullOrEmpty(templateTimeDeviationCause.ExtCode) && x.ExtCode.ToLower() == templateTimeDeviationCause.ExtCode.ToLower()));
                    if (existingTimeDeviationCause != null)
                        return existingTimeDeviationCause;
                }
            }
            return null;
        }

        #endregion

        #region TimeCodeBreakGroups
        private Dictionary<int, EmployeeGroup> EmployeeGroupMappings { get; set; }

        public void AddEmployeeGroupMapping(int templateEmployeeGroupId, EmployeeGroup item)
        {
            if (EmployeeGroupMappings == null)
                EmployeeGroupMappings = new Dictionary<int, EmployeeGroup>();
            if (!EmployeeGroupMappings.ContainsKey(templateEmployeeGroupId))
                EmployeeGroupMappings.Add(templateEmployeeGroupId, item);
        }

        public EmployeeGroup GetEmployeeGroup(int EmployeeGroupId)
        {
            if (EmployeeGroupMappings == null)
                return null;

            if (EmployeeGroupMappings.ContainsKey(EmployeeGroupId))
                return EmployeeGroupMappings[EmployeeGroupId];

            return null;
        }

        internal object GetTimeCode(object timeCodeId)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    public class TemplateCompanyCoreDataItem
    {
        public TemplateCompanyCoreDataItem()
        {
            ImportCopyItems = new List<ImportCopyItem>();
            RoleAndFeatureCopyItems = new List<RoleAndFeatureCopyItem>();
            CompanyFieldSettingCopyItems = new List<CompanyFieldSettingCopyItem>();
            CompanySettingCopyItems = new List<CompanySettingCopyItem>();
            ReportCopyItems = new List<ReportCopyItem>();
            CompanyAndFeatureCopyItems = new List<CompanyAndFeatureCopyItem>();
            ReportTemplateCopyItems = new List<ReportTemplateCopyItem>();
            ExternalCodeCopyItems = new List<ExternalCodeCopyItem>();
        }
        public List<ImportCopyItem> ImportCopyItems { get; set; }
        public List<RoleAndFeatureCopyItem> RoleAndFeatureCopyItems { get; set; }
        public List<CompanyFieldSettingCopyItem> CompanyFieldSettingCopyItems { get; set; }
        public List<CompanySettingCopyItem> CompanySettingCopyItems { get; set; }
        public List<ReportCopyItem> ReportCopyItems { get; set; }
        public List<CompanyAndFeatureCopyItem> CompanyAndFeatureCopyItems { get; set; }
        public List<ReportTemplateCopyItem> ReportTemplateCopyItems { get; set; }
        public List<ExternalCodeCopyItem> ExternalCodeCopyItems { get; set; }
        public List<UserCopyItem> UserCopyItems { get; set; }

        #region

        public CompanySettingCopyItem GetCompanySetting(CompanySettingType companySettingType)
        {
            return CompanySettingCopyItems.FirstOrDefault(f => f.SettingTypeId == companySettingType);
        }



        #endregion

        #region Roles
        private Dictionary<int, Role> RoleMappings { get; set; }

        public void AddRoleMapping(int templateRoleId, Role item)
        {
            if (RoleMappings == null)
                RoleMappings = new Dictionary<int, Role>();
            if (!RoleMappings.ContainsKey(templateRoleId))
                RoleMappings.Add(templateRoleId, item);
        }

        public Role GetRole(int RoleId)
        {
            if (RoleMappings == null)
                return null;

            if (RoleMappings.ContainsKey(RoleId))
                return RoleMappings[RoleId];

            return null;
        }

        #endregion

        #region Users
        private Dictionary<int, User> UserMappings { get; set; }

        public void AddUserMapping(int templateUserId, User item)
        {
            if (UserMappings == null)
                UserMappings = new Dictionary<int, User>();
            if (!UserMappings.ContainsKey(templateUserId))
                UserMappings.Add(templateUserId, item);
        }

        public User GetUser(int UserId)
        {
            if (UserMappings == null)
                return null;

            if (UserMappings.ContainsKey(UserId))
                return UserMappings[UserId];

            return null;
        }

        #endregion

        #region Reports
        private Dictionary<int, Report> ReportMappings { get; set; }

        public void AddReportMapping(int templateReportId, Report item)
        {
            if (ReportMappings == null)
                ReportMappings = new Dictionary<int, Report>();
            if (!ReportMappings.ContainsKey(templateReportId))
                ReportMappings.Add(templateReportId, item);
        }

        public Report GetReport(int ReportId)
        {
            if (ReportMappings == null)
                return null;

            if (ReportMappings.ContainsKey(ReportId))
                return ReportMappings[ReportId];

            return null;
        }

        #endregion

        #region ReportTemplates
        private Dictionary<int, ReportTemplate> ReportTemplateMappings { get; set; }

        public void AddReportTemplateMapping(int templateReportTemplateId, ReportTemplate item)
        {
            if (ReportTemplateMappings == null)
                ReportTemplateMappings = new Dictionary<int, ReportTemplate>();
            if (!ReportTemplateMappings.ContainsKey(templateReportTemplateId))
                ReportTemplateMappings.Add(templateReportTemplateId, item);
        }

        public ReportTemplate GetReportTemplate(int ReportTemplateId)
        {
            if (ReportTemplateMappings == null)
                return null;

            if (ReportTemplateMappings.ContainsKey(ReportTemplateId))
                return ReportTemplateMappings[ReportTemplateId];

            return null;
        }

        #endregion
    }
}
