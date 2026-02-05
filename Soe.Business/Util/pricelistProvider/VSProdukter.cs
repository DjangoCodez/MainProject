using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class VSProdukter : IPriceListProvider
    {
        private readonly List<GenericProduct> posts = new List<GenericProduct>();
        private readonly string wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.VSProdukter);

        #region Public methods

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                
                try
                {
                    posts.Add(
                        new GenericProduct
                        {
                            ProductId = line.Substring(0, 10).Trim(),
                            Name = line.Substring(10, 30).Trim(),
                            Price = GetAmount(line.Substring(44, 8)),
                            Code = line.Substring(53, 5).Trim(),
                            PurchaseUnit = line.Substring(40, 3).Trim(),
                            WholesellerName = wholeSeller
                        }
                   );
                }
                catch
                {
                    // Ignore invalid lines
                    // NOSONAR
                }
            }
            return result;
        }

        public GenericProvider ToGeneric()
        {
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled
            foreach (var product in posts)
            {
                result.products.Add(result.products.Count, product);
            }

            return result;
        }

        private decimal GetAmount(string item)
        {
            item = item.Replace(",", "");
            item = item.Replace(".", "");
            return Convert.ToDecimal(item) / 100;
        }

        #endregion
    }

}
