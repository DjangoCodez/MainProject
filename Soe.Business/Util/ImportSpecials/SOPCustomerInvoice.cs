using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SOPCustomerinvoice
    {
        public string ApplySOPCustomerInvoiceSpecialModification(string content, int actorCompanyId)
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
                string invoiceType = inputRow[0];

                if (invoiceType.Trim() == "1" || invoiceType.Trim() == "4")
                {
                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                        customerInvoice = null;
                        invoiceNumberRow = 0;
                    }
                    invoiceNumberRow = 0;
                    string invoiceNunber = inputRow[1];
                    //remove leading zeros from invoiceNunber
                    invoiceNunber = invoiceNunber.TrimStart('0');
                    string customerNr =  inputRow[3];
                    //remove leading zeros from customer number
                    //customerNr = customerNr.TrimStart('0');
                    customerNr = customerNr.Trim();
                    decimal temptotalBruttoAmount = 0;
                    decimal invoiceFee = 0;
                    decimal invoiceTotal = 0;
                    decimal centRounding = 0;
                    string belopp =  inputRow[8].Replace(',', ' ');
                    int belLength = belopp.Length;
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        temptotalBruttoAmount = decimal.Round(Convert.ToDecimal(belopp.Substring(0, belLength).Replace('.', ',')), 2);
                    }
                    belopp = inputRow[11].Replace(',', ' ');
                    belLength = belopp.Length;
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        invoiceFee = decimal.Round(Convert.ToDecimal(belopp.Substring(0, belLength).Replace('.', ',')), 2);
                    }
                    belopp = inputRow[10].Replace(',', ' ');
                    belLength = belopp.Length;
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        invoiceTotal = decimal.Round(Convert.ToDecimal(belopp.Substring(0, belLength).Replace('.', ',')), 2);
                    }
                    string invoiceStatus = inputRow[2];
                    string invoiceDate =  inputRow[4];
                    string dueDate =  inputRow[5];
                    //string voucherDate;
                    // fakturatyp line.Substring(52, 2
                    string currencyCode =  inputRow[6];
                    decimal currencyRate = 0;
                    belopp =  inputRow[7].Replace(',', ' ');
                    belLength = belopp.Length;
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        currencyRate = decimal.Round(Convert.ToDecimal(belopp.Substring(0, belLength).Replace('.', ',')), 4);
                    }
                    //string PaymentConditionCode = line.Substring(533, 3);
                    //string vatRateCode = line.Substring(539, 1);
                    //decimal vatRate = 0;
                    //if (vatRateCode == "1")
                    //{
                    //    vatRate = 25;
                    //}
                    //else if (vatRateCode == "2")
                    //{
                    //    vatRate = 12;
                    //}
                    //else if (vatRateCode == "3")
                    //{
                    //    vatRate = 6;
                    //}
                    if (invoiceDate != string.Empty)
                    {
                        invoiceDate = "20" + invoiceDate;
                    }
                    if (dueDate != string.Empty)
                    {
                        dueDate = "20" + dueDate;
                    }
                    //if (voucherDate != string.Empty)
                    //{
                    //    voucherDate = "20" + voucherDate;
                    //}
                    // );
                    //string vat25NettoAmount = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                    //decimal tempVatAmount25 = 0;
                    decimal vatAmount = 0;
                    //tempVatAmount25 = inputRow[9] != string.Empty ? Convert.ToDecimal(inputRow[9]) : 0;
                    belopp = inputRow[9].Replace(',', ' ');
                    belLength = belopp.Length;
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        vatAmount = decimal.Round(Convert.ToDecimal(belopp.Substring(0, belLength).Replace('.', ',')), 2);
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

                    decimal totBruttoAmount = decimal.Round((temptotalBruttoAmount + vatAmount + invoiceFee), 2);
                    centRounding = decimal.Round((invoiceTotal - totBruttoAmount), 2);
                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                    }

                    //string OCRCode = line.Substring(206, 30);
                    //string customerName = line.Substring(236, 30);
                    //string customerOrgNo = line.Substring(482, 10);
                    //customerAdressCo = line.Substring(356, 30);
                    //customerAdress1 = line.Substring(386, 30);
                    //customerPostnr = line.Substring(416, 6);
                    //customerPostAdress = line.Substring(422, 30);

                    customerInvoice = new XElement("CustomerInvoice");

                    customerInvoice.Add(
                        new XElement("CustomerNr", customerNr),
                        //new XElement("CustomerName", customerName),
                        //new XElement("CustomerAdressCo", customerAdressCo),
                        //new XElement("CustomerAdress1", customerAdress1),
                        //new XElement("CustomerPostnr", customerPostnr),
                        //new XElement("CustomerPostAdress", customerPostAdress),
                        //new XElement("CustomerOrgNo", customerOrgNo),
                        new XElement("CustomerInvoiceNr", invoiceNunber),
                        new XElement("CustomerInvoiceNr1", invoiceNunber),
                        //new XElement("InvoicePaymentConditionCode", PaymentConditionCode),
                        //new XElement("InvoiceOcrCode", OCRCode),
                        new XElement("InvoiceStatus", invoiceStatus),
                        new XElement("InvoiceDate", invoiceDate),
                        new XElement("DueDate", dueDate),
                        //new XElement("VoucherDate", voucherDate),
                        new XElement("Currency", currencyCode),
                        new XElement("CurrencyRate", currencyRate),
                        new XElement("VATAmount", vatAmount.ToString()),
                        new XElement("VATAmountCurrency", vatAmount.ToString()),
                        //new XElement("VatRate1", vatRate),
                        new XElement("VatAmount1", vatAmount.ToString()),
                        new XElement("TempTotalAmount", temptotalBruttoAmount.ToString()),
                        new XElement("TotalAmount", invoiceTotal.ToString()),
                        new XElement("BillingType", billingType),
                        new XElement("InvoiceFee", invoiceFee.ToString()),
                        new XElement("CentRounding", centRounding.ToString()),
                        new XElement("TotalAmountCurrency", temptotalBruttoAmount.ToString()));
                }
                else
                {
                    invoiceNumberRow++;

                    string konto =  inputRow[0];
                    string kst =  inputRow[1];
                    string proj = inputRow[2];
                    decimal belopp = 0;
                    string bel =  inputRow[3];
                    int belLength = bel.Length;
                    if (!String.IsNullOrWhiteSpace(bel))
                    {
                        belopp = decimal.Round(Convert.ToDecimal(bel.Substring(0, belLength).Replace('.', ',')), 2);
                    }
                    else
                    {
                        continue;
                    }
                    //decimal quant = 0;
                    //string kvant = line.Substring(57, 17);
                    //if (!String.IsNullOrWhiteSpace(kvant))
                    //{
                    //    quant = decimal.Round(Convert.ToDecimal(line.Substring(57, 17).Replace('.', ',')), 2);
                    //}
                    //string text = line.Substring(44, 25);

                    if (belopp != 0)
                    {
                        XElement row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", belopp.ToString()),
                            //new XElement("Text", text.Trim()),
                            new XElement("AccountNr", konto.Trim()),
                            new XElement("CostplaceNr", kst.Trim()),
                            new XElement("ProjectNr", proj.Trim())
                            //new XElement("Quantity", quant.ToString())
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
