using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class PirateCustomerInvoice
    {
        public string ApplyPirateCustomerInvoiceSpecialModification(string content, int actorCompanyId, ParameterObject parameterObject)
        {
            //New Entities with new extended timeout
            using (CompEntities entities = new CompEntities())
            {
                var pm = new ProductManager(parameterObject);
                var pgm = new ProductGroupManager(parameterObject);
                var sm = new SettingManager(parameterObject);
                var am = new AccountManager(parameterObject);
                var stm = new StockManager(parameterObject);
                //Get AccountStd from CompanySetting
                int accountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductSales, 0, actorCompanyId, 0);
                var productSalesAccount = am.GetAccount(actorCompanyId, accountId, onlyActive: false);
                //Get AccountStd 

                Account settingAccount = am.GetAccount(actorCompanyId, accountId);

                char[] delimiter = new char[1];
                delimiter[0] = ';';

                byte[] byteArray = Encoding.Default.GetBytes(content);
                MemoryStream stream = new MemoryStream(byteArray);
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream, Encoding.Default);

                string line;
                string customerNumber = string.Empty;
                string prevCustomerNumber = string.Empty;
                string accountNumber = string.Empty;
                decimal invoiceOrderNo = 0;
                decimal prevInvoiceOrderNo = 0;
                decimal headAmount = 0;
                decimal headVatAmount = 0;

                List<XElement> invoiceElements = new List<XElement>();
                XElement customerInvoicesHeadElement = new XElement("CustomerInvoices");
                XElement customerInvoice = new XElement("CustomerInvoice");

                // Kund = PIRAT

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "") continue;
                    if (line.Length <= 90) continue;

                    //Parse information              

                    prevInvoiceOrderNo = invoiceOrderNo;
                    switch (line.Length)
                    {
                        case 92: customerNumber = line.Substring(90, 2); break;
                        case 94: customerNumber = line.Substring(90, 4); break;
                        case 95: customerNumber = line.Substring(90, 5); break;
                        case 96: customerNumber = line.Substring(90, 5); break;
                        case 97: customerNumber = line.Substring(90, 5); break;
                        case 98: customerNumber = line.Substring(90, 5); break;
                        case 99: customerNumber = line.Substring(90, 5); break;
                        case 100: customerNumber = line.Substring(90, 5); break;

                    }
 
                    if (customerNumber.Trim() == "PIRAT" || customerNumber.Trim() == "LPIR")
                    {
                        prevCustomerNumber = customerNumber;
                    }
                    else
                    {
                        continue;
                    }
 
                    invoiceOrderNo = line.Substring(84, 6) != string.Empty ? Convert.ToDecimal(line.Substring(84, 6)) : 0;
                    string productNr = line.Substring(0, 5);
                    string rowProject = line.Substring(0, 4);
                    string rowCostPlace = string.Empty;
                    if (line.Length == 100)
                    {
                        rowCostPlace = line.Substring(97, 3);
                    }
                    if (line.Length == 99)
                    {
                        rowCostPlace = line.Substring(96, 3);
                    }
                    if (line.Length == 98)
                    {
                        rowCostPlace = line.Substring(95, 3);
                    }
                    if (line.Length == 97)
                    {
                        rowCostPlace = line.Substring(95, 2);
                    }
                    string rowText = line.Substring(41, 30);
                    string rowQuantityText = line.Substring(5, 6);
                    string rowPriceText = line.Substring(71, 10);
                    string rowDeliveryDate = line.Substring(84, 6);
                    decimal rowQuantity = rowQuantityText.Trim() != string.Empty ? Convert.ToDecimal(rowQuantityText) : 0;
                    decimal rowPrice = rowPriceText.Trim() != string.Empty ? Convert.ToDecimal(rowPriceText) : 0;
                    decimal rowAmount = rowQuantity * rowPrice;
                    decimal rowVatAmount = rowAmount * Convert.ToDecimal("0,06");
                    decimal avgPrice = 0;
                    headAmount += rowAmount;
                    headVatAmount += rowVatAmount;
                    accountNumber = "";
                    ProductGroupDTO productGroup = null;
                    var product = pm.GetInvoiceProductSmall(entities, productNr, actorCompanyId);
                    if (product != null && product.ProductGroupId != null)
                    {
                        var stocks = stm.GetStockProductAvgPriceDTOs(entities, actorCompanyId, product.ProductId);
                        if (stocks != null && stocks.Count > 0)
                        {
                            avgPrice = stocks[0].AvgPrice;
                        }
                        productGroup = pgm.GetProductGroupDTO(entities, product.ProductGroupId.GetValueOrDefault(), actorCompanyId);
                      }

                    if (productGroup != null)
                    {
                        switch (productGroup.Code)
                        {
                            case "10": accountNumber = "3083"; break;  //PIRAT Debtering inbunder
                            case "20": accountNumber = "3084"; break;  //PIRAT Debitering krt/storp
                            case "30": accountNumber = "3085"; break;  //PIRAT Debitering pocket
                            case "40": accountNumber = "3093"; break;  //PIRAT Kreditering inbundet
                            case "50": accountNumber = "3086"; break;  //PIRAT Debitering CD/mp3
                            case "60": accountNumber = "3094"; break;  //PIRAT Kreditering kart/storp
                            case "70": accountNumber = "3095"; break;  //PIRAT Kreditering pocket
                            case "80": accountNumber = "3096"; break;  //PIRAT Kreditering CD/mp3
                            case "90": accountNumber = ""; break;  //används ej
                        }
                    }
                    else 
                    {
                        accountNumber = productSalesAccount.AccountNr;
                    }

                    if (!productNr.IsNullOrEmpty())
                    {
                        XElement row = new XElement("CustomerInvoiceRow");
                        row.Add(
                            new XElement("ProductNr", productNr),
                            new XElement("Quantity", rowQuantity),
                            new XElement("UnitPrice", rowPrice),
                            new XElement("PurchasePrice", avgPrice),
                            new XElement("Text", rowText),
                            new XElement("Amount", rowAmount),
                            new XElement("AmountCurrency", rowAmount),
                            new XElement("SumAmount", rowAmount),
                            new XElement("SumAmountCurrency", rowAmount),
                            new XElement("Momskod", "6"),
                            new XElement("MomsProcent", 6),
                            new XElement("Radmomsbelopp", rowVatAmount),
                            new XElement("Konto", accountNumber),
                            new XElement("Projekt", rowProject),
                            new XElement("Kst", rowCostPlace.Trim()),
                            new XElement("DeliveryDate", rowDeliveryDate));
                        customerInvoice.Add(row);
                    }


                    //               invoiceElements.Add(customerInvoice);
                }
                if (!prevCustomerNumber.IsNullOrEmpty())
                {
                    customerInvoice.Add(
                      new XElement("CustomerNr", prevCustomerNumber.Trim()),
                      new XElement("FakturabeloppExklusive", headAmount),
                      new XElement("FakturaMomsbelopp", headVatAmount));
                    invoiceElements.Add(customerInvoice);

                    //         invoiceElements.Add(customerInvoice);

                    customerInvoicesHeadElement.Add(invoiceElements);
                }

                // Kund = EU

                byteArray = Encoding.Default.GetBytes(content);
                stream = new MemoryStream(byteArray);
                stream.Position = 0;
                reader = new StreamReader(stream, Encoding.Default);

                line = string.Empty;
                customerNumber = string.Empty;
                prevCustomerNumber = string.Empty;
                invoiceOrderNo = 0;
                prevInvoiceOrderNo = 0;
                headAmount = 0;
                headVatAmount = 0;
                invoiceElements = new List<XElement>();
                customerInvoice = new XElement("CustomerInvoice");

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "") continue;
                    if (line.Length <= 90) continue;

                    //Parse information              

                    prevInvoiceOrderNo = invoiceOrderNo;

                    switch (line.Length)
                    {
                        case 92: customerNumber = line.Substring(90, 2); break;
                        case 94: customerNumber = line.Substring(90, 4); break;
                        case 95: customerNumber = line.Substring(90, 5); break;
                        case 96: customerNumber = line.Substring(90, 5); break;
                        case 97: customerNumber = line.Substring(90, 5); break;
                        case 98: customerNumber = line.Substring(90, 5); break;
                        case 99: customerNumber = line.Substring(90, 5); break;
                        case 100: customerNumber = line.Substring(90, 5); break;

                    }

                    if (customerNumber.Trim() != "EU")
                    {
                        continue;
                    }
                    prevCustomerNumber = customerNumber;

                    invoiceOrderNo = line.Substring(84, 6) != string.Empty ? Convert.ToDecimal(line.Substring(84, 6)) : 0;
                    string productNr = line.Substring(0, 5);
                    string rowProject = line.Substring(0, 4);
                    string rowCostPlace = string.Empty;
                    if (line.Length == 100)
                    {
                        rowCostPlace = line.Substring(97, 3);
                    }
                    if (line.Length == 99)
                    {
                        rowCostPlace = line.Substring(96, 3);
                    }
                    if (line.Length == 98)
                    {
                        rowCostPlace = line.Substring(95, 3);
                    }
                    if (line.Length == 97)
                    {
                        rowCostPlace = line.Substring(95, 2);
                    }
                    string rowText = line.Substring(41, 30);
                    string rowQuantityText = line.Substring(5, 6);
                    string rowPriceText = line.Substring(66, 15);
                    string rowDeliveryDate = line.Substring(84, 6);
                    decimal rowQuantity = rowQuantityText.Trim() != string.Empty ? Convert.ToDecimal(rowQuantityText) : 0;
                    decimal rowPrice = rowPriceText.Trim() != string.Empty ? Convert.ToDecimal(rowPriceText) : 0;
                    decimal rowAmount = rowQuantity * rowPrice;
               //     decimal rowVatAmount = rowAmount * Convert.ToDecimal("0,06");
                    decimal avgPrice = 0;
                    headAmount += rowAmount;
                    headVatAmount += 0;
                    accountNumber = "";
                    ProductGroupDTO productGroup = null;
                    var product = pm.GetInvoiceProductSmall(entities, productNr, actorCompanyId);
                    if (product != null && product.ProductGroupId != null)
                    {
                        var stocks = stm.GetStockProductAvgPriceDTOs(entities, actorCompanyId, product.ProductId);
                        if (stocks != null && stocks.Count > 0)
                        {
                            avgPrice = stocks[0].AvgPrice;
                        }
                        productGroup = pgm.GetProductGroupDTO(entities, product.ProductGroupId.GetValueOrDefault(), actorCompanyId);
                    }

                    if (productGroup != null)
                    {
                        switch (productGroup.Code)
                        {
                            case "10": accountNumber = "3081"; break;  //EU Debtering inbunder
                            case "20": accountNumber = "3081"; break;  //EU Debitering krt/storp
                            case "30": accountNumber = "3081"; break;  //EU Debitering pocket
                            case "40": accountNumber = "3091"; break;  //EU Kreditering inbundet
                            case "50": accountNumber = "3081"; break;  //EU Debitering CD/mp3
                            case "60": accountNumber = "3091"; break;  //EU Kreditering kart/storp
                            case "70": accountNumber = "3091"; break;  //EU Kreditering pocket
                            case "80": accountNumber = "3091"; break;  //EU Kreditering CD/mp3
                            case "90": accountNumber = ""; break;  //används ej
                        }
                    }
                    else
                    {
                        accountNumber = productSalesAccount.AccountNr;
                    }
                    if (!productNr.IsNullOrEmpty())
                    {
                        XElement row = new XElement("CustomerInvoiceRow");
                        row.Add(
                            new XElement("ProductNr", productNr),
                            new XElement("Quantity", rowQuantity),
                            new XElement("UnitPrice", rowPrice),
                            new XElement("PurchasePrice", avgPrice),
                            new XElement("Text", rowText),
                            new XElement("Amount", rowAmount),
                            new XElement("AmountCurrency", rowAmount),
                            new XElement("SumAmount", rowAmount),
                            new XElement("SumAmountCurrency", rowAmount),
             //               new XElement("Momskod", "6"),
             //               new XElement("MomsProcent", 6),
             //               new XElement("Radmomsbelopp", rowVatAmount),
                            new XElement("Konto", accountNumber),
                            new XElement("Projekt", rowProject),
                            new XElement("Kst", rowCostPlace.Trim()),
                            new XElement("DeliveryDate", rowDeliveryDate));
                        customerInvoice.Add(row);
                    }


                    //               invoiceElements.Add(customerInvoice);
                }
                if (!prevCustomerNumber.IsNullOrEmpty())
                {
                    customerInvoice.Add(
                  new XElement("CustomerNr", prevCustomerNumber.Trim()),
                  new XElement("FakturabeloppExklusive", headAmount),
                  new XElement("FakturaMomsbelopp", headVatAmount));
                    invoiceElements.Add(customerInvoice);

                    //         invoiceElements.Add(customerInvoice);

                    customerInvoicesHeadElement.Add(invoiceElements);
                    customerInvoice = new XElement("CustomerInvoice");
                }
                // Kund = EXPOR

                byteArray = Encoding.Default.GetBytes(content);
                stream = new MemoryStream(byteArray);
                stream.Position = 0;
                reader = new StreamReader(stream, Encoding.Default);

                line = string.Empty;
                customerNumber = string.Empty;
                prevCustomerNumber = string.Empty;
                invoiceOrderNo = 0;
                prevInvoiceOrderNo = 0;
                headAmount = 0;
                headVatAmount = 0;
                invoiceElements = new List<XElement>();

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "") continue;
                    if (line.Length <= 90) continue;

                    //Parse information              

                    prevInvoiceOrderNo = invoiceOrderNo;
                    switch (line.Length)
                    {
                        case 92: customerNumber = line.Substring(90, 2); break;
                        case 94: customerNumber = line.Substring(90, 4); break;
                        case 95: customerNumber = line.Substring(90, 5); break;
                        case 96: customerNumber = line.Substring(90, 5); break;
                        case 97: customerNumber = line.Substring(90, 5); break;
                        case 98: customerNumber = line.Substring(90, 5); break;
                        case 99: customerNumber = line.Substring(90, 5); break;
                        case 100: customerNumber = line.Substring(90, 5); break;

                    }

                    if (customerNumber.Trim() != "EXPOR")
                    {
                        continue;
                    }
                    prevCustomerNumber = customerNumber;

                    invoiceOrderNo = line.Substring(84, 6) != string.Empty ? Convert.ToDecimal(line.Substring(84, 6)) : 0;
                    string productNr = line.Substring(0, 5);
                    if (productNr == "0781K")
                    {
                        productNr = line.Substring(0, 5);
                    }
                    string rowProject = line.Substring(0, 4);
                    string rowCostPlace = string.Empty;
                    if (line.Length == 100)
                    {
                        rowCostPlace = line.Substring(97, 3);
                    }
                    if (line.Length == 99)
                    {
                        rowCostPlace = line.Substring(96, 3);
                    }
                    if (line.Length == 98)
                    {
                        rowCostPlace = line.Substring(95, 3);
                    }
                    if (line.Length == 97)
                    {
                        rowCostPlace = line.Substring(95, 2);
                    }
                    string rowText = line.Substring(41, 30);
                    string rowQuantityText = line.Substring(5, 6);
                    string rowPriceText = line.Substring(66, 15);
                    string rowDeliveryDate = line.Substring(84, 6);
                    decimal rowQuantity = rowQuantityText.Trim() != string.Empty ? Convert.ToDecimal(rowQuantityText) : 0;
                    decimal rowPrice = rowPriceText.Trim() != string.Empty ? Convert.ToDecimal(rowPriceText) : 0;
                    decimal rowAmount = rowQuantity * rowPrice;
                
                    decimal avgPrice = 0;
                    headAmount += rowAmount;
                    headVatAmount += 0;
                    accountNumber = "";
                    ProductGroupDTO productGroup = null;
                    var product = pm.GetInvoiceProductSmall(entities, productNr, actorCompanyId);
                    if (product != null && product.ProductGroupId != null)
                    {
                        var stocks = stm.GetStockProductAvgPriceDTOs(entities, actorCompanyId, product.ProductId);
                        if (stocks != null && stocks.Count > 0)
                        {
                            avgPrice = stocks[0].AvgPrice;
                        }
                        productGroup = pgm.GetProductGroupDTO(entities, product.ProductGroupId.GetValueOrDefault(), actorCompanyId);
                    }

                    if (productGroup != null)
                    {
                        switch (productGroup.Code)
                        {
                            case "10": accountNumber = "3082"; break;  //EXPOR Debtering inbunder
                            case "20": accountNumber = "3082"; break;  //EXPOR Debitering krt/storp
                            case "30": accountNumber = "3082"; break;  //EXPOR Debitering pocket
                            case "40": accountNumber = "3092"; break;  //EXPOR Kreditering inbundet
                            case "50": accountNumber = "3082"; break;  //EXPOR Debitering CD/mp3
                            case "60": accountNumber = "3092"; break;  //EXPOR Kreditering kart/storp
                            case "70": accountNumber = "3092"; break;  //EXPOR Kreditering pocket
                            case "80": accountNumber = "3092"; break;  //EXPOR Kreditering CD/mp3
                            case "90": accountNumber = ""; break;  //används ej
                        }
                    }
                    else
                    {
                        accountNumber = productSalesAccount.AccountNr;
                    }

                    if (!productNr.IsNullOrEmpty())
                    {
                        XElement row = new XElement("CustomerInvoiceRow");
                        row.Add(
                            new XElement("ProductNr", productNr),
                            new XElement("Quantity", rowQuantity),
                            new XElement("UnitPrice", rowPrice),
                            new XElement("PurchasePrice", avgPrice),
                            new XElement("Text", rowText),
                            new XElement("Amount", rowAmount),
                            new XElement("AmountCurrency", rowAmount),
                            new XElement("SumAmount", rowAmount),
                            new XElement("SumAmountCurrency", rowAmount),
                 //           new XElement("Momskod", "6"),
                 //           new XElement("MomsProcent", 6),
                 //           new XElement("Radmomsbelopp", rowVatAmount),
                            new XElement("Konto", accountNumber),
                            new XElement("Projekt", rowProject),
                            new XElement("Kst", rowCostPlace.Trim()),
                            new XElement("DeliveryDate", rowDeliveryDate));
                        customerInvoice.Add(row);
                    }


                    //               invoiceElements.Add(customerInvoice);
                }
                if (!prevCustomerNumber.IsNullOrEmpty())
                {

                    customerInvoice.Add(
                      new XElement("CustomerNr", prevCustomerNumber.Trim()),
                      new XElement("FakturabeloppExklusive", headAmount),
                      new XElement("FakturaMomsbelopp", headVatAmount));
                    invoiceElements.Add(customerInvoice);

                    //         invoiceElements.Add(customerInvoice);

                    customerInvoicesHeadElement.Add(invoiceElements);
                }

                return customerInvoicesHeadElement.ToString();
            }
        }

    }
}
