using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    /// <summary>
    /// Skapar en datamodel som kan extraheras till XML för export
    /// For dokumentation see : Q:\Dokument\JT\SEPA\SCCT Technical Description v.1.1
    /// </summary>
    public class SEPAModel : SEPABase
    {
        private static readonly string schemaLocation = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.02 pain.001.001.02.xsd";
        private static readonly XAttribute schemaLocationAttribute = new XAttribute(Element.ns_xsi + "schemaLocation", schemaLocation);

        private readonly string MsgId;
        private XDocument xdoc;
        internal GroupHeaderNode GroupHeader {get; private set;}
        private PaymentInformationNode payments;
        internal Company Company {get; private set;}
		internal bool AggregatePayments { get; private set; }
        internal string BankId { get; private set; }

        public SEPAModel(CompEntities entities, CompanyManager companyManager, ContactManager contactManager, PaymentManager paymentManager, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, int actorCompanyId, IEnumerable<PaymentRow> paymentRows, PaymentMethod paymentMethod, bool aggregatePayments, string msgId)
            : base(entities, companyManager, contactManager, paymentManager, sysCountries, sysCurrencies)
        {
            this.Company = companyManager.GetCompany(entities, actorCompanyId);
            this.MsgId = msgId;

            if (!paymentMethod.PaymentInformationRowReference.IsLoaded)
                paymentMethod.PaymentInformationRowReference.Load();
            if (paymentMethod.PaymentInformationRow.PayerBankId != null)
                this.BankId = paymentMethod.PaymentInformationRow.PayerBankId;
            else
                this.BankId = string.Empty;

            if (this.Company.SysCountryId != null)
            {
                SysCountry country =  GetFinland(sysCountries);
                
                //Only debit invoices to count()
                this.GroupHeader = new GroupHeaderNode(this, paymentRows.Sum(x => x.AmountCurrency), paymentRows.Count(p => p.AmountCurrency > 0), country, paymentMethod);
                //Should create "New payment header foreach duedate if there is several"
                this.payments = new PaymentInformationNode(this, paymentRows.FirstOrDefault(), (int)this.Company.SysCountryId, paymentRows, paymentMethod);
                //this.payments = from x in paymentRows select new PaymentInformationNode(this, x, (int)this.Company.SysCountryId);
                this.AggregatePayments = aggregatePayments;
            }
            else
            {
                throw new Exception("Landskod saknas på företaget");
            }
        }

		private static SysCountry GetFinland(List<SysCountry> sysCountries)
		{
			return sysCountries.FirstOrDefault(x => x.SysCountryId == 3); // Hårdkodad till Finland
		}

        public bool Validate()
        {
            if (xdoc == null)
            {
                return false;
            }
            return true;
        }
        
        public void ToXml(ref XDocument document)
        {         
            Element painElement = new Element("pain.001.001.02");
                        
            List<XElement> pmtElements = payments.CreateNodeV2();
            painElement.Add(GroupHeader.CreateNode(this.MsgId, payments.nbrOfCttiNodes ));
            painElement.Add(pmtElements);                     
           
            Element rootElement = new Element("Document", 
                new XAttribute(XNamespace.Xmlns + "xsi", Element.ns_xsi), 
                schemaLocationAttribute,
				painElement);
            document.Add(rootElement);
            xdoc = document;
        }
    }
}
