using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.PeppolBilling
{
    public class PeppolBilling : PeppolBillingBase
    {
        /**
         * For dokumentation see : http://docs.peppol.eu/poacc/billing/3.0/
         * 
         * */

        #region Root elements
        public string ID { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Note { get; set; }
        public string DocumentCurrencyCode { get; set; }
        public string BuyerReference { get; set; }
        public string ProjectReference { get; set; }
        public string ContractReference { get; set; }
        public bool IsCredit { get; set; }

        #endregion

        #region Nodes

        private readonly List<AdditionalDocumentReference> additionalDocumentReferences = new List<AdditionalDocumentReference>();
        private readonly PeppolParty Supplier = new PeppolParty();
        private readonly PeppolParty Buyer = new PeppolParty();
        private readonly PeppolDelivery Delivery = new PeppolDelivery();
        private readonly List<PeppolPaymentMeans> PaymentMeans = new List<PeppolPaymentMeans>();
        private readonly PeppolPaymentTerms PaymentTerms = new PeppolPaymentTerms();
        private readonly List<PeppolAllowanceCharge> AllowanceCharges = new List<PeppolAllowanceCharge>();
        private readonly PeppolTaxTotal TaxTotal = new PeppolTaxTotal();
        private readonly PeppolLegalMonetaryTotal LegalMonetaryTotal = new PeppolLegalMonetaryTotal();
        private readonly List<PeppolInvoiceLine> InvoiceLines = new List<PeppolInvoiceLine>();
        private InitialInvoiceDocumentReference initialInvoiceDocumentReference = null;

        #endregion

        public PeppolBilling(CustomerInvoice invoice, PaymentInformation paymentInformation, Company company, SysCurrency invoiceCurrency, Customer customer, List<CustomerInvoiceRow> invoiceRows, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<ContactAddressRow> companyAddress, List<ContactAddressRow> companyBoardHQAddress, List<ContactECom> companyContactEcoms, string exemptionReason, CustomerInvoice creditedInvoice, int actorCompanyId, int userId, string timeReportFilename, List<string> attachementNames = null, string defaultInvoiceText = null)
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
            IsCredit = invoice.IsCredit;
            IssueDate = invoice.InvoiceDate ?? DateTime.Today;
            DueDate = invoice.DueDate;
            BuyerReference = invoice.ReferenceYour;
            //ProjectReference = invoice.Project.Number;
            ContractReference = invoice.ContractNr;

            Note = (!string.IsNullOrEmpty(invoice.InvoiceLabel)) ? invoice.InvoiceLabel + "\n\n" : string.Empty;
            if (invoice.IncludeOnInvoice && !string.IsNullOrEmpty(invoice.WorkingDescription))
            {
                Note += invoice.WorkingDescription + "\n\n";
            }

            if (!string.IsNullOrEmpty(defaultInvoiceText))
            {
                Note += defaultInvoiceText + "\n\n";
            }

            DocumentCurrencyCode = invoiceCurrency?.Code ?? string.Empty;

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

            #endregion

            #region Buyer (Customer)

            ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);
            ContactAddressRow customerAddressCO = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.AddressCO);

            Buyer.Name = customer.Name;
            Buyer.Address.NameCO = customerAddressCO != null && !string.IsNullOrEmpty(customerAddressCO.Text) ? ("c/o " + customerAddressCO.Text) : string.Empty;
            Buyer.Address.CityName = customerPostalAddress?.Text ?? string.Empty;
            Buyer.Address.PostalZone = customerPostalCode?.Text ?? string.Empty;
            Buyer.Address.StreetName = customerAddressStreetName?.Text ?? string.Empty;
            Buyer.ContactName = invoice.ReferenceYour ?? string.Empty;
            Buyer.OrgNumber = customer.OrgNr != null ? customer.OrgNr.Replace("-", "") : string.Empty;
            Buyer.CustomerNr = customer.CustomerNr ?? string.Empty;
            Buyer.VATNumber = customer.VatNr;

            if (invoice.ContactEComId != null && invoice.ContactEComId > 0 && customerContactEcoms != null)
            {
                var buyerEmail = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoice.ContactEComId);
                Buyer.ElectronicMail = buyerEmail == null ? string.Empty : buyerEmail.Text;
            }

            if (invoice.ContactGLNId != null && invoice.ContactGLNId > 0 && customerContactEcoms != null)
            {
                Buyer.GlnNumber = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoice.ContactGLNId)?.Text ?? string.Empty;
            }

            #endregion

            #region Seller

            ContactAddressRow companyPostalCode = companyAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow companyPostalAddress = companyAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow companyAddressStreetName = companyAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

            #region PartyName
            Supplier.Name = company.Name;
            #endregion

            #region Address
            Supplier.Address.CityName = companyPostalAddress?.Text ?? string.Empty;
            Supplier.Address.PostalZone = companyPostalCode?.Text ?? string.Empty;
            Supplier.Address.StreetName = companyAddressStreetName?.Text ?? string.Empty;
            #endregion

            #region PartyTaxScheme

            ContactAddressRow companyHQCountry = companyBoardHQAddress.GetRow(TermGroup_SysContactAddressRowType.Country);
            
            Supplier.ExemptionReason = !String.IsNullOrEmpty(exemptionReason) ? exemptionReason : string.Empty;
            Supplier.VATNumber = company.VatNr?.Replace("-", "");
            Supplier.OrgNumber = company.OrgNr?.Replace("-", "");
            Supplier.GlnNumber = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.GlnNumber)?.Text ?? string.Empty;

            #endregion

            #region Contact
            Supplier.ElectronicMail = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)?.Text ?? string.Empty;
            Supplier.Telephone = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob)?.Text ?? string.Empty;
            #endregion

            #region AccountsContactName
            Supplier.ContactName = !String.IsNullOrEmpty(invoice.ReferenceOur) ? invoice.ReferenceOur : String.Empty;
            #endregion

            #endregion

            #region Delivery

            if (invoice.DeliveryDate.HasValue)
                Delivery.ActualDeliveryDateTime = invoice.DeliveryDate.Value;

            if (!string.IsNullOrEmpty(invoice.InvoiceHeadText))
            {
                Delivery.DeliveryAddress.StreetName = invoice.InvoiceHeadText.Replace("\n"," ");
            }
            else
            {
                Delivery.DeliveryAddress.StreetName = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.Address)?.Text ?? string.Empty;
                Delivery.DeliveryAddress.PostalZone = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode)?.Text ?? string.Empty;
                Delivery.DeliveryAddress.CityName = customerDeliveryAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress)?.Text ?? string.Empty;
            }

            #endregion

            #region Payment

            if (paymentInformation != null)
            {
                AddPaymentMeans(PaymentMeans, paymentInformation, TermGroup_SysPaymentType.BG);
                AddPaymentMeans(PaymentMeans, paymentInformation, TermGroup_SysPaymentType.PG);
                AddPaymentMeans(PaymentMeans, paymentInformation, TermGroup_SysPaymentType.Bank);
                AddPaymentMeans(PaymentMeans, paymentInformation, TermGroup_SysPaymentType.BIC);
            }

            PaymentTerms.Note = invoice.PaymentCondition?.Name ?? string.Empty;
            
            #endregion

            #region AllowanceCharge

            if (invoiceFeeRow != null)
            {
                var allowanceCharge = new PeppolAllowanceCharge();
                allowanceCharge.ChargeIndicator = true;
                allowanceCharge.AllowanceChargeReason = "Fakturaavgift";
                allowanceCharge.AllowanceChargeReasonCode = PEPPOL_CHARGE_REASON_CODE_INVOICING;

                allowanceCharge.Amount = invoice.PriceListTypeInclusiveVat ? invoiceFeeRow.SumAmountCurrency- invoiceFeeRow.VatAmountCurrency : invoiceFeeRow.SumAmountCurrency;
                if (invoice.IsCredit)
                {
                    allowanceCharge.Amount = allowanceCharge.Amount * -1;
                }
                allowanceCharge.CurrencyID = DocumentCurrencyCode;
                allowanceCharge.TaxCategoryID = (invoiceFeeRow.VatRate > 0) ? "S" : "E";
                allowanceCharge.TaxCategoryPercent = invoiceFeeRow.VatRate;
                allowanceCharge.TaxCategoryTaxSchemeID = PEPPOL_TAX_SCHEME_ID_VAT;
                AllowanceCharges.Add(allowanceCharge);
            }

            if (invoiceFreightRow != null)
            {
                var allowanceCharge = new PeppolAllowanceCharge();
                allowanceCharge.ChargeIndicator = true;
                allowanceCharge.AllowanceChargeReason = "Fraktavgift"; 
                allowanceCharge.AllowanceChargeReasonCode = PEPPOL_CHARGE_REASON_CODE_FREIGHT_SERVICE;

                allowanceCharge.Amount = invoice.PriceListTypeInclusiveVat ? invoiceFreightRow.SumAmountCurrency - invoiceFreightRow.VatAmountCurrency : invoiceFreightRow.SumAmountCurrency;
                if (invoice.IsCredit)
                {
                    allowanceCharge.Amount = allowanceCharge.Amount * -1;
                }
                allowanceCharge.CurrencyID = DocumentCurrencyCode;
                allowanceCharge.TaxCategoryID = (invoiceFreightRow.VatRate > 0) ? "S" : "E";
                allowanceCharge.TaxCategoryPercent = invoiceFreightRow.VatRate;
                allowanceCharge.TaxCategoryTaxSchemeID = PEPPOL_TAX_SCHEME_ID_VAT;
                AllowanceCharges.Add(allowanceCharge);
            }

            decimal houseHoldAmount = 0;
            foreach (var houseHoldRow in houseHoldRows)
            {
                var allowanceCharge = new PeppolAllowanceCharge();
                allowanceCharge.ChargeIndicator = false;
                allowanceCharge.AllowanceChargeReason = "RotRutSumma"; //accourding to an example given by inexchange
                decimal amount = houseHoldRow.SumAmountCurrency * (-1); // our household amount is negative
                allowanceCharge.Amount = invoice.IsCredit ? (-1) * amount : amount;
                houseHoldAmount += allowanceCharge.Amount;
                allowanceCharge.CurrencyID = DocumentCurrencyCode;
                allowanceCharge.TaxCategoryID = "E";
                allowanceCharge.TaxCategoryPercent = 0;
                allowanceCharge.TaxCategoryTaxSchemeID = PEPPOL_TAX_SCHEME_ID_VAT;
                AllowanceCharges.Add(allowanceCharge);
            }

            #endregion

            #region Taxtotal

            TaxTotal.TaxAmount = invoice.IsCredit ? (-1) * invoice.VATAmountCurrency : invoice.VATAmountCurrency;
            TaxTotal.TotalTaxAmountCurrencyID = DocumentCurrencyCode;

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
                    var taxSubTotal = new PeppolTaxSubTotal();
                    taxSubTotal.TaxableAmount = invoice.IsCredit ? (-1) * taxableAmount : taxableAmount;
                    taxSubTotal.TaxAmount = invoice.IsCredit ? (-1) * taxAmount : taxAmount;
                    taxSubTotal.CurrencyID = DocumentCurrencyCode;
                    taxSubTotal.TaxCategoryID = (vatRate > 0) ? "S" : "E";
                    taxSubTotal.TaxCategoryPercent = vatRate;
                    taxSubTotal.TaxSchemeID = PEPPOL_TAX_SCHEME_ID_VAT;
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

                    TaxTotal.TaxSubtotals.Add(taxSubTotal);
                }
            }

            #endregion

            #endregion

            #region LegalTotal

            LegalMonetaryTotal.LineExtensionAmount = invoice.IsCredit ? (-1) * (invoice.SumAmountCurrency) : invoice.SumAmountCurrency;  //exklusive vat
            LegalMonetaryTotal.CurrencyID = DocumentCurrencyCode;
            if (invoice.PriceListTypeInclusiveVat)
            {
                LegalMonetaryTotal.TaxExclusiveAmount = invoice.IsCredit ? (-1) * (invoice.TotalAmountCurrency - invoice.VATAmountCurrency - invoice.CentRounding) : (invoice.TotalAmountCurrency - invoice.VATAmountCurrency - invoice.CentRounding);
            }
            else
            {
                LegalMonetaryTotal.TaxExclusiveAmount = invoice.IsCredit ? (-1) * (invoice.SumAmountCurrency + invoice.InvoiceFeeCurrency + invoice.FreightAmountCurrency + houseHoldAmount) : invoice.SumAmountCurrency + invoice.InvoiceFeeCurrency + invoice.FreightAmountCurrency - houseHoldAmount;
            }

            LegalMonetaryTotal.TaxInclusiveAmount = invoice.IsCredit ? (-1) * invoice.TotalAmountCurrency : invoice.TotalAmountCurrency;
            LegalMonetaryTotal.PayableAmount = LegalMonetaryTotal.TaxInclusiveAmount;

            LegalMonetaryTotal.PayableRoundingAmount = invoice.IsCredit ? (-1) * invoice.CentRounding : invoice.CentRounding;
            
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
                var invoiceLine = new PeppolInvoiceLine
                {
                    ID = invoiceLineId, //Not using RowNr since it sometimes can have duplicates and gaps
                    InvoicedQuantity = invoiceRow.Quantity ?? 0
                };

                invoiceLine.InvoicedQuantityUnitCode = (invoiceRow.ProductUnit != null && !string.IsNullOrEmpty(invoiceRow.ProductUnit.Name)) ? invoiceRow.ProductUnit.Name : string.Empty;

                if (invoice.PriceListTypeInclusiveVat)
                    invoiceLine.LineExtensionAmount = invoice.IsCredit ? (-1) * (invoiceRow.SumAmountCurrency-invoiceRow.VatAmountCurrency) : (invoiceRow.SumAmountCurrency-invoiceRow.VatAmountCurrency);
                else
                    invoiceLine.LineExtensionAmount = invoice.IsCredit ? (-1) * invoiceRow.SumAmountCurrency : invoiceRow.SumAmountCurrency;

                invoiceLine.CurrencyID = DocumentCurrencyCode;
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

                invoiceLine.ItemName = !string.IsNullOrEmpty(invoiceRow.Text) ? invoiceRow.Text : string.Empty;
                invoiceLine.ItemSellersIdentificationID = (invoiceRow.Product != null && !string.IsNullOrEmpty(invoiceRow.Product.Number) && !hideArticleNr) ? invoiceRow.Product.Number : string.Empty;

                //Det finns ett problem i att ha anteckningsrader med 0 % moms, och det är om mottagaren exempelvis tar emot fakturor via Peppol.Där måste man isf ange en momsspecifikation(TaxSubTotal) med en undantagsorsak(ExemptionReason) även om alla raderna bara är 0 - belopp, schematronvalideringen kräver detta.
                //Det är relativt vanligt nämligen, och rekommendationen är egentligen alltid att anteckningsraderna ska ha samma momssats som någon av fakturaraderna med faktiska belopp.
                if (invoiceRow.Type == (int)SoeInvoiceRowType.TextRow && firstProductRow != null)
                {
                    invoiceLine.TaxCategory.ID = "S";
                    invoiceLine.TaxCategory.Percent = firstProductRow.VatRate;
                }
                else
                {
                    invoiceLine.TaxCategory.ID = (invoiceRow.VatRate > 0) ? "S" : "E";
                    invoiceLine.TaxCategory.Percent = invoiceRow.VatRate;
                }
                decimal aggregatedDiscountAmount = (invoiceRow.DiscountAmountCurrency + invoiceRow.Discount2AmountCurrency);
                if (aggregatedDiscountAmount > 0 && invoiceLine.InvoicedQuantity >= 0 || aggregatedDiscountAmount < 0 && invoiceLine.InvoicedQuantity < 0)
                {
                    if (invoice.PriceListTypeInclusiveVat)
                    {
                        if (invoiceLine.InvoicedQuantity != 0)
                            invoiceLine.PriceAmount = invoice.IsCredit ? (-1) * (invoiceRow.AmountCurrency + (invoiceRow.VatAmountCurrency / invoiceLine.InvoicedQuantity)) : (invoiceRow.AmountCurrency - (invoiceRow.VatAmountCurrency / invoiceLine.InvoicedQuantity));
                        else
                            invoiceLine.PriceAmount = invoice.IsCredit ? (-1) * (invoiceRow.AmountCurrency + invoiceRow.VatAmountCurrency) : (invoiceRow.AmountCurrency - invoiceRow.VatAmountCurrency);
                    }
                    else
                    {
                        invoiceLine.PriceAmount = invoiceRow.AmountCurrency;
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
                    invoiceLine.PriceAmount = priceAmount;
                }

                //Skip case when sign for quantity and discount dont match...is handled above.....
                //AllowanceCharge är alltid positivt, sedan är det "ChargeIndicator" true / false som berättar om det är avgift(true) eller rabatt(false).
                if ( (aggregatedDiscountAmount > 0 && invoiceLine.InvoicedQuantity > 0) || (aggregatedDiscountAmount < 0 && invoiceLine.InvoicedQuantity < 0) )
                {
                    invoiceLine.AllowanceCharge = new PeppolAllowanceCharge
                    {
                        ChargeIndicator = false, // false för rabbat, true för påslag. vi använder inte true för påslag finns inte på radnivå.
                        AllowanceChargeReason = "Rabatt",
                        AllowanceChargeReasonCode = PEPPOL_ALLOWANCE_REASON_CODE_DISCOUNT,
                        Amount = Math.Abs(aggregatedDiscountAmount),
                        CurrencyID = DocumentCurrencyCode,
                    };
                }

                InvoiceLines.Add(invoiceLine);
            }

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

        private void AddPaymentMeans(List<PeppolPaymentMeans>paymentMeansList, PaymentInformation paymentInformation, TermGroup_SysPaymentType paymentType)
        {
            var paymentInfoRow = paymentInformation.ActivePaymentInformationRows.Where(x => x.SysPaymentTypeId == (int)paymentType).OrderByDescending(y => y.Default).FirstOrDefault();
            if (paymentInfoRow == null)
                return;

            var paymentMeans = new PeppolPaymentMeans(); 

            if (!string.IsNullOrEmpty(paymentInfoRow.PaymentNr))
            {
                switch (paymentInfoRow.SysPaymentTypeId)
                {
                    case (int)TermGroup_SysPaymentType.BG:
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                        paymentMeans.FinancialInstitutionBranchID = PEPPOL_FINANICIAL_INSTITUTION_ID_BG;
                        break;
                    case (int)TermGroup_SysPaymentType.PG:
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                        paymentMeans.FinancialInstitutionBranchID = PEPPOL_FINANICIAL_INSTITUTION_ID_PG;
                        break;
                    case (int)TermGroup_SysPaymentType.Bank:
                        PaymentInformationRow companyBic = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC);
                        paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                        paymentMeans.FinancialInstitutionBranchID = (companyBic != null && string.IsNullOrEmpty(companyBic.PaymentNr)) ? companyBic.PaymentNr : String.Empty;
                        break;
                    case (int)TermGroup_SysPaymentType.BIC:
                        if (!string.IsNullOrEmpty(paymentInfoRow.BIC))
                        {
                            paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(paymentInfoRow.PaymentNr);
                            paymentMeans.FinancialInstitutionBranchID = paymentInfoRow.BIC;
                        }
                        else
                        {
                            string[] splitBic = paymentInfoRow.PaymentNr.Split('/' );
                            if (splitBic.Length > 1)
                            {
                                paymentMeans.PayeeFinancialAccountID = RemoveIllegalCharacters(splitBic[1]);
                                paymentMeans.FinancialInstitutionBranchID = !String.IsNullOrEmpty(splitBic[0]) ? splitBic[0] : String.Empty;
                            }
                        }
                        break;
                        
                }
            }
            //paymentMeans.PaymentInstructionID = !string.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoice.InvoiceNr;
            paymentMeansList.Add(paymentMeans);
        }

        public bool Export()
        {
            return false;
        }

        public void ToXml(ref XDocument document)
        {
            var rootElementName = IsCredit ? "CreditNote" : "Invoice";
            var ns = IsCredit ? ns_credit : ns_invoice;
            XElement rootElement = new XElement(ns + rootElementName,
                                                    new XAttribute(XNamespace.Xmlns + "cac", ns_cac),
                                                    new XAttribute(XNamespace.Xmlns + "cbc", ns_cbc)
                                                    );

            //Must exist
            rootElement.Add(new XElement(ns_cbc + "CustomizationID", "urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0"));
            rootElement.Add(new XElement(ns_cbc + "ProfileID", "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0"));
            
            rootElement.Add(new XElement(ns_cbc + "ID", ID));           
            rootElement.Add(new XElement(ns_cbc + "IssueDate", ToPeppolDate(IssueDate)));

            if (DueDate.HasValue)
            {
                rootElement.Add(new XElement(ns_cbc + "DueDate", ToPeppolDate(DueDate.Value)));
            }

            if (IsCredit)
                rootElement.Add(new XElement(ns_cbc + "CreditNoteTypeCode", PEPPOL_INVOICE_TYPE_CODE_CREDIT));
            else
                rootElement.Add(new XElement(ns_cbc + "InvoiceTypeCode", PEPPOL_INVOICE_TYPE_CODE_DEBIT));                                  

            rootElement.Add(new XElement(ns_cbc + "Note", Note));            
            rootElement.Add(new XElement(ns_cbc + "DocumentCurrencyCode", DocumentCurrencyCode));            

            if (!string.IsNullOrEmpty(BuyerReference))
            {
                rootElement.Add(new XElement(ns_cbc + "BuyerReference", BuyerReference));
            }

            if (!string.IsNullOrEmpty(ProjectReference))
            {
                rootElement.Add(new XElement(ns_cac + "ProjectReference", new XElement(ns_cbc + "ID", ProjectReference)));
            }

            if (!string.IsNullOrEmpty(ContractReference))
            {
                rootElement.Add(new XElement(ns_cac + "ContractDocumentReference", new XElement(ns_cbc + "ID", ContractReference)));
            }

            //Add nodes
            //additionalDocumentReference.AddNode(ref rootElement);

            /*
            foreach (AdditionalDocumentReference adr in additionalDocumentReferences)
            {
                adr.AddNode(ref rootElement);
            }
            */
            Supplier.AddNode(ref rootElement, PEPPOL_PARTY_SUPPLIER_TAG_NAME);
            Buyer.AddNode(ref rootElement, PEPPOL_PARTY_CUSTOMER_TAG_NAME);
            Delivery.AddNode(ref rootElement);

            //There can only be up to 3 of these...
            foreach (var paymentMeans in PaymentMeans.Take(3) )
            {
                paymentMeans.AddNode(ref rootElement);
            }

            PaymentTerms.AddNode(ref rootElement);

            foreach (var allowanceChargeHead in AllowanceCharges)
            {
                allowanceChargeHead.AddNode(ref rootElement, true);
            }

            TaxTotal.AddNode(ref rootElement);
            LegalMonetaryTotal.AddNode(ref rootElement, AllowanceCharges);

            //NOTE: At least one line must exist or the document will not be validated against the schema
            foreach (var invoiceLine in InvoiceLines)
            {
                invoiceLine.AddNode(ref rootElement, IsCredit);    
            }

            if (initialInvoiceDocumentReference != null)
                initialInvoiceDocumentReference.AddNode(ref rootElement, ns);

            document.Add(rootElement);
        }
    }

    public class PeppolAddress : PeppolBillingBase
    {
        public string NameCO { get; set; }
        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string CountryCode { get; set; }

        public void AddNode(ref XElement root, string subParentName)
        {
            XElement adressNode = new XElement(ns_cac + subParentName);

            if (!string.IsNullOrEmpty(StreetName))
                adressNode.Add(new XElement(ns_cbc + "StreetName", StreetName));

            if (!string.IsNullOrEmpty(NameCO))
                adressNode.Add(new XElement(ns_cbc + "AdditionalStreetName", NameCO));

            if (!string.IsNullOrEmpty(CityName))
                adressNode.Add(new XElement(ns_cbc + "CityName", CityName));

            if (!string.IsNullOrEmpty(PostalZone))
                adressNode.Add(new XElement(ns_cbc + "PostalZone", RemoveIllegalCharacters(PostalZone)));

            if (!string.IsNullOrEmpty(CountryCode))
                adressNode.Add(new XElement(ns_cbc + "Country", CountryCode));

            root.Add(adressNode);
        }
    }

    public class PeppolParty : PeppolBillingBase
    {
        public string Name { get; set; }
        public PeppolAddress Address { get; set; } = new PeppolAddress();
        public string Telephone { get; set; }
        public string ElectronicMail { get; set; }
        public string ContactName { get; set; }
        public string CustomerNr { get; set; }
        public string OrgNumber { get; set; }
        public string GlnNumber { get; set; }
        public string VATNumber { get; set; }
        public string ExemptionReason { get; set; }

        public void AddNode(ref XElement root, string parentTagName)
        {
            XElement parentPartyNode = new XElement(ns_cac + parentTagName);

            #region Party

            XElement partyNode = new XElement(ns_cac + "Party");

            if (!string.IsNullOrEmpty(GlnNumber))
            {
                partyNode.Add(GetGlnNrElement("EndpointID", GlnNumber));
            }
            else if(!string.IsNullOrEmpty(OrgNumber))
            {
                partyNode.Add(GetSwedishOrgNrElement("EndpointID", OrgNumber));
            }

            //Must exist
            XElement partyNameNode = new XElement(ns_cac + "PartyName", new XElement(ns_cbc + "Name", Name));
            partyNode.Add(partyNameNode);

            Address.AddNode(ref partyNode, "PostalAddress");

            AddPartyTaxNodes(ref partyNode);

            //Must exist
            XElement legalPartyNode = new XElement(ns_cac + "PartyLegalEntity", new XElement(ns_cbc + "RegistrationName", Name));
            if (!string.IsNullOrEmpty(OrgNumber))
            {
                legalPartyNode.Add(GetSwedishOrgNrElement("CompanyID", OrgNumber));
            }
            partyNode.Add(legalPartyNode);

            #region Contact
            var contactNode = new XElement(ns_cac + "Contact");

            if ( !string.IsNullOrEmpty(ContactName) )
                contactNode.Add(new XElement(ns_cbc + "Name", ContactName));

            if ( !string.IsNullOrEmpty(ElectronicMail) )
                contactNode.Add(new XElement(ns_cbc + "ElectronicMail", ElectronicMail));

            partyNode.Add(contactNode);

            #endregion

            #endregion

            parentPartyNode.Add(partyNode);                          
            root.Add(parentPartyNode);
        }

        public void AddPartyTaxNodes(ref XElement party)
        {
            if (!String.IsNullOrEmpty(VATNumber))
            {
                AddPartyTaxNode(party, VATNumber, PEPPOL_TAX_SCHEME_ID_VAT);
            }
            else if (!String.IsNullOrEmpty(OrgNumber))
            {
                AddPartyTaxNode(party, OrgNumber, PEPPOL_TAX_SCHEME_ID_SWT);
            }

            if (!String.IsNullOrEmpty(ExemptionReason))
            {
                AddPartyTaxNode(party, ExemptionReason, PEPPOL_TAX_SCHEME_ID_TXT);
            }
        }

        private static void AddPartyTaxNode(XElement party, string companyId, string taxSchemeId)
        {
            XElement partyTaxSchemeNode = new XElement(ns_cac + "PartyTaxScheme");
            partyTaxSchemeNode.Add(new XElement(ns_cbc + "CompanyID", companyId));
            partyTaxSchemeNode.Add(new XElement(ns_cac + "TaxScheme", new XElement(ns_cbc + "ID", taxSchemeId)));
            party.Add(partyTaxSchemeNode);
        }
    }

    public class PeppolDelivery : PeppolBillingBase
    {
        public DateTime? ActualDeliveryDateTime { get; set; }
        public PeppolAddress DeliveryAddress { get; set; } = new PeppolAddress();
        
        public void AddNode(ref XElement root)
        {
            XElement deliveryNode = new XElement(ns_cac + "Delivery");
            
            if (ActualDeliveryDateTime.HasValue)
                deliveryNode.Add(new XElement(ns_cbc + "ActualDeliveryDateTime", ToStringDateWithTime(ActualDeliveryDateTime)));

            XElement deliveryLocationNode = new XElement(ns_cac + "DeliveryLocation");
            
            DeliveryAddress.AddNode(ref deliveryLocationNode, "Address");

            deliveryNode.Add(deliveryLocationNode);

            root.Add(deliveryNode);
        }
    }

    public class PeppolPaymentMeans : PeppolBillingBase
    {
        public String PayeeFinancialAccountID { get; set; }
        public String FinancialInstitutionBranchID { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement paymentMeansNode = new XElement(ns_cac + "PaymentMeans");

            #region PaymentMeans

            paymentMeansNode.Add(new XElement(ns_cbc + "PaymentMeansCode", PEPPOL_PAYMENT_MEAN_CODE_CREDIT_TRANSFER));

            #region PayeeFinancialAccount

            XElement payeeFinancialAccountNode = new XElement(ns_cac + "PayeeFinancialAccount");

            if (!string.IsNullOrEmpty(PayeeFinancialAccountID))
            {
                payeeFinancialAccountNode.Add(new XElement(ns_cbc + "ID", PayeeFinancialAccountID));

                if (!String.IsNullOrEmpty(FinancialInstitutionBranchID))
                    payeeFinancialAccountNode.Add(new XElement(ns_cac + "FinancialInstitutionBranch", new XElement(ns_cbc + "ID", FinancialInstitutionBranchID)));
            }

            paymentMeansNode.Add(payeeFinancialAccountNode);

            #endregion

            #endregion

            root.Add(paymentMeansNode);            
        }
    }

    public class PeppolPaymentTerms : PeppolBillingBase
    {
        public string Note { get; set; }

        public void AddNode(ref XElement root)
        {
            if (!string.IsNullOrEmpty(Note))
            {
                XElement paymentTermsNode = new XElement(ns_cac + "PaymentTerms");

                paymentTermsNode.Add(new XElement(ns_cbc + "Note", Note));

                root.Add(paymentTermsNode);
            }
        }
    }

    public class PeppolTaxTotal : PeppolBillingBase
    {
        //Invoice total VAT amount, Invoice total VAT amount in accounting currency
        public decimal TaxAmount { get; set; }
        public string TotalTaxAmountCurrencyID { get; set; }
        //VAT BREAKDOWN
        public readonly List<PeppolTaxSubTotal> TaxSubtotals = new List<PeppolTaxSubTotal>();

        public void AddNode(ref XElement root)
        {
            XElement taxTotalNode = new XElement(ns_cac + "TaxTotal");

            //Must exist            
            taxTotalNode.Add(new XElement(ns_cbc + "TaxAmount", TaxAmount, GetCurrencyAttribute(TotalTaxAmountCurrencyID)));

            #region TaxSubTotal

            foreach (var taxSubtotal in TaxSubtotals)
            {
                taxSubtotal.AddNode(ref taxTotalNode);
            }

            #endregion

            root.Add(taxTotalNode);
        }
    }

    public class PeppolTaxSubTotal : PeppolBillingBase
    {
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string CurrencyID { get; set; }
        
        public string TaxCategoryID { get; set; }
        public decimal TaxCategoryPercent { get; set; }
        public string TaxCategoryExemptionReason { get; set; }
        public string TaxCategoryExemptionReasonCode { get; set; }
        public string TaxSchemeID { get; set; }

        public void AddNode(ref XElement parent)
        {
            XElement taxSubTotalNode = new XElement(ns_cac + "TaxSubtotal");

            #region TaxSubTotal
            
            //Must exist
            taxSubTotalNode.Add(new XElement(ns_cbc + "TaxableAmount", TaxableAmount,  GetCurrencyAttribute(CurrencyID)));
            taxSubTotalNode.Add(new XElement(ns_cbc + "TaxAmount", TaxAmount, GetCurrencyAttribute(CurrencyID)));

            #region TaxCategory

            XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");
            //Must exist
            taxCategoryNode.Add(new XElement(ns_cbc + "ID", TaxCategoryID));
            taxCategoryNode.Add(new XElement(ns_cbc + "Percent", TaxCategoryPercent));

            if (!string.IsNullOrEmpty(TaxCategoryExemptionReasonCode))
                taxCategoryNode.Add(new XElement(ns_cbc + "TaxExemptionReasonCode", TaxCategoryExemptionReasonCode));

            if (!string.IsNullOrEmpty(TaxCategoryExemptionReason))
                taxCategoryNode.Add(new XElement(ns_cbc + "TaxExemptionReason", TaxCategoryExemptionReason));

            #region TaxScheme

            XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");
            //Must exist
            taxSchemeNode.Add(new XElement(ns_cbc + "ID", TaxSchemeID));

            taxCategoryNode.Add(taxSchemeNode);
            #endregion

            taxSubTotalNode.Add(taxCategoryNode);
            #endregion

            #endregion

            parent.Add(taxSubTotalNode);
        }
    }

    public class PeppolLegalMonetaryTotal : PeppolBillingBase
    {
        public decimal LineExtensionAmount { get; set; }
        public decimal TaxExclusiveAmount { get; set; }
        public decimal TaxInclusiveAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal PayableRoundingAmount { get; set; }
        
        public string CurrencyID { get; set; }
        

        public void AddNode(ref XElement root, List<PeppolAllowanceCharge> allowanceCharges)
        {
            XElement legalTotalNode = new XElement(ns_cac + "LegalMonetaryTotal");

            #region LegalTotal
            
            legalTotalNode.Add(new XElement(ns_cbc + "LineExtensionAmount", LineExtensionAmount, GetCurrencyAttribute(CurrencyID)));
            legalTotalNode.Add(new XElement(ns_cbc + "TaxExclusiveAmount", TaxExclusiveAmount, GetCurrencyAttribute(CurrencyID)));
            legalTotalNode.Add(new XElement(ns_cbc + "TaxInclusiveAmount", TaxInclusiveAmount, GetCurrencyAttribute(CurrencyID)));

            if (allowanceCharges.Any())
            {
                var alowanceTotalAmount = allowanceCharges.Where(ac => !ac.ChargeIndicator).Sum(ac => ac.Amount);
                if (alowanceTotalAmount != 0)
                {
                    legalTotalNode.Add(new XElement(ns_cbc + "AllowanceTotalAmount", alowanceTotalAmount, GetCurrencyAttribute(CurrencyID)));
                }
                var chargeTotalAmount = allowanceCharges.Where(ac => ac.ChargeIndicator).Sum(ac => ac.Amount);
                if (chargeTotalAmount != 0)
                {
                    legalTotalNode.Add(new XElement(ns_cbc + "ChargeTotalAmount", chargeTotalAmount, GetCurrencyAttribute(CurrencyID)));
                }
            }
            
            if (PayableRoundingAmount != 0)
            {
                legalTotalNode.Add(new XElement(ns_cbc + "PayableRoundingAmount", PayableRoundingAmount, GetCurrencyAttribute(CurrencyID)));
            }
            
            legalTotalNode.Add(new XElement(ns_cbc + "PayableAmount", PayableAmount, GetCurrencyAttribute(CurrencyID)));

            #endregion

            root.Add(legalTotalNode);    
        }
    }

    public class PeppolInvoiceLine : PeppolBillingBase
    {
        public int ID { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public string InvoicedQuantityUnitCode { get; set; }
        public decimal LineExtensionAmount { get; set; }
        public string CurrencyID { get; set; }
        public string Note { get; set; }
        public PeppolAllowanceCharge AllowanceCharge { get; set; }
        public PeppolClassifiedTaxCategory TaxCategory { get; set; } = new PeppolClassifiedTaxCategory();

        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemSellersIdentificationID { get; set; }

        public decimal PriceAmount { get; set; }

        public OrderLineReference OrderLineReference { get; set; }//not used right now, AdditionalDocumentReference is used instead.        

        public void AddNode(ref XElement parent, bool isCredit)
        {
            var lineElementName = isCredit ? "CreditNoteLine" : "InvoiceLine";
            var invoiceLineNode = new XElement(ns_cac + lineElementName);

            //Must exist
            invoiceLineNode.Add(new XElement(ns_cbc + "ID", ID));

            if (!string.IsNullOrEmpty(Note))
                invoiceLineNode.Add(new XElement(ns_cbc + "Note", Note));

            var nrOfDecimals = NumberUtility.GetDecimalCount(InvoicedQuantity);

            if (nrOfDecimals < 2)
            {
                nrOfDecimals = 2;
            }

            //changed Round from 2 since we support 6 decimals when calculating row amount and inexchange controls LineExtensionAmount against price*qty
            var quantityElementName = isCredit ? "CreditedQuantity" : "InvoicedQuantity";
            invoiceLineNode.Add(new XElement(ns_cbc + quantityElementName, Math.Round(InvoicedQuantity, nrOfDecimals), new XAttribute("unitCode", InvoicedQuantityUnitCode)));
            invoiceLineNode.Add(new XElement(ns_cbc + "LineExtensionAmount", LineExtensionAmount, GetCurrencyAttribute(CurrencyID)));

            //OrderLineReference
            if (OrderLineReference != null)
                OrderLineReference.AddNode(ref invoiceLineNode);

            #region AllowanceCharge

            if (AllowanceCharge != null)
            {
                AllowanceCharge.AddNode(ref invoiceLineNode, false);
            }
            #endregion

            #region Item

            XElement itemNode = new XElement(ns_cac + "Item");

            if (!string.IsNullOrEmpty(ItemDescription))
            {
                itemNode.Add(new XElement(ns_cbc + "Description", ItemDescription));
            }

            itemNode.Add(new XElement(ns_cbc + "Name", ItemName));

            if (!string.IsNullOrEmpty(ItemSellersIdentificationID))
            {
                XElement sellersItemIdentificationNode = new XElement(ns_cac + "SellersItemIdentification");
                //Must exist
                sellersItemIdentificationNode.Add(new XElement(ns_cbc + "ID", ItemSellersIdentificationID));
                itemNode.Add(sellersItemIdentificationNode);
            }

            TaxCategory.AddNode(ref itemNode);

            invoiceLineNode.Add(itemNode);

            #endregion

            #region Price

            XElement basePriceNode = new XElement(ns_cac + "Price");

            //Inexchange Tim: Det är helt OK att sätta "157,666666" som antal; det går att ha obegränsat antal decimaler för Antal och Enhetspris; men fr.o.m radbelopp och summering/nedåt måste det vara 2 decimaler.
            //Adding more decimals pga sometimes RowSum/Quantity = > 2 decimaler
            //Must exist
            basePriceNode.Add(new XElement(ns_cbc + "PriceAmount", Math.Round(PriceAmount, 6), GetCurrencyAttribute(CurrencyID)));
            invoiceLineNode.Add(basePriceNode);

            #endregion

            parent.Add(invoiceLineNode);
        }
    }

    public class PeppolClassifiedTaxCategory: PeppolBillingBase
    {
        public string ID { get; set; }
        public decimal Percent { get; set; }

        public void AddNode(ref XElement parent)
        {
            XElement taxNode = new XElement(ns_cac + "ClassifiedTaxCategory");

            taxNode.Add(new XElement(ns_cbc + "ID", ID));
            taxNode.Add(new XElement(ns_cbc + "Percent", Percent));
            taxNode.Add(new XElement(ns_cac + "TaxScheme", new XElement(ns_cbc + "ID", PEPPOL_TAX_SCHEME_ID_VAT)));

            parent.Add(taxNode);
        }
    }

    public class RequisitionistDocumentReference : PeppolBillingBase
    {
        public string ID { get; set; }

        public void AddNode(ref XElement root, XNamespace ns)
        {
            XElement requisitionistDocumentReferenceNode = new XElement(ns + "RequisitionistDocumentReference");
            
            //Must exist                    
            requisitionistDocumentReferenceNode.Add(new XElement(ns_cac + "ID", ID));

            root.Add(requisitionistDocumentReferenceNode);
        }
    }

    public class AdditionalDocumentReference : PeppolBillingBase
    {
        public String ID { get; set; }
        public string IdentificationSchemeID { get; set; } = "ATS"; 

        private const string identificationSchemeAgencyName = "SFTI";

        public void AddNode(ref XElement root, XNamespace ns)
        {
            XElement additionalDocumentReferenceNode = new XElement(ns + "AdditionalDocumentReference");
            
            additionalDocumentReferenceNode.Add(new XElement(ns_cac + "ID", ID, new XAttribute("identificationSchemeAgencyName", identificationSchemeAgencyName), new XAttribute("identificationSchemeID", IdentificationSchemeID)));

            root.Add(additionalDocumentReferenceNode);
        }
    }

    public class InitialInvoiceDocumentReference : PeppolBillingBase
    {
        public String ID { get; set; }

        public void AddNode(ref XElement root, XNamespace ns)
        {
            XElement initialInvoiceDocumentReferenceNode = new XElement(ns + "InitialInvoiceDocumentReference");

            //Must exist                    
            initialInvoiceDocumentReferenceNode.Add(new XElement(ns_cac + "ID", ID));

            root.Add(initialInvoiceDocumentReferenceNode);
        }
    }

    public class OrderLineReference : PeppolBillingBase
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

    
    public class PeppolAllowanceCharge : PeppolBillingBase
    {
        public bool ChargeIndicator { get; set; }
        public string AllowanceChargeReasonCode { get; set; }
        public string AllowanceChargeReason { get; set; }
        public decimal MultiplierFactorNumeric { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyID { get; set; }

        public string TaxCategoryID { get; set; }
        public decimal TaxCategoryPercent { get; set; }
        public string TaxCategoryTaxSchemeID { get; set; }

        public void AddNode(ref XElement root, bool addTaxCategory)
        {
            XElement allowanceChargeNode = new XElement(ns_cac + "AllowanceCharge");
            allowanceChargeNode.Add(new XElement(ns_cbc + "ChargeIndicator", ChargeIndicator.ToString().ToLower()));

            if (!string.IsNullOrEmpty(AllowanceChargeReasonCode))
                allowanceChargeNode.Add(new XElement(ns_cbc + "AllowanceChargeReasonCode", AllowanceChargeReasonCode));

            if (!string.IsNullOrEmpty(AllowanceChargeReason))
                allowanceChargeNode.Add(new XElement(ns_cbc + "AllowanceChargeReason", AllowanceChargeReason));

            if (MultiplierFactorNumeric != 0)
                allowanceChargeNode.Add(new XElement(ns_cbc + "MultiplierFactorNumeric", MultiplierFactorNumeric));

            allowanceChargeNode.Add(new XElement(ns_cbc + "Amount", Amount, GetCurrencyAttribute(CurrencyID)));

            #region TaxCategory

            if (addTaxCategory)
            {
                XElement taxCategoryNode = new XElement(ns_cac + "TaxCategory");

                taxCategoryNode.Add(new XElement(ns_cbc + "ID", TaxCategoryID));
                taxCategoryNode.Add(new XElement(ns_cbc + "Percent", TaxCategoryPercent));

                XElement taxSchemeNode = new XElement(ns_cac + "TaxScheme");

                taxSchemeNode.Add(new XElement(ns_cbc + "ID", TaxCategoryTaxSchemeID));

                taxCategoryNode.Add(taxSchemeNode);

                allowanceChargeNode.Add(taxCategoryNode);
            }

            #endregion


            root.Add(allowanceChargeNode);
        }
    }

    public class PeppolBillingBase
    {
        #region Namespaces

        public static readonly XNamespace ns_invoice = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        public static readonly XNamespace ns_credit = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
        public static readonly XNamespace ns_cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        public static readonly XNamespace ns_cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        //public XNamespace ns_ccts = "urn:oasis:names:tc:ubl:CoreComponentParameters:1:0";
        //public XNamespace ns_cur = "urn:oasis:names:tc:ubl:codelist:CurrencyCode:1:0";
        //public XNamespace ns_sdt = "urn:oasis:names:tc:ubl:SpecializedDatatypes:1:0";
        //public XNamespace ns_udt = "urn:oasis:names:tc:ubl:UnspecializedDatatypes:1:0";
        //public XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public const int PEPPOL_INVOICE_TYPE_CODE_DEBIT = 380;
        public const int PEPPOL_INVOICE_TYPE_CODE_CREDIT = 381;
        public const String PEPPOL_FINANICIAL_INSTITUTION_ID_BG = "SE:BANKGIRO";
        public const String PEPPOL_FINANICIAL_INSTITUTION_ID_PG = "SE:PLUSGIRO";

        public const String PEPPOL_TAX_SCHEME_ID_VAT = "VAT";
        public const String PEPPOL_TAX_SCHEME_ID_SWT = "SWT";
        public const String PEPPOL_TAX_SCHEME_ID_TXT = "TXT";

        public const string PEPPOL_PARTY_CUSTOMER_TAG_NAME = "AccountingCustomerParty";
        public const string PEPPOL_PARTY_SUPPLIER_TAG_NAME = "AccountingSupplierParty";

        public const string PEPPOL_PARTY_SCHEME_ID_SWEDISH_ORGNR = "0007";
        public const string PEPPOL_PARTY_SCHEME_ID_ADDRESS_GLN = "0088";
        public const string PEPPOL_PARTY_SCHEME_ID_FINNISH_ORGNR = "0212";

        public const string PEPPOL_PAYMENT_MEAN_CODE_CREDIT_TRANSFER = "30";

        public const string PEPPOL_CHARGE_REASON_CODE_FREIGHT_SERVICE = "FC";
        public const string PEPPOL_CHARGE_REASON_CODE_INVOICING = "IS";

        public const string PEPPOL_ALLOWANCE_REASON_CODE_DISCOUNT = "95";

        #endregion

        public string RemoveIllegalCharacters(string input)
        {
            input = input.Replace(" ", "");
            input = input.Replace("-", "");
            return input;
        }

        public string ToStringDateWithTime(DateTime? date)
        {
            return date.HasValue ? ToPeppolDate(date) + "T" + date.Value.ToString("HH:mm:ss") : string.Empty;

        }

        public static string ToPeppolDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : string.Empty;

        }

        public static XAttribute GetCurrencyAttribute(string currency)
        {
            return new XAttribute("currencyID", currency);
        }

        public static XElement GetSwedishOrgNrElement(string elementName, string orgNr)
        {
            return new XElement(ns_cbc+elementName, new XAttribute("schemeID", PEPPOL_PARTY_SCHEME_ID_SWEDISH_ORGNR), orgNr);
        }

        public static XElement GetGlnNrElement(string elementName, string glnNr)
        {
            return new XElement(ns_cbc+elementName, new XAttribute("schemeID", PEPPOL_PARTY_SCHEME_ID_ADDRESS_GLN), glnNr);
        }

    }
}
