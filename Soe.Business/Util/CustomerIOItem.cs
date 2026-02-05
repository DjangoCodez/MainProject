using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class CustomerIOItem
    {
        #region XML Nodes

        public const string CHILD_TAG = "CustomerIO";

        public const string XML_VatType_TAG = "VatType";
        public const string XML_DeliveryCondition_TAG = "DeliveryCondition";
        public const string XML_DeliveryType_TAG = "DeliveryType";
        public const string XML_PaymentCondition_TAG = "PaymentCondition";
        public const string XML_PriceListType_TAG = "DefaultPriceListType";
        public const string XML_Currency_TAG = "Currency";
        public const string XML_SysCountry_TAG = "Country";
        public const string XML_CustomerNr_TAG = "CustomerNr";
        public const string XML_Name_TAG = "Name";
        public const string XML_OrgNr_TAG = "OrgNr";
        public const string XML_VatNr_TAG = "VatNr";
        public const string XML_InvoiceReference_TAG = "InvoiceReference";
        public const string XML_PaymentMorale_TAG = "PaymentMorale";
        public const string XML_SupplierNr_TAG = "SupplierNr";
        public const string XML_ManualAccounting_TAG = "ManualAccounting";
        public const string XML_DiscountMerchandise_TAG = "DiscountMerchandise";
        public const string XML_DiscountService_TAG = "DiscountService";
        public const string XML_DisableInvoiceFee_TAG = "DisableInvoiceFee";
        public const string XML_Note_TAG = "Note";
	    public const string XML_ShowNote_TAG = "ShowNote";
        public const string XML_FinvoiceAddress_TAG = "FinvoiceAddress";
        public const string XML_FinvoiceOperator_TAG = "FinvoiceOperator";
        public const string XML_IsFinvoiceCustomer_TAG = "IsFinvoiceCustomer";
        public const string XML_BlockNote_TAG = "BlockNote";
        public const string XML_BlockOrder_TAG = "BlockOrder";
        public const string XML_BlockInvoice_TAG = "BlockInvoice";
        public const string XML_CreditLimit_TAG = "CreditLimit";
        public const string XML_IsCashCustomer_TAG = "IsCashCustomer";
        public const string XML_GLN_TAG = "GLN";
        public const string XML_InvoiceLabel_TAG = "InvoiceLabel";


        //Addresses and telecom
        public const string XML_DISTRIBUTION_ADDRESS_TAG = "DistributionAddress";
        public const string XML_DISTRIBUTION_CO_ADDRESS_TAG = "DistributionCoAddress";
        public const string XML_DISTRIBUTION_POSTAL_CODE_TAG = "DistributionPostalCode";
        public const string XML_DISTRIBUTION_POSTAL_ADDRESS_TAG = "DistributionPostalAddress";
        public const string XML_DISTRIBUTION_COUNTRY_TAG = "DistributionCountry";
        public const string XML_BILLING_ADDRESS_TAG = "BillingAddress";
        public const string XML_BILLING_CO_ADDRESS_TAG = "BillingCoAddress";
        public const string XML_BILLING_POSTAL_CODE_TAG = "BillingPostalCode";
        public const string XML_BILLING_POSTAL_ADDRESS_TAG = "BillingPostalAddress";
        public const string XML_BILLING_COUNTRY_TAG = "BillingCountry";
        public const string XML_BOARD_HQ_ADDRESS_TAG = "BoardHQAddress";
        public const string XML_BOARD_HQ_COUNTRY_TAG = "BoardHQCountry";
        public const string XML_VISTING_ADDRESS_TAG = "VisitingAddress";
        public const string XML_VISITING_CO_ADDRESS_TAG = "VisitingCoAddress";
        public const string XML_VISITING_POSTAL_CODE_TAG = "VisitingPostalCode";
        public const string XML_VISITING_POSTAL_ADDRESS_TAG = "VisitingPostalAddress";
        public const string XML_VISITING_COUNTRY_TAG = "VisitingCountry";
        public const string XML_DELIVERY_ADDRESS_TAG = "DeliveryAddress";
        public const string XML_DELIVERY_CO_ADDRESS_TAG = "DeliveryCoAddress";
        public const string XML_DELIVERY_POSTAL_CODE_TAG = "DeliveryPostalCode";
        public const string XML_DELIVERY_POSTAL_ADDRESS_TAG = "DeliveryPostalAddress";
        public const string XML_DELIVERY_COUNTRY_TAG = "DeliveryCountry";
        public const string XML_EMAIL1_TAG = "Email1";
        public const string XML_EMAIL2_TAG = "Email2";
        public const string XML_PHONE_HOME_TAG = "PhoneHome";
        public const string XML_PHONE_MOBILE_TAG = "PhoneMobile";
        public const string XML_PHONE_JOB_TAG = "PhoneJob";
        public const string XML_FAX_TAG = "Fax";
        public const string XML_WEBPAGE_TAG = "WebPage";
        public const string XML_INVOICEDELIVERYTYPE_TAG = "InvoiceDeliveryType";
        public const string XML_CONTACT_FIRSTNAME_TAG = "ContactFirstName";
        public const string XML_CONTACT_LASTNAME_TAG = "ContactLastName";
        public const string XML_CONTACT_EMAIL1_TAG = "ContactEmail1";
        public const string XML_CONTACT_EMAIL_TAG = "ContactEmail"; 
        public const string XML_INVOICE_DELIVERY_EMAIL_TAG = "InvoiceDeliveryEmail";
        public const string XML_INVOICE_IMPORT_INVOICE_DETAILED = "ImportInvoiceDetailed";


        public const string XML_GracePeriodDays_TAG = "GracePeriodDays";
        public const string XML_DeliveryMethod_TAG = "DeliveryMethod";
        public const string XML_DefaultWholeseller_TAG = "DefaultWholeseller";
        public const string XML_CustomerState_TAG = "CustomerState";
        public const string XML_OfferTemplate_TAG = "OfferTemplate";
        public const string XML_OrderTemplate_TAG = "OrderTemplate";
        public const string XML_BillingTemplate_TAG = "BillingTemplate";
        public const string XML_AgreementTemplate_TAG = "AgreementTemplate";

        public const string XML_AccountsReceivableAccountNr_TAG = "AccountsReceivableAccountNr";
        public const string XML_AccountsReceivableAccountInternal1_TAG = "AccountsReceivableAccountInternal1";
        public const string XML_AccountsReceivableAccountInternal2_TAG = "AccountsReceivableAccountInternal2";
        public const string XML_AccountsReceivableAccountInternal3_TAG = "AccountsReceivableAccountInternal3";
        public const string XML_AccountsReceivableAccountInternal4_TAG = "AccountsReceivableAccountInternal4";
        public const string XML_AccountsReceivableAccountInternal5_TAG = "AccountsReceivableAccountInternal5";
        public const string XML_AccountsReceivableAccountSieDim1_TAG = "AccountsReceivableAccountSieDim1";
        public const string XML_AccountsReceivableAccountSieDim6_TAG = "AccountsReceivableAccountSieDim6";
        public const string XML_SalesAccountNr_TAG = "SalesAccountNr";
        public const string XML_SalesAccountInternal1_TAG = "SalesAccountInternal1";
        public const string XML_SalesAccountInternal2_TAG = "SalesAccountInternal2";
        public const string XML_SalesAccountInternal3_TAG = "SalesAccountInternal3";
        public const string XML_SalesAccountInternal4_TAG = "SalesAccountInternal4";
        public const string XML_SalesAccountInternal5_TAG = "SalesAccountInternal5";
        public const string XML_SalesAccountSieDim1_TAG = "SalesAccountSieDim1";
        public const string XML_SalesAccountSieDim6_TAG = "SalesAccountSieDim6";
        public const string XML_VATAccountNr_TAG = "VATAccountNr";
        public const string XML_VATCodeNr_TAG = "VATCodeNr";
        public const string XML_CategoryCode1_TAG = "CategoryCode1";
        public const string XML_CategoryCode2_TAG = "CategoryCode2";
        public const string XML_CategoryCode3_TAG = "CategoryCode3";
        public const string XML_CategoryCode4_TAG = "CategoryCode4";
        public const string XML_CategoryCode5_TAG = "CategoryCode5";
        
        public const string XML_SysLanguageId = "SysLanguageId";
        #endregion

        #region Collections

        public List<CustomerIODTO> Customers = new List<CustomerIODTO>();

        #endregion

        #region Constructors

        public CustomerIOItem()
        {
        }

        public CustomerIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public CustomerIOItem(string content, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(content, headType);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> customerIOElements = XmlUtil.GetChildElements(xdoc, CHILD_TAG);
           
            foreach (var customerIOElement in customerIOElements)
            {
                CustomerIODTO customerIOData = new CustomerIODTO();

                #region Extract Customer Data

                customerIOData.VatType = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_VatType_TAG);
                customerIOData.DeliveryCondition = XmlUtil.GetChildElementValue(customerIOElement, XML_DeliveryCondition_TAG);                
                customerIOData.PaymentCondition = XmlUtil.GetChildElementValue(customerIOElement, XML_PaymentCondition_TAG);
                customerIOData.DefaultPriceListType = XmlUtil.GetChildElementValue(customerIOElement, XML_PriceListType_TAG);
                customerIOData.Currency = XmlUtil.GetChildElementValue(customerIOElement, XML_Currency_TAG);
                customerIOData.Country = XmlUtil.GetChildElementValue(customerIOElement, XML_SysCountry_TAG);	
	            customerIOData.CustomerNr = XmlUtil.GetChildElementValue(customerIOElement, XML_CustomerNr_TAG);
	            customerIOData.Name = XmlUtil.GetChildElementValue(customerIOElement, XML_Name_TAG);
	            customerIOData.OrgNr = XmlUtil.GetChildElementValue(customerIOElement, XML_OrgNr_TAG);
	            customerIOData.VatNr = XmlUtil.GetChildElementValue(customerIOElement, XML_VatNr_TAG);
	            customerIOData.InvoiceReference = XmlUtil.GetChildElementValue(customerIOElement, XML_InvoiceReference_TAG);                
                customerIOData.SupplierNr = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_SupplierNr_TAG); 
	            customerIOData.ManualAccounting = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_ManualAccounting_TAG);	
	            customerIOData.DiscountMerchandise = XmlUtil.GetElementNullableDecimalValue(customerIOElement, XML_DiscountMerchandise_TAG);
                customerIOData.DiscountService = XmlUtil.GetElementNullableDecimalValue(customerIOElement, XML_DiscountService_TAG);
                customerIOData.DisableInvoiceFee = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_DisableInvoiceFee_TAG);
	            customerIOData.Note = XmlUtil.GetChildElementValue(customerIOElement, XML_Note_TAG);
                customerIOData.ShowNote = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_ShowNote_TAG);
	            customerIOData.FinvoiceAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_FinvoiceAddress_TAG);
	            customerIOData.FinvoiceOperator = XmlUtil.GetChildElementValue(customerIOElement, XML_FinvoiceOperator_TAG);
                customerIOData.IsFinvoiceCustomer = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_IsFinvoiceCustomer_TAG);
	            customerIOData.BlockNote = XmlUtil.GetChildElementValue(customerIOElement, XML_BlockNote_TAG);
                customerIOData.BlockOrder = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_BlockOrder_TAG);
                customerIOData.BlockInvoice = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_BlockInvoice_TAG);
                customerIOData.CreditLimit = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_CreditLimit_TAG);
                customerIOData.IsCashCustomer = XmlUtil.GetElementNullableBoolValue(customerIOElement, XML_IsCashCustomer_TAG);
                customerIOData.GLN = XmlUtil.GetChildElementValue(customerIOElement, XML_GLN_TAG);
                customerIOData.InvoiceLabel = XmlUtil.GetChildElementValue(customerIOElement, XML_InvoiceLabel_TAG);

                customerIOData.DistributionAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_DISTRIBUTION_ADDRESS_TAG);
                customerIOData.DistributionCoAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_DISTRIBUTION_CO_ADDRESS_TAG);
                customerIOData.DistributionPostalCode = XmlUtil.GetChildElementValue(customerIOElement, XML_DISTRIBUTION_POSTAL_CODE_TAG);
                customerIOData.DistributionPostalAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_DISTRIBUTION_POSTAL_ADDRESS_TAG);
                customerIOData.DistributionCountry = XmlUtil.GetChildElementValue(customerIOElement, XML_DISTRIBUTION_COUNTRY_TAG);
                customerIOData.BillingAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_BILLING_ADDRESS_TAG);
                customerIOData.BillingCoAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_BILLING_CO_ADDRESS_TAG);
                customerIOData.BillingPostalCode = XmlUtil.GetChildElementValue(customerIOElement, XML_BILLING_POSTAL_CODE_TAG);
                customerIOData.BillingPostalAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_BILLING_POSTAL_ADDRESS_TAG);
                customerIOData.BillingCountry = XmlUtil.GetChildElementValue(customerIOElement, XML_BILLING_COUNTRY_TAG);
                customerIOData.BoardHQAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_BOARD_HQ_ADDRESS_TAG);
                customerIOData.BoardHQCountry = XmlUtil.GetChildElementValue(customerIOElement, XML_BOARD_HQ_COUNTRY_TAG);
                customerIOData.VisitingAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_VISTING_ADDRESS_TAG);
                customerIOData.VisitingCoAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_VISITING_CO_ADDRESS_TAG);
                customerIOData.VisitingPostalCode = XmlUtil.GetChildElementValue(customerIOElement, XML_VISITING_POSTAL_CODE_TAG);
                customerIOData.VisitingPostalAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_VISITING_POSTAL_ADDRESS_TAG);
                customerIOData.VisitingCountry = XmlUtil.GetChildElementValue(customerIOElement, XML_VISITING_COUNTRY_TAG);
                customerIOData.DeliveryAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_DELIVERY_ADDRESS_TAG);
                customerIOData.DeliveryCoAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_DELIVERY_CO_ADDRESS_TAG);
                customerIOData.DeliveryPostalCode = XmlUtil.GetChildElementValue(customerIOElement, XML_DELIVERY_POSTAL_CODE_TAG);
                customerIOData.DeliveryPostalAddress = XmlUtil.GetChildElementValue(customerIOElement, XML_DELIVERY_POSTAL_ADDRESS_TAG);
                customerIOData.DeliveryCountry = XmlUtil.GetChildElementValue(customerIOElement, XML_DELIVERY_COUNTRY_TAG);
                customerIOData.Email1 = XmlUtil.GetChildElementValue(customerIOElement, XML_EMAIL1_TAG);
                customerIOData.Email2 = XmlUtil.GetChildElementValue(customerIOElement, XML_EMAIL2_TAG);
                customerIOData.PhoneHome = XmlUtil.GetChildElementValue(customerIOElement, XML_PHONE_HOME_TAG);
                customerIOData.PhoneMobile = XmlUtil.GetChildElementValue(customerIOElement, XML_PHONE_MOBILE_TAG);
                customerIOData.PhoneJob = XmlUtil.GetChildElementValue(customerIOElement, XML_PHONE_JOB_TAG);
                customerIOData.Fax = XmlUtil.GetChildElementValue(customerIOElement, XML_FAX_TAG);
                customerIOData.Webpage = XmlUtil.GetChildElementValue(customerIOElement, XML_WEBPAGE_TAG);
                customerIOData.InvoiceDeliveryType = XmlUtil.GetElementIntValue(customerIOElement, XML_INVOICEDELIVERYTYPE_TAG);
                customerIOData.ContactFirstName = XmlUtil.GetChildElementValue(customerIOElement, XML_CONTACT_FIRSTNAME_TAG);
                customerIOData.ContactLastName = XmlUtil.GetChildElementValue(customerIOElement, XML_CONTACT_LASTNAME_TAG);
                customerIOData.ContactEmail = XmlUtil.GetChildElementValue(customerIOElement, XML_CONTACT_EMAIL1_TAG);
                customerIOData.InvoiceDeliveryEmail = XmlUtil.GetChildElementValue(customerIOElement, XML_INVOICE_DELIVERY_EMAIL_TAG); 
                customerIOData.ImportInvoiceDetailed = XmlUtil.GetElementBoolValue(customerIOElement, XML_INVOICE_IMPORT_INVOICE_DETAILED);


                customerIOData.GracePeriodDays = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_GracePeriodDays_TAG);
                customerIOData.DeliveryMethod  = XmlUtil.GetChildElementValue(customerIOElement, XML_DeliveryMethod_TAG);
                customerIOData.DefaultWholeseller  = XmlUtil.GetChildElementValue(customerIOElement, XML_DefaultWholeseller_TAG);
                customerIOData.CustomerState = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_CustomerState_TAG);
                customerIOData.OfferTemplate  = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_OfferTemplate_TAG);
	            customerIOData.OrderTemplate  = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_OrderTemplate_TAG);
	            customerIOData.BillingTemplate  = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_BillingTemplate_TAG);
	            customerIOData.AgreementTemplate  = XmlUtil.GetElementNullableIntValue(customerIOElement, XML_AgreementTemplate_TAG);

                customerIOData.AccountsReceivableAccountNr  = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountNr_TAG);
                customerIOData.AccountsReceivableAccountInternal1  = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountInternal1_TAG);
                customerIOData.AccountsReceivableAccountInternal2  = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountInternal2_TAG);
                customerIOData.AccountsReceivableAccountInternal3  = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountInternal3_TAG);
                customerIOData.AccountsReceivableAccountInternal4  = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountInternal4_TAG);
                customerIOData.AccountsReceivableAccountInternal5  = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountInternal5_TAG);
                customerIOData.AccountsReceivableAccountSieDim1 = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountSieDim1_TAG);
                customerIOData.AccountsReceivableAccountSieDim6 = XmlUtil.GetChildElementValue(customerIOElement, XML_AccountsReceivableAccountSieDim6_TAG);
                customerIOData.SalesAccountNr = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountNr_TAG);
                customerIOData.SalesAccountInternal1  = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountInternal1_TAG);
                customerIOData.SalesAccountInternal2  = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountInternal2_TAG);
                customerIOData.SalesAccountInternal3  = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountInternal3_TAG);
                customerIOData.SalesAccountInternal4  = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountInternal4_TAG);
                customerIOData.SalesAccountInternal5  = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountInternal5_TAG);
                customerIOData.SalesAccountSieDim1 = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountSieDim1_TAG);
                customerIOData.SalesAccountSieDim6 = XmlUtil.GetChildElementValue(customerIOElement, XML_SalesAccountSieDim6_TAG);
                customerIOData.VATAccountNr = XmlUtil.GetChildElementValue(customerIOElement, XML_VATAccountNr_TAG);
                customerIOData.VATCodeNr = XmlUtil.GetChildElementValue(customerIOElement, XML_VATCodeNr_TAG);
                customerIOData.CategoryCode1 = XmlUtil.GetChildElementValue(customerIOElement, XML_CategoryCode1_TAG);
                customerIOData.CategoryCode2 = XmlUtil.GetChildElementValue(customerIOElement, XML_CategoryCode2_TAG);
                customerIOData.CategoryCode3 = XmlUtil.GetChildElementValue(customerIOElement, XML_CategoryCode3_TAG);
                customerIOData.CategoryCode4 = XmlUtil.GetChildElementValue(customerIOElement, XML_CategoryCode4_TAG);
                customerIOData.CategoryCode5 = XmlUtil.GetChildElementValue(customerIOElement, XML_CategoryCode5_TAG);
                customerIOData.SysLanguageId = XmlUtil.GetElementIntValue(customerIOElement, XML_SysLanguageId);
                

                #endregion

                Customers.Add(customerIOData);
            }
        }

        #endregion
    }
   
}
