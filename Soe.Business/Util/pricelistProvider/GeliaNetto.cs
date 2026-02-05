using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class GeliaNetto : ExcelProviderBase
    {
        private enum GeliaNettoPositions
        {
            Product_Number = 13,
            Product_Name = 2,
            Product_Price = 3,
            EAN_Code = 6,
            Product_Group1 = 9,
            Unit = 10,
        }

        private readonly SoeCompPriceListProvider provider;

        public GeliaNetto(SoeCompPriceListProvider provider)
        {
            this.provider = provider;
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            var product = new GenericProduct()
            {
                Name = row[(int)GeliaNettoPositions.Product_Name].ToString(),
                Price = Convert.ToDecimal(row[(int)GeliaNettoPositions.Product_Price].ToString()),
                SalesUnit = row[(int)GeliaNettoPositions.Unit].ToString(),
                EAN = row[(int)GeliaNettoPositions.EAN_Code].ToString(),
                ProductId = row[(int)GeliaNettoPositions.Product_Number].ToString(),
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
