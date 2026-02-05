using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class InvitePeople
    {
        public string ApplyInvitePeopleCustomerInvoiceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.UTF8.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                decimal invoiceNr = 0;
                string customerNr = "";
                string customerName = "";
                //        string bllingType = "";
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
                string lineAccount = "";
                string lineVATAccount = "";
                string lineAccountSieDim1 = "";
                decimal currencyRate = 0;
                //         string paymentCondition = "";
                decimal quantity = 0;
                //           string unit = "";
                //           string vatCode = "";
                //           int vatProcent = 0;
                decimal sumAmount = 0;
                decimal lineAmount = 0;
                decimal linePrice = 0;
                decimal lineTaxExclusivePrice = 0;
                decimal lineTaxExclusiveAmount = 0;
                decimal lineTaxAmount = 0;
                decimal lineTaxPercent = 0;
                decimal headAmount = 0;
                decimal headVat = 0;
                //       string accountNr = "";
                string productNr = "0";
                //           string accountSieDim1 = "";
                string rowText = "";
                string rowTextLong = "";
                string rowText1 = "";
                string rowText2 = "";
                string rowText3 = "";
                int lineCount = 0;
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

                    line = line.Replace("\"", "");

                    string[] inputRow = line.Split(delimiter);

                    lineCount++;
                    int arrCount = inputRow.Length;
                    if (arrCount < 33)
                    {
                        arrCount = inputRow.Length;
                    }

                    if (inputRow[0] == "ID") continue;


                    var prevInvoiceNr = invoiceNr;


                    var prevCustomerNr = customerNr;
                    var prevCustomerNamne = customerName;
                    var prevInvoiceRemark = invoiceRemark;
                    //        prevInvoiceOrderNo = invoiceOrderNo; 

                    customerNr = inputRow[4] != null ? inputRow[4].Trim() : string.Empty;
                    customerName = inputRow[5] != null ? inputRow[5].Trim() : string.Empty;
                    invoiceRemark = inputRow[23] != null ? inputRow[23].Trim() : string.Empty;
                    //        bllingType = inputRow[1] != null ? inputRow[1] : string.Empty;
                    decimal.TryParse(inputRow[0].Replace(".", ""), out invoiceNr);

                    if (prevInvoiceNr != invoiceNr)
                    {
                        if (!(prevInvoiceNr == 0))
                        {
                            //   customerInvoice.Add(customerInvoiceRow);                       
                            customerInvoice.Add(
                                new XElement("KundId", prevCustomerNr),
                                new XElement("KundOrgnr", prevCustomerNr),
                                new XElement("Kundnamn", prevCustomerNamne),
                                new XElement("FakturaNr", prevInvoiceNr.ToString()),
                                //                   new XElement("Fakturatyp", bllingType),
                                new XElement("ReferenceOur", invoiceOurRef),
                                new XElement("ReferenceYour", invoiceYourRef),
                                new XElement("LevadressNamn", deliveryAddressName),
                                new XElement("LevadressAdress", deliveryAddressAddress),
                                new XElement("LevadressPostnr", deliveryAddressPostnr),
                                new XElement("LevadressPostadress", deliveryAddressCity.TrimStart()),
                                new XElement("LevadressLand", deliveryAddressCountry),
                                new XElement("Valutakod", currencyCode),
                                new XElement("Valutakurs", currencyRate),
                                new XElement("Label", "Beställningsnr:" + prevInvoiceRemark),
                                new XElement("Fakturadatum", invoiceDate),
                                new XElement("FakturabeloppExklusive", headAmount),
                                new XElement("Momsbelopp", headVat));
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
                            customerInvoice.Add(customerInvoiceRow);
                            customerInvoiceRow = new XElement("Fakturarad");
                            rowText1 = "";
                        }
                    }
                    DateTime startDatum = DateTime.Today;
                    decimal.TryParse(inputRow[2].Replace(".", ""), out invoiceDate);
                    DateTime.TryParse(inputRow[2], out startDatum);
                    //        invoiceDate = invoiceDate + 20000000;
                    invoiceYourRef = inputRow[8] != null ? inputRow[8] : string.Empty;
                    deliveryAddressName = inputRow[9] != null ? inputRow[9] : string.Empty;
                    if (deliveryAddressName.Length > 100)
                    {
                        deliveryAddressName = deliveryAddressName.Substring(0, 100);
                    }
                    deliveryAddressAddress = inputRow[10] != null ? inputRow[10] : string.Empty;
                    if (deliveryAddressAddress.Length > 100)
                    {
                        deliveryAddressAddress = deliveryAddressAddress.Substring(0, 100);
                    }
                    deliveryAddressPostnr = inputRow[12] != null ? inputRow[12] : string.Empty;
                    deliveryAddressCity = inputRow[11] != null ? inputRow[11] : string.Empty;
                    deliveryAddressCountry = inputRow[13] != null ? inputRow[13] : string.Empty;
                    currencyCode = inputRow[17] != null ? inputRow[17].Replace(".", "") : string.Empty;
                    headVat = NumberUtility.ToDecimalWithComma(inputRow[20], 2);
                    //        decimal.TryParse(inputRow[10].Replace(".", ""), out currencyRate);
                    //        paymentCondition = inputRow[18] != null ? inputRow[18].Replace(".", "") : string.Empty;


                    quantity = 0;
                    decimal.TryParse(inputRow[22].Replace(".", ""), out quantity);
                    //           unit = inputRow[13] != null ? inputRow[13] : string.Empty;
                    //           vatCode = inputRow[14] != null ? inputRow[14] : string.Empty;
                    sumAmount = 0;
                    sumAmount = NumberUtility.ToDecimalWithComma(inputRow[18], 2);
                    linePrice = NumberUtility.ToDecimalWithComma(inputRow[18], 2);
                    lineTaxExclusivePrice = NumberUtility.ToDecimalWithComma(inputRow[25], 2);
                    lineTaxPercent = NumberUtility.ToDecimalWithComma(inputRow[26], 2);
                    lineAmount = NumberUtility.ToDecimalWithComma(inputRow[27], 2);
                    lineTaxExclusiveAmount = NumberUtility.ToDecimalWithComma(inputRow[28], 2);
                    lineTaxAmount = NumberUtility.ToDecimalWithComma(inputRow[29], 2);
                    if (lineTaxPercent == 25)
                    {
                        lineAccount = "3010";
                        lineVATAccount = "2610";
                        lineAccountSieDim1 = "20100";
                    }
                    if (lineTaxPercent == 12)
                    {
                        lineAccount = "3042";
                        lineVATAccount = "2620";
                        lineAccountSieDim1 = "20100";
                    }

                    rowText1 = inputRow[30] != null ? inputRow[30] : string.Empty;
                    rowText2 = inputRow[31] != null ? inputRow[31] : string.Empty;
                    rowText3 = inputRow[32] != null ? inputRow[32] : string.Empty;
                    rowTextLong = rowText1.TrimEnd() + " " + rowText2.TrimEnd() + " " + rowText3.TrimEnd();
                    if (rowTextLong.Length > 100)
                    {
                        rowText = rowTextLong.Substring(0, 100);
                    }
                    else
                    {
                        rowText = rowTextLong;
                    }
                    rowText = rowText1;
                    decimal rowAmount = quantity * sumAmount;
                    decimal rowAmountExklusive = quantity * lineTaxExclusiveAmount;
                    headAmount += rowAmountExklusive;

                    customerInvoiceRow.Add(
                           new XElement("Artikelid", productNr),
                           //                  new XElement("Artikelid", 999),
                           new XElement("Namn", rowText),
                           new XElement("Text", rowTextLong),
                           new XElement("Antal", quantity),
                           new XElement("Pris", lineTaxExclusivePrice),
                           new XElement("Belopp", lineAmount),
                           new XElement("MomsProcent", lineTaxPercent),
                           new XElement("MomsKonto", lineVATAccount),
                           new XElement("Konto", lineAccount),
                           new XElement("Kst", lineAccountSieDim1),

                           new XElement("Radtyp", 2));

                }

                customerInvoice.Add(
                              new XElement("KundId", customerNr),
                              new XElement("KundOrgnr", customerNr),
                              new XElement("Kundnamn", customerName),
                               new XElement("FakturaNr", invoiceNr.ToString()),
                               //                      new XElement("Fakturatyp", bllingType),
                               new XElement("ReferenceOur", invoiceOurRef),
                               new XElement("ReferenceYour", invoiceYourRef),
                               new XElement("LevadressNamn", deliveryAddressName),
                               new XElement("LevadressAdress", deliveryAddressAddress),
                               new XElement("LevadressPostnr", deliveryAddressPostnr),
                               new XElement("LevadressPostadress", deliveryAddressCity.TrimStart()),
                               new XElement("LevadressLand", deliveryAddressCountry),
                               new XElement("Valutakod", currencyCode),
                               new XElement("Valutakurs", currencyRate),
                               new XElement("Label", "Beställningsnr:" + invoiceRemark),
                               new XElement("Fakturadatum", invoiceDate),
                               //                       new XElement("Betalningsvillkor", paymentCondition),
                               new XElement("FakturabeloppExklusive", headAmount),
                               new XElement("Momsbelopp", headVat));
                customerInvoice.Add(customerInvoiceRow);

                customerInvoices.Add(customerInvoice);

                customerInvoicesHeadElement.Add(customerInvoices);

                modifiedContent = customerInvoicesHeadElement.ToString();
            }
            return modifiedContent;

        }

    }
}