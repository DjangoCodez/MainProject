using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class HsbOrderRow
    {
        public string ApplyHsbOrderRowSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            InvoiceManager invoiceManager = new InvoiceManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            XElement accountsHeadElement = new XElement("Orders");
            string modifiedContent = string.Empty;
            string orderNo = string.Empty;
            string productNo = string.Empty;
            string productName = string.Empty;
            string amount = string.Empty;
            string date = string.Empty;
     //       string customerName = string.Empty;
            string customerNo = string.Empty;
            int orderId = 0;
            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            XElement invoicesElement = new XElement("Orderinfo");


            List<XElement> fakturor = xml.Elements("Faktura").ToList();

            foreach (XElement faktura in fakturor)
            {
                foreach (XElement subElement in faktura.Elements())
                {
                    if (subElement.Name.ToString().ToLower().Equals("ordernr"))
                        orderNo = subElement.Value;
                    if (subElement.Name.ToString().ToLower().Equals("artikelnr"))
                        productNo = subElement.Value;
                    if (subElement.Name.ToString().ToLower().Equals("benamning"))
                        productName = subElement.Value;
                    if (subElement.Name.ToString().ToLower().Equals("apris"))
                        amount = subElement.Value;
                    if (subElement.Name.ToString().ToLower().Equals("registreringsdatum"))
                        date = subElement.Value;
                }
                var order = invoiceManager.GetOrder(actorCompanyId, orderNo);

                if (order != null)
                {
                    customerNo = order.CustomerNr;
                    orderId = order.CustomerInvoiceId;
                }

                //XElement invoice = new XElement("Order");
                //invoice.Add(
                //    new XElement("Ordernr", orderNo),
                //    new XElement("Orderid", orderId),
                //    new XElement("Kundnr", customerNo)
                //    );
                XElement invoiceRow = new XElement("OrderRad");
                invoiceRow.Add(
                    new XElement("Ordernr", orderNo),
                    new XElement("Orderid", orderId),
                    new XElement("Kundnr", customerNo),
                    new XElement("Artikelnr", productNo),
                    new XElement("Benamning", productName),
                    new XElement("Apris", amount),
                    new XElement("Antal", 1),
                    new XElement("Registreringsdatum", date)
                    );
                //if (invoice != null)
                //    invoice.Add(invoiceRow);
                invoicesElement.Add(invoiceRow);
            }
              modifiedContent = invoicesElement.ToString();

            return modifiedContent;
        }

    }
}
