using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Onninen : IPriceListProvider
    {
        #region Variables

        List<OnninenPost> posts;

        #endregion

        #region Ctor

        public Onninen()
        {
            posts = new List<OnninenPost>();
        }

        #endregion

        #region IPriceListProvider Members

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                string[] items = line.Split(";".ToCharArray());
                try
                {
                    posts.Add(new OnninenPost(items));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Onninen);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now);

            foreach (var product in posts)
            {
                var gp = new GenericProduct();

                if (product.EAN != null)
                    gp.EAN = product.EAN;

                gp.PurchaseUnit = product.Unit;
                gp.SalesUnit = product.Unit;
                gp.Name = product.Name;
                gp.ProductId = product.ProductId;
                gp.Price = product.Price;
                gp.Code = string.IsNullOrEmpty(product.MaterialCode) ? product.MaterialCode : product.ProductGroup;
                gp.Code = string.IsNullOrEmpty(product.ProductGroup) ? product.ProductGroup : product.MaterialCode;
                gp.ProductLink = product.ProductLink;

                gp.WholesellerName = wholeSeller;
                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion
    }

    public class OnninenPost
    {
        #region Members

        public string ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string MaterialCode { get; set; }
        public string ProductGroup { get; set; }
        public string Unit { get; set; }
        public string EAN { get; set; }
        public string ProductLink { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Used to mock in unit test
        /// </summary>
        public OnninenPost()
        {
            //Empty constructor
        }

        public OnninenPost(string[] items)
        {
            ProductId = items[0].Trim();
            Name = items[3].Trim();
            Price = GetAmount(items[4].Trim());
            MaterialCode = items[1].Trim();
            ProductGroup = items[2].Trim();
            Unit = items[5].Trim();
            ProductLink = items.LastOrDefault();

            //try
            //{
            //    if (!string.IsNullOrEmpty(items[12].Trim()))
            //        long.TryParse(items[12].Trim(), out EAN);
            //}
            //catch
            //{}
        }
        private decimal GetAmount(string item)
        {
            return Convert.ToDecimal(item);
        }
        #endregion

        
    }
}
