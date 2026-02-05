using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Elgrossen : IPriceListProvider
    {
        #region Members

        List<ElgrossenPost> posts;

        #endregion

        #region Constructors

        public Elgrossen()
        {
            posts = new List<ElgrossenPost>();
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
                //line = line.PadRight(96);
                try
                {
                    posts.Add(new ElgrossenPost(line));
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
            var wholeSeller = "Elgrossen";
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            foreach (ElgrossenPost product in posts)
            {
                var gp = new GenericProduct();

                gp.ProductId = product.ProductId;
                gp.Name = product.Name;
                gp.Price = product.Price;
                gp.Code = product.MaterialCode;
                gp.SalesUnit = product.StorageUnit;
                gp.PurchaseUnit = product.StorageUnit;

                gp.WholesellerName = wholeSeller; //for relation

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion

        public class ElgrossenPost
        {
            #region Members

            public string ProductId { get; set; }
            public decimal Price { get; set; }
            public string MaterialCode { get; set; }
            public string StorageUnit { get; set; }
            public string Name { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public ElgrossenPost()
            {
                //Empty constructor
            }

            public ElgrossenPost(string item)
            {
                ProductId = item.Substring(0, 7);
                Name = item.Substring(10, 50);
                Price = GetAmount(item.Substring(85, 10));
                MaterialCode = item.Substring(80, 3);
               StorageUnit = item.Substring(96, (item.Length - 96));
            }

            #endregion

            #region Help methods

            private decimal GetAmount(string item)
            {
                item = item.Replace(",", "");
                item = item.Replace(".", "");
                return Convert.ToDecimal(item) / 10;
            }

            #endregion
        }
    }

}
