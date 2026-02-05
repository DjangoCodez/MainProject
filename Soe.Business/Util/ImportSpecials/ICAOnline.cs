using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class IcaOnline
    {
        public string ApplyIcaOnlineModification(string content, bool details)
        {
            XElement root = new XElement("Kundfakturor");
            List<XElement> elements = new List<XElement>();            

            XDocument xdoc = XDocument.Parse(content);            
            List<XElement> elementOrders = XmlUtil.GetChildElements(xdoc, "orders");

            foreach (XElement elementOrder in elementOrders)
            {
                #region Parse

                XElement elementOrderAmount = XmlUtil.GetChildElement(elementOrder, "orderAmount");
                XElement elementOrderVatAmountByCode = XmlUtil.GetChildElement(elementOrderAmount, "vatAmountByCode");
                string vatAmountTotal = XmlUtil.GetChildElementValue(elementOrderAmount, "vatAmountTotal").Replace('.', ',');
                string exVatTotal = XmlUtil.GetChildElementValue(elementOrderAmount, "exVatTotal").Replace('.', ',');
                decimal OrderTotalVatAmount = Decimal.Parse(vatAmountTotal);
                decimal OrderTotalAmountExVat = Decimal.Parse(exVatTotal);

                decimal decVatAmount6 = 0;
                decimal decVatAmount12 = 0;
                decimal decVatAmount25 = 0;
                List<XElement> elementVatAmountByCodeLineItems = XmlUtil.GetChildElements(elementOrderVatAmountByCode, "vatLineItems");
                if (elementVatAmountByCodeLineItems != null)
                {
                    foreach (XElement element in elementVatAmountByCodeLineItems)
                    {
                        string vatPercent = XmlUtil.GetChildElementValue(element, "vatPercent");
                        string vatAmount = XmlUtil.GetChildElementValue(element, "amount").Replace('.', ',');
                        if (vatPercent == "6") 
                            decVatAmount6 += Decimal.Parse(vatAmount);
                        if (vatPercent == "12") 
                            decVatAmount12 += Decimal.Parse(vatAmount);
                        if (vatPercent == "25") 
                            decVatAmount25 += Decimal.Parse(vatAmount);
                    }
                }

                decimal decExVatAmount6 = 0;
                decimal decExVatAmount12 = 0;
                decimal decExVatAmount25 = 0;
                XElement elementExVatAmountByCode = XmlUtil.GetChildElement(elementOrderAmount, "exVatByVatCode");
                List<XElement> elementExVatLineItems = XmlUtil.GetChildElements(elementExVatAmountByCode, "vatLineItems");
                if (elementExVatLineItems != null)
                {
                    foreach (XElement element in elementExVatLineItems)
                    {
                        string vatPercent = XmlUtil.GetChildElementValue(element, "vatPercent");
                        string vatAmount = XmlUtil.GetChildElementValue(element, "amount").Replace('.', ',');
                        if (vatPercent == "6") 
                            decExVatAmount6 += Decimal.Parse(vatAmount);
                        if (vatPercent == "12") 
                            decExVatAmount12 += Decimal.Parse(vatAmount);
                        if (vatPercent == "25") 
                            decExVatAmount25 += Decimal.Parse(vatAmount);
                    }
                }                
                                                
                decimal decVatAmount6Shipping = 0;
                decimal decVatAmount12Shipping = 0;
                decimal decVatAmount25Shipping = 0;
                XElement elementShippingAmount = XmlUtil.GetChildElement(elementOrder, "shippingAmount");
                XElement elementShippingVatAmountByCode = XmlUtil.GetChildElement(elementShippingAmount, "vatAmountByCode");
                List<XElement> elementVatLineItemsShipping = XmlUtil.GetChildElements(elementShippingVatAmountByCode, "vatLineItems");
                if (elementVatLineItemsShipping != null)
                {
                    foreach (XElement element in elementVatLineItemsShipping)
                    {
                        string vatPercent = XmlUtil.GetChildElementValue(element, "vatPercent");
                        string vatAmount = XmlUtil.GetChildElementValue(element, "amount").Replace('.', ',');
                        if (vatPercent == "6") 
                            decVatAmount6Shipping += Decimal.Parse(vatAmount);
                        if (vatPercent == "12") 
                            decVatAmount12Shipping += Decimal.Parse(vatAmount);
                        if (vatPercent == "25") 
                            decVatAmount25Shipping += Decimal.Parse(vatAmount);
                    }
                }
                               
                decimal decExVatAmount6Shipping = 0;
                decimal decExVatAmount12Shipping = 0;
                decimal decExVatAmount25Shipping = 0;
                XElement elementExVatByVatCodeShipping = XmlUtil.GetChildElement(elementShippingAmount, "exVatByVatCode");
                List<XElement> elementExVatLineItemsShipping = XmlUtil.GetChildElements(elementExVatByVatCodeShipping, "vatLineItems");
                if (elementExVatLineItemsShipping != null)
                {
                    foreach (XElement element in elementExVatLineItemsShipping)
                    {
                        string vatPercent = XmlUtil.GetChildElementValue(element, "vatPercent");
                        string vatAmount = XmlUtil.GetChildElementValue(element, "amount").Replace('.', ',');
                        if (vatPercent == "6") 
                            decExVatAmount6Shipping += Decimal.Parse(vatAmount);
                        if (vatPercent == "12") 
                            decExVatAmount12Shipping += Decimal.Parse(vatAmount);
                        if (vatPercent == "25") 
                            decExVatAmount25Shipping += Decimal.Parse(vatAmount);
                    }
                }

                decimal TotalAmountExVat_OrderAndShipping = OrderTotalAmountExVat;
                decimal TotalVatAmount_OrderAndShipping = OrderTotalVatAmount;

                string costCenter = XmlUtil.GetChildElementValue(elementOrder, "costCenter");
                string userEmail = XmlUtil.GetChildElementValue(elementOrder, "userEmail");
                string emailBillingContact = XmlUtil.GetChildElementValue(elementOrder, "emailBillingContact");
                string customerName = XmlUtil.GetChildElementValue(elementOrder, "companyName");
                string customerNumber = XmlUtil.GetChildElementValue(elementOrder, "soCustomerNumber");
                string atgCustomerNumber = XmlUtil.GetChildElementValue(elementOrder, "atgCustomerNumber");                             

                //if StoreOffice customernumber is empty (which it is 90% of the time), set ATG customernumber as customernumber instead
                if (string.IsNullOrWhiteSpace(customerNumber))
                    customerNumber = atgCustomerNumber;

                string customerCompleteBillingAddress = XmlUtil.GetChildElementValue(elementOrder, "companyAddress");
                string[] splittedAddress = customerCompleteBillingAddress.Split(',');
                string customerBillingAddress;
                string customerBillingAddressCity;
                string customerBillingAddressPostNr;
                int lengthAddress = splittedAddress.Length;
                if (lengthAddress == 4)
                {
                    customerBillingAddress = splittedAddress[0] != null ? splittedAddress[0] : "";
                    customerBillingAddressCity = splittedAddress[2] != null ? splittedAddress[2] : "";
                    customerBillingAddressPostNr = splittedAddress[3] != null ? splittedAddress[3] : "";
                }
                else
                {
                    customerBillingAddress = splittedAddress[0] != null ? splittedAddress[0] : "";
                    customerBillingAddressCity = splittedAddress[1] != null ? splittedAddress[1] : "";
                    customerBillingAddressPostNr = splittedAddress[2] != null ? splittedAddress[2] : "";
                }
                string customerCompleteDeliveryAddress = XmlUtil.GetChildElementValue(elementOrder, "deliveryAddressLine1");
                string customerDeliveryPostNr = XmlUtil.GetChildElementValue(elementOrder, "deliveryPostalCode");
                string customerDeliveryCity = XmlUtil.GetChildElementValue(elementOrder, "deliveryCity");
                string[] splittedDeliveryAddress = customerCompleteDeliveryAddress.Split(',');
                string customerDeliveryAddress;
 
                int lengthDeliveryAddress = splittedDeliveryAddress.Length;
                if (lengthDeliveryAddress > 1)
                    customerDeliveryAddress = splittedDeliveryAddress[1] != null ? splittedDeliveryAddress[0] + " " + splittedDeliveryAddress[1]: "";
                else
                    customerDeliveryAddress = customerCompleteDeliveryAddress;
                
                string orderReference = XmlUtil.GetChildElementValue(elementOrder, "orderReference");
                string orderID = XmlUtil.GetChildElementValue(elementOrder, "orderID");
                string orderDate = XmlUtil.GetChildElementValue(elementOrder, "orderDate");
                string deliveryDate = XmlUtil.GetChildElementValue(elementOrder, "orderDate");
                string receiptID = XmlUtil.GetChildElementValue(elementOrder, "receiptID");
                string invoiceDate = XmlUtil.GetChildElementValue(elementOrder, "invoiceDate");

                #endregion

                #region Element Kundfaktura

                XElement elementKundfaktura = new XElement("Kundfaktura");
                elementKundfaktura.Add(
                       new XElement("KundId", customerNumber),
                       new XElement("FakturabeloppExklusive", TotalAmountExVat_OrderAndShipping.ToString().Replace('.', ',')),
                       new XElement("Momsbelopp", TotalVatAmount_OrderAndShipping.ToString().Replace('.', ',')),
                       new XElement("Kundnr", customerNumber),
                       new XElement("Kundnamn", customerName),
                       new XElement("FakturaDatum", invoiceDate),
                       new XElement("OrderDatum", orderDate),
                       new XElement("LeveransDatum", deliveryDate),
                       new XElement("FakturaAdress", customerBillingAddress),
                       new XElement("FakturaAdressPostNr", customerBillingAddressPostNr),
                       new XElement("FakturaAdressStad", customerBillingAddressCity),
                       new XElement("LeveransAdress", customerDeliveryAddress),
                       new XElement("LeveransAdressPostNr", customerDeliveryPostNr),
                       new XElement("LeveransAdressStad", customerDeliveryCity),
                       new XElement("Epost", emailBillingContact),
                       new XElement("Costcenter", costCenter),
                       new XElement("OrderReference", orderReference),
                       new XElement("ICAfiltyp", "3"));

                #endregion

                #region Elements Fakturarad

                List<XElement> elementsFakturarad = new List<XElement>();

                if (!invoiceDate.IsNullOrEmpty())
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", "Orderdatum: " + orderDate),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                if (!orderID.IsNullOrEmpty())
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", "OrderId: " + orderID),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                if (!receiptID.IsNullOrEmpty())
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", string.Empty),
                        new XElement("Namn", "Kvitto: " + receiptID),
                        new XElement("Antal", 1),
                        new XElement("Pris", 0),
                        new XElement("Momskod", string.Empty),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 3),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }

                #region Products
                if (decExVatAmount25 != 0 || decVatAmount25 != 0)
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Varor 25% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", decExVatAmount25),
                        new XElement("Momskod", "25"),
                        new XElement("MomsProcent", 25),
                        new XElement("Radmomsbelopp", decVatAmount25),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "3"));

                    elementsFakturarad.Add(elementFakturarad);
                }
                if (decExVatAmount12 != 0 || decVatAmount12 != 0)
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Varor 12% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", decExVatAmount12),
                        new XElement("Momskod", "12"),
                        new XElement("MomsProcent", 12),
                        new XElement("Radmomsbelopp", decVatAmount12),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                if (decExVatAmount6 != 0 || decVatAmount6 != 0)
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Varor 6% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", decExVatAmount6),
                        new XElement("Momskod", "6"),
                        new XElement("MomsProcent", 6),
                        new XElement("Radmomsbelopp", decVatAmount6),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                #endregion

                #region Shipping
                if (decExVatAmount25Shipping != 0 || decVatAmount25Shipping != 0)
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Frakt 25% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", decExVatAmount25Shipping),
                        new XElement("Momskod", "25"),
                        new XElement("MomsProcent", 25),
                        new XElement("Radmomsbelopp", decVatAmount25Shipping),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                if (decExVatAmount12Shipping != 0 || decVatAmount12Shipping != 0)
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Frakt 12% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", decExVatAmount12Shipping),
                        new XElement("Momskod", "12"),
                        new XElement("MomsProcent", 12),
                        new XElement("Radmomsbelopp", decVatAmount12Shipping),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                if (decExVatAmount6Shipping != 0 || decVatAmount6Shipping != 0)
                {
                    XElement elementFakturarad = new XElement("Fakturarad");
                    elementFakturarad.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Frakt 6% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", decExVatAmount6Shipping),
                        new XElement("Momskod", "6"),
                        new XElement("MomsProcent", 6),
                        new XElement("Radmomsbelopp", decVatAmount6Shipping),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "3"));
                    elementsFakturarad.Add(elementFakturarad);
                }
                #endregion

                #region Details
                if (details)
                {
                    XElement elementsProductLine = XmlUtil.GetChildElement(elementOrder, "productLineItems");
                    //productLineItems should be changed to productLineItem here!! The xsd and xml file from ICA differs.. what is correct?
                    List<XElement> elementsProductItems = XmlUtil.GetChildElements(elementsProductLine, "productLineItems");
                    if (elementsProductItems != null)
                    {
                        foreach (XElement elementProductLineItem in elementsProductItems)
                        {
                            string productName = XmlUtil.GetChildElementValue(elementProductLineItem, "name");
                            string productAmount = XmlUtil.GetChildElementValue(elementProductLineItem, "amount").Replace('.', ',');
                            string productQuantity = XmlUtil.GetChildElementValue(elementProductLineItem, "quantity").Replace('.', ',');

                            XElement elementFakturarad = new XElement("Fakturarad");
                            elementFakturarad.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", productName + "       " + productQuantity + "           " + Decimal.Parse(productAmount).ToString("N2")),
                                new XElement("Antal", 0),
                                new XElement("Pris", 0),
                                new XElement("Momskod", "0"),
                                new XElement("MomsProcent", 0),
                                new XElement("Radmomsbelopp", 0),
                                new XElement("Radtyp", 2),
                                new XElement("ICAfiltyp", "3"));
                            elementsFakturarad.Add(elementFakturarad);
                        }
                    }
                }
                #endregion

                #endregion

                elementKundfaktura.Add(elementsFakturarad);
                elements.Add(elementKundfaktura);
            }

            root.Add(elements);
            return root.ToString();
        }
    }
}

