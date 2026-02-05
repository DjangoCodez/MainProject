using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SAPCustomerinvoice
    {
        public string ApplySAPCustomerInvoiceSpecialModification(string content, int actorCompanyId)
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
            decimal invoiceNumberRow = 0;
            string line;
            List<XElement> invoiceElements = new List<XElement>();
            XElement customerInvoice = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                string[] inputRow = line.Split(delimiter);

                //    XElement customerInvoice = new XElement("CustomerInvoice");
                string invoiceType = line.Substring(0, 4);

                if (invoiceType == "FAKT")
                {
                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                        customerInvoice = null;
                        invoiceNumberRow = 0;
                    }
                    invoiceNumberRow = 0;
                    string customerAdressCo = "";
                    string customerAdress1 = "";
                    string customerPostnr = "";
                    string customerPostAdress = "";
                    string invoiceNumber = line.Substring(58, 10);
                    //remove trailing spaces from invoiceNumber
                    invoiceNumber = invoiceNumber.TrimEnd(' ');
                    string customerNr = line.Substring(10, 12);
                    customerNr = customerNr.Trim();
                    decimal temptotalBruttoAmount = 0;
                    string belopp = line.Substring(22, 12).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        temptotalBruttoAmount = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    string invoiceDate = line.Substring(34, 6);
                    string dueDate = line.Substring(46, 6);
                    string voucherDate = line.Substring(193, 6);
                    // fakturatyp line.Substring(52, 2
                    string currencyCode = line.Substring(55, 3);
                    decimal currencyRate = 0;
                    belopp = line.Substring(576, 7).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        currencyRate = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 4);
                    }
                    string PaymentConditionCode = line.Substring(533, 3);
                    string vatRateCode = line.Substring(539, 1);
                    decimal vatRate = 0;
                    if (vatRateCode == "1")
                    {
                        vatRate = 25;
                    }
                    else if (vatRateCode == "2")
                    {
                        vatRate = 12;
                    }
                    else if (vatRateCode == "3")
                    {
                        vatRate = 6;
                    }
                    if (invoiceDate != string.Empty)
                    {
                        invoiceDate = "20" + invoiceDate;
                    }
                    if (dueDate != string.Empty)
                    {
                        dueDate = "20" + dueDate;
                    }
                    if (voucherDate != string.Empty)
                    {
                        voucherDate = "20" + voucherDate;
                    }
                    // );
                    //string vat25NettoAmount = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                    //decimal tempVatAmount25 = 0;
                    decimal vatAmount = 0;
                    //tempVatAmount25 = inputRow[9] != string.Empty ? Convert.ToDecimal(inputRow[9]) : 0;
                    belopp = line.Substring(91, 12).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        vatAmount = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    //vatAmount = tempVatAmount25 * -1;
                    string billingType;
                    if (temptotalBruttoAmount > 0)
                    {
                        billingType = "1";
                    }
                    else
                    {
                        billingType = "2";
                    }

                    decimal totBruttoAmount = decimal.Round((temptotalBruttoAmount + vatAmount), 2);
                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                    }

                    string OCRCode = line.Substring(206, 30);
                    string customerName = line.Substring(236, 30);
                    string customerOrgNo = line.Substring(482, 10);
                    customerAdressCo = line.Substring(356, 30);
                    customerAdress1 = line.Substring(386, 30);
                    customerPostnr = line.Substring(416, 6);
                    customerPostAdress = line.Substring(422, 30);

                    customerInvoice = new XElement("CustomerInvoice");

                    customerInvoice.Add(
                        new XElement("CustomerNr", customerNr),
                        new XElement("CustomerName", customerName),
                        new XElement("CustomerAdressCo", customerAdressCo),
                        new XElement("CustomerAdress1", customerAdress1),
                        new XElement("CustomerPostnr", customerPostnr),
                        new XElement("CustomerPostAdress", customerPostAdress),
                        new XElement("CustomerOrgNo", customerOrgNo),
                        new XElement("CustomerInvoiceNr", invoiceNumber),
                        new XElement("CustomerInvoiceNr1", invoiceNumber),
                        new XElement("InvoicePaymentConditionCode", PaymentConditionCode),
                        new XElement("InvoiceOcrCode", OCRCode),
                        new XElement("InvoiceDate", invoiceDate),
                        new XElement("CurrencyDate", invoiceDate),
                        new XElement("DueDate", dueDate),
                        new XElement("VoucherDate", voucherDate),
                        new XElement("Currency", currencyCode),
                        new XElement("CurrencyRate", currencyRate.ToString()),
                        new XElement("VATAmount", vatAmount.ToString()),
                        new XElement("VATAmountCurrency", vatAmount.ToString()),
                        new XElement("VatRate1", vatRate.ToString()),
                        new XElement("VatAmount1", vatAmount.ToString()),
                        new XElement("TempTotalAmount", temptotalBruttoAmount.ToString()),
                        new XElement("TotalAmount", totBruttoAmount.ToString()),
                        new XElement("BillingType", billingType),
                        new XElement("TotalAmountCurrency", temptotalBruttoAmount.ToString()));
                }
                else if (invoiceType == "KONT")
                {
                    invoiceNumberRow++;

                    string konto = line.Substring(5, 6);
                    string kst = line.Substring(11, 6);
                    string proj = line.Substring(17, 6);
                    decimal belopp = 0;
                    string bel = line.Substring(26, 17).Replace(",", ""); ;
                    if (!String.IsNullOrWhiteSpace(bel))
                    {
                        belopp = decimal.Round(Convert.ToDecimal(bel, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    else
                    {
                        continue;
                    }
                    decimal quant = 0;
                    string kvant = line.Substring(57, 17).Replace(",", ""); ;
                    if (!String.IsNullOrWhiteSpace(kvant))
                    {
                        quant = decimal.Round(Convert.ToDecimal(kvant, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    string text = line.Substring(44, 25);

                    if (belopp != 0)
                    {
                        XElement row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", belopp.ToString()),
                            new XElement("Text", text.Trim()),
                            new XElement("AccountNr", konto.Trim()),
                            new XElement("CostplaceNr", kst.Trim()),
                            new XElement("ProjectNr", proj.Trim()),
                            new XElement("Quantity", quant.ToString())
                            );
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
