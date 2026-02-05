using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    internal class GroupHeaderNode : SEPANode<SEPAModel>
    {
        internal ParticipantNode InitiatingParty { get; private set; }
		private readonly decimal totalAmount;
		private readonly int noTransactions;
        private readonly PaymentMethod paymentMethod;       

        internal GroupHeaderNode(SEPAModel parent, decimal totalAmount, int noTransactions, SysCountry country, PaymentMethod paymentMethod)
            : base(parent)
        {
			this.totalAmount = totalAmount;
			this.noTransactions = noTransactions;
            this.paymentMethod = paymentMethod;
            List<ContactAddressRow> companyAddress = GetAddressParts(parent.Company.ActorCompanyId, TermGroup_SysContactAddressType.Distribution);
            this.InitiatingParty = new ParticipantNode(this, parent.Company.Name, country, parent.Company.OrgNr, companyAddress, null, parent.BankId);
			this.CheckNode();
        }

		private void CheckNode()
		{
			if (this.totalAmount <= 0)
			{
				throw new ArgumentException("Total amount must be positive");
            }
        }

        internal Element CreateNode(string msgId, int nbrOfTransactions)
        {            
            return new Element("GrpHdr",
                new Element("MsgId", msgId.Truncate(35)),
                new Element("CreDtTm", this.CreateISODateTime(DateTime.Now)),
                new Element("BtchBookg", (this.Parent.AggregatePayments).ToString().ToLower()),
                new Element("NbOfTxs", nbrOfTransactions == 0 ? noTransactions : nbrOfTransactions),
				new Element("CtrlSum", FormatAmount(totalAmount)),
                new Element("Grpg", "MIXD"),
                InitiatingParty.CreateNode(this.paymentMethod.PaymentInformationRow.BankConnected)
                );
        }

        private string CreateISODateTime(DateTime dateTime)
        {
            return dateTime.ToString(ISODateFormat) + "T" + dateTime.ToString("hh':'mm':'ss");
        }
    }
}
