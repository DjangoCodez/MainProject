using SoftOne.Soe.Common.Util;
using System;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class ThermotechExcel : ExcelProviderBase
    {
        public ThermotechExcel()
        {
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Thermotech); }
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            decimal price = 0;
            
            var rskNumber = row[1].ToString();
            if (string.IsNullOrEmpty(rskNumber))
                return null;

            if (!decimal.TryParse(row[4].ToString(), out price))
                return null;

            var product = new GenericProduct()
            {
                ProductId = rskNumber,
                Price = price,
                ProductType = SoeSysPriceListProviderType.Plumbing,
                Name = row[2].ToString(),
                ExtendedInfo = row[3].ToString(),
                Code = row[5].ToString(),
            };

            return product;
        }
    }
}
