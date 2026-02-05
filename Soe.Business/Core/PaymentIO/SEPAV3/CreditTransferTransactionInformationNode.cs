using System;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SoftOne.Soe.Util.Exceptions;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    internal class CreditTransferTransactionInformationNode : SEPANode<PaymentInformationNode>
    {
        
        internal int nbrOfCttiNodes = 0;
        private readonly PaymentMethod paymentMethod;
        private readonly int CompanySysCountryId;

        internal CreditTransferTransactionInformationNode(PaymentInformationNode parent, int companySysCountryId, PaymentMethod paymentMethod) : base(parent)
        {
            this.paymentMethod = paymentMethod;
            this.CompanySysCountryId = companySysCountryId;
        }

        internal List<Element> CreateNode(List<PaymentRow> paymentRows, string debtorAgentBIC)
        {
            List<Element> listToReturn = new List<Element>();
            int creditInvoiceCount = paymentRows.Count(i => i.Amount < 0);

            if (creditInvoiceCount == 0)
            {
                foreach (PaymentRow row in paymentRows)
                {
                    var invoice = row.InvoiceLazy();
                    var supplierActor = invoice.ActorLazy();
                    var supplier = supplierActor.SupplierLazy();
                    var creditor = CreateCreditor(supplier, this.CompanySysCountryId);
                    var currency = invoice.CurrencyLazy();

                    var isForeginPayment = supplier.SysCountryId.GetValueOrDefault() > 0 && (supplier.SysCountryId != this.CompanySysCountryId);
                    if (isForeginPayment && !creditor.PostalAddress.HasAddressLines())
                    {
                        throw new ActionFailedException(this.Parent.paymentManager.GetText(7599, (int)TermGroup.General, "För utlandsbetalningar krävs att mottagarens utdelningsadress anges") + $":\n {invoice.InvoiceNr}:{invoice.Actor?.Supplier?.Name}");
                    }

                    Element ctti = new Element("CdtTrfTxInf",
                        this.CreatePaymentIdElement(row, 1),
                        this.CreateAmountElement(row.AmountCurrency, currency));

                    var CdtrAgt = creditor.CreateCreditorAgentElement(row, debtorAgentBIC);
                    if (CdtrAgt != null)
                    {
                        ctti.Add(CdtrAgt);
                    }

                    if (string.IsNullOrEmpty(row.PaymentNr))
                    {
                        throw new ActionFailedException(this.Parent.paymentManager.GetText(7600, (int)TermGroup.General, "Betalkonto saknas") + $":\n {invoice.InvoiceNr}:{invoice.Actor?.Supplier?.Name}");
                    }

                    ctti.Add(creditor.CreateNodeCdtr(), this.CreateCreditorAccountElement(row) );

                    if (isForeginPayment)
                    {
                        ctti.Add(this.CreateRegulatoryReportingElement(supplier,invoice, row));
                    }

                    ctti.Add(this.CreateRemittanceInformationElement(row, currency, debtorAgentBIC));

                    listToReturn.Add(ctti);
                    nbrOfCttiNodes++;
                }
            }
            else
            {
                var row = paymentRows[0];
                var invoice = row.InvoiceLazy();
                var currency = invoice.CurrencyLazy();

                var supplierActor  = invoice.ActorLazy();
                var supplier = supplierActor.SupplierLazy();
                var creditor = CreateCreditor(supplier, this.CompanySysCountryId);

                var isForeginPayment = supplier.SysCountryId.GetValueOrDefault() > 0 && (supplier.SysCountryId != this.CompanySysCountryId);
                if (isForeginPayment && !creditor.PostalAddress.HasAddressLines())
                {
                    throw new ActionFailedException(this.Parent.paymentManager.GetText(7599, (int)TermGroup.General, "För utlandsbetalningar krävs att mottagarens utdelningsadress anges") + $":\n {invoice.InvoiceNr}:{invoice.Actor?.Supplier?.Name}");
                }

                Element ctti = new Element("CdtTrfTxInf",
                this.CreatePaymentIdElement(row, paymentRows.Count),
                this.CreateAmountElement(paymentRows.Sum(r => r.AmountCurrency), currency));

                var CdtrAgt = creditor.CreateCreditorAgentElement(row, debtorAgentBIC);
                if (CdtrAgt != null)
                {
                    ctti.Add(CdtrAgt);
                }

                if (string.IsNullOrEmpty(row.PaymentNr))
                {
                    throw new ActionFailedException(this.Parent.paymentManager.GetText(7600, (int)TermGroup.General, "Betalkonto saknas") + $":\n {invoice.InvoiceNr}:{invoice.Actor?.Supplier?.Name}");
                }

                ctti.Add(creditor.CreateNodeCdtr(), this.CreateCreditorAccountElement(row) );

                if (isForeginPayment)
                {
                    ctti.Add(this.CreateRegulatoryReportingElement(supplier,invoice, row));
                }

                Element rmtInfElement = new Element("RmtInf");

                foreach (PaymentRow loopRow in paymentRows)
                {
                    rmtInfElement.Add(this.CreateCreditRemittanceInformationElement(loopRow, currency));
                }

                ctti.Add(rmtInfElement);

                listToReturn.Add(ctti);
                nbrOfCttiNodes++;
            }

            return listToReturn;
        }

        private ParticipantNode CreateCreditor(Supplier supplier, int companySysCountryId)
        {
            var countryId = supplier.SysCountryId.GetValueOrDefault() > 0 ? supplier.SysCountryId.GetValueOrDefault() : companySysCountryId;
            var country = sysCountries.First(x => x.SysCountryId == countryId);
            var paymentInformations = paymentManager.GetPaymentInformationFromActor(entities, supplier.ActorSupplierId, true, false)?.ActivePaymentInformationRows.ToList();
            //var paymentInformationRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.PaymentNr == paymentRow.PaymentNr);
            
            var addressParts = this.GetAddressParts(supplier.ActorSupplierId, TermGroup_SysContactAddressType.Distribution);
            return new ParticipantNode(this, supplier.Name, supplier.OrgNr, country, addressParts, this.paymentMethod, paymentInformations);
        }

        private Element CreateRmtInfOCR(string value, string currencyCode, decimal amount, string debtorAgentBIC)
        {
            var strdElement = new Element("Strd");

            if (IsSwedbank(debtorAgentBIC))
            {
                strdElement.Add(CreateRfrdDockAmt(currencyCode, amount));
            }

            strdElement.Add(
                        new Element("CdtrRefInf",
                            new Element("Tp", new Element("CdOrPrtry", new Element("Cd", "SCOR"))),
                            new Element("Ref", value)));

            return strdElement;
        }

        private Element CreateRmtInfCINV(string value, string currencyCode, decimal amount)
        {
            var strdElement = new Element("Strd");

            strdElement.Add(
                        new Element("RfrdDocInf",
                            new Element("Tp", new Element("CdOrPrtry", new Element("Cd", "CINV"))),
                            new Element("Nb", value)));

            strdElement.Add(CreateRfrdDockAmt(currencyCode, amount));

            return strdElement;
        }

        private Element CreateRfrdDockAmt(string currency, decimal amount)
        {
            return new Element("RfrdDocAmt", new Element("RmtdAmt", new XAttribute("Ccy", currency), FormatAmount(amount)));
        }

        private Element CreateRemittanceInformationElement(PaymentRow row, Currency currency,string debtorAgentBIC)
        {
            var rmtInf = new Element("RmtInf");
            if (!string.IsNullOrEmpty(row.Invoice.OCR) && InvoiceUtility.ValidateSwedishOCRNumber(row.Invoice.OCR) )
            {
                rmtInf.Add(CreateRmtInfOCR(row.Invoice.OCR, this.GetCurrencyCode(currency?.SysCurrencyId), row.AmountCurrency, debtorAgentBIC));
            }
            else if (!string.IsNullOrEmpty(row.Invoice.InvoiceNr) && row.Invoice.InvoiceNr.Length < 25)
            {
                rmtInf.Add(CreateRmtInfCINV(row.Invoice.InvoiceNr, this.GetCurrencyCode(currency?.SysCurrencyId), row.AmountCurrency));
            }
            /*
            else if (!string.IsNullOrEmpty(row.Invoice.ReferenceOur))
            {
                rmtInf.Add(new Element("Ustrd", row.Invoice.InvoiceNr + " " + row.Invoice.ReferenceOur));
            }
            */
            else
            {
                throw new ActionFailedException(this.Parent.paymentManager.GetText(5896, (int)TermGroup.General, "Fakturanummer saknas") + " (" +row.Invoice.SeqNr + ")");
            }

            return rmtInf;
        }

        private Element CreateCreditRemittanceInformationElement(PaymentRow row, Currency currency)
        {
            
            if (string.IsNullOrEmpty(row.Invoice.InvoiceNr))
            {
                throw new ActionFailedException(this.Parent.paymentManager.GetText(5896, (int)TermGroup.General, "Fakturanummer saknas") + " (" + row.Invoice.SeqNr + ")");
            }
            
            Element strdElement = new Element("Strd");

            string docTp = row.AmountCurrency >= 0 ? "CINV" : "CREN";
            strdElement.Add(new Element("RfrdDocInf",
                                    new Element("Tp", new Element("CdOrPrtry", new Element("Cd", docTp))),
                                    new Element("Nb", row.Invoice.InvoiceNr)));

            if (row.AmountCurrency >= 0)
                strdElement.Add(CreateRfrdDockAmt(this.GetCurrencyCode(currency.SysCurrencyId), row.AmountCurrency) );
            else
                strdElement.Add(new Element("RfrdDocAmt", new Element("CdtNoteAmt", new XAttribute("Ccy", this.GetCurrencyCode(currency?.SysCurrencyId)), FormatCreditAmount(row.AmountCurrency))));

            return strdElement;
        }

        private Element CreateAmountElement(decimal amount, Currency currency)
        {
            return new Element("Amt", new Element("InstdAmt", new XAttribute("Ccy", GetCurrencyCode(currency?.SysCurrencyId)), FormatAmount(amount)));
        }

        private Element CreatePaymentIdElement(PaymentRow paymentRow, int nrOfPayments)
        {
            return new Element("PmtId", new Element("EndToEndId", $"{paymentRow.PaymentRowId},{paymentRow.Payment.PaymentId},{nrOfPayments}") );
        }

        private Element CreateRegulatoryReportingElement(Supplier supplier,Invoice invoice, PaymentRow row)
        {
            var paymentCode = this.paymentManager.GetPaymentInformationRowsForActor(entities, supplier.ActorSupplierId, (TermGroup_SysPaymentType)row.SysPaymentTypeId).FirstOrDefault(x => x.PaymentNr == row.PaymentNr)?.PaymentCode;

            if (string.IsNullOrEmpty(paymentCode) && row.Amount > 150000)
            {
                throw new ActionFailedException(this.Parent.paymentManager.GetText(4878, (int)TermGroup.General, "Internationella betalningar över 150 000 kräver att betalningskod är angiven på betalningsuppgifterna för mottagande kontot") + $":\n {invoice.InvoiceNr} - {supplier.Name}");
            }
            else if (!string.IsNullOrEmpty(paymentCode) ){

                var paymenCodeTerm = "";
                if (int.TryParse(paymentCode,out int termId))
                {
                    paymenCodeTerm = this.Parent.paymentManager.GetText(termId, (int)TermGroup.CentralBankCode);
                }

                if (string.IsNullOrEmpty(paymenCodeTerm))
                {
                    throw new ActionFailedException(this.Parent.paymentManager.GetText(7730, (int)TermGroup.General, "Ogiltig betalningskod är angiven på betalningsuppgifterna för mottagande kontot.") + $":\n {paymentCode}: \n {invoice.InvoiceNr} - {supplier.Name}");
                }
                
            }
            else if (string.IsNullOrEmpty(paymentCode))
            {
                paymentCode = "101";
            }
            return new Element("RgltryRptg", new Element("Dtls", new Element("Cd", paymentCode) ) );
        }

        private Element CreateCreditorAccountElement(PaymentRow paymentRow)
        {
            Element element = new Element("CdtrAcct");
            switch (paymentRow.SysPaymentTypeId)
            {
                case (int)TermGroup_SysPaymentType.BG:
                    element.Add(CreateBGAccountElement(paymentRow.PaymentNr)); 
                    break;
                case (int)TermGroup_SysPaymentType.PG:
                case (int)TermGroup_SysPaymentType.Bank:
                    element.Add(new Element("Id", new Element("Othr", new Element("Id", paymentRow.PaymentNr.Replace(" ", "").Replace("-", "")),
                         new Element("SchmeNm", new Element("Cd", "BBAN")))));
                    break;

                case (int)TermGroup_SysPaymentType.BIC:
                    element.Add(this.CreateIBANorBBANElement(paymentRow.PaymentNr));
                    break;
            }
            return element;
        }
    }
}
