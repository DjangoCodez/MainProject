using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Elkedjan : RowBasedProviderBase
    {
        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Elkedjan); }
        }

        protected override GenericProduct ToGenericProduct(string line)
        {
            /*
                Fältnamn	        Från	Fältlängd
                Artikelnummer	    1	    8
                Beskrivning	        9	    30
                Pris    	        52	    8
                Radrabatt-kod	    39	    4
                Försäljningsenhet	43	    3
            */
            var product = new GenericProduct();
            product.ProductId = line.Substring(0, 8).Trim();
            product.Name = line.Substring(8, 30).Trim();
            product.Price = Convert.ToDecimal(line.Substring(51, 8).Trim().Replace('.', ',')) / (decimal)100;
            product.Code = line.Substring(38, 4).Trim();
            product.PurchaseUnit = product.SalesUnit = line.Substring(42, 3).Trim();

            return product;
        }
    }
}
