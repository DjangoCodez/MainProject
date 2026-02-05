using System.Collections.Generic;
using System.Xml.Linq;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util
{
    public class ContactIOItem
    {
        #region XML Nodes

        public const string CHILD_TAG = "ContactIO";

        public const string XML_SUPPLIERNR_TAG = "SupplierNr";
        public const string XML_CUSTOMERNR_TAG = "CustomerNr";
        public const string XML_ONLYADDRESS_TAG = "OnlyAddress";
        public const string XML_NAME_TAG = "Name";
        public const string XML_FIRSTNAME_TAG = "FirstName";
        public const string XML_LASTNAME_TAG = "LastName";

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


        public const string XML_GracePeriodDays_TAG = "GracePeriodDays";
        public const string XML_DeliveryMethod_TAG = "DeliveryMethod";
        public const string XML_DefaultWholeseller_TAG = "DefaultWholeseller";
        public const string XML_ContactState_TAG = "ContactState";
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
        public const string XML_SalesAccountNr_TAG = "SalesAccountNr";
        public const string XML_SalesAccountInternal1_TAG = "SalesAccountInternal1";
        public const string XML_SalesAccountInternal2_TAG = "SalesAccountInternal2";
        public const string XML_SalesAccountInternal3_TAG = "SalesAccountInternal3";
        public const string XML_SalesAccountInternal4_TAG = "SalesAccountInternal4";
        public const string XML_SalesAccountInternal5_TAG = "SalesAccountInternal5";
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

        public List<ContactIODTO> Contacts = new List<ContactIODTO>();

        #endregion

        #region Constructors

        public ContactIOItem()
        {
        }

        public ContactIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public ContactIOItem(string content, TermGroup_IOImportHeadType headType)
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

            List<XElement> contactIOElements = XmlUtil.GetChildElements(xdoc, CHILD_TAG);

            foreach (var contactIOElement in contactIOElements)
            {
                ContactIODTO contactIOData = new ContactIODTO();

                #region Extract Contact Data

                contactIOData.CustomerNr = XmlUtil.GetChildElementValue(contactIOElement, XML_CUSTOMERNR_TAG);
                contactIOData.SupplierNr = XmlUtil.GetChildElementValue(contactIOElement, XML_SUPPLIERNR_TAG);
                contactIOData.OnlyAddress  = XmlUtil.GetElementBoolValue(contactIOElement, XML_ONLYADDRESS_TAG);
                contactIOData.Name = XmlUtil.GetChildElementValue(contactIOElement, XML_NAME_TAG);
                contactIOData.FirstName = XmlUtil.GetChildElementValue(contactIOElement, XML_FIRSTNAME_TAG);
                contactIOData.LastName = XmlUtil.GetChildElementValue(contactIOElement, XML_LASTNAME_TAG);
                contactIOData.DistributionAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_DISTRIBUTION_ADDRESS_TAG);
                contactIOData.DistributionCoAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_DISTRIBUTION_CO_ADDRESS_TAG);
                contactIOData.DistributionPostalCode = XmlUtil.GetChildElementValue(contactIOElement, XML_DISTRIBUTION_POSTAL_CODE_TAG);
                contactIOData.DistributionPostalAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_DISTRIBUTION_POSTAL_ADDRESS_TAG);
                contactIOData.DistributionCountry = XmlUtil.GetChildElementValue(contactIOElement, XML_DISTRIBUTION_COUNTRY_TAG);
                contactIOData.BillingAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_BILLING_ADDRESS_TAG);
                contactIOData.BillingCoAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_BILLING_CO_ADDRESS_TAG);
                contactIOData.BillingPostalCode = XmlUtil.GetChildElementValue(contactIOElement, XML_BILLING_POSTAL_CODE_TAG);
                contactIOData.BillingPostalAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_BILLING_POSTAL_ADDRESS_TAG);
                contactIOData.BillingCountry = XmlUtil.GetChildElementValue(contactIOElement, XML_BILLING_COUNTRY_TAG);
                contactIOData.BoardHQAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_BOARD_HQ_ADDRESS_TAG);
                contactIOData.BoardHQCountry = XmlUtil.GetChildElementValue(contactIOElement, XML_BOARD_HQ_COUNTRY_TAG);
                contactIOData.VisitingAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_VISTING_ADDRESS_TAG);
                contactIOData.VisitingCoAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_VISITING_CO_ADDRESS_TAG);
                contactIOData.VisitingPostalCode = XmlUtil.GetChildElementValue(contactIOElement, XML_VISITING_POSTAL_CODE_TAG);
                contactIOData.VisitingPostalAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_VISITING_POSTAL_ADDRESS_TAG);
                contactIOData.VisitingCountry = XmlUtil.GetChildElementValue(contactIOElement, XML_VISITING_COUNTRY_TAG);
                contactIOData.DeliveryAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_DELIVERY_ADDRESS_TAG);
                contactIOData.DeliveryCoAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_DELIVERY_CO_ADDRESS_TAG);
                contactIOData.DeliveryPostalCode = XmlUtil.GetChildElementValue(contactIOElement, XML_DELIVERY_POSTAL_CODE_TAG);
                contactIOData.DeliveryPostalAddress = XmlUtil.GetChildElementValue(contactIOElement, XML_DELIVERY_POSTAL_ADDRESS_TAG);
                contactIOData.DeliveryCountry = XmlUtil.GetChildElementValue(contactIOElement, XML_DELIVERY_COUNTRY_TAG);
                contactIOData.Email1 = XmlUtil.GetChildElementValue(contactIOElement, XML_EMAIL1_TAG);
                contactIOData.Email2 = XmlUtil.GetChildElementValue(contactIOElement, XML_EMAIL2_TAG);
                contactIOData.PhoneHome = XmlUtil.GetChildElementValue(contactIOElement, XML_PHONE_HOME_TAG);
                contactIOData.PhoneMobile = XmlUtil.GetChildElementValue(contactIOElement, XML_PHONE_MOBILE_TAG);
                contactIOData.PhoneJob = XmlUtil.GetChildElementValue(contactIOElement, XML_PHONE_JOB_TAG);
                contactIOData.Fax = XmlUtil.GetChildElementValue(contactIOElement, XML_FAX_TAG);
                contactIOData.Webpage = XmlUtil.GetChildElementValue(contactIOElement, XML_WEBPAGE_TAG);

                #endregion

                Contacts.Add(contactIOData);
            }
        }

        #endregion
    }

}
