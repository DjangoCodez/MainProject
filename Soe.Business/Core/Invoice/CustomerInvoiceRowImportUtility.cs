using ExcelDataReader;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

public class ProductFileOutputDTO
{
    public string ArticleNr { get; set; }
    public string ArticleName { get; set; }
    public string Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
}

namespace SoftOne.Soe.Business.Core
{
    public static class CustomerInvoiceRowImportUtility
    {
        public static ActionResult ProductImport(CompEntities entities, byte[] fileData, TermGroup_InvoiceRowImportType fileTypeId, int actorCustomerId, int actorCompanyId, int wholesellerId, int invoiceId, CountryCurrencyManager currencyManager, AttestManager attestManager, ParameterObject parameterObject)
        {
            ActionResult result = new ActionResult(true);

            if (fileTypeId == TermGroup_InvoiceRowImportType.Excel) { 
                result.Value = ProductExcelImport(entities, fileData, actorCustomerId, actorCompanyId, wholesellerId, invoiceId, currencyManager, attestManager, parameterObject);
                return result;
            }
            else if (fileTypeId == TermGroup_InvoiceRowImportType.Jcad) {
                result.Value = ProductJcadImport(entities, fileData, actorCustomerId, actorCompanyId, wholesellerId, invoiceId, currencyManager, attestManager, parameterObject);
                return result;
            }
            else return new ActionResult { Success = false, ErrorMessage = "Failed finding the file type" };
        }

    public static List<ProductRowDTO> ProductJcadImport(CompEntities entities, byte[] fileData, int actorCustomerId, int actorCompanyId, int wholesellerId, int invoiceId, CountryCurrencyManager currencyManager, AttestManager attestManager, ParameterObject parameterObject)
        {
            List<ProductFileOutputDTO> fileRows = new List<ProductFileOutputDTO>();
            string line;

            using (StreamReader reader = new StreamReader(new MemoryStream(fileData), true))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('\t');
                    if (parts.Length < 7)
                        continue;

                    // map the four columns
                    var fileDto = new ProductFileOutputDTO();
                    fileDto.ArticleNr = parts[0].Trim();
                    fileDto.ArticleName = parts[1].Trim();
                    fileDto.Unit = parts[4].Trim();
                    fileDto.Quantity = decimal.Parse(parts[6].Trim().ReplaceDecimalSeparator());
                    fileDto.Amount = 0;

                    fileRows.Add(fileDto);
                }
            }
            return ProductRowGenerator(entities, fileRows, actorCustomerId, actorCompanyId, wholesellerId, invoiceId, currencyManager, attestManager, parameterObject);
        }

        public static List<ProductRowDTO> ProductExcelImport(CompEntities entities, byte[] fileData, int actorCustomerId, int actorCompanyId, int wholesellerId, int invoiceId, CountryCurrencyManager currencyManager, AttestManager attestManager, ParameterObject parameterObject)
        {
            List<ProductFileOutputDTO> fileRows = new List<ProductFileOutputDTO>();
            MemoryStream stream = new MemoryStream(fileData);
            
            using (StreamReader read = new StreamReader(new MemoryStream(fileData), true))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(stream);
                DataSet ds = excelReader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = false } });
                List<DataRow> dataRows = ds.Tables[0].Rows.Cast<DataRow>().ToList();

                foreach (DataRow row in dataRows)
                {
                    if (dataRows.IndexOf(row) == 0)
                        continue;

                    var fileDto = new ProductFileOutputDTO();
                    
                    fileDto.ArticleNr = row[0].ToString();
                    fileDto.ArticleName = row[1].ToString();
                    fileDto.Unit = row[2].ToString();

                    var qty = row[3].ToString().ReplaceDecimalSeparator();
                    fileDto.Quantity = string.IsNullOrWhiteSpace(qty) ? 1 : decimal.Parse(qty);

                    var amountStr = row.Table.Columns.Count > 4 ? row[4].ToString().ReplaceDecimalSeparator() : "0";
                    fileDto.Amount = string.IsNullOrWhiteSpace(amountStr) ? 0 : decimal.Parse(amountStr);

                    fileRows.Add(fileDto);
                }
            }
            return ProductRowGenerator(entities, fileRows, actorCustomerId, actorCompanyId, wholesellerId, invoiceId, currencyManager, attestManager, parameterObject);
        }


        private static List<ProductRowDTO> ProductRowGenerator(CompEntities entities, List<ProductFileOutputDTO> fileRows, int actorCustomerId, int actorCompanyId, int sysWholesellerId, int invoiceId,  CountryCurrencyManager currencyManager, AttestManager attestManager, ParameterObject parameterObject)
        {
            ProductManager productManager = new ProductManager(parameterObject);
            InvoiceManager invoiceManager = new InvoiceManager(parameterObject);
            SettingManager settingManager = new SettingManager(parameterObject);
            CustomerManager customerManager = new CustomerManager(parameterObject);
            WholeSellerManager wholeSellerManager = new WholeSellerManager(parameterObject);
            StockManager stockManager = new StockManager(parameterObject);
            List<ProductRowDTO> productRows = new List<ProductRowDTO>();

            int defaultHouseholdDeductionType = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultHouseholdDeductionType, 0, actorCompanyId, 0);
            bool autoCreateDateOnProductRows = settingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingAutoCreateDateOnProductRows, 0, actorCompanyId, 0);
            int productMiscId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductMisc, 0, actorCompanyId, 0);
            int defaultStockId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultStock, 0, actorCompanyId, 0);
            int grossMarginMethod = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultGrossMarginCalculationType, 0, actorCompanyId, 0);


            var customerInvoice = invoiceManager.GetCustomerInvoiceSmallEx(entities,invoiceId);

            var initialAttestStateId = InvoiceAttestStates.GetInitialAttestStateId(attestManager, entities, actorCompanyId, (SoeOriginType)customerInvoice.OriginType);

            Customer customer = customerManager.GetCustomer(entities, customerInvoice.ActorId.GetValueOrDefault());
            List<ProductUnit> units = productManager.GetProductUnits(actorCompanyId);
            StockDTO defaultStock = stockManager.GetStockDTO(actorCompanyId, defaultStockId);

            var defaultWholesellerName = wholeSellerManager.GetWholesellerName(sysWholesellerId);

            foreach (ProductFileOutputDTO fileRow in fileRows)
            {
                InvoiceProductPriceResult productPriceDetails = null;
                decimal? miscPurchasePrice = null;
                InvoiceProduct invoiceProduct = GetInvoiceProduct(entities, actorCompanyId, productManager, productMiscId, sysWholesellerId, actorCustomerId, fileRow, ref miscPurchasePrice);

                int? householdDeductionType = null;
                decimal discountValue = 0;
                decimal discount2Value = 0;

                if (invoiceProduct != null)
                {
                    //HouseholdDeduction
                    if (invoiceProduct.HouseholdDeductionType > 0)
                    {
                        householdDeductionType = invoiceProduct.HouseholdDeductionType;
                    }
                    else if (defaultHouseholdDeductionType != 0)
                    {
                        householdDeductionType = defaultHouseholdDeductionType;
                    }

                    //Discounts
                    if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.Merchandise)
                    {
                        discountValue = (customer?.DiscountMerchandise ?? 0);
                        discount2Value = (customer?.Discount2Merchandise ?? 0);
                    }
                    else if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.Service)
                    {
                        discountValue = (customer?.DiscountService ?? 0);
                        discount2Value = (customer?.Discount2Service ?? 0);
                    }

                    ProductRowDTO row = new ProductRowDTO()
                    {
                        VatRate = invoiceProduct.VatCode?.Percent ?? 0,
                        VatCodeId = invoiceProduct.VatCodeId,
                        Type = SoeInvoiceRowType.ProductRow,
                        ProductUnitId = units.FirstOrDefault(u => string.Equals(u.Code, fileRow.Unit, StringComparison.OrdinalIgnoreCase))?.ProductUnitId ?? invoiceProduct.ProductUnitId,
                        Quantity = fileRow.Quantity,
                        ProductId = invoiceProduct.ProductId,
                        Text = fileRow.ArticleName ?? invoiceProduct.Name,
                        State = SoeEntityState.Active,
                        DiscountValue = discountValue,
                        Discount2Value = discount2Value,
                        DiscountType = (int)SoeInvoiceRowDiscountType.Percent,
                        Discount2Type = (int)SoeInvoiceRowDiscountType.Percent,
                        AttestStateId = initialAttestStateId,
                        Date = autoCreateDateOnProductRows ? (DateTime?)DateTime.Today : null,
                        HouseholdDeductionType = householdDeductionType,
                    };

                    PurchasePriceHandle(entities, actorCompanyId, defaultStockId, grossMarginMethod, productManager, stockManager, currencyManager, ref productPriceDetails, invoiceProduct, miscPurchasePrice, sysWholesellerId, defaultWholesellerName, customerInvoice.CurrencyRate, actorCustomerId, fileRow, row);
                    SalesPriceHandle(fileRow, productPriceDetails, invoiceProduct, currencyManager, customerInvoice.CurrencyRate, row);
                    StockHandle(entities, stockManager, defaultStockId, defaultStock, invoiceProduct, row);

                    productRows.Add(row);
                }
            }
            return productRows;
        }

        private static void SalesPriceHandle(ProductFileOutputDTO fileRow, InvoiceProductPriceResult productPriceDetails, InvoiceProduct invoiceProduct, CountryCurrencyManager currencyManager, decimal currencyRate, ProductRowDTO row)
        {
            decimal amount = fileRow.Amount > 0 ? fileRow.Amount : productPriceDetails.SalesPrice;

            if ((TermGroup_InvoiceProductCalculationType)invoiceProduct?.CalculationType == TermGroup_InvoiceProductCalculationType.Lift)
                amount = amount * -1;

            row.Amount = amount;
            row.AmountCurrency = decimal.Round(currencyManager.GetCurrencyAmountFromBaseAmount(amount, currencyRate), 2);

       }

        private static void StockHandle(CompEntities entities, StockManager stockManager, int defaultStockId, StockDTO defaultStock, InvoiceProduct invoiceProduct, ProductRowDTO row)
        {
            if (invoiceProduct.IsStockProduct.GetValueOrDefault() && defaultStockId > 0)
            {
                var stockProduct = stockManager.GetStockProductFromInvoiceProduct(entities, invoiceProduct.ProductId, defaultStockId, 0);
                if (stockProduct != null)
                {
                    row.StockId = defaultStock?.StockId;
                    row.StockCode = defaultStock?.Code;
                }
            }
        }

        private static void PurchasePriceHandle(CompEntities entities, int actorCompanyId, int defaultStockId, int grossMarginMethod, ProductManager productManager, StockManager stockManager, CountryCurrencyManager currencyManager, ref InvoiceProductPriceResult productPriceDetails, InvoiceProduct invoiceProduct, decimal? miscPurchasePrice, int sysWholesellerId, string defaultWholesellerName, decimal currencyRate, int actorCustomerId, ProductFileOutputDTO fileRow, ProductRowDTO row)
        {
            decimal? finalPurchasePrice = null;
            productPriceDetails = productManager.GetProductPrice(entities, actorCompanyId, new ProductPriceRequestDTO { ProductId = invoiceProduct.ProductId, Quantity = fileRow.Quantity, CustomerId = actorCustomerId, WholesellerId = sysWholesellerId });
            decimal purchasePrice = (decimal?)invoiceProduct.PurchasePrice ?? productPriceDetails.PurchasePrice;
            finalPurchasePrice = miscPurchasePrice.HasValue ? miscPurchasePrice.Value : purchasePrice;

            //Average Price Calculation
            if (invoiceProduct.DefaultGrossMarginCalculationType != null ? (invoiceProduct.DefaultGrossMarginCalculationType.GetValueOrDefault() == (int)TermGroup_GrossMarginCalculationType.StockAveragePrice) : (grossMarginMethod == (int)TermGroup_GrossMarginCalculationType.StockAveragePrice))
            {
                StockProductAvgPriceDTO StockProductAvgPrice = stockManager.GetStockProductAvgPriceDTO(defaultStockId, invoiceProduct.ProductId, actorCompanyId);
                finalPurchasePrice = StockProductAvgPrice?.AvgPrice ?? invoiceProduct.PurchasePrice;
            }

            row.PurchasePrice = finalPurchasePrice.GetValueOrDefault();
            row.PurchasePriceCurrency = decimal.Round(currencyManager.GetCurrencyAmountFromBaseAmount(finalPurchasePrice.GetValueOrDefault(), currencyRate), 2);

            #region Wholeseller 

            var wholeseller = "";
            if (invoiceProduct.ExternalProductId.HasValue)
                wholeseller = productPriceDetails?.SysWholesellerName ?? defaultWholesellerName;
            row.SysWholesellerName = wholeseller;
            
            #endregion
        }

        private static InvoiceProduct GetInvoiceProduct(CompEntities entities, int actorCompanyId, ProductManager productManager, int productMiscId, int sysWholesellerId, int actorCustomerId, ProductFileOutputDTO fileRow, ref decimal? miscPurchasePrice)
        {
            #region Internal search

            InvoiceProduct invoiceProduct = productManager.GetInvoiceProductByProductNr(entities, fileRow.ArticleNr, actorCompanyId);

            #endregion

            #region External/Wholeseller search

            if (invoiceProduct == null)
                invoiceProduct = productManager.CopyExternalInvoiceProductFromSysByProductNr(entities, 0, fileRow.ArticleNr, 0, sysWholesellerId, fileRow.Unit, 0, null, actorCompanyId, actorCustomerId, true);

            #endregion


            #region miscellaneous

            if (invoiceProduct == null)
            {
                InvoiceProduct invoiceProductMisc = null;
                if (productMiscId > 0)
                {
                        invoiceProductMisc = productManager.GetInvoiceProduct(entities, productMiscId, true, false, false);
                        invoiceProduct = invoiceProductMisc;
                        miscPurchasePrice = 0;
                    }
                }

            #endregion

            return invoiceProduct;
        }

    }
}
