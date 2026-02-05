using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Comfort : IPriceListProvider
    {
        private readonly List<ComfortRecord> records = new List<ComfortRecord>();
        private readonly string wholeSeller = "Comfort";
        private readonly Dictionary<string, SoeSysPriceListProviderType> productCodes;
        private readonly SoeSysPriceListProvider priceListProvider;
        private DateTime priceListDate = DateTime.Today;

        public Comfort(SoeSysPriceListProvider provider)
        {
            priceListProvider = provider;
            var spm = new SysPriceListManager(null);

            if (provider == SoeSysPriceListProvider.Comfort_Ahlsell)
            {
                productCodes = spm.GetProductCodesForWholeseller(new List<int> { 2, 14, 15 });
                wholeSeller = "Ahlsell";
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Direkt)
            {
                wholeSeller = "Comfort";
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Solar)
            {
                wholeSeller = "Solar";
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Bevego)
            {
                wholeSeller = "Bevego";
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Dahl)
            {
                wholeSeller = "Dahl";
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Elektroskandia)
            {
                wholeSeller = "Elektroskandia";
            }
            else
            {
                throw new Exception("Invalid wholeseller");
            }
        }

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();

            if (fileName != null)
            {
                var datePart = Path.GetFileNameWithoutExtension(fileName).Split('_').LastOrDefault();
                if (!string.IsNullOrEmpty(datePart))
                {
                    if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        priceListDate = date;
                    }
                }
            }

            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                result = CreateComfortRecord(line);
                if (!result.Success)
                {
                    return result;
                }
            }

            return result;
        }

        private ActionResult CreateComfortRecord(string line)
        {
            string[] parts = line.Split(';');

            if (parts.Length > 11 && parts[0] == "1")
            {
                var record = new ComfortRecord
                {
                    ProductId = parts[3],
                    RSKNumber = parts[4] == "0" ? null : parts[4],
                    ProductName = parts[5]?.Trim(),
                    SupplierProductGroup = parts[20],
                    GNPPrice = decimal.Parse(parts[8]),
                    PurchaseUnit = parts[9]?.Trim(),
                    NetPrice = decimal.Parse(parts[10]),
                    EAN = parts[7]?.Trim(),
                    //SalesPrice = parts[11],
                    SalesUnit = parts[34]?.Trim()
                };

                if (!string.IsNullOrEmpty(record.RSKNumber))
                {
                    record.ProductType = SoeSysPriceListProviderType.Plumbing;
                }

                if (priceListProvider == SoeSysPriceListProvider.Comfort_Ahlsell && !string.IsNullOrEmpty(record.SupplierProductGroup) && record.ProductType == SoeSysPriceListProviderType.Unknown)
                {
                    record.ProductType = productCodes.ContainsKey(record.SupplierProductGroup) ? productCodes[record.SupplierProductGroup] : SoeSysPriceListProviderType.Unknown;
                }
                else if (priceListProvider == SoeSysPriceListProvider.Comfort_Solar && record.ProductType == SoeSysPriceListProviderType.Unknown)
                {
                    record.ProductType = SoeSysPriceListProviderType.Electrician;
                }
                else if (priceListProvider == SoeSysPriceListProvider.Comfort_Direkt)
                {
                    record.ProductType = SoeSysPriceListProviderType.Comfort;
                    record.ProductId = parts[2];
                    record.SupplierName = parts[1]?.Trim();
                }
                else if (priceListProvider == SoeSysPriceListProvider.Comfort_Bevego && record.ProductType == SoeSysPriceListProviderType.Unknown)
                {
                    record.ProductType = SoeSysPriceListProviderType.Bevego;
                }
                else if (priceListProvider == SoeSysPriceListProvider.Comfort_Dahl && record.ProductType == SoeSysPriceListProviderType.Unknown)
                {
                    //only plumbing products for now....
                    return new ActionResult();
                }
                else if (priceListProvider == SoeSysPriceListProvider.Comfort_Elektroskandia && record.ProductType == SoeSysPriceListProviderType.Unknown && StringUtility.IsNumeric(record.ProductId))
                {
                    //some like VK11827 are Elektroskandia products
                    record.ProductType = SoeSysPriceListProviderType.Electrician;
                }

                if (record.ProductType != SoeSysPriceListProviderType.Unknown)
                {
                    records.Add(record);
                }
            }

            return new ActionResult();
        }

        public GenericProvider ToGeneric()
        {
            GenericProvider provider = new GenericProvider(wholeSeller);
            provider.header = new GenericHeader(priceListDate);
            foreach (var record in records)
            {
                var product = new GenericProduct
                {
                    ProductId = record.RSKNumber ?? record.ProductId,
                    Name = record.ProductName,
                    Price = record.GNPPrice,
                    NetPrice = record.NetPrice,
                    WholesellerName = wholeSeller,
                    ProductType = record.ProductType,
                    Code = record.SupplierProductGroup,
                    EAN = record.EAN,
                    SalesUnit = record.SalesUnit,
                    PurchaseUnit = record.PurchaseUnit,
                    Manufacturer = record.SupplierName
                };
                provider.products.Add(provider.products.Count, product);
            }
            return provider;
        }
        public static ActionResult ValidateFileName(SoeSysPriceListProvider provider, string fileName)
        {
            if (provider == SoeSysPriceListProvider.Comfort_Direkt && !fileName.ToUpper().StartsWith("Prisfil_Direktleverantörer".ToUpper()))
            {
                return new ActionResult("Prisfil_Direktleverantörer_DATUM.txt");
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Ahlsell && !fileName.ToUpper().StartsWith("Prisfil_Ahlsell".ToUpper()))
            {
                return new ActionResult("Prisfil_Ahlsell AB_DATUM.txt");
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Solar && !fileName.ToUpper().StartsWith("Prisfil_Solar".ToUpper()))
            {
                return new ActionResult("Prisfil_Solar_Sverige AB_DATUM.txt");
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Bevego && !fileName.ToUpper().StartsWith("Prisfil_Bevego".ToUpper()))
            {
                return new ActionResult("Prisfil_Bevego_DATUM.txt");
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Elektroskandia && !fileName.ToUpper().StartsWith("Prisfil_Elektroskandia".ToUpper()))
            {
                return new ActionResult("Prisfil_Elektroskandia_DATUM.txt");
            }
            else if (provider == SoeSysPriceListProvider.Comfort_Dahl && !fileName.ToUpper().StartsWith("Prisfil_Dahl".ToUpper()))
            {
                return new ActionResult("Prisfil_Dahl Sverige AB_DATUM.txt");
            }

            return new ActionResult();
        }
    }

    public class ComfortRecord
    {
        public string SupplierName { get; set; }
        public string SupplierProductGroup { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; } 
        public string RSKNumber { get; set; }
        public decimal GNPPrice { get; set; }
        public decimal NetPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public string SalesUnit { get; set; }
        public string PurchaseUnit { get; set; }
        public string EAN { get; set; }
        public SoeSysPriceListProviderType ProductType { get; set; }
    }
}
