using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    [Log]
    [TSInclude]
    public class CustomerDTO
    {
        [LogActorId]
        public int ActorCustomerId { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public int? DeliveryConditionId { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? PaymentConditionId { get; set; }
        public int? PriceListTypeId { get; set; }
        public int CurrencyId { get; set; }
        public int? SysCountryId { get; set; }
        public int? SysLanguageId { get; set; }
        public int? SysWholeSellerId { get; set; }

        public string CustomerNr { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public string VatNr { get; set; }
        public string InvoiceReference { get; set; }
        public int GracePeriodDays { get; set; }
        public int PaymentMorale { get; set; }
        public string SupplierNr { get; set; }
        public int? OfferTemplate { get; set; }
        public int? OrderTemplate { get; set; }
        public int? BillingTemplate { get; set; }
        public int? AgreementTemplate { get; set; }
        public bool ManualAccounting { get; set; }
        public decimal DiscountMerchandise { get; set; }
        public decimal DiscountService { get; set; }
        public bool DisableInvoiceFee { get; set; }
        public decimal Discount2Merchandise { get; set; }
        public decimal Discount2Service { get; set; }
        public string Note { get; set; }
        public bool ShowNote { get; set; }
        public string FinvoiceAddress { get; set; }
        public string FinvoiceOperator { get; set; }
        public bool IsFinvoiceCustomer { get; set; }
        public string BlockNote { get; set; }
        public bool BlockOrder { get; set; }
        public bool BlockInvoice { get; set; }
        public int? CreditLimit { get; set; }
        public bool IsCashCustomer { get; set; }
        public bool IsOneTimeCustomer { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? InvoiceDeliveryProvider { get; set; }
        public bool ImportInvoicesDetailed { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string DepartmentNr { get; set; }
        public int PayingCustomerId { get; set; }
        public int? InvoicePaymentService { get; set; }
        public string BankAccountNr { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public int? ContactEComId { get; set; }
        public int? OrderContactEComId { get; set; }
        public int? ReminderContactEComId { get; set; }
        public int? ContactGLNId { get; set; }
        public string InvoiceLabel { get; set; }
        public bool AddSupplierInvoicesToEInvoice { get; set; }
        public bool IsEUCountryBased { get; set; }
        public bool TriangulationSales { get; set; }
        public string ContractNr { get; set; }

        // Extensions
        public bool Active { get; set; }

        [LogPrivateCustomer]
        public bool IsPrivatePerson { get; set; }

        public bool HasConsent { get; set; }
        public DateTime? ConsentDate { get; set; }
        public DateTime? ConsentModified { get; set; }
        public string ConsentModifiedBy { get; set; }
        public List<ContactAddressItem> ContactAddresses { get; set; }
        public List<int> ContactPersons { get; set; }
        public List<int> CategoryIds { get; set; }

        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> DebitAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> CreditAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> VatAccounts { get; set; }

        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }

        public List<CustomerUserDTO> CustomerUsers { get; set; }
        public List<CustomerProductPriceSmallDTO> CustomerProducts { get; set; }
        public List<FileUploadDTO> Files { get; set; }
    }
    [TSInclude]
    public class CustomerIODTO
    {
        public int CustomerIOId { get; set; }
        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public TermGroup_IOType Type { get; set; }
        public TermGroup_IOStatus Status { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public string BatchId { get; set; }

        public int? GracePeriodDays { get; set; }
        public string DeliveryMethod { get; set; }
        public string DefaultWholeseller { get; set; }
        public int? CustomerState { get; set; }
        public int? OfferTemplate { get; set; }
        public int? OrderTemplate { get; set; }
        public int? BillingTemplate { get; set; }
        public int? AgreementTemplate { get; set; }

        public string AccountsReceivableAccountNr { get; set; }
        public string AccountsReceivableAccountInternal1 { get; set; }
        public string AccountsReceivableAccountInternal2 { get; set; }
        public string AccountsReceivableAccountInternal3 { get; set; }
        public string AccountsReceivableAccountInternal4 { get; set; }
        public string AccountsReceivableAccountInternal5 { get; set; }
        public string AccountsReceivableAccountSieDim1 { get; set; }
        public string AccountsReceivableAccountSieDim6 { get; set; }
        public string SalesAccountNr { get; set; }
        public string SalesAccountInternal1 { get; set; }
        public string SalesAccountInternal2 { get; set; }
        public string SalesAccountInternal3 { get; set; }
        public string SalesAccountInternal4 { get; set; }
        public string SalesAccountInternal5 { get; set; }
        public string SalesAccountSieDim1 { get; set; }
        public string SalesAccountSieDim6 { get; set; }
        public string VATAccountNr { get; set; }
        public string VATCodeNr { get; set; }
        public string CategoryCode1 { get; set; }
        public string CategoryCode2 { get; set; }
        public string CategoryCode3 { get; set; }
        public string CategoryCode4 { get; set; }
        public string CategoryCode5 { get; set; }
        public List<int> CategoryIds { get; set; }

        public int? VatType { get; set; }
        public string DeliveryCondition { get; set; }
        public string PaymentCondition { get; set; }
        public string DefaultPriceListType { get; set; }
        public int? DefaultPriceListTypeId { get; set; }
        public string Currency { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string CustomerNr { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public string GLN { get; set; }
        public string VatNr { get; set; }
        public string InvoiceReference { get; set; }
        public int? SupplierNr { get; set; }
        public bool? ManualAccounting { get; set; }
        public decimal? DiscountMerchandise { get; set; }
        public decimal? DiscountService { get; set; }
        public bool? DisableInvoiceFee { get; set; }
        public string Note { get; set; }
        public bool? ShowNote { get; set; }
        public string FinvoiceAddress { get; set; }
        public string FinvoiceOperator { get; set; }
        public bool? IsFinvoiceCustomer { get; set; }
        public string BlockNote { get; set; }
        public bool? BlockOrder { get; set; }
        public bool? BlockInvoice { get; set; }
        public int? CreditLimit { get; set; }
        public bool? IsCashCustomer { get; set; }
        public bool ImportInvoiceDetailed { get; set; }

        public string DistributionAddress { get; set; }
        public string DistributionCoAddress { get; set; }
        public string DistributionPostalCode { get; set; }
        public string DistributionPostalAddress { get; set; }
        public string DistributionCountry { get; set; }

        public string BillingAddress { get; set; }
        public string BillingCoAddress { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingPostalAddress { get; set; }
        public string BillingCountry { get; set; }

        public List<ContactAdressIODTO> BillingAddresses { get; set; }
        public string BoardHQAddress { get; set; }
        public string BoardHQCountry { get; set; }

        public string VisitingAddress { get; set; }
        public string VisitingCoAddress { get; set; }
        public string VisitingPostalCode { get; set; }
        public string VisitingPostalAddress { get; set; }
        public string VisitingCountry { get; set; }

        public string DeliveryAddress { get; set; }
        public string DeliveryCoAddress { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string DeliveryPostalAddress { get; set; }
        public string DeliveryCountry { get; set; }

        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string PhoneHome { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneJob { get; set; }
        public string Fax { get; set; }
        public string Webpage { get; set; }
        public int InvoiceDeliveryType { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string InvoiceDeliveryEmail { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public string ErrorMessage { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }

        public List<string> ExternalNbrs { get; set; }
        public List<ContactEComIODTO> GLNNbrs { get; set; }
        public bool? IsPrivatePerson { get; set; }
        public string InvoiceLabel { get; set; }

        public List<CustomerProductPriceIODTO> ProductPrices { get; set; }

        // Extensions
        public string StatusName { get; set; }
        public string VatTypeName { get; set; }
        public int? SysLanguageId { get; set; }

        // Flags
        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
    }

    [TSInclude]
    public class CustomerSmallDTO
    {
        public int ActorCustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
    }

    [TSInclude]
    public class CustomerGridDTO
    {
        public int ActorCustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public string Categories { get; set; }
        public int InvoiceDeliveryType { get; set; }
        public string InvoiceReference { get; set; }
        public int InvoicePaymentService { get; set; }
        public SoeEntityState State { get; set; }
        public List<ContactAddressItem> ContactAddresses { get; set; }
        public string GridAddressText { get; set; }
        public string GridPhoneText { get; set; }
        public string GridPaymentServiceText { get; set; }
        public string GridBillingAddressText { get; set; }
        public string GridDeliveryAddressText { get; set; }
        public string GridHomePhoneText { get; set; }
        public string GridMobilePhoneText { get; set; }
        public string GridWorkPhoneText { get; set; }
        public string GridEmailText { get; set; }
        public string InvoiceDeliveryTypeText { get; set; }
        // Extensions
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }

        public bool? IsPrivatePerson { get; set; }
    }

    public class CustomerDistributionDTO
    {
        public bool IsPrivatePerson { get; set; }
        public string Email { get; set; }
        public string ReminderEmail { get; set; }
        public string MobilePhone { get; set; }
        public string CountryCode { get; set; }
        public string DeliveryAddressName { get; set; }
        public string DeliveryAddressCO { get; set; }
        public string DeliveryAddressStreet { get; set; }
        public string DeliveryAddressPostalCode { get; set; }
        public string DeliveryAddressCity { get; set; }
        public string DeliveryCountry { get; set; }
        public string BillingAddressName { get; set; }
        public string BillingAddressCO { get; set; }
        public string BillingAddressStreet { get; set; }
        public string BillingAddressPostalCode { get; set; }
        public string BillingAddressCity { get; set; }
        public string BillingCountry { get; set; }
        public string VisitorAddressStreet { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        public DateTime LastUpdated() => Modified ?? Created;

    }
}
