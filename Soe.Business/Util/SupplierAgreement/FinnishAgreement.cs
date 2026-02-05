using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class FinnishAgreement : ISupplierAgreement
    {
        private readonly SoeSupplierAgreementProvider providerType;

        public List<FinnishAgreementDiscount> supplierAgreements;
        
        public SoeSupplierAgreementProvider Provider { get { return providerType; } }

        public FinnishAgreement(SoeSupplierAgreementProvider providerType)
        {
            supplierAgreements = new List<FinnishAgreementDiscount>();
            this.providerType = providerType;
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();

       //     StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1); tolka alltid som utf8, funkar mot ascii också
            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    supplierAgreements.Add(new FinnishAgreementDiscount(line));
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
            foreach (FinnishAgreementDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class FinnishAgreementDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public FinnishAgreementDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public FinnishAgreementDiscount(string item)
        {
            MaterialClass = item.Substring(1, 6).TrimEnd();
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(74, 9));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);

            if (asInt > 0)
                return Convert.ToDecimal(asInt / 100M);

            return 0;
        }
        
        #endregion
    }
}