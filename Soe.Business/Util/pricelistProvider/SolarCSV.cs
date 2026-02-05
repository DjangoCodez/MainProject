using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class SolarCSV : CSVProviderBase<SolarCSV.SolarColumnPositions>
    {
        private readonly SoeSysPriceListProvider provider;

        public enum SolarColumnPositions
        {
            Code = 0,
            ArticleNr = 1,
            RSK = 2,
            Name = 3,
            ProductGroup = 4,
            Unit = 5,
            ListPrice = 6,
            PriceDate = 8,
            Quantity = 9,
        }

        protected override int SkipRows
        {
            get
            {
                // First line is a header line
                return 1;
            }
        }

        public SolarCSV(SoeSysPriceListProvider provider = SoeSysPriceListProvider.Solar)
        {
            this.provider = provider;
        }

        public static ActionResult ValidateFileName(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.ToLowerInvariant();
                if (fileName.Contains("solar") && fileName.EndsWith(".csv"))
                    return new ActionResult();
            }

            return new ActionResult("*solar*.csv");
        }

        protected override GenericProduct ToGenericProduct(Dictionary<SolarCSV.SolarColumnPositions, string> dict)
        {
            var product = new GenericProduct()
            {
                Name = dict[SolarColumnPositions.Name],
                Code = dict[SolarColumnPositions.ProductGroup],
                Price = this.ToDecimal(dict[SolarColumnPositions.ListPrice]),
                PurchaseUnit = dict[SolarColumnPositions.Unit],
                SalesUnit = dict[SolarColumnPositions.Unit],
            };

            var RSKNummer = dict[SolarColumnPositions.RSK]?.Trim();
            var articleNr = dict[SolarColumnPositions.ArticleNr]?.Trim();

            if (!string.IsNullOrEmpty(RSKNummer) && this.provider == SoeSysPriceListProvider.SolarVVS)
            {
                product.ProductId = RSKNummer;
            }
            else if (!string.IsNullOrEmpty(articleNr) && this.provider == SoeSysPriceListProvider.Solar)
            {
                product.ProductId = articleNr;
            }
            else
            {
                return null;
            }

            DateTime changed;
            if (DateTime.TryParse(dict[SolarColumnPositions.PriceDate], out changed))
                product.PriceChangeDate = changed;

            return product;
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), this.provider); }
        }

        public static ActionResult ValidateFile(Stream stream)
        {
            ActionResult result = new ActionResult(false);

            using (var sr = new StreamReader(stream, Constants.ENCODING_LATIN1))
            {
                // First line is a header so skip this
                sr.ReadLine();
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line) && !sr.EndOfStream)
                    line = sr.ReadLine();

                // Try and see if this is a csv file
                var columns = line.Split(';');
                if (columns.Length > 7)
                {
                    decimal price;
                    if (decimal.TryParse(columns[(int)SolarColumnPositions.ListPrice], out price))
                    {
                        // Now we can be pretty sure this is Solars new csv format, so return true
                        result.Success = true;
                        return result;
                    }
                }
            }

            return result;
        }
    }
}
