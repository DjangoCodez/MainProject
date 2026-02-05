using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class MalmbergFI: ExcelProviderBase
    {
        private enum MalmbergFIColumnPositions
        {        
            Product_Group = 0,
            Product_Number = 1,         
            Product_Name = 2,            
            EAN_Code = 3,
            Unit = 4,
            Product_Price = 5,            
        }

        private SoeCompPriceListProvider provider;

        public MalmbergFI(SoeCompPriceListProvider provider)
        {
            this.provider = provider;
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            var l = new Dictionary<MalmbergFIColumnPositions, int>();
            var product = new GenericProduct()
            {
                Name = row[(int)MalmbergFIColumnPositions.Product_Name].ToString(),
                Price = Convert.ToDecimal(row[(int)MalmbergFIColumnPositions.Product_Price].ToString()),
                SalesUnit = row[(int)MalmbergFIColumnPositions.Unit].ToString(),
                EAN = row[(int)MalmbergFIColumnPositions.EAN_Code].ToString(),                
                ProductId = row[(int)MalmbergFIColumnPositions.Product_Number].ToString(),
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
