using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Eios : IPriceListProvider
    {
        #region Members

        EiosOpeningPost openingPost;
        List<EiosProductPost> productPosts;
        EiosSummaryPost summaryPost;

        #endregion

        #region Constructors

        public Eios()
        {
            productPosts = new List<EiosProductPost>();
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
                line = line.PadRight(47);
                try
                {
                    switch (GetPostType(line))
                    {
                        case EiosPostType.OpeningPost:
                            openingPost = new EiosOpeningPost(line);
                            break;
                        case EiosPostType.ProductPost:
                            productPosts.Add(new EiosProductPost(line));
                            break;
                        case EiosPostType.SummaryPost:
                            summaryPost = new EiosSummaryPost(line);
                            break;
                        case EiosPostType.WholeSellerPost:
                            productPosts.Last().WholeSellerPosts.Add(new EiosWholeSellerPost(line));
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
            var result = new GenericProvider(string.Empty);
            result.header = new GenericHeader(openingPost.Date);
            foreach (EiosProductPost product in productPosts)
            {
                foreach (EiosWholeSellerPost price in product.WholeSellerPosts)
                {
                    var name = GetWholeSellerName(price.WholeSeller);
                    result.products.Add(result.products.Count, new GenericProduct(product.Name, price.WholeSellerPrice, product.Type, price.RebateCode, product.ProductId, product.Date, name, price.State));
                }
            }
            return result;
        }
        #endregion

        #region Help methods

        private EiosPostType GetPostType(string line)
        {
            return (EiosPostType)Convert.ToInt32(line.Substring(0, 1));
        }

        private string GetWholeSellerName(EiosWholeseller wholeSeller)
        {
            return Enum.GetName(typeof(EiosWholeseller), wholeSeller);
        }

        #endregion
    }

    #region EiosPostTypes
    public class EiosOpeningPost : IEiosPost
    {
        #region Members
        public EiosPostType PostType { get; set; }
        public DateTime Date { get; set; }
        public string DatasetName { get; set; }
        public string TextInformation { get; set; }
        #endregion

        #region Constructors
        public EiosOpeningPost(string item)
        {
            PostType = EiosPostType.OpeningPost;
            Date = EiosUtil.GetDate(item.Substring(1, 8));
            DatasetName = item.Substring(9, 10);
            TextInformation = item.Substring(19, 18);
        }
        #endregion
    }
    public class EiosProductPost : IEiosPost
    {
        #region Members
        public EiosPostType PostType { get; set; }
        public List<EiosWholeSellerPost> WholeSellerPosts;
        public int ProductId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }

        #endregion

        #region Constructors
        public EiosProductPost(string item)
        {
            PostType = EiosPostType.ProductPost;
            ProductId = Convert.ToInt32(item.Substring(1, 8));
            Name = item.Substring(9, 25);
            Date = EiosUtil.GetDate(item.Substring(34, 8));
            Type = item.Substring(34, 3);
            WholeSellerPosts = new List<EiosWholeSellerPost>();
        }
        #endregion
    }
    public class EiosWholeSellerPost : IEiosPost
    {
        #region Members
        public EiosPostType PostType { get; set; }
        public EiosWholeseller WholeSeller { get; set; }
        public decimal WholeSellerPrice { get; set; }
        public SoeProductPriceStatus State { get; set; }
        public string RebateCode { get; set; }

        #endregion

        #region Constructors
        public EiosWholeSellerPost(string item)
        {
            PostType = EiosPostType.WholeSellerPost;
            WholeSeller = (EiosWholeseller)Convert.ToInt32(item.Substring(1, 2));
            WholeSellerPrice = Convert.ToDecimal(Convert.ToInt32(item.Substring(3, 8)) / 100M);

            var state = item.Substring(11, 1);
            if (state == " ")
                State = SoeProductPriceStatus.PriceChange;
            else if (state == "+")
                State = SoeProductPriceStatus.NewProduct;
            else if (state == "-")
                State = SoeProductPriceStatus.PriceChange;
            else if (state == "?")
                State = SoeProductPriceStatus.PricedOnRequest;

            RebateCode = item.Substring(12, 4);
        }
        #endregion
    }
    public class EiosSummaryPost : IEiosPost
    {
        #region Members
        public EiosPostType PostType { get; set; }
        public int PostCount { get; set; }
        #endregion

        #region Constructors
        public EiosSummaryPost(string line)
        {
            PostType = EiosPostType.SummaryPost;
            PostCount = Convert.ToInt32(line.Substring(1, 5));
        }
        #endregion
    }
    #endregion

    #region Interfaces
    interface IEiosPost
    {
        EiosPostType PostType { get; set; }
    }
    #endregion

    #region Enumerations
    public enum EiosPostType
    {
        OpeningPost = 0,
        ProductPost = 1,
        WholeSellerPost = 2,
        SummaryPost = 3,
    }
    public enum EiosWholeseller
    {
        Elektroskandia = 1,
        Ahlsell = 2,
        Selga = 3,
        Moel = 6,
        Solar = 7,
        Elkedjan = 10,
        Storel = 11,
        Elgrossén = 12,
    }
    #endregion

    #region Private Util
    static class EiosUtil
    {
        public static DateTime GetDate(string date)
        {
            int year = Convert.ToInt32(date.Substring(0, 4));
            int month = Convert.ToInt32(date.Substring(4, 2));
            int day = Convert.ToInt32(date.Substring(6, 2));
            return new DateTime(year, month, day);
        }

    }
    #endregion
}
