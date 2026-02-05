using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class EHFInvoiceItem : EHFInvoiceBase
    {
        /**
         * For dokumentation see : http://www.anskaffelser.no,
         * http://www.anskaffelser.no/filearchive/implementation-guide-ehf-invoice-and-creditnote-v1.5_2.pdf
         * */

        private readonly ReportGenManager rgm = new ReportGenManager(null);
        private XDocument xdoc;

        #region Root elements
        public String UBLVersion = EHF_INVOICE_UBL_VERSION_ID;
        public String CustomizationID = EHF_INVOICE_CUSTOMIZATION_ID;
        public String ProfileID = EHF_INVOICE_PROFILE_ID;
        public String ID { get; set; }
        public DateTime IssueDate { get; set; }
        public int InvoiceTypeCode { get; set; }
        public String Note { get; set; }
        public DateTime? TaxPointDate { get; set; }
        public String InvoiceCurrencyCode { get; set; }
        public String AccountingCost { get; set; }
        public String PaymentTermsNote { get; set; }
        //public int LineItemCountNumeric { get; set; }
        #endregion

        #region Nodes

        private InvoicePeriod invoicePeriod = new InvoicePeriod();
        private OrderReference orderReference = new OrderReference();
        private BillingReference billingReference = new BillingReference();
        private ContractDocumentReference contractDocumentReference = new ContractDocumentReference();
        private List<AdditionalDocumentReference> DocumentReferences = new List<AdditionalDocumentReference>();
        private AccountingParty accountingSupplierParty = new AccountingParty();
        private AccountingParty accountingCustomerParty = new AccountingParty();
        private PayeeParty payeeParty = new PayeeParty();
        private Delivery delivery = new Delivery();
        private List<PaymentMeans> PaymentMeansList = new List<PaymentMeans>();
        private List<AllowanceCharge> AllowanceChargesList = new List<AllowanceCharge>();
        private TaxTotal taxTotal = new TaxTotal();
        private LegalMonetaryTotal legalMonetaryTotal = new LegalMonetaryTotal();
        private List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
        
        #endregion

        public EHFInvoiceItem(CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> invoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, String exemptionReason)
        {
            Populate(invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddress, customerDeliveryAddress, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason);
        }

        private void Populate(CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> invoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, String exemptionReason)
        {
            #region Prereq

            if (invoice == null || company == null || customer == null)
                return;

            #endregion

            ID = invoice.InvoiceNr;
            IssueDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.Date : DateTime.Today;
            InvoiceTypeCode = invoice.IsCredit ? EHF_INVOICE_TYPE_CODE_CREDIT : EHF_INVOICE_TYPE_CODE_DEBIT;
            Note = (!String.IsNullOrEmpty(invoice.InvoiceHeadText)) ? invoice.InvoiceHeadText : String.Empty;

            if (!String.IsNullOrEmpty(Note))
                Note = (!String.IsNullOrEmpty(invoice.InvoiceText)) ? invoice.InvoiceText : String.Empty;

            InvoiceCurrencyCode = (invoiceCurrency != null && !String.IsNullOrEmpty(invoiceCurrency.Code)) ? invoiceCurrency.Code : "SEK"; //Måste defaultas till nåt
            TaxPointDate = null;  //Use?
            AccountingCost = String.Empty;  //Use?


            #region Nodes

            #region InvoicePeriod
            //Skipped for now
            #endregion

            #region OrderReference
            //Skipped for now
            #endregion

            #region BillingReference

            if (invoice.IsCredit)
            {
                billingReference = new BillingReference()
                {
                     CreditNoteDocumentReferenceID = invoice.InvoiceNr,
                };
            }
            else
            {
                //??
            }

            #endregion

            #region ContractDocumentReference
            //Skipped for now
            #endregion

            #region AdditionalDocumentReferences
            //Skipped for now
            #endregion

            #region Seller

            ContactAddressRow companyPostalCode = companyAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow companyPostalAddress = companyAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow companyAddressStreetName = companyAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

            //ContactECom
            ContactECom companyEmail = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
            ContactECom companyPhoneWork = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
            ContactECom companyFax = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);

            #region PartyName

            accountingSupplierParty.PartyName = !String.IsNullOrEmpty(company.Name) ? company.Name : String.Empty;
            accountingSupplierParty.PartyID = !String.IsNullOrEmpty(company.OrgNr) ? company.OrgNr : String.Empty;

            accountingSupplierParty.PartyPostalAddress = new PostalAddress();
            accountingSupplierParty.PartyPostalAddress.StreetName = companyAddressStreetName != null && !String.IsNullOrEmpty(companyAddressStreetName.Text) ? companyAddressStreetName.Text : String.Empty;
            accountingSupplierParty.PartyPostalAddress.PostalZone = companyPostalCode != null && !String.IsNullOrEmpty(companyPostalCode.Text) ? companyPostalCode.Text : String.Empty;
            accountingSupplierParty.PartyPostalAddress.CityName = companyPostalAddress != null && !String.IsNullOrEmpty(companyPostalAddress.Text) ? companyPostalAddress.Text : String.Empty;
            accountingSupplierParty.PartyPostalAddress.CountryIdentificationCode = company.SysCountryId != null && company.SysCountryId != 0 && company.SysCountryId != 1 ? ((TermGroup_Country)company.SysCountryId).ToString() : "SE"; //Mandatory

            #endregion

            #region PartyTaxScheme

            ContactAddressRow companyHQCountry = companyBoardHQAddress.GetRow(TermGroup_SysContactAddressRowType.Country);

            PartyTaxScheme partyTaxScheme = new PartyTaxScheme();
            partyTaxScheme.CompanyID = !String.IsNullOrEmpty(company.VatNr) ? company.VatNr : String.Empty;
            partyTaxScheme.TaxSchemeID = EHF_TAX_SCHEME_ID_VAT;

            accountingSupplierParty.TaxScheme = partyTaxScheme;

            #endregion

            #region LegalEntity

            accountingSupplierParty.LegalEntity = new PartyLegalEntity()
            {
                CompanyID = company.OrgNr,
                CityName = companyPostalAddress != null && !String.IsNullOrEmpty(companyPostalAddress.Text) ? companyPostalAddress.Text : String.Empty,
                CountryIdentificationCode = company.SysCountryId != null && company.SysCountryId != 0 ? ((TermGroup_Country)company.SysCountryId).ToString() : String.Empty,
            };

            #endregion

            #region Contact

            if (companyEmail != null || companyFax != null || companyPhoneWork != null)
            {
                accountingSupplierParty.PartyContact = new PartyContact()
                {
                    Email = companyEmail != null && !String.IsNullOrEmpty(companyEmail.Text) ? companyEmail.Text : String.Empty,
                    Telefax = companyFax != null && !String.IsNullOrEmpty(companyFax.Text) ? companyFax.Text : String.Empty,
                    Telephone = companyPhoneWork != null && !String.IsNullOrEmpty(companyPhoneWork.Text) ? companyPhoneWork.Text : String.Empty,
                };
            }

            #endregion

            #region Person

            if (!String.IsNullOrEmpty(invoice.ReferenceOur))
            {
                accountingSupplierParty.PartyPerson = new PartyPerson()
                {
                    FirstName = invoice.ReferenceOur, //Ett problem. Hos oss är referens en sträng, EHF vill ha det i förnamn, mellannamn, efternamn
                };
            }

            #endregion

            #endregion

            #region Buyer (Customer)

            ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

            #region PartyName

            accountingCustomerParty.PartyName = !String.IsNullOrEmpty(customer.Name) ? customer.Name : String.Empty;
            accountingCustomerParty.PartyID = !String.IsNullOrEmpty(customer.OrgNr) ? customer.OrgNr : String.Empty;

            accountingCustomerParty.PartyPostalAddress = new PostalAddress();
            accountingCustomerParty.PartyPostalAddress.StreetName = customerAddressStreetName != null && !String.IsNullOrEmpty(customerAddressStreetName.Text) ? companyAddressStreetName.Text : String.Empty;
            accountingCustomerParty.PartyPostalAddress.PostalZone = customerPostalCode != null && !String.IsNullOrEmpty(customerPostalCode.Text) ? companyPostalCode.Text : String.Empty;
            accountingCustomerParty.PartyPostalAddress.CityName = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? companyPostalAddress.Text : String.Empty;
            accountingCustomerParty.PartyPostalAddress.CountryIdentificationCode = customer.SysCountryId != null && customer.SysCountryId != 0 ? customer.SysCountryId != 1 ? ((TermGroup_Country)customer.SysCountryId).ToString() : "SE" : String.Empty;

            #region PartyTaxScheme

            PartyTaxScheme customerPartyTaxScheme = new PartyTaxScheme();
            customerPartyTaxScheme.CompanyID = !String.IsNullOrEmpty(customer.VatNr) ? customer.VatNr : String.Empty;
            customerPartyTaxScheme.TaxSchemeID = EHF_TAX_SCHEME_ID_VAT;

            accountingCustomerParty.TaxScheme = customerPartyTaxScheme;

            #endregion

            #region LegalEntity

            accountingSupplierParty.LegalEntity = new PartyLegalEntity()
            {
                CompanyID = customer.OrgNr,
                CityName = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : String.Empty,
                CountryIdentificationCode = customer.SysCountryId != null && customer.SysCountryId != 0 ? ((TermGroup_Country)customer.SysCountryId).ToString() : String.Empty,
            };

            #endregion

            #region Contact

            /*if (customer != null || companyFax != null || companyPhoneWork != null)
            {
                accountingSupplierParty.PartyContact = new Contact()
                {
                    Email = companyEmail != null && !String.IsNullOrEmpty(companyEmail.Text) ? companyEmail.Text : String.Empty,
                    Telefax = companyFax != null && !String.IsNullOrEmpty(companyFax.Text) ? companyFax.Text : String.Empty,
                    Telephone = companyPhoneWork != null && !String.IsNullOrEmpty(companyPhoneWork.Text) ? companyPhoneWork.Text : String.Empty,
                };
            }*/

            #endregion

            #region Person

            if (!String.IsNullOrEmpty(invoice.ReferenceYour))
            {
                accountingSupplierParty.PartyPerson = new PartyPerson()
                {
                    FirstName = invoice.ReferenceYour, //Ett problem. Hos oss är referens en sträng, EHF vill ha det i förnamn, mellannamn, efternamn
                };
            }

            #endregion

            #endregion

            #endregion

            #region PayeeParty

            payeeParty.PartyName = !String.IsNullOrEmpty(company.Name) ? company.Name : String.Empty;
            payeeParty.PartyID = !String.IsNullOrEmpty(company.OrgNr) ? company.OrgNr : String.Empty;

            payeeParty.LegalEntity = new PartyLegalEntity()
            {
                CompanyID = company.OrgNr,
                CityName = companyPostalAddress != null && !String.IsNullOrEmpty(companyPostalAddress.Text) ? companyPostalAddress.Text : String.Empty,
                CountryIdentificationCode = company.SysCountryId != null && company.SysCountryId != 0 ? ((TermGroup_Country)company.SysCountryId).ToString() : String.Empty,
            };

            #endregion

            #region Delivery

            ContactAddressRow deliveryPostalCode = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow deliveryPostalAddress = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow deliveryAddress = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

            if (invoice.DeliveryDate.HasValue && invoice.DeliveryDate.Value.Date != IssueDate.Date)
                delivery.ActualDeliveryDateTime = invoice.DeliveryDate.Value;

            delivery.DeliveryAddress = new PostalAddress()
            {
                CityName = (deliveryPostalAddress != null && !String.IsNullOrEmpty(deliveryPostalAddress.Text)) ? deliveryPostalAddress.Text : String.Empty,
                PostalZone = (deliveryPostalCode != null && !String.IsNullOrEmpty(deliveryPostalCode.Text)) ? deliveryPostalCode.Text : String.Empty,
                StreetName = (deliveryAddress != null && !String.IsNullOrEmpty(deliveryAddress.Text)) ? deliveryAddress.Text : String.Empty,
            };

            #endregion

            #region PaymentMeans

            if (paymentInformation != null)
            {
                PaymentInformationRow companyBg = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG);
                PaymentInformationRow companyPg = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG);
                PaymentInformationRow companyBank = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.Bank);
                PaymentInformationRow companyBic = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC);

                PaymentMeans paymentMeans = new PaymentMeans();
                paymentMeans.PaymentMeansCode = invoice.IsCredit ? EHF_INVOICE_TYPE_CODE_CREDIT.ToString() : EHF_INVOICE_TYPE_CODE_DEBIT.ToString();
                paymentMeans.PaymentChannelCode = EHF_INVOICE_PAYMENTCHANNELCODE;
                paymentMeans.PaymentID = String.IsNullOrEmpty(invoice.OCR) ? invoice.InvoiceId.ToString() : invoice.OCR; //Här måste KID nr finnas (el OCR)

                if (invoice.DueDate.HasValue)
                    paymentMeans.PaymentDueDate = invoice.DueDate.Value;
                else
                    paymentMeans.PaymentDueDate = null;

                //Priority??
                if (companyBg != null && !String.IsNullOrEmpty(companyBg.PaymentNr) && companyBg.PaymentNr.Length > 0)
                {
                    paymentMeans.PayeeFinancialAccountID = companyBg.PaymentNr;
                    //paymentMeans.FinancialInstitutionID = companyBg.;
                }
                else if (companyPg != null && !String.IsNullOrEmpty(companyPg.PaymentNr) && companyPg.PaymentNr.Length > 0)
                {
                    paymentMeans.PayeeFinancialAccountID = companyPg.PaymentNr;
                    //paymentMeans.FinancialInstitutionID = SVEFAK_FINANICIAL_INSTITUTION_ID_PG;
                }
                else if (companyBank != null && !String.IsNullOrEmpty(companyBank.PaymentNr) && companyBank.PaymentNr.Length > 0)
                {
                    paymentMeans.PayeeFinancialAccountID = companyBank.PaymentNr;
                    //paymentMeans.FinancialInstitutionID = (companyBic != null && string.IsNullOrEmpty(companyBic.PaymentNr)) ? companyBic.PaymentNr : String.Empty;
                }

                PaymentMeansList.Add(paymentMeans);
            }

            #endregion

            #region PaymentTerms
            PaymentTermsNote = (invoice.PaymentCondition != null && !String.IsNullOrEmpty(invoice.PaymentCondition.Name)) ? invoice.PaymentCondition.Name : String.Empty;
            #endregion

            #region AllowanceCharges

            decimal totalCharges = 0;

            List<CustomerInvoiceRow> freightRows = invoiceRows.Where(r => r.IsFreightAmountRow || r.IsCentRoundingRow || r.IsInterestRow || r.IsReminderRow || r.IsInvoiceFeeRow).ToList();

            foreach (CustomerInvoiceRow fRow in freightRows)
            {
                totalCharges += fRow.AmountCurrency;

                AllowanceCharge charge = new AllowanceCharge()
                {
                    ChargeIndicator = "true",
                    AllowanceChargeReason = fRow.Product.Name + " " + fRow.Text,
                    Amount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? fRow.AmountCurrency : Decimal.Negate(fRow.AmountCurrency),
                    AmountCurrencyID = InvoiceCurrencyCode,
                    CategoryTaxID = GetTaxCategoryIdentifier(fRow.VatRate, (invoice.VatType == (int)TermGroup_InvoiceVatType.Contractor || invoice.VatType == (int)TermGroup_InvoiceVatType.NoVat)),
                    CategoryTaxPercent = fRow.VatRate,
                    CategoryTaxSchemeID = EHF_TAX_SCHEME_ID_VAT,
                };

                AllowanceChargesList.Add(charge);
            }

            #endregion

            #region Taxtotal

            taxTotal.TotalTaxAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoice.VATAmountCurrency != 0 ? invoice.VATAmountCurrency : invoice.VATAmount : invoice.VATAmountCurrency != 0 ? Decimal.Negate(invoice.VATAmountCurrency) : Decimal.Negate(invoice.VATAmount);
            taxTotal.TotalTaxAmountCurrencyID = InvoiceCurrencyCode;
            #region TaxSubTotal

            List<IGrouping<decimal, CustomerInvoiceRow>> invoiceRowsGroupedByVatRates = invoiceRows.GroupBy(g => g.VatRate).ToList();

            foreach (IGrouping<decimal, CustomerInvoiceRow> invoiceRowsGroupByVatRate in invoiceRowsGroupedByVatRates)
            {
                CustomerInvoiceRow row = invoiceRowsGroupByVatRate.FirstOrDefault();
                if (row == null)
                    continue;

                decimal vatRate = row.VatRate;
                decimal taxableAmount = 0;
                decimal taxAmount = 0;

                foreach (var invoiceRow in invoiceRowsGroupByVatRate)
                {
                    if (!invoiceRow.IsCentRoundingRow)
                    {
                        taxableAmount += invoiceRow.SumAmount;
                        taxAmount += invoiceRow.VatAmount;
                    }
                }

                TaxSubTotal taxSubTotal = new TaxSubTotal();
                taxSubTotal.TaxableAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? taxableAmount : Decimal.Negate(taxableAmount);
                taxSubTotal.TaxAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? taxAmount : Decimal.Negate(taxAmount);
                taxSubTotal.TaxableAmountCurrencyID = InvoiceCurrencyCode;
                taxSubTotal.TaxAmountCurrencyID = InvoiceCurrencyCode;
                taxSubTotal.TaxCategoryID = GetTaxCategoryIdentifier(vatRate, (invoice.VatType == (int)TermGroup_InvoiceVatType.Contractor || invoice.VatType == (int)TermGroup_InvoiceVatType.NoVat));
                taxSubTotal.TaxCategoryPercent = vatRate;
                taxSubTotal.TaxSchemeID = EHF_TAX_SCHEME_ID_VAT;
                taxTotal.taxSubtotals.Add(taxSubTotal);
            }

            #endregion

            #endregion

            #region LegalTotal
            decimal prepaid = invoice.RemainingAmount != null && invoice.RemainingAmount != 0 ? invoice.TotalAmountCurrency - (decimal)invoice.RemainingAmount : 0;
            decimal payable = invoice.RemainingAmount != null && invoice.RemainingAmount != 0? (decimal)invoice.RemainingAmount : invoice.TotalAmountCurrency;

            legalMonetaryTotal.LineExtensionAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoice.SumAmountCurrency : Decimal.Negate(invoice.SumAmountCurrency); //exklusive vat
            legalMonetaryTotal.LineExtensionAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.TaxExclusiveAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoice.SumAmountCurrency + totalCharges : Decimal.Negate(invoice.SumAmountCurrency + totalCharges);
            legalMonetaryTotal.TaxExclusiveAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.TaxInclusiveAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoice.TotalAmountCurrency : Decimal.Negate(invoice.TotalAmountCurrency);
            legalMonetaryTotal.TaxInclusiveAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.AllowanceTotalAmount = AllowanceChargesList.Where(a => a.ChargeIndicator == "false").Sum(a=> a.Amount); 
            legalMonetaryTotal.AllowanceTotalAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.ChargeTotalAmount = AllowanceChargesList.Where(a => a.ChargeIndicator == "true").Sum(a => a.Amount);
            legalMonetaryTotal.ChargeTotalAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.PrepaidAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? prepaid : Decimal.Negate(prepaid);
            legalMonetaryTotal.PrepaidAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.PayableRoundingAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoice.CentRounding : Decimal.Negate(invoice.CentRounding);
            legalMonetaryTotal.PayableRoundingAmountCurrencyID = InvoiceCurrencyCode;
            legalMonetaryTotal.PayableAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? payable : Decimal.Negate(payable);
            legalMonetaryTotal.PayableAmountCurrencyID = InvoiceCurrencyCode;

            #endregion

            #region Invoiceline

            foreach (var invoiceRow in invoiceRows.Where(r => !r.IsFreightAmountRow && !r.IsCentRoundingRow && !r.IsInterestRow && !r.IsReminderRow && !r.IsInvoiceFeeRow))
            {
                /*
                public String AccountingCost { get; set; }
                public String OrderLineID { get; set; }
                List<AllowanceCharge> AllowanceCharges { get; set; }
                public decimal TaxAmount { get; set; }
                public String TaxAmountCurrencyID { get; set; }

                public String ItemDescription { get; set; }
                public String ItemName { get; set; }
                public String SellersItemIdentificationID { get; set; }
                public String StandardItemIdentificationID { get; set; }
                public List<InvoiceItemCommodityClassification> Classifications { get; set; }
                public String ClassifiedTaxCategoryID { get; set; }
                public String ClassifiedTaxCategoryPercent { get; set; }
                public String ClassifiedTaxCategoryTaxSchemeID { get; set; }
                public Dictionary<String, String> AdditionalItemProperties { get; set; }

                public decimal PriceAmount { get; set; }
                public String PriceAmountCurrencyID { get; set; }
                public decimal BaseQuantity { get; set; }
                public String BaseQuantityUnitCode { get; set; }
                public AllowanceCharge AllowanceCharge { get; set; }*/

                InvoiceLine invoiceLine = new InvoiceLine();
                invoiceLine.ID = invoiceRow.RowNr;
                invoiceLine.Note = (invoiceRow.Product != null && !String.IsNullOrEmpty(invoiceRow.Product.Description)) ? invoiceRow.Product.Description : String.Empty;
                invoiceLine.TaxAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoiceRow.VatAmountCurrency : Decimal.Negate(invoiceRow.VatAmountCurrency);
                invoiceLine.TaxAmountCurrencyID = InvoiceCurrencyCode;
                invoiceLine.InvoicedQuantity = invoiceRow.Quantity.HasValue ? invoiceRow.Quantity.Value : 0;
                invoiceLine.InvoicedQuantityUnitCode = (invoiceRow.ProductUnit != null && !String.IsNullOrEmpty(invoiceRow.ProductUnit.Name)) ? invoiceRow.ProductUnit.Name : String.Empty;
                invoiceLine.LineExtensionAmount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoiceRow.SumAmountCurrency : Decimal.Negate(invoiceRow.SumAmountCurrency);
                invoiceLine.LineExtensionAmountCurrencyID = InvoiceCurrencyCode;
                invoiceLine.ItemName = invoiceRow.Product != null ? invoiceRow.Product.Name : "";
                invoiceLine.ItemDescription = invoiceRow.Product != null ? invoiceRow.Product.Description : "";
                invoiceLine.ClassifiedTaxCategoryID = GetTaxCategoryIdentifier(invoiceRow.VatRate, (invoice.VatType == (int)TermGroup_InvoiceVatType.Contractor || invoice.VatType == (int)TermGroup_InvoiceVatType.NoVat));
                invoiceLine.ClassifiedTaxCategoryPercent = invoiceRow.VatRate;
                invoiceLine.ClassifiedTaxCategoryTaxSchemeID = EHF_TAX_SCHEME_ID_VAT;


                invoiceLine.SellersItemIdentificationID = (invoiceRow.Product != null && !String.IsNullOrEmpty(invoiceRow.Product.Number)) ? invoiceRow.Product.Number : String.Empty;
                invoiceLine.PriceAmount = invoiceRow.Amount;
                invoiceLine.PriceAmountCurrencyID = InvoiceCurrencyCode;
                //invoiceLine.BaseQuantity = invoiceRow.Quantity != null ? (decimal)invoiceRow.Quantity : 0;
                //invoiceLine.BaseQuantityUnitCode = invoiceRow.ProductUnit != null ? invoiceRow.ProductUnit.Code : String.Empty;

                if (invoiceRow.DiscountPercent > 0)
                {
                    AllowanceCharge charge = new AllowanceCharge()
                    {
                        ChargeIndicator = "false",
                        AllowanceChargeReason = String.Empty,
                        Amount = InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? invoiceRow.DiscountAmountCurrency : Decimal.Negate(invoiceRow.DiscountAmountCurrency),
                        AmountCurrencyID = InvoiceCurrencyCode,
                        CategoryTaxID = GetTaxCategoryIdentifier(invoiceRow.VatRate, (invoice.VatType == (int)TermGroup_InvoiceVatType.Contractor || invoice.VatType == (int)TermGroup_InvoiceVatType.NoVat)),
                        CategoryTaxPercent = invoiceRow.VatRate,
                        CategoryTaxSchemeID = EHF_TAX_SCHEME_ID_VAT,
                    };

                    invoiceLine.AllowanceCharges = new List<AllowanceCharge>();
                    invoiceLine.AllowanceCharges.Add(charge);
                }

                invoiceLines.Add(invoiceLine);
            }

            #endregion

            #endregion

        }

        public bool Export()
        {
            return false;
        }

        public bool Validate()
        {
            if (xdoc == null)
                return false;

            List<String> schemas = new List<String>();

            #region populate xsd paths
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "SFTI-BasicInvoice-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "SFTI-CommonAggregateComponents-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-AllowanceChargeReasonCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-ChannelCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-ChipCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-CountryIdentificationCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-CurrencyCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-DocumentStatusCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-LatitudeDirectionCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-LineStatusCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-LongitudeDirectionCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-OperatorCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-PaymentMeansCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CodeList-SubstitutionStatusCode-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CommonAggregateComponents-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CommonBasicComponents-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CoreComponentParameters-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-CoreComponentTypes-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-SpecializedDatatypes-1.0.xsd");
            schemas.Add(ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL + "UBL-UnspecializedDatatypes-1.0.xsd");
            #endregion

            return rgm.ValidateXDocument(xdoc, schemas);
        }

        public void ToXml(ref XDocument document)
        {
            //Deras
            /*<CreditNote xsi:schemaLocation="urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 UBL-CreditNote-2.0.xsd" 
             * xmlns="urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2" 
             * xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
             * xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" 
             * xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" 
             * xmlns:ext="urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2">*/

            //Min
            /*<CreditNote xsi:schemaLocation="urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 UBL-CreditNote-2.0.xsd" 
             * xmlns="urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2"> 
             * xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" 
             * xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" 
             * xmlns:ext="urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2" 
             * xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"*/

            XElement rootElement = null;

            if (InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT)
            {
                rootElement = new XElement(ns + "Invoice",
                                new XAttribute(XNamespace.Xmlns + "cac", ns_cac),
                                new XAttribute(XNamespace.Xmlns + "cbc", ns_cbc),
                                new XAttribute(XNamespace.Xmlns + "ccts", ns_ccts),
                                new XAttribute(XNamespace.Xmlns + "ext", ns_ext),
                                new XAttribute(XNamespace.Xmlns + "qdt", ns_qdt),
                                new XAttribute(XNamespace.Xmlns + "udt", ns_udt),
                                new XAttribute(XNamespace.Xmlns + "xsd", ns_xsd),
                                new XAttribute(XNamespace.Xmlns + "xsi", ns_xsi));
            }
            else
            {
                ns = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";

                rootElement = new XElement(ns + "CreditNote",
                                new XAttribute(ns_xsi + "schemaLocation", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 UBL-CreditNote-2.0.xsd"),
                                new XAttribute(XNamespace.Xmlns + "cac", ns_cac),
                                new XAttribute(XNamespace.Xmlns + "cbc", ns_cbc),
                                new XAttribute(XNamespace.Xmlns + "ext", ns_ext),
                                new XAttribute(XNamespace.Xmlns + "xsi", ns_xsi));
            }

            //Must exist
            rootElement.Add(new XElement(ns_cbc + "UBLVersionID", UBLVersion));
            rootElement.Add(new XElement(ns_cbc + "CustomizationID", CustomizationID));
            rootElement.Add(new XElement(ns_cbc + "ProfileID", ProfileID));
            rootElement.Add(new XElement(ns_cbc + "ID", ID));
            rootElement.Add(new XElement(ns_cbc + "IssueDate", IssueDate.ToShortDateString()));

            if(InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT)
                rootElement.Add(new XElement(ns_cbc + "InvoiceTypeCode", InvoiceTypeCode));

            rootElement.Add(new XElement(ns_cbc + "Note", Note));
            rootElement.Add(new XElement(ns_cbc + "DocumentCurrencyCode", InvoiceCurrencyCode));

            //Add nodes
            //invoicePeriod.AddNode(ref rootElement);

            if (InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT)
            {
                orderReference.AddNode(ref rootElement);
                //contractDocumentReference.AddNode(ref rootElement);
            }
            else
            {
                billingReference.AddNode(ref rootElement);
            }

            if (InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_CREDIT)
                billingReference.AddNode(ref rootElement);

            foreach (var adr in DocumentReferences)
            {
                adr.AddNode(ref rootElement);
            }

            accountingSupplierParty.AddNode(ref rootElement, true);
            accountingCustomerParty.AddNode(ref rootElement, false);
            payeeParty.AddNode(ref rootElement);

            if (InvoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT)
            {
                delivery.AddNode(ref rootElement);

                foreach (var paymentMeans in PaymentMeansList)
                {
                    paymentMeans.AddNode(ref rootElement);
                }

                XElement payementTermsNode = new XElement(ns_cac + "PaymentTerms");
                payementTermsNode.Add(new XElement(ns_cbc + "Note", PaymentTermsNote));
                rootElement.Add(payementTermsNode);
            }

            foreach (var allowanceCharce in AllowanceChargesList)
            {
                allowanceCharce.AddNode(ref rootElement);
            }

            taxTotal.AddNode(ref rootElement);
            legalMonetaryTotal.AddNode(ref rootElement);

            //NOTE: At least one line must exist or the document will not be validated against the schema
            foreach (var invoiceLine in invoiceLines)
            {
                invoiceLine.AddNode(ref rootElement, InvoiceTypeCode);
            }

            document.Add(rootElement);
            xdoc = document;
        }

        #region Future implementation, maybe

        public EHFInvoiceItem(XDocument invoice)
        {
            ParseEHFInvoice(invoice);
        }

        private void ParseEHFInvoice(XDocument invoice)
        {

        }

        public void ToSoeInvoice(ref Invoice invoice)
        {

        }

        public bool Import()
        {
            return false;
        }

        #endregion

        public String GetTaxCategoryIdentifier(decimal taxRate, bool isVatFree)
        {
            if (isVatFree)
            {
                return "E";
            }
            else
            {
                if (taxRate == 25)
                {
                    return "S";
                }
                else if (taxRate < 25 && taxRate > 8)
                {
                    return "H";
                }
                else if (taxRate < 0)
                {
                    return "AA";
                }
                else
                {
                    return "Z";
                }
            }
        }
    }

    public class InvoicePeriod : EHFInvoiceBase
    {
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement invoicePeriodNode = new XElement(ns_cac + "InvoicePeriod");

            //Must exist                    
            invoicePeriodNode.Add(new XElement(ns_cbc + "StartDate", StartDate));
            invoicePeriodNode.Add(new XElement(ns_cbc + "EndDate", StartDate));

            root.Add(invoicePeriodNode);
        }
    }

    public class OrderReference : EHFInvoiceBase
    {
        public String ID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement orderReferenceNode = new XElement(ns_cac + "OrderReference");

            //Must exist                    
            orderReferenceNode.Add(new XElement(ns_cbc + "ID", ID));

            root.Add(orderReferenceNode);
        }
    }

    public class BillingReference : EHFInvoiceBase
    {
        public String InvoiceDocumentReferenceID { get; set; }
        public String CreditNoteDocumentReferenceID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement billingReferenceNode = new XElement(ns_cac + "BillingReference");

            if(!String.IsNullOrEmpty(InvoiceDocumentReferenceID))
            {
                XElement invoiceReferenceNode = new XElement(ns_cac + "InvoiceDocumentReference");
                invoiceReferenceNode.Add(new XElement(ns_cbc + "ID", InvoiceDocumentReferenceID));
                billingReferenceNode.Add(invoiceReferenceNode);
            }

            if (!String.IsNullOrEmpty(CreditNoteDocumentReferenceID))
            {
                XElement creditReferenceNode = new XElement(ns_cac + "CreditNoteDocumentReference");
                creditReferenceNode.Add(new XElement(ns_cbc + "ID", CreditNoteDocumentReferenceID));
                billingReferenceNode.Add(creditReferenceNode);
            }

            root.Add(billingReferenceNode);
        }
    }

    public class ContractDocumentReference : EHFInvoiceBase
    {
        public String ID { get; set; }
        public String DocumentType { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement contractReferenceNode = new XElement(ns_cac + "ContractReference");

            //Must exist                    
            contractReferenceNode.Add(new XElement(ns_cbc + "ID", ID));

            contractReferenceNode.Add(new XElement(ns_cbc + "DocumentType", DocumentType));

            root.Add(contractReferenceNode);
        }
    }

    public class AdditionalDocumentReference : EHFInvoiceBase
    {
        public bool IsExternalReference { get; set;}
        public String ID { get; set; }
        public String DocumentType { get; set; }
        public String Data{ get; set; }
        public String MimeType { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement additionalDocumentReferenceNode = new XElement(ns_cac + "AdditionalDocumentReference");

            //Must exist                    
            additionalDocumentReferenceNode.Add(new XElement(ns_cbc + "ID", ID));
            additionalDocumentReferenceNode.Add(new XElement(ns_cbc + "DocumentType", DocumentType));

            XElement attachmentNode = new XElement(ns_cac + "Attachment");

            if (IsExternalReference)
            {
                XElement externalReferenceNode = new XElement(ns_cac + "ExternalReference");

                externalReferenceNode.Add(new XElement(ns_cbc + "URI", Data));

                attachmentNode.Add(externalReferenceNode);
            }
            else
            {
                attachmentNode.Add(new XElement(ns_cbc + "EmbeddedDocumentBinaryObject", Data, new XAttribute("mimeCode", MimeType)));
            }

            additionalDocumentReferenceNode.Add(attachmentNode);
            root.Add(additionalDocumentReferenceNode);
        }
    }

    public class PostalAddress : EHFInvoiceBase
    {
        public String PostalAddressID { get; set; }
        public String PostBox { get; set; }
        public String StreetName { get; set; }
        public String AdditionalStreetName { get; set; }
        public String BuildingNumber { get; set; }
        public String Department { get; set; }
        public String CityName { get; set; }
        public String PostalZone { get; set; }
        public String CountrySubentityCode { get; set; }
        public String CountryIdentificationCode { get; set; }

        public void AddNode(ref XElement root, bool usePartial)
        {
            XElement postalAddressNode = usePartial ? new XElement(ns_cac + "Address") : new XElement(ns_cac + "PostalAddress");

            if (!String.IsNullOrEmpty(PostalAddressID))
            {
                if(usePartial)
                    root.Add(new XElement(ns_cbc + "ID", PostalAddressID));
                else
                    postalAddressNode.Add(new XElement(ns_cbc + "ID", PostalAddressID));
            }

            if (!String.IsNullOrEmpty(PostBox) && !usePartial)
                postalAddressNode.Add(new XElement(ns_cbc + "PostBox", PostBox));

            if (!String.IsNullOrEmpty(StreetName))
                postalAddressNode.Add(new XElement(ns_cbc + "StreetName", StreetName));

            if (!String.IsNullOrEmpty(AdditionalStreetName))
                postalAddressNode.Add(new XElement(ns_cbc + "AdditionalStreetName", AdditionalStreetName));

            if (!String.IsNullOrEmpty(BuildingNumber))
                postalAddressNode.Add(new XElement(ns_cbc + "BuildingNumber", BuildingNumber));

            if (!String.IsNullOrEmpty(Department) && !usePartial)
                postalAddressNode.Add(new XElement(ns_cbc + "Department", Department));

            if (!String.IsNullOrEmpty(CityName))
                postalAddressNode.Add(new XElement(ns_cbc + "CityName", CityName));

            if (!String.IsNullOrEmpty(PostalZone))
                postalAddressNode.Add(new XElement(ns_cbc + "PostalZone", PostalZone));

            if (!String.IsNullOrEmpty(CountrySubentityCode))
                postalAddressNode.Add(new XElement(ns_cbc + "CountrySubentityCode", CountrySubentityCode));

            if (!String.IsNullOrEmpty(CountryIdentificationCode))
            {
                XElement countryNode = new XElement(ns_cac + "Country");
                countryNode.Add(new XElement(ns_cbc + "IdentificationCode", CountryIdentificationCode, new XAttribute("listID", "ISO3166-1"), new XAttribute("listAgencyID", "6")));
                postalAddressNode.Add(countryNode);
            }
            
            root.Add(postalAddressNode);
        }
    }

    public class PartyTaxScheme : EHFInvoiceBase
    {
        public String CompanyID { get; set; }
        public String TaxSchemeID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement partyTaxSchemeNode = new XElement(ns_cac + "PartyTaxScheme");

            if (!String.IsNullOrEmpty(CompanyID))
                partyTaxSchemeNode.Add(new XElement(ns_cbc + "CompanyID", CompanyID));

            if (!String.IsNullOrEmpty(TaxSchemeID))
            {
                XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");
                taxSchemeNode.Add(new XElement(ns_cbc + "ID", TaxSchemeID, new XAttribute("schemeID", "UN/ECE 5153"), new XAttribute("schemeAgencyID", "6")));  
                partyTaxSchemeNode.Add(taxSchemeNode);
            }

            root.Add(partyTaxSchemeNode);
        }
    }

    public class PartyLegalEntity : EHFInvoiceBase
    {
        public String RegistrationName { get; set; }
        public String CompanyID { get; set; }
        public String CityName { get; set; }
        public String CountrySubentity { get; set; }
        public String CountryIdentificationCode { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement partyLegalEntityNode = new XElement(ns_cac + "PartyLegalEntity");

            if (!String.IsNullOrEmpty(RegistrationName))
                partyLegalEntityNode.Add(new XElement(ns_cbc + "RegistrationName", RegistrationName));

            if (!String.IsNullOrEmpty(CompanyID))
                partyLegalEntityNode.Add(new XElement(ns_cbc + "CompanyID", CompanyID));

            if (!String.IsNullOrEmpty(CityName))
            {
                XElement addressNode = new XElement(ns_cac + "RegistrationAddress");
                addressNode.Add(new XElement(ns_cbc + "CityName", CityName));

                if (!String.IsNullOrEmpty(CountrySubentity))
                    addressNode.Add(new XElement(ns_cbc + "CountrySubentity", CountrySubentity));

                if (!String.IsNullOrEmpty(CountryIdentificationCode))
                {
                    XElement countryNode = new XElement(ns_cac + "Country");
                    countryNode.Add(new XElement(ns_cbc + "IdentificationCode", CountryIdentificationCode, new XAttribute("listID", "ISO3166-1"), new XAttribute("listAgencyID", "6")));
                    addressNode.Add(countryNode);
                }

                partyLegalEntityNode.Add(addressNode);
            }

            root.Add(partyLegalEntityNode);
        }
    }

    public class PartyContact : EHFInvoiceBase
    {
        public String ID { get; set; }
        public String Telephone { get; set; }
        public String Telefax { get; set; }
        public String Email { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement contactNode = new XElement(ns_cac + "PostalAddress");

            if (!String.IsNullOrEmpty(ID))
                contactNode.Add(new XElement(ns_cbc + "ID", ID));

            if (!String.IsNullOrEmpty(Telephone))
                contactNode.Add(new XElement(ns_cbc + "Telephone", Telephone));

            if (!String.IsNullOrEmpty(Telefax))
                contactNode.Add(new XElement(ns_cbc + "Telefax", Telefax));

            if (!String.IsNullOrEmpty(Email))
                contactNode.Add(new XElement(ns_cbc + "ElectronicMail", Email));

            root.Add(contactNode);
        }
    }

    public class PartyPerson : EHFInvoiceBase
    {
        public String FirstName { get; set; }
        public String FamilyName { get; set; }
        public String MiddleName { get; set; }
        public String JobTitle { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement personNode = new XElement(ns_cac + "PostalAddress");

            if (!String.IsNullOrEmpty(FirstName))
                personNode.Add(new XElement(ns_cbc + "FirstName", FirstName));

            if (!String.IsNullOrEmpty(FamilyName))
                personNode.Add(new XElement(ns_cbc + "FamilyName", FamilyName));

            if (!String.IsNullOrEmpty(MiddleName))
                personNode.Add(new XElement(ns_cbc + "MiddleName", MiddleName));

            if (!String.IsNullOrEmpty(JobTitle))
                personNode.Add(new XElement(ns_cbc + "JobTitle", JobTitle));

            root.Add(personNode);
        }
    }

    public class AccountingParty : EHFInvoiceBase
    {
        public String EndPointID { get; set; }
        public String PartyID { get; set; } //Mandatory
        public String PartyName { get; set; } //Mandatory
        public PostalAddress PartyPostalAddress { get; set; } //Mandatory
        public PartyTaxScheme TaxScheme { get; set; }
        public PartyLegalEntity LegalEntity { get; set; }
        public PartyContact PartyContact { get; set; }
        public PartyPerson PartyPerson { get; set; }

        public void AddNode(ref XElement root, bool isSupplierParty)
        {
            XElement partyTypeNode = isSupplierParty ? new XElement(ns_cac + "AccountingSupplierParty") : new XElement(ns_cac + "AccountingCustomerParty");
            XElement partyNode = new XElement(ns_cac + "Party");

            if (!String.IsNullOrEmpty(EndPointID))
                partyNode.Add(new XElement(ns_cbc + "EndPointID", EndPointID));

            XElement partyIdentificationNode = new XElement(ns_cac + "PartyIdentification");
            partyIdentificationNode.Add(new XElement(ns_cbc + "ID", PartyID));
            partyNode.Add(partyIdentificationNode);

            XElement partyNameNode = new XElement(ns_cac + "PartyName");
            partyNameNode.Add(new XElement(ns_cbc + "Name", PartyName));
            partyNode.Add(partyNameNode);

            PartyPostalAddress.AddNode(ref partyNode, false);

            if (TaxScheme != null)
                TaxScheme.AddNode(ref partyNode);

            if (PartyContact != null)
                PartyContact.AddNode(ref partyNode);

            if (PartyPerson != null)
                PartyPerson.AddNode(ref partyNode);

            partyTypeNode.Add(partyNode);
            root.Add(partyTypeNode);
        }
    }

    public class PayeeParty : EHFInvoiceBase
    {
        public String PartyID { get; set; } //Mandatory
        public String PartyName { get; set; } 
        public PartyLegalEntity LegalEntity { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement payeePartyNode = new XElement(ns_cac + "PayeeParty");

            XElement partyIdentificationNode = new XElement(ns_cac + "PartyIdentification");
            partyIdentificationNode.Add(new XElement(ns_cbc + "ID", PartyID));
            payeePartyNode.Add(partyIdentificationNode);

            XElement partyNameNode = new XElement(ns_cac + "PartyName");
            partyNameNode.Add(new XElement(ns_cbc + "Name", PartyName));
            payeePartyNode.Add(partyNameNode);

            if (LegalEntity != null)
                LegalEntity.AddNode(ref payeePartyNode);

            root.Add(payeePartyNode);
        }
    }

    public class Delivery : EHFInvoiceBase
    {
        public DateTime? ActualDeliveryDateTime { get; set; }
        public PostalAddress DeliveryAddress { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement deliveryNode = new XElement(ns_cac + "Delivery");

            #region Delivery

            if (ActualDeliveryDateTime.HasValue)
                deliveryNode.Add(new XElement(ns_cbc + "ActualDeliveryDate", ActualDeliveryDateTime.Value.ToShortDateString()));


            XElement deliveryAddressNode = new XElement(ns_cac + "DeliveryAddress");

            if (DeliveryAddress != null)
                DeliveryAddress.AddNode(ref deliveryAddressNode, true);

            deliveryNode.Add(deliveryAddressNode);

            #endregion

            root.Add(deliveryNode);
        }
    }

    public class PaymentMeans : EHFInvoiceBase
    {
        public String PaymentMeansCode { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public String PaymentChannelCode { get; set; }
        public String PaymentID { get; set; }
        public String PayeeFinancialAccountID { get; set; }
        public String FinancialInstitutionBranchID { get; set; }
        public String FinancialInstitutionID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement paymentMeansNode = new XElement(ns_cac + "PaymentMeans");

            #region PaymentMeans

            if (!String.IsNullOrEmpty(PaymentMeansCode))
                paymentMeansNode.Add(new XElement(ns_cbc + "PaymentMeansCode", PaymentMeansCode));

            if (PaymentDueDate.HasValue)
                paymentMeansNode.Add(new XElement(ns_cbc + "PaymentDueDate", PaymentDueDate.Value.ToShortDateString()));

            if (!String.IsNullOrEmpty(PaymentChannelCode))
                paymentMeansNode.Add(new XElement(ns_cbc + "PaymentChannelCode", PaymentChannelCode));

            if (!String.IsNullOrEmpty(PaymentID))
                paymentMeansNode.Add(new XElement(ns_cbc + "PaymentID", PaymentID));

            #region PayeeFinancialAccount

            if (!String.IsNullOrEmpty(PayeeFinancialAccountID))
            {
                XElement payeeFinancialAccountNode = new XElement(ns_cac + "PayeeFinancialAccount");

                if (!String.IsNullOrEmpty(PayeeFinancialAccountID) && PayeeFinancialAccountID.Length > 0)
                    payeeFinancialAccountNode.Add(new XElement(ns_cbc + "ID", PayeeFinancialAccountID));

                XElement financialInstitutionBranchNode = new XElement(ns_cac + "FinancialInstitutionBranch");

                if (!String.IsNullOrEmpty(FinancialInstitutionBranchID) && FinancialInstitutionBranchID.Length > 0)
                    financialInstitutionBranchNode.Add(new XElement(ns_cbc + "ID", FinancialInstitutionBranchID));

                XElement financialInstitutionNode = new XElement(ns_cac + "FinancialInstitution");

                if (!String.IsNullOrEmpty(FinancialInstitutionID) && FinancialInstitutionID.Length > 0)
                    financialInstitutionNode.Add(new XElement(ns_cbc + "ID", FinancialInstitutionID));

                financialInstitutionBranchNode.Add(financialInstitutionNode);
                payeeFinancialAccountNode.Add(financialInstitutionBranchNode);
                paymentMeansNode.Add(payeeFinancialAccountNode);
            }

            #endregion

            #endregion

            root.Add(paymentMeansNode);
        }
    }

    public class PaymentTerms : EHFInvoiceBase
    {
        public String Note { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement paymentTermsNode = new XElement(ns_cac + "PaymentTerms");

            if (!String.IsNullOrEmpty(Note) && Note.Length > 0)
                paymentTermsNode.Add(new XElement(ns_cbc + "Note", Note));

            root.Add(paymentTermsNode);
        }
    }

    public class AllowanceCharge : EHFInvoiceBase
    {
        public String ChargeIndicator { get; set; } //Mandatory
        public String AllowanceChargeReason { get; set; }
        public decimal Amount { get; set; } //Mandatory
        public String AmountCurrencyID { get; set; }
        public String CategoryTaxID { get; set; } //Mandatory
        public decimal CategoryTaxPercent { get; set; }
        public String CategoryTaxSchemeID { get; set; } //Mandatory

        public void AddNode(ref XElement root)
        {
            XElement allowanceNode = new XElement(ns_cac + "AllowanceCharge");

            allowanceNode.Add(new XElement(ns_cbc + "ChargeIndicator", ChargeIndicator));

            if (!String.IsNullOrEmpty(AllowanceChargeReason) && AllowanceChargeReason.Length > 0)
                allowanceNode.Add(new XElement(ns_cbc + "AllowanceChargeReason", AllowanceChargeReason));

            allowanceNode.Add(new XElement(ns_cbc + "Amount", Amount, new XAttribute("currencyID", AmountCurrencyID)));

            XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");

            taxCategoryNode.Add(new XElement(ns_cbc + "ID", CategoryTaxID));

            if (CategoryTaxPercent > 0)
                taxCategoryNode.Add(new XElement(ns_cbc + "Percent", CategoryTaxPercent));

            XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");

            taxSchemeNode.Add(new XElement(ns_cbc + "ID", CategoryTaxSchemeID, new XAttribute("schemeID", "UN/ECE 5153"), new XAttribute("schemeAgencyID", "6")));

            taxCategoryNode.Add(taxSchemeNode);
            allowanceNode.Add(taxCategoryNode);
            root.Add(allowanceNode);
        }
    }

    public class TaxSubTotal : EHFInvoiceBase
    {
        public decimal TaxableAmount { get; set; }
        public String TaxableAmountCurrencyID { get; set; }
        public decimal TaxAmount { get; set; }
        public String TaxAmountCurrencyID { get; set; }
        public String TaxCategoryID { get; set; }
        public decimal TaxCategoryPercent { get; set; }
        public String TaxSchemeID { get; set; }
        public String TaxExcemptionReasonCode { get; set; }
        public String TaxExcemptionReason { get; set; }

        public void AddNode(ref XElement parent)
        {

            XElement taxSubTotalNode = new XElement(ns_cac + "TaxSubtotal");

            #region TaxSubTotal

            //Must exist
            taxSubTotalNode.Add(new XElement(ns_cbc + "TaxableAmount", TaxableAmount, new XAttribute("currencyID", TaxableAmountCurrencyID)));
            taxSubTotalNode.Add(new XElement(ns_cbc + "TaxAmount", TaxAmount, new XAttribute("currencyID", TaxAmountCurrencyID)));

            #region TaxCategory

            XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");
            //Must exist
            taxCategoryNode.Add(new XElement(ns_cbc + "ID", TaxCategoryID));
            taxCategoryNode.Add(new XElement(ns_cbc + "Percent", TaxCategoryPercent));

            if (TaxCategoryPercent == 0)
            {
                taxCategoryNode.Add(new XElement(ns_cbc + "TaxExcemptionReasonCode", TaxExcemptionReasonCode));
                taxCategoryNode.Add(new XElement(ns_cbc + "TaxExcemptionReason", TaxExcemptionReason));
            }

            #region TaxScheme

            XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");
            //Must exist
            taxSchemeNode.Add(new XElement(ns_cbc + "ID", TaxSchemeID, new XAttribute("schemeID", "UN/ECE 5153"), new XAttribute("schemeAgencyID", "6")));

            taxCategoryNode.Add(taxSchemeNode);
            #endregion

            taxSubTotalNode.Add(taxCategoryNode);
            #endregion

            #endregion

            parent.Add(taxSubTotalNode);

        }

    }

    public class TaxTotal : EHFInvoiceBase
    {
        public decimal TotalTaxAmount { get; set; }
        public String TotalTaxAmountCurrencyID { get; set; }
        public List<TaxSubTotal> taxSubtotals = new List<TaxSubTotal>();

        public void AddNode(ref XElement root)
        {
            XElement taxTotalNode = new XElement(ns_cac + "TaxTotal");

            #region TaxTotal
            //Must exist            
            taxTotalNode.Add(new XElement(ns_cbc + "TaxAmount", TotalTaxAmount, new XAttribute("currencyID", TotalTaxAmountCurrencyID)));

            #region TaxSubTotal

            foreach (var taxSubtotal in taxSubtotals)
            {
                taxSubtotal.AddNode(ref taxTotalNode);
            }

            #endregion

            #endregion

            root.Add(taxTotalNode);
        }
    }

    public class LegalMonetaryTotal : EHFInvoiceBase
    {
        public decimal LineExtensionAmount { get; set; }
        public decimal TaxExclusiveAmount { get; set; }
        public decimal TaxInclusiveAmount { get; set; }
        public decimal AllowanceTotalAmount { get; set; }
        public decimal ChargeTotalAmount { get; set; }
        public decimal PrepaidAmount { get; set; }
        public decimal PayableRoundingAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public String LineExtensionAmountCurrencyID { get; set; }
        public String TaxExclusiveAmountCurrencyID { get; set; }
        public String TaxInclusiveAmountCurrencyID { get; set; }
        public String AllowanceTotalAmountCurrencyID { get; set; }
        public String ChargeTotalAmountCurrencyID { get; set; }
        public String PrepaidAmountCurrencyID { get; set; }
        public String PayableRoundingAmountCurrencyID { get; set; }
        public String PayableAmountCurrencyID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement legalMonetaryTotalNode = new XElement(ns_cac + "LegalMonetaryTotal");

            #region LegalTotal

            legalMonetaryTotalNode.Add(new XElement(ns_cbc + "LineExtensionAmount", LineExtensionAmount, new XAttribute("currencyID", LineExtensionAmountCurrencyID)));
            legalMonetaryTotalNode.Add(new XElement(ns_cbc + "TaxExclusiveAmount", TaxExclusiveAmount, new XAttribute("currencyID", TaxExclusiveAmountCurrencyID)));
            legalMonetaryTotalNode.Add(new XElement(ns_cbc + "TaxInclusiveAmount", TaxInclusiveAmount, new XAttribute("currencyID", TaxInclusiveAmountCurrencyID)));

            if (AllowanceTotalAmount > 0)
                legalMonetaryTotalNode.Add(new XElement(ns_cbc + "AllowanceTotalAmount", AllowanceTotalAmount, new XAttribute("currencyID", AllowanceTotalAmountCurrencyID)));

            if (ChargeTotalAmount > 0)
                legalMonetaryTotalNode.Add(new XElement(ns_cbc + "ChargeTotalAmount", ChargeTotalAmount, new XAttribute("currencyID", ChargeTotalAmountCurrencyID)));

            if (PrepaidAmount > 0)
                legalMonetaryTotalNode.Add(new XElement(ns_cbc + "PrepaidAmount", PrepaidAmount, new XAttribute("currencyID", PrepaidAmountCurrencyID)));

            if (PayableRoundingAmount != 0)
                legalMonetaryTotalNode.Add(new XElement(ns_cbc + "PayableRoundingAmount", PayableRoundingAmount, new XAttribute("currencyID", PayableRoundingAmountCurrencyID)));

            legalMonetaryTotalNode.Add(new XElement(ns_cbc + "PayableAmount", PayableAmount, new XAttribute("currencyID", PayableAmountCurrencyID)));

            #endregion

            root.Add(legalMonetaryTotalNode);
        }
    }

    public class InvoiceItemCommodityClassification
    {
        public int ListAgencyID { get; set; }
        public int ListID { get; set; }
        public String ItemClassificationCode { get; set; }
    }

    public class InvoiceLine : EHFInvoiceBase
    {
        #region Basics

        public int ID { get; set; }
        public String Note { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public String InvoicedQuantityUnitCode { get; set; }
        public decimal LineExtensionAmount { get; set; }
        public String LineExtensionAmountCurrencyID { get; set; }
        public String AccountingCost { get; set; }
        public String OrderLineID { get; set; }
        public List<AllowanceCharge> AllowanceCharges { get; set; }
        public decimal TaxAmount { get; set; }
        public String TaxAmountCurrencyID { get; set; }

        #endregion

        #region Item

        public String ItemDescription { get; set; }
        public String ItemName { get; set; }
        public String SellersItemIdentificationID { get; set; }
        public String StandardItemIdentificationID { get; set; }
        public List<InvoiceItemCommodityClassification> Classifications { get; set; }
        public String ClassifiedTaxCategoryID { get; set; }
        public decimal ClassifiedTaxCategoryPercent { get; set; }
        public String ClassifiedTaxCategoryTaxSchemeID { get; set; }
        public Dictionary<String, String> AdditionalItemProperties { get; set; }


        #endregion

        #region Price

        public decimal PriceAmount { get; set; }
        public String PriceAmountCurrencyID { get; set; }
        public decimal BaseQuantity { get; set; }
        public String BaseQuantityUnitCode { get; set; }
        public AllowanceCharge AllowanceCharge { get; set; }

        #endregion

        public void AddNode(ref XElement root, int invoiceTypeCode)
        {
            XElement invoiceLineNode = new XElement(ns_cac + (invoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? "InvoiceLine" : "CreditNoteLine"));

            #region InvoiceLine

            #region Basics

            //Must exist
            invoiceLineNode.Add(new XElement(ns_cbc + "ID", ID));

            if(!String.IsNullOrEmpty(Note))
                invoiceLineNode.Add(new XElement(ns_cbc + "Note", Note));

            invoiceLineNode.Add(new XElement(ns_cbc + (invoiceTypeCode == EHF_INVOICE_TYPE_CODE_DEBIT ? "InvoicedQuantity" : "CreditedQuantity"), InvoicedQuantity, new XAttribute("unitCode", InvoicedQuantityUnitCode)));
            invoiceLineNode.Add(new XElement(ns_cbc + "LineExtensionAmount", LineExtensionAmount, new XAttribute("currencyID", LineExtensionAmountCurrencyID)));

            if (!String.IsNullOrEmpty(AccountingCost))
                invoiceLineNode.Add(new XElement(ns_cbc + "AccountingCost", AccountingCost));

            if (!String.IsNullOrEmpty(OrderLineID))
            {
                XElement orderLineReferenceNode = new XElement(ns_cac + "OrderLineReference");
                orderLineReferenceNode.Add(new XElement(ns_cbc + "LineID", OrderLineID));
                invoiceLineNode.Add(orderLineReferenceNode);
            }

            if (AllowanceCharges != null && AllowanceCharges.Count > 0)
            {
                foreach (AllowanceCharge charge in AllowanceCharges)
                {
                    charge.AddNode(ref invoiceLineNode);
                }
            }

            XElement taxTotalNode = new XElement(ns_cac + "TaxTotal");
            taxTotalNode.Add(new XElement(ns_cbc + "TaxAmount", TaxAmount, new XAttribute("currencyID", TaxAmountCurrencyID)));
            invoiceLineNode.Add(taxTotalNode);

            #endregion

            #region Item

            XElement itemNode = new XElement(ns_cac + "Item");

            if (!String.IsNullOrEmpty(ItemDescription))
                itemNode.Add(new XElement(ns_cbc + "Description", ItemDescription));

            if (!String.IsNullOrEmpty(ItemName))
                itemNode.Add(new XElement(ns_cbc + "Name", ItemName));


            if (!String.IsNullOrEmpty(SellersItemIdentificationID))
            {
                XElement sellersItemIdentificationNode = new XElement(ns_cac + "SellersItemIdentification");
                sellersItemIdentificationNode.Add(new XElement(ns_cbc + "ID", SellersItemIdentificationID));

                itemNode.Add(sellersItemIdentificationNode);
            }

            if (!String.IsNullOrEmpty(StandardItemIdentificationID))
            {
                XElement standardItemIdentificationNode = new XElement(ns_cac + "StandardItemIdentification");
                standardItemIdentificationNode.Add(new XElement(ns_cbc + "ID", StandardItemIdentificationID));

                itemNode.Add(standardItemIdentificationNode);
            }

            if (Classifications != null && Classifications.Count > 0)
            {
                foreach (InvoiceItemCommodityClassification classification in Classifications)
                {
                    XElement commodityClassificationNode = new XElement(ns_cac + "CommodityClassification");
                    invoiceLineNode.Add(new XElement(ns_cbc + "ItemClassificationCode", classification.ItemClassificationCode, new XAttribute("listAgencyID", classification.ListAgencyID), new XAttribute("listID", classification.ListID)));
                    itemNode.Add(commodityClassificationNode);
                }
            }

            XElement classifiedTaxCategoryNode = new XElement(ns_cac + "ClassifiedTaxCategory", new XAttribute("xmlsn", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"));
            classifiedTaxCategoryNode.Add(new XElement(ns_cbc + "ID", ClassifiedTaxCategoryID));
            classifiedTaxCategoryNode.Add(new XElement(ns_cbc + "Percent", ClassifiedTaxCategoryPercent));
            XElement classifiedTaxCategoryTaxSchemeNode = new XElement(ns_cac + "TaxScheme");
            classifiedTaxCategoryTaxSchemeNode.Add(new XElement(ns_cbc + "ID", ClassifiedTaxCategoryTaxSchemeID, new XAttribute("schemeID", "UN/ECE 5153"), new XAttribute("schemeAgencyID", "6")));
            classifiedTaxCategoryNode.Add(classifiedTaxCategoryTaxSchemeNode);
            itemNode.Add(classifiedTaxCategoryNode);

            if (AdditionalItemProperties != null && AdditionalItemProperties.Count > 0)
            {
                for (int i = 0; i < AdditionalItemProperties.Count; i++)
                {
                    XElement additionalItemPropertyNode = new XElement(ns_cac + "AdditionalItemProperty");
                    additionalItemPropertyNode.Add(new XElement(ns_cbc + "Name", AdditionalItemProperties.ElementAt(i).Key));
                    additionalItemPropertyNode.Add(new XElement(ns_cbc + "Value", AdditionalItemProperties.ElementAt(i).Value));
                    itemNode.Add(additionalItemPropertyNode);
                }
            }

            invoiceLineNode.Add(itemNode);

            #endregion

            #region BasePrice

            XElement basePriceNode = new XElement(ns_cac + "Price");
            //Must exist
            basePriceNode.Add(new XElement(ns_cbc + "PriceAmount", PriceAmount, new XAttribute("currencyID", PriceAmountCurrencyID)));

            if (BaseQuantity > 0)
            {
                basePriceNode.Add(new XElement(ns_cbc + "BaseQuantity", BaseQuantity, new XAttribute("unitCode", BaseQuantityUnitCode)));
            }

            if (AllowanceCharge != null)
            {
                AllowanceCharge.AddNode(ref basePriceNode);
            }

            invoiceLineNode.Add(basePriceNode);

            #endregion

            #endregion

            root.Add(invoiceLineNode);
        }
    }

    public class EHFInvoiceBase
    {
        #region Namespaces

        public XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        public XNamespace ns_cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        public XNamespace ns_cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        public XNamespace ns_ccts = "urn:oasis:names:tc:ubl:CoreComponentParameters:1:0";
        public XNamespace ns_ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        public XNamespace ns_qdt = "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2";
        public XNamespace ns_udt = "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2";
        public XNamespace ns_xsd = "http://www.w3.org/2001/XMLSchema";
        public XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public const int EHF_INVOICE_TYPE_CODE_DEBIT = 380;
        public const int EHF_INVOICE_TYPE_CODE_CREDIT = 381;
        public const String EHF_INVOICE_UBL_VERSION_ID = "2.0";
        public const String EHF_INVOICE_CUSTOMIZATION_ID = "urn:www.cenbii.eu:transaction:biicoretrdm010:ver1.0:#urn:www.peppol.eu:bis:peppol4a:ver1.0#urn:www.difi.no:ehf:faktura:ver1";
        public const String EHF_INVOICE_PROFILE_ID = "urn:www.cenbii.eu:profile:bii05:ver1.0";
        public const String EHF_INVOICE_PAYMENTCHANNELCODE = "SW";
        /*public const String SVEFAK_FINANICIAL_INSTITUTION_ID_BG = "BGABSESS";
        public const String SVEFAK_FINANICIAL_INSTITUTION_ID_PG = "PGSISESS";*/
        public const String EHF_TAX_SCHEME_ID_VAT = "VAT";
        public const String EH_TAX_SCHEME_ID_SWT = "SWT";

        #endregion
    }
}
