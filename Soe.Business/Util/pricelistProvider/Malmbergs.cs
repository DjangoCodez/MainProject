using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Malmbergs : IPriceListProvider
    {
        #region Members

        List<MalmbergsPost> posts;

        #endregion

        #region Constructors

        public Malmbergs()
        {
            posts = new List<MalmbergsPost>();
        }

        #endregion

        #region Public methods

        public ActionResult Read(Stream stream, string fileName = null)
        {
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            sr.ReadLine(); //First line is the headers
            int i = 1;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                i++;
                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    posts.Add(new MalmbergsPost(line));
                }
                catch (Exception e)
                {
                    return new ActionResult { Success = false, ErrorMessage = e.Message, IntegerValue2 = i };
                }
            }
            return new ActionResult { Success=true, IntegerValue = i };
        }

        public GenericProvider ToGeneric()
        {
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Malmbergs);
            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            foreach (MalmbergsPost product in posts)
            {
                var gp = new GenericProduct();

                gp.ProductId = product.ProductId;
                gp.Name = product.Name;
                gp.Price = product.Price;
                gp.SalesUnit = product.StorageUnit;
                gp.PurchaseUnit = product.StorageUnit;
                gp.EAN = product.EAN;
                gp.Code = product.RebateGroup;

                gp.WholesellerName = wholeSeller; //for relation

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion

        public class MalmbergsPost
        {
            #region Members

            public string ProductId { get; set; }
            public decimal Price { get; set; }
            public string StorageUnit { get; set; }
            public string Name { get; set; }
            public string RebateGroup { get; set; }
            public string EAN { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public MalmbergsPost()
            {
                //Empty constructor
            }

            public MalmbergsPost(string item)
            {
                string[] parts = item.Split(";".ToCharArray());

                if (parts.Length < 5)
                {
                    throw new Exception("Malmbergs: felaktigt antal kolumner");
                }

                //RebateGroup = parts[0]; Verkar inte använda numera - 201809
                ProductId = parts[0];
                Name = parts[1];
                EAN = (parts[2]);
                StorageUnit = parts[3];

                decimal number;
                if (Decimal.TryParse(parts[4], out number))
                    Price = number;
                else
                    Price = 0;

                if (string.IsNullOrEmpty( ProductId) )
                {
                    throw new Exception("Malmbergs: produktnummer saknas");
                }
            }

            #endregion

            #region Help methods

            private decimal GetAmount(string item)
            {
                return Convert.ToDecimal(item) / 100;
            }

            #endregion
        }
    }

}
