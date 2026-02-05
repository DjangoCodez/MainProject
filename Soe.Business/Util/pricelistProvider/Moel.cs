using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Moel : IPriceListProvider
    {
        #region Members
        List<MoelPost> products;
        #endregion

        #region Constructors
        public Moel()
        {
            products = new List<MoelPost>();
        }
        #endregion

        #region Public methods
        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_IBM437);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                if (string.IsNullOrEmpty(line)) continue;

                int tmp; //to prevent -> for last row to be read
                if (!int.TryParse(line.Substring(0, 1), out tmp)) continue;

                line = line.PadRight(60);
                try
                {
                    products.Add(new MoelPost(line));
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
            var wholeSeller = "Moel";
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now);

            foreach (MoelPost product in products)
            {
                int i = 0;
                try
                {
                    var gp = new GenericProduct();
                    gp.ProductId = product.ProductId;
                    gp.Name = product.Name;
                    gp.Price = product.Price;
                    gp.Code = product.MaterialCode;
                    gp.PurchaseUnit = product.Unit;
                    gp.SalesUnit = product.Unit;
                    gp.WholesellerName = wholeSeller;
                    i++;
                    result.products.Add(result.products.Count, gp);
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }

            }
            return result;
        }
        #endregion

        public class MoelPost
        {
            #region Members

            public string ProductId { get; set; }
            public string Name { get; set; }
            public string MaterialCode { get; set; }
            public decimal Price { get; set; }
            public string Unit { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public MoelPost()
            {
                //Empty constructor
            }

            public MoelPost(string item)
            {
                try
                {
                    ProductId = item.Substring(1, 8).Trim();
                    Name = item.Substring(9, 26).Trim();
                    Price = GetAmount(item.Substring(34, 8));
                    MaterialCode = item.Substring(42, 4).Trim();
                    Unit = item.Substring(46, 3);
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }
            }
            #endregion

            #region Help methods
            private decimal GetAmount(string item)
            {
                return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
            }
            #endregion
        }
    }
}
