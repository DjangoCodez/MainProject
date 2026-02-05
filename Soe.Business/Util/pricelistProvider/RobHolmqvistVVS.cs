using SoftOne.Soe.Common.Util;
using System;
using System.Text;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class RobHolmqvistVVS : RowBasedProviderBase
    {
        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.RobHolmqvistVVS); }
        }

        protected override Encoding FileEncoding
        {
            get
            {
                return Constants.ENCODING_IBM865;
            }
        }

        protected override int SkipRows
        {
            get
            {
                return 1;
            }
        }

        protected override GenericProduct ToGenericProduct(string line)
        {
            /*
                Fältnamn	        Från	Till	Fältlängd
                Artikelnummer	    1	    20	    20
                Beskrivning	        21	    50	    30
                Pris heltal	        51	    57	    7
                Punkt	            58	    58	    1
                Pris decimal	    59	    60	    2
                Radrabatt-kod	    61	    70	    10
                Försäljningsenhet	71	    80	    10
            */
            var product = new GenericProduct();
            product.ProductId = line.Substring(0, 20).Trim();
            product.Name = line.Substring(20, 30).Trim();
            product.Price = Convert.ToDecimal(line.Substring(50, 10).Replace('.',','));
            product.Code = line.Substring(60, 10).Trim();
            product.PurchaseUnit = product.SalesUnit = line.Substring(70, 10).Trim();

            return product;
        }
    }
}
