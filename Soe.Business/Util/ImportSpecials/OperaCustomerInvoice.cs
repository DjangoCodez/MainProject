using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class OperaCustomerinvoice
    {
        public string ApplyOperaCustomerInvoiceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            AccountManager accountManager = new AccountManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';
            var defaultCreditAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerSalesVat, 0, actorCompanyId, 0);
            var defaultCreditaccount = accountManager.GetAccount(actorCompanyId, defaultCreditAccountId);
            var defaultNoVatAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerSalesNoVat, 0, actorCompanyId, 0);
            var defaultNoVatAccount = accountManager.GetAccount(actorCompanyId, defaultNoVatAccountId);
            var defaultVatAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);
            var defaultVatAccount = accountManager.GetAccount(actorCompanyId, defaultVatAccountId);

            if (defaultCreditaccount == null) {
                throw new ActionFailedException("Konto saknas: Försäljning - momspliktig");
            }
            if (defaultVatAccount == null)
            {
                throw new ActionFailedException("Konto saknas: Försäljning - momsfri");
            }
            if (defaultNoVatAccount == null)
            {
                throw new ActionFailedException("Konto saknas: Utgående moms 1");
            }

            XElement customerInvoicesHeadElement = new XElement("CustomerInvoices");

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

                string[] inputRow = line.Split(delimiter);

                //string invoiceType = line.Substring(0, 4);

                if (customerInvoice != null)
                {
                    invoiceElements.Add(customerInvoice);
                    customerInvoice = null;
                }
                
                decimal vatAmount = 0;
                
                //string customerNr = inputRow[0]?.Trim().Replace(".", "") ?? string.Empty;
                //// a 0 customerNr
                //customerNr = customerNr.TrimStart('0').Trim();
                //// b 1 customerName
                //string customerName = inputRow[1]?.Trim().Replace(".", "") ?? string.Empty;
                //// c 2 customerAdressCo
                //string customerAdressCo = inputRow[2]?.Trim().Replace(".", "") ?? string.Empty;
                //// d  3 customerAdress1
                //string customerAdress1 = inputRow[3]?.Trim().Replace(".", "") ?? string.Empty;
                //// e  4 customerPostnr
                //string customerPostnr = inputRow[4]?.Trim().Replace(".", "") ?? string.Empty;
                //// f  5 customerPostAdress
                //string customerPostAdress = inputRow[5]?.Trim().Replace(".", "") ?? string.Empty;
                //// g  6 customerLand
                //// h  7 customerTelnr

                //// j 9 betalvillkor
                //string PaymentConditionCode = inputRow[9]?.Trim().Replace(".", "") ?? string.Empty;
                //// k 10 invoiceNumber
                //string invoiceNumber = inputRow[10]?.Trim().Replace(".", "") ?? string.Empty;
                                
                //// l  11 invoiceDate
                //string invoiceDate = inputRow[11]?.Trim().Replace(".", "") ?? string.Empty;
                
                //// m  12 dueDate
                //string dueDate = inputRow[12]?.Trim().Replace(".", "") ?? string.Empty;
                
                //// n  13 kundfodran
                //string kundfodran = inputRow[13]?.Trim().Replace(".", "") ?? string.Empty;
                //// o  14 belopp
                //decimal temptotalBruttoAmount = 0;
                //string belopp = inputRow[14]?.Trim().Replace(".", ",") ?? string.Empty;
                string customerNr = inputRow[0]?.Trim().Replace(".", "") ?? string.Empty;
                // 0 customerNr
                customerNr = customerNr.TrimStart('0').Trim();
                // 1 customerName
                string customerName = inputRow[1]?.Trim().Replace(".", "") ?? string.Empty;
                // 2 customerAdressCo
                string customerAdressCo = inputRow[2]?.Trim().Replace(".", "") ?? string.Empty;
                // 3 customerAdress1
                string customerAdress1 = inputRow[3]?.Trim().Replace(".", "") ?? string.Empty;
                // 4 customerPostnr
                string customerPostnr = inputRow[4]?.Trim().Replace(".", "") ?? string.Empty;
                // 5 customerPostAdress
                string customerPostAdress = inputRow[5]?.Trim().Replace(".", "") ?? string.Empty;
                // 6 customerLand
                // 7 customerTelnr

                // 9 betalvillkor
                string PaymentConditionCode = inputRow[9]?.Trim().Replace(".", "") ?? string.Empty;
                // 10 invoiceNumber
                string invoiceNumber = inputRow[10]?.Trim().Replace(".", "") ?? string.Empty;

                // 11 invoiceDate
                string invoiceDate = inputRow[11]?.Trim().Replace(".", "") ?? string.Empty;

                // 12 dueDate
                string dueDate = inputRow[12]?.Trim().Replace(".", "") ?? string.Empty;

                // 13 kundfodran
                string kundfodran = inputRow[13]?.Trim().Replace(".", "") ?? string.Empty;

                decimal temptotalBruttoAmount = 0;
                string belopp = inputRow[14]?.Trim().Replace(".", ",") ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(belopp))
                {
                    temptotalBruttoAmount = decimal.Round(Convert.ToDecimal(belopp.Replace('.', ',')), 2);
                }

                string billingType;
                if (temptotalBruttoAmount > 0)
                {
                    billingType = "1";
                }
                else
                {
                    billingType = "2";
                }
                decimal totBruttoAmount = decimal.Round((temptotalBruttoAmount), 2);
                if (customerInvoice != null)
                {
                    invoiceElements.Add(customerInvoice);
                }

                customerInvoice = new XElement("CustomerInvoice");

                customerInvoice.Add(
                    new XElement("CustomerNr", customerNr),
                    new XElement("CustomerName", customerName),
                    new XElement("CustomerAdressCo", " "),
                    new XElement("CustomerAdress1", customerAdress1),
                    new XElement("CustomerPostnr", customerPostnr),
                    new XElement("CustomerPostAdress", customerPostAdress),
                    //new XElement("CustomerOrgNo", customerOrgNo),
                    //new XElement("CustomerInvoiceNr", ""),
                    new XElement("CustomerInvoiceNr1", invoiceNumber),
                    new XElement("InvoicePaymentConditionCode", PaymentConditionCode),
                    //new XElement("InvoiceOcrCode", OCRCode),
                    new XElement("InvoiceDate", invoiceDate),
                    new XElement("CurrencyDate", invoiceDate),
                    new XElement("DueDate", dueDate),
                    //new XElement("VoucherDate", voucherDate),
                    //new XElement("Currency", currencyCode),
                    //new XElement("CurrencyRate", currencyRate),
                    new XElement("VATAmount", vatAmount.ToString()),
                    new XElement("VATAmountCurrency", vatAmount.ToString()),
                    //new XElement("VatRate1", vatRate),
                    new XElement("VatAmount1", vatAmount.ToString()),
                    new XElement("TempTotalAmount", temptotalBruttoAmount.ToString()),
                    new XElement("TotalAmount", totBruttoAmount.ToString()),
                    new XElement("BillingType", billingType),
                    new XElement("TotalAmountCurrency", temptotalBruttoAmount.ToString()));

                int invoiceNumberRow = 1;

                if (temptotalBruttoAmount != 0)
                {
                    XElement row = new XElement("InvoiceAccountRow");
                    row.Add(
                        new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                        new XElement("Amount", temptotalBruttoAmount.ToString()),
                        //new XElement("Text", text.Trim()),
                        new XElement("AccountNr", kundfodran.Trim()));
                    customerInvoice.Add(row);
                    if (vatAmount !=0)
                    {
                        invoiceNumberRow++;
                        row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", vatAmount.ToString()),
                            //new XElement("Text", text.Trim()),
                            new XElement("AccountNr", defaultVatAccount.AccountNr.ToString()));
                        customerInvoice.Add(row);
                    }
                    temptotalBruttoAmount = temptotalBruttoAmount - vatAmount;
                    temptotalBruttoAmount = temptotalBruttoAmount * -1;
                    if (vatAmount >= 0)
                    {
                        invoiceNumberRow++;
                        row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", temptotalBruttoAmount.ToString()),
                            //new XElement("Text", text.Trim()),
                            new XElement("AccountNr", defaultCreditaccount.AccountNr.ToString()));
                        customerInvoice.Add(row);
                    }
                    else
                    {
                        invoiceNumberRow++;
                        row = new XElement("InvoiceAccountRow");
                        row.Add(
                            new XElement("CustomerInvoiceRowNr", invoiceNumberRow.ToString()),
                            new XElement("Amount", temptotalBruttoAmount.ToString()),
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

            var modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;
        }

 
     
 

 

        

    }
}
