
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ICACustomer2
    {

        public XName getElementName(XElement xml, string localName)
        {
            return XName.Get(localName, xml.GetDefaultNamespace().ToString());
        }

        public string ApplyICACustomer2(string content, int actorCompanyId, Account accountStdPurchase)
        {


            string modifiedContent = string.Empty;

            XElement customersElement = new XElement("Kunder");
            XElement customer = null;
            string customerNo = string.Empty;
            string customerName = string.Empty;
            string street = string.Empty;
            string city = string.Empty;
            string zipcode = string.Empty;
            string countryCode = string.Empty;
            string contactName = string.Empty;
            string contactFirstName = string.Empty;
            string contactLastName = string.Empty;
            string contactEmail = string.Empty;
            string active = string.Empty;
            string invoicingFrequency = string.Empty;

            string reference = string.Empty;
            string invoiceLabel = string.Empty;
            string organizationNumber = string.Empty;
            string VATRegistrationNumber = string.Empty;
            string gln = string.Empty;
            string invoicingMethod = string.Empty;
            string invoiceDeliveryType = string.Empty;
            string InvoiceDeliveryEmail = string.Empty;

            Decimal invoiceDiscountPercent = 0;
            Decimal invoiceCharge = 0;
            Decimal creditLimit = 0;


            string deliveryStreet = string.Empty;
            string deliveryCity = string.Empty;
            string deliveryZipcode = string.Empty;
            string deliveryCountryCode = string.Empty;

            string paymentTermsCode = string.Empty;
            string requirementFrequency = string.Empty;
            string coInvoicing = string.Empty;
            string attachments = string.Empty;

            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }
            List<XElement> customers = xml.Elements(getElementName(xml, "CustomerChangeMessage")).ToList();

            foreach (XElement Customer in customers)
            {
                foreach (XElement subElement in Customer.Elements())
                {
                    if (subElement.Name.LocalName.ToString().Equals("CustomerId"))
                        customerNo = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("Name"))
                        customerName = subElement.Value?.Left(100);
                    if (subElement.Name.LocalName.ToString().Equals("Street"))
                        street = subElement.Value?.Left(100);
                    if (subElement.Name.LocalName.ToString().Equals("City"))
                        city = subElement.Value?.Left(100);
                    if (subElement.Name.LocalName.ToString().Equals("ZipCode"))
                        zipcode = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("CountryCode"))
                        countryCode = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("InvoiceDiscountPercent"))
                        invoiceDiscountPercent = NumberUtility.ToDecimal(subElement.Value, 1);
                    if (subElement.Name.LocalName.ToString().Equals("InvoiceCharge"))
                        invoiceCharge = NumberUtility.ToDecimal(subElement.Value, 1);
                    if (subElement.Name.LocalName.ToString().Equals("CreditLimit"))
                        creditLimit = NumberUtility.ToDecimal(subElement.Value, 1);
                    if (subElement.Name.LocalName.ToString().Equals("Reference"))
                        reference = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("OrganizationNumber"))
                        organizationNumber = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("VATRegistrationNumber"))
                        VATRegistrationNumber = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("InvoiceDeliveryEmail"))
                        InvoiceDeliveryEmail = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("GLN"))
                        gln = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("InvoicingMethod"))
                        invoicingMethod = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("InvoicingMethod"))
                        invoiceDeliveryType = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("Active"))
                        active = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("PaymentTermsCode"))
                        paymentTermsCode = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("RequirementFrequency"))
                        requirementFrequency = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("CoInvoicing"))
                        coInvoicing = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("Attachments"))
                        attachments = subElement.Value;


                }
                List<XElement> contactsItem =
                        (from e in Customer.Elements()
                         where e.Name.LocalName == "Contact"
                         select e).ToList();

               foreach(XElement subElement in contactsItem.Elements())
                {
                    if (subElement.Name.LocalName.ToString().Equals("Name"))
                        contactName = subElement.Value?.Left(100);
                    if (subElement.Name.LocalName.ToString().Equals("Email"))
                        contactEmail = subElement.Value?.Left(100);
                }

                List<XElement> elementsItem =
                        (from e in Customer.Elements()
                         where e.Name.LocalName == "DeliveryLocation"
                         select e).ToList();
                foreach (XElement subElement in elementsItem.Elements())
                {
                    if (subElement.Name.LocalName.ToString().Equals("Street"))
                        deliveryStreet = subElement.Value?.Left(100);
                    if (subElement.Name.LocalName.ToString().Equals("City"))
                        deliveryCity = subElement.Value?.Left(100);
                    if (subElement.Name.LocalName.ToString().Equals("ZipCode"))
                        deliveryZipcode = subElement.Value;
                    if (subElement.Name.LocalName.ToString().Equals("CountryCode"))
                        deliveryCountryCode = subElement.Value;
                }
                if (invoiceCharge > 1)
                {
                    invoiceCharge = 0;
                }
                else
                {
                    invoiceCharge = 1;
                }
                switch (invoiceDeliveryType)
                {
                    case "300": invoiceDeliveryType = "3"; break;
                    case "500": invoiceDeliveryType = "1"; break;
                    case "600": invoiceDeliveryType = "2"; break;
                }
                var items = contactName.Split(' ');
                contactFirstName = items[0];
                contactLastName = items[1];

                var references = reference.Split('/');
                reference = references[0];
                if(references.Length > 1)
                    invoiceLabel = references[1];

                if (!string.IsNullOrEmpty(customerNo))
                {
                    customer = new XElement("Kund",
                    new XElement("Kundnr", customerNo),
                    new XElement("Namn", customerName),
                    new XElement("Adress1", street),
                    new XElement("Postnr", zipcode),
                    new XElement("Postadress", city),
                    new XElement("Land", countryCode),
                    new XElement("BetalningsvillkorsKod", paymentTermsCode),
                    new XElement("GenerellRabattVara", invoiceDiscountPercent),
                    new XElement("Faktureringsavgift", invoiceCharge),
                    new XElement("ErReferens", reference),
                    new XElement("Märke", invoiceLabel),
                    new XElement("Organisationsnr", organizationNumber),
                    new XElement("VATRegistrationNumber", VATRegistrationNumber),
                    new XElement("GLN", gln),
                    new XElement("LevAdress1", deliveryStreet),
                    new XElement("LevPostnr", deliveryZipcode),
                    new XElement("LevPostadress", deliveryCity),
                    new XElement("LevLand", deliveryCountryCode),
                    new XElement("FakturaEpost", InvoiceDeliveryEmail),
                    new XElement("Kreditgräns", creditLimit),
                    new XElement("Kontaktnamn", contactName),
                    new XElement("Kontaktförnamn", contactFirstName),
                    new XElement("Kontaktefternamn", contactLastName),
                    new XElement("KontaktEpost", contactEmail),
                    new XElement("Faktureringsmetod", invoiceDeliveryType),
                    new XElement("ImporteraDetaljerat", attachments)
                    );
                }



                if (customer != null)
                    customersElement.Add(customer);
            }

        //    voucherHeadsElement.Add(voucherHeads);

            modifiedContent = customersElement.ToString();

            return modifiedContent;
        }
    }
}

