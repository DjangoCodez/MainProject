using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    internal class GroupHeaderNode : SEPANode<SEPAModel>
    {
        internal ParticipantNode InitiatingParty { get; private set; }
        internal ParticipantNode DebtorParty { get; private set; }
		private readonly int noTransactions;

        internal GroupHeaderNode(SEPAModel parent, decimal totalAmount, int noTransactions, SysCountry country, PaymentMethod paymentMethod) : base(parent)
        {
			this.noTransactions = noTransactions;
            List<ContactAddressRow> companyAddress = GetAddressParts(parent.Company.ActorCompanyId, TermGroup_SysContactAddressType.Distribution);
            if (!string.IsNullOrEmpty(parent.exportSettings.HeaderSignerId))
            {
                InitiatingParty = new ParticipantNode(this, parent.exportSettings.HeaderSignerName, parent.exportSettings.HeaderSignerId, country, null, paymentMethod, null);
            }
            else
            {
                InitiatingParty = new ParticipantNode(this, parent.Company.Name, parent.Company.OrgNr, country, companyAddress, paymentMethod, null);
            }
            
            DebtorParty = new ParticipantNode(this, parent.Company.Name, parent.Company.OrgNr, country, companyAddress, paymentMethod, null);
        }

        internal Element CreateNode(string msgId)
        {
            return new Element("GrpHdr",
                new Element("MsgId", msgId.Truncate(35)),
                new Element("CreDtTm", this.CreateISODateTime(DateTime.Now)),               
                new Element("NbOfTxs", noTransactions),
                //new Element("CtrlSum", FormatAmount(totalAmount)),  //not required by Nordea              
                InitiatingParty.CreateNodeInitgPty(this.Parent.exportSettings.HeaderSignerId, this.Parent.exportSettings.HeaderSignerSchemaName)
            );
        }

        private string CreateISODateTime(DateTime dateTime)
        {
            return dateTime.ToString(ISODateFormat) + "T" + dateTime.ToString("hh':'mm':'ss");           
        }
    }
}
