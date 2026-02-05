using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Solar : IPriceListProvider, IFileValidator<IPriceListProvider>
    {
        #region Members
        List<SolarPost> products;
        #endregion

        #region Constructors
        public Solar()
        {
            products = new List<SolarPost>();
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
                line = line.PadRight(77);
                try
                {
                    products.Add(new SolarPost(line));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Solar);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled
            foreach (SolarPost product in products)
            {
                var gp = new GenericProduct();

                gp.ProductId = product.ProductId;
                gp.Name = product.Name;
                gp.SalesUnit = product.Unit;
                gp.PurchaseUnit = product.Unit;
                gp.Code = product.RebateGroup;
                gp.Price = product.Price;

                gp.WholesellerName = wholeSeller; //for relation

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
            return new SolarCSV();
        }
        #endregion

        public class SolarPost
        {
            #region Members
            public string ProductId { get; set; }
            public string Name { get; set; }
            public string Unit { get; set; }
            public string RebateGroup { get; set; }
            public decimal Price { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public SolarPost()
            {
                //Empty constructor
            }

            public SolarPost(string item)
            {
                ProductId = item.Substring(5, 7);
                Name = item.Substring(17, 41);
                RebateGroup = item.Substring(58, 5);
                Unit = item.Substring(63, 4);
                Price = GetAmount(item.Substring(67, 10));
            }
            #endregion

            #region Help methods
            private int GetEan(string item)
            {
                int result = 0;
                item = item.Trim();
                if (!string.IsNullOrEmpty(item))
                    int.TryParse(item, out result);
                return result;
            }
            private decimal GetAmount(string item)
            {
                return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
            }
            private bool ParseBool(string item)
            {
                item = item.ToLower();
                if (item == "j")
                    return true;
                if (item == "n" || item == " ")
                    return false;
                return false;
            }
            #endregion
        }
    }
}
