using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class BadVarme : IPriceListProvider
    {
        private readonly List<GenericProduct> posts = new List<GenericProduct>();
        private readonly string wholeSeller = "Bad & Värme";

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
                    var rowType = line.Substring(0, 1);
                    if (rowType == "2" && line.Length > 109)
                    {
                        var purchasePrice = line.Substring(102, 6).Trim();

                        //# is and error price according to bad och värme....mostly due to negative prices (discount)
                        if (string.IsNullOrEmpty(purchasePrice) || purchasePrice.Contains("#")) continue;

                        var product = new GenericProduct
                            {
                                ProductId = line.Substring(1, 15).Trim(),
                                Name = line.Substring(16, 40).Trim(),
                                Price = GetAmount(purchasePrice, line.Substring(108, 2)),
                                PurchaseUnit = line.Substring(70, 2).Trim(),
                                EAN = line.Substring(89,13).Trim(),
                                //Code = line.Substring(84, 6).Trim(),
                                SalesPrice = GetAmount(line.Substring(56, 6), line.Substring(62, 2)),
                                WholesellerName = wholeSeller,
                            };

                        //number = RSK
                        product.ProductType = StringUtility.IsNumeric(product.ProductId) ? SoeSysPriceListProviderType.Plumbing: SoeSysPriceListProviderType.BadVarme;
                        if (product.EAN.Length != 13)
                        {
                            product.EAN = string.Empty;
                        }
                        
                        posts.Add(product);
                    }
                }
                catch (Exception)
                {

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

        private decimal GetAmount(string item, string cents)
        {
            var value = decimal.Parse(item);
            var centValue = int.Parse(cents);
            if (centValue > 0) {
                value += (decimal)centValue / 100;
            }

            return value;
        }

        #endregion
    }

}
