using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class SupplierIOItem
    {
        #region XML Nodes

        public const string CHILD_TAG = "SupplierIO";

        public const string XML_SUPPLIER_NR_TAG = "SupplierNr";
        public const string XML_NAME_TAG = "Name";
        public const string XML_ORG_NR_TAG = "OrgNr";
        public const string XML_VAT_NR_TAG = "VatNr";
        public const string XML_VATTYPE_TAG = "VatType";
        public const string XML_RIKSBANKSCODE_TAG = "RiksbanksCode";
        public const string XML_OURCUSTOMER_NR_TAG = "OurCustomerNr";
        public const string XML_FACTORINGSUPPLIER_NR_TAG = "FactoringSupplierNr";
        public const string XML_SYSCOUNTRY_TAG = "SysCountry";
        public const string XML_CURRENCY_TAG = "Currency";
        public const string XML_STANDARD_PAYMENT_TYPE_TAG = "StandardPaymentType";
        public const string XML_BANKGIRO_NR_TAG = "BankGiroNr";
        public const string XML_PLUSGIRO_NR_TAG = "PlusGiroNr";
        public const string XML_BANK_NR_TAG = "BankNr";
        public const string XML_BIC_TAG = "BIC";
        public const string XML_IBAN_TAG = "IBAN";
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
        public const string XML_WEBPAGE_TAG = "Webpage";
        public const string XML_PAYMENTCONDITION_CODE_TAG = "PaymentConditionCode";
        public const string XML_DELIVERYTYPE_CODE_TAG = "DeliveryTypeCode";
        public const string XML_DELIVERYCONDITION_CODE_TAG = "DeliveryConditionCode";
        public const string XML_COPY_INVOICE_NR_TO_OCR_TAG = "CopyInvoiceNrToOcr";
        public const string XML_BLOCK_PAYMENT_TAG = "BlockPayment";
        public const string XML_MANUAL_ACCOUNTING_TAG = "ManualAccounting";
        public const string XML_ACCOUNTS_PAYABLE_ACCOUNT_NR_TAG = "AccountsPayableAccountNr";
        public const string XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL1_TAG = "AccountsPayableAccountInternal1";
        public const string XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL2_TAG = "AccountsPayableAccountInternal2";
        public const string XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL3_TAG = "AccountsPayableAccountInternal3";
        public const string XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL4_TAG = "AccountsPayableAccountInternal4";
        public const string XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL5_TAG = "AccountsPayableAccountInternal5";
        public const string XML_PURCHASE_ACCOUNT_NR_TAG = "PurchaseAccountNr";
        public const string XML_PURCHASE_ACCOUNT_INTERNAL1_TAG = "PurchaseAccountInternal1";
        public const string XML_PURCHASE_ACCOUNT_INTERNAL2_TAG = "PurchaseAccountInternal2";
        public const string XML_PURCHASE_ACCOUNT_INTERNAL3_TAG = "PurchaseAccountInternal3";
        public const string XML_PURCHASE_ACCOUNT_INTERNAL4_TAG = "PurchaseAccountInternal4";
        public const string XML_PURCHASE_ACCOUNT_INTERNAL5_TAG = "PurchaseAccountInternal5";
        public const string XML_VAT_ACCOUNT_NR_TAG = "VATAccountNr";
        public const string XML_VAT_CODE_NR_TAG = "VATCodeNr";
        public const string XML_INTRASTAT_CODE = "IntrastatCode";

        public string XML_SupplierNrPart1_TAG = "SupplierNrPart1";
        public string XML_SupplierNrPart2_TAG = "SupplierNrPart2";
        public string XML_SupplierNrPart3_TAG = "SupplierNrPart3";
        public string XML_SupplierNrPart4_TAG = "SupplierNrPart4";
        public string XML_AccountsPayableAccountSieDim1_TAG = "AccountsPayableAccountSieDim1";
        public string XML_AccountsPayableAccountSieDim6_TAG = "AccountsPayableAccountSieDim6";
        public string XML_PurchaseAccountSieDim1_TAG = "PurchaseAccountSieDim1";
        public string XML_PurchaseAccountSieDim6_TAG = "PurchaseAccountSieDim6";
        #endregion

        #region Collections

        public List<SupplierIODTO> Suppliers = new List<SupplierIODTO>();

        #endregion

        #region Constructors

        public SupplierIOItem()
        {
        }

        public SupplierIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public SupplierIOItem(string content, TermGroup_IOImportHeadType headType)
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

            List<XElement> supplierIOElements = XmlUtil.GetChildElements(xdoc, CHILD_TAG);

            foreach (var supplierIOElement in supplierIOElements)
            {
                SupplierIODTO supplierIOData = new SupplierIODTO();

                #region Extract Supplier Data

                supplierIOData.SupplierNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_SUPPLIER_NR_TAG);
                supplierIOData.Name = XmlUtil.GetChildElementValue(supplierIOElement, XML_NAME_TAG);
                supplierIOData.OrgNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_ORG_NR_TAG);
                supplierIOData.VatNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_VAT_NR_TAG);
                supplierIOData.VatType = XmlUtil.GetElementNullableIntValue(supplierIOElement, XML_VATTYPE_TAG);
                supplierIOData.RiksbanksCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_RIKSBANKSCODE_TAG);
                supplierIOData.OurCustomerNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_OURCUSTOMER_NR_TAG);
                supplierIOData.FactoringSupplierNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_FACTORINGSUPPLIER_NR_TAG);
                supplierIOData.SysCountry = XmlUtil.GetChildElementValue(supplierIOElement, XML_SYSCOUNTRY_TAG);
                supplierIOData.Currency = XmlUtil.GetChildElementValue(supplierIOElement, XML_CURRENCY_TAG);
                supplierIOData.StandardPaymentType = XmlUtil.GetElementNullableIntValue(supplierIOElement, XML_STANDARD_PAYMENT_TYPE_TAG);
                supplierIOData.BankGiroNr = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_BANKGIRO_NR_TAG);
                supplierIOData.PlusGiroNr = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_PLUSGIRO_NR_TAG);
                supplierIOData.BankNr = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_BANK_NR_TAG);
                supplierIOData.BIC = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_BIC_TAG).Replace(" ", "");
                supplierIOData.IBAN = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_IBAN_TAG).Replace(" ","");
                supplierIOData.DistributionAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_DISTRIBUTION_ADDRESS_TAG);
                supplierIOData.DistributionCoAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_DISTRIBUTION_CO_ADDRESS_TAG);
                supplierIOData.DistributionPostalCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_DISTRIBUTION_POSTAL_CODE_TAG);
                supplierIOData.DistributionPostalAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_DISTRIBUTION_POSTAL_ADDRESS_TAG);
                supplierIOData.DistributionCountry = XmlUtil.GetChildElementValue(supplierIOElement, XML_DISTRIBUTION_COUNTRY_TAG);
                supplierIOData.BillingAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_BILLING_ADDRESS_TAG);
                supplierIOData.BillingCoAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_BILLING_CO_ADDRESS_TAG);
                supplierIOData.BillingPostalCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_BILLING_POSTAL_CODE_TAG);
                supplierIOData.BillingPostalAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_BILLING_POSTAL_ADDRESS_TAG);
                supplierIOData.BillingCountry = XmlUtil.GetChildElementValue(supplierIOElement, XML_BILLING_COUNTRY_TAG);
                supplierIOData.BoardHQAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_BOARD_HQ_ADDRESS_TAG);
                supplierIOData.BoardHQCountry = XmlUtil.GetChildElementValue(supplierIOElement, XML_BOARD_HQ_COUNTRY_TAG);
                supplierIOData.VisitingAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_VISTING_ADDRESS_TAG);
                supplierIOData.VisitingCoAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_VISITING_CO_ADDRESS_TAG);
                supplierIOData.VisitingPostalCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_VISITING_POSTAL_CODE_TAG);
                supplierIOData.VisitingPostalAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_VISITING_POSTAL_ADDRESS_TAG);
                supplierIOData.VisitingCountry = XmlUtil.GetChildElementValue(supplierIOElement, XML_VISITING_COUNTRY_TAG);
                supplierIOData.DeliveryAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERY_ADDRESS_TAG);
                supplierIOData.DeliveryCoAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERY_CO_ADDRESS_TAG);
                supplierIOData.DeliveryPostalCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERY_POSTAL_CODE_TAG);
                supplierIOData.DeliveryPostalAddress = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERY_POSTAL_ADDRESS_TAG);
                supplierIOData.DeliveryCountry = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERY_COUNTRY_TAG);
                supplierIOData.Email1 = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_EMAIL1_TAG);
                supplierIOData.Email2 = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_EMAIL2_TAG);
                supplierIOData.PhoneHome = XmlUtil.GetChildElementValue(supplierIOElement, XML_PHONE_HOME_TAG);
                supplierIOData.PhoneMobile = XmlUtil.GetChildElementValue(supplierIOElement, XML_PHONE_MOBILE_TAG);
                supplierIOData.PhoneJob = XmlUtil.GetChildElementValue(supplierIOElement, XML_PHONE_JOB_TAG);
                supplierIOData.Fax = XmlUtil.GetElementValueLowerCase(supplierIOElement, XML_FAX_TAG);
                supplierIOData.Webpage = XmlUtil.GetChildElementValue(supplierIOElement, XML_WEBPAGE_TAG);
                supplierIOData.PaymentConditionCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_PAYMENTCONDITION_CODE_TAG);
                supplierIOData.DeliveryTypeCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERYTYPE_CODE_TAG);
                supplierIOData.DeliveryConditionCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_DELIVERYCONDITION_CODE_TAG);
                supplierIOData.CopyInvoiceNrToOcr = XmlUtil.GetElementNullableBoolValue(supplierIOElement, XML_COPY_INVOICE_NR_TO_OCR_TAG);
                supplierIOData.BlockPayment = XmlUtil.GetElementNullableBoolValue(supplierIOElement, XML_BLOCK_PAYMENT_TAG);
                supplierIOData.ManualAccounting = XmlUtil.GetElementNullableBoolValue(supplierIOElement, XML_MANUAL_ACCOUNTING_TAG);
                supplierIOData.AccountsPayableAccountNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_ACCOUNTS_PAYABLE_ACCOUNT_NR_TAG);
                supplierIOData.AccountsPayableAccountInternal1 = XmlUtil.GetChildElementValue(supplierIOElement, XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL1_TAG);
                supplierIOData.AccountsPayableAccountInternal2 = XmlUtil.GetChildElementValue(supplierIOElement, XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL2_TAG);
                supplierIOData.AccountsPayableAccountInternal3 = XmlUtil.GetChildElementValue(supplierIOElement, XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL3_TAG);
                supplierIOData.AccountsPayableAccountInternal4 = XmlUtil.GetChildElementValue(supplierIOElement, XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL4_TAG);
                supplierIOData.AccountsPayableAccountInternal5 = XmlUtil.GetChildElementValue(supplierIOElement, XML_ACCOUNTS_PAYABLE_ACCOUNT_INTERNAL5_TAG);
                supplierIOData.PurchaseAccountNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_PURCHASE_ACCOUNT_NR_TAG);
                supplierIOData.PurchaseAccountInternal1 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PURCHASE_ACCOUNT_INTERNAL1_TAG);
                supplierIOData.PurchaseAccountInternal2 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PURCHASE_ACCOUNT_INTERNAL2_TAG);
                supplierIOData.PurchaseAccountInternal3 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PURCHASE_ACCOUNT_INTERNAL3_TAG);
                supplierIOData.PurchaseAccountInternal4 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PURCHASE_ACCOUNT_INTERNAL4_TAG);
                supplierIOData.PurchaseAccountInternal5 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PURCHASE_ACCOUNT_INTERNAL5_TAG);
                supplierIOData.VATAccountNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_VAT_ACCOUNT_NR_TAG);
                supplierIOData.VATCodeNr = XmlUtil.GetChildElementValue(supplierIOElement, XML_VAT_CODE_NR_TAG);
                supplierIOData.AccountsPayableAccountSieDim1 = XmlUtil.GetChildElementValue(supplierIOElement, XML_AccountsPayableAccountSieDim1_TAG);
                supplierIOData.AccountsPayableAccountSieDim6 = XmlUtil.GetChildElementValue(supplierIOElement, XML_AccountsPayableAccountSieDim6_TAG);
                supplierIOData.PurchaseAccountSieDim1 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PurchaseAccountSieDim1_TAG);
                supplierIOData.PurchaseAccountSieDim6 = XmlUtil.GetChildElementValue(supplierIOElement, XML_PurchaseAccountSieDim6_TAG);
                supplierIOData.IntrastatCode = XmlUtil.GetChildElementValue(supplierIOElement, XML_INTRASTAT_CODE);

                #endregion

                #region Fix

                if (string.IsNullOrEmpty(supplierIOData.StandardPaymentTypeName) && !string.IsNullOrEmpty(supplierIOData.BIC))
                    supplierIOData.StandardPaymentType = (int?)TermGroup_SysPaymentType.BIC;


                #endregion


                #region SupplierNr field parts

                string SupplierNrPart1 = XmlUtil.GetChildElementValue(supplierIOElement, XML_SupplierNrPart1_TAG);
                string SupplierNrPart2 = XmlUtil.GetChildElementValue(supplierIOElement, XML_SupplierNrPart2_TAG);
                string SupplierNrPart3 = XmlUtil.GetChildElementValue(supplierIOElement, XML_SupplierNrPart3_TAG);
                string SupplierNrPart4 = XmlUtil.GetChildElementValue(supplierIOElement, XML_SupplierNrPart4_TAG);

                if (string.IsNullOrEmpty(supplierIOData.SupplierNr))
                {
                    supplierIOData.SupplierNr = string.Empty;

                    if (!string.IsNullOrEmpty(SupplierNrPart1))
                        supplierIOData.SupplierNr += SupplierNrPart1;

                    if (!string.IsNullOrEmpty(SupplierNrPart2))
                        supplierIOData.SupplierNr += SupplierNrPart2;

                    if (!string.IsNullOrEmpty(SupplierNrPart3))
                        supplierIOData.SupplierNr += SupplierNrPart3;

                    if (!string.IsNullOrEmpty(SupplierNrPart4))
                        supplierIOData.SupplierNr += SupplierNrPart4;
                }

                #endregion


                Suppliers.Add(supplierIOData);
            }
        }

        #endregion
    }
}
