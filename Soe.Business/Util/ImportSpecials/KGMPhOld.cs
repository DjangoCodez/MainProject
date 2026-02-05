using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class KGMphOLD
    {
        public string ApplyKGMphOLD(string content)
        {
            //Method to create standard invoice based on KGM:S old PH format
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
            string customerPhoneNo = "";
            string customerFaxNo = "";
            string invoiceYourRef = "";
            string invoiceOurRef = "";
            string invoiceRemark = "";
            decimal invoiceOrderNo = 0;
            decimal prevInvoiceOrderNo = 0;
            string labelText1 = "";
            string labelText2 = "";
            string rowText1 = "";
            string rowPriceText = " ";

            string vatCode = "";
            int vatProcent = 0;
            decimal headAmount = 0;
            decimal headVat = 0;
            XElement customerInvoiceRow = new XElement("Fakturarad");

            XElement customerInvoicesHeadElement = new XElement("Kundfakturor");

            List<XElement> customerInvoices = new List<XElement>();

             XElement customerInvoice = new XElement("Kundfaktura");                                                               
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" || line.Length < 10) continue;

                string articleId = line.Substring(318, 12);

                if (string.IsNullOrEmpty(articleId) || articleId == "            ")
                {
                    rowText1 = rowText1.TrimEnd();
                    rowText1 = rowText1 + " ";
                    rowText1 += line.Substring(334, 60);
                    continue;
                }
                //Parse information              
                var prevCustomerNumber = customerNumber;
                prevInvoiceOrderNo = invoiceOrderNo;
                customerNumber = line.Substring(0, 12);
                invoiceOrderNo = line.Substring(411, 6) != string.Empty ? Convert.ToDecimal(line.Substring(411, 6)) : 0;

                if (prevInvoiceOrderNo != invoiceOrderNo)
                {
                    if (!(prevInvoiceOrderNo == 0))
                    {
                        //   customerInvoice.Add(customerInvoiceRow);                       
                        customerInvoice.Add(
                            new XElement("KundId", prevCustomerNumber),
                            new XElement("Kundnamn", customerName),
                            new XElement("OrderNr", prevInvoiceOrderNo.ToString()),
                            new XElement("ReferenceOur", invoiceOurRef),
                            new XElement("ReferenceYour", invoiceYourRef),
                            new XElement("Label", invoiceRemark),
                            new XElement("FakturabeloppExklusive", headAmount),
                            new XElement("BillingAdressCO", customerAdressCo),
                            new XElement("BillingAdress1", customerAdress1),
                            new XElement("BillingPostNr", customerPostnr),
                            new XElement("BillingPostadress", customerPostAdress),
                            new XElement("Momsbelopp", headVat));
                        customerInvoiceRow.Add(
                            new XElement("Namn", rowText1.TrimEnd()),
                            new XElement("Text", rowText1.TrimEnd()));
                        customerInvoice.Add(customerInvoiceRow);
                        customerInvoices.Add(customerInvoice);
                        customerInvoice = new XElement("Kundfaktura");
                        customerInvoiceRow = new XElement("Fakturarad");
                        rowText1 = "";
                        headAmount = 0;
                        headVat = 0;
                    }
                }
                else
                { 
                     if (!(prevInvoiceOrderNo == 0))
                     {   

                        customerInvoiceRow.Add(
                        new XElement("Namn", rowText1.TrimEnd()),
                        new XElement("Text", rowText1.TrimEnd()));
                        customerInvoice.Add(customerInvoiceRow);
                        customerInvoiceRow = new XElement("Fakturarad");
                        rowText1 = "";
                    }
                }
                
                customerName = line.Substring(12, 30);
                customerAdressCo = line.Substring(42, 30);
                customerAdress1 = line.Substring(72, 30);
                customerPostnr = line.Substring(102, 6);
                customerPostAdress = line.Substring(108, 30);
                customerPhoneNo = line.Substring(138, 6);
                customerFaxNo = line.Substring(168, 30);
                invoiceYourRef = line.Substring(198, 30);
                invoiceOurRef = line.Substring(228, 30);
                labelText1 = line.Substring(258, 30);
                labelText2 = line.Substring(288, 30);
                invoiceRemark = labelText1.Trim() + " " + labelText2.TrimEnd();
 
                vatCode = line.Substring(417, 1);
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
 
                 string rowAccount = line.Substring(330, 4);
                rowText1 += line.Substring(334, 60);
                rowPriceText = line.Substring(401, 10);

                decimal rowQuantity = line.Substring(394, 7) != string.Empty ? Convert.ToDecimal(line.Substring(394, 7)) : 0;
                decimal rowPrice = rowPriceText.Trim() != string.Empty ? Convert.ToDecimal(rowPriceText) : 0;
                decimal rowAmount = rowQuantity * rowPrice;
                headAmount += rowAmount;
  
                customerInvoiceRow.Add(
                    new XElement("Artikelid", articleId),
                    new XElement("Antal", rowQuantity),
                    new XElement("Pris", rowPrice),
                    new XElement("Belopp", rowAmount),
                    new XElement("Konto", rowAccount),
                    new XElement("MomsProcent", vatProcent),
                    new XElement("MomsKod", vatCode),
                    new XElement("Radtyp", 2));         
             }
            customerInvoice.Add(
                           new XElement("KundId", customerNumber),
                            new XElement("Kundnamn", customerName),
                            new XElement("OrderNr", invoiceOrderNo.ToString()),
                            new XElement("ReferenceOur", invoiceOurRef),
                            new XElement("ReferenceYour", invoiceYourRef),
                            new XElement("Label", invoiceRemark),
                            new XElement("FakturabeloppExklusive", headAmount),
                            new XElement("BillingAdressCO", customerAdressCo),
                            new XElement("BillingAdress1", customerAdress1),
                            new XElement("BillingPostNr", customerPostnr),
                            new XElement("BillingPostadress", customerPostAdress),
                            new XElement("Momsbelopp", headVat));
            customerInvoiceRow.Add(
                        new XElement("Namn", rowText1.TrimEnd()),
                        new XElement("Text", rowText1.TrimEnd()));
            customerInvoice.Add(customerInvoiceRow);
            
            customerInvoices.Add(customerInvoice);
            
            customerInvoicesHeadElement.Add(customerInvoices);
 
            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;
        }

    }
}
