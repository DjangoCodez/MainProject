using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class CurrentumExcel : ExcelProviderBase
    {
        private readonly Dictionary<string, SoeSysPriceListProviderType> productCodes;
        private readonly SoeSysPriceListProvider priceListProvider;
        private readonly string wholeSeller = "Currentum";

        public CurrentumExcel(SoeSysPriceListProvider provider)
        {
            priceListProvider = provider;

            if (provider == SoeSysPriceListProvider.Currentum_Ahlsell)
            {
                var spm = new SysPriceListManager(null);
                productCodes = spm.GetProductCodesForWholeseller(new List<int> { 2, 14, 15 });
                wholeSeller = "Ahlsell";
            }
        }

        protected override string WholesellerName
        {
            get { return wholeSeller; }
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            decimal price = 0;
            
            var productNr = row[0].ToString();
            if (string.IsNullOrEmpty(productNr))
                return null;

            if (!decimal.TryParse(row[1].ToString(), out price))
                return null;

            var product = new GenericProduct()
            {
                ProductId = productNr,
                Price = price,
                ProductType = SoeSysPriceListProviderType.Unknown,
                Name = row[5].ToString(),
                Code = row[2].ToString(),
                PurchaseUnit = row[3].ToString()
            };

            if (priceListProvider == SoeSysPriceListProvider.Currentum_Ahlsell && !string.IsNullOrEmpty(product.Code) && productCodes.TryGetValue(product.Code, out var type))
            {
                product.ProductType = type;
            }

            if (product.ProductType == SoeSysPriceListProviderType.Unknown)
            {
                return null;
            }

            return product;
        }
    }
}
