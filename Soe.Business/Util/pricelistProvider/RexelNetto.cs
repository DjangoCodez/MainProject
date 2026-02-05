using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Data;


namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class RexelNetto : ExcelProviderBase
    {
        public RexelNetto()
        {
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Rexel); }
        }

        protected override GenericProduct ToGenericProduct(DataRow row)
        {
            decimal price = 0;
            decimal netPrice = 0;
            string productNr = row[0].ToString();

            if (!decimal.TryParse(row[4].ToString(), out price))
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

        protected override ActionResult Validate(string fileName, int nrOfColumns)
        {
            var result = base.Validate(fileName, nrOfColumns);
            if (!result.Success)
                return result;

            if (nrOfColumns != 9)
            {
                return new ActionResult(TermCacheManager.Instance.GetText(516, (int)TermGroup.General, "Filen innehåller felaktiga rubriker eller saknar data."));
            }

            return result;
        }
    }
}
