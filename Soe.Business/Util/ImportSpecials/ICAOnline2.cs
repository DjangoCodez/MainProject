using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ICAOnline2
    {

        public XName getElementName(XElement xml, string localName)
        {
            return XName.Get(localName, xml.GetDefaultNamespace().ToString());
        }

        public string ApplyIcaOnline2(string content, int actorCompanyId, bool details)
        {

            var customerManager = new CustomerManager( null );
            var productPricelistManager = new ProductPricelistManager(null);

            List<Customer> customers = customerManager.GetCustomersByCompany(actorCompanyId, true);
            string modifiedContent = string.Empty;

            XElement customerInvoicesHeadElement = new XElement("Kundfakturor");
            List<XElement> customerInvoices = new List<XElement>();


            string receiptID = string.Empty;
            string requisitionID = string.Empty;

            string addDocRefID = string.Empty;
            string addDocRefType = string.Empty;
            string attachmentDocType = string.Empty;
            string attachmentData = string.Empty;
            string customerNumber = "";
            string issueDate = "";
            string txtPriceAmountInclVat = "";
            decimal taxAmont = 0;
            string actualDeliveryDate = string.Empty;
            string name = string.Empty;
            decimal decQuantity = 0;
            decimal decPriceAmount = 0;
            decimal decPriceAmountInclVat = 0;
            decimal vat_0_decPriceAmountInclVat = 0;
            decimal vat_6_decPriceAmountInclVat = 0;
            decimal vat_12_decPriceAmountInclVat = 0;
            decimal vat_25_decPriceAmountInclVat = 0;
            decimal vat_0_decVatAmount = 0;
            decimal vat_6_decVatAmount = 0;
            decimal vat_12_decVatAmount = 0;
            decimal vat_25_decVatAmount = 0;
            decimal decTaxExclusiveAmount = 0;
            decimal decVatPercent = 0;
            decimal decVatAmount = 0;
            decimal decExVatAmount = 0;
            string discountAmount = string.Empty;
            decimal decDiscountAmount = 0;
            string tender_cashID = string.Empty;
            decimal decTender_cashID = 0;
            decimal decTender_cashSUM = 0; 
            decimal tender_CreditDebitAmount = 0;
            decimal tender_GiftCertificateAmount = 0;
            decimal tender_VoucherAmount = 0;

            string personnrID = string.Empty;

            XDocument xdoc = XDocument.Parse(content);

            var xml =
              (from e in xdoc.Elements()
               where e.Name.LocalName == "Orders"
               select e).FirstOrDefault();

            if (xml == null)
            {
                throw new SoeGeneralException("Kan inte läsa från XML fil", this.ToString());
            }

            List<XElement> elementsOrder =
               (from e in xml.Elements()
                where e.Name.LocalName == "Order"
                select e).ToList();

            foreach (XElement Order in elementsOrder)
            {

                vat_0_decPriceAmountInclVat = 0;
                vat_6_decPriceAmountInclVat = 0;
                vat_12_decPriceAmountInclVat = 0;
                vat_25_decPriceAmountInclVat = 0;
                vat_0_decVatAmount = 0;
                vat_6_decVatAmount = 0;
                vat_12_decVatAmount = 0;
                vat_25_decVatAmount = 0;
                decTender_cashID = 0;
                decTender_cashSUM = 0;
                tender_CreditDebitAmount = 0;
                tender_GiftCertificateAmount = 0;
                tender_VoucherAmount = 0;


                XElement customerInvoice = new XElement("Kundfaktura");

                List<XElement> customerInvoiceRows = new List<XElement>();

                issueDate = XmlUtil.GetChildElementValue(Order, "IssueDate");

                discountAmount = XmlUtil.GetChildElementValue(Order, "Note");
                decDiscountAmount = NumberUtility.ToDecimal(discountAmount, 2);

                receiptID = XmlUtil.GetChildElementValue(Order, "ID");

                List<XElement> elementsAdditionalDocumentReference =
                         (from e in Order.Elements()
                          where e.Name.LocalName == "AdditionalDocumentReference"
                          select e).ToList();

                foreach (XElement AdditionalDocumentReference in elementsAdditionalDocumentReference)
                {

                    foreach (XElement subElement in AdditionalDocumentReference.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("ID"))
                            addDocRefID = subElement.Value;
                        if (subElement.Name.LocalName.ToString().Equals("DocumentType"))
                            addDocRefType = subElement.Value;
                    }
                    if (addDocRefType == "RECEIPT_NUMBER")
                    {
                        receiptID = addDocRefID;
                    }
                    if (addDocRefType == "PERSONNR")
                    {
                        personnrID = addDocRefID;
                    }
                    if (addDocRefType == "TENDER_CASH")
                    {
                        tender_cashID = addDocRefID;
                        decTender_cashID += NumberUtility.ToDecimal(tender_cashID, 2);
                        decTender_cashSUM = decTender_cashID * -1;
                    }
                    if (addDocRefType == "TENDER_CREDITDEBIT") 
                    {
                        tender_CreditDebitAmount += NumberUtility.ToDecimal(addDocRefID, 2) * -1;
                    }
                    if (addDocRefType == "TENDER_GIFTCERTIFICATE")
                    {
                        tender_GiftCertificateAmount += NumberUtility.ToDecimal(addDocRefID, 2) * -1;
                    }
                    if (addDocRefType == "TENDER_VOUCHER")
                    {
                        tender_VoucherAmount += NumberUtility.ToDecimal(addDocRefID, 2) * -1;
                    }
                    if (addDocRefType == "REQUISITION_NUMBER")
                    {
                        requisitionID = addDocRefID;
                    }
                    if (addDocRefType == "BINARY_ATTACHMENT")
                    {

                        XElement embeddedDocumentBinaryAttachment =
                                                             (from e in AdditionalDocumentReference.Elements()
                                                              where e.Name.LocalName == "Attachment"
                                                              select e).FirstOrDefault();
                        foreach (XElement subElement in embeddedDocumentBinaryAttachment.Elements())
                        {
                            if (subElement.Attribute("mimeCode") != null)
                            {
                                attachmentDocType = subElement.Attribute("mimeCode").Value;
                                attachmentData = subElement.Value;

                                //#if DEBUG
                                //        File.WriteAllBytes(@"C:\Temp\IcaKvitto.pdf", Convert.FromBase64String(attachmentData));
                                //#endif
                            }
                        }
                    }
                }

                List<XElement> elementsBuyerCustomerParty =
                        (from e in Order.Elements()
                         where e.Name.LocalName == "BuyerCustomerParty"
                         select e).ToList();

                List<XElement> elementsBuyerParty =
                           (from e in elementsBuyerCustomerParty.Elements()
                            where e.Name.LocalName == "Party"
                            select e).ToList();

                foreach (XElement BuyerCustomerParty in elementsBuyerParty)
                {
                    customerNumber = "";
                    foreach (XElement subElement in BuyerCustomerParty.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("PartyLegalEntity"))
                            customerNumber = subElement.Value;
                    }

                }

                List<XElement> elementsTaxTotal =
                       (from e in Order.Elements()
                        where e.Name.LocalName == "TaxTotal"
                        select e).ToList();

                foreach (XElement TaxTotal in elementsTaxTotal)
                {
                    taxAmont = 0;

                    foreach (XElement subElement in TaxTotal.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("TaxAmount"))
                            taxAmont = NumberUtility.ToDecimal(subElement.Value, 2);
                    }
                }

                List<XElement> elementsSAnticipatedMonetaryTotal =
                       (from e in Order.Elements()
                        where e.Name.LocalName == "AnticipatedMonetaryTotal"
                        select e).ToList();

                foreach (XElement AnticipatedMonetaryTotal in elementsSAnticipatedMonetaryTotal)
                {
                    decTaxExclusiveAmount = 0;
                    foreach (XElement subElement in AnticipatedMonetaryTotal.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("TaxExclusiveAmount"))
                        {
                            decTaxExclusiveAmount = NumberUtility.ToDecimal(subElement.Value, 2);
                        }
                    }
                }

                Customer customer = customerManager.GetCustomerByNr(actorCompanyId, customerNumber, customers, false);

                string customerName = customer?.Name ?? string.Empty;

                customerInvoice.Add(
                           new XElement("KundId", customerNumber),
                           new XElement("FakturabeloppExklusive", decTaxExclusiveAmount.ToString().Replace('.', ',')),
                           new XElement("Momsbelopp", taxAmont.ToString().Replace('.', ',')),
                           new XElement("Kundnr", customerNumber),
                           new XElement("Kundnamn", customerName),
                           new XElement("FakturaDatum", issueDate),
                           new XElement("OrderDatum", issueDate),
                           new XElement("LeveransDatum", issueDate),
                           new XElement("PriceListTypeId", customer?.PriceListTypeId ?? null),
                           new XElement("ICAfiltyp", "3"));

                if (issueDate != "")
                {
                    XElement customerInvoiceRowInfo3 = new XElement("Fakturarad");
                    string dateInfo = "Orderdatum: " + issueDate;
                    customerInvoiceRowInfo3.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", dateInfo),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    customerInvoiceRows.Add(customerInvoiceRowInfo3);
                }
                if (requisitionID != "")
                {
                    XElement customerInvoiceRowInfo2 = new XElement("Fakturarad");
                    string orderInfo = "Rekvisition: " + requisitionID;
                    customerInvoiceRowInfo2.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", orderInfo),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    customerInvoiceRows.Add(customerInvoiceRowInfo2);
                }

                if (!String.IsNullOrEmpty(personnrID))
                {
                    XElement customerInvoiceRowPersonnr = new XElement("Fakturarad");
                    string personNrInfo = "Personnr: " + personnrID;
                    customerInvoiceRowPersonnr.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", personNrInfo), 
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    customerInvoiceRows.Add(customerInvoiceRowPersonnr);
                }

                if (receiptID != "")
                {
                    XElement customerInvoiceRowInfo = new XElement("Fakturarad");
                    string receiptInfo = "Kvitto: " + receiptID;
                    customerInvoiceRowInfo.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", receiptInfo),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    customerInvoiceRows.Add(customerInvoiceRowInfo);
                }

                if (decDiscountAmount != 0)
                {
                    XElement customerInvoiceRowInfo = new XElement("Fakturarad");
                    string receiptInfo = "Erhållen rabatt: " + decDiscountAmount.ToString().Replace('.', ',');
                    customerInvoiceRowInfo.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", receiptInfo),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    customerInvoiceRows.Add(customerInvoiceRowInfo);
                }

                issueDate = "";
                requisitionID = "";
                receiptID = "";


                List<XElement> elementsOrderLine =
                       (from e in Order.Elements()
                        where e.Name.LocalName == "OrderLine"
                        select e).ToList();


                decQuantity = 0;
                decPriceAmount = 0;
                decPriceAmountInclVat = 0;
                actualDeliveryDate = "";
                name = "";
                decVatPercent = 0;


                foreach (XElement OrderLine in elementsOrderLine)
                {
                    decQuantity = 0;
                    decPriceAmount = 0;
                    decPriceAmountInclVat = 0;
                    txtPriceAmountInclVat = "";
                    decVatPercent = 0;

                    decExVatAmount = 0;

                    List<XElement> elementsLineItem =
                       (from e in OrderLine.Elements()
                        where e.Name.LocalName == "LineItem"
                        select e).ToList();

                    foreach (XElement subElement in elementsLineItem.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("LineExtensionAmount"))
                            decExVatAmount = NumberUtility.ToDecimal(subElement.Value, 2);
                        if (subElement.Name.LocalName.ToString().Equals("TotalTaxAmount"))
                            decVatAmount = NumberUtility.ToDecimal(subElement.Value, 2);
                        if (subElement.Name.LocalName.ToString().Equals("Quantity"))
                            decQuantity = NumberUtility.ToDecimal(subElement.Value, 2);
                    }
 
                    List<XElement> elementsDelivery =
                            (from e in elementsLineItem.Elements()
                             where e.Name.LocalName == "Delivery"
                             select e).ToList();


                    foreach (XElement subElement in elementsDelivery.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("ActualDeliveryDate"))
                            actualDeliveryDate = subElement.Value;
                    }

                    List<XElement> elementsPrice =
                             (from e in elementsLineItem.Elements()
                              where e.Name.LocalName == "Price"
                              select e).ToList();


                    foreach (XElement subElement in elementsPrice.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("PriceAmount"))
                            decPriceAmount = NumberUtility.ToDecimal(subElement.Value, 2);
                    }
                    if (decPriceAmount <= 0)
                    {
                        decPriceAmount = decPriceAmount * -1;
                    }
                    List<XElement> elementsItem =
                           (from e in elementsLineItem.Elements()
                            where e.Name.LocalName == "Item"
                            select e).ToList();


                    foreach (XElement subElement in elementsItem.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("Name"))
                            name = subElement.Value;
                    }

                    List<XElement> elementsClassifiedTaxCategory =
                          (from e in elementsItem.Elements()
                           where e.Name.LocalName == "ClassifiedTaxCategory"
                           select e).ToList();


                    foreach (XElement subElement in elementsClassifiedTaxCategory.Elements())
                    {
                        if (subElement.Name.LocalName.ToString().Equals("Percent"))
                            decVatPercent = NumberUtility.ToDecimal(subElement.Value, 0);
                    }

                    List<XElement> elementsAdditionalItemProperty =
                           (from e in elementsItem.Elements()
                            where e.Name.LocalName == "AdditionalItemProperty"
                            select e).ToList();


                    foreach (XElement subElement in elementsAdditionalItemProperty.Elements())
                    {
                        if (txtPriceAmountInclVat == "GROSS_AMOUNT")
                            decPriceAmountInclVat = NumberUtility.ToDecimal(subElement.Value, 2);
                        if (subElement.Value.ToString().Equals("GROSS_AMOUNT"))
                            txtPriceAmountInclVat = subElement.Value.ToString();
                    }

                    //---- Products (Varor) --------------------------
                    if (decQuantity == 0)
                        decQuantity = 1;

                    decPriceAmountInclVat = decPriceAmountInclVat - decVatAmount;

                    switch (decVatPercent)
                    {
                        case 0:
                            vat_0_decPriceAmountInclVat += decPriceAmountInclVat;
                            vat_0_decVatAmount += decVatAmount;
                            break;
                        case 6:
                            vat_6_decPriceAmountInclVat += decPriceAmountInclVat;
                            vat_6_decVatAmount += decVatAmount;
                            break;
                        case 12:
                            vat_12_decPriceAmountInclVat += decPriceAmountInclVat;
                            vat_12_decVatAmount += decVatAmount;
                            break;
                        case 25:
                            vat_25_decPriceAmountInclVat += decPriceAmountInclVat;
                            vat_25_decVatAmount += decVatAmount;
                            break;
                    }

                    //if (decQuantity != 0)
                    //    decVatAmount = decVatAmount / decQuantity;

                    if (customer == null || customer.ImportInvoicesDetailed)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            XElement customerInvoiceRow = new XElement("Fakturarad");

                            customerInvoiceRow.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", name),
                                new XElement("Antal", decQuantity),
                                 new XElement("Pris", decPriceAmountInclVat.ToString().Replace('.', ',')),
                                new XElement("Momskod", decVatPercent.ToString()),
                                new XElement("MomsProcent", decVatPercent),
                                new XElement("Radmomsbelopp", decVatAmount.ToString().Replace('.', ',')),
                                new XElement("Radtyp", 2));

                            customerInvoiceRows.Add(customerInvoiceRow);
                        }
                    }
                }

                if (customer != null && !customer.ImportInvoicesDetailed)
                {
                    if (vat_0_decPriceAmountInclVat != 0)
                    {
                        XElement customerInvoiceRow = new XElement("Fakturarad");

                        customerInvoiceRow.Add(
                            new XElement("Artikelid", "0"),
                            new XElement("Namn", "varor momsfritt"),
                            new XElement("Antal", 1),
                             new XElement("Pris", vat_0_decPriceAmountInclVat.ToString().Replace('.', ',')),
                            new XElement("Momskod", "0"),
                            new XElement("MomsProcent", 0),
                            new XElement("Radmomsbelopp", vat_0_decVatAmount.ToString().Replace('.', ',')),
                            new XElement("Radtyp", 2));

                        customerInvoiceRows.Add(customerInvoiceRow);
                    }
                    if (vat_6_decPriceAmountInclVat != 0)
                    {
                        XElement customerInvoiceRow = new XElement("Fakturarad");

                        customerInvoiceRow.Add(
                            new XElement("Artikelid", "0"),
                            new XElement("Namn", "varor 6% moms"),
                            new XElement("Antal", 1),
                             new XElement("Pris", vat_6_decPriceAmountInclVat.ToString().Replace('.', ',')),
                            new XElement("Momskod", "6"),
                            new XElement("MomsProcent", 6),
                            new XElement("Radmomsbelopp", vat_6_decVatAmount.ToString().Replace('.', ',')),
                            new XElement("Radtyp", 2));

                        customerInvoiceRows.Add(customerInvoiceRow);
                    }
                    if (vat_12_decPriceAmountInclVat != 0)
                    {
                        XElement customerInvoiceRow = new XElement("Fakturarad");

                        customerInvoiceRow.Add(
                            new XElement("Artikelid", "0"),
                            new XElement("Namn", "varor 12% moms"),
                            new XElement("Antal", 1),
                             new XElement("Pris", vat_12_decPriceAmountInclVat.ToString().Replace('.', ',')),
                            new XElement("Momskod", "12"),
                            new XElement("MomsProcent", 12),
                            new XElement("Radmomsbelopp", vat_12_decVatAmount.ToString().Replace('.', ',')),
                            new XElement("Radtyp", 2));

                        customerInvoiceRows.Add(customerInvoiceRow);
                    }
                    if (vat_25_decPriceAmountInclVat != 0)
                    {
                        XElement customerInvoiceRow = new XElement("Fakturarad");

                        customerInvoiceRow.Add(
                            new XElement("Artikelid", "0"),
                            new XElement("Namn", "varor 25% moms"),
                            new XElement("Antal", 1),
                             new XElement("Pris", vat_25_decPriceAmountInclVat.ToString().Replace('.', ',')),
                            new XElement("Momskod", "25"),
                            new XElement("MomsProcent", 25),
                            new XElement("Radmomsbelopp", vat_25_decVatAmount.ToString().Replace('.', ',')),
                            new XElement("Radtyp", 2));

                        customerInvoiceRows.Add(customerInvoiceRow);
                    }
                }
                if (decTender_cashID != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Delbetalning kontant"),
                        new XElement("Antal", -1),
                         new XElement("Pris", decTender_cashSUM.ToString().Replace('.', ',')),
                        new XElement("Momskod", "0"),
                        new XElement("MomsProcent", 0),
                         new XElement("Radtyp", 2));

                    customerInvoiceRows.Add(customerInvoiceRow);
                }
                if (tender_CreditDebitAmount != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Delbetalning kort"),
                        new XElement("Antal", -1),
                         new XElement("Pris", tender_CreditDebitAmount.ToString().Replace('.', ',')),
                        new XElement("Momskod", "0"),
                        new XElement("MomsProcent", 0),
                         new XElement("Radtyp", 2));

                    customerInvoiceRows.Add(customerInvoiceRow);
                }
                if (tender_GiftCertificateAmount != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Delbetalning bonuscheck/presentkort"),
                        new XElement("Antal", -1),
                         new XElement("Pris", tender_GiftCertificateAmount.ToString().Replace('.', ',')),
                        new XElement("Momskod", "0"),
                        new XElement("MomsProcent", 0),
                         new XElement("Radtyp", 2));

                    customerInvoiceRows.Add(customerInvoiceRow);
                }
                if (tender_VoucherAmount != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Delbetalning swish"),
                        new XElement("Antal", -1),
                         new XElement("Pris", tender_VoucherAmount.ToString().Replace('.', ',')),
                        new XElement("Momskod", "0"),
                        new XElement("MomsProcent", 0),
                         new XElement("Radtyp", 2));

                    customerInvoiceRows.Add(customerInvoiceRow);
                }

                customerInvoice.Add(customerInvoiceRows);
                if (customerInvoice != null)
                    customerInvoices.Add(customerInvoice);
            }
            
            customerInvoicesHeadElement.Add(customerInvoices);

            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;
        }
    }
}

