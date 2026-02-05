using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.Svefaktura
{
    public class SvefakturaItem : SvefakturaBase
    {
        /**
         * For dokumentation see : http://www.svefaktura.se/SFTI_Basic_Invoice20081227/SFTI%20Basic%20Invoice_1.0/index.html
         * 
         * */

        private readonly ReportGenManager rgm = new ReportGenManager(null);
        private XDocument xdoc;

        #region Root elements
        public string ID { get; set; }
        public DateTime IssueDate { get; set; }
        public int InvoiceTypeCode { get; set; }
        public string Note { get; set; }
        public string InvoiceCurrencyCode { get; set; }
        public int LineItemCountNumeric { get; set; }
        #endregion

        #region Nodes
        private readonly List<AdditionalDocumentReference> additionalDocumentReferences = new List<AdditionalDocumentReference>();
        private readonly BuyerParty buyer = new BuyerParty();
        private readonly SellerParty seller = new SellerParty();
        private readonly Delivery delivery = new Delivery();
        private readonly List<PaymentMeans> paymentMeansList = new List<PaymentMeans>();
        private readonly PaymentTerms paymentTerms = new PaymentTerms();
        private readonly List<AllowanceChargeHead> allowanceChargeHeads = new List<AllowanceChargeHead>();
        private readonly TaxTotal taxTotal = new TaxTotal();
        private readonly LegalTotal legalTotal = new LegalTotal();
        private readonly List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
        private readonly List<RequisitionistDocumentReference> requisitionistDocumentReferences = new List<RequisitionistDocumentReference>();
        private InitialInvoiceDocumentReference initialInvoiceDocumentReference = null;
        #endregion

        public SvefakturaItem(CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> invoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, string exemptionReason, CustomerInvoice creditedInvoice, int actorCompanyId, int userId, string timeReportFilename, List<string> attachementNames = null, string defaultInvoiceText = null)
        {
            Populate(invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddress, customerDeliveryAddress, customerContactEcoms, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason, creditedInvoice, actorCompanyId, userId, timeReportFilename, attachementNames, defaultInvoiceText);
        }

        private void Populate(CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> invoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, string exemptionReason, CustomerInvoice creditedInvoice, int actorCompanyId, int userId, string timeReportFilename, List<string> attachementNames, string defaultInvoiceText)
        {
            #region Prereq

            if (invoice == null || company == null || customer == null)
                return;

            SettingManager sm = new SettingManager(null);
            bool hideArticleNr = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingHideArticleNrOnSvefaktura, userId, actorCompanyId, 0);

            #endregion

            CustomerInvoiceRow invoiceFeeRow = invoiceRows.FirstOrDefault(x => x.IsInvoiceFeeRow);
            CustomerInvoiceRow invoiceFreightRow = invoiceRows.FirstOrDefault(x => x.IsFreightAmountRow);
            List<CustomerInvoiceRow> houseHoldRows = invoiceRows.Where(x => x.HouseholdTaxDeductionRow != null).ToList();
            foreach (CustomerInvoiceRow houseHoldRow in houseHoldRows)
            {
                houseHoldRow.VatRate = 0;
            }

            ID = invoice.InvoiceNr;
            IssueDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.Date : DateTime.Today;
            InvoiceTypeCode = invoice.IsCredit ? SVEFAK_INVOICE_TYPE_CODE_CREDIT : SVEFAK_INVOICE_TYPE_CODE_DEBIT;

            Note = (!string.IsNullOrEmpty(invoice.InvoiceLabel)) ? invoice.InvoiceLabel + "\n\n" : string.Empty;
            if (invoice.IncludeOnInvoice && !string.IsNullOrEmpty(invoice.WorkingDescription))
            {
                Note += invoice.WorkingDescription + "\n\n";
            }

            if (!string.IsNullOrEmpty(defaultInvoiceText))
            {
                Note += defaultInvoiceText + "\n\n";
            }

            InvoiceCurrencyCode = (invoiceCurrency != null && !string.IsNullOrEmpty(invoiceCurrency.Code)) ? invoiceCurrency.Code : string.Empty;

            #region Nodes

            #region AdditionalDocumentReference

            if (!string.IsNullOrEmpty(invoice.InvoiceLabel) && invoice.InvoiceLabel.Contains('#'))
            {
                string orderNumber = invoice.InvoiceLabel;
                int startIndex = orderNumber.IndexOf("#");
                int endIndex = orderNumber.LastIndexOf("#");
                int length = endIndex - startIndex;
                orderNumber = orderNumber.Substring(startIndex, length);
                orderNumber = orderNumber.Replace("#", "");

                additionalDocumentReferences.Add(new AdditionalDocumentReference() { ID = orderNumber });
                //additionalDocumentReference.ID = orderNumber;//inköpsordernummer 
            }

            if (!String.IsNullOrEmpty(timeReportFilename.Trim()))
            {
                additionalDocumentReferences.Add(new AdditionalDocumentReference() { ID = timeReportFilename });
            }

            if (attachementNames != null)
            {
                foreach (var imageName in attachementNames)
                    additionalDocumentReferences.Add(new AdditionalDocumentReference() { ID = imageName });
            }

            if(!string.IsNullOrEmpty(invoice.ContractNr))
            {
                additionalDocumentReferences.Add(new AdditionalDocumentReference() { ID = invoice.ContractNr, IdentificationSchemeID = "CT" });
            }

            #endregion

            #region Buyer (Customer)

            ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);
            ContactAddressRow customerAddressCO = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.AddressCO);

            buyer.Name = customer.Name;
            buyer.NameCO = customerAddressCO != null && !string.IsNullOrEmpty(customerAddressCO.Text) ? ("c/o " + customerAddressCO.Text) : string.Empty;
            buyer.CityName = customerPostalAddress != null && !string.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : string.Empty;
            buyer.PostalZone = customerPostalCode != null && !string.IsNullOrEmpty(customerPostalCode.Text) ? customerPostalCode.Text : string.Empty;
            buyer.StreetName = customerAddressStreetName != null && !string.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : string.Empty;
            buyer.ContactName = !string.IsNullOrEmpty(invoice.ReferenceYour) ? invoice.ReferenceYour : string.Empty;
            buyer.OrgNr = !string.IsNullOrEmpty(customer.OrgNr) ? customer.OrgNr.Replace("-", "") : string.Empty;
            buyer.CustomerNr = !string.IsNullOrEmpty(customer.CustomerNr) ? customer.CustomerNr : string.Empty;

            if (invoice.ContactEComId != null && invoice.ContactEComId > 0 && customerContactEcoms != null)
            {
                var buyerEmail = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoice.ContactEComId);
                buyer.ElectronicMail = buyerEmail == null ? string.Empty : buyerEmail.Text;
            }

            if (invoice.ContactGLNId != null && invoice.ContactGLNId > 0 && customerContactEcoms != null)
            {
                var buyerGLN = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoice.ContactGLNId);
                buyer.GlnNumber = buyerGLN == null ? string.Empty : buyerGLN.Text;
            }

            if (!string.IsNullOrEmpty(customer.OrgNr))
            {
                var partyTaxSchemeBuyer = new PartyTaxScheme();
                partyTaxSchemeBuyer.CompanyID = customer.OrgNr.Replace("-", "");
                partyTaxSchemeBuyer.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_SWT;
                buyer.partyTaxSchemes.Add(partyTaxSchemeBuyer);
            }

            if (!string.IsNullOrEmpty(customer.VatNr))
            {
                var partyTaxSchemeVat = new PartyTaxScheme();
                partyTaxSchemeVat.CompanyID = customer.VatNr.Replace("-", "");
                partyTaxSchemeVat.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;
                buyer.partyTaxSchemes.Add(partyTaxSchemeVat);
            }

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
            seller.Name = company.Name;
            #endregion

            #region Address
            seller.CityName = companyPostalAddress != null && !String.IsNullOrEmpty(companyPostalAddress.Text) ? companyPostalAddress.Text : String.Empty;
            seller.PostalZone = companyPostalCode != null && !String.IsNullOrEmpty(companyPostalCode.Text) ? companyPostalCode.Text : String.Empty;
            seller.StreetName = companyAddressStreetName != null && !String.IsNullOrEmpty(companyAddressStreetName.Text) ? companyAddressStreetName.Text : String.Empty;
            #endregion

            #region PartyTaxScheme

            ContactAddressRow companyHQCountry = companyBoardHQAddress.GetRow(TermGroup_SysContactAddressRowType.Country);

            if (!string.IsNullOrEmpty(company.OrgNr))
            {
                var partyTaxScheme = new PartyTaxScheme();
                partyTaxScheme.CompanyID = company.OrgNr.Replace("-", "");
                partyTaxScheme.ExemptionReason = !String.IsNullOrEmpty(exemptionReason) ? exemptionReason : String.Empty;
                partyTaxScheme.RegistrationAddressCityName = companyHQCountry != null && !string.IsNullOrEmpty(companyHQCountry.Text) ? companyHQCountry.Text : String.Empty;
                partyTaxScheme.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_SWT;
                seller.partyTaxSchemes.Add(partyTaxScheme);
            }

            if (!string.IsNullOrEmpty(company.VatNr))
            {
                var partyTaxScheme = new PartyTaxScheme();
                partyTaxScheme.CompanyID = company.VatNr.Replace("-", "");
                partyTaxScheme.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;
                seller.partyTaxSchemes.Add(partyTaxScheme);
            }

            #endregion

            #region Contact
            seller.ElectronicMail = companyEmail != null && !String.IsNullOrEmpty(companyEmail.Text) ? companyEmail.Text : String.Empty;
            seller.Telefax = companyFax != null && !String.IsNullOrEmpty(companyFax.Text) ? companyFax.Text : String.Empty;
            seller.Telephone = companyPhoneWork != null && !String.IsNullOrEmpty(companyPhoneWork.Text) ? companyPhoneWork.Text : String.Empty;
            #endregion

            #region AccountsContactName
            seller.AccountsContactName = !String.IsNullOrEmpty(invoice.ReferenceOur) ? invoice.ReferenceOur : String.Empty;
            #endregion

            #endregion

            #region Delivery

            if (invoice.DeliveryDate.HasValue)
                delivery.ActualDeliveryDateTime = invoice.DeliveryDate.Value;

            if (!string.IsNullOrEmpty(invoice.InvoiceHeadText))
            {
                delivery.StreetName = invoice.InvoiceHeadText.Replace("\n"," ");
            }
            else
            {
                ContactAddressRow deliveryPostalCode = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
                ContactAddressRow deliveryPostalAddress = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
                ContactAddressRow deliveryAddress = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

                delivery.CityName = deliveryPostalAddress?.Text ?? string.Empty;
                delivery.PostalZone = deliveryPostalCode?.Text ?? string.Empty;
                delivery.StreetName = deliveryAddress?.Text ?? string.Empty;
            }

            #endregion

            #region PaymentMeans
            if (paymentInformation != null)
            {
                AddPaymentMeans(paymentMeansList, invoice, paymentInformation, TermGroup_SysPaymentType.BG);
                AddPaymentMeans(paymentMeansList, invoice, paymentInformation, TermGroup_SysPaymentType.PG);
                AddPaymentMeans(paymentMeansList, invoice, paymentInformation, TermGroup_SysPaymentType.Bank);
                AddPaymentMeans(paymentMeansList, invoice, paymentInformation, TermGroup_SysPaymentType.BIC);
                
                /*
                foreach (PaymentInformationRow pIRow in paymentInformation.ActivePaymentInformationRows.OrderByDescending(a => a.Default))
                {
                    PaymentMeans paymentMeans = new PaymentMeans();
                    paymentMeans.PaymentMeansTypeCode = 1; //Accourding to dokumentation "SFTI Basic Invoice - Termlista.pdf" , search for PaymentMeansCode
                    if (invoice.DueDate.HasValue)
                        paymentMeans.DuePaymentDate = invoice.DueDate.Value;
                    else
                        paymentMeans.DuePaymentDate = null;

                    if (pIRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG && !String.IsNullOrEmpty(pIRow.PaymentNr) && pIRow.PaymentNr.Length > 0)
                    {
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(pIRow.PaymentNr);
                        paymentMeans.FinancialInstitutionID = SVEFAK_FINANICIAL_INSTITUTION_ID_BG;
                    }
                    else if (pIRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG && !String.IsNullOrEmpty(pIRow.PaymentNr) && pIRow.PaymentNr.Length > 0)
                    {
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(pIRow.PaymentNr);
                        paymentMeans.FinancialInstitutionID = SVEFAK_FINANICIAL_INSTITUTION_ID_PG;
                    }
                    else if (pIRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.Bank && !String.IsNullOrEmpty(pIRow.PaymentNr) && pIRow.PaymentNr.Length > 0)
                    {
                        PaymentInformationRow companyBic = paymentInformation.ActivePaymentInformationRows.Where(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC).FirstOrDefault();
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(pIRow.PaymentNr);
                        paymentMeans.FinancialInstitutionID = (companyBic != null && string.IsNullOrEmpty(companyBic.PaymentNr)) ? companyBic.PaymentNr : String.Empty;
                    }
                    else if (pIRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC && !String.IsNullOrEmpty(pIRow.PaymentNr) && pIRow.PaymentNr.Length > 0)
                    {
                        string[] splitBic = pIRow.PaymentNr.Split(new char[] { '/' });
                        if (splitBic.Length > 1)
                        {
                            paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(splitBic[1]);
                            paymentMeans.IdentificationSchemeName = (splitBic[1] != null && !string.IsNullOrEmpty(splitBic[1])) ? "IBAN" : String.Empty;
                            paymentMeans.FinancialInstitutionID = !String.IsNullOrEmpty(splitBic[0]) ? splitBic[0] : String.Empty;
                        }
                    }

                    paymentMeans.PaymentInstructionID = !String.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoice.InvoiceNr;
                    
                    paymentMeansList.Add(paymentMeans);
                }
                */

            }
            #endregion

            #region PaymentTerms
            paymentTerms.Note = (invoice.PaymentCondition != null && !String.IsNullOrEmpty(invoice.PaymentCondition.Name)) ? invoice.PaymentCondition.Name : String.Empty;
            paymentTerms.PenaltySurchargePercent = String.Empty;
            #endregion

            #region AllowanceCharge
            //see Svefaktura exempelsamling,http://www.sftiverifiering.se/svefakturaexempelsamling/SvefakturaExempelsamling.htm, "Faktura m rabatt på huvudnivå".

            if (invoiceFeeRow != null)
            {
                AllowanceChargeHead allowanceChargeHead = new AllowanceChargeHead();
                allowanceChargeHead.ChargeIndicator = true;
                allowanceChargeHead.ReasonCodeName = "Fakturaavgift"; // i dont think we can translate this, accourding to documentation http://svefaktura.sftiverifiering.se/svefaktura.aspx?lang=se#AllowanceCharge
                
                allowanceChargeHead.Amount = invoice.PriceListTypeInclusiveVat ? invoiceFeeRow.SumAmountCurrency- invoiceFeeRow.VatAmountCurrency : invoiceFeeRow.SumAmountCurrency;
                if (invoice.IsCredit)
                {
                    allowanceChargeHead.Amount = allowanceChargeHead.Amount * -1;
                }
                allowanceChargeHead.AmountCurrencyID = InvoiceCurrencyCode;
                allowanceChargeHead.TaxCategoryID = (invoiceFeeRow.VatRate > 0) ? "S" : "E";
                allowanceChargeHead.TaxCategoryPercent = invoiceFeeRow.VatRate;
                allowanceChargeHead.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;
                allowanceChargeHeads.Add(allowanceChargeHead);
            }

            if (invoiceFreightRow != null)
            {
                AllowanceChargeHead allowanceChargeHead = new AllowanceChargeHead();
                allowanceChargeHead.ChargeIndicator = true;
                allowanceChargeHead.ReasonCodeName = "Fraktavgift"; // i dont think we can translate this, accourding to documentation http://svefaktura.sftiverifiering.se/svefaktura.aspx?lang=se#AllowanceCharge
                allowanceChargeHead.Amount = invoice.PriceListTypeInclusiveVat ? invoiceFreightRow.SumAmountCurrency - invoiceFreightRow.VatAmountCurrency : invoiceFreightRow.SumAmountCurrency;
                if (invoice.IsCredit)
                {
                    allowanceChargeHead.Amount = allowanceChargeHead.Amount * -1;
                }
                allowanceChargeHead.AmountCurrencyID = InvoiceCurrencyCode;
                allowanceChargeHead.TaxCategoryID = (invoiceFreightRow.VatRate > 0) ? "S" : "E";
                allowanceChargeHead.TaxCategoryPercent = invoiceFreightRow.VatRate;
                allowanceChargeHead.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;
                allowanceChargeHeads.Add(allowanceChargeHead);
            }

            decimal houseHoldAmount = 0;
            foreach (var houseHoldRow in houseHoldRows)
            {
                AllowanceChargeHead allowanceChargeHead = new AllowanceChargeHead();
                allowanceChargeHead.ChargeIndicator = false;
                allowanceChargeHead.ReasonCodeName = "RotRutSumma"; //accourding to an example given by inexchange
                decimal amount = houseHoldRow.SumAmountCurrency * (-1); // our household amount is negative
                allowanceChargeHead.Amount = invoice.IsCredit ? (-1) * amount : amount;
                houseHoldAmount += allowanceChargeHead.Amount;
                allowanceChargeHead.AmountCurrencyID = InvoiceCurrencyCode;
                allowanceChargeHead.TaxCategoryID = "E";
                allowanceChargeHead.TaxCategoryPercent = 0;
                allowanceChargeHead.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;
                allowanceChargeHeads.Add(allowanceChargeHead);
            }

            #endregion

            #region Taxtotal

            taxTotal.TotalTaxAmount = invoice.IsCredit ? (-1) * invoice.VATAmountCurrency : invoice.VATAmountCurrency;
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
                    if (invoiceRow.IsCentRoundingRow)
                    {
                        continue;
                    }

                    taxableAmount += invoice.PriceListTypeInclusiveVat ? invoiceRow.SumAmountCurrency-invoiceRow.VatAmountCurrency : invoiceRow.SumAmountCurrency;
                    taxAmount += invoiceRow.VatAmountCurrency;
                }
                
                if (taxableAmount != 0)
                {
                    var taxSubTotal = new TaxSubTotal();
                    taxSubTotal.TaxableAmount = invoice.IsCredit ? (-1) * taxableAmount : taxableAmount;
                    taxSubTotal.TaxAmount = invoice.IsCredit ? (-1) * taxAmount : taxAmount;
                    taxSubTotal.TaxableAmountCurrencyID = InvoiceCurrencyCode;
                    taxSubTotal.TaxAmountCurrencyID = InvoiceCurrencyCode;
                    taxSubTotal.TaxCategoryID = (vatRate > 0) ? "S" : "E";
                    taxSubTotal.TaxCategoryPercent = vatRate;
                    taxSubTotal.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;
                    if (vatRate == 0 && invoice.VatType == (int)TermGroup_InvoiceVatType.Contractor)
                    {
                        taxSubTotal.TaxCategoryExemptionReason = "Omvänd betalningsskyldighet";
                    }
                    else if (vatRate == 0 && invoice.VatType == (int)TermGroup_InvoiceVatType.ExportWithinEU)
                    {
                        int langId = (int)TermGroup_Languages.Swedish;
                        if (invoice.Actor.Customer.SysCountryId.HasValue && invoice.Actor.Customer.SysCountryId != (int)TermGroup_Country.SE)
                        {
                            langId = (int)TermGroup_Languages.English;
                        }
                        taxSubTotal.TaxCategoryExemptionReason = TermCacheManager.Instance.GetText(7697, 1, "Artikel 138 i mervärdesskattedirektivet", langId);
                    }
                    else
                    {
                        taxSubTotal.TaxCategoryExemptionReason = "";
                    }

                    

                    taxTotal.taxSubtotals.Add(taxSubTotal);
                }
                
            }

            #endregion

            #endregion

            #region LegalTotal

            legalTotal.LineExtensionTotalAmount = invoice.IsCredit ? (-1) * (invoice.SumAmountCurrency) : invoice.SumAmountCurrency;  //exklusive vat
            legalTotal.LineExtensionTotalAmountCurrencyID = InvoiceCurrencyCode;
            if (invoice.PriceListTypeInclusiveVat)
            {
                legalTotal.TaxExclusiveTotalAmount = invoice.IsCredit ? (-1) * (invoice.TotalAmountCurrency - invoice.VATAmountCurrency - invoice.CentRounding) : (invoice.TotalAmountCurrency - invoice.VATAmountCurrency - invoice.CentRounding);
            }
            else
            {
                legalTotal.TaxExclusiveTotalAmount = invoice.IsCredit ? (-1) * (invoice.SumAmountCurrency + invoice.InvoiceFeeCurrency + invoice.FreightAmountCurrency + houseHoldAmount) : invoice.SumAmountCurrency + invoice.InvoiceFeeCurrency + invoice.FreightAmountCurrency - houseHoldAmount;
            }
            legalTotal.TaxExclusiveTotalAmountCurrencyID = InvoiceCurrencyCode;
            legalTotal.TaxInclusiveTotalAmount = invoice.IsCredit ? (-1) * invoice.TotalAmountCurrency : invoice.TotalAmountCurrency;
            legalTotal.TaxInclusiveTotalAmountCurrencyID = InvoiceCurrencyCode;
            legalTotal.RoundOfAmount = invoice.IsCredit ? (-1) * invoice.CentRounding : invoice.CentRounding;
            legalTotal.RoundOfAmountCurrencyID = InvoiceCurrencyCode;

            #endregion

            #region Invoiceline

            int invoiceLineId = 0;
            var firstProductRow = invoiceRows.FirstOrDefault(r => r.Type == (int)SoeInvoiceRowType.ProductRow && r.VatRate > 0);
            var allPositivQuantityCrediInvoice = false;
            
            if (invoice.IsCredit)
            {
                //fix to try to handle both 98596 and 91366
                allPositivQuantityCrediInvoice = !invoiceRows.Any(x => x.Quantity < 0);
            }

            foreach (var invoiceRow in invoiceRows.OrderBy(x => x.RowNr))
            {
                if (invoiceRow.IsCentRoundingRow || invoiceRow.IsInvoiceFeeRow || invoiceRow.IsFreightAmountRow || invoiceRow.HouseholdTaxDeductionRow != null)
                    continue;

                if (invoiceRow.Type == (int)SoeInvoiceRowType.TextRow && (string.IsNullOrEmpty(invoiceRow.Text) || invoiceRow.Text.Trim().Length == 0))
                    continue;

                invoiceLineId++;
                var invoiceLine = new InvoiceLine
                {
                    ID = invoiceLineId, //Not using RowNr since it sometimes can have duplicates and gaps
                    InvoicedQuantity = invoiceRow.Quantity ?? 0
                };

                invoiceLine.InvoicedQuantityUnitCode = (invoiceRow.ProductUnit != null && !string.IsNullOrEmpty(invoiceRow.ProductUnit.Name)) ? invoiceRow.ProductUnit.Name : string.Empty;

                if (invoice.PriceListTypeInclusiveVat)
                    invoiceLine.LineExtensionAmount = invoice.IsCredit ? (-1) * (invoiceRow.SumAmountCurrency-invoiceRow.VatAmountCurrency) : (invoiceRow.SumAmountCurrency-invoiceRow.VatAmountCurrency);
                else
                    invoiceLine.LineExtensionAmount = invoice.IsCredit ? (-1) * invoiceRow.SumAmountCurrency : invoiceRow.SumAmountCurrency;

                invoiceLine.LineExtensionAmountCurrencyID = InvoiceCurrencyCode;
                invoiceLine.Note = (invoiceRow.Product != null && ((InvoiceProduct)invoiceRow.Product).ShowDescriptionAsTextRow && !string.IsNullOrEmpty(invoiceRow.Product.Description)) ? invoiceRow.Product.Description : String.Empty;

                //Not used right now, AdditionalDocumentReference is used instead
                if (!string.IsNullOrEmpty(invoice.OrderReference))
                {
                    invoiceLine.OrderLineReference = new OrderLineReference
                    {
                        //BuyersLineID = 66; //Reference to orderrow, not used right now
                        BuyersID = invoice.OrderReference
                    };
                }

                invoiceLine.ItemDescription = !string.IsNullOrEmpty(invoiceRow.Text) ? invoiceRow.Text : string.Empty;
                invoiceLine.ItemSellerIdentificationID = (invoiceRow.Product != null && !string.IsNullOrEmpty(invoiceRow.Product.Number) && !hideArticleNr) ? invoiceRow.Product.Number : string.Empty;

                //Det finns ett problem i att ha anteckningsrader med 0 % moms, och det är om mottagaren exempelvis tar emot fakturor via Peppol.Där måste man isf ange en momsspecifikation(TaxSubTotal) med en undantagsorsak(ExemptionReason) även om alla raderna bara är 0 - belopp, schematronvalideringen kräver detta.
                //Det är relativt vanligt nämligen, och rekommendationen är egentligen alltid att anteckningsraderna ska ha samma momssats som någon av fakturaraderna med faktiska belopp.
                if (invoiceRow.Type == (int)SoeInvoiceRowType.TextRow && firstProductRow != null)
                {
                    invoiceLine.TaxCategoryID = "S";
                    invoiceLine.TaxCategoryPercent = firstProductRow.VatRate;
                }
                else
                {
                    invoiceLine.TaxCategoryID = (invoiceRow.VatRate > 0) ? "S" : "E";
                    invoiceLine.TaxCategoryPercent = invoiceRow.VatRate;
                }

                invoiceLine.TaxSchemeID = SVEFAK_TAX_SCHEME_ID_VAT;

                invoiceLine.ActualDeliveryDateTime = invoiceRow.Date;

                decimal aggregatedDiscountAmount = (invoiceRow.DiscountAmountCurrency + invoiceRow.Discount2AmountCurrency);
                if (aggregatedDiscountAmount > 0 && invoiceLine.InvoicedQuantity >= 0 || aggregatedDiscountAmount < 0 && invoiceLine.InvoicedQuantity < 0)
                {
                    if (invoice.PriceListTypeInclusiveVat)
                    {
                        if (invoiceLine.InvoicedQuantity != 0)
                            invoiceLine.BasePriceAmount = invoice.IsCredit ? (-1) * (invoiceRow.AmountCurrency + (invoiceRow.VatAmountCurrency / invoiceLine.InvoicedQuantity)) : (invoiceRow.AmountCurrency - (invoiceRow.VatAmountCurrency / invoiceLine.InvoicedQuantity));
                        else
                            invoiceLine.BasePriceAmount = invoice.IsCredit ? (-1) * (invoiceRow.AmountCurrency + invoiceRow.VatAmountCurrency) : (invoiceRow.AmountCurrency - invoiceRow.VatAmountCurrency);
                    }
                    else
                    {
                        invoiceLine.BasePriceAmount = invoiceRow.AmountCurrency;
                    }
                }
                else
                {
                    decimal priceAmount = invoiceRow.AmountCurrency;
                    if (invoiceLine.InvoicedQuantity != 0 && invoice.PriceListTypeInclusiveVat)
                    {
                        priceAmount = priceAmount - ( (invoice.IsCredit && invoiceLine.InvoicedQuantity > 0 && allPositivQuantityCrediInvoice)  ? -(invoiceRow.VatAmountCurrency / invoiceLine.InvoicedQuantity) : (invoiceRow.VatAmountCurrency / invoiceLine.InvoicedQuantity));
                    }

                    if (invoiceLine.InvoicedQuantity != 0 && aggregatedDiscountAmount != 0)
                    {
                        priceAmount = priceAmount - (aggregatedDiscountAmount / invoiceLine.InvoicedQuantity); //har sett att discountamont används som "påslag" om man har negativ rabattsats på kund, då ska den inte räknas som rabatt i svefaktura
                    }
                    invoiceLine.BasePriceAmount = priceAmount;
                }

                invoiceLine.BasePriceAmountCurrencyID = InvoiceCurrencyCode;

                //Skip case when sign for quantity and discount dont match...is handled above.....
                //AllowanceCharge är alltid positivt, sedan är det "ChargeIndicator" true / false som berättar om det är avgift(true) eller rabatt(false).
                if ( (aggregatedDiscountAmount > 0 && invoiceLine.InvoicedQuantity > 0) || (aggregatedDiscountAmount < 0 && invoiceLine.InvoicedQuantity < 0) )
                {
                    var allowanceCharge = new AllowanceChargeRow
                    {
                        ChargeIndicator = false, // false för rabbat, true för påslag. vi använder inte true för påslag finns inte på radnivå.
                        Amount = Math.Abs(aggregatedDiscountAmount),
                        AmountCurrencyID = InvoiceCurrencyCode
                    };
                    invoiceLine.AllowanceChargeRow = allowanceCharge;
                }

                invoiceLines.Add(invoiceLine);
            }

            #endregion

            #region RequisitionistDocumentReference

            RequisitionistDocumentReference requisitionistDocumentReference = new RequisitionistDocumentReference();
            requisitionistDocumentReference.ID = !string.IsNullOrEmpty(invoice.InvoiceLabel) ? invoice.InvoiceLabel : "N/A"; // for now

            requisitionistDocumentReferences.Add(requisitionistDocumentReference);
            #endregion

            #region InitialInvoiceDocumentReference

            if (invoice.IsCredit && creditedInvoice != null)
            {
                initialInvoiceDocumentReference = new InitialInvoiceDocumentReference();
                initialInvoiceDocumentReference.ID = creditedInvoice.InvoiceNr;
            }

            #endregion

            #endregion

        }

        private bool AddPaymentMeans(List<PaymentMeans>paymentMeansList, CustomerInvoice invoice, PaymentInformation paymentInformation, TermGroup_SysPaymentType paymentType)
        {
            var paymentInfoRow = paymentInformation.ActivePaymentInformationRows.Where(x => x.SysPaymentTypeId == (int)paymentType).OrderByDescending(y => y.Default).FirstOrDefault();
            if (paymentInfoRow == null)
                return false;

            var paymentMeans = new PaymentMeans {
                PaymentMeansTypeCode = 1, //Accourding to dokumentation "SFTI Basic Invoice - Termlista.pdf" , search for PaymentMeansCode
                DuePaymentDate = (invoice.DueDate.HasValue) ? invoice.DueDate.Value : (DateTime?)null
            }; 

            if (!string.IsNullOrEmpty(paymentInfoRow.PaymentNr))
            {
                switch (paymentInfoRow.SysPaymentTypeId)
                {
                    case (int)TermGroup_SysPaymentType.BG:
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                        paymentMeans.FinancialInstitutionID = SVEFAK_FINANICIAL_INSTITUTION_ID_BG;
                        break;
                    case (int)TermGroup_SysPaymentType.PG:
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                        paymentMeans.FinancialInstitutionID = SVEFAK_FINANICIAL_INSTITUTION_ID_PG;
                        break;
                    case (int)TermGroup_SysPaymentType.Bank:
                        PaymentInformationRow companyBic = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC);
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                        paymentMeans.FinancialInstitutionID = (companyBic != null && string.IsNullOrEmpty(companyBic.PaymentNr)) ? companyBic.PaymentNr : String.Empty;
                        break;
                    case (int)TermGroup_SysPaymentType.BIC:
                        string[] splitBic = paymentInfoRow.PaymentNr.Split(new char[] { '/' });
                        if (splitBic.Length > 1)
                        {
                            paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(splitBic[1]);
                            paymentMeans.IdentificationSchemeName = (splitBic[1] != null && !string.IsNullOrEmpty(splitBic[1])) ? "IBAN" : String.Empty;
                            paymentMeans.FinancialInstitutionID = !String.IsNullOrEmpty(splitBic[0]) ? splitBic[0] : String.Empty;
                        }
                        break;
                }
            }
            paymentMeans.PaymentInstructionID = !string.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoice.InvoiceNr;
            paymentMeansList.Add(paymentMeans);
            return true;
        }

        public bool Export()
        {
            return false;
        }

        public bool Validate()
        {
            if (xdoc == null)
                return false;

            List<string> schemas = new List<string>();

            #region populate xsd paths
            var rootPath = ConfigSettings.SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL;
            schemas.Add(rootPath + "SFTI-BasicInvoice-1.0.xsd");
            schemas.Add(rootPath + "SFTI-CommonAggregateComponents-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-AllowanceChargeReasonCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-ChannelCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-ChipCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-CountryIdentificationCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-CurrencyCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-DocumentStatusCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-LatitudeDirectionCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-LineStatusCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-LongitudeDirectionCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-OperatorCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-PaymentMeansCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CodeList-SubstitutionStatusCode-1.0.xsd");
            schemas.Add(rootPath + "UBL-CommonAggregateComponents-1.0.xsd");
            schemas.Add(rootPath + "UBL-CommonBasicComponents-1.0.xsd");
            schemas.Add(rootPath + "UBL-CoreComponentParameters-1.0.xsd");
            schemas.Add(rootPath + "UBL-CoreComponentTypes-1.0.xsd");
            schemas.Add(rootPath + "UBL-SpecializedDatatypes-1.0.xsd");
            schemas.Add(rootPath + "UBL-UnspecializedDatatypes-1.0.xsd");
            #endregion

            return rgm.ValidateXDocument(xdoc, schemas);
        }
        
        public void ToXml(ref XDocument document)
        {
            #region Prereq

            LineItemCountNumeric = invoiceLines.Count;

            #endregion

            XElement rootElement = new XElement(ns + "Invoice",
                                                    new XAttribute(XNamespace.Xmlns + "cac", ns_cac),
                                                    new XAttribute(XNamespace.Xmlns + "cbc", ns_cbc),
                                                    new XAttribute(XNamespace.Xmlns + "ccts", ns_ccts),
                                                    new XAttribute(XNamespace.Xmlns + "cur", ns_cur),
                                                    new XAttribute(XNamespace.Xmlns + "sdt", ns_sdt),
                                                    new XAttribute(XNamespace.Xmlns + "udt", ns_udt),
                                                    new XAttribute(XNamespace.Xmlns + "xsi", ns_xsi));
            
            //Must exist
            rootElement.Add(new XElement(ns + "ID", ID));           
            rootElement.Add(new XElement(ns_cbc + "IssueDate", IssueDate.ToString(ISODateFormat)));
            rootElement.Add(new XElement(ns + "InvoiceTypeCode", InvoiceTypeCode));                                  
            rootElement.Add(new XElement(ns_cbc + "Note", Note));            
            rootElement.Add(new XElement(ns + "InvoiceCurrencyCode", InvoiceCurrencyCode));            
            rootElement.Add(new XElement(ns + "LineItemCountNumeric", LineItemCountNumeric));
                                      
            //Add nodes
            //additionalDocumentReference.AddNode(ref rootElement);
            foreach (AdditionalDocumentReference adr in additionalDocumentReferences)
            {
                adr.AddNode(ref rootElement);
            }
            buyer.AddNode(ref rootElement);
            seller.AddNode(ref rootElement);
            delivery.AddNode(ref rootElement);

            //There can only be up to 3 of these...
            foreach (var paymentMeans in paymentMeansList.Take(3) )
            {
                paymentMeans.AddNode(ref rootElement);
            }

            paymentTerms.AddNode(ref rootElement);

            foreach (var allowanceChargeHead in allowanceChargeHeads)
            {
                allowanceChargeHead.AddNode(ref rootElement);
            }
            taxTotal.AddNode(ref rootElement);
            legalTotal.AddNode(ref rootElement);

            //NOTE: At least one line must exist or the document will not be validated against the schema
            foreach (var invoiceLine in invoiceLines)
            {
                invoiceLine.AddNode(ref rootElement);    
            }

            //NOTE: At least one line must exist or the document will not be validated against the schema
            foreach (var requisitionistDocumentReference in requisitionistDocumentReferences)
            {
                requisitionistDocumentReference.AddNode(ref rootElement);
            }

            if (initialInvoiceDocumentReference != null)
                initialInvoiceDocumentReference.AddNode(ref rootElement);


            document.Add(rootElement);
            xdoc = document;
        }
    }

    public class BuyerParty : SvefakturaBase
    {
        public string Name { get; set; }
        public string NameCO { get; set; }
        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string ElectronicMail { get; set; }
        public string ContactName { get; set; }
        public string OrgNr { get; set; }
        public string CustomerNr { get; set; }
        public string GlnNumber { get; set; }

        public List<PartyTaxScheme> partyTaxSchemes = new List<PartyTaxScheme>();

        public void AddNode(ref XElement root)
        {
            XElement buyerPartyNode = new XElement(ns_cac + "BuyerParty");

            #region Party

            XElement partyNode = new XElement(ns_cac + "Party");

            #region PartyIdentification

            partyNode.Add(new XElement(ns_cac + "PartyIdentification",new XElement(ns_cac + "ID", CustomerNr)));

            if (!string.IsNullOrEmpty(GlnNumber))
            {
                partyNode.Add( new XElement(ns_cac + "PartyIdentification", new XElement(ns_cac + "ID", GlnNumber, new XAttribute("identificationSchemeAgencyID", "9"))) );
            }

            #endregion

            #region PartyName
            //Must exist
            XElement partyNameNode = new XElement(ns_cac + "PartyName",
                            new XElement(ns_cbc + "Name", Name));

            //if (!String.IsNullOrEmpty(NameCO) && NameCO.Length > 0)
            //    partyNameNode.Add(new XElement(ns_cbc + "Name", NameCO));

            partyNode.Add(partyNameNode);          

            #endregion

            #region Adress
            XElement adressNode = new XElement(ns_cac + "Address");

            //if (!string.IsNullOrEmpty(NameCO))
            //    adressNode.Add(new XElement(ns_cbc + "Postbox", NameCO));

            if (!string.IsNullOrEmpty(StreetName) )
                adressNode.Add(new XElement(ns_cbc + "StreetName", StreetName));

            //changed from PostBox tag since it ends up below street adress on invoice paper and Department seems not!
            if (!string.IsNullOrEmpty(NameCO))
                adressNode.Add(new XElement(ns_cbc + "Department", NameCO));

            if ( !string.IsNullOrEmpty(CityName) )
                adressNode.Add(new XElement(ns_cbc + "CityName", CityName));

            if ( !string.IsNullOrEmpty(PostalZone) )
                adressNode.Add(new XElement(ns_cbc + "PostalZone", RemoveIllegalCharacters(PostalZone)));
                       
            partyNode.Add(adressNode);

            #endregion

            #region PartyTaxScheme

            foreach (var partyTaxScheme in partyTaxSchemes)
            {
                partyTaxScheme.AddNode(ref partyNode);
            }

            #endregion   

            #region Contact
            var contactNode = new XElement(ns_cac + "Contact");

            if ( !string.IsNullOrEmpty(ContactName) )
                contactNode.Add(new XElement(ns_cbc + "Name", ContactName));

            if ( !string.IsNullOrEmpty(ElectronicMail) )
                contactNode.Add(new XElement(ns_cbc + "ElectronicMail", ElectronicMail));

            partyNode.Add(contactNode);
            #endregion

            #endregion

            buyerPartyNode.Add(partyNode);                          
            root.Add(buyerPartyNode);
        }
    }

    public class SellerParty : SvefakturaBase
    {
        public string Name { get; set; }
        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string Telephone { get; set; }
        public string Telefax { get; set; }
        public string ElectronicMail { get; set; }
        public string AccountsContactName { get; set; }
        public List<PartyTaxScheme> partyTaxSchemes = new List<PartyTaxScheme>();

        public void AddNode(ref XElement root)
        {
            XElement sellerPartyNode = new XElement(ns_cac + "SellerParty");

            #region SellerParty

            #region Party
            XElement partyNode = new XElement(ns_cac + "Party");

            #region PartyName            
            //Must exist
            partyNode.Add(new XElement(ns_cac + "PartyName",
                            new XElement(ns_cbc + "Name", Name)));

            #endregion

            #region Adress
            XElement adressNode = new XElement(ns_cac + "Address");

            if (!String.IsNullOrEmpty(StreetName) && StreetName.Length > 0)
                adressNode.Add(new XElement(ns_cbc + "StreetName", StreetName));

            if (!String.IsNullOrEmpty(CityName) && CityName.Length > 0)
                adressNode.Add(new XElement(ns_cbc + "CityName", CityName));

            if (!String.IsNullOrEmpty(PostalZone) && PostalZone.Length > 0)
                adressNode.Add(new XElement(ns_cbc + "PostalZone", RemoveIllegalCharacters(PostalZone)));

            partyNode.Add(adressNode);
            #endregion

            #region PartyTaxScheme

            foreach (var partyTaxScheme in partyTaxSchemes)
            {
                partyTaxScheme.AddNode(ref partyNode);
            }

            #endregion            

            #region Contact
            XElement contactNode = new XElement(ns_cac + "Contact");

            if (!String.IsNullOrEmpty(Telephone) && Telephone.Length > 0)
                contactNode.Add(new XElement(ns_cbc + "Telephone", RemoveIllegalCharacters(Telephone)));

            if (!String.IsNullOrEmpty(Telefax) && Telefax.Length > 0)
                contactNode.Add(new XElement(ns_cbc + "Telefax", RemoveIllegalCharacters(Telefax)));

            if (!String.IsNullOrEmpty(ElectronicMail) && ElectronicMail.Length > 0)
                contactNode.Add(new XElement(ns_cbc + "ElectronicMail", ElectronicMail));

            partyNode.Add(contactNode);

            #endregion

            #endregion

            sellerPartyNode.Add(partyNode);        

            #region AccountsContact
            XElement accountsContactNode = new XElement(ns_cac + "AccountsContact");

            if (!String.IsNullOrEmpty(AccountsContactName) && AccountsContactName.Length > 0)
                accountsContactNode.Add(new XElement(ns_cbc + "Name", AccountsContactName));            
                
            #endregion

            sellerPartyNode.Add(accountsContactNode);   

            #endregion

            root.Add(sellerPartyNode);
        }
    }

    public class PartyTaxScheme : SvefakturaBase
    {
        public String CompanyID { get; set; }
        public String ExemptionReason { get; set; }        
        public String RegistrationAddressCityName { get; set; }
        public String CountryIdentificationCode { get; set; }
        public String TaxSchemeID { get; set; }

        public void AddNode(ref XElement party)
        {
            XElement partyTaxSchemeNode = new XElement(ns_cac + "PartyTaxScheme");

            #region PartyTaxScheme

            if (!String.IsNullOrEmpty(CompanyID) && CompanyID.Length > 0)
                partyTaxSchemeNode.Add(new XElement(ns_cac + "CompanyID", CompanyID));

            if (!String.IsNullOrEmpty(ExemptionReason) && ExemptionReason.Length > 0)
                partyTaxSchemeNode.Add(new XElement(ns_cbc + "ExemptionReason", ExemptionReason));

            #region RegistrationAddress
            XElement registrationAddress = new XElement(ns_cac + "RegistrationAddress");

            if (!string.IsNullOrEmpty(RegistrationAddressCityName) || !string.IsNullOrEmpty(CountryIdentificationCode))
            {
                if ( !string.IsNullOrEmpty(RegistrationAddressCityName) )
                    registrationAddress.Add(new XElement(ns_cbc + "CityName", RegistrationAddressCityName));

                if ( !string.IsNullOrEmpty(CountryIdentificationCode) )
                    registrationAddress.Add(new XElement(ns_cac + "Country", new XElement(ns_cac + "IdentificationCode", CountryIdentificationCode)));

                partyTaxSchemeNode.Add(registrationAddress);
            }
            
            #endregion

            #region TaxScheme

            partyTaxSchemeNode.Add(new XElement(ns_cac + "TaxScheme", new XElement(ns_cac + "ID", TaxSchemeID)));

            #endregion

            #endregion

            party.Add(partyTaxSchemeNode);
        }
    }
    
    public class Delivery : SvefakturaBase
    {
        public DateTime? ActualDeliveryDateTime { get; set; }
        public String StreetName { get; set; }
        public String CityName { get; set; }
        public String PostalZone { get; set; }
        
        public void AddNode(ref XElement root)
        {
            XElement deliveryNode = new XElement(ns_cac + "Delivery");
            
            #region Delivery

            if (ActualDeliveryDateTime.HasValue)
                deliveryNode.Add(new XElement(ns_cbc + "ActualDeliveryDateTime", ToStringDateWithTime(ActualDeliveryDateTime)));
            

            XElement deliveryAddressNode = new XElement(ns_cac + "DeliveryAddress");

            if (!String.IsNullOrEmpty(StreetName) && StreetName.Length > 0)
                deliveryAddressNode.Add(new XElement(ns_cbc + "StreetName", StreetName));

             if (!String.IsNullOrEmpty(CityName) && CityName.Length > 0)
                deliveryAddressNode.Add(new XElement(ns_cbc + "CityName", CityName));

             if (!String.IsNullOrEmpty(PostalZone) && PostalZone.Length > 0)
                deliveryAddressNode.Add(new XElement(ns_cbc + "PostalZone", PostalZone));

            deliveryNode.Add(deliveryAddressNode);

            #endregion

            root.Add(deliveryNode);
        }
    }

    public class PaymentMeans : SvefakturaBase
    {
        public int PaymentMeansTypeCode { get; set; }
        public DateTime? DuePaymentDate { get; set; }
        public String PayeeFinancialAccountID { get; set; }
        public String FinancialInstitutionID { get; set; }
        public String PaymentInstructionID { get; set; }
        public String IdentificationSchemeName { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement paymentMeansNode = new XElement(ns_cac + "PaymentMeans");

            #region PaymentMeans

            if (PaymentMeansTypeCode > 0)
                paymentMeansNode.Add(new XElement(ns_cac + "PaymentMeansTypeCode", PaymentMeansTypeCode));

            if (DuePaymentDate.HasValue)
                paymentMeansNode.Add(new XElement(ns_cbc + "DuePaymentDate", DuePaymentDate.Value.ToString(ISODateFormat)));

            #region PayeeFinancialAccount

            XElement payeeFinancialAccountNode = new XElement(ns_cac + "PayeeFinancialAccount");

            if (!String.IsNullOrEmpty(PayeeFinancialAccountID) && PayeeFinancialAccountID.Length > 0)
                if (!String.IsNullOrEmpty(IdentificationSchemeName) && IdentificationSchemeName.Length > 0)
                    payeeFinancialAccountNode.Add(new XElement(ns_cac + "ID", PayeeFinancialAccountID,new XAttribute("identificationSchemeName", IdentificationSchemeName)));
                else
                    payeeFinancialAccountNode.Add(new XElement(ns_cac + "ID", PayeeFinancialAccountID));

            #region FinancialInstitutionBranch

            if (!String.IsNullOrEmpty(FinancialInstitutionID) && FinancialInstitutionID.Length > 0)
                payeeFinancialAccountNode.Add(new XElement(ns_cac + "FinancialInstitutionBranch", new XElement(ns_cac + "FinancialInstitution", new XElement(ns_cac + "ID", FinancialInstitutionID))));

            #endregion

            if (!String.IsNullOrEmpty(PaymentInstructionID) && PaymentInstructionID.Length > 0)
                payeeFinancialAccountNode.Add(new XElement(ns_cac + "PaymentInstructionID", PaymentInstructionID));

            paymentMeansNode.Add(payeeFinancialAccountNode);

            #endregion

            #endregion

            root.Add(paymentMeansNode);            
        }
    }

    public class PaymentTerms : SvefakturaBase
    {
        public String Note { get; set; }
        public String PenaltySurchargePercent { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement paymentTermsNode = new XElement(ns_cac + "PaymentTerms");

            if (!String.IsNullOrEmpty(Note) && Note.Length > 0)
                paymentTermsNode.Add(new XElement(ns_cbc + "Note", Note));

            if (!String.IsNullOrEmpty(PenaltySurchargePercent) && PenaltySurchargePercent.Length > 0)
                paymentTermsNode.Add(new XElement(ns_cbc + "PenaltySurchargePercent", PenaltySurchargePercent));

             root.Add(paymentTermsNode);
        }
    }

    public class TaxTotal : SvefakturaBase
    {
        public decimal TotalTaxAmount { get; set; }
        public String TotalTaxAmountCurrencyID { get; set; }
        public List<TaxSubTotal> taxSubtotals = new List<TaxSubTotal>();

        public void AddNode(ref XElement root)
        {
            XElement taxTotalNode = new XElement(ns_cac + "TaxTotal");

            #region TaxTotal
            //Must exist            
            taxTotalNode.Add(new XElement(ns_cbc + "TotalTaxAmount", TotalTaxAmount, new XAttribute("amountCurrencyID", TotalTaxAmountCurrencyID)));

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

    public class TaxSubTotal : SvefakturaBase
    {
        public decimal TaxableAmount { get; set; }
        public String TaxableAmountCurrencyID { get; set; }
        public decimal TaxAmount { get; set; }
        public String TaxAmountCurrencyID { get; set; }
        public String TaxCategoryID { get; set; }
        public decimal TaxCategoryPercent { get; set; }
        public String TaxCategoryExemptionReason { get; set; }
        public String TaxSchemeID { get; set; }

        public void AddNode(ref XElement parent)
        {

            XElement taxSubTotalNode = new XElement(ns_cac + "TaxSubTotal");

            #region TaxSubTotal
            
            //Must exist
            taxSubTotalNode.Add(new XElement(ns_cbc + "TaxableAmount", TaxableAmount, new XAttribute("amountCurrencyID", TaxableAmountCurrencyID)));
            taxSubTotalNode.Add(new XElement(ns_cbc + "TaxAmount", TaxAmount, new XAttribute("amountCurrencyID", TaxAmountCurrencyID)));

            #region TaxCategory

            XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");
            //Must exist
            taxCategoryNode.Add(new XElement(ns_cac + "ID", TaxCategoryID));
            taxCategoryNode.Add(new XElement(ns_cbc + "Percent", TaxCategoryPercent));
            if(!string.IsNullOrEmpty(TaxCategoryExemptionReason))
                taxCategoryNode.Add(new XElement(ns_cbc + "ExemptionReason", TaxCategoryExemptionReason));


            #region TaxScheme

            XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");
            //Must exist
            taxSchemeNode.Add(new XElement(ns_cac + "ID", TaxSchemeID));

            taxCategoryNode.Add(taxSchemeNode);
            #endregion

            taxSubTotalNode.Add(taxCategoryNode);
            #endregion

            #endregion

            parent.Add(taxSubTotalNode);

        }

    }

    public class LegalTotal : SvefakturaBase
    {
        public decimal LineExtensionTotalAmount { get; set; }
        public decimal TaxExclusiveTotalAmount { get; set; }
        public decimal TaxInclusiveTotalAmount { get; set; }
        public decimal RoundOfAmount { get; set; }

        public String LineExtensionTotalAmountCurrencyID { get; set; }
        public String TaxExclusiveTotalAmountCurrencyID { get; set; }
        public String TaxInclusiveTotalAmountCurrencyID { get; set; }
        public String RoundOfAmountCurrencyID { get; set; }
        

        public void AddNode(ref XElement root)
        {
            XElement legalTotalNode = new XElement(ns_cac + "LegalTotal");

            #region LegalTotal
            
            legalTotalNode.Add(new XElement(ns_cbc + "LineExtensionTotalAmount", LineExtensionTotalAmount, new XAttribute("amountCurrencyID", LineExtensionTotalAmountCurrencyID)));

            if (TaxExclusiveTotalAmount > 0)
                legalTotalNode.Add(new XElement(ns_cbc + "TaxExclusiveTotalAmount", TaxExclusiveTotalAmount, new XAttribute("amountCurrencyID", TaxExclusiveTotalAmountCurrencyID)));
            
             legalTotalNode.Add(new XElement(ns_cbc + "TaxInclusiveTotalAmount", TaxInclusiveTotalAmount,new XAttribute("amountCurrencyID",TaxInclusiveTotalAmountCurrencyID)));
             legalTotalNode.Add(new XElement(ns_cac + "RoundOffAmount", RoundOfAmount, new XAttribute("amountCurrencyID", RoundOfAmountCurrencyID)));     
            
            #endregion  

            root.Add(legalTotalNode);    
        }
    }

    public class InvoiceLine : SvefakturaBase
    {
        public int ID { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public string InvoicedQuantityUnitCode { get; set; }
        public decimal LineExtensionAmount { get; set; }
        public string LineExtensionAmountCurrencyID { get; set; }
        public string Note { get; set; }
        public AllowanceChargeRow AllowanceChargeRow { get; set; }

        public string ItemDescription { get; set; }
        public string ItemSellerIdentificationID { get; set; }

        public string TaxCategoryID { get; set; }
        public decimal TaxCategoryPercent { get; set; }
        public string TaxSchemeID { get; set; }

        public decimal BasePriceAmount { get; set; }
        public string BasePriceAmountCurrencyID { get; set; }

        public DateTime? ActualDeliveryDateTime { get; set; }

        public OrderLineReference OrderLineReference { get; set; }//not used right now, AdditionalDocumentReference is used instead.        

        public void AddNode(ref XElement root)
        {
            XElement invoiceLineNode = new XElement(ns_cac + "InvoiceLine");

            #region InvoiceLine
    
            //Must exist
            invoiceLineNode.Add(new XElement(ns_cac + "ID", ID));

            var nrOfDecimals = NumberUtility.GetDecimalCount(InvoicedQuantity);

            if (nrOfDecimals < 2)
            {
                nrOfDecimals = 2;
            }

            //changed Round from 2 since we support 6 decimals when calculating row amount and inexchange controls LineExtensionAmount against price*qty
            invoiceLineNode.Add(new XElement(ns_cbc + "InvoicedQuantity", Math.Round(InvoicedQuantity, nrOfDecimals), new XAttribute("quantityUnitCode", InvoicedQuantityUnitCode)));
            invoiceLineNode.Add(new XElement(ns_cbc + "LineExtensionAmount", LineExtensionAmount, new XAttribute("amountCurrencyID", LineExtensionAmountCurrencyID)));

            if (!string.IsNullOrEmpty(Note) && Note.Length > 0)
                invoiceLineNode.Add(new XElement(ns_cbc + "Note", Note));

            //OrderLineReference
            if (OrderLineReference != null)
                OrderLineReference.AddNode(ref invoiceLineNode);
            
            #region DeliveryDate
            if ( ActualDeliveryDateTime.HasValue ) 
            {
                XElement DeliveryDateNode = new XElement(ns_cac + "Delivery");
                 DeliveryDateNode.Add(new XElement(ns_cbc + "ActualDeliveryDateTime", ToStringDateWithTime(ActualDeliveryDateTime)));
                invoiceLineNode.Add(DeliveryDateNode);
            }
            #endregion
            
            #region AllowanceCharge
            if (AllowanceChargeRow != null)
            {
                AllowanceChargeRow.AddNode(ref invoiceLineNode);
            }

            #endregion

            #region Item

            XElement itemNode = new XElement(ns_cac + "Item");

            if (!string.IsNullOrEmpty(ItemDescription) && ItemDescription.Length > 0)
                itemNode.Add(new XElement(ns_cbc + "Description", ItemDescription));

            #region SellersItemIdentification

            XElement sellersItemIdentificationNode = new XElement(ns_cac + "SellersItemIdentification");
            //Must exist
            sellersItemIdentificationNode.Add(new XElement(ns_cac + "ID", ItemSellerIdentificationID));

            itemNode.Add(sellersItemIdentificationNode);

            #endregion


            #region TaxCategory

            XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");
            //Must exist
            taxCategoryNode.Add(new XElement(ns_cac + "ID", TaxCategoryID));
            taxCategoryNode.Add(new XElement(ns_cbc + "Percent", TaxCategoryPercent));

            #region TaxScheme

            XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");
            //Must exist
            taxSchemeNode.Add(new XElement(ns_cac + "ID", TaxSchemeID));

            taxCategoryNode.Add(taxSchemeNode);
            #endregion

            itemNode.Add(taxCategoryNode);
            #endregion

            #region BasePrice

            XElement basePriceNode = new XElement(ns_cac + "BasePrice");

            //Inexchange Tim: Det är helt OK att sätta "157,666666" som antal; det går att ha obegränsat antal decimaler för Antal och Enhetspris; men fr.o.m radbelopp och summering/nedåt måste det vara 2 decimaler.
            //Adding more decimals pga sometimes RowSum/Quantity = > 2 decimaler
            //Must exist
            basePriceNode.Add(new XElement(ns_cbc + "PriceAmount", Math.Round(BasePriceAmount, 6), new XAttribute("amountCurrencyID", BasePriceAmountCurrencyID)));

            itemNode.Add(basePriceNode);
            #endregion

            invoiceLineNode.Add(itemNode);

            #endregion

            #endregion

            root.Add(invoiceLineNode);
        }
    }

    public class RequisitionistDocumentReference : SvefakturaBase
    {
        public string ID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement requisitionistDocumentReferenceNode = new XElement(ns + "RequisitionistDocumentReference");
            
            //Must exist                    
            requisitionistDocumentReferenceNode.Add(new XElement(ns_cac + "ID", ID));

            root.Add(requisitionistDocumentReferenceNode);
        }
    }

    public class AdditionalDocumentReference : SvefakturaBase
    {
        public String ID { get; set; }
        public string IdentificationSchemeID { get; set; } = "ATS"; 

        private const string identificationSchemeAgencyName = "SFTI";

        public void AddNode(ref XElement root)
        {
            XElement additionalDocumentReferenceNode = new XElement(ns + "AdditionalDocumentReference");
            
            additionalDocumentReferenceNode.Add(new XElement(ns_cac + "ID", ID, new XAttribute("identificationSchemeAgencyName", identificationSchemeAgencyName), new XAttribute("identificationSchemeID", IdentificationSchemeID)));

            root.Add(additionalDocumentReferenceNode);
        }
    }

    public class InitialInvoiceDocumentReference : SvefakturaBase
    {
        public String ID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement initialInvoiceDocumentReferenceNode = new XElement(ns + "InitialInvoiceDocumentReference");

            //Must exist                    
            initialInvoiceDocumentReferenceNode.Add(new XElement(ns_cac + "ID", ID));

            root.Add(initialInvoiceDocumentReferenceNode);
        }
    }

    public class OrderLineReference : SvefakturaBase
    {
        public String BuyersLineID { get; set; } //reference to orderrow
        public String BuyersID { get; set; } //reference to Order

        public void AddNode(ref XElement root)
        {
            XElement orderLineReferenceNode = new XElement(ns_cac + "OrderLineReference");
            if (!String.IsNullOrEmpty(BuyersLineID) && BuyersLineID.Length > 0)
                orderLineReferenceNode.Add(new XElement(ns_cac + "BuyersLineID", BuyersLineID));

            if (!String.IsNullOrEmpty(BuyersID) && BuyersID.Length > 0)
            {
                XElement orderReferenceNode = new XElement(ns_cac + "OrderReference");
                orderReferenceNode.Add(new XElement(ns_cac + "BuyersID", BuyersID));
                orderLineReferenceNode.Add(orderReferenceNode);
            }
            root.Add(orderLineReferenceNode);
        }
    }

    public class AllowanceChargeRow : SvefakturaBase
    {
        public bool ChargeIndicator { get; set; }
        public decimal Amount { get; set; }
        public string AmountCurrencyID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement allowanceChargeNode = new XElement(ns_cac + "AllowanceCharge");
            allowanceChargeNode.Add(new XElement(ns_cbc + "ChargeIndicator", ChargeIndicator.ToString().ToLower()));
            allowanceChargeNode.Add(new XElement(ns_cbc + "Amount", Amount, new XAttribute("amountCurrencyID", AmountCurrencyID)));
            root.Add(allowanceChargeNode);
        }
    }

    public class AllowanceChargeHead : SvefakturaBase
    {
        public bool ChargeIndicator { get; set; }
        public string ReasonCodeName { get; set; }
        public decimal Amount { get; set; }
        public String AmountCurrencyID { get; set; }

        public String TaxCategoryID { get; set; }
        public decimal TaxCategoryPercent { get; set; }
        public String TaxSchemeID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement allowanceChargeNode = new XElement(ns + "AllowanceCharge");
            allowanceChargeNode.Add(new XElement(ns_cbc + "ChargeIndicator", ChargeIndicator.ToString().ToLower()));
            allowanceChargeNode.Add(new XElement(ns_cac + "ReasonCode", "ZZZ", new XAttribute("name", ReasonCodeName)));
            allowanceChargeNode.Add(new XElement(ns_cbc + "Amount", Amount, new XAttribute("amountCurrencyID", AmountCurrencyID)));

            #region TaxCategory

            XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");
            //Must exist
            taxCategoryNode.Add(new XElement(ns_cac + "ID", TaxCategoryID));
            taxCategoryNode.Add(new XElement(ns_cbc + "Percent", TaxCategoryPercent));

            #region TaxScheme

            XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");
            //Must exist
            taxSchemeNode.Add(new XElement(ns_cac + "ID", TaxSchemeID));

            taxCategoryNode.Add(taxSchemeNode);
            #endregion

            allowanceChargeNode.Add(taxCategoryNode);
            #endregion


            root.Add(allowanceChargeNode);
        }
    }

    public class SvefakturaBase
    {
        #region Namespaces

        public XNamespace ns = "urn:sfti:documents:BasicInvoice:1:0";
        public XNamespace ns_cac = "urn:sfti:CommonAggregateComponents:1:0";
        public XNamespace ns_cbc = "urn:oasis:names:tc:ubl:CommonBasicComponents:1:0";
        public XNamespace ns_ccts = "urn:oasis:names:tc:ubl:CoreComponentParameters:1:0";
        public XNamespace ns_cur = "urn:oasis:names:tc:ubl:codelist:CurrencyCode:1:0";
        public XNamespace ns_sdt = "urn:oasis:names:tc:ubl:SpecializedDatatypes:1:0";
        public XNamespace ns_udt = "urn:oasis:names:tc:ubl:UnspecializedDatatypes:1:0";
        public XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public const int SVEFAK_INVOICE_TYPE_CODE_DEBIT = 380;
        public const int SVEFAK_INVOICE_TYPE_CODE_CREDIT = 381;
        public const String SVEFAK_FINANICIAL_INSTITUTION_ID_BG = "BGABSESS";
        public const String SVEFAK_FINANICIAL_INSTITUTION_ID_PG = "PGSISESS";
        public const String SVEFAK_TAX_SCHEME_ID_VAT = "VAT";
        public const String SVEFAK_TAX_SCHEME_ID_SWT = "SWT";

        internal const string ISODateFormat = "yyyy-MM-dd";

        #endregion

        public string RemoveIllegalCharacters(string input)
        {
            input = input.Replace(" ", "");
            input = input.Replace("-", "");
            return input;
        }

        public string ToStringDateWithTime(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString(ISODateFormat) + "T" + date.Value.ToString("HH:mm:ss") : string.Empty;

        }

    }
}
