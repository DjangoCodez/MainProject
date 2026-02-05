using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Bragross : IPriceListProvider
    {
        #region Members

        List<BragrossPost> posts;

        #endregion

        #region Constructors

        public Bragross()
        {
            posts = new List<BragrossPost>();
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
                    posts.Add(new BragrossPost(line));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Bragross);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            foreach (BragrossPost product in posts)
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

        public class BragrossPost
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
            public BragrossPost()
            {
                //Empty constructor
            }

            public BragrossPost(string item)
            {
                // 2358100HÅLSÅG IN SITU 127                      001190.00113100ST  
                ProductId = item.Substring(1, 7);
                Name = item.Substring(8, 40);
                Price = GetAmount(item.Substring(48, 9));
                MaterialCode = item.Substring(57, 6);
                StorageUnit = item.Substring(63, 4).TrimEnd(' ');
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
