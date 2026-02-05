using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class KGMphNEW
    {
        public string ApplyKGMphNEW(string content)
        {
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;
            string customerNumber = "";
            string customerName = "";
            string customerAdressCo = "";
            string customerAdress1 = "";
            string customerPostnr = "";
            string customerPostAdress = "";
             string invoiceYourRef = "";
            string invoiceOurRef = "";
            string invoiceRemark = "";
            decimal invoiceOrderNo = 0;
            decimal prevInvoiceOrderNo = 0;
            string invoiceDate = "";
            string labelText1 = "";
            string labelText2 = "";
            string vatCode = "";
            int vatProcent = 0;
            decimal headAmount = 0;
            decimal headVat = 0;


            XElement customerInvoicesHeadElement = new XElement("Kundfakturor");

            List<XElement> customerInvoices = new List<XElement>();

              XElement customerInvoice = new XElement("Kundfaktura");
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" || line.Length < 10) continue;

                XElement customerInvoiceRow = new XElement("Fakturarad");
                //Parse information              
                var prevCustomerNumber = customerNumber;
                prevInvoiceOrderNo = invoiceOrderNo;
                int lineLength = line.Length;
                customerNumber = line.Substring(0, 12);
                 invoiceOrderNo = line.Substring(540, 6) != string.Empty ? Convert.ToDecimal(line.Substring(540, 6)) : 0;

                if (prevInvoiceOrderNo != invoiceOrderNo)
                {
                    if (!(prevInvoiceOrderNo==0))
                    {
              
                        customerInvoice.Add(
                            new XElement("KundId", prevCustomerNumber),
                            new XElement("Kundnamn", customerName),
                            new XElement("OrderNr", prevInvoiceOrderNo.ToString()),
                            new XElement("ReferenceOur", invoiceOurRef),
                            new XElement("ReferenceYour", invoiceYourRef),
                            new XElement("Label", invoiceRemark),
                            new XElement("Fakturadatum", invoiceDate),
                            new XElement("FakturabeloppExklusive", headAmount),
                            new XElement("BillingAdressCO", customerAdressCo),
                            new XElement("BillingAdress1", customerAdress1),
                            new XElement("BillingPostNr", customerPostnr),
                            new XElement("BillingPostadress", customerPostAdress),
                            new XElement("Momsbelopp", headVat));
                        customerInvoices.Add(customerInvoice);
                        customerInvoice = new XElement("Kundfaktura");
                        headAmount = 0;
                        headVat = 0;
                    }
                }

                customerName = line.Substring(12, 40);
                customerAdressCo = line.Substring(52, 40);
                customerAdress1 = line.Substring(92, 40);
                customerPostnr = line.Substring(132, 6);
                customerPostAdress = line.Substring(138, 30);
                invoiceYourRef = line.Substring(198, 40);
                invoiceOurRef = line.Substring(238, 30);
                labelText1 = line.Substring(268, 10);
                labelText2 = line.Substring(278, 50);
                invoiceDate = line.Substring(547, 10);
                invoiceRemark = labelText1.Trim() + " " + labelText2.TrimEnd();

                vatCode = line.Substring(546, 1);

                if (vatCode == "3")
                {
                    vatProcent = 6;
                }
                if (vatCode == "2")
                {
                    vatProcent = 12;
                }
                if (vatCode == "1")
                {
                    vatProcent = 25;
                }
                if(vatCode == "0")
                {
                    vatCode = String.Empty;
                }

                string articleId = line.Substring(328, 12);
                if (string.IsNullOrEmpty(articleId) || articleId == "            ") continue;

                string rowAccount = line.Substring(348, 4);
                string rowText1 = line.Substring(352, 159);
                string rowQuantityText = line.Substring(511, 17);
                string rowPriceText = line.Substring(528, 12);
                decimal rowQuantity = rowQuantityText.Trim() != string.Empty ? Convert.ToDecimal(rowQuantityText) : 0;
                decimal rowPrice = rowPriceText.Trim() != string.Empty ? Convert.ToDecimal(rowPriceText) : 0;
                decimal rowAmount = rowQuantity * rowPrice;
                headAmount += rowAmount;

                customerInvoiceRow.Add(
                    new XElement("Artikelid", articleId),
                    new XElement("Namn", rowText1),
                    new XElement("Text", rowText1),
                    new XElement("Antal", rowQuantity),
                    new XElement("Pris", rowPrice),
                    new XElement("Belopp", rowAmount),
                    new XElement("Konto", rowAccount),
                    new XElement("MomsProcent", vatProcent),
                    new XElement("MomsKod", vatCode),
                    new XElement("Radtyp", 2));
                customerInvoice.Add(customerInvoiceRow);
            }
            customerInvoice.Add(
                           new XElement("KundId", customerNumber),
                            new XElement("Kundnamn", customerName),
                            new XElement("OrderNr", invoiceOrderNo.ToString()),
                            new XElement("ReferenceOur", invoiceOurRef),
                            new XElement("ReferenceYour", invoiceYourRef),
                            new XElement("Label", invoiceRemark),
                            new XElement("Fakturadatum", invoiceDate),
                            new XElement("FakturabeloppExklusive", headAmount),
                            new XElement("BillingAdressCO", customerAdressCo),
                            new XElement("BillingAdress1", customerAdress1),
                            new XElement("BillingPostNr", customerPostnr),
                            new XElement("BillingPostadress", customerPostAdress),
                            new XElement("Momsbelopp", headVat));

            customerInvoices.Add(customerInvoice);

            customerInvoicesHeadElement.Add(customerInvoices);

            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;
        }

    }

}