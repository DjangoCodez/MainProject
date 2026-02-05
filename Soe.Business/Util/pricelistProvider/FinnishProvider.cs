using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class FinnishProvider : IPriceListProvider
    {
        #region Members

        readonly List<FinnishProviderPost> posts;
        readonly HybridDictionary tmpStore;

        readonly Enum PriceListProvider;
        readonly bool netPriceList = false;

        #endregion

        #region Constructors

        public FinnishProvider(Enum priceListProvider)
        {
            posts = new List<FinnishProviderPost>();
            tmpStore = new HybridDictionary();
            this.PriceListProvider = priceListProvider;
            if (priceListProvider.GetType().Name == "SoeCompPriceListProvider" &&
                    (
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.RexelFINetto ||
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.AhlsellFINetto ||
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.AhlsellFIPLNetto ||
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.SoneparFINetto ||
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.OnninenFINettoS ||
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.DahlFINetto ||
                        (SoeCompPriceListProvider)priceListProvider == SoeCompPriceListProvider.OnninenFINettoLVI
                    )
               )
               netPriceList = true;
        }

        #endregion

        #region Public methods

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            FinnishProviderPost tmpPost;
            FinnishProviderPost existingPost;
            var importPrices = false;
            posts.Clear();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                //header
                if (line.StartsWith("O"))
                {
                    string code = line.Substring(20, 3);

                    if (code == "L01")
                        importPrices = false;
                    else if (code == "H10")
                        importPrices = true;

                    continue;
                }
                else if (line.StartsWith("----"))
                {
                    importPrices = true;
                    continue;
                }

                // Validate format
                if (netPriceList && !Validate(line))
                    continue;

                try
                {
                    tmpPost = new FinnishProviderPost(line, importPrices);
                    existingPost = (FinnishProviderPost)tmpStore[tmpPost.ProductId];
                    if (existingPost != null)
                    {
                        existingPost.Price = tmpPost.Price;
                    }
                    else
                    {
                        tmpStore.Add(tmpPost.ProductId, tmpPost);
                    }

                }
                catch (Exception ex) //hide error
                {
                    ex.ToString(); //prevent compiler warning
                }
            }

            try
            {
                foreach (var fpp in tmpStore)
                {
                    posts.Add((FinnishProviderPost)(((DictionaryEntry)fpp).Value));
                }
            }
            catch (Exception ex) //hide error
            {
                ex.ToString(); //prevent compiler warning
            }

            return result;
        }

        public GenericProvider ToGeneric()
        {
            var wholeSeller = PriceListProvider.ToString();

            var result = new GenericProvider(wholeSeller, TermGroup_Country.FI);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            try
            {
                foreach (FinnishProviderPost product in posts)
                {
                    if ((!netPriceList && product.Name != null) || (netPriceList && product.Price > 0))
                    {
                        var gp = new GenericProduct();

                        if (netPriceList)
                        { gp.NetPrice = product.Price; }
                        else
                        { gp.Price = product.Price; }

                        gp.ProductId = product.ProductId;
                        gp.Name = product.Name;
                        gp.Code = product.MaterialCode;
                        gp.SalesUnit = product.StorageUnit;
                        gp.PurchaseUnit = product.StorageUnit;
                        gp.EAN = product.EAN;

                        gp.WholesellerName = wholeSeller; //for relation

                        result.products.Add(result.products.Count, gp);
                    }
                }
            }
            catch (Exception ex) //hide error
            {
                ex.ToString(); //prevent compiler warning
            }
            return result;
        }

        #region Validation

        private bool Validate(string line)
        {
            switch((SoeCompPriceListProvider)PriceListProvider)
            {
                case SoeCompPriceListProvider.AhlsellFINetto:
                case SoeCompPriceListProvider.RexelFINetto:
                    return line.StartsWith("RS");
                case SoeCompPriceListProvider.AhlsellFIPLNetto:
                    return (line.StartsWith("RL") || line.StartsWith("RI") || line.StartsWith("RP") || line.StartsWith("RR"));
                default:
                    return true;
            }
        }

        #endregion

        #endregion

        public class FinnishProviderPost
        {
            #region Members

            public string ProductId { get; set; }
            public decimal Price { get; set; }
            public string MaterialCode { get; set; }
            public string StorageUnit { get; set; }
            public string Name { get; set; }
            public int NrOfPieces { get; set; }

            public string EAN { get; set; }


            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public FinnishProviderPost()
            {
                //Empty constructor
            }

            public FinnishProviderPost(string item, bool importPrices)
            {
                ProductId = item.Substring(2, 9)?.Trim();

                if (importPrices)
                {
                    Price = GetAmount(item.Substring(13, 9));
                    NrOfPieces = GetNrOfPieces(item.Substring(39, 4));
                    if (NrOfPieces > 1)
                    {
                        Price = Price / NrOfPieces;
                    }
                }
                else
                {
                    //We need to combain 2 name fields to one
                    Name = item.Substring(23, 35).Trim() + " " + item.Substring(58, 35).Trim();
                    MaterialCode = item.Substring(120, 6);
                    StorageUnit = item.Substring(126, 3);
                    EAN = item.Substring(200, 13)?.Trim();
                    if (EAN == "null")
                    {
                        EAN = string.Empty;
                    }
                }
            }

            #endregion

            #region Help methods

            private decimal GetAmount(string item)
            {
                return Convert.ToDecimal(item) / 100;
            }

            private int GetNrOfPieces(string item)
            {
                return Convert.ToInt16(item);
            }

            #endregion
        }
    }

}