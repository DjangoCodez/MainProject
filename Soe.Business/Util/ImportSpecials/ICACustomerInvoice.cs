using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class IcaCustomerInvoice
    {
        private const decimal V06 = (1 / 1.06m);
        private const decimal V12 = (1 / 1.12m);
        private const decimal V25 = (1 / 1.25m);

        public string ApplyIcaCustomerInvoiceSpecialModification(string content, int actorCompanyId)
        {
            var customerManager = new CustomerManager(null);
            List<Customer> customers = customerManager.GetCustomersByCompany(actorCompanyId, true);
            //Method to create standard invoice based on ICA:S three different formats
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;

            //First we sort the file based on customer number and put them in a list

            XElement customerInvoicesHeadElement = new XElement("Kundfakturor");

            List<XElement> customerInvoices = new List<XElement>();

            bool hasProductRows = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;

                if (line.Contains(";"))
                {
                    hasProductRows = true;
                    break;
                }

                XElement customerInvoice = new XElement("Kundfaktura");

                //Parse information              
                string customerNumber = line.Substring(12, 4);
                string receiptNumber = line.Substring(16, 10);
                string date = line.Substring(26, 6);
                decimal amount = line.Substring(32, 8) != string.Empty ? Convert.ToDecimal(line.Substring(32, 8)) / 100 : 0;
                bool isCreditRow = (line.Substring(40, 1) != string.Empty ? Convert.ToInt32(line.Substring(40, 1)) : 0) == 0 ? false : true;
                decimal vat = StringUtility.GetDecimal(line.Substring(41, 8)) / 100;
                bool vat25Negative=false;
                if (amount == 0)
                    continue;

                receiptNumber = receiptNumber + " Datum: 20" + date;

                decimal vatFreeAmount = 0;
                decimal vat6 = 0;
                decimal vat12 = 0;
                decimal vat25 = 0;

                Decimal backwardVat6 = (1 / 1.06m);
                Decimal backwardVat12 = (1 / 1.12m);
                Decimal backwardVat25 = (1 / 1.25m);

                backwardVat6 = 1 - backwardVat6;
                backwardVat12 = 1 - backwardVat12;
                backwardVat25 = 1 - backwardVat25;


                if (line.Length != 49)
                {
                    vat25 = line.Substring(49, 8) != string.Empty ? Convert.ToDecimal(line.Substring(49, 8)) / 100 : 0;
                    vat12 = line.Substring(57, 8) != string.Empty ? Convert.ToDecimal(line.Substring(57, 8)) / 100 : 0;
                    vat6 = line.Substring(65, 8) != string.Empty ? Convert.ToDecimal(line.Substring(65, 8)) / 100 : 0;
                    vatFreeAmount = line.Substring(73, 8) != string.Empty ? Convert.ToDecimal(line.Substring(73, 8)) / 100 : 0;
                }

                if(vat == (vat25 + vat12 + vat6))
                {
                    vat25Negative = false;
                }
                if (vat == (vat12 + vat6 - vat25))
                {
                    vat25Negative = true;
                }
                Decimal headAmount = amount;
                Decimal headVat = vat;

                if (isCreditRow)
                {
                    headAmount = Decimal.Negate(amount);
                    headVat = Decimal.Negate(vat);
                }

                int fileType = line.Length != 49 ? 2 : 1;

                customerInvoice.Add(
                    new XElement("KundId", customerNumber),
                    new XElement("FakturabeloppExklusive", headAmount),
                    new XElement("Momsbelopp", headVat),
                    new XElement("ICAfiltyp", fileType));

                //We need to control the amounts in order to get correct accountingrows

                decimal amountDiff = 0;


                if ((vat6 + vat12 + vat25 + vatFreeAmount) != 0)
                {

                    decimal priceVat6Rounded = Math.Round(Convert.ToDecimal((vat6 / backwardVat6) - vat6), 2);
                    decimal priceVat12Rounded = Math.Round(Convert.ToDecimal((vat12 / backwardVat12) - vat12), 2);
                    decimal priceVat25Rounded = Math.Round(Convert.ToDecimal((vat25 / backwardVat25) - vat25), 2);
                    if(vat25Negative)
                    {
                        priceVat25Rounded = priceVat25Rounded * -1;
                    }

                    //Check if the rows has a different sum then the total
                    //if different add diff to amountdiff
                    if (amount != ((priceVat6Rounded + priceVat12Rounded + priceVat25Rounded) + vat))
                    {
                        amountDiff = amount - vat - (priceVat6Rounded + priceVat12Rounded + priceVat25Rounded + vatFreeAmount);

                        //If amountDiff is to big it better not to adjust the amount.
                        if (amountDiff > 1 || amountDiff < -1)
                        { 
                           // amountDiff = 0;
                           // vatFreeAmount = amountDiff * -1;
                            vatFreeAmount = vatFreeAmount + amountDiff;
                            amountDiff = 0;
                        }
                    }
                }

                if (vat25 != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    decimal adjustment = 0;
                    if (amountDiff != 0)
                    {
                        adjustment = amountDiff;
                        amountDiff = 0;
                    }

                    string price = backwardVat25 != 0 ? (Math.Round(Convert.ToDecimal((vat25 / backwardVat25) - vat25), 2) + adjustment).ToString() : "0";
                    if (vat25Negative)
                    {
                        price = "-" + price;
                    }
                    if (isCreditRow)
                    {
                        price = "-" + price;
                        vat25 = Decimal.Negate(vat25);
                    }

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Kvitto: " + receiptNumber + " 25% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", price),
                        new XElement("Momskod", "25"),
                        new XElement("MomsProcent", 25),
                        new XElement("Radmomsbelopp", vat25),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "2"));

                    customerInvoice.Add(customerInvoiceRow);
                }

                if (vat12 != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    decimal adjustment = 0;
                    if (amountDiff != 0)
                    {
                        adjustment = amountDiff;
                        amountDiff = 0;
                    }

                    string price = backwardVat12 != 0 ? (Math.Round(Convert.ToDecimal((vat12 / backwardVat12) - vat12), 2) + adjustment).ToString() : "0";

                    if (isCreditRow)
                    {
                        price = "-" + price;
                        vat12 = Decimal.Negate(vat12);
                    }

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Kvitto: " + receiptNumber + " 12% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", price),
                        new XElement("Momskod", "12"),
                        new XElement("MomsProcent", 12),
                        new XElement("Radmomsbelopp", vat12),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "2"));

                    customerInvoice.Add(customerInvoiceRow);
                }

                if (vat6 != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    decimal adjustment = 0;
                    if (amountDiff != 0)
                    {
                        adjustment = amountDiff;
                        amountDiff = 0;
                    }

                    string price = backwardVat6 != 0 ? (Math.Round(Convert.ToDecimal((vat6 / backwardVat6) - vat6), 2) + adjustment).ToString() : "0";


                    if (isCreditRow)
                    {
                        price = "-" + price;
                        vat6 = Decimal.Negate(vat6);
                    }


                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Kvitto: " + receiptNumber + " 6% moms"),
                        new XElement("Antal", 1),
                        new XElement("Pris", price),
                        new XElement("Momskod", "6"),
                        new XElement("MomsProcent", 6),
                        new XElement("Radmomsbelopp", vat6),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "2"));

                    customerInvoice.Add(customerInvoiceRow);
                }
                if (vatFreeAmount != 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    decimal adjustment = 0;
                    if (amountDiff != 0)
                    {
                        adjustment = amountDiff;
                        amountDiff = 0;
                    }

                    string price = (Math.Round(Convert.ToDecimal((vat6 / backwardVat6) - vat6), 2) + adjustment).ToString();

                    if (isCreditRow)
                    {
                        vatFreeAmount = Decimal.Negate(vatFreeAmount);
                    }

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Kvitto: " + receiptNumber + " momsfritt"),
                        new XElement("Antal", 1),
                        new XElement("Pris", vatFreeAmount),
                        new XElement("Momskod", "0"),
                        new XElement("MomsProcent", 0),
                        new XElement("Radmomsbelopp", 0),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "2"));

                    customerInvoice.Add(customerInvoiceRow);
                }


                if (vat6 + vat12 + vat25 + vatFreeAmount == 0)
                {
                    XElement customerInvoiceRow = new XElement("Fakturarad");

                    Decimal price = amount - vat;
                    decimal vatRate = amount != 0 ? (vat / (amount - vat)) * 100 : 0;
                    vatRate = Math.Round(vatRate, 2);
                    string vatFree = string.Empty;

                    if (vat == 0)
                        vatFree = " momsfritt";

                    // If credit
                    if (isCreditRow)
                    {
                        price = decimal.Negate(price);
                        vat = decimal.Negate(vat);
                    }

                    customerInvoiceRow.Add(
                        new XElement("Artikelid", "0"),
                        new XElement("Namn", "Kvitto: " + receiptNumber + vatFree),
                        new XElement("Antal", 1),
                        new XElement("Pris", price),
                        new XElement("MomsProcent", vatRate),
                        new XElement("Radmomsbelopp", vat),
                        new XElement("Radtyp", 2),
                        new XElement("ICAfiltyp", "1"));

                    customerInvoice.Add(customerInvoiceRow);
                }

                customerInvoices.Add(customerInvoice);
            }

            string customerName = string.Empty;
            string customerNr = string.Empty;
            string customerBillingAddress = string.Empty;
            string customerBillingAddressCO = string.Empty;
            string customerBillingPostNr = string.Empty;
            string customerBillingAddressCity = string.Empty;
            string invoiceDate = string.Empty;
            decimal customerDiscount = 0;

            if (hasProductRows) // Customerinvoice with detail VAT and productrows - different format
            {
                List<XElement> customerInvoiceRows = new List<XElement>();

                bool previousWasProductRow = true;
                customerDiscount = 0;
                XElement customerInvoice = new XElement("Kundfaktura");

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "") continue;

                    if (!line.Contains(";"))
                    {
                        break;
                    }

                    if (line.StartsWith("I") && previousWasProductRow)
                    {
                        string[] inputRow = line.Split(delimiter);

                        customerNr = inputRow[1] != null ? inputRow[1] : string.Empty;

                        Customer customer = null;
                        customer = customerManager.GetCustomerByNr(actorCompanyId, customerNr, customers, false);
                        if (customer != null)
                        {
                            customerDiscount = customer.DiscountMerchandise;
                        }
                        else
                        {
                            customerDiscount = 0;
                        }
                        customerName = inputRow[2] != null ? inputRow[2] : string.Empty;
                        customerBillingAddress = inputRow[3] != null ? inputRow[3] : string.Empty;
                        customerBillingPostNr = inputRow[5] != null ? inputRow[5] : string.Empty;
                        customerBillingAddressCity = inputRow[6] != null ? inputRow[6] : string.Empty;

                    }

                    if (line.StartsWith("K") && ( previousWasProductRow || customerInvoiceRows.Count > 0))
                    {
                        string[] inputRow = line.Split(delimiter);

                        //We need to add the previousInvoices to the list
                        if (customerInvoiceRows.Count > 0)
                        {
                            customerInvoice.Add(customerInvoiceRows);
                            customerInvoices.Add(customerInvoice);
                            customerInvoice = new XElement("Kundfaktura");
                            customerInvoiceRows = new List<XElement>();
                            invoiceDate = string.Empty;
                        }

                        //Parse information              
                        string customerNumber = inputRow[2] != null ? inputRow[2] : string.Empty;
                        string date = inputRow[3] ?? string.Empty;
                        if(invoiceDate== string.Empty || invoiceDate == null) 
                            invoiceDate = inputRow[3] != null ? inputRow[3] : string.Empty;
                        string time = inputRow[4] != null ? inputRow[4] : string.Empty;
                        string receiptNumber = inputRow[5] != null ? inputRow[5] : string.Empty;
                        string refInfo = inputRow[6].Trim() != null ? inputRow[6] : string.Empty;

                        if (refInfo.Trim() != string.Empty)
                            refInfo = " Ref:" + refInfo;

                        string receiptInfo = "Kvitto: " + receiptNumber + " Datum: " + date + " " + time + refInfo;
                        decimal amount = 0;
                        decimal amountdisc = 0;
                        decimal vatFreeAmount = 0;
                        decimal vat = 0;
                        decimal vat6 = 0;
                        decimal vat12 = 0;
                        decimal vat25 = 0;
                        decimal vat6disc = 0;
                        decimal vat12disc = 0;
                        decimal vat25disc = 0;
                        decimal amountDiff = 0;
                        decimal amountSmallDiff6 = 0;
                        decimal amountSmallDiff12 = 0;
                        decimal amountSmallDiff25 = 0;
                        decimal amountSmallDiff = 0;
                        decimal cashPayment = 0;

                        decimal.TryParse(inputRow[8].Replace(".", ","), out amount);
                        decimal.TryParse(inputRow[10].Replace(".", ","), out vat25);
                        decimal.TryParse(inputRow[12].Replace(".", ","), out vat12);
                        decimal.TryParse(inputRow[14].Replace(".", ","), out vat6);

                        //{
                        //    Math.Round(Convert.ToDecimal(amountdisc = (1 - (customerDiscount / 100)) * amount), 2);
                        //    if (vat25 != 0)
                        //    {
                        //        Math.Round(Convert.ToDecimal(vat25disc = (1 - (customerDiscount / 100)) * vat25), 2);
                        //    }
                        //    if (vat12 != 0)
                        //    {
                        //        Math.Round(Convert.ToDecimal(vat12disc = (1 - (customerDiscount / 100)) * vat12), 2);
                        //    }
                        //    if (vat6 != 0)
                        //    {
                        //        Math.Round(Convert.ToDecimal(vat6disc = (1 - (customerDiscount / 100)) * vat6), 2);
                        //    }
                        decimal backwardVat6 = V06;
                        decimal backwardVat12 = V12;
                        decimal backwardVat25 = V25;

                        backwardVat6 = 1 - backwardVat6;
                        backwardVat12 = 1 - backwardVat12;
                        backwardVat25 = 1 - backwardVat25;

                        vat = vat6 + vat12 + vat25;

                        if (vat == 0)
                        {
                            if (customerDiscount != 0)
                            {
                                vatFreeAmount = amountdisc;
                            }
                            else
                            {
                                vatFreeAmount = amount;
                            }
                            vatFreeAmount = amount;
                        }

                        //We need to control the amounts in order to get correct accountingrows
                        if ((vat6 + vat12 + vat25) != 0)
                        {
                            decimal priceVat6Rounded = Math.Round(Convert.ToDecimal((vat6 / backwardVat6) - vat6), 2);
                            decimal priceVat12Rounded = Math.Round(Convert.ToDecimal((vat12 / backwardVat12) - vat12), 2);
                            decimal priceVat25Rounded = Math.Round(Convert.ToDecimal((vat25 / backwardVat25) - vat25), 2);

                            //Check if the rows has a different sum then the total
                            //if different add diff to amountdiff
                            if (amount != ((priceVat6Rounded + priceVat12Rounded + priceVat25Rounded)))
                            {
                                amountDiff = amount - (priceVat6Rounded + priceVat12Rounded + priceVat25Rounded) - vat;                               

                                //If amountDiff is to big it better not to adjust the amount.
                                if (amountDiff < 1 && amountDiff > -1)
                                {
                                    amountSmallDiff = amountDiff;
                                    amountDiff = 0;
                                    if (priceVat6Rounded > priceVat12Rounded)
                                    {
                                        if (priceVat6Rounded > priceVat25Rounded)
                                        {
                                            amountSmallDiff6 = amountSmallDiff;
                                        }
                                        else
                                        {
                                            amountSmallDiff25 = amountSmallDiff;
                                        }
                                    }
                                    else
                                    {
                                        if (priceVat12Rounded > priceVat25Rounded)
                                        {
                                            amountSmallDiff12 = amountSmallDiff;
                                        }
                                        else
                                        {
                                            amountSmallDiff25 = amountSmallDiff;
                                        }
                                    }
                                }
                                else
                                {
                                    vatFreeAmount = amountDiff;
                                    //}
                                    //if ((amountDiff < 1 && amountDiff > -1) == false && amountDiff < 0)
                                    //{
                                    //If diff is bigger than we asume that is cash payment.
                                    //    cashPayment = amountDiff;
                                    //}
                                }
                            }
                        }

                        customerInvoice.Add(
                            new XElement("KundId", customerNumber),
                            new XElement("FakturabeloppExklusive", amount),
                            new XElement("Momsbelopp", vat),
                            new XElement("Kundnr", customerNr),
                            new XElement("Kundnamn", customerName.Replace("\n", ",")),
                            new XElement("Fakturadatum", invoiceDate),
                            new XElement("FakturaAdressCo", customerBillingAddressCO.Replace("\n", ",")),
                            new XElement("FakturaAdress", customerBillingAddress.Replace("\n", ",")),
                            new XElement("FakturaAdressPostNr", customerBillingPostNr),
                            new XElement("FakturaAdressStad", customerBillingAddressCity.Replace("\n", ",")),
                            new XElement("ICAfiltyp", "3"));

                        //ReceiptInfo row
                        XElement customerInvoiceRowInfo = new XElement("Fakturarad");

                        customerInvoiceRowInfo.Add(
                            new XElement("Artikelid", string.Empty),
                            new XElement("Namn", receiptInfo),
                            new XElement("Antal", 1),
                            new XElement("Pris", 0),
                            new XElement("Momskod", string.Empty),
                            new XElement("MomsProcent", 0),
                            new XElement("Radmomsbelopp", 0),
                            new XElement("Radtyp", 3),
                            new XElement("ICAfiltyp", "3"));

                        customerInvoiceRows.Add(customerInvoiceRowInfo);


                        if (vat25 != 0)
                        {
                            XElement customerInvoiceRow = new XElement("Fakturarad");

                            string price = Math.Round(Convert.ToDecimal((vat25 / backwardVat25) - vat25 + amountSmallDiff25), 2).ToString();
                            if (customerDiscount != 0)
                            {
                                vat25 = vat25disc;
                            }
                            customerInvoiceRow.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", "Kvitto: " + receiptNumber + " 25% moms"),
                                new XElement("Antal", 1),
                                new XElement("Pris", price),
                                new XElement("Momskod", "25"),
                                new XElement("MomsProcent", 25),
                                new XElement("Radmomsbelopp", vat25),
                                new XElement("Radtyp", 2),
                                new XElement("ICAfiltyp", "3"));

                            customerInvoiceRows.Add(customerInvoiceRow);
                        }

                        if (vat12 != 0)
                        {
                            XElement customerInvoiceRow = new XElement("Fakturarad");

                            string price = Math.Round(Convert.ToDecimal((vat12 / backwardVat12) - vat12 + amountSmallDiff12), 2).ToString();
                            if (customerDiscount != 0)
                            {
                                vat12 = vat12disc;
                            }
                            customerInvoiceRow.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", "Kvitto: " + receiptNumber + " 12% moms"),
                                new XElement("Antal", 1),
                                new XElement("Pris", price),
                                new XElement("Momskod", "12"),
                                new XElement("MomsProcent", 12),
                                new XElement("Radmomsbelopp", vat12),
                                new XElement("Radtyp", 2),
                                new XElement("ICAfiltyp", "3"));

                            customerInvoiceRows.Add(customerInvoiceRow);
                        }

                        if (vat6 != 0)
                        {
                            XElement customerInvoiceRow = new XElement("Fakturarad");

                            string price = Math.Round(Convert.ToDecimal((vat6 / backwardVat6) - vat6 + amountSmallDiff6), 2).ToString();
                            if (customerDiscount != 0)
                            {
                                vat6 = vat6disc;
                            }
                            customerInvoiceRow.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", "Kvitto: " + receiptNumber + " 6% moms"),
                                new XElement("Antal", 1),
                                new XElement("Pris", price),
                                new XElement("Momskod", "6"),
                                new XElement("MomsProcent", 6),
                                new XElement("Radmomsbelopp", vat6),
                                new XElement("Radtyp", 2),
                                new XElement("ICAfiltyp", "3"));

                            customerInvoiceRows.Add(customerInvoiceRow);
                        }

                        if (cashPayment != 0)
                        {
                            XElement customerInvoiceRow = new XElement("Fakturarad");

                            string price = Math.Round(Convert.ToDecimal((vat6 / backwardVat6) - vat6), 2).ToString();

                            customerInvoiceRow.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", "Kvitto: " + receiptNumber + " Kontantbetalning "),
                                new XElement("Antal", 1),
                                new XElement("Pris", cashPayment),
                                new XElement("Momskod", "0"),
                                new XElement("MomsProcent", 0),
                                new XElement("Radmomsbelopp", 0),
                                new XElement("Radtyp", 2),
                                new XElement("ICAfiltyp", "3"));

                            customerInvoiceRows.Add(customerInvoiceRow);
                        }

                        if (vatFreeAmount != 0)
                        {
                            XElement customerInvoiceRow = new XElement("Fakturarad");

                            customerInvoiceRow.Add(
                                new XElement("Artikelid", "0"),
                                new XElement("Namn", receiptNumber + " momsfritt"),
                                new XElement("Antal", 1),
                                new XElement("Pris", vatFreeAmount),
                                new XElement("Momskod", "0"),
                                new XElement("MomsProcent", 0),
                                new XElement("Radmomsbelopp", 0),
                                new XElement("Radtyp", 2),
                                new XElement("ICAfiltyp", "3"));

                            customerInvoiceRows.Add(customerInvoiceRow);
                        }

                        previousWasProductRow = false;

                    }

                    if (line.StartsWith("V"))
                    {
                        string[] inputRow = line.Split(delimiter);

                        XElement customerInvoiceRow = new XElement("Fakturarad");

                        //Parse information              
                        string name = inputRow[3] != null ? inputRow[3] : string.Empty;
                        string quantity = inputRow.Length > 4 ? inputRow[4] : string.Empty;
                        quantity = quantity.Replace(".000", "");

                        if (quantity == string.Empty)
                        {
                            customerInvoiceRow.Add(
                                 new XElement("Artikelid", string.Empty),
                                 new XElement("Namn", name.Replace("\n", ",")),
                                 new XElement("Antal", 1),
                                 new XElement("Pris", 0),
                                 new XElement("Momskod", string.Empty),
                                 new XElement("MomsProcent", 0),
                                 new XElement("Radmomsbelopp", 0),
                                 new XElement("Radtyp", 3),
                                 new XElement("ICAfiltyp", "3"));
                        }
                        else 
                        {
                            customerInvoiceRow.Add(
                                new XElement("Artikelid", string.Empty),
                                new XElement("Namn", "Vara: " + name.Replace("\n", ",") + " Antal: " + quantity),
                                new XElement("Antal", 1),
                                new XElement("Pris", 0),
                                new XElement("Momskod", string.Empty),
                                new XElement("MomsProcent", 0),
                                new XElement("Radmomsbelopp", 0),
                                new XElement("Radtyp", 3),
                                new XElement("ICAfiltyp", "3"));
                        }
 

                        customerInvoiceRows.Add(customerInvoiceRow);
                        previousWasProductRow = true;
                    }

                }

                //Add last invoice
                customerInvoice.Add(customerInvoiceRows);
                customerInvoices.Add(customerInvoice);

            }

            //aggregate invoices on same customer

            List<XElement> sortedCustomerInvoices = customerInvoices.OrderBy(e => (int)e.Element("KundId")).ToList();
            List<XElement> aggregatedCustomerInvoices = new List<XElement>();

            XElement previousInvoice = null;
            List<XElement> currentCustomerInvoiceRows = new List<XElement>();

            decimal vatSum = 0;
            decimal invoiceAmount = 0;
            int numberOfInvoices = 0;

            foreach (XElement customerInvoice in sortedCustomerInvoices)
            {
                numberOfInvoices += 1;

                if (previousInvoice != null && (customerInvoice.Element("KundId").ToString() != previousInvoice.Element("KundId").ToString()))
                {
                    previousInvoice.Element("Momsbelopp").Remove();
                    previousInvoice.Element("FakturabeloppExklusive").Remove();
                    previousInvoice.Elements("Fakturarad").Remove();
                    previousInvoice.Add(new XElement("Momsbelopp", vatSum));
                    previousInvoice.Add(new XElement("FakturabeloppExklusive", invoiceAmount));


                    //foreach (XElement customerInvoiceRow in currentCustomerInvoiceRows)
                    //{
                    //    previousInvoice.Add(customerInvoiceRow);
                    //}

                    previousInvoice.Add(currentCustomerInvoiceRows);

                    //Done looping on same customer, add to list
                    if (previousInvoice != null)
                        aggregatedCustomerInvoices.Add(previousInvoice);

                    vatSum = 0;
                    invoiceAmount = 0;

                    currentCustomerInvoiceRows.Clear();

                }

                currentCustomerInvoiceRows.AddRange(customerInvoice.Elements("Fakturarad").ToList());
                vatSum += (decimal)(customerInvoice.Element("Momsbelopp"));
                invoiceAmount += (decimal)(customerInvoice.Element("FakturabeloppExklusive"));

                previousInvoice = customerInvoice;

            }

            if (currentCustomerInvoiceRows.Count > 0)
            {
                if (customerDiscount != 0)
                {
                    Math.Round(Convert.ToDecimal(vatSum = (1 - (customerDiscount / 100)) * vatSum), 2);
                }
                previousInvoice.Element("Momsbelopp").Remove();
                previousInvoice.Element("FakturabeloppExklusive").Remove();
                previousInvoice.Elements("Fakturarad").Remove();
                previousInvoice.Add(new XElement("Momsbelopp", vatSum));
                previousInvoice.Add(new XElement("FakturabeloppExklusive", invoiceAmount));

                previousInvoice.Add(currentCustomerInvoiceRows);

                if (previousInvoice != null)
                    aggregatedCustomerInvoices.Add(previousInvoice);

            }

            customerInvoicesHeadElement.Add(aggregatedCustomerInvoices);

            modifiedContent = customerInvoicesHeadElement.ToString();

            return modifiedContent;
        }

    }
}
