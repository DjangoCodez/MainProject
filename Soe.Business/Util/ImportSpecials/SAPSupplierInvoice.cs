using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SAPSupplierinvoice
    {
        public string ApplySAPSupplierInvoiceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            XElement supplierInvoicesHeadElement = new XElement("SupplierInvoices");
            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            decimal invoiceNumberRow = 0;
            string line;
            List<XElement> invoiceElements = new List<XElement>();
            XElement supplierInvoice = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                string[] inputRow = line.Split(delimiter);

                //    XElement supplierInvoice = new XElement("SupplierInvoice");
                string invoiceType = line.Substring(0, 4);

                if (invoiceType == "FAKT")
                {
                    if (supplierInvoice != null)
                    {
                        invoiceElements.Add(supplierInvoice);
                        supplierInvoice = null;
                        invoiceNumberRow = 0;
                    }
                    invoiceNumberRow = 0;
                    string invoiceNunber = line.Substring(119, 25);
                    //remove leading zeros from invoiceNunber
                    invoiceNunber = invoiceNunber.TrimStart('0');
                    invoiceNunber = invoiceNunber.Trim();
                    string supplierNr = line.Substring(144, 12);
                    //remove leading zeros from supplier number
                    supplierNr = supplierNr.TrimStart('0');
                    supplierNr = supplierNr.Trim();
                    decimal temptotalBruttoAmount = 0;
                    decimal cashRegAmount = 0;
                    string belopp = line.Substring(29, 16).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        temptotalBruttoAmount = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    belopp = line.Substring(100, 7).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        cashRegAmount = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    string invoiceDate = line.Substring(15, 6);
                    string dueDate = line.Substring(22, 6);
                    string cashregDate = line.Substring(94, 6);
                    string regDate = line.Substring(168, 6);
                    string attest = line.Substring(45, 1);
                    string attestSign = line.Substring(46, 30);
                    if (invoiceDate != string.Empty)
                    {
                        invoiceDate = "20" + invoiceDate;
                    }
                    if (dueDate != string.Empty)
                    {
                        dueDate = "20" + dueDate;
                    }
                    if (cashregDate != string.Empty)
                    {
                        cashregDate = "20" + cashregDate;
                    }
                    if (regDate != string.Empty)
                    {
                        regDate = "20" + regDate;
                    }
                    //string currencyCode = line.Substring(55, 3);
                    //decimal currencyRate = 0;
                    //currencyRate = decimal.Round(Convert.ToDecimal(line.Substring(577, 7).Replace('.', ',')), 4);
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

                    // );
                    //string vat25NettoAmount = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                    //decimal tempVatAmount25 = 0;
                    decimal vatAmount = 0;
                    belopp = line.Substring(156, 12).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        vatAmount = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    //vatAmount = tempVatAmount25 * -1;
                    string billingType = line.Substring(6, 1);

                    if (supplierInvoice != null)
                    {
                        invoiceElements.Add(supplierInvoice);
                    }

                    //string OCRCode = line.Substring(206, 30);
                    //  string supplierOrgNo = line.Substring(482, 10);

                    supplierInvoice = new XElement("SupplierInvoice");

                    supplierInvoice.Add(
                        new XElement("SupplierNr", supplierNr),
                        new XElement("SupplierInvoiceNr", invoiceNunber),
                        new XElement("SupplierInvoiceNr1", invoiceNunber),
                        new XElement("InvoiceDate", invoiceDate),
                        new XElement("DueDate", dueDate),
                        new XElement("CashregDate", cashregDate),
                        new XElement("CashRegAmount", cashRegAmount),
                        new XElement("RegDate", regDate),
                        new XElement("Attest", attest),
                        new XElement("AttestSign", attestSign),
                        new XElement("VATAmount", vatAmount.ToString()),
                        new XElement("TotalAmount", temptotalBruttoAmount.ToString()),
                        new XElement("BillingType", billingType),
                        new XElement("TotalAmountCurrency", temptotalBruttoAmount.ToString()));
                }
                else if (invoiceType == "KONT")
                {
                    invoiceNumberRow++;

                    string konto = line.Substring(5, 6);
                    string kst = line.Substring(11, 6);
                    string proj = line.Substring(19, 6);
                    string bel = line.Substring(27, 16).Replace(",", "");
                    decimal belopp = 0;
                    if (!String.IsNullOrWhiteSpace(bel))
                    {
                        belopp = decimal.Round(Convert.ToDecimal(bel, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    else
                    {
                        continue;
                    }
                    decimal quant = 0;
                    bel = line.Substring(44, 17).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(bel))
                    {
                        quant = decimal.Round(Convert.ToDecimal(bel, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    string text = line.Substring(44, 25);

                    if (belopp != 0)
                    {
                        XElement row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("SupplierInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", belopp.ToString()),
                            new XElement("Text", text.Trim()),
                            new XElement("AccountNr", konto.Trim()),
                            new XElement("CostplaceNr", kst.Trim()),
                            new XElement("ProjectNr", proj.Trim()),
                            new XElement("Quantity", quant.ToString())
                            );
                        supplierInvoice.Add(row);
                    }
                }

            }

            if (supplierInvoice != null)
            {
                invoiceElements.Add(supplierInvoice);
            }
            supplierInvoicesHeadElement.Add(invoiceElements);

            modifiedContent = supplierInvoicesHeadElement.ToString();

            return modifiedContent;

        }
    }
}
