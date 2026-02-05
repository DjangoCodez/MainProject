using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class E2Elteknik : ISupplierAgreement
    {
        public List<E2ElteknikDiscount> supplierAgreements { get; set; }
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.E2Teknik; } }

        public E2Elteknik()
        {
            supplierAgreements = new List<E2ElteknikDiscount>();
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
                    supplierAgreements.Add(new E2ElteknikDiscount(line));
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
            foreach (E2ElteknikDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class E2ElteknikDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public E2ElteknikDiscount()
        {
            //Empty constructor for unit testing
        }

        public E2ElteknikDiscount(string item)
        {
            MaterialClass = item.Substring(1, 3);
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(5, 5));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / 100M);
        }

        #endregion
    }
}
