using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class CustomerInvoiceRowIOItem
    {
        #region Collections

        public List<CustomerInvoiceRowIODTO> customerInvoiceRows = new List<CustomerInvoiceRowIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "CustomerInvoiceRowIO";

        public string XML_CustomerInvoiceNr_TAG = "InvoiceNr";
        public string XML_CustomerInvoiceId_TAG = "InvoiceId";
        public string XML_CustomerRowType_TAG = "CustomerRowType";
        public string XML_ProductNr_TAG = "ProductNr";
        public string XML_ProductName_TAG = "ProductName";
        public string XML_ProductName2_TAG = "ProductName2";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_UnitPrice_TAG = "UnitPrice";
        public string XML_Discount_TAG = "Discount";
        public string XML_VatRate_TAG = "VatRate";
        public string XML_Unit_TAG = "Unit";
        public string XML_UnitId_TAG = "ProductUnitId";


        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_AccountDim6Nr_TAG = "AccountDim6Nr";
        public string XML_AccountSieDim1_TAG = "AccountSieDim1";
        public string XML_AccountSieDim6_TAG = "AccountSieDim6";

        public string XML_PurchasePrice_TAG = "PurchasePrice";
        public string XML_PurchasePriceCurrency_TAG = "PurchasePriceCurrency";
        public string XML_Amount_TAG = "Amount";
        public string XML_AmountCurrency_TAG = "AmountCurrency";
        public string XML_VAmount_TAG = "VatAmount";
        public string XML_VAmountCurrency_TAG = "VatAmountCurrency";
        public string XML_DiscountAmount_TAG = "DiscountAmount";
        public string XML_DiscountAmountCurrency_TAG = "DiscountAmountCurrency";
        public string XML_MarginalIncome_TAG = "MarginalIncome";
        public string XML_MarginalIncomeCurrency_TAG = "MarginalIncomeCurrency";
        public string XML_SumAmount_TAG = "SumAmount";
        public string XML_SumAmountCurrency_TAG = "SumAmountCurrency";

        public string XML_RowStatus_TAG = "RowStatus";
        public string XML_VatCode_TAG = "VatCode";
        public string XML_InvoiceQuantity_TAG = "InvoiceQuantity";
        public string XML_PreviouslyInvoicedQuantity_TAG = "PreviouslyInvoicedQuantity";
        public string XML_Stock_TAG = "Stock";
        public string XML_RowDate_TAG = "RowDate";
        public string XML_ClaimAccountNr_TAG = "ClaimAccountNr";
        public string XML_ClaimAccountNrDim2_TAG = "ClaimAccountNrDim2";
        public string XML_ClaimAccountNrDim3_TAG = "ClaimAccountNrDim3";
        public string XML_ClaimAccountNrDim4_TAG = "ClaimAccountNrDim4";
        public string XML_ClaimAccountNrDim5_TAG = "ClaimAccountNrDim5";
        public string XML_ClaimAccountNrDim6_TAG = "ClaimAccountNrDim6";
        public string XML_ClaimAccountNrSieDim1_TAG = "ClaimAccountNrSieDim1";
        public string XML_ClaimAccountNrSieDim6_TAG = "ClaimAccountNrSieDim6";

        public string XML_VatAccountnr_TAG = "VatAccountnr";

        public string XML_ExternalId_TAG = "ExternalId";


        public string XML_Text_TAG = "Text";

        #endregion

        #region Constructors
        public CustomerInvoiceRowIOItem()
        {
        }

        public CustomerInvoiceRowIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public CustomerInvoiceRowIOItem(string content, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(content, headType);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> customerInvoiceRowIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(customerInvoiceRowIOElements, headType);

        }

        public void CreateObjects(List<XElement> customerInvoiceRowIOElements, TermGroup_IOImportHeadType headType, decimal currencyRate = 1)
        {
            foreach (var customerInvoiceRowIOElement in customerInvoiceRowIOElements)
            {
                CustomerInvoiceRowIODTO invoiceRowIODTO = new CustomerInvoiceRowIODTO();

                #region Extract CustomerInvoiceRow Data

                DateTime rowDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_RowDate_TAG), "yyyyMMdd");
                if (rowDate == CalendarUtility.DATETIME_DEFAULT)
                    rowDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_RowDate_TAG), "yyyy-MM-dd");

                invoiceRowIODTO.InvoiceNr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_CustomerInvoiceNr_TAG);
                invoiceRowIODTO.InvoiceId = XmlUtil.GetElementIntValue(customerInvoiceRowIOElement, XML_CustomerInvoiceId_TAG);

                invoiceRowIODTO.CustomerRowType = (SoeInvoiceRowType)XmlUtil.GetElementIntValue(customerInvoiceRowIOElement, XML_CustomerRowType_TAG);
                invoiceRowIODTO.ProductNr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ProductNr_TAG);
                invoiceRowIODTO.ProductName = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ProductName_TAG);
                invoiceRowIODTO.ProductName2 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ProductName2_TAG);
                invoiceRowIODTO.Quantity = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_Quantity_TAG);
                invoiceRowIODTO.UnitPrice = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_UnitPrice_TAG);
                invoiceRowIODTO.Discount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_Discount_TAG);
                invoiceRowIODTO.VatRate = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_VatRate_TAG);
                invoiceRowIODTO.Unit = XmlUtil.GetElementNullableValue(customerInvoiceRowIOElement, XML_Unit_TAG);
                invoiceRowIODTO.ProductUnitId = XmlUtil.GetElementNullableIntValue(customerInvoiceRowIOElement, XML_UnitId_TAG);

                invoiceRowIODTO.AccountNr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountNr_TAG);
                invoiceRowIODTO.AccountDim2Nr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountDim2Nr_TAG);
                invoiceRowIODTO.AccountDim3Nr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountDim3Nr_TAG);
                invoiceRowIODTO.AccountDim4Nr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountDim4Nr_TAG);
                invoiceRowIODTO.AccountDim5Nr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountDim5Nr_TAG);
                invoiceRowIODTO.AccountDim6Nr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountDim6Nr_TAG);
                invoiceRowIODTO.AccountSieDim1 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountSieDim1_TAG);
                invoiceRowIODTO.AccountSieDim6 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_AccountSieDim6_TAG);
                

                invoiceRowIODTO.PurchasePrice = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_PurchasePrice_TAG);
                invoiceRowIODTO.PurchasePriceCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_PurchasePriceCurrency_TAG);
                invoiceRowIODTO.Amount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_Amount_TAG);
                invoiceRowIODTO.AmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_AmountCurrency_TAG);
                invoiceRowIODTO.VatAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_VAmount_TAG);
                invoiceRowIODTO.VatAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_VAmountCurrency_TAG);
                invoiceRowIODTO.DiscountAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_DiscountAmount_TAG);
                invoiceRowIODTO.DiscountAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_DiscountAmountCurrency_TAG);
                invoiceRowIODTO.MarginalIncome = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_MarginalIncome_TAG);
                invoiceRowIODTO.MarginalIncomeCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_MarginalIncomeCurrency_TAG);
                invoiceRowIODTO.SumAmount = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_SumAmount_TAG);
                invoiceRowIODTO.SumAmountCurrency = XmlUtil.GetElementNullableDecimalValue(customerInvoiceRowIOElement, XML_SumAmountCurrency_TAG);

                invoiceRowIODTO.RowStatus = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_RowStatus_TAG);
                invoiceRowIODTO.VatCode = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_VatCode_TAG);
                invoiceRowIODTO.InvoiceQuantity = XmlUtil.GetElementIntValue(customerInvoiceRowIOElement, XML_InvoiceQuantity_TAG);
                invoiceRowIODTO.PreviouslyInvoicedQuantity = XmlUtil.GetElementIntValue(customerInvoiceRowIOElement, XML_PreviouslyInvoicedQuantity_TAG);
                invoiceRowIODTO.Stock = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_Stock_TAG);
                invoiceRowIODTO.RowDate = rowDate;
                invoiceRowIODTO.ClaimAccountNr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNr_TAG);
                invoiceRowIODTO.ClaimAccountNrDim2 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrDim2_TAG);
                invoiceRowIODTO.ClaimAccountNrDim3 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrDim3_TAG);
                invoiceRowIODTO.ClaimAccountNrDim4 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrDim4_TAG);
                invoiceRowIODTO.ClaimAccountNrDim5 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrDim5_TAG);
                invoiceRowIODTO.ClaimAccountNrDim6 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrDim6_TAG);
                invoiceRowIODTO.ClaimAccountNrSieDim1 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrSieDim1_TAG);
                invoiceRowIODTO.ClaimAccountNrSieDim6 = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ClaimAccountNrSieDim6_TAG);
                invoiceRowIODTO.VatAccountnr = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_VatAccountnr_TAG);

                invoiceRowIODTO.ExternalId = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_ExternalId_TAG);


                invoiceRowIODTO.Text = XmlUtil.GetChildElementValue(customerInvoiceRowIOElement, XML_Text_TAG);

                #endregion

                #region fix

                decimal amount = invoiceRowIODTO.Amount.HasValue ? (decimal)invoiceRowIODTO.Amount : 0;
                decimal amountCurrency = invoiceRowIODTO.AmountCurrency.HasValue ? (decimal)invoiceRowIODTO.AmountCurrency : 0;
                decimal sumAmount = invoiceRowIODTO.SumAmount.HasValue ? (decimal)invoiceRowIODTO.SumAmount : 0;
                decimal sumAmountCurrency = invoiceRowIODTO.SumAmountCurrency.HasValue ? (decimal)invoiceRowIODTO.SumAmountCurrency : 0;
                decimal vatAmount = invoiceRowIODTO.VatAmount.HasValue ? (decimal)invoiceRowIODTO.VatAmount : 0;
                decimal vatAmountCurrency = invoiceRowIODTO.VatAmountCurrency.HasValue ? (decimal)invoiceRowIODTO.VatAmountCurrency : 0;
                decimal discount = invoiceRowIODTO.Discount.HasValue ? (decimal)invoiceRowIODTO.Discount : 0;              
                decimal quantity = invoiceRowIODTO.Quantity.HasValue ? (decimal)invoiceRowIODTO.Quantity : 0;
                decimal invoiceQuantity = invoiceRowIODTO.InvoiceQuantity;
                decimal unitPrice = invoiceRowIODTO.UnitPrice.HasValue ? (decimal)invoiceRowIODTO.UnitPrice : 0;
                decimal vatRate = invoiceRowIODTO.VatRate.HasValue ? (decimal)invoiceRowIODTO.VatRate : 0;
                decimal allAmounts = amount + amountCurrency + sumAmount + sumAmountCurrency + vatAmount + vatAmountCurrency;

                //If the row i empty do not add it
                if (string.IsNullOrEmpty(invoiceRowIODTO.ProductNr) && string.IsNullOrEmpty(invoiceRowIODTO.ProductName) && string.IsNullOrEmpty(invoiceRowIODTO.Text) && allAmounts == 0)
                    continue;

                //Add as textrow when everything is blank but Text
                if (string.IsNullOrEmpty(invoiceRowIODTO.ProductNr) && allAmounts == 0)
                {
                    if (string.IsNullOrEmpty(invoiceRowIODTO.Text) && !string.IsNullOrEmpty(invoiceRowIODTO.ProductName))
                        invoiceRowIODTO.Text = invoiceRowIODTO.ProductName;

                    invoiceRowIODTO.CustomerRowType = SoeInvoiceRowType.TextRow;
                    customerInvoiceRows.Add(invoiceRowIODTO);
                    continue;
                }

                //Add quantity from InvoiceQuantity
                if (quantity == 0 && invoiceQuantity != 0)
                {
                    invoiceRowIODTO.Quantity = invoiceRowIODTO.InvoiceQuantity;
                    quantity = invoiceQuantity;
                }

                //calculate Unitprice if 0;
                if (amount != 0 && quantity != 0 && unitPrice == 0)
                {
                    if (invoiceRowIODTO.Amount > 0 && invoiceRowIODTO.AmountCurrency > 0 && currencyRate != 0)
                    {
                        invoiceRowIODTO.UnitPrice = invoiceRowIODTO.AmountCurrency;
                    }
                    else
                    {
                        invoiceRowIODTO.UnitPrice = invoiceRowIODTO.Amount / invoiceRowIODTO.Quantity;
                        unitPrice = (decimal)invoiceRowIODTO.UnitPrice;
                        unitPrice = Math.Round(unitPrice, 4);
                        invoiceRowIODTO.UnitPrice = unitPrice;
                    }
                }


                //Unitprice is the same as Amount
                if (unitPrice != 0 && amount == 0)
                {
                    invoiceRowIODTO.Amount = invoiceRowIODTO.UnitPrice;
                    amount = (decimal)invoiceRowIODTO.Amount;
                }

                //Set quantity to 1 if 0 and there is an amount on the row
                if (!invoiceRowIODTO.Quantity.HasValue && (amount != 0 || sumAmount != 0))
                {
                    invoiceRowIODTO.Quantity = 1;
                    quantity = 1;
                }

                //Amounts
                if (quantity != 0 && unitPrice != 0 && invoiceRowIODTO.CustomerRowType == SoeInvoiceRowType.ProductRow) 
                    //&& sumAmount == 0) 
                {
                    decimal discountAmount = 0;

                    discountAmount = invoiceRowIODTO.DiscountAmount.HasValue ? invoiceRowIODTO.DiscountAmount.Value : 0;

                    if (discountAmount == 0 && invoiceRowIODTO.Discount.HasValue && invoiceRowIODTO.Discount != 0)
                        discountAmount = (quantity * amount) * (discount / 100m);

                    if (discountAmount != 0)
                    {
                        invoiceRowIODTO.DiscountAmount = discountAmount * currencyRate;
                        invoiceRowIODTO.DiscountAmountCurrency = discountAmount;
                    }

                    sumAmount = ((quantity * amount) - discountAmount) * currencyRate;
                    invoiceRowIODTO.SumAmount = sumAmount;
                }

                //Set VatAmount
                if (vatRate != 0 && vatAmount == 0 && sumAmount != 0 && invoiceRowIODTO.CustomerRowType == SoeInvoiceRowType.ProductRow)
                {
                    invoiceRowIODTO.VatAmount = sumAmount * (vatRate / 100);
                    vatAmount = (decimal)invoiceRowIODTO.VatAmount;
                }

                //Postive Vat and negative SumAmount
                if (vatAmount > 0 && sumAmount < 0 && invoiceRowIODTO.CustomerRowType == SoeInvoiceRowType.ProductRow)
                {
                    vatAmount = vatAmount * -1;
                    invoiceRowIODTO.VatAmount = (decimal?)vatAmount;
                }

                //Set Currency amounts
                if (amount != 0 && amountCurrency == 0 && invoiceRowIODTO.CustomerRowType == SoeInvoiceRowType.ProductRow)
                    invoiceRowIODTO.AmountCurrency = amount;

                if (sumAmount != 0 && sumAmountCurrency == 0)
                    invoiceRowIODTO.SumAmountCurrency = sumAmount;
                
                if (vatAmount != 0 && vatAmountCurrency == 0)
                    invoiceRowIODTO.VatAmountCurrency = 0;

                if (invoiceRowIODTO.PurchasePrice.HasValue && invoiceRowIODTO.PurchasePrice != 0 && invoiceRowIODTO.PurchasePriceCurrency == 0)
                    invoiceRowIODTO.PurchasePriceCurrency = invoiceRowIODTO.PurchasePrice;

                if (invoiceRowIODTO.MarginalIncome.HasValue && invoiceRowIODTO.MarginalIncome != 0 && invoiceRowIODTO.MarginalIncomeCurrency == 0)
                    invoiceRowIODTO.MarginalIncomeCurrency = invoiceRowIODTO.MarginalIncome;

                if (invoiceRowIODTO.DiscountAmount.HasValue && invoiceRowIODTO.DiscountAmount != 0 && invoiceRowIODTO.DiscountAmountCurrency == 0)
                    invoiceRowIODTO.DiscountAmountCurrency = invoiceRowIODTO.DiscountAmount;

                #endregion

                customerInvoiceRows.Add(invoiceRowIODTO);
            }
        }
        #endregion
    }
}
