using SoftOne.Soe.Business.Util.Svefaktura.Schema.BusinessDocument;
using SoftOne.Soe.Business.Util.Svefaktura.Schema.Envelope;
using SoftOne.Soe.Business.Util.Svefaktura.Schema.Invoice;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SoftOne.Soe.Business.Util.API.Models
{
    class InExchangeApiIncomingDocumentResponse
    {
        public List<InExchangeApiIncomingDocument> documents { get; set; }
        public int Total { get; set; }
    }

    public class InExchangeApiIncomingDocument
    {
        private const int SVEFAK_INVOICE_TYPE_CODE_DEBIT = 380;
        private const int SVEFAK_INVOICE_TYPE_CODE_CREDIT = 381;
        private const string SVEFAK_FINANICIAL_INSTITUTION_ID_BG = "BGABSESS";
        private const string SVEFAK_FINANICIAL_INSTITUTION_ID_PG = "PGSISESS";
        private const string SVEFAK_FINANICIAL_INSTITUTION_ID_IBAN = "SWEDSESS";

        public string id { get; set; }
        public string documentType { get; set; }
        public string documentFormat { get; set; }
        public string infoUrl { get; set; }
        public string downloadUrl { get; set; }
        public int sequenceNo { get; set; }
        public MemoryStream filedata { get; set; }
        public bool parseRows { get; set; }

        public SupplierInvoiceIORawData CreateSupplierInvoiceIO(bool parseProductRows)
        {
            this.parseRows = parseProductRows;

            var businessDocumentSerializer = new XmlSerializer(typeof(StandardBusinessDocument));
            var input = (StandardBusinessDocument)businessDocumentSerializer.Deserialize(filedata);
            var attachments = new Dictionary<string, byte[]>();
            SFTIInvoiceType invoice = null;
            foreach (var item in input.Any)
            {
                if (item.Name == "Invoice")
                {
                    var invoiceSerializer = new XmlSerializer(typeof(SFTIInvoiceType));
                    using (TextReader sr = new StringReader(item.OuterXml))
                    {
                        invoice = (SFTIInvoiceType)invoiceSerializer.Deserialize(sr);
#if DEBUG
                        //File.WriteAllText(@"C:\Temp\Inexchange\incoming\svefaktura.xml", item.OuterXml);
#endif
                    }
                }
                else if (item.Name == "ObjectEnvelope")
                {
                    var envelopeSerializer = new XmlSerializer(typeof(ObjectEnvelopeType));
                    using (TextReader sr = new StringReader(item.OuterXml))
                    {
                        var envelope = (ObjectEnvelopeType)envelopeSerializer.Deserialize(sr);
                        var invoiceImageWithAttachments = envelope.EncodedObject.FirstOrDefault(file => file.EncodedData.filename == "InvoiceImage.pdf");
                        if (invoiceImageWithAttachments != null)
                        {
                            attachments.Add(invoiceImageWithAttachments.EncodedData.filename, invoiceImageWithAttachments.EncodedData.Value);
#if DEBUG
                            //File.WriteAllBytes(@"C:\Temp\Inexchange\incoming\" + invoiceImageWithAttachments.EncodedData.filename, invoiceImageWithAttachments.EncodedData.Value);
#endif
                        }
                        else
                        {
                            bool firstFileAsImage = true;
                            foreach (var file in envelope.EncodedObject)
                            {
                                var fileName = file.EncodedData.filename;
                                var data = file.EncodedData.Value;

                                if (firstFileAsImage)
                                {
                                    firstFileAsImage = false;
                                    var fileExt = Path.GetExtension(file.EncodedData.filename);
                                    var allowedExtensions = new List<string> { ".pdf", ".tif", ".tiff", ".jpg", ".jpeg", ".png" };

                                    if (allowedExtensions.Contains(fileExt))
                                    {
                                        //Convert 2 pdf?
                                        if (fileExt == ".tif" || fileExt == ".tiff")
                                        {
                                            //try create a pdf
                                            var pdfdata = PDFUtility.CreatePdfFromTifInMemory(data);
                                            if (pdfdata != null)
                                            {
                                                //Set original as attachment
                                                attachments.Add(fileName, data);
                                                data = pdfdata;
                                                fileExt = ".pdf";
                                            }
                                        }
                                        
                                        fileName = "InvoiceImage" + fileExt;
                                    }
                                }
                                
                                if (attachments.ContainsKey(fileName))
                                    attachments.Add(Path.GetFileNameWithoutExtension(fileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(fileName), data);
                                else
                                    attachments.Add(fileName, data);
                                
#if DEBUG
                                File.WriteAllBytes(@"C:\Temp\Inexchange\incoming\" + fileName, data);
#endif
                            }
                        }
                    }
                }
            }

            var supplierInvoice = CreateSupplierInvoiceDTO(invoice);
            if (attachments.Any())
            {
                supplierInvoice.Attachements = attachments;
            }

            return supplierInvoice;
        }

        private SupplierInvoiceIORawData CreateSupplierInvoiceDTO(SFTIInvoiceType svefaktura)
        {
            bool isCredit = int.Parse(svefaktura.InvoiceTypeCode.Value) == SVEFAK_INVOICE_TYPE_CODE_CREDIT;
            var supplierInvoice = new SupplierInvoiceIORawData
            {
                SupplierInvoiceNr = svefaktura.ID.Value,
                BillingType = isCredit ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                InvoiceDate = svefaktura.IssueDate?.Value,
                OriginStatus = (int)SoeOriginStatus.Draft,
                Note = svefaktura.Note?.Value,
                PaymentNumbers = new Dictionary<int, string>()
            };

            var paymentMean = svefaktura.PaymentMeans?.FirstOrDefault();
            if (paymentMean != null)
            {
                supplierInvoice.DueDate = paymentMean.DuePaymentDate?.Value;
                supplierInvoice.PaymentNr = paymentMean.PayeeFinancialAccount?.ID.Value;
                supplierInvoice.SysPaymentType = GetSysPaymentType(paymentMean.PayeeFinancialAccount?.FinancialInstitutionBranch?.FinancialInstitution?.ID?.Value);
                supplierInvoice.OCR = paymentMean.PayeeFinancialAccount?.PaymentInstructionID?.Value;
                
                foreach (var paymentMean2 in svefaktura.PaymentMeans)
                {
                    var sysPaymentType = (int)GetSysPaymentType(paymentMean2.PayeeFinancialAccount?.FinancialInstitutionBranch?.FinancialInstitution?.ID?.Value);
                    var paymentNumber = paymentMean2.PayeeFinancialAccount?.ID?.Value;
                    if (!string.IsNullOrEmpty(paymentNumber) && !supplierInvoice.PaymentNumbers.ContainsKey(sysPaymentType))
                    {
                        supplierInvoice.PaymentNumbers.Add(sysPaymentType, paymentNumber);
                    }
                }
            }

            if (!string.IsNullOrEmpty(supplierInvoice.SupplierInvoiceNr))
            {
                supplierInvoice.SupplierInvoiceNr = supplierInvoice.SupplierInvoiceNr.Replace(" ", "");
            }

            if (!string.IsNullOrEmpty(supplierInvoice.OCR))
            {
                supplierInvoice.OCR = supplierInvoice.OCR.Replace(" ", "");
            }

            foreach (var sellerTaxScheme in svefaktura.SellerParty.Party.PartyTaxScheme)
            {
                if (sellerTaxScheme.TaxScheme.ID.Value == "SWT")
                {
                    supplierInvoice.SupplierOrgNr = sellerTaxScheme.CompanyID.Value;
                }
                else if (sellerTaxScheme.TaxScheme.ID.Value == "VAT")
                {
                    supplierInvoice.SupplierVatNr = sellerTaxScheme.CompanyID.Value;
                }
            }

            supplierInvoice.TotalAmount = supplierInvoice.TotalAmountCurrency = svefaktura.LegalTotal.TaxInclusiveTotalAmount.Value;
            supplierInvoice.Currency = svefaktura.LegalTotal.TaxInclusiveTotalAmount.amountCurrencyID;
            
            foreach (var taxTotal in svefaktura.TaxTotal)
            {
                supplierInvoice.VATAmount = supplierInvoice.VATAmountCurrency = taxTotal.TotalTaxAmount.Value;
                if (taxTotal.TaxSubTotal != null)
                {
                    var taxSubTotal = taxTotal.TaxSubTotal.FirstOrDefault(x => x.TaxCategory.TaxScheme.ID.Value == "VAT");
                    if (taxSubTotal != null)
                    {
                        supplierInvoice.VatPercent = taxSubTotal.TaxCategory.Percent.Value;
                    }
                }
            }

            //Always parses rows. Should be conditional.
            if (this.parseRows)
            {
                supplierInvoice.ProductRows = new List<SupplierInvoiceProductRowIORawData>();
                foreach (var invoiceLine in svefaktura.InvoiceLine)
                {
                    var productRow = new SupplierInvoiceProductRowIORawData();
                    //If we want to add text rows.
                    //productRow.Text = invoiceLine.Note?.Value;
                    productRow.Quantity = invoiceLine.InvoicedQuantity?.Value ?? 0;
                    productRow.UnitCode = invoiceLine.InvoicedQuantity?.quantityUnitCode;
                    productRow.AmountCurrency = invoiceLine.LineExtensionAmount?.Value ?? 0;
                    if (invoiceLine.Item != null)
                    {
                        productRow.Text = invoiceLine.Item.Description?.Value;
                        //Priority 1: SellerProductNumber
                        productRow.SellerProductNumber = invoiceLine.Item.SellersItemIdentification?.ID?.Value;
                        if (string.IsNullOrEmpty(productRow.SellerProductNumber))
                        {
                            //Priority 2: BuyerProductNumber
                            productRow.SellerProductNumber = invoiceLine.Item.BuyersItemIdentification?.ID?.Value;
                        }
                        if (string.IsNullOrEmpty(productRow.SellerProductNumber))
                        {
                            //Priority 3: StandardProductNumber?
                            productRow.SellerProductNumber = invoiceLine.Item.StandardItemIdentification?.ID?.Value;
                        }

                        productRow.PriceCurrency = invoiceLine.Item.BasePrice?.PriceAmount?.Value ?? 0;
                        productRow.VatRate = invoiceLine.Item.TaxCategory?.FirstOrDefault()?.Percent?.Value ?? 0;
                    }
                    productRow.VatAmountCurrency = productRow.AmountCurrency * ( productRow.VatRate / 100 );

                    //Type of row
                    productRow.SupplierInvoiceRowType = string.IsNullOrEmpty(productRow.SellerProductNumber) && productRow.AmountCurrency == 0 && productRow.Quantity == 0 ? SupplierInvoiceRowType.TextRow : SupplierInvoiceRowType.ProductRow;

                    if (productRow.SupplierInvoiceRowType == SupplierInvoiceRowType.TextRow && string.IsNullOrEmpty(productRow.Text))
                    {
                        continue;
                    }

                    if (isCredit && productRow.AmountCurrency == 0 && productRow.Quantity == 0)
                    {
                        //Attempt to store credit rows in a unison manner.
                        if (productRow.AmountCurrency < 0)
                        {
                            productRow.SetDebitProperties();
                        }
                        else
                        {
                            productRow.SetCreditProperties();
                        }
                    }

                  supplierInvoice.ProductRows.Add(productRow);
                }
            }

            //We can not handle invoice for muliple orders....
            try
            {
                var orderLineReference = svefaktura.InvoiceLine.Where(y => y.OrderLineReference != null && y.OrderLineReference.OrderReference != null).Select(z => z.OrderLineReference.OrderReference.BuyersID.Value).Distinct().ToList();
                if (orderLineReference.Count == 1)
                {
                    supplierInvoice.OrderNr = orderLineReference.First();
                }

                foreach (var reference in svefaktura.RequisitionistDocumentReference)
                {
                    supplierInvoice.ReferenceYour += reference.ID.Value + " ";
                }
            }
            catch
            {
                // Do nothing
                // NOSONAR
            }

            return supplierInvoice;
        }

        private TermGroup_SysPaymentType GetSysPaymentType(string svefaktFinancialInstitution)
        {
            switch (svefaktFinancialInstitution)
            {
                case SVEFAK_FINANICIAL_INSTITUTION_ID_BG:
                    return TermGroup_SysPaymentType.BG;
                case SVEFAK_FINANICIAL_INSTITUTION_ID_PG:
                    return TermGroup_SysPaymentType.PG;
                case SVEFAK_FINANICIAL_INSTITUTION_ID_IBAN:
                    return TermGroup_SysPaymentType.BIC;
                default:
                    return TermGroup_SysPaymentType.Unknown;
            }
        }
        
    }
}
