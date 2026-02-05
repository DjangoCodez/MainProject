using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Cask
    {
        public string ApplyCaskCustomerInvoiceSpecialModification(string content, int actorCompanyId)
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

            int VatCode25AccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);
            int VatCode12AccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable2, 0, actorCompanyId, 0);

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" || !line.Contains(";")) continue;

                string[] inputRow = line.Split(delimiter);

                int customerNumber = 0;
                int.TryParse(inputRow[0], out customerNumber);
                if (customerNumber == 0) continue; //Skip the columnheader row

                // Format for CASK customerinvoice importfile 20150429
                //1   Cust.nr;
                //2	  Name;
                //3	  Sales org.;
                //4	  Org.nr.;
                //5	  Inv.nr.;
                //6	  Inv.date;
                //7	  Due date;
                //8	  Debit/Credit;
                //9	  Netto 12%;
                //10  Netto 25%;
                //11  12% tax;
                //12  25% tax;
                //13  Rounding;
                //14  Brutto;
                //15  Currency;
                //16  Saleschannel;
                //17  Ordertype;
                //18  Description;

                string customerNr = inputRow[0] != null ? inputRow[0].Trim() : string.Empty;
                string customerName = inputRow[1] != null ? inputRow[1] : string.Empty;
                string invoiceNr = inputRow[4] != null ? inputRow[4] : string.Empty;
                string invoiceDate = inputRow[5] != null ? inputRow[5] : string.Empty;
                string dueDate = inputRow[6] != null ? inputRow[6] : string.Empty;

                string vat12NettoAmount = inputRow[8] != null ? inputRow[8].Replace(".", "") : string.Empty;
                string vat25NettoAmount = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                string vat12Amount = inputRow[10] != null ? inputRow[10].Replace(".", "") : string.Empty;
                string vat25Amount = inputRow[11] != null ? inputRow[11].Replace(".", "") : string.Empty;

                string totalBruttoAmount = inputRow[13] != null ? inputRow[13].Replace(".", "") : string.Empty;
                string currencyCode = inputRow[14] != null ? inputRow[14] : string.Empty;

                decimal tempVatAmount12 = 0;
                decimal tempVatAmount25 = 0;
                decimal.TryParse(inputRow[10].Replace(".", ""), out tempVatAmount12);
                decimal.TryParse(inputRow[11].Replace(".", ""), out tempVatAmount25);
                decimal vatAmount = tempVatAmount12 + tempVatAmount25;

                XElement customerInvoice = new XElement("CustomerInvoice");

                customerInvoice.Add(
                    new XElement("CustomerNr", customerNr),
                    new XElement("CustomerName", customerName),
                    new XElement("CustomerInvoiceNr", invoiceNr),
                    new XElement("InvoiceDate", invoiceDate),
                    new XElement("DueDate", dueDate),
                    new XElement("Currency", currencyCode),
                    new XElement("VatAmount", vatAmount.ToString()),
                    new XElement("VatRate1", "12.00"),
                    new XElement("VatRate2", "25,00"),
                    new XElement("VatAmount1", tempVatAmount12.ToString()),
                    new XElement("VatAmount2", tempVatAmount25.ToString()),
                    new XElement("TotalAmount", totalBruttoAmount));

                List<XElement> rows = new List<XElement>();

                //if Vat12 sales
                if (vat12NettoAmount != "0,00")
                {
                    XElement row12 = new XElement("CustomerInvoiceRow");
                    row12.Add(
                        new XElement("ProductNr", "101"),
                        new XElement("Amount", vat12NettoAmount),
                        new XElement("AmountCurrency", vat12NettoAmount),
                        new XElement("SumAmount", vat12NettoAmount),
                        new XElement("SumAmountCurrency", vat12NettoAmount),
                        new XElement("VatRate", "12,00"),
                        new XElement("VatAmount", vat12Amount),
                        new XElement("VatCodeId", VatCode12AccountId.ToString()),
                        new XElement("AccountNr", "3043"));
                    rows.Add(row12);
                }

                //if Vat25 sales                
                if (vat25NettoAmount != "0,00")
                {
                    XElement row25 = new XElement("CustomerInvoiceRow");
                    row25.Add(
                        new XElement("ProductNr", "100"),
                        new XElement("Amount", vat25NettoAmount),
                        new XElement("AmountCurrency", vat25NettoAmount),
                        new XElement("SumAmount", vat25NettoAmount),
                        new XElement("SumAmountCurrency", vat25NettoAmount),
                        new XElement("VatRate", "25,00"),
                        new XElement("VatAmount", vat25Amount),
                        new XElement("VatCodeId", VatCode25AccountId.ToString()),
                        new XElement("AccountNr", "3042"));

                    rows.Add(row25);
                }

                if (rows.Count > 0)
                    customerInvoice.Add(rows);

                invoiceElements.Add(customerInvoice);
            }

            customerInvoicesHeadElement.Add(invoiceElements);

            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;

        }

    }
}
