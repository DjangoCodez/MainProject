using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Spectra
    {
        public string ApplySpectraCustomerInvoiceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            XElement customerInvoicesHeadElement = new XElement("CustomerInvoices");
            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;
            List<XElement> invoiceElements = new List<XElement>();
            XElement customerInvoice = null;
            
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" || !line.Contains(";")) continue;

                string[] inputRow = line.Split(delimiter);

                int invoiceNr = 0;
                int.TryParse(inputRow[1], out invoiceNr);
                if (invoiceNr == 0) continue;

                // Format for Spectra customerinvoice importfile 2090116
                //1   Kundreskontra;
                //2   Fakturanr;
                //3   Cust.nr;
                //4	  Fakturadatum;
                //5	  Förfallodatum;
                //9	  Momsbelopp;              
                //10  Rounding;
                //13  Valutakod;
                //14  Kurs;
                //15  Belopp inkl. moms;
                //1   Kontering;
                //2   Fakturanr;
                //3	  Fakturadatum;
                //4	  Konto;
                //5	  Belopp;              
                //8   Text;

            //    XElement customerInvoice = new XElement("CustomerInvoice");
                string invoiceType = inputRow[0] != null ? inputRow[0].Trim() : string.Empty;
                
                if (invoiceType == "Kundreskontra")
                {
                    string customerNr = inputRow[2] != null ? inputRow[2].Trim() : string.Empty;
                    //remove leading zeros from customer number
                    customerNr = customerNr.TrimStart('0');
                    string invoiceNunber = inputRow[1] != null ? inputRow[1] : string.Empty;
                    //remove leading zeros from invoiceNunberr
                    invoiceNunber = invoiceNunber.TrimStart('0');
                    string invoiceDate = inputRow[3] != null ? inputRow[3] : string.Empty;
                    string dueDate = inputRow[4] != null ? inputRow[4] : string.Empty;

                    string vat25NettoAmount = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                    string totalBruttoAmount = inputRow[15] != null ? inputRow[15].Replace(".", "") : string.Empty;
                    string currencyCode = inputRow[13] != null ? inputRow[13] : string.Empty;

                    decimal tempVatAmount25 = 0;
                    decimal temptotalBruttoAmount = 0;
                    decimal vatAmount = 0;
                    tempVatAmount25 = inputRow[9] != string.Empty ? Convert.ToDecimal(inputRow[9]) : 0;
                    temptotalBruttoAmount = inputRow[15] != string.Empty ? Convert.ToDecimal(inputRow[15]) : 0;
                    vatAmount = tempVatAmount25 * -1;
                    decimal totBruttoAmount = temptotalBruttoAmount - vatAmount;
                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                    }
                    customerInvoice = new XElement("CustomerInvoice");

                    customerInvoice.Add(
                        new XElement("CustomerNr", customerNr),
                        new XElement("CustomerInvoiceNr", invoiceNunber),
                        new XElement("InvoiceDate", invoiceDate),
                        new XElement("DueDate", dueDate),
                        new XElement("Currency", currencyCode),
                        new XElement("VATAmount", vatAmount.ToString()),
                        new XElement("VATAmountCurrency", vatAmount.ToString()),
                        new XElement("VatRate1", "25,00"),
                        new XElement("VatAmount1", vatAmount.ToString()),
                        new XElement("TempTotalAmount", temptotalBruttoAmount.ToString()),
                        new XElement("TotalAmount", temptotalBruttoAmount.ToString()),
                        new XElement("TotalAmountCurrency", temptotalBruttoAmount.ToString()));
                }
                else if (customerInvoice!=null)
                {
                    decimal tempBelopp = 0;
                    string invoiceNunberRow = inputRow[1] != null ? inputRow[1] : string.Empty;
                    //remove leading zeros from invoiceNunberr
                    invoiceNunberRow = invoiceNunberRow.TrimStart('0');
                    string invoiceDate = inputRow[2] != null ? inputRow[2] : string.Empty;
                    string konto = inputRow[3] != null ? inputRow[3] : string.Empty;
                    string belopp = inputRow[4] != null ? inputRow[4] : string.Empty;
                    string text = inputRow[8] != null ? inputRow[8] : string.Empty;
                    tempBelopp = inputRow[4] != string.Empty ? Convert.ToDecimal(inputRow[4]) : 0;
                    if (belopp != "0,00")
                    {
                        XElement row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceNr", invoiceNunberRow),
                            new XElement("Amount", tempBelopp.ToString()),
                            new XElement("Text", text.Trim()),                         
                            new XElement("AccountNr", konto.Trim()));
                        customerInvoice.Add(row);
                    }
                }
                
            }

            if (customerInvoice != null)
            { 
                invoiceElements.Add(customerInvoice);
            }
            customerInvoicesHeadElement.Add(invoiceElements);

            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;

        }

    }
}
