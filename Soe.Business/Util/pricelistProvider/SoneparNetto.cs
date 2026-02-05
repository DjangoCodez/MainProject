using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class SoneparNetto : ExcelProviderBase
    {
        public SoneparNetto()
        {
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Sonepar); }
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            decimal price = 0;
            decimal netPrice = 0;
            string productNr = row[0].ToString();

            //only fetch electrician products
            if (string.IsNullOrEmpty(productNr) || !productNr.StartsWith("E"))
                return null;

            productNr = productNr.Right(productNr.Length-1); //remove E

            if (!decimal.TryParse(row[5].ToString(), out price))
                return null;
            if (!decimal.TryParse(row[7].ToString(), out netPrice))
                return null;
            if (netPrice == 0)
                return null;

            var product = new GenericProduct()
            {
                ProductId = productNr,
                Price = Math.Round(price, 2),
                NetPrice = Math.Round(netPrice, 2),
                ProductType = SoeSysPriceListProviderType.Electrician,
            };

            return product;
        }

        protected override ActionResult Validate(string fileName, int columnCount)
        {
            var result = base.Validate(fileName, columnCount);
            if (!result.Success)
                return result;  

            if (columnCount != 8)
            {
                return new ActionResult(TermCacheManager.Instance.GetText(516, (int)TermGroup.General, "Filen innehåller felaktiga rubriker eller saknar data."));
            }

            return result;
        }
    }
}
