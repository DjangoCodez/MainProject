using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SEBACustomerinvoice
    {
        // Import from SEBA (Used by Kungsörs Grus AB)
        public string ApplySEBACustomerInvoiceSpecialModification(string content, int actorCompanyId, ParameterObject parameterObject)
        {
            //New Entities with new extended timeout
            using (CompEntities entities = new CompEntities())
            {
            //    var pm = new ProductManager(parameterObject);
            //    var pgm = new ProductGroupManager(parameterObject);
                var sm = new SettingManager(parameterObject);
                var am = new AccountManager(parameterObject);
             //   var stm = new StockManager(parameterObject);
                char[] delimiter = new char[1];
                delimiter[0] = ';';
                var defaultCreditAccountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerSalesVat, 0, actorCompanyId, 0);
                var defaultDebitAccountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerClaim, 0, actorCompanyId, 0);
                var defaultDebitAccount = am.GetAccount(actorCompanyId, defaultDebitAccountId);
                var defaultVatAccountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);
                var defaultCreditaccount = am.GetAccount(actorCompanyId, defaultCreditAccountId);
                var defaultNoVatAccountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerSalesNoVat, 0, actorCompanyId, 0);
                var defaultNoVatAccount = am.GetAccount(actorCompanyId, defaultNoVatAccountId);
                var defaultVatAccount = am.GetAccount(actorCompanyId, defaultVatAccountId);

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
                    if (line == "") continue;

                    //    string[] inputRow = line.Split(delimiter);

                    //    XElement customerInvoice = new XElement("CustomerInvoice");

                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                        customerInvoice = null;

                    }
                    string customerAdress1 = "";
                    string invoiceNumber = line.Substring(0, 6);
                    invoiceNumber = invoiceNumber.Trim();
                    string customerNr = line.Substring(7, 8);
                    customerNr = customerNr.Trim();
                    string customerName = line.Substring(15, 30);
                    customerName = customerName.Trim();
                    string customerAdressCo = line.Substring(45, 30);
                    customerAdressCo = customerAdressCo.Trim();
                    string customerPostnr = line.Substring(88, 6);
                    customerPostnr = customerPostnr.Trim();
                    string customerPostAdress = line.Substring(94, 20);
                    customerPostAdress = customerPostAdress.Trim();
                    decimal temptotalBruttoAmount = 0;
                    string belopp = line.Substring(114, 12).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        temptotalBruttoAmount = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
                    string invoiceDate = line.Substring(132, 6);
                    string dueDate = line.Substring(138, 6);
                    string voucherDate = invoiceDate;
                    // fakturatyp line.Substring(52, 2
                    //string currencyCode = line.Substring(55, 3);
                    //decimal currencyRate = 0;
                    //belopp = line.Substring(576, 7).Replace(",", "");
                    //if (!String.IsNullOrWhiteSpace(belopp))
                    //{
                    // currencyRate = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 4);
                    //}
                    string PaymentConditionCode = "30";
                    //string vatRateCode = line.Substring(539, 1);
                    //decimal vatRate = line.Substring(126, 2) != string.Empty ? Convert.ToDecimal(line.Substring(126, 2)) : 0; ;
                    decimal vatRate = 25;
                    //if (vatRateCode == "1")
                    //{
                    //  vatRate = 25;
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
                    if (voucherDate != string.Empty)
                    {
                        voucherDate = "20" + voucherDate;
                    }
                    Decimal backwardVat6 = (1 / 1.06m);
                    Decimal backwardVat12 = (1 / 1.12m);
                    Decimal backwardVat25 = (1 / 1.25m);

                    backwardVat6 = 1 - backwardVat6;
                    backwardVat12 = 1 - backwardVat12;
                    backwardVat25 = 1 - backwardVat25;

                    //);
                    //string vat25NettoAmount = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                    decimal tempVatAmount25 = 0;
                    decimal vatAmount = 0;
                    //tempVatAmount25 = inputRow[9] != string.Empty ? Convert.ToDecimal(inputRow[9]) : 0;
                    belopp = line.Substring(114, 12).Replace(",", "");
                    if (!String.IsNullOrWhiteSpace(belopp))
                    {
                        tempVatAmount25 = decimal.Round(Convert.ToDecimal(belopp, System.Globalization.CultureInfo.InvariantCulture), 2);
                    }
 
                    vatAmount = tempVatAmount25 * backwardVat25;
                    string billingType;
                    if (temptotalBruttoAmount > 0)
                    {
                        billingType = "1";
                    }
                    else
                    {
                        billingType = "2";
                    }

                    decimal totBruttoAmount = decimal.Round((temptotalBruttoAmount - vatAmount), 2);
                    if (customerInvoice != null)
                    {
                        invoiceElements.Add(customerInvoice);
                    }

                    string customerPhoneNo = line.Substring(168, 12);
                    customerPhoneNo = customerPhoneNo.Trim();
                    string customerContact = line.Substring(198, 30);
                    customerContact = customerContact.Trim();
                    string customerContactName = line.Substring(198, 30);
                    customerContactName = customerContactName.Trim();

                    customerInvoice = new XElement("CustomerInvoice");

                    customerInvoice.Add(
                        new XElement("CustomerNr", customerNr),
                        new XElement("CustomerName", customerName),
                        new XElement("CustomerAdressCo", customerAdressCo),
                        new XElement("CustomerAdress1", customerAdress1),
                        new XElement("CustomerPostnr", customerPostnr),
                        new XElement("CustomerPostAdress", customerPostAdress),
                        new XElement("CustomerInvoiceNr", invoiceNumber),
                        new XElement("CustomerInvoiceNr1", invoiceNumber),
                        new XElement("InvoicePaymentConditionCode", PaymentConditionCode),
                        new XElement("InvoiceDate", invoiceDate),
                        new XElement("DueDate", dueDate),
                        new XElement("VoucherDate", voucherDate),
                        new XElement("VATAmount", vatAmount.ToString()),
                        new XElement("VATAmountCurrency", vatAmount.ToString()),
                        new XElement("VatRate1", vatRate),
                        new XElement("VatRate2", vatRate),
                        new XElement("VatAmount1", vatAmount.ToString()),
                        new XElement("TempTotalAmount", totBruttoAmount.ToString()),
                        new XElement("TotalAmount", temptotalBruttoAmount.ToString()),
                        new XElement("BillingType", billingType),
                        new XElement("TotalAmountCurrency", temptotalBruttoAmount.ToString())
                        );
                    int invoiceNumberRow = 1;

                    if (temptotalBruttoAmount != 0)
                    {
                        XElement row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", temptotalBruttoAmount.ToString()),
                            //new XElement("Text", text.Trim()),
                            new XElement("AccountNr", defaultDebitAccount.AccountNr.Trim()));
                        customerInvoice.Add(row);
                        vatAmount = vatAmount * -1;
                        if (vatAmount != 0)
                        {
                            invoiceNumberRow++;
                            row = new XElement("InvoiceAccountRow");
                            row.Add(
                                new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                                new XElement("Amount", vatAmount.ToString()),
                                //new XElement("Text", text.Trim()),
                                new XElement("AccountNr", defaultVatAccount.AccountNr.Trim()));
                            customerInvoice.Add(row);
                        }
                        //temptotalBruttoAmount = temptotalBruttoAmount + vatAmount;
                        vatAmount = vatAmount * -1;
                        totBruttoAmount = totBruttoAmount * -1;
                        if (vatAmount >= 0)
                        {
                            invoiceNumberRow++;
                             row = new XElement("InvoiceAccountRow");
                            row.Add(
                                new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                                new XElement("Amount", totBruttoAmount.ToString()),
                                //new XElement("Text", text.Trim()),
                                new XElement("AccountNr", defaultCreditaccount.AccountNr.Trim()));
                            customerInvoice.Add(row);
                        }
                        else
                        {
                            invoiceNumberRow++;
                            row = new XElement("InvoiceAccountRow");
                            row.Add(
                                new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                                new XElement("Amount", totBruttoAmount.ToString()),
                                //new XElement("Text", text.Trim()),
                                new XElement("AccountNr", defaultNoVatAccount.AccountNr.ToString()));
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
}
