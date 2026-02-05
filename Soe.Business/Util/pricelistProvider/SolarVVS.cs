using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class SolarVVS : IPriceListProvider, IFileValidator<IPriceListProvider>
    {
        #region Members
        private readonly List<SolarVVSPost> products;
        #endregion

        #region Constructors
        public SolarVVS()
        {
            products = new List<SolarVVSPost>();
        }
        #endregion

        #region Public methods
        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(75);
                try
                {
                    products.Add(new SolarVVSPost(line));
                }
                catch (Exception ex) //hide error
                {
                    ex.ToString(); //prevent compiler warning
                }
            }
            return result;
        }
        public GenericProvider ToGeneric()
        {
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.SolarVVS);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled
            foreach (SolarVVSPost product in products)
            {
                var gp = new GenericProduct();

                gp.ProductId = product.ProductId;
                gp.Name = product.Name;
                gp.Price = product.Price;
                gp.Code = product.MaterialCode;
                gp.WholesellerName = wholeSeller; //for relation
                gp.SalesUnit = product.StorageUnit;

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        public ActionResult ValidateFile(Stream stream)
        {
            var result = SolarCSV.ValidateFile(stream);
            // Invert result
            result.Success = !result.Success;
            return result;
        }

        public IPriceListProvider GetSecondaryProvider()
        {
            // Solar is changing format to csv, so try that
            return new SolarCSV(SoeSysPriceListProvider.SolarVVS);
        }
        #endregion

        public class SolarVVSPost
        {
            #region Members
            public string ProductId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string MaterialCode { get; set; }
            public string StorageUnit { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public SolarVVSPost()
            {
                //Empty constructor
            }

            public SolarVVSPost(string item) //Not verified against any specification only against file, positions questionable
            {
                ProductId = item.Substring(1, 7);
                Name = item.Substring(15, 40);
                Price = GetAmount(item.Substring(63, 8));
                StorageUnit = item.Substring(81, 3);
                MaterialCode = item.Substring(71, 4);
            }

            #endregion

            #region Help methods

            private decimal GetAmount(string item)
            {
                item = item.Trim().Replace(".", "");
                return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
            }
            #endregion
        }
    }
}
