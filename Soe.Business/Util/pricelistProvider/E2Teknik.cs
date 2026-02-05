using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.IO;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class E2Teknik : IPriceListProvider
    {
        private readonly GenericProvider genProvider;
        public const string WHOLESELLERNAME = "E2Teknik";

        public E2Teknik()
        {
            genProvider = new GenericProvider(WHOLESELLERNAME);
            genProvider.header = new GenericHeader(DateTime.Now);
        }

        public ActionResult Read(Stream stream, string fileName = null)
        {
            var result = new ActionResult();
            var sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(85);
                try
                {
                    var prod = this.ReadLine(line);
                    if(prod != null)
                        genProvider.products.Add(genProvider.products.Count, prod);
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
            return genProvider;
        }

        private GenericProduct ReadLine(string line)
        {
            /*
            POS   Fältlängd INNEHÅLL
            01-01 1         Posttyp (alltid 2)
            02-09 8         Artikelnummer 8 siffror (TBEL08.txt)
            02-08 7         Artikelnummer 7 siffror (TBEL07.txt)
            10-34 25        Benämning
            35-37 3         Rabattgrupp
            38-38 1         Alem 09 Garantikod
            39-46 8         Riktpris
            47-49 3         Förpackningsenhet
            51-57 8         Prisändringsdatum
             */
            var prod = new GenericProduct();
            prod.ProductId = line.Substring(1, 7).Trim();
            //Validate
            if (prod.ProductId.Length < 7 || prod.ProductId.Length > 8)
                return null;

            prod.Name = line.Substring(9, 25);
            prod.Code = line.Substring(34, 3);
            prod.Price = GetAmount(line.Substring(38, 8));
            prod.PurchaseUnit = line.Substring(46, 3).Trim(); 
            prod.EnvironmentFee = false;
            prod.WholesellerName = WHOLESELLERNAME;

            return prod;
        }


        private decimal GetAmount(string item)
        {
            return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
        }

    }
}
