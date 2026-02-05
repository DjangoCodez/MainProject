using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    internal class PaymentInformationNode : SEPANode<SEPAModel>
    {
        private readonly IEnumerable<PaymentRow> paymentRows;
        internal CreditTransferTransactionInformationNode CttiNode;
        private readonly PaymentMethod paymentMethod;
        internal int nbrOfCttiNodes;
        private readonly bool ContainsForeignPayments;
        
        internal PaymentInformationNode(SEPAModel parent, int companySysCountryId, IEnumerable<PaymentRow> paymentRows, PaymentMethod paymentMethod, bool containsForeignPayments) : base(parent)
        {
            this.ContainsForeignPayments = containsForeignPayments;
            this.paymentMethod = paymentMethod;
            this.paymentRows = paymentRows;
            this.CttiNode = new CreditTransferTransactionInformationNode(this, companySysCountryId, paymentMethod);
        }

        internal List<XElement> CreateNode()
        {
            var listToreturn = new List<XElement>();
            var debtorAgentBIC = GetBIC(this.paymentMethod.PaymentInformationRow);

            //own Payment Information Node for each paydate
            List<DateTime> payDates = paymentRows.Select(n => n.PayDate.Date).Distinct().OrderByDescending(a => a.Date.Date).ToList();
            foreach (DateTime payDate in payDates)
            {
                List<PaymentRow> paymentsForTheDate = paymentRows.Where(n => n.PayDate.Date == payDate.Date).ToList();

                Element pmtInfElement = new Element("PmtInf",
                    new Element("PmtInfId", paymentsForTheDate[0].PaymentRowId.ToString().Truncate(35)),
                    this.CreatePaymentMethodElement(),
                    new Element("BtchBookg", (this.Parent.exportSettings.AggregatePayments).ToString().ToLower()),
                    this.CreatePaymentTypeElement(),
                    new Element("ReqdExctnDt", payDate.ToString(SEPABase.ISODateFormat)),
                    this.Debtor.CreateNodeDbtr(),
                    this.CreateDebtorAccountElement(),
                    this.CreateDebtorAgentElement()
                );

                //if (this.paymentMethod.PaymentType == (int)TermGroup_SysPaymentType.BIC || this.ContainsForeignPayments)
                if (this.ContainsForeignPayments)
                {
                    pmtInfElement.Add(this.CreateChargeBearerElement());
                }

                //handle one supplier's payments at a time
                List<string> paymentNumbers = paymentsForTheDate.Select(n => n.PaymentNr).Distinct().ToList();
                                
                foreach (string paymentNumber in paymentNumbers)
                {
                    List<PaymentRow> paymentsForTheSupplier = paymentsForTheDate.Where(n => n.PaymentNr == paymentNumber).ToList();

                    pmtInfElement.Add(this.CttiNode.CreateNode(paymentsForTheSupplier, debtorAgentBIC));
                }

                listToreturn.Add(pmtInfElement);
            }

            this.nbrOfCttiNodes = this.CttiNode.nbrOfCttiNodes;
            return listToreturn;
        }

        private ParticipantNode Debtor
        {
            get
            {
                return this.Parent.GroupHeader.DebtorParty;
            }
        }

        private Element CreateDebtorAgentElement()
        {
            return new Element("DbtrAgt",
                new Element("FinInstnId",
                    this.CreateBICElement(this.paymentMethod.PaymentInformationRow),
                    this.CreateDebtorCountryElement()));
        }

        private Element CreateDebtorAccountElement()
        {
            var paymentNr = this.paymentMethod.PaymentInformationRow.PaymentNr;
            string accountCurrency = "";
            if (this.paymentMethod.PaymentInformationRow.CurrencyId.GetValueOrDefault() > 0)
            {
                var sysCurrencyId = countryCurrencyManager.GetSysCurrencyId(this.entities, this.paymentMethod.PaymentInformationRow.CurrencyId.Value);
                accountCurrency = countryCurrencyManager.GetCurrencyCode(sysCurrencyId);
            }
            
            Element accountElement;
            switch (this.paymentMethod.PaymentInformationRow.SysPaymentTypeId)
            {
                case (int)TermGroup_SysPaymentType.BG:
                    accountElement = CreateBGAccountElement(paymentNr);
                    break;
                default:
                    accountElement = this.CreateIBANorBBANElement(paymentNr);
                    break;
            }
            return new Element("DbtrAcct", accountElement, CreateDebtorCcy(accountCurrency) );
        }

        private Element CreateChargeBearerElement()
        {
            return new Element("ChrgBr", "SHAR");
        }

        private Element CreateDebtorCcy(string accountCurrency)
        {
            if (string.IsNullOrEmpty(accountCurrency))
            {
                var baseCurrency = this.Parent.countryCurrencyManager.GetCompanyBaseCurrencyDTO(this.Parent.entities, this.Parent.Company.ActorCompanyId);
                if (baseCurrency == null)
                {
                    throw new ActionFailedException("Missing base currency for company");
                }
                accountCurrency = baseCurrency.Code;
            }
            return new Element("Ccy", accountCurrency);
        }

        private Element CreateDebtorCountryElement()
        {
            return new Element("PstlAdr", new Element("Ctry", this.GetCountryCode(this.Parent.Company.SysCountryId.HasValue ? (int)this.Parent.Company.SysCountryId : 0)));
        }

        private Element CreatePaymentMethodElement()
        {
            return new Element("PmtMtd", "TRF");
        }
    }
}
