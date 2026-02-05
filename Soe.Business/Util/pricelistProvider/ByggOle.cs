using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class ByggOle : IPriceListProvider
    {
        #region Members

        private readonly List<ByggOlePost> posts;

        #endregion

        #region Constructors

        public ByggOle()
        {
            posts = new List<ByggOlePost>();
        }

        #endregion

        #region Public methods

        public ActionResult Read(Stream stream, string fileName = null)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            sr.ReadLine(); //First line is the headers
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                //line = line.PadRight(110);
                try
                {
                    posts.Add(new ByggOlePost(line));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.ByggOle);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            foreach (ByggOlePost product in posts)
            {
                var gp = new GenericProduct();

                //gp.ProductId = product.ProductId;
                //gp.Name = product.Name;
                //gp.Price = product.Price;
                //gp.SalesUnit = product.StorageUnit;
                //gp.PurchaseUnit = product.StorageUnit;
                //gp.EAN = product.EAN;
                //gp.Code = product.RebateGroup;

                gp.WholesellerName = wholeSeller; //for relation

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion

        public class ByggOlePost
        {
            #region Members

            public string ProductId { get; set; }
            public decimal Price { get; set; }
            public string StorageUnit { get; set; }
            public string Name { get; set; }
            public string RebateGroup { get; set; }
            public long EAN { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public ByggOlePost()
            {
                //Empty constructor
            }

            public ByggOlePost(string item)
            {
                string[] parts = item.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                RebateGroup = parts[0];
                ProductId = parts[1];
                Name = parts[2];
                EAN = long.Parse(parts[3]);
                StorageUnit = parts[4];

                decimal number;
                if (Decimal.TryParse(parts[5], out number))
                    Price = number;
                else
                    Price = 0;
                               
            }

            #endregion
        }
    }
}
