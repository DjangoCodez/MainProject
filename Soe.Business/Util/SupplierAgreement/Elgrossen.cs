using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Elgrossen : ISupplierAgreement
    {
        public List<ElgrossenDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Bragross; } }

        public Elgrossen()
        {
            supplierAgreements = new List<ElgrossenDiscount>();
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.IndexOf("Rabatt") > -1) continue;
                line = line.PadRight(30);
                try
                {
                    supplierAgreements.Add(new ElgrossenDiscount(line));
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
            foreach (ElgrossenDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class ElgrossenDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public ElgrossenDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public ElgrossenDiscount(string item)
        {
            MaterialClass = formatString(item.Substring(0, 4));
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(4, 11));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            item = item.Replace(";", "");
            var asInt = Convert.ToInt32(item);

            if (asInt > 0)
                return Convert.ToDecimal(asInt / 100M);

            return 0;
        }

        private string formatString(string item)
        {
            item = item.Replace(";", "");

            return item;
        }

        #endregion
    }
}