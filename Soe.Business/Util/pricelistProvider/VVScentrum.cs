using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class VVScentrum : IPriceListProvider
    {
        #region Members
        VVScentrumOpeningPost openingPost;
        List<VVScentrumProductPost> productPosts;
        #endregion

        #region Constructors
        public VVScentrum()
        {
            productPosts = new List<VVScentrumProductPost>();
        }
        #endregion

        #region Public methods

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            //StreamReader sr = new StreamReader(stream, Constants.ENCODING850);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(92);
                try
                {
                    switch (GetPostType(line))
                    {
                        case VVScentrumPostType.OpeningPost:
                            openingPost = new VVScentrumOpeningPost(line);
                            break;
                        case VVScentrumPostType.ProductPost:
                            productPosts.Add(new VVScentrumProductPost(line));
                            break;
                    }
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.VVScentrum);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(openingPost.Date);

            foreach (VVScentrumProductPost product in productPosts)
            {
                var gp = new GenericProduct();
                gp.EAN = product.EAN;
                gp.PackageSizeMin = product.MinPackageSize;
                gp.Name = product.Name;
                gp.ProductId = product.ProductId;
                gp.Code = product.RebateGroup;
                gp.Storage = product.Storage;
                gp.PurchaseUnit = product.Unit;
                gp.SalesUnit = product.Unit;
                gp.WholesellerName = wholeSeller;
                if (product.NetPrice)
                    gp.Price = product.Price;
                else
                    gp.Price = product.Price;

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion

        #region Help methods
        private VVScentrumPostType GetPostType(string line)
        {
            line = line.Substring(0, 1);
            int result = 0;
            int.TryParse(line, out result);
            return (VVScentrumPostType)Convert.ToInt32(result);
        }
        #endregion

        #region VVScentrumPosts

        public class VVScentrumOpeningPost : IVVScentrumPost
        {
            #region Members
            public VVScentrumPostType PostType { get; set; }
            public DateTime Date { get; set; }
            public string Provider { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public VVScentrumOpeningPost()
            {
                //Empty constructor
            }

            public VVScentrumOpeningPost(string item)
            {
                PostType = VVScentrumPostType.OpeningPost;
                Date = GetDate(item.Substring(1, 8));
                Provider = item.Substring(10, 10);
            }
            #endregion

            #region Help method
            private DateTime GetDate(string item)
            {
                return DateTime.ParseExact(item, "yyyyMMdd", CultureInfo.InvariantCulture);
                /*
                int year = Convert.ToInt32(DateTime.Now.Year.ToString().Substring(0, 2) + item.Substring(0, 2));
                int month = Convert.ToInt32(item.Substring(2, 2));
                int day = Convert.ToInt32(item.Substring(4, 2));
                
                return new DateTime(year, month, day);
                */
            }
            #endregion
        }
        public class VVScentrumProductPost : IVVScentrumPost
        {
            #region Members
            public VVScentrumPostType PostType { get; set; }
            public string ProductId { get; set; }
            public string Name { get; set; }
            public string Unit { get; set; }
            public string RebateGroup { get; set; }
            public bool Storage { get; set; }
            public decimal Price { get; set; }
            public decimal MinPackageSize { get; set; }
            public bool NetPrice { get; set; }
            public string EAN { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public VVScentrumProductPost()
            {
                //Empty constructor
            }

            public VVScentrumProductPost(string item)
            {
                PostType = VVScentrumPostType.ProductPost;
                ProductId = item.Substring(1, 15);
                Name = item.Substring(16, 30);
                Unit = item.Substring(46, 4);
                RebateGroup = item.Substring(50, 6);
                Storage = item.Substring(57, 1).ToLower() == "j" ? true : false;
                Price = GetAmount(item.Substring(58, 10));
                MinPackageSize = Convert.ToDecimal(item.Substring(68, 9));
                NetPrice = ParseBool(item.Substring(78, 1));
                EAN = (item.Substring(79, 13));
            }

            #endregion

            #region Help methods
 
            private decimal GetAmount(string item)
            {
                item = item.Replace(".", "");
                item = item.Replace(",", "");
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
        #endregion

        #region Interfaces
        interface IVVScentrumPost
        {
            VVScentrumPostType PostType { get; set; }
        }
        #endregion

        #region Enumerations
        public enum VVScentrumPostType
        {
            Undefined = 0,
            OpeningPost = 1,
            ProductPost = 2,
        }
        #endregion
    }
}
