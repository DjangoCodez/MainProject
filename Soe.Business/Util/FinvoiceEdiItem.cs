using Soe.Edi.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.Converter;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.Finvoice
{
    public class FinvoiceEdiItem : FinvoiceBase
    {
        #region Variables

        private readonly ReportDataManager rdm;
        private readonly ProjectManager pm;
        private readonly InvoiceManager im;
        private readonly UserManager um;
        private readonly ProductPricelistManager productPricelistManager;
        private readonly InvoiceDistributionManager idm;
        private readonly ReportGenManager rgm;
        private readonly PaymentManager paym;
        private readonly AccountManager am;

        public XDocument xdoc;

        //Soap_envelope doc
        public XDocument xsoapenvdoc;

        private List<FinvoiceEdiRowItem> rows;
        public List<FinvoiceEdiRowItem> Rows
        {
            get
            {
                return rows;
            }
        }
        public XElement Finvoice { get; set; }
        public XElement SellerPartyDetails { get; set; }
        public XElement SellerPostalAddressDetails { get; set; }
        public XElement SellerCommunicationDetails { get; set; }
        public XElement SellerInformationDetails { get; set; }
        public XElement SellerAccountDetails { get; set; }
        public XElement BuyerPartyDetails { get; set; }
        public XElement BuyerPostalAddressDetails { get; set; }
        public XElement BuyerCommunicationDetails { get; set; }
        public XElement DeliveryPartyDetails { get; set; }
        public XElement DeliveryPostalAddressDetails { get; set; }
        public XElement DeliveryDetails { get; set; }
        public XElement InvoiceDetails { get; set; }
        public XElement InvoiceDate { get; set; }
        public XElement InvoiceTotalVatExcludedAmount { get; set; }
        public XElement InvoiceTotalVatAmount { get; set; }
        public XElement InvoiceTotalVatIncludedAmount { get; set; }
        public XElement VatSpecificationDetails { get; set; }
        public XElement VatBaseAmount { get; set; }
        public XElement VatRateAmount { get; set; }
        public XElement PaymentTermsDetails { get; set; }
        public XElement InvoiceDueDate { get; set; }
        public XElement PaymentOverDueFineDetails { get; set; }
        public XElement InvoiceRow { get; set; }
        public XElement DeliveredQuantity { get; set; }
        public XElement UnitPriceAmount { get; set; }
        public XElement RowVatAmount { get; set; }
        public XElement RowVatExcludedAmount { get; set; }
        public XElement EpiDetails { get; set; }
        public XElement EpiIdentificationDetails { get; set; }
        public XElement EpiDate { get; set; }
        public XElement EpiPartyDetails { get; set; }
        public XElement EpiBfiPartyDetails { get; set; }
        public XElement EpiBfiIdentifier { get; set; }
        public XElement EpiBeneficiaryPartyDetails { get; set; }
        public XElement EpiAccountID { get; set; }
        public XElement EpiPaymentInstructionDetails { get; set; }
        public XElement EpiRemittanceInfoIdentifier { get; set; }
        public XElement EpiInstructedAmount { get; set; }
        public XElement EpiCharge { get; set; }
        public XElement EpiDateOptionDate { get; set; }
        public XElement EpiPaymentMeansCode { get; set; }

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
        public string SellerCountryName { get; set; }
        public string SellerPhone { get; set; }
        public string SellerFax { get; set; }
        public string SellerWebAddress { get; set; }
        public string SellerEmailAddress { get; set; }
        public string SellerReference { get; set; }
        public string SellerReferencePhone { get; set; }

        public string BuyerId { get; set; }
        public string BuyerOrganisationNumber { get; set; }
        public string BuyerOrganisationTaxCode { get; set; }
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

        #region Root elements

        public string TimestampStr { get; set; }
        public string SellerOrganisationUnitNumber { get; set; }//Org.nr
        public string SellerIban { get; set; }
        public string SellerBic { get; set; }
        public string SellerContactPersonName { get; set; }
        public string BuyerContactPersonName { get; set; }
        public string BuyerOrganisationUnitNumber { get; set; }
        public string DeliverySiteCode { get; set; }
        public bool AddAttachmentMessageDetails { get; set; }
        public readonly MessageTransmissionDetails MessageTransmissionDetails = new MessageTransmissionDetails();
        public SellerPartyDetails sellerPartyDetails = new SellerPartyDetails();
        public SellerCommunicationDetails sellerCommunicationDetails = new SellerCommunicationDetails();
        public SellerInformationDetails sellerInformationDetails = new SellerInformationDetails();
        public BuyerPartyDetails buyerPartyDetails = new BuyerPartyDetails();
        public BuyerCommunicationDetails buyerCommunicationDetails = new BuyerCommunicationDetails();
        public DeliveryPartyDetails deliveryPartyDetails = new DeliveryPartyDetails();
        public DeliveryDetails deliveryDetails = new DeliveryDetails();
        public InvoiceDetails invoiceDetails = new InvoiceDetails();
        public List<InvoiceRow> invoiceRows = new List<InvoiceRow>();
        public SpecificationDetails specificationDetails = new SpecificationDetails();
        public EpiDetails epiDetails = new EpiDetails();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for parsing finvoice (Import).
        /// </summary>
        /// <param name="xml"></param>
        public FinvoiceEdiItem(XDocument xml, ParameterObject parameterObject)
        {
            rdm = new ReportDataManager(parameterObject);
            pm = new ProjectManager(parameterObject);
            im = new InvoiceManager(parameterObject);
            um = new UserManager(parameterObject);
            rgm = new ReportGenManager(parameterObject);
            paym = new PaymentManager(parameterObject);
            productPricelistManager = new ProductPricelistManager(parameterObject);
            idm = new InvoiceDistributionManager(parameterObject);
            this.xdoc = xml;
            Parse();
        }

        /// <summary>
        /// Constructor for preparing for creation of finvoice (Export).
        /// Call ToXml to create the file.
        /// </summary>        
        public FinvoiceEdiItem(ParameterObject parameterObject, CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> invoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, string exemptionReason, bool original, bool printService, string defaultInvoiceText, TermGroup_EInvoiceFormat eInvoiceFormat)
        {

            rdm = new ReportDataManager(parameterObject);
            pm = new ProjectManager(parameterObject);
            im = new InvoiceManager(parameterObject);
            um = new UserManager(parameterObject);
            rgm = new ReportGenManager(parameterObject);
            paym = new PaymentManager(parameterObject);
            am = new AccountManager(parameterObject);
            productPricelistManager = new ProductPricelistManager(parameterObject);

            Populate(invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddress, customerDeliveryAddress, customerContactEcoms, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason, original, printService, defaultInvoiceText, eInvoiceFormat);
        }

        public FinvoiceEdiItem(XDocument xdoc, string fileName, int ediEntryType, bool loadRows, bool loadAccountCodes, ParameterObject parameterObject, SysEdiMessageHeadDTO sysEdiMessageHeadDTO = null)
        {
            rdm = new ReportDataManager(parameterObject);
            pm = new ProjectManager(parameterObject);
            im = new InvoiceManager(parameterObject);
            um = new UserManager(parameterObject);
            rgm = new ReportGenManager(parameterObject);
            paym = new PaymentManager(parameterObject);
            productPricelistManager = new ProductPricelistManager(parameterObject);

            this.xdoc = xdoc;
            this.rows = new List<FinvoiceEdiRowItem>();

            if (sysEdiMessageHeadDTO == null)
                Parse(loadRows, loadAccountCodes, ediEntryType);
            else
                Convert(sysEdiMessageHeadDTO);
        }

        #endregion

        #region Import

        private void Parse()
        {
            if (xdoc == null)
                return;

            #region Parse

            #region BuyerPartyDetails

            BuyerPartyDetails = XmlUtil.GetChildElement(xdoc, "BuyerPartyDetails");

            if (BuyerPartyDetails != null)
            {
                BuyerOrganisationNumber = XmlUtil.GetChildElementValue(BuyerPartyDetails, "BuyerPartyIdentifier")?.Trim();
                BuyerOrganisationTaxCode = XmlUtil.GetChildElementValue(BuyerPartyDetails, "BuyerOrganisationTaxCode")?.Trim();
            }
            #endregion

            #region SellerPartyDetails

            XElement sellerPartyDetailsElement = XmlUtil.GetChildElement(xdoc, "SellerPartyDetails");
            sellerPartyDetails.ParseNode(sellerPartyDetailsElement);

            SellerOrganisationUnitNumber = XmlUtil.GetChildElementValue(xdoc.Root, "SellerOrganisationUnitNumber");

            DeliverySiteCode = XmlUtil.GetChildElementValue(xdoc.Root, "DeliverySiteCode");

            #endregion

            #region SellerInformationDetails

            SellerInformationDetails = XmlUtil.GetChildElement(xdoc, "SellerInformationDetails");

            if (SellerInformationDetails != null)
            {
                var SellerAccountDetails = (from e in SellerInformationDetails.Elements()
                                            where e.Name.LocalName == "SellerAccountDetails"
                                            select e).FirstOrDefault();


                SellerIban = XmlUtil.GetChildElementValue(SellerAccountDetails, "SellerAccountID");
                SellerBic = XmlUtil.GetChildElementValue(SellerAccountDetails, "SellerBic");
            }

            #endregion

            #region InvoiceDetails

            XElement invoiceDetailsElement = XmlUtil.GetChildElement(xdoc, "InvoiceDetails");
            invoiceDetails.ParseNode(invoiceDetailsElement);

            #endregion

            #region InvoiceRows

            List<XElement> invoiceRowElements = XmlUtil.GetChildElements(xdoc, "InvoiceRow");
            foreach (XElement rowElement in invoiceRowElements)
            {
                InvoiceRow row = new InvoiceRow();
                row.ParseNode(rowElement);
                invoiceRows.Add(row);
            }

            #endregion

            #region EpiDetails

            XElement epiDetailsElement = XmlUtil.GetChildElement(xdoc, "EpiDetails");
            epiDetails.ParseNode(epiDetailsElement);
            HeadInvoiceDueDate = epiDetails.EpiDateOptionDate;

            #endregion

            #region MessageTransmissionDetails

            var messageTransmissionDetailsXML = XmlUtil.GetChildElement(xdoc, "MessageTransmissionDetails");
            var messageDetailsXML = XmlUtil.GetChildElement(messageTransmissionDetailsXML, "MessageDetails");
            MessageTransmissionDetails.MessageIdentifier = XmlUtil.GetChildElementValue(messageDetailsXML, "MessageIdentifier")?.Trim();
            MessageTransmissionDetails.SpecificationIdentifier = XmlUtil.GetChildElementValue(messageDetailsXML, "SpecificationIdentifier")?.Trim();

            #endregion

            #endregion
        }

        #endregion

        #region Export

        private void Populate(CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> customerInvoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, string exemptionReason, bool original, bool printService, string defaultInvoiceText, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            string currency = (invoiceCurrency != null && !string.IsNullOrEmpty(invoiceCurrency.Code)) ? invoiceCurrency.Code : string.Empty;
            string bic = string.Empty;
            string iban = string.Empty;
            decimal vatRateMain = am.GetDefaultBillingVatCode(company.ActorCompanyId)?.Percent ?? 25.5M;
            bool finvoice_v2 = eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice2;
            bool finvoice_v3 = eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice3;
            MessageTransmissionDetails.SpecificationIdentifier = "EN16931";
            TimestampStr = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH':'mm':'ss");
            SettingManager sm = new SettingManager(null);
            string SenderAddress = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceAddress, 0, company.ActorCompanyId, 0);
            string SenderOperator = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceOperator, 0, company.ActorCompanyId, 0);
            MessageTransmissionDetails.FromIdentifier = SenderAddress;
            MessageTransmissionDetails.FromIntermediator = SenderOperator;
            MessageTransmissionDetails.ToIdentifier = customer.FinvoiceAddress;
            MessageTransmissionDetails.ToIntermediator = customer.FinvoiceOperator;
            MessageTransmissionDetails.MessageIdentifier = GetMessageIdentifier(invoice, eInvoiceFormat);
            MessageTransmissionDetails.MessageTimeStamp = TimestampStr;
            im.SetPriceListTypeInclusiveVat(invoice, company.ActorCompanyId);
            PriceListType priceListType = productPricelistManager.GetPriceListType(invoice.PriceListTypeId != null ? (int)invoice.PriceListTypeId : 0, company.ActorCompanyId);

            if (paymentInformation != null)
            {
                PaymentInformationRow paymentRow;

                // Get all tagged as ShowInInvoice
                var paymentInformationRows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true);

                // first try getting paymentinformationrow having default syspaymenttypeid and which is selected as default
                paymentRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == paymentInformation.DefaultSysPaymentTypeId && i.Default);

                // then try getting paymentinformationrow having default syspaymenttypeid
                if (paymentRow == null)
                    paymentRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == paymentInformation.DefaultSysPaymentTypeId);

                // then try getting paymentinformationrow having paymenttype BIC or SEPA, and which is selected as default
                if (paymentRow == null)
                    paymentRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => (i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC || i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.SEPA) && i.Default);

                // last try getting paymentinformationrow having paymenttype BIC or SEPA
                if (paymentRow == null)
                    paymentRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => (i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC || i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.SEPA));

                if (paymentRow != null)
                {
                    paymentInformationRows = paymentInformationRows.Where(p => p.PaymentInformationRowId != paymentRow.PaymentInformationRowId);

                    if (paymentRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
                    {
                        string[] bicAndIBAN = this.GetBicAndIban(paymentRow);
                        if (bicAndIBAN.Length == 2)
                        {
                            if (bic == string.Empty && iban == string.Empty)
                            {
                                bic = bicAndIBAN[0].Trim();
                                iban = bicAndIBAN[1].Trim();
                            }

                            sellerInformationDetails.SellerBicAndIbans.Add(new Tuple<string, string>(bicAndIBAN[0].Trim(), bicAndIBAN[1].Trim()));
                        }
                    }
                    else if (paymentRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.SEPA)
                    {
                        if (bic == string.Empty && iban == string.Empty)
                        {
                            iban = paymentRow.PaymentNr;
                            bic = paymentRow.BIC.Trim() != String.Empty ? paymentRow.BIC : paym.GetBicFromIban(iban);
                        }

                        sellerInformationDetails.SellerBicAndIbans.Add(new Tuple<string, string>(paymentRow.BIC.Trim() != String.Empty ? paymentRow.BIC : paym.GetBicFromIban(paymentRow.PaymentNr), paymentRow.PaymentNr));
                    }

                    foreach (var row in paymentInformationRows)
                    {
                        if (row.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
                        {
                            string[] bicAndIBAN = this.GetBicAndIban(row);
                            if (bicAndIBAN.Length == 2)
                            {
                                if (bic == string.Empty && iban == string.Empty)
                                {
                                    bic = bicAndIBAN[0].Trim();
                                    iban = bicAndIBAN[1].Trim();
                                }

                                sellerInformationDetails.SellerBicAndIbans.Add(new Tuple<string, string>(bicAndIBAN[0].Trim(), bicAndIBAN[1].Trim()));
                            }
                        }
                        else if (row.SysPaymentTypeId == (int)TermGroup_SysPaymentType.SEPA)
                        {
                            if (bic == string.Empty && iban == string.Empty)
                            {
                                iban = row.PaymentNr;
                                bic = bic = row.BIC.Trim() != String.Empty ? row.BIC : paym.GetBicFromIban(iban);
                            }

                            sellerInformationDetails.SellerBicAndIbans.Add(new Tuple<string, string>(row.BIC.Trim() != String.Empty ? row.BIC : paym.GetBicFromIban(row.PaymentNr), row.PaymentNr));
                        }
                    }
                }
                else
                {
                    foreach (var row in paymentInformationRows)
                    {
                        if (row.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
                        {
                            string[] bicAndIBAN = this.GetBicAndIban(row);
                            if (bicAndIBAN.Length == 2)
                            {
                                if (bic == string.Empty && iban == string.Empty)
                                {
                                    bic = bicAndIBAN[0].Trim();
                                    iban = bicAndIBAN[1].Trim();
                                }

                                sellerInformationDetails.SellerBicAndIbans.Add(new Tuple<string, string>(bicAndIBAN[0].Trim(), bicAndIBAN[1].Trim()));
                            }
                        }
                        else if (row.SysPaymentTypeId == (int)TermGroup_SysPaymentType.SEPA)
                        {
                            if (bic == string.Empty && iban == string.Empty)
                            {
                                iban = row.PaymentNr;
                                bic = bic = row.BIC.Trim() != String.Empty ? row.BIC : paym.GetBicFromIban(iban);
                            }

                            sellerInformationDetails.SellerBicAndIbans.Add(new Tuple<string, string>(row.BIC.Trim() != String.Empty ? row.BIC : paym.GetBicFromIban(row.PaymentNr), row.PaymentNr));
                        }
                    }
                }
            }

            SellerOrganisationUnitNumber = !string.IsNullOrEmpty(company.OrgNr) ? company.OrgNr : string.Empty;
            if (!string.IsNullOrEmpty(SellerOrganisationUnitNumber))
            {
                //Has to be in SFS-format
                SellerOrganisationUnitNumber = "0037" + SellerOrganisationUnitNumber.Replace("-", "");
            }

            bool isInUsersDict = false;
            if (!string.IsNullOrEmpty(invoice.ReferenceOur))
            {
                Dictionary<int, string> usersDict = um.GetUsersByCompanyDict(company.ActorCompanyId, 0, 0, false, false, true, false);
                isInUsersDict = usersDict.Any(i => i.Value == invoice.ReferenceOur);
            }

            SellerContactPersonName = isInUsersDict && !string.IsNullOrEmpty(invoice.ReferenceOur) ? invoice.ReferenceOur : string.Empty;
            BuyerContactPersonName = !string.IsNullOrEmpty(invoice.ReferenceYour) ? invoice.ReferenceYour : string.Empty;

            #region Seller

            ContactAddressRow companyPostalCode = companyAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow companyPostalAddress = companyAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow companyAddressStreetName = companyAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

            //ContactECom
            ContactECom companyEmail = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
            ContactECom companyPhoneWork = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
            ContactECom companyFax = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);
            ContactECom companyWeb = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Web);

            #region SellerPartyDetails

            //Maks 35 chars, can be many
            sellerPartyDetails.SellerOrganisationName = company.Name;
            sellerPartyDetails.SellerOrganisationTaxCode = !String.IsNullOrEmpty(company.VatNr) ? company.VatNr : String.Empty;
            sellerPartyDetails.SellerPartyIdentifier = !String.IsNullOrEmpty(company.OrgNr) ? company.OrgNr : String.Empty;

            sellerPartyDetails.SellerStreetName = companyAddressStreetName != null && !String.IsNullOrEmpty(companyAddressStreetName.Text) ? companyAddressStreetName.Text : String.Empty;
            sellerPartyDetails.SellerTownName = companyPostalAddress != null && !String.IsNullOrEmpty(companyPostalAddress.Text) ? companyPostalAddress.Text : String.Empty;
            sellerPartyDetails.SellerPostCodeIdentifier = companyPostalCode != null && !String.IsNullOrEmpty(companyPostalCode.Text) ? companyPostalCode.Text : String.Empty;
            sellerPartyDetails.CountryCode = "FI"; //TODO should not be hardcoded
            sellerPartyDetails.CountryName = "FINLAND"; //TODO should not be hardcoded

            #endregion

            #region SellerCommunicationDetails

            sellerCommunicationDetails.SellerPhoneNumberIdentifier = companyPhoneWork != null && !String.IsNullOrEmpty(companyPhoneWork.Text) ? companyPhoneWork.Text : String.Empty;
            sellerCommunicationDetails.SellerEmailaddressIdentifier = companyEmail != null && !String.IsNullOrEmpty(companyEmail.Text) ? companyEmail.Text : String.Empty;

            #endregion

            #region SellerInformationDetails

            sellerInformationDetails.SellerHomeTownName = companyPostalAddress != null && !String.IsNullOrEmpty(companyPostalAddress.Text) ? companyPostalAddress.Text : String.Empty;
            sellerInformationDetails.SellerVatRegistrationText = !String.IsNullOrEmpty(exemptionReason) ? exemptionReason : String.Empty;
            sellerInformationDetails.SellerPhoneNumber = companyPhoneWork != null && !String.IsNullOrEmpty(companyPhoneWork.Text) ? companyPhoneWork.Text : String.Empty;
            sellerInformationDetails.SellerFaxNumber = companyFax != null && !String.IsNullOrEmpty(companyFax.Text) ? companyFax.Text : String.Empty;
            sellerInformationDetails.SellerWebaddressIdentifier = companyWeb != null && !String.IsNullOrEmpty(companyWeb.Text) ? companyWeb.Text : String.Empty;

            //Account
            //sellerInformationDetails.SellerIBAN = iban;
            //sellerInformationDetails.SellerBIC = bic;
            //sellerInformationDetails.SellerIBAN = iban != null && !String.IsNullOrEmpty(iban) ? iban : "NA";
            //sellerInformationDetails.SellerBIC = bic != null && !String.IsNullOrEmpty(bic) ? bic : "XXXXXXXX";
            #endregion

            #endregion

            #region BuyerPartyDetails (Customer)

            ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);
            ContactAddressRow customerCoAddressName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.AddressCO);

            //ContactECom
            ContactECom customerEmail = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
            ContactECom customerPhoneWork = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
            //Maks 35 chars, can be many
            string customerName = customer.Name.Replace("\x0D0A", ";");
            customerName = customerName.Replace("\x0D", ";");
            customerName = customerName.Replace("\x0A", ";");
            buyerPartyDetails.BuyerOrganisationName = customerName;
            buyerPartyDetails.BuyerOrganisationTaxCode = !String.IsNullOrEmpty(customer.VatNr) ? customer.VatNr : String.Empty;
            if (printService)
                buyerPartyDetails.BuyerPartyIdentifier = !String.IsNullOrEmpty(customer.CustomerNr) ? customer.CustomerNr : String.Empty;
            else
                buyerPartyDetails.BuyerPartyIdentifier = !String.IsNullOrEmpty(customer.OrgNr) ? customer.OrgNr : String.Empty;

            if (!string.IsNullOrEmpty(invoice.BillingAdressText))
            {
                string[] separators = { Environment.NewLine, "\n", "\r" };
                string[] address = invoice.BillingAdressText.Split(separators, StringSplitOptions.None);

                if (address.Count() >= 2) { buyerPartyDetails.BuyerStreetName.Add(address[1]); }
                if (address.Count() >= 3) { buyerPartyDetails.BuyerPostCodeIdentifier = address[2]; } else { buyerPartyDetails.BuyerPostCodeIdentifier = ""; }
                if (address.Count() >= 4) { buyerPartyDetails.BuyerTownName = address[3]; } else { buyerPartyDetails.BuyerTownName = ""; }
            }
            else
            {
                if (customerCoAddressName != null && !string.IsNullOrEmpty(customerCoAddressName.Text))
                {
                    buyerPartyDetails.BuyerStreetName.Add(customerCoAddressName.Text);
                }
                buyerPartyDetails.BuyerStreetName.Add(customerAddressStreetName != null && !String.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : string.Empty);
                buyerPartyDetails.BuyerTownName = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : string.Empty;
                buyerPartyDetails.BuyerPostCodeIdentifier = customerPostalCode != null && !String.IsNullOrEmpty(customerPostalCode.Text) ? customerPostalCode.Text : string.Empty;
            }

            buyerPartyDetails.CountryCode = "FI"; //TODO should not be hardcoded
            buyerPartyDetails.CountryName = "FINLAND"; //TODO should not be hardcoded                      

            BuyerOrganisationUnitNumber = String.IsNullOrEmpty(customer.OrgNr) ? String.Empty : "0037" + customer.OrgNr.Replace("-", "") + customer.DepartmentNr;

            #endregion

            #region BuyerCommunicationDetails

            //Todo contactecom for customer
            buyerCommunicationDetails.BuyerPhoneNumberIdentifier = customerPhoneWork != null && !String.IsNullOrEmpty(customerPhoneWork.Text) ? customerPhoneWork.Text : String.Empty;
            buyerCommunicationDetails.BuyerEmailaddressIdentifier = customerEmail != null && !String.IsNullOrEmpty(customerEmail.Text) ? customerEmail.Text : String.Empty;

            #endregion

            #region DeliveryPartyDetails
            //This can only be if all the latter is found: name, address,postalcode,postaladdress. Otherwise not valid material
            ContactAddressRow deliveryPostalCode = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow deliveryPostalAddress = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow deliveryAddress = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

            //If delivery text is filled, then invoiceHeadText gets the value (Try to get delivery data from there)
            if (invoice.InvoiceHeadText.HasValue())
            {

                if (invoice.InvoiceHeadText.Length > 0)
                {
                    string[] separators = { Environment.NewLine, "\n", "\r", "\r\n" };
                    string[] address = invoice.InvoiceHeadText.Split(separators, StringSplitOptions.None);
                    string deliveryOrganisationName = address[0].Replace("\x0D0A", " ");
                    deliveryOrganisationName = deliveryOrganisationName.Replace("\x0D", " ");
                    deliveryOrganisationName = deliveryOrganisationName.Replace("\x0A", " ");
                    if (address.Any()) { deliveryPartyDetails.DeliveryOrganisationName = deliveryOrganisationName; }
                    if (address.Count() >= 2) { deliveryPartyDetails.DeliveryStreetName = address[1]; }
                    if (address.Count() >= 3) { deliveryPartyDetails.DeliveryPostCodeIdentifier = address[2]; }
                    if (address.Count() >= 4) { deliveryPartyDetails.DeliveryTownName = address[3]; }
                }

                if (deliveryPartyDetails.DeliveryOrganisationName.Length < 3)
                    deliveryPartyDetails.DeliveryOrganisationName = buyerPartyDetails.BuyerOrganisationName;

                deliveryPartyDetails.CountryCode = "FI"; // TODO should not be hardcoded
                deliveryPartyDetails.CountryName = "FINLAND"; // TODO should not be hardcoded          

            }
            else
            {
                //if (customer.Name != null && deliveryPostalAddress != null && deliveryPostalCode != null && deliveryAddress != null)
                //{
                //if (!String.IsNullOrEmpty(deliveryPostalAddress.Text) && !String.IsNullOrEmpty(deliveryPostalCode.Text) && !String.IsNullOrEmpty(deliveryAddress.Text) && !String.IsNullOrEmpty(customer.Name))
                // {
                //Maks 35 chars, can be many
                deliveryPartyDetails.DeliveryOrganisationName = buyerPartyDetails.BuyerOrganisationName;
                deliveryPartyDetails.DeliveryOrganisationTaxCode = buyerPartyDetails.BuyerOrganisationTaxCode;
                deliveryPartyDetails.DeliveryPartyIdentifier = buyerPartyDetails.BuyerPartyIdentifier;

                deliveryPartyDetails.DeliveryTownName = (deliveryPostalAddress != null && !String.IsNullOrEmpty(deliveryPostalAddress.Text)) ? deliveryPostalAddress.Text : String.Empty;
                deliveryPartyDetails.DeliveryPostCodeIdentifier = (deliveryPostalCode != null && !String.IsNullOrEmpty(deliveryPostalCode.Text)) ? deliveryPostalCode.Text : String.Empty;
                deliveryPartyDetails.DeliveryStreetName = (deliveryAddress != null && !String.IsNullOrEmpty(deliveryAddress.Text)) ? deliveryAddress.Text : String.Empty;

                deliveryPartyDetails.CountryCode = "FI"; // TODO should not be hardcoded
                deliveryPartyDetails.CountryName = "FINLAND"; // TODO should not be hardcoded          
                                                              // }
                                                              // }
            }
            #endregion

            #region DeliveryDetails

            if (invoice.DeliveryDate.HasValue)
                deliveryDetails.DeliveryDate = invoice.DeliveryDate.Value;

            deliveryDetails.DeliveryMethodText = (invoice.DeliveryType != null && !String.IsNullOrEmpty(invoice.DeliveryType.Name)) ? invoice.DeliveryType.Name : String.Empty;
            deliveryDetails.DeliveryTermsText = (invoice.DeliveryCondition != null && !String.IsNullOrEmpty(invoice.DeliveryCondition.Name)) ? invoice.DeliveryCondition.Name : String.Empty;

            #endregion

            #region InvoiceDetails

            if (invoice.IsCredit)
            {
                invoiceDetails.InvoiceTypeCode = FINVOICE_TYPE_CODE_CREDIT;
                invoiceDetails.InvoiceTypeText = FINVOICE_TYPE_TEXT_CREDIT;
                invoiceDetails.InvoiceTypeCodeUN = "381";
            }
            else
            {
                invoiceDetails.InvoiceTypeCode = FINVOICE_TYPE_CODE_DEBIT;
                invoiceDetails.InvoiceTypeText = FINVOICE_TYPE_TEXT_DEBIT;
                invoiceDetails.InvoiceTypeCodeUN = "380";
            }

            if (original)
            {
                invoiceDetails.OriginCode = FINVOICE_ORIGIN_CODE_ORIGINAL;
            }
            else
            {
                invoiceDetails.OriginCode = FINVOICE_ORIGIN_CODE_COPY;
                invoiceDetails.OriginText = FINVOICE_ORIGIN_TEXT_COPY;
            }

            invoiceDetails.InvoiceNumber = invoice.InvoiceNr;
            invoiceDetails.InvoiceId = invoice.InvoiceId.ToString();
            invoiceDetails.InvoiceDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.Date : DateTime.Today;

            var originalInvoices = im.GetInvoiceTraceViews(invoice.InvoiceId, 0);

            invoiceDetails.OriginalInvoiceNumber = string.Join(",", originalInvoices.Where(i => i.IsInvoice).Select(x => x.Number));
            invoiceDetails.SellerReferenceIdentifier = originalInvoices.FirstOrDefault(x => x.IsOrder)?.Number;

            if (string.IsNullOrEmpty(invoiceDetails.SellerReferenceIdentifier) && !string.IsNullOrEmpty(invoice.ReferenceOur) && !isInUsersDict)
            {
                invoiceDetails.SellerReferenceIdentifier = invoice.ReferenceOur.Trim();
            }

            invoiceDetails.SellersBuyerIdentifier = invoice.ActorNr.Length > 35 ? invoice.ActorNr.Substring(0, 35) : invoice.ActorNr;

            if (invoice.InvoiceFee != 0)
            {
                invoiceDetails.InvoiceTotalVatExcludedAmount = invoice.SumAmount + invoice.InvoiceFee;
                invoiceDetails.RowsTotalVatExcludedAmount = invoice.SumAmount + invoice.InvoiceFee;
            }
            else
            {
                invoiceDetails.InvoiceTotalVatExcludedAmount = invoice.SumAmount;
                invoiceDetails.RowsTotalVatExcludedAmount = invoice.SumAmount;
            }

            if (invoice.PriceListTypeInclusiveVat)
            {
                invoiceDetails.InvoiceTotalVatExcludedAmount -= invoice.VATAmount;
                invoiceDetails.RowsTotalVatExcludedAmount -= invoice.VATAmount;
            }

            if (invoice.FreightAmount != 0)
            {
                invoiceDetails.InvoiceTotalVatExcludedAmount += invoice.FreightAmount;
                invoiceDetails.RowsTotalVatExcludedAmount += invoice.FreightAmount;
            }

            invoiceDetails.VatExcludedAmountCurrencyIdentifier = currency;
            invoiceDetails.InvoiceTotalVatIncludedAmount = invoice.TotalAmount;
            invoiceDetails.VatIncludedAmountCurrencyIdentifier = currency;
            invoiceDetails.InvoiceTotalVatAmount = invoice.VATAmount;
            invoiceDetails.VatAmountCurrencyIdentifier = currency;

            var finvoiceInvoiceLabelToOrderIdentifier = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceInvoiceLabelToOrderIdentifier, 0, company.ActorCompanyId, 0);
            if (originalInvoices.Any(i => i.IsOrder) || !finvoiceInvoiceLabelToOrderIdentifier)
            {
                if (!string.IsNullOrEmpty(invoice.OrderReference))
                {
                    invoiceDetails.OrderIdentifier = invoice.OrderReference.Trim();
                }
                if (!string.IsNullOrEmpty(invoice.InvoiceLabel))
                {
                    invoiceDetails.FreeTexts.Add(invoice.InvoiceLabel.Trim());
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(invoice.InvoiceLabel))
                {
                    invoiceDetails.OrderIdentifier = invoice.InvoiceLabel.Trim();
                }
                else if (!string.IsNullOrEmpty(invoice.OrderReference))
                {
                    invoiceDetails.OrderIdentifier = invoice.OrderReference.Trim();
                }
            }

            if (!string.IsNullOrEmpty(invoice.ContractNr))
            {
                invoiceDetails.AgreementIdentifier = invoice.ContractNr.Trim();
            }

            if (!string.IsNullOrEmpty(invoice.WorkingDescription))
            {
                invoiceDetails.FreeTexts.Add(invoice.WorkingDescription.Trim());
            }
            if (!string.IsNullOrEmpty(invoice.InvoiceText))
            {
                invoiceDetails.FreeTexts.Add(invoice.InvoiceText.Trim());
            }
            else if (!string.IsNullOrEmpty(defaultInvoiceText))
            {
                /*defaultInvoiceText = defaultInvoiceText.Replace("\x0D0A", " ");
                defaultInvoiceText = defaultInvoiceText.Replace("\x0D", " ");
                defaultInvoiceText = defaultInvoiceText.Replace("\x0A", " ");*/
                invoiceDetails.FreeTexts.Add(defaultInvoiceText.Trim());
            }

            //invoiceDetails
            //VatText
            if ((int)TermGroup_InvoiceVatType.Contractor == invoice.VatType)
                invoiceDetails.VatText = "Käännetty verovelvollisuus. Ostaja verovelvollinen AVL 8c §";

            #region VatSpecificationDetails
            List<CustomerInvoiceRow> rowsForGrouping = customerInvoiceRows.Where(i => i.Type != (int)SoeInvoiceRowType.TextRow).ToList();
            List<IGrouping<decimal, CustomerInvoiceRow>> invoiceRowsGroupedByVatRates = rowsForGrouping.GroupBy(g => g.VatRate).ToList();
            bool first = true;
            foreach (IGrouping<decimal, CustomerInvoiceRow> invoiceRowsGroupByVatRate in invoiceRowsGroupedByVatRates)
            {
                CustomerInvoiceRow row = invoiceRowsGroupByVatRate.FirstOrDefault();
                if (row == null)
                    continue;

                decimal vatRate = row.VatRate;
                //VatCode
                //             if ((int)TermGroup_InvoiceVatType.Contractor == invoice.VatType)
                //                 invoiceDetails.v;
                //             string vatCode = !String.IsNullOrEmpty(row.VatCode.Code) ? row.VatCode.Code : String.Empty;
                decimal taxableAmount = 0;
                decimal taxAmount = 0;
                if (first)
                    vatRateMain = row.VatRate;

                foreach (var invoiceRow in invoiceRowsGroupByVatRate)
                {
                    taxableAmount += invoice.PriceListTypeInclusiveVat ? invoiceRow.SumAmount - invoiceRow.VatAmount : invoiceRow.SumAmount;
                    taxAmount += invoiceRow.VatAmount;
                }

                /*if (invoice.InvoiceFee > 0 && first)
                {
                    taxableAmount += invoice.InvoiceFee;
                    taxAmount += (invoice.InvoiceFee * (vatRateMain / 100));
                }*/

                VatSpecificationDetails vatSpecificationDetails = new VatSpecificationDetails();
                vatSpecificationDetails.VatBaseAmount = taxableAmount;
                vatSpecificationDetails.VatRateAmount = Decimal.Round(taxAmount, 2);
                vatSpecificationDetails.VatBaseAmountCurrencyIdentifier = currency;
                vatSpecificationDetails.VatRateAmountCurrencyIdentifier = currency;
                vatSpecificationDetails.VatRatePercent = vatRate;
                if ((int)TermGroup_InvoiceVatType.Contractor == invoice.VatType)
                    vatSpecificationDetails.VatFreeText = "Käännetty verovelvollisuus. Ostaja verovelvollinen AVL 8c §";
                if ((int)TermGroup_InvoiceVatType.Contractor == invoice.VatType)
                    vatSpecificationDetails.VatCode = "AE";
                else if ((int)TermGroup_InvoiceVatType.NoVat == invoice.VatType)
                    vatSpecificationDetails.VatCode = "Z";
                else if (vatRate == 0)
                    vatSpecificationDetails.VatCode = "O";
                else
                    vatSpecificationDetails.VatCode = "S";

                invoiceDetails.VatSpecificationDetails.Add(vatSpecificationDetails);
                first = false;

            }

            #region DefinitionDetails

            string worksitekey = "", worksitenumber = "";

            if (invoice.ProjectId.HasValue)
            {
                var project = pm.GetProject(invoice.ProjectId.GetValueOrDefault(), false);
                worksitekey = project.WorkSiteKey;
                worksitenumber = project.WorkSiteNumber;
            }

            //worksitekey            
            DefinitionDetails definitionDetailsTA0001 = new DefinitionDetails();
            definitionDetailsTA0001.DefinitionHeaderText = "Työmaa-avain";
            definitionDetailsTA0001.DefinitionCode = "TA0001";
            definitionDetailsTA0001.DefinitionValue = worksitekey;
            invoiceDetails.DefinitionDetails.Add(definitionDetailsTA0001);

            //worksitenumber            
            DefinitionDetails definitionDetailsTA0002 = new DefinitionDetails();
            definitionDetailsTA0002.DefinitionHeaderText = "Työmaanumero";
            definitionDetailsTA0002.DefinitionCode = "TA0002";
            definitionDetailsTA0002.DefinitionValue = worksitenumber;
            invoiceDetails.DefinitionDetails.Add(definitionDetailsTA0002);

            #endregion


            #endregion

            #region PaymentTermsDetails

            var paymentTerm = new PaymentTermsDetails();
            paymentTerm.PaymentTermsFreeText = (invoice.PaymentCondition != null && !string.IsNullOrEmpty(invoice.PaymentCondition.Name)) ? invoice.PaymentCondition.Name : string.Empty;
            if (invoice.DueDate.HasValue)
                paymentTerm.InvoiceDueDate = invoice.DueDate.Value;

            paymentTerm.PaymentOverDueFinePercent = sm.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInterestPercent, 0, company.ActorCompanyId, 0);
            paymentTerm.PaymentOverDueFixedAmountCurrencyIdentifier = currency;

            //if (invoice.TimeDiscountPercent.HasValue && invoice.TimeDiscountDate.HasValue)
            if (invoice.PaymentCondition != null && invoice.PaymentCondition.DiscountPercent.GetValueOrDefault() > 0 && invoice.PaymentCondition.DiscountDays.HasValue)
            {
                paymentTerm.CashDiscountDate = invoice.InvoiceDate.Value.AddDays(invoice.PaymentCondition.DiscountDays.Value);
                paymentTerm.CashDiscountPercent = Math.Round(invoice.PaymentCondition.DiscountPercent.Value, 2);
                paymentTerm.CashDiscountAmount = Math.Round(invoice.TotalAmount * (paymentTerm.CashDiscountPercent.Value / 100), 2);
            }

            invoiceDetails.PaymentTerms.Add(paymentTerm);

            #endregion

            #endregion

            #region InvoiceRows

            foreach (var customerInvoiceRow in customerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.ProductRow || i.Type == (int)SoeInvoiceRowType.TextRow).OrderBy(r => r.RowNr))
            {
                invoiceRows.Add(CreateFinvoiceRow(invoice, customerInvoiceRow, priceListType, eInvoiceFormat, vatRateMain, currency, printService, customerInvoiceRow.RowNr));
            }

            var invoiceFeeRow = customerInvoiceRows.FirstOrDefault(i => i.IsInvoiceFeeRow);
            if (invoiceFeeRow != null)
            {
                invoiceRows.Add(CreateFinvoiceRow(invoice, invoiceFeeRow, priceListType, eInvoiceFormat, vatRateMain, currency, printService, invoiceRows.Count + 1));
            }

            var freighAmountRow = customerInvoiceRows.FirstOrDefault(i => i.IsFreightAmountRow);
            if (freighAmountRow != null)
            {
                invoiceRows.Add(CreateFinvoiceRow(invoice, freighAmountRow, priceListType, eInvoiceFormat, vatRateMain, currency, printService, invoiceRows.Count + 1));
            }

            #region InvoiceFee Old and removed

            //Have to create "bogus"-row to match header sums
            /*if (invoice.InvoiceFee > 0)
            {
                InvoiceRow invoiceRow = new InvoiceRow();
                invoiceRow.ArticleName = "Laskutuslisä";
                invoiceRow.ArticleIdentifier = string.Empty;
                decimal quantity = 1;
                rowPositionIndentifier = rowPositionIndentifier + 1;
                invoiceRow.RowPositionIndentifier = rowPositionIndentifier;
                //Maks 5 decimals
                if (printService)
                    invoiceRow.DeliveredQuantity = Math.Round(quantity, 2);
                else
                    invoiceRow.DeliveredQuantity = Math.Round(quantity, 5);
                invoiceRow.DeliveredQuantityUnitCode = "kpl";
                invoiceRow.UnitPriceAmount = Math.Round(invoice.InvoiceFee, 2);
                invoiceRow.UnitPriceNetAmount = invoiceRow.UnitPriceAmount;
                invoiceRow.UnitPriceAmountCurrencyIdentifier = currency;
                invoiceRow.RowDiscountPercent = 0;
                invoiceRow.RowVatRatePercent = vatRateMain;
                if ((int)TermGroup_InvoiceVatType.Contractor == invoice.VatType)
                    invoiceRow.RowVatCode = "AE";
                else if ((int)TermGroup_InvoiceVatType.NoVat == invoice.VatType)
                    invoiceRow.RowVatCode = "Z";
                else if (invoiceRow.RowVatRatePercent == 0)
                    invoiceRow.RowVatCode = "O";
                else
                    invoiceRow.RowVatCode = "S";
                if (priceListType != null && priceListType.InclusiveVat)
                {
                    invoiceRow.RowVatAmount = Math.Round((invoice.InvoiceFee * (vatRateMain / (vatRateMain + 100))), 2);
                    invoiceRow.RowVatExcludedAmount = Math.Round((invoice.InvoiceFee - invoiceRow.RowVatAmount), 2);
                }
                else
                {
                    invoiceRow.RowVatAmount = Math.Round((invoice.InvoiceFee * (vatRateMain / 100)), 2);
                    invoiceRow.RowVatExcludedAmount = invoice.InvoiceFee;
                }
                invoiceRow.RowVatAmountCurrencyIdentifier = currency;
                invoiceRow.RowVatExcludedAmountCurrencyIdentifier = currency;

                invoiceRows.Add(invoiceRow);
            }*/

            #endregion

            List<SubInvoiceRow> subRows = new List<SubInvoiceRow>();
            foreach (var customerInvoiceRow in customerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.BaseProductRow && !i.IsInvoiceFeeRow && !i.IsFreightAmountRow))
            {
                SubInvoiceRow subRow = new SubInvoiceRow();
                string productName = (!string.IsNullOrEmpty(customerInvoiceRow.Product.Name)) ? customerInvoiceRow.Product.Name : string.Empty;
                productName = productName.Replace("\x0D0A", " ");
                productName = productName.Replace("\x0D", " ");
                productName = productName.Replace("\x0A", " ");
                subRow.SubArticleName = (customerInvoiceRow.Product != null && !string.IsNullOrEmpty(productName)) ? productName : string.Empty;
                subRow.SubRowVatRatePercent = customerInvoiceRow.VatRate;
                subRow.SubRowVatAmount = customerInvoiceRow.VatAmount;
                subRow.SubRowVatAmountCurrencyIdentifier = currency;
                if (priceListType != null && priceListType.InclusiveVat)
                    subRow.SubRowVatExcludedAmount = customerInvoiceRow.SumAmount - customerInvoiceRow.VatAmount;
                else
                    subRow.SubRowVatExcludedAmount = customerInvoiceRow.SumAmount;
                subRow.SubRowVatExcludedAmountCurrencyIdentifier = currency;

                subRows.Add(subRow);
            }

            if (subRows.Count > 0)
            {
                InvoiceRow invoiceRowForSubRows = new InvoiceRow();
                invoiceRowForSubRows.UseOnlyAsSubInvoiceRow = true;
                invoiceRowForSubRows.subInvoiceRows.AddRange(subRows);
                invoiceRows.Add(invoiceRowForSubRows);
            }

            #endregion

            #region EpiDetails

            epiDetails.EpiDate = DateTime.Now;
            //Not In use in finland
            //epiDetails.EpiReference = !String.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoiceDetails.InvoiceNumber;          
            epiDetails.EpiBfiIdentifier = bic;
            epiDetails.EpiNameAddressDetails = company.Name;
            epiDetails.EpiAccountID = iban;
            epiDetails.EpiRemittanceInfoIdentifier = !String.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoiceDetails.InvoiceNumber;
            if (epiDetails.EpiRemittanceInfoIdentifier.StartsWith("RF"))
            {
                epiDetails.EpiRemittanceInfoIdentifierType = "ISO";
            }
            else
            {
                epiDetails.EpiRemittanceInfoIdentifierType = "SPY";
            }

            epiDetails.EpiInstructedAmount = invoice.TotalAmount;
            epiDetails.EpiInstructedAmountCurrencyId = currency;
            epiDetails.EpiCharge = "SHA";
            epiDetails.EpiPaymentMeansCode = "58";
            epiDetails.EpiChargeOption = "SHA";

            if (invoice.DueDate.HasValue)
                epiDetails.EpiDateOptionDate = invoice.DueDate.Value;

            #endregion
        }

        private InvoiceRow CreateFinvoiceRow(CustomerInvoice invoice, CustomerInvoiceRow customerInvoiceRow, PriceListType priceListType, TermGroup_EInvoiceFormat eInvoiceFormat, decimal vatRateMain, string currency, bool printService, int rowNr)
        {
            InvoiceRow invoiceRow = new InvoiceRow();
            string rowFreeText = customerInvoiceRow.Text;
            rowFreeText = rowFreeText.Replace("\x0D0A", ";");
            rowFreeText = rowFreeText.Replace("\x0D", ";");
            rowFreeText = rowFreeText.Replace("\x0A", ";");

            invoiceRow.RowFreeText = (customerInvoiceRow.Type == (int)SoeInvoiceRowType.TextRow) ? rowFreeText : "";
            invoiceRow.ArticleIdentifier = (customerInvoiceRow.Product != null && !string.IsNullOrEmpty(customerInvoiceRow.Product.Number)) ? customerInvoiceRow.Product.Number : "";
            invoiceRow.ArticleName = (customerInvoiceRow.Type == (int)SoeInvoiceRowType.TextRow) ? "-" : customerInvoiceRow.Text;

            //Maks 5 decimals
            if (printService)
                invoiceRow.DeliveredQuantity = customerInvoiceRow.Quantity.HasValue ? Math.Round(customerInvoiceRow.Quantity.Value, 2) : 0;
            else
                invoiceRow.DeliveredQuantity = customerInvoiceRow.Quantity.HasValue ? Math.Round(customerInvoiceRow.Quantity.Value, 5) : 0;

            //As per PBI - #108689
            if (invoice.IsCustomerInvoice && invoice.IsCredit)
                invoiceRow.DeliveredQuantity *= -1;

            invoiceRow.DeliveredQuantityUnitCode = (customerInvoiceRow.ProductUnit != null && !string.IsNullOrEmpty(customerInvoiceRow.ProductUnit.Name)) ? customerInvoiceRow.ProductUnit.Name : string.Empty;
            if (invoice.PriceListTypeInclusiveVat)
                invoiceRow.UnitPriceAmount = invoiceRow.UnitPriceNetAmount = customerInvoiceRow.Amount / ((customerInvoiceRow.VatRate + 100) / 100);
            else
                invoiceRow.UnitPriceAmount = invoiceRow.UnitPriceNetAmount = customerInvoiceRow.Amount;

            var discountAmount = customerInvoiceRow.DiscountPercent > 0 ? (invoiceRow.UnitPriceAmount * (customerInvoiceRow.DiscountPercent / 100)) : 0;
            var discount2Amount = customerInvoiceRow.Discount2Percent > 0 ? ((invoiceRow.UnitPriceAmount - discountAmount) * (customerInvoiceRow.Discount2Percent / 100)) : 0;

            invoiceRow.UnitPriceNetAmount = invoiceRow.UnitPriceAmount - discountAmount - discount2Amount;

            invoiceRow.UnitPriceAmount = Math.Round(invoiceRow.UnitPriceAmount, 3);
            invoiceRow.UnitPriceNetAmount = Math.Round(invoiceRow.UnitPriceNetAmount, 3);
            
            if (invoice.IsCredit)
            {
                invoiceRow.UnitPriceAmount = Math.Abs(invoiceRow.UnitPriceAmount);
                invoiceRow.UnitPriceNetAmount = Math.Abs(invoiceRow.UnitPriceNetAmount);
            }

            invoiceRow.RowPositionIndentifier = rowNr;
            invoiceRow.UnitPriceAmountCurrencyIdentifier = currency;

            // Handle discount
            if (customerInvoiceRow.DiscountPercent > 0 && customerInvoiceRow.Discount2Percent > 0)
            {
                // Primary discount
                invoiceRow.progressiveDiscountRows.Add(new InvoiceRowProgressiveDiscount()
                {
                    RowDiscountAmount = customerInvoiceRow.DiscountAmount,
                    RowDiscountPercent = customerInvoiceRow.DiscountPercent,
                    RowDiscountTypeText = "Alennus",
                });

                // Secondary discount
                invoiceRow.progressiveDiscountRows.Add(new InvoiceRowProgressiveDiscount()
                {
                    RowDiscountAmount = customerInvoiceRow.Discount2Amount,
                    RowDiscountPercent = customerInvoiceRow.Discount2Percent,
                    RowDiscountTypeText = "Alennus2",
                });
            }
            else
            {
                if (customerInvoiceRow.Discount2Percent > 0)
                {
                    invoiceRow.RowDiscountPercent = customerInvoiceRow.Discount2Percent;
                    invoiceRow.RowDiscountAmount = customerInvoiceRow.Discount2Amount;
                }
                else
                {
                    invoiceRow.RowDiscountPercent = customerInvoiceRow.DiscountPercent;
                    invoiceRow.RowDiscountAmount = customerInvoiceRow.DiscountAmount;
                }
            }

            invoiceRow.RowVatRatePercent = customerInvoiceRow.VatRate;
            invoiceRow.RowVatAmount = customerInvoiceRow.VatAmount;
            if ((int)TermGroup_InvoiceVatType.Contractor == invoice.VatType)
                invoiceRow.RowVatCode = "AE";
            else if ((int)TermGroup_InvoiceVatType.NoVat == invoice.VatType)
                invoiceRow.RowVatCode = "Z";
            else if (customerInvoiceRow.VatRate == 0 && !IsFinvoiceV3(eInvoiceFormat))
                invoiceRow.RowVatCode = "O";
            else
            {
                invoiceRow.RowVatCode = "S";
                if (invoiceRow.RowVatRatePercent == 0)
                {
                    invoiceRow.RowVatRatePercent = vatRateMain;
                }
            }

            invoiceRow.RowVatAmountCurrencyIdentifier = currency;
            if (priceListType != null && priceListType.InclusiveVat)
                invoiceRow.RowVatExcludedAmount = customerInvoiceRow.SumAmount - customerInvoiceRow.VatAmount;
            else
                invoiceRow.RowVatExcludedAmount = customerInvoiceRow.SumAmount;
            invoiceRow.RowVatExcludedAmountCurrencyIdentifier = currency;

            return invoiceRow;
        }

        /// <summary>
        /// Method for creating finvoice (Export).
        /// </summary>
        /// <param name="document"></param>        
        public void ToXml(ref XDocument document, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            //Namespaces
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XElement rootElement;

            if (IsFinvoiceV2(eInvoiceFormat))
                rootElement = new XElement("Finvoice", new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName), new XAttribute(xsi + "noNamespaceSchemaLocation", "Finvoice2.01.xsd"), new XAttribute("Version", "2.01"));
            else if (IsFinvoiceV3(eInvoiceFormat))
                rootElement = new XElement("Finvoice", new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName), new XAttribute(xsi + "noNamespaceSchemaLocation", "Finvoice3.0.xsd"), new XAttribute("Version", "3.0"));
            else
                rootElement = new XElement("Finvoice", new XAttribute("Version", new System.Version("1.3")), new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName), new XAttribute(xsi + "noNamespaceSchemaLocation", "finvoice.xsd"));

            XProcessingInstruction instruction = new XProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"Finvoice.xsl\"");

            document.Add(instruction);

            if (IsFinvoiceV3(eInvoiceFormat))
            {
                MessageTransmissionDetails.AddNode(ref rootElement, false);
            }

            // SellerPartyDetails
            sellerPartyDetails.AddNode(ref rootElement, eInvoiceFormat);

            // SellerOrganisationUnitNumber
            parseToNodes(rootElement, "SellerOrganisationUnitNumber", GetString(SellerOrganisationUnitNumber), 0, 35, 1);

            // SellerContactPersonName
            parseToNodes(rootElement, "SellerContactPersonName", GetString(SellerContactPersonName), 0, IsFinvoiceV3(eInvoiceFormat) ? 70 : 35, 1);

            // sellerCommunicationDetails
            sellerCommunicationDetails.AddNode(ref rootElement);

            // sellerInformationDetails
            sellerInformationDetails.AddNode(ref rootElement);

            // buyerPartyDetails
            buyerPartyDetails.AddNode(ref rootElement, eInvoiceFormat);

            // buyerOrganisationUnitNumber
            parseToNodes(rootElement, "BuyerOrganisationUnitNumber", GetString(BuyerOrganisationUnitNumber), 0, 35, 1);

            //BuyerContactPersonName
            parseToNodes(rootElement, "BuyerContactPersonName", GetString(BuyerContactPersonName), 0, IsFinvoiceV3(eInvoiceFormat) ? 70 : 35, 1);

            // buyerCommunicationDetails
            buyerCommunicationDetails.AddNode(ref rootElement);

            // deliveryPartyDetails
            deliveryPartyDetails.AddNode(ref rootElement);

            if (IsFinvoiceV3(eInvoiceFormat))
            {
                parseToNodes(rootElement, "DeliverySiteCode", invoiceDetails.DefinitionDetails.FirstOrDefault(d => d.DefinitionCode == "TA0001")?.DefinitionValue, 0, 35, 1);
            }

            // deliveryDetails
            deliveryDetails.AddNode(ref rootElement);

            //invoiceDetails
            invoiceDetails.AddNode(ref rootElement, eInvoiceFormat);

            // invoiceRows
            foreach (var invoiceRow in invoiceRows)
            {
                invoiceRow.AddNode(ref rootElement, eInvoiceFormat);
            }

            // epiDetails
            epiDetails.AddNode(ref rootElement, eInvoiceFormat);

            if (AddAttachmentMessageDetails)
            {
                var attachmentMessageDetails = new XElement("AttachmentMessageDetails");
                parseToNodes(attachmentMessageDetails, "AttachmentMessageIdentifier", GetMessageAttachmentIdentifier(MessageTransmissionDetails.MessageIdentifier), 0, 48, 1);

                rootElement.Add(attachmentMessageDetails);
            }

            document.Add(rootElement);
            xdoc = document;
        }

        public bool Validate(TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            if (xdoc == null)
                return false;
            //Disabled until directory in the database is not affecting this ( now we have to change this manually everyday after customer calls us)

            var schemas = new List<string>();

            if (eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice2)
                schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_PHYSICAL + "Finvoice2.01.xsd");
            else if (eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice3)
                schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_PHYSICAL + "Finvoice3.0.xsd");
            else
                schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_PHYSICAL + "Finvoice.xsd");


            return rgm.ValidateXDocument(xdoc, schemas);
        }

        public void AddSoapEnvelope(ref XDocument soapDocument, Customer customer, string SenderAddressComp, string SenderIntermediatorComp, string TimestampStr)
        {
            //Got to do own doc for envelope and combine them
            //Timestamp
            if (!TimestampStr.HasValue())
            {
                TimestampStr = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH':'mm':'ss") + "+02";
            }
            //Namespaces
            XNamespace SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace EB = "http://www.oasis-open.org/committees/ebxml-msg/schema/msg-header-2_0.xsd";
            XNamespace xlinkns = MessageTransmissionDetails.MessageIdentifier;
            XNamespace xlink = "http://www.w3.org/1999/xlink";

            XElement rootElement = new XElement(SoapEnv + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", SoapEnv.NamespaceName), new XAttribute(XNamespace.Xmlns + "xlink", "http://www.w3.org/1999/xlink"), new XAttribute(XNamespace.Xmlns + "eb", EB.NamespaceName));

            #region Header
            XElement header = new XElement(SoapEnv + "Header");

            #region Messageheader
            XElement messageHeader = new XElement(EB + "MessageHeader", new XAttribute(XNamespace.Xmlns + "eb", EB.NamespaceName), new XAttribute(SoapEnv + "mustUnderstand", "1"));

            XElement FromSender = new XElement(EB + "From");
            FromSender.Add(new XElement(EB + "PartyId", SenderAddressComp));
            FromSender.Add(new XElement(EB + "Role", "Sender"));
            messageHeader.Add(FromSender);
            XElement FromSender2 = new XElement(EB + "From");
            FromSender2.Add(new XElement(EB + "PartyId", SenderIntermediatorComp));
            FromSender2.Add(new XElement(EB + "Role", "Intermediator"));
            messageHeader.Add(FromSender2);

            XElement FromTo = new XElement(EB + "To");
            FromTo.Add(new XElement(EB + "PartyId", customer.FinvoiceAddress));
            FromTo.Add(new XElement(EB + "Role", "Receiver"));
            messageHeader.Add(FromTo);

            XElement FromTo2 = new XElement(EB + "To");
            FromTo2.Add(new XElement(EB + "PartyId", customer.FinvoiceOperator));
            FromTo2.Add(new XElement(EB + "Role", "Intermediator"));
            messageHeader.Add(FromTo2);

            messageHeader.Add(new XElement(EB + "CPAId", "yoursandmycpa"));
            messageHeader.Add(new XElement(EB + "ConversationId", ""));
            messageHeader.Add(new XElement(EB + "Service", "Routing"));
            messageHeader.Add(new XElement(EB + "Action", "ProcessInvoice"));
            #endregion

            #region Messagedata
            XElement MessageData = new XElement(EB + "MessageData");
            XElement MessageId = new XElement(EB + "MessageId", xlinkns.NamespaceName);
            XElement Timestamp = new XElement(EB + "Timestamp", TimestampStr);
            MessageData.Add(MessageId);
            MessageData.Add(Timestamp);
            MessageData.Add(new XElement(EB + "RefToMessageId", ""));
            messageHeader.Add(MessageData);
            #endregion

            header.Add(messageHeader);

            #endregion

            #region Body
            XElement body = new XElement(SoapEnv + "Body");

            XElement manifest = new XElement(EB + "Manifest", new XAttribute(EB + "id", "Manifest"), new XAttribute(EB + "version", "2.0"));

            XElement reference = new XElement(EB + "Reference", new XAttribute(EB + "id", "Finvoice"), new XAttribute(xlink + "href", xlinkns.NamespaceName));

            XElement ebschema = new XElement(EB + "schema", new XAttribute(EB + "location", "http://www.pankkiyhdistys.fi/verkkolasku/finvoice/finvoice.xsd"), new XAttribute(EB + "version", "2.0"));

            //combine
            reference.Add(ebschema);
            manifest.Add(reference);
            body.Add(manifest);

            #endregion

            rootElement.Add(header);
            rootElement.Add(body);

            soapDocument.Add(rootElement);
            xdoc = soapDocument;
        }

        #endregion

        #region FinvoiceToPdf

        public static FinvoiceEdiItem CreateItem(string xml, string fileName, int ediEntryType, ParameterObject parameterObject)
        {
            XDocument xdoc = null;
            // Check for byte-order mark and remove it from the beginning if thats the case
            char byteOrderMark = (char)0xFEFF;
            if (xml.FirstOrDefault().Equals(byteOrderMark))
                xml = xml.TrimStart(byteOrderMark);

            xdoc = XDocument.Parse(XmlUtil.FormatXml(xml));
            if (xdoc == null)
                return null;

            return new FinvoiceEdiItem(xdoc, fileName, ediEntryType, true, true, parameterObject);
        }

        private XDocument xdocument = null;
        public XDocument ToXDocument()
        {
            if (xdocument == null)
            {
                xdocument = rdm.CreateFinvoiceEdiDataDocument(this);
            }
            return xdocument;
        }

        private void Parse(bool loadRows, bool loadAccountCodes, int ediEntryType)
        {
            if (this.xdoc == null)
                return;

            //if (ediEntryType != (int)TermGroup_EDISourceType.Finvoice)
            //{
            #region FINVOICE            

            #region MessageInfo

            //ElementMessageInfo = XmlUtil.GetChildElement(xdoc, "MessageInfo");
            //if (ElementMessageInfo != null)
            //{
            //    MessageSenderId = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageSenderId");
            //    MessageType = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageType");
            //    MessageDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageDate"));
            //    MessageImageFileName = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageImageFileName");
            //    MessageCultureInfo = XmlUtil.GetChildElementValue(ElementMessageInfo, "MessageCultureInfo");  // Added to get Finnish translation for header
            //}

            #endregion

            #region Seller                                                

            SellerPartyDetails = XmlUtil.GetChildElement(xdoc, "SellerPartyDetails");
            if (SellerPartyDetails != null)
            {
                //  SellerId = XmlUtil.GetChildElementValue(ElementSeller, "SellerId");
                SellerOrganisationNumber = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerPartyIdentifier");
                SellerVatNumber = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerOrganisationTaxCode");
                SellerName = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerOrganisationName");

                SellerPostalAddressDetails = XmlUtil.GetChildElement(SellerPartyDetails, "SellerPostalAddressDetails");
                if (SellerPostalAddressDetails != null)
                {
                    SellerAddress = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "SellerStreetName");
                    SellerPostalCode = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "SellerPostCodeIdentifier");
                    SellerPostalAddress = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "SellerTownName");
                    SellerCountryCode = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "CountryCode");
                    SellerCountryName = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "CountryName");
                }
            }

            SellerReference = XmlUtil.GetChildElementValue(xdoc, "SellerContactPersonName");

            SellerInformationDetails = XmlUtil.GetChildElement(xdoc, "SellerInformationDetails");
            if (SellerInformationDetails != null)
            {
                SellerPhone = XmlUtil.GetChildElementValue(SellerInformationDetails, "SellerPhoneNumber");
                SellerFax = XmlUtil.GetChildElementValue(SellerInformationDetails, "SellerFaxNumber");
                SellerWebAddress = XmlUtil.GetChildElementValue(SellerInformationDetails, "SellerWebaddressIdentifier");
            }

            SellerCommunicationDetails = XmlUtil.GetChildElement(xdoc, "SellerCommunicationDetails");
            if (SellerCommunicationDetails != null)
            {
                SellerReferencePhone = XmlUtil.GetChildElementValue(SellerCommunicationDetails, "SellerPhoneNumberIdentifier");
                SellerEmailAddress = XmlUtil.GetChildElementValue(SellerCommunicationDetails, "SellerEmailaddressIdentifier");
            }

            #endregion

            #region Buyer                                                           

            BuyerPartyDetails = XmlUtil.GetChildElement(xdoc, "BuyerPartyDetails");
            if (BuyerPartyDetails != null)
            {
                // BuyerId = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerId");
                BuyerOrganisationNumber = XmlUtil.GetChildElementValue(BuyerPartyDetails, "BuyerPartyIdentifier");
                BuyerVatNumber = XmlUtil.GetChildElementValue(BuyerPartyDetails, "BuyerOrganisationTaxCode");
                BuyerName = XmlUtil.GetChildElementValue(BuyerPartyDetails, "BuyerOrganisationName");

                BuyerPostalAddressDetails = XmlUtil.GetChildElement(BuyerPartyDetails, "BuyerPostalAddressDetails");
                if (BuyerPostalAddressDetails != null)
                {
                    BuyerAddress = XmlUtil.GetChildElementValue(BuyerPostalAddressDetails, "BuyerAddress");
                    BuyerPostalCode = XmlUtil.GetChildElementValue(BuyerPostalAddressDetails, "BuyerPostalCode");
                    BuyerPostalAddress = XmlUtil.GetChildElementValue(BuyerPostalAddressDetails, "BuyerPostalAddress");
                    BuyerCountryCode = XmlUtil.GetChildElementValue(BuyerPostalAddressDetails, "BuyerCountryCode");
                }
            }

            BuyerReference = XmlUtil.GetChildElementValue(xdoc, "BuyerContactPersonName");

            BuyerCommunicationDetails = XmlUtil.GetChildElement(xdoc, "BuyerCommunicationDetails");
            if (BuyerCommunicationDetails != null)
            {
                BuyerPhone = XmlUtil.GetChildElementValue(BuyerCommunicationDetails, "BuyerPhoneNumberIdentifier");
                BuyerEmailAddress = XmlUtil.GetChildElementValue(BuyerCommunicationDetails, "BuyerEmailaddressIdentifier");
            }

            DeliveryPartyDetails = XmlUtil.GetChildElement(xdoc, "DeliveryPartyDetails");
            if (DeliveryPartyDetails != null)
            {
                BuyerDeliveryName = XmlUtil.GetChildElementValue(DeliveryPartyDetails, "DeliveryOrganisationName");

                DeliveryPostalAddressDetails = XmlUtil.GetChildElement(DeliveryPartyDetails, "DeliveryPostalAddressDetails");
                if (DeliveryPostalAddressDetails != null)
                {
                    //BuyerDeliveryCoAddress = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryCoAddress");
                    BuyerDeliveryAddress = XmlUtil.GetChildElementValue(DeliveryPostalAddressDetails, "DeliveryStreetName");
                    BuyerDeliveryPostalCode = XmlUtil.GetChildElementValue(DeliveryPostalAddressDetails, "DeliveryPostCodeIdentifier");
                    BuyerDeliveryPostalAddress = XmlUtil.GetChildElementValue(DeliveryPostalAddressDetails, "DeliveryTownName");
                    BuyerDeliveryCountryCode = XmlUtil.GetChildElementValue(DeliveryPostalAddressDetails, "CountryCode");
                }

            }

            //BuyerDeliveryNoteText = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryNoteText");
            //BuyerDeliveryGoodsMarking = XmlUtil.GetChildElementValue(ElementBuyer, "BuyerDeliveryGoodsMarking");


            #endregion

            #region Head

            // Invoicedetails
            InvoiceDetails = XmlUtil.GetChildElement(xdoc, "InvoiceDetails");
            if (InvoiceDetails != null)
            {
                MessageType = XmlUtil.GetChildElementValue(InvoiceDetails, "InvoiceTypeCode");
                HeadInvoiceNumber = XmlUtil.GetChildElementValue(InvoiceDetails, "InvoiceNumber");
                HeadInvoiceDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(InvoiceDetails, "InvoiceDate"));
                HeadInvoiceNetAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(InvoiceDetails, "InvoiceTotalVatExcludedAmount"), 2);
                HeadVatAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(InvoiceDetails, "InvoiceTotalVatAmount"), 2);
                HeadInvoiceGrossAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(InvoiceDetails, "InvoiceTotalVatIncludedAmount"), 2);
                //if (HeadInvoiceGrossAmount == 0)
                //    HeadInvoiceGrossAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadGrossAmount"), 2);
                //if (HeadInvoiceNetAmount == 0)
                //    HeadInvoiceNetAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadNetAmount"), 2);
                XElement InvoiceTotalVatIncludedAmount = XmlUtil.GetChildElement(InvoiceDetails, "InvoiceTotalVatIncludedAmount");
                HeadCurrencyCode = XmlUtil.GetAttributeStringValue(InvoiceTotalVatIncludedAmount, "AmountCurrencyIdentifier");

                // VatSpecificationDetails
                VatSpecificationDetails = XmlUtil.GetChildElement(InvoiceDetails, "VatSpecificationDetails");
                if (VatSpecificationDetails != null)
                {
                    HeadVatBasisAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(VatSpecificationDetails, "VatBaseAmount"), 2);
                    HeadVatPercentage = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(VatSpecificationDetails, "VatRatePercent"), 2);
                }

                // PaymentTermsDetails
                PaymentTermsDetails = XmlUtil.GetChildElement(InvoiceDetails, "PaymentTermsDetails");
                if (PaymentTermsDetails != null)
                {
                    //HeadPaymentConditionDays = StringUtility.GetInt(XmlUtil.GetChildElementValue(ElementHead, "HeadPaymentConditionDays"), 0);
                    HeadPaymentConditionText = XmlUtil.GetChildElementValue(PaymentTermsDetails, "PaymentTermsFreeText");

                    // PaymentOverDueFineDetails
                    PaymentOverDueFineDetails = XmlUtil.GetChildElement(PaymentTermsDetails, "PaymentOverDueFineDetails");
                    if (PaymentOverDueFineDetails != null)
                    {
                        HeadInterestPaymentPercent = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(PaymentOverDueFineDetails, "PaymentOverDueFinePercent"), 2);
                        HeadInterestPaymentText = XmlUtil.GetChildElementValue(PaymentOverDueFineDetails, "PaymentOverDueFineFreeText");
                    }
                }
            }

            // EpiDetails
            EpiDetails = XmlUtil.GetChildElement(xdoc, "EpiDetails");
            if (EpiDetails != null)
            {
                // EpiPartyDetails
                EpiPartyDetails = XmlUtil.GetChildElement(EpiDetails, "EpiPartyDetails");
                if (EpiPartyDetails != null)
                {
                    EpiBfiPartyDetails = XmlUtil.GetChildElement(EpiPartyDetails, "EpiBfiPartyDetails");
                    if (EpiBfiPartyDetails != null)
                        HeadBicAddress = XmlUtil.GetChildElementValue(EpiBfiPartyDetails, "EpiBfiIdentifier");

                    EpiBeneficiaryPartyDetails = XmlUtil.GetChildElement(EpiPartyDetails, "EpiBeneficiaryPartyDetails");
                    if (EpiBeneficiaryPartyDetails != null)
                        HeadIbanNumber = XmlUtil.GetChildElementValue(EpiBeneficiaryPartyDetails, "EpiAccountID");

                }

                // EpiPaymentInstructionDetails
                EpiPaymentInstructionDetails = XmlUtil.GetChildElement(EpiDetails, "EpiPaymentInstructionDetails");
                if (EpiPaymentInstructionDetails != null)
                {
                    HeadInvoiceOcr = XmlUtil.GetChildElementValue(EpiPaymentInstructionDetails, "EpiRemittanceInfoIdentifier");
                    HeadInvoiceDueDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(EpiPaymentInstructionDetails, "EpiDateOptionDate"));
                }

            }


            //HeadDeliveryDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(ElementHead, "HeadDeliveryDate"));
            //HeadBuyerOrderNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadBuyerOrderNumber");
            //HeadBuyerProjectNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadBuyerProjectNumber");
            //HeadBuyerInstallationNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadBuyerInstallationNumber");
            //HeadSellerOrderNumber = XmlUtil.GetChildElementValue(ElementHead, "HeadSellerOrderNumber");
            //HeadPostalGiro = XmlUtil.GetChildElementValue(ElementHead, "HeadPostalGiro");
            //HeadBankGiro = XmlUtil.GetChildElementValue(ElementHead, "HeadBankGiro");
            //HeadBank = XmlUtil.GetChildElementValue(ElementHead, "HeadBank");                                                                                                     
            //HeadFreightFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadFreightFeeAmount"), 2);
            //HeadHandlingChargeFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadHandlingChargeFeeAmount"), 2);
            //HeadInsuranceFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadInsuranceFeeAmount"), 2);
            //HeadRemainingFeeAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadRemainingFeeAmount"), 2);
            //HeadDiscountAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadDiscountAmount"), 2);
            //HeadRoundingAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(ElementHead, "HeadRoundingAmount"), 2);
            //HeadInvoiceArrival = StringUtility.GetInt(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceArrival"), 0);
            //HeadInvoiceAuthorized = StringUtility.GetBool(XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceAuthorized"));
            //HeadInvoiceAuthorizedBy = XmlUtil.GetChildElementValue(ElementHead, "HeadInvoiceAuthorizedBy");


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

            LoadRows();

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

        }

        public void LoadRows()
        {
            if (xdoc == null)
                return;

            List<XElement> rows = XmlUtil.GetChildElements(xdoc, "InvoiceRow");
            foreach (XElement row in rows)
            {
                Rows.Add(new FinvoiceEdiRowItem(row));
            }
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
            int.TryParse(dto.HeadInvoiceType.Replace(".", ","), out headInvoiceType);

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
            decimal.TryParse(dto.HeadInterestPaymentPercent.Replace(".", ","), out headInterestPaymentPercent);

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
            int.TryParse(dto.HeadInvoiceArrival.Replace(".", ","), out headInvoiceArrival);

            HeadInvoiceArrival = headInvoiceArrival;
            HeadInvoiceAuthorized = HeadInvoiceAuthorized;
            HeadInvoiceAuthorizedBy = dto.HeadInvoiceAuthorizedBy;


            #endregion

            #region Rows

            if (!dto.SysEdiEdiMessageRowDTOs.IsNullOrEmpty())
            {
                rows = new List<FinvoiceEdiRowItem>();

                foreach (var dtoRow in dto.SysEdiEdiMessageRowDTOs)
                {
                    var row = new FinvoiceEdiRowItem();

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
                    decimal.TryParse(row.RowUnitPrice.Replace(".", ","), out unitPrice);

                    if (row.RowNetAmount.HasValue && row.RowVatAmount.HasValue)
                        rowAmount = row.RowNetAmount.Value + row.RowVatAmount.Value;
                    else if (row.RowQuantity.HasValue)
                        rowAmount = row.RowQuantity.Value * unitPrice;

                    this.HeadInvoiceGrossAmount += rowAmount;
                }
            }

            #endregion
        }


        #endregion

        #region Validations

        public ActionResult ValidateOperatorForAttachment(Customer customer, SettingManager settingManager, int userId, int actorCompanyId, int licenseId, string invoiceNr)
        {
            // Validate FInvoice Operator
            string companyFInvoiceOperator = GetString(settingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceOperator, userId, actorCompanyId, licenseId));
            string customerFInvoiceOperator = customer.FinvoiceOperator;
            
            if (customer.IsFinvoiceCustomer)
            {
                if (!IsValidAttachmentRecipients(companyFInvoiceOperator, customerFInvoiceOperator))
                {
                    return new ActionResult
                    {
                        Success = false,
                        ErrorMessage = string.Format(settingManager.GetText(3603, 1, "Din e-faktureringsoperatör har inte bekräftat sändningen av bilagor med en eller flera mottagare.\r\nFaktura: {0}"), invoiceNr),
                        CanUserOverride = true
                    };
                }
            }

            return new ActionResult();
        }

        public bool IsValidAttachmentRecipients(string companyFinvoiceOperator, string customerFinvoiceOperator)
        {
            companyFinvoiceOperator = companyFinvoiceOperator.Trim().ToUpper();
            customerFinvoiceOperator = customerFinvoiceOperator.Trim().ToUpper();

            var companyOperators = new List<string>()
            {
                "OKOYFIHH",
                "NDEAFIHH"
            };

            // Check whether attatchment validation is needed
            if (!companyOperators.Contains(companyFinvoiceOperator))
                return true;

            var OKOYFIHH_Receivers = new List<string>()
            {
                "OKOYFIHH",
                "E204503",
                "FI28768767",
                "003714377140",
                "003701011385",
                "5909000716438",
                "INEXCHANGE",
            };

            var NDEAFIHH_Receivers = new List<string>()
            {
                "NDEAFIHH",
                "E204503",
                "003714377140"
            };

            switch (companyFinvoiceOperator)
            {
                case "OKOYFIHH":
                    return OKOYFIHH_Receivers.Contains(customerFinvoiceOperator);
                case "NDEAFIHH":
                    return NDEAFIHH_Receivers.Contains(customerFinvoiceOperator);
                default:
                    return false;
            }
        }

        #endregion
    }

    public class FinvoiceEdiRowItem
    {
        #region Variables

        private readonly XElement row;

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

        #endregion

        public FinvoiceEdiRowItem()
        {
        }

        public FinvoiceEdiRowItem(XElement row)
        {
            this.row = row;

            #region Row

            RowSellerArticleNumber = XmlUtil.GetChildElementValue(row, "ArticleIdentifier");
            RowSellerArticleDescription1 = XmlUtil.GetChildElementValue(row, "ArticleName");
            //RowSellerArticleDescription2 = XmlUtil.GetChildElementValue(row, "RowSellerArticleDescription2");
            //RowSellerRowNumber = XmlUtil.GetChildElementValue(row, "RowSellerRowNumber");
            //RowBuyerArticleNumber = XmlUtil.GetChildElementValue(row, "RowBuyerArticleNumber");
            //RowBuyerRowNumber = XmlUtil.GetChildElementValue(row, "RowBuyerRowNumber");
            //RowDeliveryDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(row, "RowDeliveryDate"), null);
            //RowBuyerReference = XmlUtil.GetChildElementValue(row, "RowBuyerReference");
            //RowBuyerObjectId = XmlUtil.GetChildElementValue(row, "RowBuyerObjectId");
            RowQuantity = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "DeliveredQuantity"), 2);
            XElement DeliveredQuantity = XmlUtil.GetChildElement(row, "DeliveredQuantity");
            RowUnitCode = XmlUtil.GetAttributeStringValue(DeliveredQuantity, "QuantityUnitCode");
            RowUnitPrice = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "UnitPriceAmount"), 2);
            RowDiscountPercent = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowDiscountPercent"), 2);
            RowNetAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowVatExcludedAmount"), 2);
            RowVatAmount = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowVatAmount"), 2);
            RowVatPercentage = NumberUtility.ToDecimal(XmlUtil.GetChildElementValue(row, "RowVatRatePercent"), 2);
            //RowSellerArticleText1 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText1");
            //RowSellerArticleText2 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText2");
            //RowSellerArticleText3 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText3");
            //RowSellerArticleText4 = XmlUtil.GetChildElementValue(row, "RowSellerArticleText4");
            //StockCode = XmlUtil.GetChildElementValue(row, "StockCode");

            #endregion
        }

        public FinvoiceEdiRowItem(SysEdiMessageRowDTO row)
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
                purchaseAmount = RowUnitPrice - (RowUnitPrice * RowDiscountPercent);

            return purchaseAmount;
        }

        #endregion
    }

    public class MessageTransmissionDetails : FinvoiceBase
    {
        public string SpecificationIdentifier { get; set; }
        public string FromIdentifier { get; set; }
        public string FromIntermediator { get; set; }
        public string ToIdentifier { get; set; }
        public string ToIntermediator { get; set; }
        public string MessageIdentifier { get; set; }
        public string MessageTimeStamp { get; set; }

        public void AddNode(ref XElement root, bool isAttachment)
        {
            XElement messageTransmissionDetails = new XElement("MessageTransmissionDetails");

            // MessageSenderDetails
            XElement messageSenderDetails = new XElement("MessageSenderDetails");

            var identiferSchemaAttribute = isAttachment ? null : new XAttribute("SchemeID", "0037");
            //FromIdentifier
            parseToNodes(messageSenderDetails, "FromIdentifier", GetString(FromIdentifier), 0, 35, 1, identiferSchemaAttribute);
            //FromIntermediator
            parseToNodes(messageSenderDetails, "FromIntermediator", GetString(FromIntermediator), 0, 35, 1);
            // MessageReceiverDetails
            XElement messageReceiverDetails = new XElement("MessageReceiverDetails");
            //ToIdentifier
            parseToNodes(messageReceiverDetails, "ToIdentifier", GetString(ToIdentifier, true), 0, 35, 1, identiferSchemaAttribute, null, true);
            //ToIntermediator
            parseToNodes(messageReceiverDetails, "ToIntermediator", GetString(ToIntermediator, true), 0, 35, 1, null, null, true);
            // MessageDetails
            XElement messageDetails = new XElement("MessageDetails");
            // MessageIdentifier

            if (isAttachment)
            {
                parseToNodes(messageDetails, "MessageIdentifier", GetMessageAttachmentIdentifier(MessageIdentifier), 15, 61, 1);
            }
            else
            {
                parseToNodes(messageDetails, "MessageIdentifier", GetString(MessageIdentifier), 0, 35, 1);
            }

            // MessageTimeStamp
            parseToNodes(messageDetails, "MessageTimeStamp", GetString(MessageTimeStamp), 0, 35, 1);

            if (isAttachment)
            {
                parseToNodes(messageDetails, "RefToMessageIdentifier", GetString(MessageIdentifier), 0, 35, 1);
            }
            else
            {
                parseToNodes(messageDetails, "SpecificationIdentifier", GetString(SpecificationIdentifier), 0, 35, 1);
            }

            if (messageSenderDetails.HasElements)
                messageTransmissionDetails.Add(messageSenderDetails);
            if (messageReceiverDetails.HasElements)
                messageTransmissionDetails.Add(messageReceiverDetails);
            if (messageDetails.HasElements)
                messageTransmissionDetails.Add(messageDetails);

            root.Add(messageTransmissionDetails);
        }
    }

    public class SellerPartyDetails : FinvoiceBase
    {
        public string SellerPartyIdentifier { get; set; }
        public string SellerOrganisationName { get; set; }
        public string SellerOrganisationDepartment { get; set; }
        public string SellerOrganisationTaxCode { get; set; }

        #region SellerPostalAddressDetails

        public String SellerStreetName { get; set; }
        public String SellerTownName { get; set; }
        public String SellerPostCodeIdentifier { get; set; }
        public String CountryCode { get; set; }
        public String CountryName { get; set; }
        public String SellerPostOfficeBoxIdentifier { get; set; }

        #endregion

        public void AddNode(ref XElement root, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            XElement sellerPartyDetails = new XElement("SellerPartyDetails");

            // SellerPartyIdentifier
            parseToNodes(sellerPartyDetails, "SellerPartyIdentifier", GetString(SellerPartyIdentifier), 0, 35, 1);

            // SellerPartyIdentifierUrlText
            if (IsFinvoiceV2orV3(eInvoiceFormat))
                sellerPartyDetails.Add(new XElement("SellerPartyIdentifierUrlText", "https://tietopalvelu.ytj.fi/yrityshaku.aspx"));

            // SellerOrganisationName - max 35 chars per node in Finvoice 1.3 and max 70 chars per node in Finvoice 2.01
            int maxLen = IsFinvoiceV2orV3(eInvoiceFormat) ? 70 : 35;

            parseToNodes(sellerPartyDetails, "SellerOrganisationName", GetString(SellerOrganisationName), 2, maxLen, 10);

            // SellerOrganisationTaxCode
            if (SellerOrganisationTaxCode.HasValue())
                sellerPartyDetails.Add(new XElement("SellerOrganisationTaxCode", GetString(SellerOrganisationTaxCode)));

            #region SellerPostalAddressDetails
            XElement sellerPostalAddressDetails = new XElement("SellerPostalAddressDetails");

            parseToNodes(sellerPostalAddressDetails, "SellerStreetName", GetString(SellerStreetName), 2, 35, 3);
            parseToNodes(sellerPostalAddressDetails, "SellerTownName", GetString(SellerTownName), 2, 35, 1);
            parseToNodes(sellerPostalAddressDetails, "SellerPostCodeIdentifier", GetString(SellerPostCodeIdentifier), 2, 35, 1);
            parseToNodes(sellerPostalAddressDetails, "CountryCode", GetString(CountryCode), 2, 2, 1);
            parseToNodes(sellerPostalAddressDetails, "CountryName", GetString(CountryName), 0, 35, 1);
            parseToNodes(sellerPostalAddressDetails, "SellerPostOfficeBoxIdentifier", GetString(SellerPostOfficeBoxIdentifier), 0, 35, 1);

            if (sellerPostalAddressDetails.HasElements)
                sellerPartyDetails.Add(sellerPostalAddressDetails);
            #endregion

            root.Add(sellerPartyDetails);
        }

        public void ParseNode(XElement sellerPartyDetailsElement)
        {
            if (sellerPartyDetailsElement != null)
            {
                SellerPartyIdentifier = XmlUtil.GetChildElementValue(sellerPartyDetailsElement, "SellerPartyIdentifier");
                SellerOrganisationName = XmlUtil.GetChildElementValue(sellerPartyDetailsElement, "SellerOrganisationName");
                SellerOrganisationDepartment = XmlUtil.GetChildElementValue(sellerPartyDetailsElement, "SellerOrganisationDepartment");
                SellerOrganisationTaxCode = XmlUtil.GetChildElementValue(sellerPartyDetailsElement, "SellerOrganisationTaxCode");
            }

        }
    }

    public class SellerCommunicationDetails : FinvoiceBase
    {
        public String SellerPhoneNumberIdentifier { get; set; }
        public String SellerEmailaddressIdentifier { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement sellerCommunicationDetails = new XElement("SellerCommunicationDetails");

            parseToNodes(sellerCommunicationDetails, "SellerPhoneNumberIdentifier", GetString(SellerPhoneNumberIdentifier), 0, 35, 1);
            parseToNodes(sellerCommunicationDetails, "SellerEmailaddressIdentifier", GetString(SellerEmailaddressIdentifier), 0, 70, 1);

            if (sellerCommunicationDetails.HasElements)
                root.Add(sellerCommunicationDetails);
        }
    }

    public class SellerInformationDetails : FinvoiceBase
    {
        public String SellerHomeTownName { get; set; }
        public String SellerVatRegistrationText { get; set; }
        public String SellerPhoneNumber { get; set; }
        public String SellerFaxNumber { get; set; }
        public String SellerWebaddressIdentifier { get; set; }
        public String SellerFreeText { get; set; }
        public String SellerIBAN { get; set; }
        public String SellerBIC { get; set; }
        public List<Tuple<string, string>> SellerBicAndIbans { get; set; }

        public SellerInformationDetails()
        {
            SellerBicAndIbans = new List<Tuple<string, string>>();
        }

        public void AddNode(ref XElement root)
        {
            XElement sellerInformationDetails = new XElement("SellerInformationDetails");

            parseToNodes(sellerInformationDetails, "SellerHomeTownName", GetString(SellerHomeTownName), 0, 35, 1);
            parseToNodes(sellerInformationDetails, "SellerVatRegistrationText", GetString(SellerVatRegistrationText), 0, 35, 1);
            parseToNodes(sellerInformationDetails, "SellerPhoneNumber", GetString(SellerPhoneNumber), 0, 35, 1);
            parseToNodes(sellerInformationDetails, "SellerFaxNumber", GetString(SellerFaxNumber), 0, 35, 1);
            parseToNodes(sellerInformationDetails, "SellerWebaddressIdentifier", GetString(SellerWebaddressIdentifier), 0, 70, 1);
            parseToNodes(sellerInformationDetails, "SellerFreeText", GetString(SellerFreeText), 0, 512, 1);

            foreach (var sellerBicIban in SellerBicAndIbans)
            {
                //Bank information Own use XE
                XElement sellerAccountDetails = new XElement("SellerAccountDetails");

                parseToNodes(sellerAccountDetails, "SellerAccountID", sellerBicIban.Item2, 2, 35, 1, new XAttribute("IdentificationSchemeName", "IBAN"));
                parseToNodes(sellerAccountDetails, "SellerBic", sellerBicIban.Item1, 8, 11, 1, new XAttribute("IdentificationSchemeName", "BIC"));

                if (sellerInformationDetails.HasElements)
                    sellerInformationDetails.Add(sellerAccountDetails);
            }

            if (sellerInformationDetails.HasElements)
                root.Add(sellerInformationDetails);
        }

    }

    public class BuyerPartyDetails : FinvoiceBase
    {
        public string BuyerPartyIdentifier { get; set; }
        public string BuyerOrganisationName { get; set; }
        public string BuyerOrganisationDepartment { get; set; }
        public string BuyerOrganisationTaxCode { get; set; }

        #region BuyerPostalAddressDetails

        public readonly List<string> BuyerStreetName = new List<string>();

        public String BuyerTownName { get; set; }
        public String BuyerPostCodeIdentifier { get; set; }
        public String CountryCode { get; set; }
        public String CountryName { get; set; }
        public String BuyerPostOfficeBoxIdentifier { get; set; }
        #endregion

        public void AddNode(ref XElement root, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            XElement buyerPartyDetails = new XElement("BuyerPartyDetails");

            // BuyerPartyIdentifier
            parseToNodes(buyerPartyDetails, "BuyerPartyIdentifier", GetString(BuyerPartyIdentifier), 0, 35, 1);

            // BuyerOrganisationName - max 35 chars per node in Finvoice 1.3 and max 70 chars per node in Finvoice 2.01
            int maxLen = IsFinvoiceV2orV3(eInvoiceFormat) ? 70 : 35;
            parseToNodes(buyerPartyDetails, "BuyerOrganisationName", GetString(BuyerOrganisationName), 2, maxLen, 10, splitOnChar: BuyerOrganisationName.Contains(';'), charToSplitOn: ';');

            // BuyerOrganisationDepartment
            parseToNodes(buyerPartyDetails, "BuyerOrganisationDepartment", GetString(BuyerOrganisationDepartment), 0, 35, 2);

            // BuyerOrganisationTaxCode
            parseToNodes(buyerPartyDetails, "BuyerOrganisationTaxCode", GetString(BuyerOrganisationTaxCode), 0, 35, 2);

            #region BuyerPostalAddressDetails
            XElement buyerPostalAddressDetails = new XElement("BuyerPostalAddressDetails");

            foreach (var buyerStreeName in BuyerStreetName)
            {
                parseToNodes(buyerPostalAddressDetails, "BuyerStreetName", GetString(buyerStreeName), 2, 35, 3);
            }

            parseToNodes(buyerPostalAddressDetails, "BuyerTownName", GetString(BuyerTownName), 2, 35, 1);
            parseToNodes(buyerPostalAddressDetails, "BuyerPostCodeIdentifier", GetString(BuyerPostCodeIdentifier), 2, 35, 1);

            if (buyerPostalAddressDetails.HasElements)
            {
                parseToNodes(buyerPostalAddressDetails, "CountryCode", GetString(CountryCode), 2, 2, 1);
                parseToNodes(buyerPostalAddressDetails, "CountryName", GetString(CountryName), 0, 35, 1);
                parseToNodes(buyerPostalAddressDetails, "BuyerPostOfficeBoxIdentifier", GetString(BuyerPostOfficeBoxIdentifier), 0, 35, 1);
            }

            if (buyerPostalAddressDetails.HasElements)
                buyerPartyDetails.Add(buyerPostalAddressDetails);
            #endregion

            root.Add(buyerPartyDetails);
        }
    }

    public class BuyerCommunicationDetails : FinvoiceBase
    {
        public string BuyerPhoneNumberIdentifier { get; set; }
        public string BuyerEmailaddressIdentifier { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement buyerCommunicationDetails = new XElement("BuyerCommunicationDetails");

            parseToNodes(buyerCommunicationDetails, "BuyerPhoneNumberIdentifier", GetString(BuyerPhoneNumberIdentifier), 0, 35, 1);
            parseToNodes(buyerCommunicationDetails, "BuyerEmailaddressIdentifier", GetString(BuyerEmailaddressIdentifier), 0, 70, 1);

            if (buyerCommunicationDetails.HasElements)
                root.Add(buyerCommunicationDetails);
        }
    }

    public class DeliveryPartyDetails : FinvoiceBase
    {
        public string DeliveryPartyIdentifier { get; set; }
        public string DeliveryOrganisationName { get; set; }
        public string DeliveryOrganisationDepartment { get; set; }
        public string DeliveryOrganisationTaxCode { get; set; }

        #region DeliveryPostalAddressDetails

        public String DeliveryStreetName { get; set; }
        public String DeliveryTownName { get; set; }
        public String DeliveryPostCodeIdentifier { get; set; }
        public String CountryCode { get; set; }
        public String CountryName { get; set; }
        public String DeliveryPostofficeBoxIdentifier { get; set; }

        #endregion

        public void AddNode(ref XElement root)
        {
            XElement deliveryPartyDetails = new XElement("DeliveryPartyDetails");

            // DeliveryPartyIdentifier
            parseToNodes(deliveryPartyDetails, "DeliveryPartyIdentifier", GetString(DeliveryPartyIdentifier), 0, 35, 1);

            // DeliveryOrganisationName
            parseToNodes(deliveryPartyDetails, "DeliveryOrganisationName", GetString(DeliveryOrganisationName), 2, 35, 10);

            // DeliveryOrganisationDepartment
            parseToNodes(deliveryPartyDetails, "DeliveryOrganisationDepartment", GetString(DeliveryOrganisationDepartment), 0, 35, 2);

            // DeliveryOrganisationTaxCode
            parseToNodes(deliveryPartyDetails, "DeliveryOrganisationTaxCode", GetString(DeliveryOrganisationTaxCode), 0, 35, 1);

            #region DeliveryPostalAddressDetails

            XElement deliveryPostalAddressDetails = new XElement("DeliveryPostalAddressDetails");

            parseToNodes(deliveryPostalAddressDetails, "DeliveryStreetName", GetString(DeliveryStreetName), 2, 35, 3);
            parseToNodes(deliveryPostalAddressDetails, "DeliveryTownName", GetString(DeliveryTownName), 2, 35, 1);
            parseToNodes(deliveryPostalAddressDetails, "DeliveryPostCodeIdentifier", GetString(DeliveryPostCodeIdentifier), 2, 35, 1);
            parseToNodes(deliveryPostalAddressDetails, "CountryCode", GetString(CountryCode), 2, 2, 1);
            parseToNodes(deliveryPostalAddressDetails, "CountryName", GetString(CountryName), 0, 35, 1);
            parseToNodes(deliveryPostalAddressDetails, "DeliveryPostofficeBoxIdentifier", GetString(DeliveryPostofficeBoxIdentifier), 0, 35, 1);

            if (DeliveryOrganisationName.HasValue() && DeliveryOrganisationName.Length >= 2 && DeliveryStreetName.HasValue() && DeliveryStreetName.Length >= 2 &&
                DeliveryTownName.HasValue() && DeliveryTownName.Length >= 2 && DeliveryPostCodeIdentifier.HasValue() && DeliveryPostCodeIdentifier.Length >= 2)
            {
                deliveryPartyDetails.Add(deliveryPostalAddressDetails);
                root.Add(deliveryPartyDetails);
            }

            #endregion

        }
    }

    public class DeliveryDetails : FinvoiceBase
    {
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryMethodText { get; set; }
        public string DeliveryTermsText { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement deliveryDetails = new XElement("DeliveryDetails");

            parseToNodes(deliveryDetails, "DeliveryDate", GetDate(DeliveryDate), 8, 8, 1, new XAttribute("Format", FINVOICE_DATE_FORMAT));
            parseToNodes(deliveryDetails, "DeliveryMethodText", GetString(DeliveryMethodText), 0, 512, 1);
            parseToNodes(deliveryDetails, "DeliveryTermsText", GetString(DeliveryTermsText), 0, 512, 1);

            if (deliveryDetails.HasElements)
                root.Add(deliveryDetails);
        }
    }

    public class InvoiceDetails : FinvoiceBase
    {
        public string InvoiceTypeCode { get; set; }

        public decimal InvoiceTotalVatIncludedAmount { get; set; }
        public int SoeCompatibleBillingType
        {
            get
            {
                if ((InvoiceTypeCode == "INV01" || InvoiceTypeCode == "INV02") && InvoiceTotalVatIncludedAmount > 0)
                    return (int)TermGroup_BillingType.Debit;
                else if ((InvoiceTypeCode == "INV01" || InvoiceTypeCode == "INV02") && InvoiceTotalVatIncludedAmount < 0)
                    return (int)TermGroup_BillingType.Credit;
                else if (InvoiceTypeCode == "INV03")
                    return (int)TermGroup_BillingType.Interest;
                else
                    return (int)TermGroup_BillingType.Debit; //must have a value that is not 0 or else the finvoice will not be vivible in grid.
            }
        }
        public string InvoiceTypeCodeUN { get; set; }
        public string InvoiceTypeText { get; set; }
        public string OriginCode { get; set; }
        public string OriginText { get; set; }
        public string SellerReferenceIdentifier { get; set; }
        public string SellersBuyerIdentifier { get; set; }
        public string OrderIdentifier { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string OriginalInvoiceNumber { get; set; }
        public decimal InvoiceTotalVatAmount { get; set; }
        public string VatAmountCurrencyIdentifier { get; set; }
        public string VatIncludedAmountCurrencyIdentifier { get; set; }
        public decimal InvoiceTotalVatExcludedAmount { get; set; }
        public decimal RowsTotalVatExcludedAmount { get; set; }
        public string VatExcludedAmountCurrencyIdentifier { get; set; }
        public string VatText { get; set; }
        public string BuyerReferenceIdentifier { get; set; }
        public string WorkSiteKey { get; set; }
        public string WorkSiteNumber { get; set; }
        public DateTime? InvoiceDueDate { get; set; } //Used only for import, TODO: Import and export should use the same variable        
        public int VatType { get; set; }
        public decimal VatRatePercent { get; set; }
        public DateTime? TimeDiscountDate { get; set; } //Used only for import, TODO: Import and export should use the same variable        
        public decimal TimeDiscountPercent { get; set; } //Used only for import, TODO: Import and export should use the same variable        

        public string AgreementIdentifier { get; set; }

        public readonly List<VatSpecificationDetails> VatSpecificationDetails = new List<VatSpecificationDetails>();
        public readonly List<PaymentTermsDetails> PaymentTerms = new List<PaymentTermsDetails>();
        public readonly List<DefinitionDetails> DefinitionDetails = new List<DefinitionDetails>();
        public readonly List<string> FreeTexts = new List<string>();

        //Do they exist in finvoice xml?
        public string PostalGiro { get; set; }
        public string BankGiro { get; set; }

        public void ParseNode(XElement invoiceDetailsElement)
        {
            if (invoiceDetailsElement != null)
            {
                #region Parse

                #region Head

                InvoiceTypeCode = XmlUtil.GetChildElementValue(invoiceDetailsElement, "InvoiceTypeCode");
                InvoiceNumber = XmlUtil.GetChildElementValue(invoiceDetailsElement, "InvoiceNumber");
                InvoiceDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(invoiceDetailsElement, "InvoiceDate"));
                OriginalInvoiceNumber = XmlUtil.GetChildElementValue(invoiceDetailsElement, "OriginalInvoiceNumber");
                InvoiceTotalVatAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceDetailsElement, "InvoiceTotalVatAmount"));
                var InvoiceTotalVatAmountElement = (from e in invoiceDetailsElement.Elements()
                                                    where e.Name.LocalName == "InvoiceTotalVatAmount"
                                                    select e).FirstOrDefault();
                VatAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(InvoiceTotalVatAmountElement, "AmountCurrencyIdentifier");

                InvoiceTotalVatIncludedAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceDetailsElement, "InvoiceTotalVatIncludedAmount"));
                var InvoiceTotalVatIncludedAmountElement = (from e in invoiceDetailsElement.Elements()
                                                            where e.Name.LocalName == "InvoiceTotalVatIncludedAmount"
                                                            select e).FirstOrDefault();
                VatIncludedAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(InvoiceTotalVatIncludedAmountElement, "AmountCurrencyIdentifier");


                InvoiceTotalVatExcludedAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceDetailsElement, "InvoiceTotalVatExcludedAmount"));
                var InvoiceTotalVatExcludedAmountElement = (from e in invoiceDetailsElement.Elements()
                                                            where e.Name.LocalName == "InvoiceTotalVatIncludedAmount"
                                                            select e).FirstOrDefault();

                VatExcludedAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(InvoiceTotalVatExcludedAmountElement, "AmountCurrencyIdentifier");

                var PaymentTermsDetails = (from e in invoiceDetailsElement.Elements()
                                           where e.Name.LocalName == "PaymentTermsDetails"
                                           select e);

                foreach (var paymentTemrmsDetail in PaymentTermsDetails)
                {
                    DateTime? prelDiscountDate = null;
                    decimal? prelDiscountPercent = null;

                    var invDueDateElement = XmlUtil.GetChildElementValue(paymentTemrmsDetail, "InvoiceDueDate");
                    if (invDueDateElement != String.Empty)
                        InvoiceDueDate = CalendarUtility.GetNullableDateTime(invDueDateElement);
                    var cashDiscountDateElement = XmlUtil.GetChildElementValue(paymentTemrmsDetail, "CashDiscountDate");
                    if (cashDiscountDateElement != String.Empty)
                        prelDiscountDate = CalendarUtility.GetNullableDateTime(cashDiscountDateElement);
                    var cashDiscountPercentElement = XmlUtil.GetChildElementValue(paymentTemrmsDetail, "CashDiscountPercent");
                    if (cashDiscountPercentElement != String.Empty)
                        prelDiscountPercent = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(cashDiscountPercentElement);

                    if (prelDiscountPercent.HasValue && prelDiscountPercent > TimeDiscountPercent)
                    {
                        TimeDiscountDate = prelDiscountDate;
                        TimeDiscountPercent = prelDiscountPercent.Value;
                    }
                }

                BuyerReferenceIdentifier = XmlUtil.GetChildElementValue(invoiceDetailsElement, "BuyerReferenceIdentifier");

                foreach (XElement definitiondetail in XmlUtil.GetChildElements(invoiceDetailsElement, "DefinitionDetails"))
                {
                    XElement definitionHeaderText = XmlUtil.GetChildElement(definitiondetail, "DefinitionHeaderText");
                    if (XmlUtil.GetAttributeStringValue(definitionHeaderText, "DefinitionCode") == "TA0001")
                        WorkSiteKey = XmlUtil.GetChildElementValue(definitiondetail, "DefinitionValue");
                    else if (XmlUtil.GetAttributeStringValue(definitionHeaderText, "DefinitionCode") == "TA0002")
                        WorkSiteNumber = XmlUtil.GetChildElementValue(definitiondetail, "DefinitionValue");
                }

                //VatSpesification

                var vatSpeficationDetails = (from e in invoiceDetailsElement.Elements()
                                             where e.Name.LocalName == "VatSpecificationDetails"
                                             select e);

                if (vatSpeficationDetails.Count() == 1)
                {
                    var vatDetails = vatSpeficationDetails.FirstOrDefault();

                    VatRatePercent = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(vatDetails, "VatRatePercent"));
                    var vatAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(vatDetails, "VatRateAmount"));
                    var vatCode = XmlUtil.GetChildElementValue(vatDetails, "VatCode");

                    switch (vatCode)
                    {
                        case "AE":
                            VatType = (int)TermGroup_InvoiceVatType.Contractor;
                            break;
                        case "O":
                        case "Z":
                            VatType = vatAmount == 0 ? (int)TermGroup_InvoiceVatType.NoVat : (int)TermGroup_InvoiceVatType.Merchandise;
                            break;
                        default:
                            VatType = (int)TermGroup_InvoiceVatType.Merchandise;
                            break;
                    }
                }
                else
                {
                    var vatAmount = 0m;
                    var hasVatCodeAE = false;
                    var hasVatCodeOZ = false;
                    foreach (var vatDetails in vatSpeficationDetails)
                    {
                        if (VatRatePercent == 0)
                            VatRatePercent = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(vatDetails, "VatRatePercent"));

                        vatAmount += NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(vatDetails, "VatRateAmount"));
                        var vatCode = XmlUtil.GetChildElementValue(vatDetails, "VatCode");

                        switch (vatCode)
                        {
                            case "AE":
                                hasVatCodeAE = true;
                                break;
                            case "O":
                            case "Z":
                                hasVatCodeOZ = true;
                                break;
                        }
                    }

                    if (hasVatCodeAE)
                        VatType = (int)TermGroup_InvoiceVatType.Contractor;
                    else if (hasVatCodeOZ && vatAmount == 0)
                        VatType = (int)TermGroup_InvoiceVatType.NoVat;
                    else
                        VatType = (int)TermGroup_InvoiceVatType.Merchandise;
                }

                /*var vatSpeficationDetails = (from e in invoiceDetailsElement.Elements()
                                             where e.Name.LocalName == "VatSpecificationDetails"
                                             select e).FirstOrDefault();

                VatRatePercent = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(vatSpeficationDetails, "VatRatePercent"));

                if (decimal.Equals(VatRatePercent, 0.00))
                    VatType = (int)TermGroup_InvoiceVatType.NoVat;
                else
                    VatType = (int)TermGroup_InvoiceVatType.Merchandise;*/

                #endregion

                #endregion
            }
        }

        public void AddNode(ref XElement root, TermGroup_EInvoiceFormat eInvoiceFormat)
        {

            XElement invoiceDetailsElement = new XElement("InvoiceDetails");

            parseToNodes(invoiceDetailsElement, "InvoiceTypeCode", GetString(InvoiceTypeCode), 5, 5, 1);
            if (IsFinvoiceV3(eInvoiceFormat))
            {
                parseToNodes(invoiceDetailsElement, "InvoiceTypeCodeUN", GetString(InvoiceTypeCodeUN), 1, 3, 1);
            }
            parseToNodes(invoiceDetailsElement, "InvoiceTypeText", GetString(InvoiceTypeText), 1, 35, 1);
            parseToNodes(invoiceDetailsElement, "OriginCode", GetString(OriginCode), 1, 35, 1);
            parseToNodes(invoiceDetailsElement, "OriginText", GetString(OriginText), 0, 35, 1);
            parseToNodes(invoiceDetailsElement, "InvoiceNumber", GetString(InvoiceNumber), 1, 20, 1);
            parseToNodes(invoiceDetailsElement, "InvoiceDate", GetDate(InvoiceDate), 8, 8, 1, new XAttribute("Format", FINVOICE_DATE_FORMAT));
            parseToNodes(invoiceDetailsElement, "OriginalInvoiceNumber", GetString(OriginalInvoiceNumber), 1, 20, 1);

            var identifierMaxLen = IsFinvoiceV3(eInvoiceFormat) ? 70 : 35;
            if (!SellerReferenceIdentifier.IsNullOrEmpty())
                parseToNodes(invoiceDetailsElement, "SellerReferenceIdentifier", GetString(SellerReferenceIdentifier), 0, identifierMaxLen, 1);
            parseToNodes(invoiceDetailsElement, "SellersBuyerIdentifier", GetString(SellersBuyerIdentifier), 0, identifierMaxLen, 1);
            parseToNodes(invoiceDetailsElement, "OrderIdentifier", GetString(OrderIdentifier), 0, identifierMaxLen, 1);
            if (!AgreementIdentifier.IsNullOrEmpty())
                parseToNodes(invoiceDetailsElement, "AgreementIdentifier", GetString(AgreementIdentifier), 0, identifierMaxLen, 1);

            // definitionDetails
            foreach (var definitionDetail in DefinitionDetails.Where(i => i.DefinitionValue.HasValue()))
            {
                definitionDetail.AddNode(ref invoiceDetailsElement);
            }
            if (IsFinvoiceV3(eInvoiceFormat))
            {
                parseToNodes(invoiceDetailsElement, "RowsTotalVatExcludedAmount", GetDecimal(RowsTotalVatExcludedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(VatExcludedAmountCurrencyIdentifier)));
            }
            parseToNodes(invoiceDetailsElement, "InvoiceTotalVatExcludedAmount", GetDecimal(InvoiceTotalVatExcludedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(VatExcludedAmountCurrencyIdentifier)));
            parseToNodes(invoiceDetailsElement, "InvoiceTotalVatAmount", GetDecimal(InvoiceTotalVatAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(VatAmountCurrencyIdentifier)));
            parseToNodes(invoiceDetailsElement, "InvoiceTotalVatIncludedAmount", GetDecimal(InvoiceTotalVatIncludedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(VatIncludedAmountCurrencyIdentifier)));

            // vatSpecificationDetails
            foreach (var vatSpecificationDetail in VatSpecificationDetails)
            {
                vatSpecificationDetail.AddNode(ref invoiceDetailsElement, eInvoiceFormat);
            }

            foreach (var freeText in FreeTexts)
            {
                string freeTxt = freeText.Replace("\x0D0A", ";");
                freeTxt = freeTxt.Replace("\x0D", ";");
                freeTxt = freeTxt.Replace("\x0A", ";");
                parseToNodes(invoiceDetailsElement, "InvoiceFreeText", GetString(freeTxt), 0, 512, 10, splitOnChar: freeTxt.Contains(';'), charToSplitOn: ';');
            }

            foreach (var paymentTerm in PaymentTerms)
            {
                paymentTerm.AddNode(ref invoiceDetailsElement, eInvoiceFormat);
            }

            root.Add(invoiceDetailsElement);
        }
    }

    public class VatSpecificationDetails : FinvoiceBase
    {
        public decimal VatBaseAmount { get; set; }
        public string VatBaseAmountCurrencyIdentifier { get; set; }
        public decimal VatRatePercent { get; set; }
        public string VatCode { get; set; }
        public decimal VatRateAmount { get; set; }
        public string VatRateAmountCurrencyIdentifier { get; set; }
        public string VatFreeText { get; set; }

        public void AddNode(ref XElement invoiceDetailsElement, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            XElement vatSpecificationDetails = new XElement("VatSpecificationDetails");

            parseToNodes(vatSpecificationDetails, "VatBaseAmount", GetDecimal(VatBaseAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(VatBaseAmountCurrencyIdentifier)));
            parseToNodes(vatSpecificationDetails, "VatRatePercent", GetDecimal(VatRatePercent), 1, 7, 1);
            if (IsFinvoiceV3(eInvoiceFormat))
                parseToNodes(vatSpecificationDetails, "VatCode", GetString(VatCode), 0, 10, 3);
            parseToNodes(vatSpecificationDetails, "VatRateAmount", GetDecimal(VatRateAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(VatRateAmountCurrencyIdentifier)));

            parseToNodes(vatSpecificationDetails, "VatFreeText", GetString(VatFreeText), 0, IsFinvoiceV2orV3(eInvoiceFormat) ? 70 : 35, 3);


            if (vatSpecificationDetails.HasElements)
                invoiceDetailsElement.Add(vatSpecificationDetails);
        }
    }

    public class DefinitionDetails : FinvoiceBase
    {
        public string DefinitionHeaderText { get; set; }
        public string DefinitionValue { get; set; }
        public string DefinitionCode { get; set; }

        public void AddNode(ref XElement invoiceDetailsElement)
        {
            XElement definitionDetails = new XElement("DefinitionDetails");

            parseToNodes(definitionDetails, "DefinitionHeaderText", GetString(DefinitionHeaderText), 0, 70, 1, new XAttribute("DefinitionCode", GetString(DefinitionCode)));
            parseToNodes(definitionDetails, "DefinitionValue", GetString(DefinitionValue), 0, 70, 1);

            if (definitionDetails.HasElements)
                invoiceDetailsElement.Add(definitionDetails);
        }
    }

    public class PaymentTermsDetails : FinvoiceBase
    {
        public string PaymentTermsFreeText { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public string PaymentOverDueFineFreeText { get; set; }
        public decimal PaymentOverDueFinePercent { get; set; }
        public decimal PaymentOverDueFixedAmount { get; set; }
        public string PaymentOverDueFixedAmountCurrencyIdentifier { get; set; }
        public DateTime? CashDiscountDate { get; set; }
        public decimal? CashDiscountPercent { get; set; }
        public decimal CashDiscountAmount { get; set; }

        public void AddNode(ref XElement invoiceDetailsElement, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            XElement paymentTermsDetails = new XElement("PaymentTermsDetails");

            int minLen = 1;
            int maxLen = 35;
            if (IsFinvoiceV2orV3(eInvoiceFormat))
            {
                minLen = 0;
                maxLen = 70;
            }

            parseToNodes(paymentTermsDetails, "PaymentTermsFreeText", GetString(PaymentTermsFreeText), minLen, maxLen, 2);
            parseToNodes(paymentTermsDetails, "InvoiceDueDate", GetDate(InvoiceDueDate), 8, 8, 1, new XAttribute("Format", FINVOICE_DATE_FORMAT));

            if (CashDiscountDate.HasValue && CashDiscountPercent.HasValue)
            {
                parseToNodes(paymentTermsDetails, "CashDiscountDate", GetDate(CashDiscountDate), 8, 8, 1, new XAttribute("Format", FINVOICE_DATE_FORMAT));
                parseToNodes(paymentTermsDetails, "CashDiscountPercent", GetDecimal(CashDiscountPercent.Value), 1, 7, 1);
                parseToNodes(paymentTermsDetails, "CashDiscountAmount", GetDecimal(CashDiscountAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", PaymentOverDueFixedAmountCurrencyIdentifier));
            }

            #region PaymentOverDueFineDetails
            XElement paymentOverDueFineDetails = new XElement("PaymentOverDueFineDetails");

            parseToNodes(paymentOverDueFineDetails, "PaymentOverDueFineFreeText", GetString(PaymentOverDueFineFreeText), 0, 70, 3);
            parseToNodes(paymentOverDueFineDetails, "PaymentOverDueFinePercent", GetDecimal(PaymentOverDueFinePercent), 1, 7, 1);
            if (PaymentOverDueFixedAmount > 0)
                parseToNodes(paymentOverDueFineDetails, "PaymentOverDueFixedAmount", GetDecimal(PaymentOverDueFixedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", PaymentOverDueFixedAmountCurrencyIdentifier));

            if (paymentOverDueFineDetails.HasElements)
                paymentTermsDetails.Add(paymentOverDueFineDetails);
            #endregion

            if (paymentTermsDetails.HasElements)
                invoiceDetailsElement.Add(paymentTermsDetails);

        }
    }

    public class InvoiceRow : FinvoiceBase
    {
        public bool UseOnlyAsSubInvoiceRow { get; set; }
        public string ArticleIdentifier { get; set; }
        public string ArticleName { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public string DeliveredQuantityUnitCode { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public string InvoicedQuantityUnitCode { get; set; }
        public decimal UnitPriceAmount { get; set; }
        public decimal UnitPriceNetAmount { get; set; }
        public int RowPositionIndentifier { get; set; }
        public string UnitPriceAmountCurrencyIdentifier { get; set; }
        public decimal RowDiscountPercent { get; set; }
        public decimal RowDiscountAmount { get; set; }
        public decimal RowVatRatePercent { get; set; }
        public decimal RowVatAmount { get; set; }
        public string RowVatCode { get; set; }
        public string RowVatAmountCurrencyIdentifier { get; set; }
        public decimal RowVatExcludedAmount { get; set; }
        public string RowVatExcludedAmountCurrencyIdentifier { get; set; }
        public decimal RowAmount { get; set; }
        public string RowAmountCurrencyIdentifier { get; set; }
        public string RowFreeText { get; set; }

        public readonly List<SubInvoiceRow> subInvoiceRows = new List<SubInvoiceRow>();
        public readonly List<InvoiceRowProgressiveDiscount> progressiveDiscountRows = new List<InvoiceRowProgressiveDiscount>();

        public void ParseNode(XElement invoiceRowElement)
        {
            #region Row

            ArticleIdentifier = XmlUtil.GetChildElementValue(invoiceRowElement, "ArticleIdentifier");
            ArticleName = XmlUtil.GetChildElementValue(invoiceRowElement, "ArticleName");

            DeliveredQuantity = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "DeliveredQuantity"));
            var DeliveredQuantityElement = (from e in invoiceRowElement.Elements()
                                            where e.Name.LocalName == "DeliveredQuantity"
                                            select e).FirstOrDefault();
            DeliveredQuantityUnitCode = XmlUtil.GetAttributeStringValue(DeliveredQuantityElement, "QuantityUnitCode");

            InvoicedQuantity = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "InvoicedQuantity"));
            var InvoicedQuantityElement = (from e in invoiceRowElement.Elements()
                                           where e.Name.LocalName == "InvoicedQuantity"
                                           select e).FirstOrDefault();
            DeliveredQuantityUnitCode = XmlUtil.GetAttributeStringValue(InvoicedQuantityElement, "QuantityUnitCode");

            UnitPriceAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "UnitPriceAmount"));
            var UnitPriceAmountElement = (from e in invoiceRowElement.Elements()
                                          where e.Name.LocalName == "UnitPriceAmount"
                                          select e).FirstOrDefault();
            UnitPriceAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(UnitPriceAmountElement, "AmountCurrencyIdentifier");

            RowVatAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "RowVatAmount"));
            var RowVatAmountElement = (from e in invoiceRowElement.Elements()
                                       where e.Name.LocalName == "RowVatAmount"
                                       select e).FirstOrDefault();
            RowVatAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(RowVatAmountElement, "AmountCurrencyIdentifier");

            RowVatRatePercent = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "RowVatRatePercent"));
            RowVatExcludedAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "RowVatExcludedAmount"));
            var RowVatExcludedAmountElement = (from e in invoiceRowElement.Elements()
                                               where e.Name.LocalName == "RowVatExcludedAmount"
                                               select e).FirstOrDefault();
            RowVatExcludedAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(RowVatExcludedAmountElement, "AmountCurrencyIdentifier");

            RowAmount = NumberUtility.GetDecimalRemoveDuplicateSubtractionSign(XmlUtil.GetChildElementValue(invoiceRowElement, "RowAmount"));
            var RowAmountElement = (from e in invoiceRowElement.Elements()
                                    where e.Name.LocalName == "RowAmount"
                                    select e).FirstOrDefault();
            RowAmountCurrencyIdentifier = XmlUtil.GetAttributeStringValue(RowAmountElement, "AmountCurrencyIdentifier");

            #endregion
        }



        public void AddNode(ref XElement root, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            XElement invoiceRow = new XElement("InvoiceRow");

            if (UseOnlyAsSubInvoiceRow)
            {
                foreach (var subRow in subInvoiceRows)
                {
                    subRow.AddNode(ref invoiceRow);
                }
            }
            else
            {
                parseToNodes(invoiceRow, "ArticleIdentifier", GetString(ArticleIdentifier), 0, 35, 1);
                parseToNodes(invoiceRow, "ArticleName", GetString(ArticleName), 0, 100, 1);

                if (DeliveredQuantity != 0)
                {
                    parseToNodes(invoiceRow, "DeliveredQuantity", GetDecimal(DeliveredQuantity), 0, 14, 10, new XAttribute("QuantityUnitCode", DeliveredQuantityUnitCode), IsFinvoiceV3(eInvoiceFormat) ? new XAttribute("QuantityUnitCodeUN", "C62") : null);
                    parseToNodes(invoiceRow, "InvoicedQuantity", GetDecimal(DeliveredQuantity), 0, 14, 10, new XAttribute("QuantityUnitCode", DeliveredQuantityUnitCode), IsFinvoiceV3(eInvoiceFormat) ? new XAttribute("QuantityUnitCodeUN", "C62") : null);
                }
                else if (IsFinvoiceV3(eInvoiceFormat))
                {
                    //must be there but can be empty
                    invoiceRow.Add(new XElement("DeliveredQuantity", GetDecimal(1), new XAttribute("QuantityUnitCode", "kpl"), new XAttribute("QuantityUnitCodeUN", "C62")));
                    invoiceRow.Add(new XElement("InvoicedQuantity", GetDecimal(1), new XAttribute("QuantityUnitCode", "kpl"), new XAttribute("QuantityUnitCodeUN", "C62")));
                }

                if (IsFinvoiceV3(eInvoiceFormat))
                {
                    parseToNodes(invoiceRow, "UnitPriceAmount", GetDecimal(UnitPriceAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", UnitPriceAmountCurrencyIdentifier));
                    if (UnitPriceNetAmount != 0)
                        parseToNodes(invoiceRow, "UnitPriceDiscountAmount", GetDecimal(UnitPriceAmount - UnitPriceNetAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", UnitPriceAmountCurrencyIdentifier));
                    parseToNodes(invoiceRow, "UnitPriceNetAmount", GetDecimal(UnitPriceNetAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", UnitPriceAmountCurrencyIdentifier));
                }
                else
                {
                    if (UnitPriceAmount != 0)
                    {
                        parseToNodes(invoiceRow, "UnitPriceAmount", GetDecimal(UnitPriceAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", UnitPriceAmountCurrencyIdentifier));
                    }
                }

                parseToNodes(invoiceRow, "RowPositionIdentifier", GetInt(RowPositionIndentifier), 1, 7, 1);
                parseToNodes(invoiceRow, "RowFreeText", GetString(RowFreeText), 0, 512, 10, splitOnChar: GetString(RowFreeText).Contains(';'), charToSplitOn: ';');

                // Handle discount
                if(progressiveDiscountRows.Count > 1)
                {
                    foreach (var progressiveDiscountRow in progressiveDiscountRows)
                    {
                        progressiveDiscountRow.AddNode(UnitPriceAmountCurrencyIdentifier, ref invoiceRow);
                    }
                }
                else if (RowDiscountPercent != 0)
                {
                    parseToNodes(invoiceRow, "RowDiscountPercent", GetDecimal(RowDiscountPercent), 1, 7, 1);
                    parseToNodes(invoiceRow, "RowDiscountAmount", GetDecimal(RowDiscountAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", UnitPriceAmountCurrencyIdentifier));
                    parseToNodes(invoiceRow, "RowDiscountTypeText", "Alennus", 1, 10, 1);
                }

                if (IsFinvoiceV3(eInvoiceFormat))
                {
                    parseToNodes(invoiceRow, "RowVatRatePercent", GetDecimal(RowVatRatePercent), 0, 5, 3);
                    parseToNodes(invoiceRow, "RowVatCode", GetString(RowVatCode), 0, 10, 3);
                }

                parseToNodes(invoiceRow, "RowVatAmount", GetDecimal(RowVatAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", RowVatAmountCurrencyIdentifier));
                parseToNodes(invoiceRow, "RowVatExcludedAmount", GetDecimal(RowVatExcludedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", RowVatExcludedAmountCurrencyIdentifier));
                if (IsFinvoiceV3(eInvoiceFormat))
                    parseToNodes(invoiceRow, "RowAmount", GetDecimal(RowVatAmount + RowVatExcludedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", RowVatExcludedAmountCurrencyIdentifier));
            }

            if (invoiceRow.HasElements)
                root.Add(invoiceRow);
        }
    }

    public class SubInvoiceRow : FinvoiceBase
    {
        public String SubArticleName { get; set; }
        public decimal SubRowVatRatePercent { get; set; }
        public decimal SubRowVatAmount { get; set; }
        public String SubRowVatAmountCurrencyIdentifier { get; set; }
        public decimal SubRowVatExcludedAmount { get; set; }
        public String SubRowVatExcludedAmountCurrencyIdentifier { get; set; }
        public decimal SubRowAmount { get; set; }
        public String SubRowAmountIdentifier { get; set; }

        public void AddNode(ref XElement invoiceRow)
        {
            XElement subInvoiceRow = new XElement("SubInvoiceRow");

            parseToNodes(subInvoiceRow, "SubArticleName", GetString(SubArticleName), 0, 100, 1);
            if (SubRowVatRatePercent != 0)
                parseToNodes(subInvoiceRow, "SubRowVatRatePercent", GetInt(SubRowVatRatePercent), 1, 7, 1);
            if (SubRowVatAmount != 0)
                parseToNodes(subInvoiceRow, "SubRowVatAmount", GetDecimal(SubRowVatAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(SubRowVatAmountCurrencyIdentifier)));
            if (SubRowVatExcludedAmount != 0)
                parseToNodes(subInvoiceRow, "SubRowVatExcludedAmount", GetDecimal(SubRowVatExcludedAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(SubRowVatExcludedAmountCurrencyIdentifier)));
            if (SubRowAmount != 0)
                parseToNodes(subInvoiceRow, "SubRowAmount", GetDecimal(SubRowAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", GetString(SubRowAmountIdentifier)));

            if (subInvoiceRow.HasElements)
                invoiceRow.Add(subInvoiceRow);
        }
    }



    public class InvoiceRowProgressiveDiscount : FinvoiceBase
    {
        public decimal RowDiscountPercent { get; set; }
        public decimal RowDiscountAmount { get; set; }
        public string RowDiscountTypeText { get; set; }

        public void AddNode(string UnitPriceAmountCurrencyIdentifier, ref XElement invoiceRow)
        {
            XElement subInvoiceRow = new XElement("RowProgressiveDiscountDetails");

            parseToNodes(subInvoiceRow, "RowDiscountPercent", GetDecimal(RowDiscountPercent), 1, 7, 1);
            parseToNodes(subInvoiceRow, "RowDiscountAmount", GetDecimal(RowDiscountAmount), 1, 22, 1, new XAttribute("AmountCurrencyIdentifier", UnitPriceAmountCurrencyIdentifier));
            parseToNodes(subInvoiceRow, "RowDiscountTypeText", RowDiscountTypeText, 1, 10, 1);

            invoiceRow.Add(subInvoiceRow);
        }
    }

    // "Described separately by the seller or line of business eg energy, telecommunication" - def13.pdf --- meaning?
    public class SpecificationDetails : FinvoiceBase
    {
        public String DeliveryNoteReference { get; set; }
        public String CustomerReferenceNumber { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement specificationDetails = new XElement("SpecificationDetails");
            XElement references = new XElement("References");

            references.Add(new XElement("DeliveryNoteReference", GetString(DeliveryNoteReference)));
            references.Add(new XElement("CustomerReferenceNumber", GetString(CustomerReferenceNumber)));

            specificationDetails.Add(references);
            root.Add(specificationDetails);
        }
    }

    public class EpiDetails : FinvoiceBase
    {
        public DateTime? EpiDate { get; set; }
        public string EpiReference { get; set; }
        public string EpiBfiIdentifier { get; set; } //BIC
        public string EpiNameAddressDetails { get; set; }
        public string EpiAccountID { get; set; } //IBAN

        public string EpiRemittanceInfoIdentifier { get; set; } //Ocr
        public string EpiRemittanceInfoIdentifierType { get; set; }
        public decimal EpiInstructedAmount { get; set; }
        public string EpiInstructedAmountCurrencyId { get; set; }
        public string EpiCharge { get; set; }
        public string EpiChargeOption { get; set; }
        public DateTime? EpiDateOptionDate { get; set; }
        public string EpiPaymentMeansCode { get; set; }

        public void ParseNode(XElement epidetailsElement)
        {
            if (epidetailsElement != null)
            {
                var EpiPartyDetails = (from e in epidetailsElement.Elements()
                                       where e.Name.LocalName == "EpiPartyDetails"
                                       select e).FirstOrDefault();

                var EpiBfiPartyDetails = (from e in EpiPartyDetails.Elements()
                                          where e.Name.LocalName == "EpiBfiPartyDetails"
                                          select e).FirstOrDefault();

                var EpiBeneficiaryPartyDetails = (from e in EpiPartyDetails.Elements()
                                                  where e.Name.LocalName == "EpiBeneficiaryPartyDetails"
                                                  select e).FirstOrDefault();

                var EpiPaymentInstructionDetails = (from e in epidetailsElement.Elements()
                                                    where e.Name.LocalName == "EpiPaymentInstructionDetails"
                                                    select e).FirstOrDefault();


                EpiBfiIdentifier = XmlUtil.GetChildElementValue(EpiBfiPartyDetails, "EpiBfiIdentifier");
                EpiAccountID = XmlUtil.GetChildElementValue(EpiBeneficiaryPartyDetails, "EpiAccountID");
                EpiRemittanceInfoIdentifier = XmlUtil.GetChildElementValue(EpiPaymentInstructionDetails, "EpiRemittanceInfoIdentifier");
                EpiDateOptionDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(EpiPaymentInstructionDetails, "EpiDateOptionDate"));
            }
        }

        public void AddNode(ref XElement root, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            XElement epiDetails = new XElement("EpiDetails");

            #region EpiDetails

            #region EpiIdentificationDetails

            XElement epiIdentificationDetails = new XElement("EpiIdentificationDetails");

            parseToNodes(epiIdentificationDetails, "EpiDate", GetDate(EpiDate), 8, 8, 1, new XAttribute("Format", FINVOICE_DATE_FORMAT));
            epiIdentificationDetails.Add(new XElement("EpiReference"));

            #endregion

            #region EpiPartyDetails

            XElement epiPartyDetails = new XElement("EpiPartyDetails");

            #region EpiBfiPartyDetails

            XElement epiBfiPartyDetails = new XElement("EpiBfiPartyDetails");
            parseToNodes(epiBfiPartyDetails, "EpiBfiIdentifier", GetString(EpiBfiIdentifier), 8, 8, 1, new XAttribute("IdentificationSchemeName", FINVOICE_IDENTIFICATIONSCHEMENAME_BIC));

            #endregion

            epiPartyDetails.Add(epiBfiPartyDetails);

            #region EpiBeneficiaryPartyDetails

            XElement epiBeneficiaryPartyDetails = new XElement("EpiBeneficiaryPartyDetails");

            parseToNodes(epiBeneficiaryPartyDetails, "EpiNameAddressDetails", GetString(EpiNameAddressDetails), 2, 35, 1);
            parseToNodes(epiBeneficiaryPartyDetails, "EpiAccountID", GetString(EpiAccountID), 1, 34, 1, new XAttribute("IdentificationSchemeName", FINVOICE_IDENTIFICATIONSCHEMENAME_IBAN));

            #endregion
            epiPartyDetails.Add(epiBeneficiaryPartyDetails);

            #endregion

            #region EpiPaymentInstructionDetails

            XElement epiPaymentInstructionDetails = new XElement("EpiPaymentInstructionDetails");

            parseToNodes(epiPaymentInstructionDetails, "EpiRemittanceInfoIdentifier", GetString(EpiRemittanceInfoIdentifier), 2, 25, 1, new XAttribute("IdentificationSchemeName", GetString(EpiRemittanceInfoIdentifierType)));
            parseToNodes(epiPaymentInstructionDetails, "EpiInstructedAmount", GetDecimal(EpiInstructedAmount), 4, 19, 1, new XAttribute("AmountCurrencyIdentifier", GetString(EpiInstructedAmountCurrencyId)));
            //EpiCharge aiheuttaa ongelman, fiksaa se
            parseToNodes(epiPaymentInstructionDetails, "EpiCharge", GetString(EpiCharge), 0, 20, 1, new XAttribute("ChargeOption", GetString(EpiChargeOption)));
            parseToNodes(epiPaymentInstructionDetails, "EpiDateOptionDate", GetDate(EpiDateOptionDate), 8, 8, 1, new XAttribute("Format", FINVOICE_DATE_FORMAT));
            if (IsFinvoiceV3(eInvoiceFormat))
                parseToNodes(epiPaymentInstructionDetails, "EpiPaymentMeansCode", GetString(EpiPaymentMeansCode), 0, 20, 1);

            #endregion

            epiDetails.Add(epiIdentificationDetails);
            epiDetails.Add(epiPartyDetails);
            epiDetails.Add(epiPaymentInstructionDetails);

            #endregion

            root.Add(epiDetails);
        }
    }

    public class FinvoiceBase
    {
        public const string FINVOICE_DATE_FORMAT = "CCYYMMDD";
        public const string FINVOICE_IDENTIFICATIONSCHEMENAME_BIC = "BIC";
        public const string FINVOICE_IDENTIFICATIONSCHEMENAME_IBAN = "IBAN";
        public const string FINVOICE_IDENTIFICATIONSCHEMENAME_SPY = "SPY";
        public const string FINVOICE_IDENTIFICATIONSCHEMENAME_ISO = "ISO";
        public const string FINVOICE_TYPE_CODE_DEBIT = "INV01";
        public const string FINVOICE_TYPE_CODE_CREDIT = "INV02";
        public const string FINVOICE_TYPE_TEXT_DEBIT = "INVOICE";
        public const string FINVOICE_TYPE_TEXT_CREDIT = "CREDIT NOTE";
        public const string FINVOICE_ORIGIN_CODE_ORIGINAL = "Original";
        public const string FINVOICE_ORIGIN_CODE_COPY = "Copy";
        public const string FINVOICE_ORIGIN_TEXT_COPY = "Copy";

        public string GetDate(DateTime? date)
        {
            return (date.HasValue) ? date.Value.ToString("yyyyMMdd") : string.Empty;
        }

        protected bool IsFinvoiceV2(TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            return eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice2;
        }

        protected bool IsFinvoiceV3(TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            return eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice3;
        }

        protected bool IsFinvoiceV2orV3(TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            return IsFinvoiceV2(eInvoiceFormat) || IsFinvoiceV3(eInvoiceFormat);
        }

        public static bool IsFinvoice(TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            return eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice || eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice2 || eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice3;
        }

        public string GetString(string value, bool alwaysReturn)
        {
            if (alwaysReturn)
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "  ";
                }
                return value;
            }
            else
            {
                if (!string.IsNullOrEmpty(value))
                    return value;
                else
                    return string.Empty;
            }
        }

        public string GetString(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string GetDecimal(decimal value)
        {
            return value.ToString("F").Replace(".", ",");
        }
        public string GetInt(decimal value)
        {
            return ((int)value).ToString();
        }

        public string GetMessageIdentifier(CustomerInvoice invoice, TermGroup_EInvoiceFormat eInvoiceFormat)
        {
            string invoiceNr = GetString(invoice.InvoiceNr);
            string invoiceId = GetString(invoice.InvoiceId.ToString());
            return invoiceNr + "_" + invoiceId + "_" + DateTime.Now.ToString("yyMMddHHmmss");
        }

        public bool parseToNodes(XElement parentNode, string childNode, string nodeValue, int minLength, int maxLength, int maxOccurs, XAttribute attribute = null, XAttribute attribute1 = null, bool alwaysReturn = false, bool splitOnChar = false, char? charToSplitOn = null)
        {
            if (nodeValue == null || nodeValue.Length < minLength || (!nodeValue.HasValue() && alwaysReturn == false))
                return false;

            if (splitOnChar && charToSplitOn.HasValue)
            {
                var valueStrings = nodeValue.Split(charToSplitOn.Value);
                foreach (var valueStr in valueStrings)
                {
                    int occurs = 1;
                    int strLen = valueStr.Length;
                    int length = maxLength;
                    for (int i = 0; i < strLen; i += length)
                    {
                        if (i + length > strLen)
                            length = strLen - i;

                        if (occurs <= maxOccurs && valueStr.Substring(i, length).Length >= minLength)
                        {
                            if (attribute == null && attribute1 == null)
                                parentNode.Add(new XElement(childNode, valueStr.Substring(i, length)));
                            else if (attribute1 == null)
                                parentNode.Add(new XElement(childNode, valueStr.Substring(i, length), attribute));
                            else
                                parentNode.Add(new XElement(childNode, valueStr.Substring(i, length), attribute, attribute1));
                        }

                        occurs++;
                    }
                }
            }
            else
            {
                int occurs = 1;
                int strLen = nodeValue.Length;
                for (int i = 0; i < strLen; i += maxLength)
                {
                    if (i + maxLength > strLen)
                        maxLength = strLen - i;

                    if (occurs <= maxOccurs && nodeValue.Substring(i, maxLength).Length >= minLength)
                    {
                        if (attribute == null && attribute1 == null)
                            parentNode.Add(new XElement(childNode, nodeValue.Substring(i, maxLength)));
                        else if (attribute1 == null)
                            parentNode.Add(new XElement(childNode, nodeValue.Substring(i, maxLength), attribute));
                        else
                            parentNode.Add(new XElement(childNode, nodeValue.Substring(i, maxLength), attribute, attribute1));
                    }

                    occurs++;
                }
            }

            return true;
        }

        public string GetMessageAttachmentIdentifier(string messageIdentifier)
        {
            return GetString(messageIdentifier) + "::attachments";
        }

        #region Moved to payment manager
        /*public string getBIC(string iban)
        {
            string bic = string.Empty;

            // try find bic using one number
            int code = 0;
            int.TryParse(iban.Substring(4, 1), out code);

            switch (code)
            {
                case 1:
                case 2:
                    bic = "NDEAFIHH";
                    break;
                case 5:
                    bic = "OKOYFIHH";
                    break;
                case 6:
                    bic = "AABAFI22";
                    break;
                case 8:
                    bic = "DABAFIHH";
                    break;
            }

            if (bic.HasValue())
                return bic;

            // try find bic using two numbers
            int.TryParse(iban.Substring(4, 2), out code);
            switch (code)
            {
                case 31:
                    bic = "HANDFIHH";
                    break;
                case 33:
                    bic = "ESSEFIHX";
                    break;
                case 34:
                    bic = "DABAFIHH";
                    break;
                case 36:
                case 39:
                    bic = "SBANFIHH";
                    break;
                case 37:
                    bic = "DNBAFIHX";
                    break;
                case 38:
                    bic = "SWEDFIHH";
                    break;
            }

            if (bic.HasValue())
                return bic;

            // try find bic using three numbers
            int.TryParse(iban.Substring(4, 3), out code);
            switch (code)
            {
                case 400:
                case 402:
                case 403:
                case 715:
                    bic = "ITELFIHH";
                    break;
                case 405:
                case 497:
                    bic = "HELSFIHH";
                    break;
                case 717:
                    bic = "BIGKFIH1";
                    break;
                case 713:
                    bic = "CITIFIHX";
                    break;
                case 799:
                    bic = "HOLVFIHH";
                    break;
            }

            if (bic.HasValue())
                return bic;

            if (code >= 470 && code <= 478)
                bic = "POPFFI22";
            else if ((code >= 406 && code <= 408) || (code >= 410 && code <= 412) || (code >= 414 && code <= 421) || (code >= 423 && code <= 432) ||
     (code >= 435 && code <= 452) || (code >= 454 && code <= 464) || (code >= 483 && code <= 493) || (code >= 495 && code <= 496))
                bic = "ITELFIHH";


            return bic;
        }*/
        #endregion

        public string[] GetBicAndIban(PaymentInformationRow paymentInformationRow)
        {
            if (paymentInformationRow.PaymentNr.Contains('/'))
            {
                return paymentInformationRow.PaymentNr.Split('/');
            }
            else
            {
                return new string[] { paymentInformationRow.BIC, paymentInformationRow.PaymentNr };
            }
        }

    }

    public static class FInvoiceFileGen
    {
        private static void AddLog(string msg, bool error)
        {
            SysLogManager slm = new SysLogManager(null);
            if (error)
            {
                slm.AddSysLogErrorMessage(Environment.MachineName, "FInvoiceHTMLGen error", msg);
            }
            else
            {
                slm.AddSysLogInfoMessage(Environment.MachineName, "FInvoiceHTMLGen info", msg);
            }
        }

        public static byte[] GetPdf(EdiEntry edientry)
        {
            var html = GetHTML(edientry);
            return PdfConvertConnector.ConvertHtmlToPdf(html, true);
        }

        public static string GetHTML(EdiEntry edientry)
        {
            //path = UrlUtil.GetFilePath(ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE, Finvoice, "xsd");
            XmlTextReader xmlReader = null;
            var sb = new StringBuilder();
            var sw = new System.IO.StringWriter(sb);
            string xslPath = "Empty";
            string pathOnServer = "empty";
            try
            {
                xslPath = ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE + "Finvoice.xsl";
                pathOnServer = ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_REPORT_PHYSICAL + Guid.NewGuid().ToString() + edientry.FileName;

                var xmlContent = edientry.XML;

                XDocument xdoc = XDocument.Parse(xmlContent);
                xdoc.Save(pathOnServer);

                xmlReader = new XmlTextReader(pathOnServer);
                var doc = new System.Xml.XPath.XPathDocument(xmlReader);

                //TODO: XslTransform is obsolete, use XslCompiledTransform if possible
                var transform = new System.Xml.Xsl.XslCompiledTransform();
                transform.Load(xslPath);
                transform.Transform(doc, null, sw);

                string result = sb.ToString();
                result = result.Replace("\"&#xA;\t\t\t\t\tsellerDetails\"", "\"sellerDetails\"");

                return result;
            }
            catch (Exception ex)
            {
                AddLog(ex.Message, true);
                return $"<html><body>{ex.Message} xslPath={xslPath} pathOnServer={pathOnServer} <body/><html/>";
            }
            finally
            {
                if (xmlReader != null)
                    xmlReader.Close();
                if (sw != null)
                    sw.Close();

                //RemoveFileFromServer(pathOnServer);
            }
        }
    }
}
