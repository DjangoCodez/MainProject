using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Carpings : ISupplierAgreement
    {
        public List<CarpingsDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Carpings; } }

        public Carpings()
        {
            supplierAgreements = new List<CarpingsDiscount>();
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();
            var sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.IndexOf("Rabatt") > -1) continue;
                if (line.IndexOf("RABATT") > -1) continue;
                line = line.PadRight(30);
                try
                {
                    var discount = new CarpingsDiscount(line);
                    //we currently dont support special prices...
                    if (!string.IsNullOrEmpty(discount.MaterialClass))
                    {
                        supplierAgreements.Add(discount);
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
            var result = new GenericProvider();
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.SthlmElgross);
            foreach (CarpingsDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class CarpingsDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }
        public string Kundnr { get; set; }
        public string Artikelnr { get; set; }
        public decimal Nettopris { get; set; }
        
        #endregion

        #region Constructors

        public CarpingsDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public CarpingsDiscount(string item)
        {
            MaterialClass = item.Substring(0, 5).Trim();
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(32, 4).Trim());

            if (item.Length >= 68 )
            {
                Kundnr = item.Substring(35, 10).Trim();
                Artikelnr = item.Substring(45, 15).Trim();
                Nettopris = ToDecimal(item.Substring(60, 8).Trim());
            }
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            if (!string.IsNullOrEmpty(item))
            {
                item = item.Replace(" ", "");
                item = item.Replace(".", "");
                item = item.Replace(",", "");
                var asInt = Convert.ToInt32(item);

                if (asInt > 0)
                    return Convert.ToDecimal(asInt / 10M);
            }
            return 0;
        }

        #endregion
    }
}
