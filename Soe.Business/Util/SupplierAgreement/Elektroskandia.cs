using System;
using System.Collections.Generic;
using System.IO;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Elektroskandia : ISupplierAgreement
    {
        private readonly List<ElektroskandiaDiscount> supplierAgreements;
        
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Sonepar; } }

        public Elektroskandia()
        {
            supplierAgreements = new List<ElektroskandiaDiscount>();
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
                line = line.PadRight(20);
                try
                {
                    supplierAgreements.Add(new ElektroskandiaDiscount(line));
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
            foreach (ElektroskandiaDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class ElektroskandiaDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public ElektroskandiaDiscount()
        {
            //Empty constructor for unit testing
        }

        public ElektroskandiaDiscount(string item)
        {
            MaterialClass = item.Substring(3, 3);
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(10, 4));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / 10M);
        }

        #endregion
    }
}
