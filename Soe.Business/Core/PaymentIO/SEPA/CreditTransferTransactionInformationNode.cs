using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    internal class CreditTransferTransactionInformationNode : SEPANode<PaymentInformationNode>
    {
        private ParticipantNode creditor;
        private PaymentRow paymentRow;
        private Invoice invoice;
        private Actor supplierActor;
        private Supplier supplier;
        internal SysCountry Country;
        internal int nbrOfCttiNodes = 0;

        internal CreditTransferTransactionInformationNode(PaymentInformationNode parent, PaymentRow paymentRow, int sysCountryId) : base(parent)
        {
            this.paymentRow = paymentRow;
            //Check if there is credit row/rows with same actor and date
            this.invoice = paymentRow.InvoiceLazy();
            this.supplierActor = invoice.ActorLazy();
            this.supplier = supplierActor.SupplierLazy();
            this.Country = sysCountries.First(x => x.SysCountryId == (supplier.SysCountryId != null ? supplier.SysCountryId : sysCountryId));
            var paymentInformations = paymentManager.GetPaymentInformationFromActor(entities, supplier.ActorSupplierId, true, false)?.ActivePaymentInformationRows.ToList();
            var addressParts = this.GetAddressParts(supplierActor.ActorId, TermGroup_SysContactAddressType.Distribution);
            this.creditor = new ParticipantNode(this, supplier.Name, Country, supplier.OrgNr, addressParts, paymentInformations);
        }

        internal List<Element> CreateNodeV2(List<PaymentRow> paymentRows)
        {
            List<Element> listToreturn = new List<Element>();

            int creditInvoiceCount = paymentRows.Count(i => i.Amount < 0);

            if (creditInvoiceCount == 0)
            {
                foreach (PaymentRow row in paymentRows)
                {
                    this.paymentRow = row;
                    this.invoice = paymentRow.InvoiceLazy();
                    this.supplierActor = invoice.ActorLazy();
                    this.supplier = supplierActor.SupplierLazy();
                    var paymentInformations = paymentManager.GetPaymentInformationFromActor(entities, supplier.ActorSupplierId, true, false)?.ActivePaymentInformationRows.ToList();
                    var addressParts = this.GetAddressParts(supplierActor.ActorId, TermGroup_SysContactAddressType.Distribution);
                    this.creditor = new ParticipantNode(this, supplier.Name, sysCountries.FirstOrDefault(c => c.SysCountryId == supplier.SysCountryId) ?? this.Country, supplier.OrgNr, addressParts, paymentInformations);

                    Element ctti = new Element("CdtTrfTxInf",
                    this.CreatePaymentIdElement(),
                    this.CreatePaymentTypeElement(),
                    this.CreateAmountElementV2(this.paymentRow.AmountCurrency)
                    );

                    var cdtrAgt = this.creditor.CreateCreditorAgentElement(this.paymentRow);
                    if (cdtrAgt != null)
                    {
                        ctti.Add(cdtrAgt);
                    }
                    ctti.Add(this.creditor.CreateNodeCdtr());
                    ctti.Add(this.CreateCreditorAccountElement());
                    ctti.Add(this.CreateRemittanceInformationElementV2());

                    listToreturn.Add(ctti);
                    nbrOfCttiNodes++;
                }
            }
            else
            {
                this.paymentRow = paymentRows[0];
                this.invoice = paymentRow.InvoiceLazy();
                this.supplierActor = invoice.ActorLazy();
                this.supplier = supplierActor.SupplierLazy();
                var addressParts = this.GetAddressParts(supplierActor.ActorId, TermGroup_SysContactAddressType.Distribution);
                var paymentInformations = paymentManager.GetPaymentInformationFromActor(entities, supplier.ActorSupplierId, true, false)?.ActivePaymentInformationRows.ToList();
                this.creditor = new ParticipantNode(this, supplier.Name, sysCountries.FirstOrDefault(c => c.SysCountryId == supplier.SysCountryId) ?? this.Country, supplier.OrgNr, addressParts, paymentInformations);

                Element ctti = new Element("CdtTrfTxInf",
                this.CreatePaymentIdElement(),
                this.CreatePaymentTypeElement(),
                this.CreateAmountElementV2(paymentRows.Sum(r => r.Amount))
                );

                var cdtrAgt = this.creditor.CreateCreditorAgentElement(this.paymentRow);
                if (cdtrAgt != null)
                {
                    ctti.Add(cdtrAgt);
                }
                ctti.Add(this.creditor.CreateNodeCdtr());
                ctti.Add(this.CreateCreditorAccountElement());

                Element rmtInfElement = new Element("RmtInf");
                rmtInfElement.Add(new Element("Ustrd", "Nipputiedot"));

                foreach (PaymentRow row in paymentRows)
                {
                    rmtInfElement.Add(this.CreateCreditRemittanceInformationElementV2(row));
                }

                ctti.Add(rmtInfElement);

                listToreturn.Add(ctti);
                nbrOfCttiNodes++;
            }

            return listToreturn;

        }

        private object CreateRemittanceInformationElementV2()
        {
            if (paymentRow.Invoice.OCR.HasValue() && paymentRow.Invoice.OCR.TrimStart('0').HasValue())
            {                
                return new Element("RmtInf",
                    new Element("Strd",
                        new Element("CdtrRefInf",
                            new Element("CdtrRefTp", new Element("Cd", "SCOR")),
                            new Element("CdtrRef", paymentRow.Invoice.OCR)))
                );               
            }
            else if (paymentRow.Invoice.ReferenceOur.HasValue())
            {
                return new Element("RmtInf", new Element("Ustrd", paymentRow.Invoice.InvoiceNr.ToString() + " " + paymentRow.Invoice.ReferenceOur));
            }
            else //If no refence found insert invoicenr
            {
                return new Element("RmtInf", new Element("Ustrd", paymentRow.Invoice.InvoiceNr.ToString()));
            }
        }

        private object CreateCreditRemittanceInformationElementV2(PaymentRow row)
        {
            Element strdElement = new Element("Strd");

            string docTp = row.Amount >= 0 ? "CINV" : "CREN";
            strdElement.Add(new Element("RfrdDocInf",
                new Element("RfrdDocTp",
                    new Element("Cd", docTp))));

            if (row.Amount >= 0)
                strdElement.Add(new Element("RfrdDocAmt", new Element("RmtdAmt", new XAttribute("Ccy", this.GetCode(this.invoice.CurrencyLazy())), FormatAmount(row.Amount))));
            else
                strdElement.Add(new Element("RfrdDocAmt", new Element("CdtNoteAmt", new XAttribute("Ccy", this.GetCode(this.invoice.CurrencyLazy())), FormatCreditAmount(row.Amount))));

            if (row.Invoice.OCR.HasValue())
            {
                strdElement.Add(new Element("CdtrRefInf",
                                        new Element("CdtrRefTp", new Element("Cd", "SCOR")),
                                        new Element("CdtrRef", row.Invoice.OCR)));
            }
            else
            {
                strdElement.Add(new Element("AddtlRmtInf", string.IsNullOrEmpty(row.Invoice.ReferenceOur.Trim()) ? row.Invoice.InvoiceNr : row.Invoice.ReferenceOur));
            }

            return strdElement;
        }

        private object CreateAmountElementV2(decimal amount)
        {
            return new Element("Amt", new Element("InstdAmt", new XAttribute("Ccy", this.GetCode(this.invoice.CurrencyLazy())), FormatAmount(amount)));
        }

        private Element CreatePaymentIdElement()
        {
            return new Element("PmtId",
                new Element("EndToEndId", this.paymentRow.PaymentRowId.ToString())
                );
        }

        private object CreateCreditorAccountElement()
        {
            return new Element("CdtrAcct",
                new Element("Id", this.TryCreateIBANElementFromPaymentNr(this.invoice.PaymentNr, "Supplier: " + this.supplier.Name))
                //this.CreateCurrencyElement()
                );
        }
    }
}
