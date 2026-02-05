using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    internal class PaymentInformationNode : SEPANode<SEPAModel>
    {
        private readonly IEnumerable<PaymentRow> paymentRows;
        internal CreditTransferTransactionInformationNode CttiNode;
        private readonly PaymentMethod paymentMethod;
        internal int nbrOfCttiNodes;


        internal PaymentInformationNode(SEPAModel parent, PaymentRow paymentRow, int sysCountryId, IEnumerable<PaymentRow> paymentRows, PaymentMethod paymentMethod) : base(parent)
        {
            this.paymentMethod = paymentMethod;
            this.paymentRows = paymentRows;
            this.CttiNode = new CreditTransferTransactionInformationNode(this, paymentRow, sysCountryId);
        }

        internal List<XElement> CreateNodeV2()
        {
            List<XElement> listToreturn = new List<XElement>();            

            //own Payment Information Node for each paydate
            List<DateTime> payDates = paymentRows.Select(n => n.PayDate.Date).Distinct().ToList();
            payDates = payDates.OrderByDescending(a => a.Date.Date).ToList();
            foreach (DateTime payDate in payDates)
            {                
                List<PaymentRow> paymentsForTheDate = paymentRows.Where(n => n.PayDate.Date == payDate.Date).ToList();

                Element pmtInfElement = new Element("PmtInf",
                    new Element("PmtInfId", paymentsForTheDate[0].PaymentRowId.ToString().Truncate(35)),
                    this.CreatePaymentMethodNode(),
                    this.CreatePaymentTypeElement(),
                    new Element("ReqdExctnDt", payDate.ToString(SEPABase.ISODateFormat)),
                    this.Debtor.CreateNodeBank(),
                    this.CreateDebtorAccountElement(),
                    this.CreateDebtrAgentElement(),
                    new Element("ChrgBr", "SLEV")
                    );

                //handle one supplier's payments at a time
                List <String> paymentNumbers = paymentsForTheDate.Select(n => n.PaymentNr).Distinct().ToList();
                foreach (string paymentNumber in paymentNumbers)
                {
                    List<PaymentRow> paymentsForTheSupplier = paymentsForTheDate.Where(n => n.PaymentNr == paymentNumber).ToList();

                    pmtInfElement.Add(this.CttiNode.CreateNodeV2(paymentsForTheSupplier));                                       
                }

                listToreturn.Add(pmtInfElement);                
            }

            this.nbrOfCttiNodes = this.CttiNode.nbrOfCttiNodes;
            return listToreturn;
        }

        internal Company Company
        {
            get
            {
                return this.Parent.Company;
            }
        }

        private ParticipantNode Debtor
        {
            get
            {
                return this.Parent.GroupHeader.InitiatingParty;
            }
        }

        private object CreateDebtrAgentElement()
        {
            if (this.paymentMethod != null && !this.paymentMethod.PaymentInformationRowReference.IsLoaded)
                this.paymentMethod.PaymentInformationRowReference.Load();

            return new Element("DbtrAgt", this.TryCreateBICElement(this.Company.ActorCompanyId, "Company: " + this.Company.Name, this.paymentMethod != null ? this.paymentMethod.PaymentInformationRow : null));
        }

        private Element CreateDebtorAccountElement()
        {
            if (this.paymentMethod != null && !this.paymentMethod.PaymentInformationRowReference.IsLoaded)
                this.paymentMethod.PaymentInformationRowReference.Load();

            return new Element("DbtrAcct",
                new Element("Id", this.TryCreateIBANElement(this.Company.ActorCompanyId, "Company: " + this.Company.Name, this.paymentMethod != null ? this.paymentMethod.SysPaymentMethodId : 0, this.paymentMethod != null ? this.paymentMethod.PaymentInformationRow : null))
                );
        }

        /// <summary>
        /// Betalningsmetod hårdkodad till TRF = endast Credit transfers
        /// </summary>
        /// <returns></returns>
        private Element CreatePaymentMethodNode()
        {
            return new Element("PmtMtd", "TRF");
        }
    }
}
