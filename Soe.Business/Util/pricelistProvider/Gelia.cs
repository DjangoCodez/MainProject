using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Gelia : RowBasedProviderBase
    {
        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Gelia); }
        }

        protected override GenericProduct ToGenericProduct(string line)
        {
            /*
                Fältnamn	        Från	Fältlängd
                Artikelnummer	    1	    10
                Beskrivning	        12	    51
                Pris    	        64	    12
                EAN                 78      13
                Radrabatt-kod	    103	    4
                Försäljningsenhet	112	    5
            */
            var product = new GenericProduct();
            product.ProductId = line.Substring(0, 10).Trim();
            product.Name = line.Substring(11, 51).Trim();
            product.Price = Convert.ToDecimal(line.Substring(63, 12).Trim().Replace('.', ','));
            product.EAN = line.Substring(77, 13)?.Trim();
            product.Code = line.Substring(102, 4).Trim();
            product.PurchaseUnit = product.SalesUnit = line.Substring(111, 5).Trim();

            return product;
        }
    }
}
