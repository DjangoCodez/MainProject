using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Ahlsell : IPriceListProvider
    {
        #region Members
        List<AhlsellPost> posts;
        string wholeSellerName;
        #endregion

        #region Constructors
        public Ahlsell(string wholeSellerName)
        {
            this.wholeSellerName = wholeSellerName;
            posts = new List<AhlsellPost>();
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
                line = line.PadRight(85);
                try
                {
                    posts.Add(new AhlsellPost(line));
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
            var wholeSeller = wholeSellerName; // "Ahlsell El";
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now);
            foreach (AhlsellPost post in posts)
            {
                result.products.Add(result.products.Count, new GenericProduct(post.ProductId, post.Name, post.NetPrice, post.MaterialClass, post.StorageUnit, post.EnvironmentFee, post.Storage, wholeSeller, post.State));
            }
            return result;
        }
        #endregion

        public class AhlsellPost
        {
            #region Members
            public string ProductId { get; set; }
            public decimal NetPrice { get; set; }
            public SoeProductPriceStatus State { get; set; }
            public string MaterialClass { get; set; }
            public string StorageUnit { get; set; }
            public bool EnvironmentFee { get; set; }
            public bool Storage { get; set; }
            public string Name { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public AhlsellPost()
            {
                //Empty constructor
            }

            public AhlsellPost(string item)
            {
                ProductId = item.Substring(0, 20).Trim();
                NetPrice = GetAmount(item.Substring(20, 12));
                State = SoeProductPriceStatus.PriceChange;

                if (NetPrice == 0)
                    State = SoeProductPriceStatus.PricedOnRequest;

                MaterialClass = item.Substring(32, 6);
                StorageUnit = item.Substring(38, 3);
                EnvironmentFee = false;
                Storage = ParseBool(item.Substring(41, 1));
                Name = item.Substring(42, 60).Trim();
            }
            #endregion

            #region Help methods
            private decimal GetAmount(string item)
            {
                int value = Convert.ToInt32(item);
                decimal amount = (decimal)value / 100;
                return amount;
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
