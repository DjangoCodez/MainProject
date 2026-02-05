using SoftOne.Soe.Common.Util;
using System;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class ElkedjanNetto : ExcelProviderBase
    {
        public ElkedjanNetto()
        {
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Elkedjan); }
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            Decimal price = 0;
            Decimal netPrice = 0;
            if (!Decimal.TryParse(row[5].ToString(), out price))
                return null;
            if (!Decimal.TryParse(row[6].ToString(), out netPrice))
                return null;
            if(netPrice == 0)
                return null;

            var product = new GenericProduct()
            {
                ProductId = row[0].ToString(),
                Price = price,
                NetPrice = netPrice,
                ProductType = SoeSysPriceListProviderType.Electrician,
            };

            return product;
        }
    }
}
