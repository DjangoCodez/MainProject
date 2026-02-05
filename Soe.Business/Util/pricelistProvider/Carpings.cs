using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Carpings : IPriceListProvider
    {
        #region Members

        List<CarpingsPost> posts;

        #endregion

        #region Constructors

        public Carpings()
        {
            posts = new List<CarpingsPost>();
        }

        #endregion

        #region Public methods

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Encoding.Default, true);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                //line = line.PadRight(96);
                try
                {
                    posts.Add(new CarpingsPost(line));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Carpings);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            foreach (CarpingsPost product in posts)
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

        public class CarpingsPost
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
            public CarpingsPost()
            {
                //Empty constructor
            }

            public CarpingsPost(string item)
            {
                ProductId = item.Substring(0, 10);
                Name = item.Substring(10, 40);
                Price = GetAmount(item.Substring(45, 8));
                MaterialCode = item.Substring(53, 5);
                StorageUnit = item.Substring(40, 3);
            }

            #endregion

            #region Help methods

            private decimal GetAmount(string item)
            {
                item = item.Replace(",", "");
                item = item.Replace(".", "");
                return Convert.ToDecimal(item) / 100;
            }

            #endregion
        }
    }

}
