using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Ksab
    {
        public string ApplyKsabCustomerInvoiceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

           string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            decimal invoiceNr = 0;
            string customerNr = "";
            string bllingType = "";
            decimal invoiceDate = 0;
            string invoiceYourRef = "";
            string invoiceOurRef = "";
            string invoiceRemark = "";
            string deliveryAddressName = "";
            string deliveryAddressAddress = "";
            string deliveryAddressPostnr = "";
            string deliveryAddressCity = "";
            string deliveryAddressCountry = "";
            string currencyCode = "";
            decimal currencyRate = 0;
            string paymentCondition = "";
             decimal quantity = 0;
            string unit = "";
            string vatCode = "";
            int vatProcent = 0;
            decimal sumAmount = 0;
            decimal headAmount = 0;
            decimal headVat = 0;
            string accountNr = "";
            string productNr = "";
            string accountSieDim1 = "";
            string rowText1 = "";
//            string rowPriceText = " ";
            string line;

            XElement customerInvoiceRow = new XElement("Fakturarad");
            XElement customerInvoicesHeadElement = new XElement("Kundfakturor");
            List<XElement> customerInvoices = new List<XElement>();

            XElement customerInvoice = new XElement("Kundfaktura");
            List<XElement> invoiceElements = new List<XElement>();

            int VatCode25AccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);
            int VatCode12AccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable2, 0, actorCompanyId, 0);

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" || !line.Contains(";")) continue;

                string[] inputRow = line.Split(delimiter);

                var prevInvoiceNr = invoiceNr;

                // Format for KSAB customerinvoice importfile 20210514

                productNr = inputRow[17] != null ? inputRow[17] : string.Empty;

                if (string.IsNullOrEmpty(productNr) || productNr == "            ")
                {
                    continue;
                }

                var prevCustomerNr = customerNr;
        //        prevInvoiceOrderNo = invoiceOrderNo; 
                
                customerNr = inputRow[0] != null ? inputRow[0].Trim() : string.Empty;
                bllingType = inputRow[1] != null ? inputRow[1] : string.Empty;
                decimal.TryParse(inputRow[2].Replace(".", ""), out invoiceNr);

                if (prevInvoiceNr != invoiceNr)
                {
                    if (!(prevInvoiceNr == 0))
                    {
                        //   customerInvoice.Add(customerInvoiceRow);                       
                        customerInvoice.Add(
                            new XElement("KundId", prevCustomerNr),
                            new XElement("FakturaNr", prevInvoiceNr.ToString()),
                            new XElement("Fakturatyp", bllingType),
                            new XElement("ReferenceOur", invoiceOurRef),
                            new XElement("ReferenceYour", invoiceYourRef),
                            new XElement("LevadressNamn", deliveryAddressName),
                            new XElement("LevadressAdress", deliveryAddressAddress),
                            new XElement("LevadressPostnr", deliveryAddressPostnr),
                            new XElement("LevadressPostadress", deliveryAddressCity.TrimStart()),
                            new XElement("LevadressLand", deliveryAddressCountry),
                            new XElement("Valutakod", currencyCode),
                            new XElement("Valutakurs", currencyRate),
                            new XElement("Label", invoiceRemark),
                            new XElement("Fakturadatum", invoiceDate),
                            new XElement("Betalningsvillkor", paymentCondition),
                            new XElement("FakturabeloppExklusive", headAmount),
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
                    if (!(prevInvoiceNr == 0))
                    {

                        customerInvoiceRow.Add(
                        new XElement("Namn", rowText1.TrimEnd()),
                        new XElement("Text", rowText1.TrimEnd()));
                        customerInvoice.Add(customerInvoiceRow);
                        customerInvoiceRow = new XElement("Fakturarad");
                        rowText1 = "";
                    }
                }
                decimal.TryParse(inputRow[3].Replace(".", ""), out invoiceDate);
                invoiceDate = invoiceDate + 20000000;
                deliveryAddressName = inputRow[4] != null ? inputRow[4] : string.Empty;
                deliveryAddressAddress = inputRow[5] != null ? inputRow[5] : string.Empty;
                deliveryAddressPostnr = inputRow[6] != null ? inputRow[6] : string.Empty;
                deliveryAddressCity = inputRow[7] != null ? inputRow[7] : string.Empty;
                deliveryAddressCountry = inputRow[8] != null ? inputRow[8] : string.Empty;
                currencyCode = inputRow[9] != null ? inputRow[9].Replace(".", "") : string.Empty;
                decimal.TryParse(inputRow[10].Replace(".", ""), out currencyRate);
                paymentCondition = inputRow[18] != null ? inputRow[18].Replace(".", "") : string.Empty;

                rowText1 = inputRow[11] != null ? inputRow[11] : string.Empty;
                quantity = 0;
                decimal.TryParse(inputRow[12].Replace(".", ""), out quantity);
                unit = inputRow[13] != null ? inputRow[13] : string.Empty;
                vatCode = inputRow[14] != null ? inputRow[14] : string.Empty;
                sumAmount = 0;
                decimal.TryParse(inputRow[15].Replace(".", ""), out sumAmount);
                sumAmount = sumAmount / 100;
                accountNr = inputRow[16] != null ? inputRow[16] : string.Empty;
                productNr = inputRow[17] != null ? inputRow[17] : string.Empty;
                accountSieDim1 = inputRow[19] != null ? inputRow[19] : string.Empty;
                decimal rowAmount = quantity * sumAmount;
                headAmount += rowAmount;

                if (vatCode == "4")
                {
                    vatProcent = 0;
                }
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

                customerInvoiceRow.Add(
                       new XElement("Artikelid", productNr),
                       new XElement("Antal", quantity),
                       new XElement("Pris", sumAmount),
                       new XElement("Belopp", rowAmount),
                       new XElement("Konto", accountNr),
                       new XElement("Kst", accountSieDim1),
                       new XElement("MomsProcent", vatProcent),
                       new XElement("Radtyp", 2));

             }

            customerInvoice.Add(
                          new XElement("KundId", customerNr),
                           new XElement("FakturaNr", invoiceNr.ToString()),
                           new XElement("Fakturatyp", bllingType),
                           new XElement("ReferenceOur", invoiceOurRef),
                           new XElement("ReferenceYour", invoiceYourRef),
                           new XElement("LevadressNamn", deliveryAddressName),
                           new XElement("LevadressAdress", deliveryAddressAddress),
                           new XElement("LevadressPostnr", deliveryAddressPostnr),
                           new XElement("LevadressPostadress", deliveryAddressCity.TrimStart()),
                           new XElement("LevadressLand", deliveryAddressCountry),
                           new XElement("Valutakod", currencyCode),
                           new XElement("Valutakurs", currencyRate),
                           new XElement("Label", invoiceRemark),
                           new XElement("Fakturadatum", invoiceDate),
                           new XElement("Betalningsvillkor", paymentCondition),
                           new XElement("FakturabeloppExklusive", headAmount),
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
