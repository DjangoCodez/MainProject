using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class CustomerInvoiceIOItem
    {

        #region Collections

        public List<CustomerInvoiceIODTO> CustomerInvoices = new List<CustomerInvoiceIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "CustomerInvoiceHeadIO";

        private string XML_InvoiceId_TAG = "InvoiceId";
        public string XML_RegistrationType_TAG = "RegistrationType";
        public string XML_CustomerInvoiceN_TAG = "CustomerInvoiceNr";
        public string XML_SeqN_TAG = "SeqNr";
        public string XML_OC_TAG = "OCR";
        public string XML_OriginType_TAG = "OriginType";
        public string XML_OriginStatus_TAG = "OriginStatus";
        public string XML_PaymentCondition_TAG = "PaymentCondition";
        public string XML_CustomerN_TAG = "CustomerNr";
        public string XML_CustomerName_TAG = "CustomerName";
        public string XML_PriceListTypeId_TAG = "PriceListTypeId";
        public string XML_InvoiceDate_TAG = "InvoiceDate";
        public string XML_DeliveryDate_TAG = "DeliveryDate";
        public string XML_DueDate_TAG = "DueDate";
        public string XML_VoucherDate_TAG = "VoucherDate";
        public string XML_ReferenceOur_TAG = "ReferenceOur";
        public string XML_ReferenceYour_TAG = "ReferenceYour";
        public string XML_CurrencyRate_TAG = "CurrencyRate";
        public string XML_CurrencyDate_TAG = "CurrencyDate";
        public string XML_SumAmount_TAG = "SumAmount";
        public string XML_SumAmountCurrency_TAG = "SumAmountCurrency";
        public string XML_TotalAmount_TAG = "TotalAmount";
        public string XML_TotalAmountCurrency_TAG = "TotalAmountCurrency";
        public string XML_VATAmount_TAG = "VATAmount";
        public string XML_VATAmountCurrency_TAG = "VATAmountCurrency";
        public string XML_PaidAmount_TAG = "PaidAmount";
        public string XML_PaidAmountCurrency_TAG = "PaidAmountCurrency";
        public string XML_RemainingAmount_TAG = "RemainingAmount";
        public string XML_FreightAmount_TAG = "FreightAmount";
        public string XML_FreightAmountCurrency_TAG = "FreightAmountCurrency";
        public string XML_InvoiceFee_TAG = "InvoiceFee";
        public string XML_InvoiceFeeCurrency_TAG = "InvoiceFeeCurrency";
        public string XML_CentRounding_TAG = "CentRounding";
        public string XML_FullyPayed_TAG = "FullyPayed";
        public string XML_PaymentN_TAG = "PaymentNr";
        public string XML_VoucherN_TAG = "VoucherNr";
        public string XML_CreateAccountingInXE_TAG = "CreateAccountingInXE";
        public string XML_Note_TAG = "Note";
        public string XML_BillingType_TAG = "BillingType";
        public string XML_TransferType_TAG = "TransferType";
        public string XML_CustomerInvoiceHeadIO_TAG = "CustomerInvoiceHeadIO";
        public string XML_BillingAddressAddress_TAG = "BillingAddressAddress";
        public string XML_BillingAddressCO_TAG = "BillingAddressCO";
        public string XML_BillingAddressPostN_TAG = "BillingAddressPostNr";
        public string XML_BillingAddressCity_TAG = "BillingAddressCity";
        public string XML_DeliveryAddressAddress_TAG = "DeliveryAddressAddress";
        public string XML_DeliveryAddressPostN_TAG = "DeliveryAddressPostNr";
        public string XML_DeliveryAddressCity_TAG = "DeliveryAddressCity";
        public string XML_UseFixedPriceFromHead_TAG = "UseFixedPriceFromHead";
        public string XML_VatRate1_TAG = "VatRate1";
        public string XML_VatRate2_TAG = "VatRate2";
        public string XML_VatRate3_TAG = "VatRate3";
        public string XML_VatAmount1_TAG = "VatAmount1";
        public string XML_VatAmount2_TAG = "VatAmount2";
        public string XML_VatAmount3_TAG = "VatAmount3";
        private string XML_Currency_TAG = "Currency";

        public string XML_BillingAddressCountry_TAG = "BillingAddressCountry";
        public string XML_DeliveryAddressCO_TAG = "DeliveryAddressCO";
        public string XML_DeliveryAddressCountry_TAG = "DeliveryAddressCountry";
        public string XML_InvoiceState_TAG = "InvoiceState";
        public string XML_VatType_TAG = "VatType";
        public string XML_BillingAddressName_TAG = "BillingAddressName";
        public string XML_DeliveryAddressName_TAG = "DeliveryAddressName";
        public string XML_Language_TAG = "Language";
        public string XML_PaymentConditionCode_TAG = "PaymentConditionCode";
        public string XML_InvoiceDeliveryType_TAG = "InvoiceDeliveryType";
        public string XML_SaleAccountN_TAG = "SaleAccountNr";
        public string XML_SaleAccountNrDim2_TAG = "SaleAccountNrDim2";
        public string XML_SaleAccountNrDim3_TAG = "SaleAccountNrDim3";
        public string XML_SaleAccountNrDim4_TAG = "SaleAccountNrDim4";
        public string XML_SaleAccountNrDim5_TAG = "SaleAccountNrDim5";
        public string XML_SaleAccountNrDim6_TAG = "SaleAccountNrDim6";
        public string XML_SaleAccountNrSieDim1_TAG = "SaleAccountNrSieDim1";
        public string XML_SaleAccountNrSieDim6_TAG = "SaleAccountNrSieDim6";
        public string XML_ClaimAccountN_TAG = "ClaimAccountNr";
        public string XML_ClaimAccountNrDim2_TAG = "ClaimAccountNrDim2";
        public string XML_ClaimAccountNrDim3_TAG = "ClaimAccountNrDim3";
        public string XML_ClaimAccountNrDim4_TAG = "ClaimAccountNrDim4";
        public string XML_ClaimAccountNrDim5_TAG = "ClaimAccountNrDim5";
        public string XML_ClaimAccountNrDim6_TAG = "ClaimAccountNrDim6";
        public string XML_ClaimAccountNrSieDim1_TAG = "ClaimAccountNrSieDim1";
        public string XML_ClaimAccountNrSieDim6_TAG = "ClaimAccountNrSieDim6";
        public string XML_VatAccountN_TAG = "VatAccountNr";
        public string XML_OrderN_TAG = "OrderNr";
        public string XML_OfferN_TAG = "OfferNr";
        public string XML_ContractN_TAG = "ContractNr";
        public string XML_WorkingDescription_TAG = "WorkingDescription";
        public string XML_InternalDescription_TAG = "InternalDescription";
        public string XML_ExternalDescription_TAG = "ExternalDescription";
        public string XML_ProjectN_TAG = "ProjectNr";

        public string XML_NextContractPeriodYear_TAG = "NextContractPeriodYear";
        public string XML_NextContractPeriodValue_TAG = "NextContractPeriodValue";
        public string XML_NextContractPeriodDate_TAG = "NextContractPeriodDate";
        public string XML_ContractGroupId_TAG = "ContractGroupId";
        public string XML_ContractGroupInterval_TAG = "ContractGroupInterval";
        public string XML_ContractGroupDayInMonth_TAG = "ContractGroupDayInMonth";
        public string XML_ContractGroupOrderTemplate_TAG = "ContractGroupOrderTemplate";
        public string XML_ContractGroupInvoiceTemplate_TAG = "ContractGroupInvoiceTemplate";
        public string XML_ContractGroupDecription_TAG = "ContractGroupDecription";
        public string XML_ContractGroupName_TAG = "ContractGroupName";
        public string XML_ContractGroupPeriod_TAG = "ContractGroupPeriod";
        public string XML_ContractGroupPriceManagementName_TAG = "ContractGroupPriceManagementName";
        public string XML_ContractGroupInvoiceText_TAG = "ContractGroupInvoiceText";
        public string XML_ContractGroupInvoiceRowText_TAG = "ContractGroupInvoiceRowText";

        public string XML_ContractEndYear_TAG = "ContractEndYear";
        public string XML_ContractEndDay_TAG = "ContractEndDay";
        public string XML_ContractEndMonth_TAG = "ContractEndMonth";

        public string XML_InvoiceLabel_TAG = "InvoiceLabel";
        public string XML_InvoiceHeadText_TAG = "InvoiceHeadText";
        public string XML_CreateDeliveryAddressAsTextOnly_TAG = "CreateDeliveryAddressAsTextOnly";
        public string XML_ExternalId_TAG = "ExternalId";
        public string XML_Email_TAG = "Email";

        public CustomerInvoiceIOItem() { }

        public CustomerInvoiceIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public CustomerInvoiceIOItem(string content, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(content, headType);
        }

        public void CreateSelectXML()
        {
            Type elementType = typeof(CustomerInvoiceIODTO);

            //<xs:schema id="NewDataSet" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
            //    <xs:element name="NewDataSet" msdata:IsDataSet="true" msdata:UseCurrentLocale="true">
            //        <xs:complexType>
            //            <xs:choice minOccurs="0" maxOccurs="unbounded">
            //                <xs:element name="Columns">
            //                    <xs:complexType>
            //                        <xs:sequence>
            //                            <xs:element name="Column" type="xs:string" minOccurs="0" />
            //                            <xs:element name="Text" type="xs:string" minOccurs="0" />
            //                            <xs:element name="DataType" type="xs:string" minOccurs="0" />
            //                            <xs:element name="Mandatory" type="xs:boolean" minOccurs="0" />
            //                            <xs:element name="Position" type="xs:int" minOccurs="0" />
            //                        </xs:sequence>
            //                    </xs:complexType>
            //                </xs:element>
            //            </xs:choice>
            //        </xs:complexType>
            //    </xs:element>
            //</xs:schema>

            string output = string.Empty;

            List<XElement> elements = new List<XElement>();
            XElement datasetelement = new XElement("DataSet");
            int position = 1;

            //add a column to table for each public property on T
            foreach (var propInfo in elementType.GetProperties())
            {
                string dataType = string.Empty;

                if (propInfo.PropertyType.FullName == "System.String")
                    dataType = "Sträng";
                else if (propInfo.PropertyType.FullName == "System.Int32")
                    dataType = "Heltal";
                else if (propInfo.PropertyType.FullName == "System.Decimal")
                    dataType = "Belopp";
                else if (propInfo.PropertyType.FullName == "System.Bool")
                    dataType = "Ja/Nej";

                XElement element = new XElement("Columns");

                element.Add(new XElement("Column", propInfo.Name));
                element.Add(new XElement("Text", propInfo.Name));
                element.Add(new XElement("DataType", dataType));
                element.Add(new XElement("Mandatory", "false"));
                element.Add(new XElement("Mandatory", position.ToString()));

                elements.Add(element);

                position++;
            }

            datasetelement.Add(elements);
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

            List<XElement> customerInvoiceIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            foreach (var customerInvoiceIOElement in customerInvoiceIOElements)
            {
                CustomerInvoiceIODTO customerInvoiceIODTO = new CustomerInvoiceIODTO();

                #region Extract CustomerInvoice Data

                customerInvoiceIODTO.InvoiceId = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_InvoiceId_TAG);
                customerInvoiceIODTO.RegistrationType = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_RegistrationType_TAG);
                customerInvoiceIODTO.CustomerInvoiceNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_CustomerInvoiceN_TAG);
                customerInvoiceIODTO.SeqNr = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_SeqN_TAG);
                customerInvoiceIODTO.OCR = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_OC_TAG);
                customerInvoiceIODTO.OriginType = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_OriginType_TAG);
                customerInvoiceIODTO.OriginStatus = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_OriginStatus_TAG);
                customerInvoiceIODTO.PaymentCondition = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_PaymentCondition_TAG);
                customerInvoiceIODTO.CustomerNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_CustomerN_TAG);
                customerInvoiceIODTO.CustomerName = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_CustomerName_TAG);
                customerInvoiceIODTO.PriceListTypeId = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_PriceListTypeId_TAG);
                customerInvoiceIODTO.InvoiceDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_InvoiceDate_TAG));
                customerInvoiceIODTO.DeliveryDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryDate_TAG));
                customerInvoiceIODTO.DueDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DueDate_TAG));
                customerInvoiceIODTO.VoucherDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_VoucherDate_TAG));
                customerInvoiceIODTO.ReferenceOur = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ReferenceOur_TAG);
                customerInvoiceIODTO.ReferenceYour = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ReferenceYour_TAG);
                customerInvoiceIODTO.CurrencyRate = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_CurrencyRate_TAG);
                if (customerInvoiceIODTO.CurrencyRate == 0)
                    customerInvoiceIODTO.CurrencyRate = 1;
                customerInvoiceIODTO.CurrencyDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_CurrencyDate_TAG));
                customerInvoiceIODTO.SumAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_SumAmount_TAG);
                customerInvoiceIODTO.SumAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_SumAmountCurrency_TAG);
                customerInvoiceIODTO.TotalAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_TotalAmount_TAG);
                customerInvoiceIODTO.TotalAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_TotalAmountCurrency_TAG);
                customerInvoiceIODTO.VATAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VATAmount_TAG);
                customerInvoiceIODTO.VATAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VATAmountCurrency_TAG);
                customerInvoiceIODTO.PaidAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_PaidAmount_TAG);
                customerInvoiceIODTO.PaidAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_PaidAmountCurrency_TAG);
                customerInvoiceIODTO.RemainingAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_RemainingAmount_TAG);
                customerInvoiceIODTO.FreightAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_FreightAmount_TAG);
                customerInvoiceIODTO.FreightAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_FreightAmountCurrency_TAG);
                customerInvoiceIODTO.InvoiceFee = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_InvoiceFee_TAG);
                customerInvoiceIODTO.InvoiceFeeCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_InvoiceFeeCurrency_TAG);
                customerInvoiceIODTO.CentRounding = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_CentRounding_TAG);
                customerInvoiceIODTO.FullyPayed = XmlUtil.GetElementNullableBoolValue(customerInvoiceIOElement, XML_FullyPayed_TAG);
                customerInvoiceIODTO.PaymentNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_PaymentN_TAG);
                customerInvoiceIODTO.VoucherNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_VoucherN_TAG);
                customerInvoiceIODTO.CreateAccountingInXE = XmlUtil.GetElementNullableBoolValue(customerInvoiceIOElement, XML_CreateAccountingInXE_TAG);
                customerInvoiceIODTO.Note = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_Note_TAG);
                customerInvoiceIODTO.BillingType = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_BillingType_TAG);
                customerInvoiceIODTO.TransferType = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_TransferType_TAG);
                customerInvoiceIODTO.BillingAddressAddress = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_BillingAddressAddress_TAG);
                customerInvoiceIODTO.BillingAddressCity = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_BillingAddressCity_TAG);
                customerInvoiceIODTO.BillingAddressCO = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_BillingAddressCO_TAG);
                customerInvoiceIODTO.BillingAddressPostNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_BillingAddressPostN_TAG);
                customerInvoiceIODTO.DeliveryAddressAddress = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryAddressAddress_TAG);
                customerInvoiceIODTO.DeliveryAddressCity = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryAddressCity_TAG);
                customerInvoiceIODTO.DeliveryAddressPostNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryAddressPostN_TAG);
                customerInvoiceIODTO.UseFixedPriceArticle = XmlUtil.GetElementNullableBoolValue(customerInvoiceIOElement, XML_UseFixedPriceFromHead_TAG);
                customerInvoiceIODTO.Currency = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_Currency_TAG);

                customerInvoiceIODTO.VatRate1 = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VatRate1_TAG);
                customerInvoiceIODTO.VatRate2 = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VatRate2_TAG);
                customerInvoiceIODTO.VatRate3 = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VatRate3_TAG);

                customerInvoiceIODTO.VatAmount1 = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VatAmount1_TAG);
                customerInvoiceIODTO.VatAmount2 = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VatAmount2_TAG);
                customerInvoiceIODTO.VatAmount3 = XmlUtil.GetElementNullableDecimalValue(customerInvoiceIOElement, XML_VatAmount3_TAG);

                if (!customerInvoiceIODTO.InvoiceDate.HasValue && !customerInvoiceIODTO.DueDate.HasValue && !customerInvoiceIODTO.VoucherDate.HasValue)
                    customerInvoiceIODTO.InvoiceDate = /*customerInvoiceIODTO.DueDate = customerInvoiceIODTO.VoucherDate = */ DateTime.Now.Date;

                customerInvoiceIODTO.BillingAddressCountry = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_BillingAddressCountry_TAG);
                customerInvoiceIODTO.DeliveryAddressCO = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryAddressCO_TAG);
                customerInvoiceIODTO.DeliveryAddressCountry = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryAddressCountry_TAG);
                customerInvoiceIODTO.InvoiceState = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_InvoiceState_TAG);
                customerInvoiceIODTO.VatType = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_VatType_TAG);
                customerInvoiceIODTO.BillingAddressName = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_BillingAddressName_TAG);
                customerInvoiceIODTO.DeliveryAddressName = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_DeliveryAddressName_TAG);
                customerInvoiceIODTO.Language = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_Language_TAG);
                customerInvoiceIODTO.PaymentConditionCode = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_PaymentConditionCode_TAG);
                customerInvoiceIODTO.InvoiceDeliveryType = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_InvoiceDeliveryType_TAG);
                customerInvoiceIODTO.SaleAccountNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountN_TAG);
                customerInvoiceIODTO.SaleAccountNrDim2 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrDim2_TAG);
                customerInvoiceIODTO.SaleAccountNrDim3 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrDim3_TAG);
                customerInvoiceIODTO.SaleAccountNrDim4 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrDim4_TAG);
                customerInvoiceIODTO.SaleAccountNrDim5 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrDim5_TAG);
                customerInvoiceIODTO.SaleAccountNrDim6 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrDim6_TAG);
                customerInvoiceIODTO.SaleAccountNrSieDim1 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrSieDim1_TAG);
                customerInvoiceIODTO.SaleAccountNrSieDim6 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_SaleAccountNrSieDim6_TAG);
                customerInvoiceIODTO.ClaimAccountNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountN_TAG);
                customerInvoiceIODTO.ClaimAccountNrDim2 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrDim2_TAG);
                customerInvoiceIODTO.ClaimAccountNrDim3 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrDim3_TAG);
                customerInvoiceIODTO.ClaimAccountNrDim4 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrDim4_TAG);
                customerInvoiceIODTO.ClaimAccountNrDim5 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrDim5_TAG);
                customerInvoiceIODTO.ClaimAccountNrDim6 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrDim6_TAG);
                customerInvoiceIODTO.ClaimAccountNrSieDim1 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrSieDim1_TAG);
                customerInvoiceIODTO.ClaimAccountNrSieDim6 = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ClaimAccountNrSieDim6_TAG);
                customerInvoiceIODTO.VatAccountNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_VatAccountN_TAG);
                customerInvoiceIODTO.OrderNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_OrderN_TAG);
                customerInvoiceIODTO.OfferNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_OfferN_TAG);
                customerInvoiceIODTO.ContractNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractN_TAG);
                customerInvoiceIODTO.WorkingDescription = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_WorkingDescription_TAG);
                customerInvoiceIODTO.InternalDescription = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_InternalDescription_TAG);
                customerInvoiceIODTO.ExternalDescription = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ExternalDescription_TAG);
                customerInvoiceIODTO.ProjectNr = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ProjectN_TAG);

                customerInvoiceIODTO.NextContractPeriodYear = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_NextContractPeriodYear_TAG);
                customerInvoiceIODTO.NextContractPeriodValue = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_NextContractPeriodValue_TAG);
                customerInvoiceIODTO.NextContractPeriodDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_NextContractPeriodDate_TAG));
                customerInvoiceIODTO.ContractGroupId = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_ContractGroupId_TAG);
                customerInvoiceIODTO.ContractGroupInterval = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_ContractGroupInterval_TAG);
                customerInvoiceIODTO.ContractGroupDayInMonth = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_ContractGroupDayInMonth_TAG);
                customerInvoiceIODTO.ContractGroupOrderTemplate = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_ContractGroupOrderTemplate_TAG);
                customerInvoiceIODTO.ContractGroupInvoiceTemplate = XmlUtil.GetElementNullableIntValue(customerInvoiceIOElement, XML_ContractGroupInvoiceTemplate_TAG);
                customerInvoiceIODTO.ContractGroupDecription = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractGroupDecription_TAG);
                customerInvoiceIODTO.ContractGroupName = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractGroupName_TAG);
                customerInvoiceIODTO.ContractGroupPeriod = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractGroupPeriod_TAG);
                customerInvoiceIODTO.ContractGroupPriceManagementName = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractGroupPriceManagementName_TAG);
                customerInvoiceIODTO.ContractGroupInvoiceText = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractGroupInvoiceText_TAG);
                customerInvoiceIODTO.ContractGroupInvoiceRowText = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ContractGroupInvoiceRowText_TAG);
                customerInvoiceIODTO.ContractEndYear = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_ContractEndYear_TAG);
                customerInvoiceIODTO.ContractEndMonth = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_ContractEndMonth_TAG);
                customerInvoiceIODTO.ContractEndDay = XmlUtil.GetElementIntValue(customerInvoiceIOElement, XML_ContractEndDay_TAG);

                customerInvoiceIODTO.InvoiceLabel = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_InvoiceLabel_TAG);
                customerInvoiceIODTO.InvoiceHeadText = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_InvoiceHeadText_TAG);
                customerInvoiceIODTO.ExternalId = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_ExternalId_TAG);
                customerInvoiceIODTO.Email = XmlUtil.GetChildElementValue(customerInvoiceIOElement, XML_Email_TAG);
                customerInvoiceIODTO.CreateDeliveryAddressAsTextOnly  = XmlUtil.GetElementNullableBoolValue(customerInvoiceIOElement, XML_CreateDeliveryAddressAsTextOnly_TAG);




                #endregion

                #region Fix

                if (!customerInvoiceIODTO.SeqNr.HasValue && customerInvoiceIODTO.SeqNr != 0 && !string.IsNullOrEmpty(customerInvoiceIODTO.CustomerInvoiceNr) && customerInvoiceIODTO.CustomerInvoiceNr != "0")
                {
                    int seqnr = 0;

                    int.TryParse(customerInvoiceIODTO.CustomerInvoiceNr, out seqnr);

                    if (seqnr != 0)
                        customerInvoiceIODTO.SeqNr = seqnr;
                }


                if (customerInvoiceIODTO.RegistrationType == (int)OrderInvoiceRegistrationType.Contract)
                {

                    if (customerInvoiceIODTO.ContractEndYear < 99)
                        customerInvoiceIODTO.ContractEndYear = customerInvoiceIODTO.ContractEndYear + 2000;

                    if (customerInvoiceIODTO.NextContractPeriodYear != null && customerInvoiceIODTO.NextContractPeriodYear > 0 && customerInvoiceIODTO.NextContractPeriodValue != null && customerInvoiceIODTO.NextContractPeriodValue > 0)
                    {
                        string year = customerInvoiceIODTO.NextContractPeriodYear.HasValue ? customerInvoiceIODTO.NextContractPeriodYear.ToString() : DateTime.Now.Year.ToString();
                        string period = customerInvoiceIODTO.NextContractPeriodValue.HasValue ? customerInvoiceIODTO.NextContractPeriodValue.ToString() : DateTime.Now.Month.ToString();
                        string day = customerInvoiceIODTO.ContractGroupDayInMonth.HasValue && customerInvoiceIODTO.ContractGroupDayInMonth != 0 ? customerInvoiceIODTO.ContractGroupDayInMonth.ToString() : "14";

                        if (customerInvoiceIODTO.NextContractPeriodYear < 99)
                        {
                            year = 20 + year;
                            customerInvoiceIODTO.NextContractPeriodYear = Convert.ToInt32(year);
                        }

                        if (day == "0")
                            day = "1";

                        try
                        {
                            if (period == "2" && Convert.ToInt32(day) > 28)
                                day = "28";
                        }
                        catch
                        {
                            //Continue:
                        }


                        string nextContractPeriodDateString = year + "-" + period + "-" + day;

                        DateTime nextContractPeriodDate = CalendarUtility.DATETIME_DEFAULT;

                        DateTime.TryParse(nextContractPeriodDateString, out nextContractPeriodDate);

                        if (nextContractPeriodDate > DateTime.Now.AddYears(-5))
                        {
                            if (!customerInvoiceIODTO.ContractGroupDayInMonth.HasValue)
                            {
                                nextContractPeriodDate = CalendarUtility.GetLastDateOfMonth(nextContractPeriodDate);
                                customerInvoiceIODTO.ContractGroupDayInMonth = nextContractPeriodDate.Day;
                            }

                            customerInvoiceIODTO.NextContractPeriodDate = nextContractPeriodDate;
                        }

                    }

                    if (customerInvoiceIODTO.DueDate == null)
                    {
                        if (customerInvoiceIODTO.ContractEndYear != 0 && customerInvoiceIODTO.ContractEndMonth != 0)
                        {
                            try
                            {
                                if (customerInvoiceIODTO.ContractEndDay == 0 || customerInvoiceIODTO.ContractEndDay > 31)
                                {
                                    if (customerInvoiceIODTO.ContractGroupDayInMonth != null && customerInvoiceIODTO.ContractGroupDayInMonth != 0)
                                        customerInvoiceIODTO.ContractEndDay = (int)customerInvoiceIODTO.ContractGroupDayInMonth;
                                    else
                                        customerInvoiceIODTO.ContractEndDay = 1;
                                }

                                if (customerInvoiceIODTO.ContractEndMonth == 2 && customerInvoiceIODTO.ContractEndDay > 28)
                                    customerInvoiceIODTO.ContractEndDay = 28;


                                DateTime dueDate = new DateTime(customerInvoiceIODTO.ContractEndYear,
                                                                                     customerInvoiceIODTO.ContractEndMonth,
                                                                                     customerInvoiceIODTO.ContractEndDay);

                                if (dueDate > DateTime.Now.AddYears(-20))
                                    customerInvoiceIODTO.DueDate = dueDate;

                            }
                            catch
                            {
                                // Do nothing
                                // NOSONAR
                            }
                        }

                    }
                }

                if (customerInvoiceIODTO.RemainingAmount == 0 && customerInvoiceIODTO.TotalAmountCurrency != 0 && !string.IsNullOrEmpty(customerInvoiceIODTO.CustomerInvoiceNr) && customerInvoiceIODTO.PaidAmount == null)
                {
                    customerInvoiceIODTO.FullyPayed = true;
                    customerInvoiceIODTO.PaidAmount = customerInvoiceIODTO.TotalAmountCurrency;
                }

                if (!string.IsNullOrEmpty(customerInvoiceIODTO.CustomerInvoiceNr) && !customerInvoiceIODTO.SeqNr.HasValue)
                {
                    int seqNr = 0;
                    int.TryParse(customerInvoiceIODTO.CustomerInvoiceNr, out seqNr);

                    if (seqNr != 0)
                        customerInvoiceIODTO.SeqNr = seqNr;
                }

                if (customerInvoiceIODTO.VoucherDate == CalendarUtility.DATETIME_DEFAULT && customerInvoiceIODTO.InvoiceDate != CalendarUtility.DATETIME_DEFAULT && !string.IsNullOrEmpty(customerInvoiceIODTO.VoucherNr))
                    customerInvoiceIODTO.VoucherDate = customerInvoiceIODTO.InvoiceDate;

                #endregion

                CustomerInvoiceRowIOItem rowIOItem = new CustomerInvoiceRowIOItem();

                List<XElement> customerInvoiceRowIOElements = XmlUtil.GetChildElements(customerInvoiceIOElement, rowIOItem.XML_PARENT_TAG);
                rowIOItem.CreateObjects(customerInvoiceRowIOElements, headType, customerInvoiceIODTO.CurrencyRate ?? 1);
                customerInvoiceIODTO.InvoiceRows.AddRange(rowIOItem.customerInvoiceRows);

                CustomerInvoices.Add(customerInvoiceIODTO);
            }
        }

        #endregion
    }

}
