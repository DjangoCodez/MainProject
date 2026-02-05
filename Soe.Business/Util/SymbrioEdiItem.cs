using Soe.Edi.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using static Soe.Edi.Common.Enumerations;

namespace SoftOne.Soe.Business.Util
{
    public class SymbrioEdiItem : ISoeImportItem
    {
        #region Variables

        private ReportDataManager rdm;

        private readonly XDocument xdoc;
        public string XML
        {
            get
            {
                return xdoc != null ? xdoc.ToString() : string.Empty;
            }
        }
        private readonly string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
        }
        private List<SymbrioEdiRowItem> rows;
        public List<SymbrioEdiRowItem> Rows
        {
            get
            {
                return rows;
            }
        }
        private List<SymbrioEdiRowAccountCode> accountCodes;
        public List<SymbrioEdiRowAccountCode> AccountCodes
        {
            get
            {
                return accountCodes;
            }
        }

        public XElement ElementMessageInfo { get; set; }
        public XElement ElementSeller { get; set; }
        public XElement ElementSellerAddress { get; set; }
        public XElement ElementSellerInfo { get; set; }
        public XElement ElementBuyer { get; set; }
        public XElement ElementBuyerPostalAddress { get; set; }
        public XElement ElementBuyerInfo { get; set; }
        public XElement ElementHead { get; set; }
        public XElement ElementInvoicePaymentDetails { get; set; }

        public XElement ElementInvoiceVatDetails { get; set; }
        public XElement ElementLabels { get; set; }  // Added for translations

        public string MessageSenderId { get; set; }
        public string MessageType { get; set; }
        public DateTime? MessageDate { get; set; }
        public string MessageImageFileName { get; set; }
        public string MessageCultureInfo { get; set; }


        public string SellerId { get; set; }
        public string SellerOrganisationNumber { get; set; }
        public string SellerName { get; set; }
        public string SellerVatNumber { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPostalCode { get; set; }
        public string SellerPostalAddress { get; set; }
        public string SellerCountryCode { get; set; }
        public string SellerPhone { get; set; }
        public string SellerFax { get; set; }
        public string SellerReference { get; set; }
        public string SellerReferencePhone { get; set; }

        public string BuyerId { get; set; }
        public string BuyerOrganisationNumber { get; set; }
        public string BuyerVatNumber { get; set; }
        public string BuyerName { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPostalCode { get; set; }
        public string BuyerPostalAddress { get; set; }
        public string BuyerCountryCode { get; set; }
        public string BuyerReference { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerFax { get; set; }
        public string BuyerEmailAddress { get; set; }
        public string BuyerDeliveryName { get; set; }
        public string BuyerDeliveryCoAddress { get; set; }
        public string BuyerDeliveryAddress { get; set; }
        public string BuyerDeliveryPostalCode { get; set; }
        public string BuyerDeliveryPostalAddress { get; set; }
        public string BuyerDeliveryCountryCode { get; set; }
        public string BuyerDeliveryNoteText { get; set; }
        public string BuyerDeliveryGoodsMarking { get; set; }

        public string HeadInvoiceNumber { get; set; }
        public string HeadInvoiceOcr { get; set; }
        public int HeadInvoiceType { get; set; }
        public DateTime? HeadInvoiceDate { get; set; }
        public DateTime? HeadInvoiceDueDate { get; set; }
        public DateTime? HeadDeliveryDate { get; set; }
        public string HeadBuyerOrderNumber { get; set; }
        public string HeadBuyerProjectNumber { get; set; }
        public string HeadBuyerInstallationNumber { get; set; }
        public string HeadSellerOrderNumber { get; set; }
        public string HeadPostalGiro { get; set; }
        public string HeadBankGiro { get; set; }
        public string HeadBank { get; set; }
        public string HeadBicAddress { get; set; }
        public string HeadIbanNumber { get; set; }
        public string HeadCurrencyCode { get; set; }
        public decimal HeadVatPercentage { get; set; }
        public int HeadPaymentConditionDays { get; set; }
        public string HeadPaymentConditionText { get; set; }
        public decimal HeadInterestPaymentPercent { get; set; }
        public string HeadInterestPaymentText { get; set; }
        public decimal HeadInvoiceGrossAmount { get; set; }
        public decimal HeadInvoiceNetAmount { get; set; }
        public decimal HeadVatBasisAmount { get; set; }
        public decimal HeadVatAmount { get; set; }
        public decimal HeadFreightFeeAmount { get; set; }
        public decimal HeadHandlingChargeFeeAmount { get; set; }
        public decimal HeadInsuranceFeeAmount { get; set; }
        public decimal HeadRemainingFeeAmount { get; set; }
        public decimal HeadDiscountAmount { get; set; }
        public decimal HeadRoundingAmount { get; set; }
        public int HeadInvoiceArrival { get; set; }
        public bool HeadInvoiceAuthorized { get; set; }
        public string HeadInvoiceAuthorizedBy { get; set; }
        public string HeadInvoiceTaxText { get; set; }

        #region Label variables
        public string LabelMessageType { get; set; }
        public string LabelNumber { get; set; }
        public string LabelDate { get; set; }
        public string LabelDeliveryDate { get; set; }
        public string LabelDueDate { get; set; }
        public string LabelOCR { get; set; }
        public string LabelDeliveryAddress { get; set; }
        public string LabelSellersOrderNumber { get; set; }
        public string LabelYourReference { get; set; }
        public string LabelYourOrderNumber { get; set; }
        public string LabelOurReference { get; set; }
        public string LabelProjectNumber { get; set; }
        public string LabelInstallationNumber { get; set; }
        public string LabelDiscount { get; set; }
        public string LabelOrganizationNumber { get; set; }
        public string LabelProductNumber { get; set; }
        public string LabelProductName { get; set; }
        public string LabelAmount { get; set; }
        public string LabelUnitPrice { get; set; }
        public string LabelVAT { get; set; }
        public string LabelRowDiscount { get; set; }
        public string LabelRowSum { get; set; }
        public string LabelRowDeliveryDate { get; set; }
        public string LabelSum { get; set; }
        public string LabelFeeAmount { get; set; }
        public string LabelFreightAmount { get; set; }
        public string LabelVATAmount { get; set; }
        public string LabelCurrencyCode { get; set; }
        public string LabelRowDiscountPercent { get; set; }
        public string LabelTotalGrossAmount { get; set; }
        public string LabelSellerOrganizationNumber { get; set; }
        public string LabelSellerVatNumber { get; set; }
        public string LabelHeadpostalGiro { get; set; }
        public string LabelHeadBankGiro { get; set; }
        public string LabelHeadBankNumber { get; set; }
        public string LabelSellerPhone { get; set; }
        public string LabelSellerFax { get; set; }
        public string LabelSellerAddress { get; set; }
        public string LabelPoweredBySoftOne { get; set; }

        #endregion


        #endregion

        #region Ctor

        public SymbrioEdiItem(XDocument xdoc, string fileName, int ediEntryType, bool loadRows, bool loadAccountCodes, SysEdiMessageHeadDTO sysEdiMessageHeadDTO = null)
        {
            this.xdoc = xdoc;
            this.fileName = fileName;
            this.rows = new List<SymbrioEdiRowItem>();
            this.accountCodes = new List<SymbrioEdiRowAccountCode>();

            if (sysEdiMessageHeadDTO == null)
                Parse(loadRows, loadAccountCodes);
            else
                Convert(sysEdiMessageHeadDTO);
        }

        #endregion

        #region Static methods

        public static SymbrioEdiItem CreateItem(string xml, string fileName, int ediEntryType, bool loadRows, bool loadAccountCodes, SysEdiMessageHeadDTO sysEdiMessageHeadDTO = null)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            // Check for byte-order mark and remove it from the beginning if thats the case
            char byteOrderMark = (char)0xFEFF;
            if (xml.FirstOrDefault().Equals(byteOrderMark))
                xml = xml.TrimStart(byteOrderMark);

            var xdoc = XDocument.Parse(XmlUtil.FormatXml(xml));
            if (xdoc == null)
                return null;

            return new SymbrioEdiItem(xdoc, fileName, ediEntryType, true, true, sysEdiMessageHeadDTO);
        }

        #endregion

        #region Public methods

        public void ParseRows()
        {
            if (xdoc == null)
                return;

            foreach (XElement row in XmlUtil.GetChildElements(xdoc, "Row"))
            {
                Rows.Add(new SymbrioEdiRowItem(row));
            }
        }

        public bool IsInvoice()
        {
            return !string.IsNullOrEmpty(MessageType) && MessageType == "INVOICE";
        }

        public void LoadAccountCodes()
        {
            foreach (XElement accountCode in XmlUtil.GetChildElements(xdoc, "AccountCode"))
            {
                AccountCodes.Add(new SymbrioEdiRowAccountCode(accountCode));
            }
        }

        #endregion

        #region Help-methods

        private void Parse(bool loadRows, bool loadAccountCodes)
        {
            if (this.xdoc == null)
                return;

            //if (ediEntryType != (int)TermGroup_EDISourceType.Finvoice)
            //{
            #region EDI/SCANNING

            #region MessageInfo

            ElementMessageInfo = XmlUtil.GetChildElement(xdoc, "MessageInfo");
            if (ElementMessageInfo != null)
            {
                MessageSenderId = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageSenderId");
                MessageType = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageType");
                MessageDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageDate"));
                MessageImageFileName = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageImageFileName");
                MessageCultureInfo = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageCultureInfo");  // Added to get Finnish translation for header
            }

            #endregion

            #region Seller

            ElementSeller = XmlUtil.GetChildElement(xdoc, "Seller");
            if (ElementSeller != null)
            {
                SellerId = XmlUtil.GetChildElementValue(ElementSeller, "SellerId");
                SellerOrganisationNumber = XmlUtil.GetChildElementValue(ElementSeller, "SellerOrganisationNumber");
                SellerVatNumber = XmlUtil.GetChildElementValue(ElementSeller, "SellerVatNumber");
                SellerName = XmlUtil.GetChildElementValue(ElementSeller, "SellerName");
                SellerAddress = XmlUtil.GetChildElementValue(ElementSeller, "SellerAddress");
                SellerPostalCode = XmlUtil.GetChildElementValue(ElementSeller, "SellerPostalCode");
                SellerPostalAddress = XmlUtil.GetChildElementValue(ElementSeller, "SellerPostalAddress");
                SellerCountryCode = XmlUtil.GetChildElementValue(ElementSeller, "SellerCountryCode");
                SellerPhone = XmlUtil.GetChildElementValue(ElementSeller, "SellerPhone");
                SellerFax = XmlUtil.GetChildElementValue(ElementSeller, "SellerFax");
                SellerReference = XmlUtil.GetChildElementValue(ElementSeller, "SellerReference");
                SellerReferencePhone = XmlUtil.GetChildElementValue(ElementSeller, "SellerReferencePhone");
            }

            #endregion

            #region Buyer

            ElementBuyer = XmlUtil.GetChildElement(xdoc, "Buyer");
            if (ElementBuyer != null)
            {
                BuyerId = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerId");
                BuyerOrganisationNumber = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerOrganisationNumber");
                BuyerVatNumber = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerVatNumber");
                BuyerName = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerName");
                BuyerAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerAddress");
                BuyerPostalCode = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerPostalCode");
                BuyerPostalAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerPostalAddress");
                BuyerCountryCode = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerCountryCode");
                BuyerReference = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerReference");
                BuyerPhone = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerPhone");
                BuyerFax = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerFax");
                BuyerEmailAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerEmailAddress");
                BuyerDeliveryName = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryName");
                BuyerDeliveryCoAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryCoAddress");
                BuyerDeliveryAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryAddress");
                BuyerDeliveryPostalCode = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryPostalCode");
                BuyerDeliveryPostalAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryPostalAddress");
                BuyerDeliveryCountryCode = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryCountryCode");
                BuyerDeliveryNoteText = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryNoteText");
                BuyerDeliveryGoodsMarking = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryGoodsMarking");
            }

            #endregion

            #region Head

            ElementHead = XmlUtil.GetChildElement(xdoc, "Head");
            if (ElementHead != null)
            {
                HeadInvoiceNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceNumber");
                HeadInvoiceOcr = XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceOcr");
                HeadInvoiceType = StringUtility.GetInt(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceType"), 0);
                HeadInvoiceDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceDate"));
                HeadInvoiceDueDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceDueDate"));
                HeadDeliveryDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(ElementHead, "HeadDeliveryDate"));
                HeadBuyerOrderNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadBuyerOrderNumber");
                HeadBuyerProjectNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadBuyerProjectNumber");
                HeadBuyerInstallationNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadBuyerInstallationNumber");
                HeadSellerOrderNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadSellerOrderNumber");
                HeadPostalGiro = XmlUtil.GetChildElementValue(ElementHead, "HeadPostalGiro");
                HeadBankGiro = XmlUtil.GetChildElementValue(ElementHead, "HeadBankGiro");
                HeadBank = XmlUtil.GetChildElementValue(ElementHead, "HeadBank");
                HeadBicAddress = XmlUtil.GetChildElementValue(ElementHead, "HeadBicAddress");
                HeadIbanNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadIbanNumber");
                HeadCurrencyCode = XmlUtil.GetChildElementValue(ElementHead, "HeadCurrencyCode");
                HeadVatPercentage = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadVatPercentage"), 2);
                HeadPaymentConditionDays = StringUtility.GetInt(XmlUtil.GetChildElementValue(ElementHead, "HeadPaymentConditionDays"), 0);
                HeadPaymentConditionText = XmlUtil.GetChildElementValue(ElementHead, "HeadPaymentConditionText");
                HeadInterestPaymentPercent = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadInterestPaymentPercent"), 2);
                HeadInterestPaymentText = XmlUtil.GetChildElementValue(ElementHead, "HeadInterestPaymentText");
                HeadInvoiceGrossAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceGrossAmount"), 2);
                if (HeadInvoiceGrossAmount == 0)
                    HeadInvoiceGrossAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadGrossAmount"), 2);
                HeadInvoiceNetAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceNetAmount"), 2);
                if (HeadInvoiceNetAmount == 0)
                    HeadInvoiceNetAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadNetAmount"), 2);
                HeadVatBasisAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadVatBasisAmount"), 2);
                HeadVatAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadVatAmount"), 2);
                HeadFreightFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadFreightFeeAmount"), 2);
                HeadHandlingChargeFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadHandlingChargeFeeAmount"), 2);
                HeadInsuranceFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadInsuranceFeeAmount"), 2);
                HeadRemainingFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadRemainingFeeAmount"), 2);
                HeadDiscountAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadDiscountAmount"), 2);
                HeadRoundingAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadRoundingAmount"), 2);
                HeadInvoiceArrival = StringUtility.GetInt(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceArrival"), 0);
                HeadInvoiceAuthorized = StringUtility.GetBool(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceAuthorized"));
                HeadInvoiceAuthorizedBy = XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceAuthorizedBy");
                HeadInvoiceTaxText = XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceTaxText");
            }

            #endregion

            #region Labels
            // 18.11.2014 Added labels for translations
            ElementLabels = XmlUtil.GetChildElement(xdoc, "Labels");
            if (ElementLabels != null)
            {

                LabelMessageType = XmlUtil.GetChildElementValue(ElementLabels, "LabelMessageType");
                LabelNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelNumber");
                LabelDate = XmlUtil.GetChildElementValue(ElementLabels, "LabelDate");
                LabelDeliveryDate = XmlUtil.GetChildElementValue(ElementLabels, "LabelDeliveryDate");
                LabelDueDate = XmlUtil.GetChildElementValue(ElementLabels, "LabelDueDate");
                LabelOCR = XmlUtil.GetChildElementValue(ElementLabels, "LabelOCR");
                LabelDeliveryAddress = XmlUtil.GetChildElementValue(ElementLabels, "LabelDeliveryAddress");
                LabelSellersOrderNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelSellersOrderNumber");
                LabelYourReference = XmlUtil.GetChildElementValue(ElementLabels, "LabelYourReference");
                LabelYourOrderNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelYourOrderNumber");
                LabelOurReference = XmlUtil.GetChildElementValue(ElementLabels, "LabelOurReference");
                LabelProjectNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelProjectNumber");
                LabelInstallationNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelInstallationNumber");
                LabelDiscount = XmlUtil.GetChildElementValue(ElementLabels, "LabelDiscount");
                LabelOrganizationNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelOrganizationNumber");
                LabelProductNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelProductNumber");
                LabelProductName = XmlUtil.GetChildElementValue(ElementLabels, "LabelProductName");
                LabelAmount = XmlUtil.GetChildElementValue(ElementLabels, "LabelAmount");
                LabelUnitPrice = XmlUtil.GetChildElementValue(ElementLabels, "LabelUnitPrice");
                LabelVAT = XmlUtil.GetChildElementValue(ElementLabels, "LabelVAT");
                LabelRowDiscount = XmlUtil.GetChildElementValue(ElementLabels, "LabelRowDiscount");
                LabelRowSum = XmlUtil.GetChildElementValue(ElementLabels, "LabelRowSum");
                LabelRowDeliveryDate = XmlUtil.GetChildElementValue(ElementLabels, "LabelRowDeliveryDate");
                LabelSum = XmlUtil.GetChildElementValue(ElementLabels, "LabelSum");
                LabelFeeAmount = XmlUtil.GetChildElementValue(ElementLabels, "LabelFeeAmount");
                LabelFreightAmount = XmlUtil.GetChildElementValue(ElementLabels, "LabelFreightAmount");
                LabelVATAmount = XmlUtil.GetChildElementValue(ElementLabels, "LabelVATAmount");
                LabelCurrencyCode = XmlUtil.GetChildElementValue(ElementLabels, "LabelCurrencyCode");
                LabelRowDiscountPercent = XmlUtil.GetChildElementValue(ElementLabels, "LabelRowDiscountPercent");
                LabelTotalGrossAmount = XmlUtil.GetChildElementValue(ElementLabels, "LabelTotalGrossAmount");
                LabelSellerOrganizationNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelSellerOrganizationNumber");
                LabelSellerVatNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelSellerVatNumber");
                LabelHeadpostalGiro = XmlUtil.GetChildElementValue(ElementLabels, "LabelHeadpostalGiro");
                LabelHeadBankGiro = XmlUtil.GetChildElementValue(ElementLabels, "LabelHeadBankGiro");
                LabelHeadBankNumber = XmlUtil.GetChildElementValue(ElementLabels, "LabelHeadBankNumber");
                LabelSellerPhone = XmlUtil.GetChildElementValue(ElementLabels, "LabelSellerPhone");
                LabelSellerFax = XmlUtil.GetChildElementValue(ElementLabels, "LabelSellerFax");
                LabelSellerAddress = XmlUtil.GetChildElementValue(ElementLabels, "LabelSellerAddress");
                LabelPoweredBySoftOne = XmlUtil.GetChildElementValue(ElementLabels, "LabelPoweredBySoftOne");
            }

            #endregion

            #region Rows

            if (loadRows)
                ParseRows();

            #endregion

            #region AccountCodes

            if (loadAccountCodes)
                LoadAccountCodes();

            #endregion

            #region Postfix

            //Calculate sum if it not is given
            if (this.HeadInvoiceGrossAmount == 0)
            {
                foreach (var row in this.Rows)
                {
                    decimal rowAmount = 0;
                    if (row.RowNetAmount > 0)
                        rowAmount = row.RowNetAmount + row.RowVatAmount;
                    else
                        rowAmount = row.RowQuantity * row.RowUnitPrice;

                    this.HeadInvoiceGrossAmount += rowAmount;
                }
            }

            #endregion

            #endregion
            /*}
            else
            {
                #region FINVOICE

                #region MessageInfo

                ElementMessageInfo = XmlUtil.GetElement(xdoc, "MessageInfo"); //DONT EXTIST
                if (ElementMessageInfo != null)
                {
                    MessageSenderId = XmlUtil.GetElementValue(ElementMessageInfo, "MessageSenderId");
                    MessageType = XmlUtil.GetElementValue(ElementMessageInfo, "MessageType");
                    MessageDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetElementValue(ElementMessageInfo, "MessageDate"));
                    MessageImageFileName = XmlUtil.GetElementValue(ElementMessageInfo, "MessageImageFileName");
                    MessageCultureInfo = XmlUtil.GetElementValue(ElementMessageInfo, "MessageCultureInfo");  // Added to get Finnish translation for header
                }

                #endregion

                #region Seller

                ElementSeller = XmlUtil.GetElement(xdoc, "SellerPartyDetails");
                if (ElementSeller != null)
                {
                    SellerId = XmlUtil.GetElementValue(ElementSeller, "SellerId"); //DONT EXTIST
                    SellerOrganisationNumber = XmlUtil.GetElementValue(ElementSeller, "SellerPartyIdentifier");
                    SellerVatNumber = XmlUtil.GetElementValue(ElementSeller, "SellerOrganisationTaxCode");
                    SellerName = XmlUtil.GetElementValue(ElementSeller, "SellerOrganisationName");

                    ElementSellerAddress = XmlUtil.GetElement(ElementSeller, "SellerPostalAddressDetails");
                    if (ElementSellerAddress != null)
                    {
                        SellerAddress = XmlUtil.GetElementValue(ElementSeller, "SellerStreetName");
                        SellerPostalCode = XmlUtil.GetElementValue(ElementSeller, "SellerTownName");
                        SellerPostalAddress = XmlUtil.GetElementValue(ElementSeller, "SellerPostalAddress");
                        SellerCountryCode = XmlUtil.GetElementValue(ElementSeller, "CountryCode");
                    }

                    ElementSellerInfo = XmlUtil.GetElement(ElementSeller, "SellerInformationDetails");
                    if (ElementSellerAddress != null)
                    {
                        SellerPhone = XmlUtil.GetElementValue(ElementSeller, "SellerPhoneNumber");
                        SellerFax = XmlUtil.GetElementValue(ElementSeller, "SellerFaxNumber");
                        SellerReferencePhone = XmlUtil.GetElementValue(ElementSeller, "SellerReferencePhone"); //DONT EXTIST
                    }

                    SellerReference = XmlUtil.GetElementValue(ElementSeller, "SellerContactPersonName");
                }

                #endregion

                #region Buyer

                ElementBuyer = XmlUtil.GetElement(xdoc, "BuyerPartyDetails");
                if (ElementBuyer != null)
                {
                    BuyerId = XmlUtil.GetElementValue(ElementBuyer, "BuyerId"); //DONT EXTIST
                    BuyerOrganisationNumber = XmlUtil.GetElementValue(ElementBuyer, "BuyerPartyIdentifier");
                    BuyerVatNumber = XmlUtil.GetElementValue(ElementBuyer, "BuyerOrganisationTaxCode");
                    BuyerName = XmlUtil.GetElementValue(ElementBuyer, "BuyerOrganisationName");

                    ElementBuyerPostalAddress = XmlUtil.GetElement(ElementBuyer, "BuyerPostalAddressDetails");
                    if (ElementBuyerPostalAddress != null)
                    {
                        BuyerAddress = XmlUtil.GetElementValue(ElementBuyer, "BuyerStreetName");
                        BuyerPostalCode = XmlUtil.GetElementValue(ElementBuyer, "BuyerPostCodeIdentifier");
                        BuyerPostalAddress = XmlUtil.GetElementValue(ElementBuyer, "BuyerTownName");
                        BuyerCountryCode = XmlUtil.GetElementValue(ElementBuyer, "CountryCode");
                    }

                    BuyerReference = XmlUtil.GetElementValue(ElementBuyer, "BuyerContactPersonName");

                    ElementBuyerInfo = XmlUtil.GetElement(ElementBuyer, "BuyerPostalAddressDetails");
                    if (ElementBuyerInfo != null)
                    {
                        BuyerPhone = XmlUtil.GetElementValue(ElementBuyer, "BuyerPhoneNumberIdentifier");
                        BuyerFax = XmlUtil.GetElementValue(ElementBuyer, "BuyerFax"); //DONT EXTIST
                        BuyerEmailAddress = XmlUtil.GetElementValue(ElementBuyer, "BuyerEmailaddressIdentifier");
                    }

                    BuyerDeliveryName = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryName"); //DONT EXTIST
                    BuyerDeliveryCoAddress = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryCoAddress"); //DONT EXTIST
                    BuyerDeliveryAddress = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryAddress"); //DONT EXTIST
                    BuyerDeliveryPostalCode = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryPostalCode"); //DONT EXTIST
                    BuyerDeliveryPostalAddress = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryPostalAddress"); //DONT EXTIST
                    BuyerDeliveryCountryCode = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryCountryCode"); //DONT EXTIST
                    BuyerDeliveryNoteText = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryNoteText"); //DONT EXTIST
                    BuyerDeliveryGoodsMarking = XmlUtil.GetElementValue(ElementBuyer, "BuyerDeliveryGoodsMarking"); //DONT EXTIST
                }

                #endregion

                #region Head

                ElementHead = XmlUtil.GetElement(xdoc, "InvoiceDetails");
                if (ElementHead != null)
                {
                    HeadInvoiceNumber = XmlUtil.GetElementValue(ElementHead, "InvoiceNumber");
                    HeadInvoiceOcr = XmlUtil.GetElementValue(ElementHead, "HeadInvoiceOcr"); //DONT EXTIST
                    HeadInvoiceType = StringUtility.GetInt(XmlUtil.GetElementValue(ElementHead, "HeadInvoiceType"), 0); //DONT EXTIST
                    HeadInvoiceDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetElementValue(ElementHead, "InvoiceDate"));

                    ElementInvoicePaymentDetails = XmlUtil.GetElement(ElementHead, "InvoiceDetails");
                    if (ElementInvoicePaymentDetails != null)
                    {
                        HeadInvoiceDueDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetElementValue(ElementHead, "HeadInvoiceDueDate"));
                    }

                    HeadDeliveryDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetElementValue(ElementHead, "HeadDeliveryDate")); //DONT EXTIST
                    HeadBuyerOrderNumber = XmlUtil.GetElementValue(ElementHead, "HeadBuyerOrderNumber"); //DONT EXTIST
                    HeadBuyerProjectNumber = XmlUtil.GetElementValue(ElementHead, "HeadBuyerProjectNumber"); //DONT EXTIST
                    HeadBuyerInstallationNumber = XmlUtil.GetElementValue(ElementHead, "HeadBuyerInstallationNumber"); //DONT EXTIST
                    HeadSellerOrderNumber = XmlUtil.GetElementValue(ElementHead, "OrderIdentifier");
                    HeadPostalGiro = XmlUtil.GetElementValue(ElementHead, "HeadPostalGiro"); //DONT EXTIST
                    HeadBankGiro = XmlUtil.GetElementValue(ElementHead, "HeadBankGiro"); //DONT EXTIST
                    HeadBank = XmlUtil.GetElementValue(ElementHead, "HeadBank"); //DONT EXTIST
                    HeadBicAddress = XmlUtil.GetElementValue(ElementHead, "HeadBicAddress"); //DONT EXTIST
                    HeadIbanNumber = XmlUtil.GetElementValue(ElementHead, "HeadIbanNumber"); //DONT EXTIST

                    XElement amountElement = XmlUtil.GetElement(ElementHead, "InvoiceTotalVatExcludedAmount");
                    if(amountElement != null)
                        HeadCurrencyCode = XmlUtil.GetAttributeStringValue(amountElement, "AmountCurrencyIdentifier");
                    else
                        HeadCurrencyCode = XmlUtil.GetElementValue(ElementHead, "HeadCurrencyCode");

                    ElementInvoiceVatDetails = XmlUtil.GetElement(ElementHead, "VatSpecificationDetails");
                    if (ElementInvoicePaymentDetails != null)
                    {
                        HeadVatPercentage = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "VatRatePercent"), 2);
                    }
                        
                    HeadPaymentConditionDays = StringUtility.GetInt(XmlUtil.GetElementValue(ElementHead, "HeadPaymentConditionDays"), 0);
                    HeadPaymentConditionText = XmlUtil.GetElementValue(ElementHead, "HeadPaymentConditionText");
                    HeadInterestPaymentPercent = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadInterestPaymentPercent"), 2);
                    HeadInterestPaymentText = XmlUtil.GetElementValue(ElementHead, "HeadInterestPaymentText");
                    HeadInvoiceGrossAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadInvoiceGrossAmount"), 2);
                    if (HeadInvoiceGrossAmount == 0)
                        HeadInvoiceGrossAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadGrossAmount"), 2);
                    HeadInvoiceNetAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadInvoiceNetAmount"), 2);
                    if (HeadInvoiceNetAmount == 0)
                        HeadInvoiceNetAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadNetAmount"), 2);
                    HeadVatBasisAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadVatBasisAmount"), 2);
                    HeadVatAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadVatAmount"), 2);
                    HeadFreightFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadFreightFeeAmount"), 2);
                    HeadHandlingChargeFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadHandlingChargeFeeAmount"), 2);
                    HeadInsuranceFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadInsuranceFeeAmount"), 2);
                    HeadRemainingFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadRemainingFeeAmount"), 2);
                    HeadDiscountAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadDiscountAmount"), 2);
                    HeadRoundingAmount = NumberUtility.ToDecimal(XmlUtil.GetElementValue(ElementHead, "HeadRoundingAmount"), 2);
                    HeadInvoiceArrival = StringUtility.GetInt(XmlUtil.GetElementValue(ElementHead, "HeadInvoiceArrival"), 0);
                    HeadInvoiceAuthorized = StringUtility.GetBool(XmlUtil.GetElementValue(ElementHead, "HeadInvoiceAuthorized"));
                    HeadInvoiceAuthorizedBy = XmlUtil.GetElementValue(ElementHead, "HeadInvoiceAuthorizedBy");
                }

                #endregion

                #region Labels
                // 18.11.2014 Added labels for translations
                ElementLabels = XmlUtil.GetElement(xdoc, "Labels");
                if (ElementLabels != null)
                {

                    LabelMessageType = XmlUtil.GetElementValue(ElementLabels, "LabelMessageType");
                    LabelNumber = XmlUtil.GetElementValue(ElementLabels, "LabelNumber");
                    LabelDate = XmlUtil.GetElementValue(ElementLabels, "LabelDate");
                    LabelDeliveryDate = XmlUtil.GetElementValue(ElementLabels, "LabelDeliveryDate");
                    LabelDueDate = XmlUtil.GetElementValue(ElementLabels, "LabelDueDate");
                    LabelOCR = XmlUtil.GetElementValue(ElementLabels, "LabelOCR");
                    LabelDeliveryAddress = XmlUtil.GetElementValue(ElementLabels, "LabelDeliveryAddress");
                    LabelSellersOrderNumber = XmlUtil.GetElementValue(ElementLabels, "LabelSellersOrderNumber");
                    LabelYourReference = XmlUtil.GetElementValue(ElementLabels, "LabelYourReference");
                    LabelYourOrderNumber = XmlUtil.GetElementValue(ElementLabels, "LabelYourOrderNumber");
                    LabelOurReference = XmlUtil.GetElementValue(ElementLabels, "LabelOurReference");
                    LabelProjectNumber = XmlUtil.GetElementValue(ElementLabels, "LabelProjectNumber");
                    LabelInstallationNumber = XmlUtil.GetElementValue(ElementLabels, "LabelInstallationNumber");
                    LabelDiscount = XmlUtil.GetElementValue(ElementLabels, "LabelDiscount");
                    LabelOrganizationNumber = XmlUtil.GetElementValue(ElementLabels, "LabelOrganizationNumber");
                    LabelProductNumber = XmlUtil.GetElementValue(ElementLabels, "LabelProductNumber");
                    LabelProductName = XmlUtil.GetElementValue(ElementLabels, "LabelProductName");
                    LabelAmount = XmlUtil.GetElementValue(ElementLabels, "LabelAmount");
                    LabelUnitPrice = XmlUtil.GetElementValue(ElementLabels, "LabelUnitPrice");
                    LabelVAT = XmlUtil.GetElementValue(ElementLabels, "LabelVAT");
                    LabelRowDiscount = XmlUtil.GetElementValue(ElementLabels, "LabelRowDiscount");
                    LabelRowSum = XmlUtil.GetElementValue(ElementLabels, "LabelRowSum");
                    LabelRowDeliveryDate = XmlUtil.GetElementValue(ElementLabels, "LabelRowDeliveryDate");
                    LabelSum = XmlUtil.GetElementValue(ElementLabels, "LabelSum");
                    LabelFeeAmount = XmlUtil.GetElementValue(ElementLabels, "LabelFeeAmount");
                    LabelFreightAmount = XmlUtil.GetElementValue(ElementLabels, "LabelFreightAmount");
                    LabelVATAmount = XmlUtil.GetElementValue(ElementLabels, "LabelVATAmount");
                    LabelCurrencyCode = XmlUtil.GetElementValue(ElementLabels, "LabelCurrencyCode");
                    LabelRowDiscountPercent = XmlUtil.GetElementValue(ElementLabels, "LabelRowDiscountPercent");
                    LabelTotalGrossAmount = XmlUtil.GetElementValue(ElementLabels, "LabelTotalGrossAmount");
                    LabelSellerOrganizationNumber = XmlUtil.GetElementValue(ElementLabels, "LabelSellerOrganizationNumber");
                    LabelSellerVatNumber = XmlUtil.GetElementValue(ElementLabels, "LabelSellerVatNumber");
                    LabelHeadpostalGiro = XmlUtil.GetElementValue(ElementLabels, "LabelHeadpostalGiro");
                    LabelHeadBankGiro = XmlUtil.GetElementValue(ElementLabels, "LabelHeadBankGiro");
                    LabelHeadBankNumber = XmlUtil.GetElementValue(ElementLabels, "LabelHeadBankNumber");
                    LabelSellerPhone = XmlUtil.GetElementValue(ElementLabels, "LabelSellerPhone");
                    LabelSellerFax = XmlUtil.GetElementValue(ElementLabels, "LabelSellerFax");
                    LabelSellerAddress = XmlUtil.GetElementValue(ElementLabels, "LabelSellerAddress");
                    LabelPoweredBySoftOne = XmlUtil.GetElementValue(ElementLabels, "LabelPoweredBySoftOne");
                }

                #endregion
                #region Rows

                if (loadRows)
                    LoadRows();

                #endregion

                #region AccountCodes

                if (loadAccountCodes)
                    LoadAccountCodes();

                #endregion

                #region Postfix

                //Calculate sum if it not is given
                if (this.HeadInvoiceGrossAmount == 0)
                {
                    foreach (var row in this.Rows)
                    {
                        decimal rowAmount = 0;
                        if (row.RowNetAmount > 0)
                            rowAmount = row.RowNetAmount + row.RowVatAmount;
                        else
                            rowAmount = row.RowQuantity * row.RowUnitPrice;

                        this.HeadInvoiceGrossAmount += rowAmount;
                    }
                }

                #endregion

                #endregion
            }*/
        }

        private void Convert(SysEdiMessageHeadDTO dto)
        {
            if (dto == null)
                return;

            #region MessageInfo

            MessageSenderId = dto.MessageSenderId;
            MessageType = dto.MessageType;
            MessageDate = dto.MessageDate;
            MessageImageFileName = "Soe.Edi.Core";
            MessageCultureInfo = "";  // TODO


            #endregion

            #region Seller

            SellerId = dto.SellerId;
            SellerOrganisationNumber = dto.SellerOrganisationNumber;
            SellerVatNumber = dto.SellerVatNumber;
            SellerName = dto.SellerName;
            SellerAddress = dto.SellerAddress;
            SellerPostalCode = dto.SellerPostalCode;
            SellerPostalAddress = dto.SellerPostalAddress;
            SellerCountryCode = dto.SellerCountryCode;
            SellerPhone = dto.SellerPhone;
            SellerFax = dto.SellerFax;
            SellerReference = dto.SellerReference;
            SellerReferencePhone = dto.SellerReferencePhone;

            #endregion

            #region Buyer

            BuyerId = dto.BuyerId;
            BuyerOrganisationNumber = dto.BuyerOrganisationNumber;
            BuyerVatNumber = dto.BuyerVatNumber;
            BuyerName = dto.BuyerName;
            BuyerAddress = dto.BuyerAddress;
            BuyerPostalCode = dto.BuyerPostalCode;
            BuyerPostalAddress = dto.BuyerPostalAddress;
            BuyerCountryCode = dto.BuyerCountryCode;
            BuyerReference = dto.BuyerReference;
            BuyerPhone = dto.BuyerPhone;
            BuyerFax = dto.BuyerFax;
            BuyerEmailAddress = dto.BuyerEmailAddress;
            BuyerDeliveryName = dto.BuyerDeliveryName;
            BuyerDeliveryCoAddress = dto.BuyerDeliveryCoAddress;
            BuyerDeliveryAddress = dto.BuyerDeliveryAddress;
            BuyerDeliveryPostalCode = dto.BuyerDeliveryPostalCode;
            BuyerDeliveryPostalAddress = dto.BuyerDeliveryPostalAddress;
            BuyerDeliveryCountryCode = dto.BuyerDeliveryCountryCode;
            BuyerDeliveryNoteText = dto.BuyerDeliveryNoteText;
            BuyerDeliveryGoodsMarking = dto.BuyerDeliveryGoodsMarking;

            #endregion

            #region Head

            HeadInvoiceNumber = dto.HeadInvoiceNumber;
            HeadInvoiceOcr = dto.HeadInvoiceOcr;

            int headInvoiceType = 0;
            int.TryParse(dto.HeadInvoiceType?.Replace(".", ","), out headInvoiceType);

            HeadInvoiceType = headInvoiceType;
            HeadInvoiceDate = dto.HeadInvoiceDate;
            HeadInvoiceDueDate = dto.HeadInvoiceDueDate;
            HeadDeliveryDate = dto.HeadDeliveryDate;
            HeadBuyerOrderNumber = dto.HeadBuyerOrderNumber;
            HeadBuyerProjectNumber = dto.HeadBuyerProjectNumber;
            HeadBuyerInstallationNumber = dto.HeadBuyerInstallationNumber;
            HeadSellerOrderNumber = dto.HeadSellerOrderNumber;
            HeadPostalGiro = dto.HeadPostalGiro;
            HeadBankGiro = dto.HeadBankGiro;
            HeadBank = dto.HeadBank;
            HeadBicAddress = dto.HeadBicAddress;
            HeadIbanNumber = dto.HeadIbanNumber;
            HeadCurrencyCode = dto.HeadCurrencyCode;
            HeadVatPercentage = dto.HeadVatPercentage.HasValue ? dto.HeadVatPercentage.Value : 0;
            HeadPaymentConditionDays = dto.HeadPaymentConditionDays.HasValue ? dto.HeadPaymentConditionDays.Value : 0;
            HeadPaymentConditionText = dto.HeadPaymentConditionText;

            decimal headInterestPaymentPercent = 0;
            decimal.TryParse(dto.HeadInterestPaymentPercent?.Replace(".", ","), out headInterestPaymentPercent);

            HeadInterestPaymentPercent = headInterestPaymentPercent;
            HeadInterestPaymentText = dto.HeadInterestPaymentText;
            HeadInvoiceGrossAmount = dto.HeadInvoiceGrossAmount;
            HeadInvoiceNetAmount = dto.HeadInvoiceNetAmount;
            HeadVatBasisAmount = dto.HeadVatBasisAmount;
            HeadVatAmount = dto.HeadVatAmount;
            HeadFreightFeeAmount = dto.HeadFreightFeeAmount;
            HeadHandlingChargeFeeAmount = dto.HeadHandlingChargeFeeAmount;
            HeadInsuranceFeeAmount = dto.HeadInsuranceFeeAmount;
            HeadRemainingFeeAmount = dto.HeadRemainingFeeAmount;
            HeadDiscountAmount = dto.HeadDiscountAmount;
            HeadRoundingAmount = dto.HeadRoundingAmount;

            int headInvoiceArrival = 0;
            int.TryParse(dto.HeadInvoiceArrival?.Replace(".", ","), out headInvoiceArrival);

            HeadInvoiceArrival = headInvoiceArrival;
            HeadInvoiceAuthorized = HeadInvoiceAuthorized;
            HeadInvoiceAuthorizedBy = dto.HeadInvoiceAuthorizedBy;


            #endregion

            #region Rows

            if (!dto.SysEdiEdiMessageRowDTOs.IsNullOrEmpty())
            {
                rows = new List<SymbrioEdiRowItem>();

                foreach (var dtoRow in dto.SysEdiEdiMessageRowDTOs)
                {
                    var row = new SymbrioEdiRowItem();

                    decimal unitPrice = 0;

                    decimal.TryParse(dtoRow.RowUnitPrice, out unitPrice);

                    row.RowSellerArticleNumber = dtoRow.RowSellerArticleNumber;
                    row.RowSellerArticleDescription1 = dtoRow.RowSellerArticleDescription1;
                    row.RowSellerArticleDescription2 = dtoRow.RowSellerArticleDescription2;
                    row.RowSellerRowNumber = dtoRow.RowSellerRowNumber;
                    row.RowBuyerArticleNumber = dtoRow.RowBuyerArticleNumber;
                    row.RowBuyerRowNumber = dtoRow.RowBuyerRowNumber;
                    row.RowDeliveryDate = dtoRow.RowDeliveryDate;
                    row.RowBuyerReference = dtoRow.RowBuyerReference;
                    row.RowBuyerObjectId = dtoRow.RowBuyerObjectId;
                    row.ExternalProductId = dtoRow.ExternalProductId;
                    row.RowQuantity = dtoRow.RowQuantity.HasValue ? dtoRow.RowQuantity.Value : 0;
                    row.RowUnitCode = dtoRow.RowUnitCode;
                    row.RowUnitPrice = unitPrice;
                    row.RowDiscountPercent = dtoRow.RowDiscountPercent.HasValue ? dtoRow.RowDiscountPercent.Value : 0;
                    row.RowNetAmount = dtoRow.RowNetAmount.HasValue ? dtoRow.RowNetAmount.Value : 0;
                    row.RowVatAmount = dtoRow.RowVatAmount.HasValue ? dtoRow.RowVatAmount.Value : 0;
                    row.RowVatPercentage = dtoRow.RowVatPercentage.HasValue ? dtoRow.RowVatPercentage.Value : 0;
                    row.StockCode = dtoRow.StockCode;
                    row.ActionCode = (EDIMessageRowActionCode)dtoRow.ActionCode;

                    Rows.Add(row);
                }
            }

            #endregion

            #region Postfix

            //Calculate sum if it not is given
            if (this.HeadInvoiceGrossAmount == 0 && dto.SysEdiEdiMessageRowDTOs != null)
            {
                foreach (var row in dto.SysEdiEdiMessageRowDTOs)
                {
                    decimal rowAmount = 0;
                    decimal unitPrice = 0;
                    decimal.TryParse(row.RowUnitPrice.Replace(".",","), out unitPrice);

                    if (row.RowNetAmount.HasValue)
                        rowAmount = row.RowNetAmount.Value + row.RowVatAmount.GetValueOrDefault(0);
                    else if (row.RowQuantity.HasValue)
                        rowAmount = row.RowQuantity.Value * unitPrice;

                    this.HeadInvoiceGrossAmount += rowAmount;
                }
            }

            #endregion
        }

        #endregion

        #region ISoeImportItem implementation

        public DataSet ToDataSet()
        {
            if (rdm == null)
                rdm = new ReportDataManager(null);

            return rdm.CreateSymbrioEdiData(this);
        }

        private XDocument xdocument = null;
        public XDocument ToXDocument()
        {
            if (xdocument == null)
            {
                if (rdm == null)
                    rdm = new ReportDataManager(null);

                xdocument = rdm.CreateSymbrioEdiDataDocument(this);
            }
            return xdocument;
        }

        #endregion
    }

    public class SymbrioEdiRowItem
    {
        #region Variables

        public string RowSellerArticleNumber { get; set; }
        public string RowSellerArticleDescription1 { get; set; }
        public string RowSellerArticleDescription2 { get; set; }
        public string RowSellerRowNumber { get; set; }
        public string RowBuyerArticleNumber { get; set; }
        public string RowBuyerRowNumber { get; set; }
        public DateTime? RowDeliveryDate { get; set; }
        public string RowBuyerReference { get; set; }
        public string RowBuyerObjectId { get; set; }
        public decimal RowQuantity { get; set; }
        public string RowUnitCode { get; set; }
        public decimal RowUnitPrice { get; set; }
        public decimal RowDiscountPercent { get; set; }
        public decimal RowNetAmount { get; set; }
        public decimal RowVatAmount { get; set; }
        public decimal RowVatPercentage { get; set; }
        public string RowSellerArticleText1 { get; set; }
        public string RowSellerArticleText2 { get; set; }
        public string RowSellerArticleText3 { get; set; }
        public string RowSellerArticleText4 { get; set; }

        public int ExternalProductId { get; set; }
        public string StockCode { get; set; }
        public EDIMessageRowActionCode ActionCode { get; set; }

        #endregion

        public SymbrioEdiRowItem()
        {
        }

        public SymbrioEdiRowItem(XElement row)
        {
            #region Row

            RowSellerArticleNumber = XmlUtil.GetChildElementValue(row, "RowSellerArticleNumber");
            RowSellerArticleDescription1 = XmlUtil.GetChildElementValue(row, "RowSellerArticleDescription1");
            RowSellerArticleDescription2 = XmlUtil.GetChildElementValue(row, "RowSellerArticleDescription2");
            RowSellerRowNumber = XmlUtil.GetChildElementValue(row, "RowSellerRowNumber");
            RowBuyerArticleNumber = XmlUtil.GetChildElementValue(row, "RowBuyerArticleNumber");
            RowBuyerRowNumber = XmlUtil.GetChildElementValue(row, "RowBuyerRowNumber");
            RowDeliveryDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(row, "RowDeliveryDate"));
            RowBuyerReference = XmlUtil.GetChildElementValue(row, "RowBuyerReference");
            RowBuyerObjectId = XmlUtil.GetChildElementValue(row, "RowBuyerObjectId");
            RowQuantity = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowQuantity"), 2);
            RowUnitCode = XmlUtil.GetChildElementValue(row, "RowUnitCode");
            RowUnitPrice = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowUnitPrice"), 2);
            RowDiscountPercent = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowDiscountPercent"), 2);
            RowNetAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowNetAmount"), 2);
            RowVatAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowVatAmount"), 2);
            RowVatPercentage = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowVatPercentage"), 2);
            RowSellerArticleText1 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText1");
            RowSellerArticleText2 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText2");
            RowSellerArticleText3 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText3");
            RowSellerArticleText4 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText4");
            StockCode = XmlUtil.GetChildElementValue(row, "StockCode");
            ActionCode = (EDIMessageRowActionCode)XmlUtil.GetChildElementValueInt(row, "ActionCode", (int)EDIMessageRowActionCode.Unknown);

            #endregion
        }

        public SymbrioEdiRowItem(SysEdiMessageRowDTO row)
        {

            #region Row

            RowSellerArticleNumber = row.RowSellerArticleNumber;
            RowSellerArticleDescription1 = row.RowSellerArticleDescription1;
            RowSellerArticleDescription2 = row.RowSellerArticleDescription2;
            RowSellerRowNumber = row.RowSellerRowNumber;
            RowBuyerArticleNumber = row.RowBuyerArticleNumber;
            RowBuyerRowNumber = row.RowBuyerRowNumber;
            RowDeliveryDate = row.RowDeliveryDate;
            RowBuyerReference = row.RowBuyerReference;
            RowBuyerObjectId = row.RowBuyerObjectId;
            RowQuantity = row.RowQuantity.HasValue ? row.RowQuantity.Value : 0;
            RowUnitCode = row.RowUnitCode;
            RowUnitPrice = 0;//TODO row.RowUnitPrice ? row.RowQuantity.Value : 0;
            RowDiscountPercent = row.RowDiscountPercent.HasValue ? row.RowDiscountPercent.Value : 0;
            RowNetAmount = row.RowNetAmount.HasValue ? row.RowNetAmount.Value : 0;
            RowVatAmount = row.RowVatAmount.HasValue ? row.RowVatAmount.Value : 0;
            RowVatPercentage = row.RowVatPercentage.HasValue ? row.RowVatPercentage.Value : 0;
  //          RowSellerArticleText1 = row.RowSellerArticleText1;
  //          RowSellerArticleText2 = row.RowSellerArticleText2;

            #endregion
        }

        #region Public methods

        public decimal GetPurchaseAmount()
        {
            decimal purchaseAmount = RowNetAmount / (RowQuantity != 0 ? RowQuantity : 1);
            
            if (purchaseAmount == 0)
                purchaseAmount = RowUnitPrice - NumberUtility.MultiplyPercent(RowUnitPrice, RowDiscountPercent);

            return purchaseAmount;
        }

        #endregion
    }

    public class SymbrioEdiRowAccountCode
    {
        #region Variables

        public string AccountCodeAccount { get; set; }
        public string AccountCodeCostCentre { get; set; }
        public string AccountCodeProject { get; set; }
        public decimal AccountCodeBalance { get; set; }
        public decimal AccountCodeQuantity { get; set; }
        public string AccountCodeText { get; set; }

        #endregion

        public SymbrioEdiRowAccountCode(XElement accountCode)
        {
            AccountCodeAccount = XmlUtil.GetChildElementValue(accountCode, "AccountCodeAccount =");
            AccountCodeCostCentre = XmlUtil.GetChildElementValue(accountCode, "AccountCodeCostCentre");
            AccountCodeProject = XmlUtil.GetChildElementValue(accountCode, "AccountCodeProject");
            AccountCodeBalance = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(accountCode, "AccountCodeBalance"), 2);
            AccountCodeQuantity = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(accountCode, "AccountCodeQuantity"), 2);
            AccountCodeText = XmlUtil.GetChildElementValue(accountCode, "AccountCodeText");
        }
    }
}
