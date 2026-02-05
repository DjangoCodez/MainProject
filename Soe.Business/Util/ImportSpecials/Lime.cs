using SoftOne.Soe.Business.Core;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Lime
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="actorCompanyId"></param>
        /// <returns></returns>
        public string ApplyLimeCustomerInvoiceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            XElement customerInvoicesHeadElement = new XElement("CustomerInvoices");
            string modifiedContent = string.Empty;
            string prevInvoiceNr = string.Empty;
            decimal totAmount = 0;
            string invoiceNr = string.Empty;
            string quantity = string.Empty;
            string unitPrice = string.Empty;
            string vatCode = string.Empty;
            decimal vatRate = 0;
            decimal vatAmount = 0;
            string accountNr = string.Empty;
            string accountDim2Nr = string.Empty;
            string accountDim3Nr = string.Empty;
            string text = string.Empty;
            decimal tempQuantity = 0;            
            decimal tempUnitPrice = 0;            
            decimal amount = 0;
            string customerNr = string.Empty;
            string customerName = string.Empty;
            string invoiceDate = string.Empty;
            string dueDate = string.Empty;
            string billingAddressAddress = string.Empty;
            string billingAddressPostnr = string.Empty;
            string billingAddressCity = string.Empty;
            string referenceOur = string.Empty;
            string referenceYour = string.Empty;
            string productNr = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;
            List<XElement> invoiceElements = new List<XElement>();
            XElement customerInvoice = new XElement("CustomerInvoice");
            List<XElement> srows = new List<XElement>();
            List<XElement> mrows = new List<XElement>();



            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" || !line.Contains(";")) continue;

                string[] inputRow = line.Split(delimiter);
                if(string.IsNullOrEmpty(inputRow[0]))
                    continue;

                // Format for Lime customerinvoice importfile 20170516
                //1   Cust.nr;
                //3	  Invoice.nr.;
                //4	  Inv.date;
                //5	  Due date;
                //6	  Billingaddress;
                //7	  PostNr;
                //8	  City;
                //9	  ReferenceOur;
                //10  ReferenceYour;
                //11  CustomerName;
                //16  Productnr;
                //17  Quantity;
                //18  UnitPrice;
                //19  VatCode;
                //20  AccuntNr;
                //21  AccountDim2nr;
                //22  AccountDim3nr;
                //23  Text;

                invoiceNr = inputRow[2] != null ? inputRow[2] : string.Empty;
                            
                if (prevInvoiceNr != invoiceNr)
                {
                    if (prevInvoiceNr != string.Empty)
                    {
// skriv faktura
                        
                        customerInvoice = new XElement("CustomerInvoice");
                        customerInvoice.Add(
                         new XElement("CustomerNr", customerNr),
                         new XElement("CustomerName", customerName),
                         new XElement("CustomerInvoiceNr", prevInvoiceNr),
                         new XElement("InvoiceDate", invoiceDate),
                         new XElement("DueDate", dueDate),
                         new XElement("BillingAddressAddress", billingAddressAddress),
                         new XElement("BillingAddressPostNr", billingAddressPostnr),
                         new XElement("BillingAddressCity", billingAddressCity),
                         new XElement("ReferenceOur", referenceOur),
                         new XElement("ReferenceYour", referenceYour),
                         new XElement("Amount", totAmount.ToString()),
                         new XElement("VatCode", vatCode));
                        if (srows.Count > 0)
                            customerInvoice.Add(srows);
                        if (mrows.Count > 0)
                            customerInvoice.Add(mrows);                 
                        invoiceElements.Add(customerInvoice);
                        totAmount = 0;
                    }
                    

                    srows = new List<XElement>();
                    mrows = new List<XElement>();

                    prevInvoiceNr = inputRow[2] != null ? inputRow[2] : string.Empty;

                    customerNr = inputRow[0] != null ? inputRow[0].Trim() : string.Empty;
                    customerName = inputRow[10] != null ? inputRow[10] : string.Empty;
                    invoiceDate = inputRow[3] != null ? "20" + inputRow[3] : string.Empty;
                    dueDate = inputRow[4] != null ? "20" + inputRow[4] : string.Empty;
                    billingAddressAddress = inputRow[5] != null ? inputRow[5] : string.Empty;
                    billingAddressPostnr = inputRow[6] != null ? inputRow[6] : string.Empty;
                    billingAddressCity = inputRow[7] != null ? inputRow[7] : string.Empty;
                    referenceOur = inputRow[8] != null ? inputRow[8] : string.Empty;
                    referenceYour = inputRow[9] != null ? inputRow[9] : string.Empty;
                    vatCode = inputRow[18] != null ? inputRow[18] : string.Empty;

                    productNr = inputRow[15] != null ? inputRow[15] : string.Empty;
                    quantity = inputRow[16] != null ? inputRow[16] : string.Empty;
                    unitPrice = inputRow[17] != null ? inputRow[17] : string.Empty;
                    vatCode = inputRow[18] != null ? inputRow[18] : string.Empty;
                    accountNr = inputRow[19] != null ? inputRow[19] : string.Empty;
                    accountDim2Nr = inputRow[20] != null ? inputRow[20] : string.Empty;
                    accountDim3Nr = inputRow[21] != null ? inputRow[21] : string.Empty;
                    text = inputRow[22] != null ? inputRow[22] : string.Empty;
                    decimal.TryParse(inputRow[16], out tempQuantity);
                    decimal.TryParse(inputRow[17], out tempUnitPrice);
                    amount = tempQuantity * tempUnitPrice;
                    if (vatCode == "5")
                        vatRate = 0;
                    if (vatCode == "5")
                        vatAmount = 0;
                    if (vatCode == "1")
                        vatRate = 25;
                    if (vatCode == "1")
                        vatAmount = amount * 25 / 100;
                    totAmount = totAmount + amount;
                    customerNr = inputRow[0] != null ? inputRow[0].Trim() : string.Empty;
                    customerName = inputRow[10] != null ? inputRow[10] : string.Empty;
                    invoiceDate = inputRow[3] != null ? "20" + inputRow[3] : string.Empty;
                    dueDate = inputRow[4] != null ? "20" + inputRow[4] : string.Empty;
                    billingAddressAddress = inputRow[5] != null ? inputRow[5] : string.Empty;
                    billingAddressPostnr = inputRow[6] != null ? inputRow[6] : string.Empty;
                    billingAddressCity = inputRow[7] != null ? inputRow[7] : string.Empty;
                    referenceOur = inputRow[8] != null ? inputRow[8] : string.Empty;
                    referenceYour = inputRow[9] != null ? inputRow[9] : string.Empty;
                    XElement srow = new XElement("CustomerInvoiceRow");
                    srow.Add(
                        new XElement("ProductNr", productNr),
                        new XElement("Quantity", quantity),
                        new XElement("UnitPrice", unitPrice),
                        new XElement("Amount", amount.ToString()),
                        new XElement("VatCode", vatCode),
                        new XElement("VatRate", vatRate),
                        new XElement("VatAmount", vatAmount),
                        new XElement("AccountNr", accountNr),
                        new XElement("AccountDim2Nr", accountDim2Nr),
                        new XElement("AccountDim3Nr", accountDim3Nr));
                    srows.Add(srow);
                    srow = new XElement("CustomerInvoiceRow");
                    srow.Add(
                        new XElement("ProductNr", string.Empty),
                        new XElement("Quantity",0),
                        new XElement("UnitPrice", 0),
                        new XElement("VatCode", vatCode),
                        new XElement("VatRate", vatRate),
                        new XElement("VatAmount", 0),
                        new XElement("Text", text));
                    srows.Add(srow);
                }
                else
                {
                    productNr = inputRow[15] != null ? inputRow[15] : string.Empty;
                    quantity = inputRow[16] != null ? inputRow[16] : string.Empty;
                    unitPrice = inputRow[17] != null ? inputRow[17] : string.Empty;
                    vatCode = inputRow[18] != null ? inputRow[18] : string.Empty;
                    accountNr = inputRow[19] != null ? inputRow[19] : string.Empty;
                    accountDim2Nr = inputRow[20] != null ? inputRow[20] : string.Empty;
                    accountDim3Nr = inputRow[21] != null ? inputRow[21] : string.Empty;
                    text = inputRow[22] != null ? inputRow[22] : string.Empty;
                    decimal.TryParse(inputRow[16], out tempQuantity);
                    decimal.TryParse(inputRow[17], out tempUnitPrice);
                    amount = tempQuantity * tempUnitPrice;
                    if (vatCode == "5")
                        vatRate = 0;
                    if (vatCode == "5")
                        vatAmount = 0;
                    if (vatCode == "1")
                        vatRate = 25;
                    if (vatCode == "1")
                        vatAmount = amount * 25 / 100;
                    totAmount = totAmount + amount;
                    XElement mrow = new XElement("CustomerInvoiceRow");
                    mrow.Add(
                        new XElement("ProductNr", productNr),
                        new XElement("Quantity", quantity),
                        new XElement("UnitPrice", unitPrice),
                        new XElement("Amount", amount.ToString()),
                        new XElement("VatCode", vatCode),
                        new XElement("VatRate", vatRate),
                        new XElement("VatAmount", vatAmount),
                        new XElement("AccountNr", accountNr),
                        new XElement("AccountDim2Nr", accountDim2Nr),
                        new XElement("AccountDim3Nr", accountDim3Nr));
                    mrows.Add(mrow);
                    mrow = new XElement("CustomerInvoiceRow");
                    mrow.Add(
                        new XElement("ProductNr", string.Empty),
                        new XElement("Quantity", 0),
                        new XElement("UnitPrice", 0),
                        new XElement("VatCode", vatCode),
                        new XElement("VatRate", vatRate),
                        new XElement("VatAmount", 0),
                        new XElement("Text", text));
                    mrows.Add(mrow);
                    }
                }
            if (prevInvoiceNr != string.Empty)
            {
                customerInvoice = new XElement("CustomerInvoice");
                customerInvoice.Add(
                 new XElement("CustomerNr", customerNr),
                 new XElement("CustomerName", customerName),
                 new XElement("CustomerInvoiceNr", prevInvoiceNr),
                 new XElement("InvoiceDate", invoiceDate),
                 new XElement("DueDate", dueDate),
                 new XElement("BillingAddressAddress", billingAddressAddress),
                 new XElement("BillingAddressPostNr", billingAddressPostnr),
                 new XElement("BillingAddressCity", billingAddressCity),
                 new XElement("ReferenceOur", referenceOur),
                 new XElement("ReferenceYour", referenceYour),
                 new XElement("Amount", totAmount.ToString()),
                 new XElement("VatCode", vatCode));
                if (srows.Count > 0)
                    customerInvoice.Add(srows);
                if (mrows.Count > 0)
                    customerInvoice.Add(mrows);

                invoiceElements.Add(customerInvoice);
            }
            customerInvoicesHeadElement.Add(invoiceElements);

            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;

        }

    }
}
