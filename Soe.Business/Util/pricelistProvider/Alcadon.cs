using SoftOne.Soe.Common.Util;
using System;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Alcadon : CSVProviderBase
    {
        private enum AlcadonPositions
        {
            Product_Number = 0,
            Product_Name = 4,
            EAN_Code = 5,
            Unit = 8,
            Product_Price = 9,
            //Product_Group1 = 4,
            //Product_Group2 = 5,
            //Product_Group3 = 6,
        }

        private readonly SoeCompPriceListProvider provider;

        public Alcadon(SoeCompPriceListProvider provider)
        {
            this.provider = provider;
        }

        protected override GenericProduct ToGenericProduct(string[] row)
        {
            var product = new GenericProduct()
            {
                Name = row[(int)AlcadonPositions.Product_Name].ToString(),
                Price = Convert.ToDecimal(row[(int)AlcadonPositions.Product_Price].ToString()),
                SalesUnit = row[(int)AlcadonPositions.Unit].ToString(),
                EAN = row[(int)AlcadonPositions.EAN_Code].ToString(),
                ProductId = row[(int)AlcadonPositions.Product_Number].ToString(),
                WholesellerName = WholesellerName,
            };
            return product;
        }

        protected override string WholesellerName
        {
            get { return this.provider.ToString(); }
        }
    }
}
