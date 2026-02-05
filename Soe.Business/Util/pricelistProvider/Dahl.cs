using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Dahl : IPriceListProvider
    {
        #region Members
        DahlOpeningPost openingPost;
        readonly List<DahlProductPost> productPosts;
        #endregion

        #region Constructors
        public Dahl()
        {
            productPosts = new List<DahlProductPost>();
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
                line = line.PadRight(92);
                try
                {
                    switch (GetPostType(line))
                    {
                        case DahlPostType.OpeningPost:
                            openingPost = new DahlOpeningPost(line);
                            break;
                        case DahlPostType.ProductPost:
                            productPosts.Add(new DahlProductPost(line));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Dahl);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(openingPost.Date);

            foreach (DahlProductPost product in productPosts)
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
                gp.Price = product.Price;

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion

        #region Help methods
        private DahlPostType GetPostType(string line)
        {
            line = line.Substring(0, 1);
            int result = 0;
            int.TryParse(line, out result);
            return (DahlPostType)Convert.ToInt32(result);
        }
        #endregion

        #region DahlPosts

        public class DahlOpeningPost : IDahlPost
        {
            #region Members
            public DahlPostType PostType { get; set; }
            public DateTime Date { get; set; }
            public string Provider { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public DahlOpeningPost()
            {
                //Empty constructor
            }

            public DahlOpeningPost(string item)
            {
                PostType = DahlPostType.OpeningPost;
                Date = GetDate(item.Substring(1, 8));
                Provider = item.Substring(8, 4);
            }
            #endregion

            #region Help method
            private DateTime GetDate(string item)
            {
                int year = Convert.ToInt32(item.Substring(0, 4));
                int month = Convert.ToInt32(item.Substring(4, 2));
                int day = Convert.ToInt32(item.Substring(6, 2));
                return new DateTime(year, month, day);
            }
            #endregion
        }
        public class DahlProductPost : IDahlPost
        {
            #region Members
            public DahlPostType PostType { get; set; }
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
            public DahlProductPost()
            {
                //Empty constructor
            }

            public DahlProductPost(string item)
            {
                /*
                                            Ant. tecken	    Position	        Förklaring
                Radtyp		                9(1) 	        1  -      1	        1 = startpost, 2 = artikelpost
                Artikelnummer X             X (15)	        2  -     16	
                Benämning		            X(30)	        17  -    46	
                Enhet		                X(4)	        47  -    50
                Rabattgrupp	                9(6)	        51  -    56
                Beräkn. av bonus 	        X	            57  -    57	        (B=byggande,G=givande, N=nej)
                Lagervara          	        X	            58  -    58    	    (J=ja, N=nej)
                Pris		                9(7).99	        59  -    68	
                Minsta beställbara kvt.     9(9)	        69  -    77
                Strukturartikel	            X	            78  -    78         (S = Ja)
                Nettopris		            X	            79  -    79         ( J = Ja,  N = Nej)
                EANKOD                      X(13)           80  -    92      
                */

                PostType = DahlPostType.ProductPost;
                ProductId = item.Substring(1, 15);
                Name = item.Substring(16, 30);
                Unit = item.Substring(46, 4);
                RebateGroup = item.Substring(50, 6);
                Storage = item.Substring(57, 1).ToLower() == "j" ? true : false;
                Price = GetAmount(item.Substring(58, 10));
                MinPackageSize = Convert.ToDecimal(item.Substring(68, 9).Replace('.', ',').Trim());
                NetPrice = ParseBool(item.Substring(78, 1));
                EAN = (item.Substring(79, 13));
            }

            #endregion

            #region Help methods

            private static decimal GetAmount(string item)
            {
                item = item.Replace(".", "");
                item = item.Replace(",", "");
                return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
            }
            private static bool ParseBool(string item)
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
        interface IDahlPost
        {
            DahlPostType PostType { get; set; }
        }
        #endregion

        #region Enumerations
        public enum DahlPostType
        {
            Undefined = 0,
            OpeningPost = 1,
            ProductPost = 2,
        }
        #endregion
    }


    public class DahlNetPrice : IPriceListProvider
    {
        #region Members

        private readonly List<DahlNetPricePost> productPosts;

        #endregion

        #region Constructors
        public DahlNetPrice()
        {
            productPosts = new List<DahlNetPricePost>();
        }

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();

            if (!DahlNetPrice.ValidFileName(fileName))
            {
                return new ActionResult(false, 0, "Ogiltigt filformat för Dahl filerna, ska vara .txt");
            }

            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(92);
                try
                {
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("00"))
                    {
                        var post = new DahlNetPricePost(line);
                        if (!string.IsNullOrEmpty(post.ProductId))
                        {
                            productPosts.Add(post);
                        }
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Dahl);
            var result = new GenericProvider(wholeSeller);

            foreach (var product in productPosts)
            {
                var gp = new GenericProduct();
                gp.WholesellerName = wholeSeller;
                gp.NetPrice = product.NetPrice;
                gp.ProductId = product.ProductId;
                gp.ProductType = product.ProductType;

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }


        public static bool ValidFileName(string fileName)
        {
            return Path.GetExtension(fileName.ToLower()) == ".txt";
        }

        #endregion

        public class DahlNetPricePost
        {
            public string ProductId { get; set; }
            public decimal NetPrice { get; set; }
            public SoeSysPriceListProviderType ProductType { get; set; }

            public DahlNetPricePost(string item)
            {
                ProductId = item.Substring(0, 15).Trim();
                NetPrice = GetAmount(item.Substring(16, 10));
                ProductType = SoeSysPriceListProviderType.Plumbing;
                if (ProductId.StartsWith("E"))
                {
                    ProductId = ProductId.Remove(0,1);
                    ProductType = SoeSysPriceListProviderType.Electrician;
                }
                else if (ProductId.StartsWith("K")) {
                    ProductId = "";
                }
            }
            private static decimal GetAmount(string item)
            {
                item = item.Replace(".", "");
                item = item.Replace(",", "");
                return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
            }
        }
    }
}
