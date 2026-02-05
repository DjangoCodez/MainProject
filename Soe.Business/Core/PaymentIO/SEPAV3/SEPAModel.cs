using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    /// <summary>
    /// Skapar en datamodel som kan extraheras till XML för export
    /// For dokumentation see : Q:\Dokument\JT\SEPA\SCCT Technical Description v.1.1
    /// </summary>
    public class SEPAModel : SEPABase
    {
        //private static readonly string schemaLocation = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";
        //private static readonly XAttribute schemaLocationAttribute = new XAttribute(Element.ns_xsi + "schemaLocation", schemaLocation);

        private XDocument xdoc;
        internal GroupHeaderNode GroupHeader {get; private set;}
        private readonly PaymentInformationNode payments;
        internal Company Company {get; private set;}
        internal PaymentExportSettings exportSettings { get; private set; }

        public SEPAModel(CompEntities entities, Company company, ContactManager contactManager, CountryCurrencyManager countryCurrencyManager, PaymentManager paymentManager, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, int actorCompanyId, IEnumerable<PaymentRow> paymentRows, PaymentMethod paymentMethod, PaymentExportSettings exportSettings, bool containsForeignPayments)
            : base(entities, contactManager, countryCurrencyManager, paymentManager, sysCountries, sysCurrencies, exportSettings.ForeignBank)
        {
            this.Company = company;
            this.exportSettings = exportSettings;

            if (!paymentMethod.PaymentInformationRowReference.IsLoaded)
                paymentMethod.PaymentInformationRowReference.Load();

            if (this.Company.SysCountryId == null)
            {
                throw new ActionFailedException("Landskod saknas på företaget");
            }

            SysCountry country = sysCountries.FirstOrDefault(i => i.SysCountryId == this.Company.SysCountryId);

            //Only debit invoices to count()
            var noOfTransactions = 0;

            var groupedPayments = paymentRows.Where(p => p.Amount != 0).GroupBy(x => new { x.PayDate, x.PaymentNr, x.SysPaymentTypeId });

            foreach (var groupedPayment in groupedPayments)
            {
                if (groupedPayment.Any(r => r.Amount <= 0))
                    noOfTransactions++; //Credits and Debit to match them will go into the same CdtTrfTxInf
                else
                    noOfTransactions += groupedPayment.Count();
            }

            this.GroupHeader = new GroupHeaderNode(this, paymentRows.Sum(x => x.Amount), noOfTransactions, country, paymentMethod);

            //Should create "New payment header foreach duedate if there is several"
            this.payments = new PaymentInformationNode(this, this.Company.SysCountryId.GetValueOrDefault(), paymentRows, paymentMethod, containsForeignPayments);
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
            //GroupHeader element
			Element painElement = new Element("CstmrCdtTrfInitn", GroupHeader.CreateNode(this.exportSettings.MessageGuid));
			
            //PaymentInformation elements
			painElement.Add(payments.CreateNode());
			
            //document
			Element rootElement = new Element("Document", 
                new XAttribute(XNamespace.Xmlns + "xsi", Element.ns_xsi), 
                //schemaLocationAttribute,
				painElement);
            document.Add(rootElement);
            xdoc = document;
        }
    }
}
